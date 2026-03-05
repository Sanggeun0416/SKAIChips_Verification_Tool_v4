using System;
using System.Linq;

namespace SKAIChips_Verification_Tool.RegisterControl
{
    /// <summary>
    /// FTDI 칩셋(FT4222H 또는 UM232H)을 사용하여 SPI 통신 및 GPIO 제어를 수행하는 통합 버스 클래스입니다.
    /// ISpiBus, IChicagoSpiBus, IGpioController 인터페이스를 구현하며, 설정된 디바이스 종류에 따라
    /// 표준 SPI 통신 및 특정 타겟(Chicago)을 위한 커스텀 트랜잭션을 지원합니다.
    /// </summary>
    public sealed class SpiBus : ISpiBus, IChicagoSpiBus, IGpioController
    {
        private readonly uint _deviceIndex;
        private readonly ProtocolSettings _settings;

        private FT4222H? _ft4222;
        private UM232H? _um232h;

        // FT4222H의 4개 GPIO 핀 방향(입력/출력) 상태를 추적하기 위한 배열 (기본값: INPUT)
        private readonly FT4222H.GPIO_Direction[] _ft4222GpioDirs =
            Enumerable.Repeat(FT4222H.GPIO_Direction.INPUT, 4).ToArray();

        /// <summary>
        /// 현재 디바이스와 정상적으로 연결되어 SPI 통신이 가능한 상태인지 여부를 가져옵니다.
        /// </summary>
        public bool IsConnected
        {
            get; private set;
        }

        /// <summary>
        /// SpiBus 클래스의 새 인스턴스를 초기화합니다.
        /// </summary>
        /// <param name="deviceIndex">FTDI 디바이스 목록에서의 0 기반 인덱스입니다.</param>
        /// <param name="settings">디바이스 종류(DeviceKind)와 SPI 클럭, 모드 등을 포함한 프로토콜 설정 객체입니다.</param>
        /// <exception cref="ArgumentNullException">설정 객체가 null일 경우 발생합니다.</exception>
        public SpiBus(uint deviceIndex, ProtocolSettings settings)
        {
            _deviceIndex = deviceIndex;
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        /// <summary>
        /// 설정된 DeviceKind에 맞춰 디바이스를 열고 SPI 마스터 모드로 초기화합니다.
        /// 시카고(Chicago) 타겟을 위한 특정 초기화 로직(ForceIdleHighOnConnect)도 함께 수행됩니다.
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

                    if (!_ft4222.Master(_settings.SpiClockKHz, _settings.SpiMode))
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

                    if (!_um232h.SpiInit(_settings.SpiMode, _settings.SpiClockKHz, _settings.SpiLsbFirst))
                    {
                        _um232h.Close();
                        _um232h = null;
                        return false;
                    }

                    // Chicago 타겟을 위한 핀 초기화 옵션
                    if (_settings.ForceIdleHighOnConnect)
                    {
                        _um232h.SetGpioLHighForChicago();
                        _um232h.SetGpioHLow();
                    }

                    IsConnected = true;
                    return true;

                default:
                    throw new NotSupportedException($"DeviceKind not supported: {_settings.DeviceKind}");
            }
        }

        /// <summary>
        /// 현재 연결된 SPI 디바이스의 통신을 안전하게 종료하고 리소스를 해제합니다.
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
        /// 전이중(Full-Duplex) 방식으로 SPI 버스에서 데이터를 동시에 송수신합니다.
        /// </summary>
        /// <param name="tx">전송할 데이터가 담긴 읽기 전용 바이트 범위입니다.</param>
        /// <param name="rx">수신된 데이터를 저장할 바이트 범위입니다.</param>
        /// <param name="endTransaction">통신 완료 후 CS(Chip Select) 핀을 비활성화(High)할지 여부입니다. (기본값: true)</param>
        public void Transfer(ReadOnlySpan<byte> tx, Span<byte> rx, bool endTransaction = true)
        {
            EnsureConnected();

            if (_settings.DeviceKind == DeviceKind.FT4222)
            {
                var w = tx.ToArray();
                var rbuf = new byte[Math.Max(rx.Length, w.Length)];
                _ft4222!.SpiReadWriteBytes(rbuf, w, endTransaction);
                rbuf.AsSpan(0, rx.Length).CopyTo(rx);
                return;
            }

            if (rx.Length > 0)
            {
                var w = tx.ToArray();
                // UM232H의 경우 읽어올 길이만큼 쓰기 버퍼의 길이도 맞춰주어야 함
                if (w.Length < rx.Length)
                {
                    var w2 = new byte[rx.Length];
                    w.CopyTo(w2, 0);
                    w = w2;
                }

                var r = _um232h!.SpiReadWrite(w, rx.Length);
                r.AsSpan(0, Math.Min(r.Length, rx.Length)).CopyTo(rx);
            }
        }

        /// <summary>
        /// SPI 버스를 통해 데이터를 전송(Write-Only)합니다.
        /// </summary>
        /// <param name="tx">전송할 데이터가 담긴 읽기 전용 바이트 범위입니다.</param>
        /// <param name="endTransaction">통신 완료 후 CS 핀을 비활성화할지 여부입니다. (기본값: true)</param>
        public void Write(ReadOnlySpan<byte> tx, bool endTransaction = true)
        {
            EnsureConnected();

            if (_settings.DeviceKind == DeviceKind.FT4222)
            {
                _ft4222!.SpiWriteBytes(tx.ToArray(), endTransaction);
                return;
            }

            _um232h!.SpiWrite(tx.ToArray(), endTransaction);
        }

        /// <summary>
        /// SPI 버스를 통해 데이터를 수신(Read-Only)합니다.
        /// </summary>
        /// <param name="rx">수신된 데이터를 저장할 버퍼 범위입니다.</param>
        /// <param name="endTransaction">통신 완료 후 CS 핀을 비활성화할지 여부입니다. (기본값: true)</param>
        public void Read(Span<byte> rx, bool endTransaction = true)
        {
            EnsureConnected();

            if (_settings.DeviceKind == DeviceKind.FT4222)
            {
                var tmp = new byte[rx.Length];
                _ft4222!.SpiReadBytes(tmp, endTransaction);
                tmp.AsSpan().CopyTo(rx);
                return;
            }

            var r = _um232h!.SpiRead(rx.Length);
            r.AsSpan(0, Math.Min(r.Length, rx.Length)).CopyTo(rx);
        }

        /// <summary>
        /// 특정 GPIO 핀의 입출력(Direction) 방향을 설정합니다.
        /// </summary>
        /// <param name="pinIndex">제어할 논리적 GPIO 핀 번호입니다.</param>
        /// <param name="isOutput">true이면 출력 모드, false이면 입력 모드로 설정합니다.</param>
        /// <exception cref="ArgumentOutOfRangeException">허용되지 않은 핀 번호일 때 발생합니다.</exception>
        public void SetGpioDirection(int pinIndex, bool isOutput)
        {
            EnsureConnected();

            if (_settings.DeviceKind == DeviceKind.UM232H)
            {
                // GPIOH0~7 -> ACBUS 0~7
                if (pinIndex >= 0 && pinIndex <= 7)
                {
                    _um232h!.SetGpioHDirection(pinIndex, isOutput);
                }
                // GPIOL0~3 -> ADBUS 4~7
                else if (pinIndex >= 8 && pinIndex <= 11)
                {
                    int adbusIndex = pinIndex - 8 + 4;
                    _um232h!.SetGpioLDirection(adbusIndex, isOutput);
                }
                else
                {
                    throw new ArgumentOutOfRangeException(nameof(pinIndex), "UM232H GPIO Pin must be 0-11 (0-7: ACBUS, 8-11: ADBUS4-7)");
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
        /// 특정 GPIO 핀(출력 모드)에 디지털 신호를 출력합니다.
        /// </summary>
        public void SetGpioValue(int pinIndex, bool isHigh)
        {
            EnsureConnected();

            if (_settings.DeviceKind == DeviceKind.UM232H)
            {
                if (pinIndex >= 0 && pinIndex <= 7)
                {
                    _um232h!.SetGpioHPin(pinIndex, isHigh);
                }
                else if (pinIndex >= 8 && pinIndex <= 11)
                {
                    int adbusIndex = pinIndex - 8 + 4;
                    _um232h!.SetGpioLPin(adbusIndex, isHigh);
                }
                else
                {
                    throw new ArgumentOutOfRangeException(nameof(pinIndex));
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
        /// 특정 GPIO 핀(입력 모드)의 디지털 신호 상태를 읽어옵니다.
        /// </summary>
        public bool GetGpioValue(int pinIndex)
        {
            EnsureConnected();

            if (_settings.DeviceKind == DeviceKind.UM232H)
            {
                if (pinIndex >= 0 && pinIndex <= 7)
                {
                    return _um232h!.GetGpioHPin(pinIndex);
                }
                else if (pinIndex >= 8 && pinIndex <= 11)
                {
                    int adbusIndex = pinIndex - 8 + 4;
                    return _um232h!.GetGpioLPin(adbusIndex);
                }
                else
                {
                    throw new ArgumentOutOfRangeException(nameof(pinIndex));
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
        /// 사용이 끝난 SPI 버스를 해제합니다.
        /// </summary>
        public void Dispose() => Disconnect();

        private void EnsureConnected()
        {
            if (!IsConnected)
                throw new InvalidOperationException("SPI bus is not connected.");
        }

        #region Chicago-Specific Methods (UM232H 전용)

        /// <summary>
        /// Chicago 타겟 전용 SPI 쓰기 명령을 수행합니다. 전송 완료 후 특정 핀 상태를 High로 유지합니다.
        /// </summary>
        /// <exception cref="NotSupportedException">디바이스 종류가 UM232H가 아닐 경우 발생합니다.</exception>
        public void ChicagoWrite(ReadOnlySpan<byte> tx)
        {
            EnsureConnected();

            if (_settings.DeviceKind != DeviceKind.UM232H)
                throw new NotSupportedException("Chicago requires UM232H.");

            _um232h!.SpiWrite(tx.ToArray(), endTransaction: true);
            _um232h.SetGpioLHighForChicago();
        }

        /// <summary>
        /// Chicago 타겟 전용 복합(쓰기 및 읽기) 트랜잭션을 수행합니다.
        /// </summary>
        /// <exception cref="NotSupportedException">디바이스 종류가 UM232H가 아닐 경우 발생합니다.</exception>
        public void ChicagoWriteRead(ReadOnlySpan<byte> tx, Span<byte> rx)
        {
            EnsureConnected();

            if (_settings.DeviceKind != DeviceKind.UM232H)
                throw new NotSupportedException("Chicago requires UM232H.");

            var r = _um232h!.SpiWriteAndReadForChicago(tx.ToArray(), rx.Length);
            r.AsSpan(0, Math.Min(r.Length, rx.Length)).CopyTo(rx);
        }

        /// <summary>
        /// Chicago 타겟 통신 버스를 강제로 대기(Idle High) 상태로 만듭니다.
        /// </summary>
        public void ChicagoForceIdleHigh()
        {
            EnsureConnected();

            if (_settings.DeviceKind != DeviceKind.UM232H)
                return;

            _um232h!.SetGpioLHighForChicago();
        }

        #endregion
    }
}