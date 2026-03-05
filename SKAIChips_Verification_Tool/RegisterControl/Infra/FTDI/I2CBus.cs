using System;
using System.Linq;

namespace SKAIChips_Verification_Tool.RegisterControl
{
    /// <summary>
    /// FTDI 칩셋(FT4222H 또는 UM232H)을 사용하여 I2C 통신 및 GPIO 제어를 수행하는 통합 버스 클래스입니다.
    /// II2cBus 및 IGpioController 인터페이스를 구현하며, 설정된 디바이스 종류(DeviceKind)에 따라
    /// 내부적으로 적절한 하위 하드웨어 제어 인스턴스로 명령을 라우팅합니다.
    /// </summary>
    public sealed class I2cBus : II2cBus, IGpioController
    {
        private readonly uint _deviceIndex;
        private readonly ProtocolSettings _settings;

        private FT4222H? _ft4222;
        private UM232H? _um232h;

        // FT4222H의 4개 GPIO 핀 방향(입력/출력) 상태를 추적하기 위한 배열 (기본값: INPUT)
        private readonly FT4222H.GPIO_Direction[] _ft4222GpioDirs =
            Enumerable.Repeat(FT4222H.GPIO_Direction.INPUT, 4).ToArray();

        /// <summary>
        /// 현재 디바이스와 정상적으로 연결되어 통신 가능한 상태인지 여부를 가져옵니다.
        /// </summary>
        public bool IsConnected
        {
            get; private set;
        }

        /// <summary>
        /// I2cBus 클래스의 새 인스턴스를 초기화합니다.
        /// </summary>
        /// <param name="deviceIndex">FTDI 디바이스 목록에서의 0 기반 인덱스입니다.</param>
        /// <param name="settings">디바이스 종류(DeviceKind)와 통신 속도 등을 포함한 프로토콜 설정 객체입니다.</param>
        /// <exception cref="ArgumentNullException">설정 객체가 null일 경우 발생합니다.</exception>
        public I2cBus(uint deviceIndex, ProtocolSettings settings)
        {
            _deviceIndex = deviceIndex;
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        /// <summary>
        /// 설정된 DeviceKind에 맞춰 FT4222H 또는 UM232H 디바이스를 열고 I2C 마스터 모드로 초기화합니다.
        /// </summary>
        /// <returns>연결 및 초기화에 성공하면 true, 실패하면 false를 반환합니다.</returns>
        /// <exception cref="NotSupportedException">지원하지 않는 DeviceKind가 설정된 경우 발생합니다.</exception>
        public bool Connect()
        {
            Disconnect();

            switch (_settings.DeviceKind)
            {
                case DeviceKind.FT4222:
                    _ft4222 = new FT4222H();
                    if (!_ft4222.Open(_deviceIndex))
                        return false;

                    if (!_ft4222.I2cInit((ushort)_settings.SpeedKbps))
                    {
                        _ft4222.Close();
                        _ft4222 = null;
                        return false;
                    }

                    IsConnected = true;
                    return true;

                case DeviceKind.UM232H:
                    _um232h = new UM232H(_deviceIndex);
                    if (!_um232h.Open())
                    {
                        _um232h = null;
                        return false;
                    }

                    if (!_um232h.I2cInit(_settings.SpeedKbps))
                    {
                        _um232h.Close();
                        _um232h = null;
                        return false;
                    }

                    IsConnected = true;
                    return true;

                default:
                    throw new NotSupportedException($"DeviceKind not supported: {_settings.DeviceKind}");
            }
        }

        /// <summary>
        /// 현재 연결된 디바이스의 통신을 안전하게 종료하고 리소스를 해제합니다.
        /// </summary>
        public void Disconnect()
        {
            IsConnected = false;

            try
            {
                _ft4222?.Close();
            }
            catch { }
            _ft4222 = null;

            try
            {
                _um232h?.Close();
            }
            catch { }
            _um232h = null;
        }

        /// <summary>
        /// 지정된 I2C 슬레이브 주소로 데이터를 전송합니다.
        /// </summary>
        /// <param name="slaveAddr">데이터를 받을 I2C 슬레이브의 7비트 주소입니다.</param>
        /// <param name="data">전송할 데이터가 담긴 바이트 범위(Span)입니다.</param>
        /// <param name="stop">통신 종료 후 STOP 비트를 전송할지 여부입니다.</param>
        public void Write(byte slaveAddr, ReadOnlySpan<byte> data, bool stop = true)
        {
            EnsureConnected();

            byte targetAddr = _settings.I2cSlaveAddress;

            if (_settings.DeviceKind == DeviceKind.FT4222)
            {
                var buf = data.ToArray();
                _ft4222!.I2cWrite(slaveAddr, buf);
                return;
            }

            _um232h!.I2cWrite(slaveAddr, data.ToArray(), stop);
        }

        /// <summary>
        /// 지정된 I2C 슬레이브 주소로부터 데이터를 읽어옵니다.
        /// </summary>
        /// <param name="slaveAddr">데이터를 전송할 I2C 슬레이브의 7비트 주소입니다.</param>
        /// <param name="buffer">읽어온 데이터를 저장할 대상 바이트 범위(Span)입니다.</param>
        /// <param name="timeoutMs">읽기 작업의 타임아웃(밀리초)입니다. (현재 하위 로직에서 미사용)</param>
        public void Read(byte slaveAddr, Span<byte> buffer, int timeoutMs)
        {
            EnsureConnected();

            byte targetAddr = _settings.I2cSlaveAddress;

            if (_settings.DeviceKind == DeviceKind.FT4222)
            {
                var tmp = new byte[buffer.Length];
                _ft4222!.I2cRead(slaveAddr, tmp);
                tmp.AsSpan().CopyTo(buffer);
                return;
            }

            var r = _um232h!.I2cRead(slaveAddr, buffer.Length);
            r.AsSpan(0, Math.Min(r.Length, buffer.Length)).CopyTo(buffer);
        }

        /// <summary>
        /// 지정된 I2C 슬레이브 주소로 데이터를 전송한 직후, 즉시 데이터를 읽어오는 복합(Write-then-Read) 트랜잭션을 수행합니다.
        /// </summary>
        /// <param name="slaveAddr">I2C 슬레이브 디바이스의 7비트 주소입니다.</param>
        /// <param name="w">전송할 데이터가 담긴 읽기 전용 바이트 범위입니다.</param>
        /// <param name="r">읽어온 데이터를 저장할 대상 바이트 범위입니다.</param>
        /// <param name="timeoutMs">작업 타임아웃(밀리초)입니다.</param>
        public void WriteRead(byte slaveAddr, ReadOnlySpan<byte> w, Span<byte> r, int timeoutMs)
        {
            EnsureConnected();

            byte targetAddr = _settings.I2cSlaveAddress;

            if (_settings.DeviceKind == DeviceKind.FT4222)
            {
                // FT4222H는 쓰기 후 별도의 읽기 명령으로 처리
                Write(slaveAddr, w, stop: true);
                Read(slaveAddr, r, timeoutMs);
                return;
            }

            // UM232H는 전용 WriteAndRead 메서드를 사용하여 연속된 트랜잭션 보장
            var rr = _um232h!.I2cWriteAndRead(slaveAddr, w.ToArray(), w.Length, r.Length);
            rr.AsSpan(0, Math.Min(rr.Length, r.Length)).CopyTo(r);
        }

        /// <summary>
        /// 특정 GPIO 핀의 입출력(Direction) 방향을 설정합니다. 디바이스 종류에 따라 내부 핀 맵핑이 다르게 처리됩니다.
        /// </summary>
        /// <param name="pinIndex">제어할 논리적 GPIO 핀 번호입니다.</param>
        /// <param name="isOutput">true이면 출력(Output) 모드, false이면 입력(Input) 모드로 설정합니다.</param>
        /// <exception cref="ArgumentOutOfRangeException">허용되지 않은 핀 번호일 때 발생합니다.</exception>
        public void SetGpioDirection(int pinIndex, bool isOutput)
        {
            EnsureConnected();

            if (_settings.DeviceKind == DeviceKind.UM232H)
            {
                // UM232H 핀 맵핑:
                // 논리 핀 0~7 -> 물리 ACBUS 0~7 (GPIOH)
                if (pinIndex >= 0 && pinIndex <= 7)
                {
                    _um232h!.SetGpioHDirection(pinIndex, isOutput);
                }
                // 논리 핀 8~11 -> 물리 ADBUS 4~7 (GPIOL)
                else if (pinIndex >= 8 && pinIndex <= 11)
                {
                    int adbusIndex = pinIndex - 8 + 4;
                    _um232h!.SetGpioLDirection(adbusIndex, isOutput);
                }
                else
                {
                    throw new ArgumentOutOfRangeException(nameof(pinIndex), "UM232H GPIO Pin must be 0-11");
                }
            }
            else if (_settings.DeviceKind == DeviceKind.FT4222)
            {
                if (pinIndex < 0 || pinIndex > 3)
                    throw new ArgumentOutOfRangeException(nameof(pinIndex), "FT4222 GPIO Pin must be 0-3");

                _ft4222GpioDirs[pinIndex] = isOutput ? FT4222H.GPIO_Direction.OUTPUT : FT4222H.GPIO_Direction.INPUT;
                _ft4222!.GpioInit(_ft4222GpioDirs);
            }
        }

        /// <summary>
        /// 특정 GPIO 핀(출력 모드)에 디지털 신호(High/Low)를 출력합니다.
        /// </summary>
        /// <param name="pinIndex">신호를 출력할 논리적 GPIO 핀 번호입니다.</param>
        /// <param name="isHigh">true이면 High(1), false이면 Low(0) 신호를 출력합니다.</param>
        public void SetGpioValue(int pinIndex, bool isHigh)
        {
            EnsureConnected();

            if (_settings.DeviceKind == DeviceKind.UM232H)
            {
                // GPIOH0~7 -> ACBUS 0~7
                if (pinIndex >= 0 && pinIndex <= 7)
                {
                    _um232h!.SetGpioHPin(pinIndex, isHigh);
                }
                // GPIOL0~3 -> ADBUS 4~7
                else if (pinIndex >= 8 && pinIndex <= 11)
                {
                    int adbusIndex = pinIndex - 8 + 4;
                    _um232h!.SetGpioLPin(adbusIndex, isHigh);
                }
            }
            else if (_settings.DeviceKind == DeviceKind.FT4222)
            {
                if (pinIndex < 0 || pinIndex > 3)
                    throw new ArgumentOutOfRangeException(nameof(pinIndex));

                _ft4222!.GpioWrite((FT4222H.GPIO_Port)pinIndex, isHigh);
            }
        }

        /// <summary>
        /// 특정 GPIO 핀(입력 모드)에서 현재의 디지털 신호 상태(High/Low)를 읽어옵니다.
        /// </summary>
        /// <param name="pinIndex">상태를 읽어올 논리적 GPIO 핀 번호입니다.</param>
        /// <returns>핀의 상태가 High이면 true, Low이면 false를 반환합니다.</returns>
        public bool GetGpioValue(int pinIndex)
        {
            EnsureConnected();

            if (_settings.DeviceKind == DeviceKind.UM232H)
            {
                // GPIOH0~7 -> ACBUS 0~7
                if (pinIndex >= 0 && pinIndex <= 7)
                {
                    return _um232h!.GetGpioHPin(pinIndex);
                }
                // GPIOL0~3 -> ADBUS 4~7
                else if (pinIndex >= 8 && pinIndex <= 11)
                {
                    int adbusIndex = pinIndex - 8 + 4;
                    return _um232h!.GetGpioLPin(adbusIndex);
                }
            }
            else if (_settings.DeviceKind == DeviceKind.FT4222)
            {
                if (pinIndex < 0 || pinIndex > 3)
                    throw new ArgumentOutOfRangeException(nameof(pinIndex));

                _ft4222!.GpioRead((FT4222H.GPIO_Port)pinIndex, out bool val);
                return val;
            }

            return false;
        }

        /// <summary>
        /// 인스턴스가 삭제될 때 연결된 I2C 버스를 안전하게 해제합니다.
        /// </summary>
        public void Dispose() => Disconnect();

        /// <summary>
        /// I2C 버스가 연결되어 있는지 확인하고, 연결되지 않았을 경우 예외를 발생시킵니다.
        /// </summary>
        private void EnsureConnected()
        {
            if (!IsConnected)
                throw new InvalidOperationException("I2C bus is not connected.");
        }
    }
}