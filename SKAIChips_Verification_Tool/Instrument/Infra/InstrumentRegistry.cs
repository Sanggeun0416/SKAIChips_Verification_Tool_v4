using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace SKAIChips_Verification_Tool.Instrument
{
    /// <summary>
    /// 시스템에 등록된 모든 계측기(Instrument)의 정보를 중앙 관리하고, 실제 통신 클라이언트 인스턴스를 유지하는 싱글톤 레지스트리 클래스입니다.
    /// JSON 설정 파일 로드, 계측기 인스턴스 캐싱 및 스레드 안전한 접근을 보장합니다.
    /// </summary>
    public sealed class InstrumentRegistry
    {
        /// <summary>
        /// InstrumentRegistry의 유일한 전역 인스턴스를 가져옵니다.
        /// </summary>
        public static InstrumentRegistry Instance { get; } = new InstrumentRegistry();

        private readonly object _sync = new object();
        private readonly List<InstrumentInfo> _infos = new();

        // 계측기 타입(Type)을 키로 사용하여 생성된 통신 클라이언트를 보관하는 캐시 딕셔너리
        private readonly Dictionary<string, IScpiClient> _clients = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// 외부에서 인스턴스를 생성할 수 없도록 생성자를 private으로 제한합니다.
        /// </summary>
        private InstrumentRegistry()
        {
        }

        /// <summary>
        /// 새로운 계측기 정보 목록으로 레지스트리를 갱신합니다. 
        /// 기존에 연결되어 있던 모든 클라이언트의 연결을 종료하고 초기화합니다.
        /// </summary>
        /// <param name="instruments">새로 적용할 계측기 정보 컬렉션</param>
        /// <exception cref="ArgumentNullException">매개변수가 null일 경우 발생합니다.</exception>
        public void Update(IEnumerable<InstrumentInfo> instruments)
        {
            if (instruments == null)
                throw new ArgumentNullException(nameof(instruments));

            lock (_sync)
            {
                // 기존 메타데이터 초기화 및 복사본 저장
                _infos.Clear();
                _infos.AddRange(instruments.Select(Clone));

                // 기존에 생성되어 있던 실제 통신 세션들을 모두 안전하게 닫고 해제
                foreach (var c in _clients.Values)
                {
                    try
                    {
                        c.Close();
                        c.Dispose();
                    }
                    catch { }
                }
                _clients.Clear();
            }
        }

        /// <summary>
        /// 원본 계측기 정보 객체를 깊은 복사(Deep Copy)하여 반환합니다.
        /// </summary>
        private static InstrumentInfo Clone(InstrumentInfo src) => new()
        {
            Type = src.Type,
            Enabled = src.Enabled,
            VisaAddress = src.VisaAddress,
            Name = src.Name
        };

        /// <summary>
        /// 지정된 타입의 계측기 통신 클라이언트를 가져옵니다. 
        /// 처음 호출 시 인스턴스를 생성하고 연결을 시도하며, 이후 호출 시에는 캐싱된 객체를 반환합니다.
        /// </summary>
        /// <param name="type">찾고자 하는 계측기 타입 (예: "Oscilloscope", "SMU")</param>
        /// <returns>해당 계측기와 통신 가능한 IScpiClient 인스턴스</returns>
        /// <exception cref="InvalidOperationException">계측기를 찾을 수 없거나, 비활성화 상태이거나, 연결에 실패했을 때 발생합니다.</exception>
        public IScpiClient GetByType(string type)
        {
            if (string.IsNullOrWhiteSpace(type))
                throw new ArgumentNullException(nameof(type));

            type = type.Trim();

            lock (_sync)
            {
                // 1. 이미 생성된 클라이언트가 있다면 즉시 반환 (캐싱)
                if (_clients.TryGetValue(type, out var cached))
                    return cached;

                // 2. 등록된 정보 중 활성화된 상태의 해당 타입 계측기를 검색
                var info = _infos
                    .Where(x => x.Enabled)
                    .FirstOrDefault(x => string.Equals(x.Type, type, StringComparison.OrdinalIgnoreCase));

                if (info == null)
                    throw new InvalidOperationException($"Instrument(Type='{type}') not found or not enabled.");

                if (string.IsNullOrWhiteSpace(info.VisaAddress))
                    throw new InvalidOperationException($"Instrument(Type='{type}') has empty VISA address.");

                // 3. 실제 통신 클라이언트(VisaScpiClient) 생성 및 연결 시도
                var client = new VisaScpiClient(info.VisaAddress);
                if (!client.Open())
                {
                    client.Dispose();
                    throw new InvalidOperationException($"Failed to open instrument(Type='{type}', Addr='{info.VisaAddress}').");
                }

                // 4. 생성된 클라이언트를 딕셔너리에 등록하여 재사용 가능하게 함
                _clients[type] = client;
                return client;
            }
        }

        /// <summary>
        /// 현재 활성화(Enabled) 되어 있는 모든 계측기의 타입 목록을 가져옵니다.
        /// </summary>
        /// <returns>활성화된 계측기 타입 문자열 컬렉션</returns>
        public IEnumerable<string> GetEnabledInstrumentTypes()
        {
            lock (_sync)
            {
                return _infos
                    .Where(x => x.Enabled)
                    .Select(x => x.Type)
                    .ToList();
            }
        }

        /// <summary>
        /// 실행 파일 디렉토리에 위치한 'InstrumentSettings.json' 파일로부터 계측기 설정을 읽어와 레지스트리를 초기화합니다.
        /// 프로그램 시작 시 주로 호출됩니다.
        /// </summary>
        public void Load()
        {
            try
            {
                string exeDir = AppDomain.CurrentDomain.BaseDirectory;
                string path = Path.Combine(exeDir, "InstrumentSettings.json");

                if (!File.Exists(path))
                    return;

                string json = File.ReadAllText(path);
                var list = JsonSerializer.Deserialize<List<InstrumentInfo>>(json);

                if (list != null)
                {
                    Update(list);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load instruments: {ex.Message}");
            }
        }
    }
}