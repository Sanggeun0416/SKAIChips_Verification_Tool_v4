using System;
using System.Collections.Generic;
using System.Threading;

namespace SKAIChips_Verification_Tool.RegisterControl
{
    /// <summary>
    /// FTDI UM232H 모듈을 제어하기 위한 래퍼 클래스입니다.
    /// FTD2XX_NET 라이브러리의 MPSSE(Multi-Protocol Synchronous Serial Engine) 모드를 직접 제어하여
    /// I2C, SPI 통신 및 GPIO(ADBUS, ACBUS) 제어를 수행합니다.
    /// </summary>
    public sealed class UM232H : IDisposable
    {
        private readonly uint _deviceIndex;
        private readonly FTD2XX_NET.FTDI _ftdi = new FTD2XX_NET.FTDI();

        // MPSSE 명령어를 일괄 전송하기 위한 버퍼 큐
        private readonly CommandQueue _queue = new CommandQueue(65536);

        // 동기화 처리를 위한 이벤트 객체
        private readonly ManualResetEvent _busyEvent = new ManualResetEvent(true);

        private bool _disposed;
        private bool _isOpen;
        private int _numBytesToRead;

        private ushort _clockDivisor;

        // 하위 8핀 (ADBUS / GPIOL) 상태 및 방향 저장 (비트 플래그)
        private byte _lowPinsDirections = 0xFB;
        private byte _lowPinsStates = 0xFB;

        // 상위 8핀 (ACBUS / GPIOH) 상태 및 방향 저장 (비트 플래그)
        private byte _highPinsDirections = 0xFF;
        private byte _highPinsStates = 0xFE;

        // SPI 통신 설정 상태
        private int _spiMode;
        private Command.BitFirst _spiBitFirst;
        private Command.ClockEdge _spiEdge;

        /// <summary>
        /// UM232H 디바이스가 현재 열려 있는지 여부를 가져옵니다.
        /// </summary>
        public bool IsOpen => _isOpen;

        /// <summary>
        /// 현재 설정된 MPSSE 클럭 속도(KHz)를 가져옵니다.
        /// </summary>
        public double CurrentClockKHz => GetClockKHz();

        private enum GPIO_Direction
        {
            Input,
            Output
        }

        /// <summary>
        /// UM232H 클래스의 새 인스턴스를 초기화합니다.
        /// </summary>
        /// <param name="deviceIndex">FTDI 디바이스 목록의 인덱스 번호입니다.</param>
        public UM232H(uint deviceIndex)
        {
            _deviceIndex = deviceIndex;
        }

        /// <summary>
        /// UM232H 디바이스를 열고 MPSSE 모드로 진입하기 위한 초기 설정을 수행합니다.
        /// (동기화 확인을 위한 'Bad Command' 테스트 포함)
        /// </summary>
        public bool Open()
        {
            ThrowIfDisposed();

            if (_isOpen)
                return true;

            var status = _ftdi.OpenByIndex(_deviceIndex);
            if (status != FTD2XX_NET.FTDI.FT_STATUS.FT_OK)
                return false;

            // MPSSE 진입을 위한 기본 파라미터 리셋 및 설정
            status |= _ftdi.ResetDevice();
            status |= _ftdi.SetCharacters(0, false, 0, false);
            status |= _ftdi.SetTimeouts(5000, 5000);
            status |= _ftdi.SetLatency(16);
            status |= _ftdi.SetBitMode(0x00, 0x00); // 리셋
            status |= _ftdi.SetBitMode(0x00, 0x02); // MPSSE 모드 켜기

            if (status != FTD2XX_NET.FTDI.FT_STATUS.FT_OK)
            {
                _ftdi.Close();
                return false;
            }

            Thread.Sleep(50);

            // MPSSE 동기화 확인: 잘못된 명령어(0xAA, 0xAB)를 보내고 칩이 'BadCommand(0xFA)'를 반환하는지 테스트
            for (byte b = 0xAA; b <= 0xAB; b++)
            {
                _queue.Clear();
                _queue.Set(b);
                if (!SendCommand())
                {
                    _ftdi.Close();
                    return false;
                }

                var recv = GetReceivedBytes(2);
                if (recv == null || recv.Length < 2)
                {
                    _ftdi.Close();
                    return false;
                }

                bool ok = false;
                for (int i = 0; i < recv.Length - 1; i++)
                {
                    if (recv[i] == Command.BadCommand && recv[i + 1] == b)
                    {
                        ok = true;
                        break;
                    }
                }

                if (!ok)
                {
                    _ftdi.Close();
                    return false;
                }
            }

            // 기본 클럭 및 루프백 설정 초기화
            _queue.Clear();
            _queue.Set(Command.Clock.Disable5Divisor);
            _queue.Set(Command.Loopback.Disable);
            SendCommand();

            // GPIO(ADBUS, ACBUS) 초기 상태 출력
            _queue.Set(Command.GPIO.SetGPIOL);
            _queue.Set(_lowPinsStates);
            _queue.Set(_lowPinsDirections);
            _queue.Set(Command.GPIO.SetGPIOH);
            _queue.Set(_highPinsStates);
            _queue.Set(_highPinsDirections);
            SendCommand();

            _isOpen = true;
            return true;
        }

        /// <summary>
        /// 디바이스 연결을 안전하게 종료합니다.
        /// </summary>
        public void Close()
        {
            ThrowIfDisposed();

            if (!_isOpen)
                return;

            _ftdi.Close();
            _isOpen = false;
            _numBytesToRead = 0;
        }

        /// <summary>
        /// UM232H의 MPSSE 엔진을 I2C 마스터 모드로 사용하기 위한 설정을 구성합니다.
        /// (오픈 드레인 설정을 위한 DriveOnlyZero 및 I2C용 3상 클럭킹 활성화)
        /// </summary>
        public bool I2cInit(int clockKHz)
        {
            ThrowIfDisposed();
            EnsureOpen();

            _queue.Clear();
            _queue.Set(Command.AdaptiveClocking.Disable);
            _queue.Set(Command.ThreePhase.Enable);

            // I2C Open-Drain 에뮬레이션을 위해 특정 핀에 Drive-Zero 모드 활성화
            _queue.Set(Command.DriveOnlyZero);
            _queue.Set(7); // LSB 3비트 설정 (SDA, SCL 등)
            _queue.Set(0);

            if (!SendCommand())
                return false;

            if (!SetClock(clockKHz))
                return false;

            Thread.Sleep(20);

            GPIOL_SetPins(0xF0, 0xF0, false);
            GPIOH_SetPins(0xFF, 0xFE);
            SendCommand();

            Thread.Sleep(30);
            return true;
        }

        /// <summary>
        /// I2C Start 조건 -> 주소 -> 데이터 전송 -> Stop 조건을 순차적으로 생성하여 데이터를 씁니다.
        /// </summary>
        public bool I2cWrite(byte slaveAddress7bit, byte[] data, bool sendStop)
        {
            ThrowIfDisposed();
            EnsureOpen();

            if (data == null || data.Length == 0)
                return true;

            _queue.Clear();
            _numBytesToRead = 0;

            I2C_SetStart();
            I2C_SetWriteByte((byte)((slaveAddress7bit << 1) | 0x00)); // 주소 + Write 비트(0)
            for (int i = 0; i < data.Length; i++)
            {
                I2C_SetWriteByte(data[i]);
            }
            if (sendStop)
                I2C_SetStop();

            if (SendCommand(true))
            {
                var recv = GetReceivedBytes(_numBytesToRead);
                if (recv != null && recv.Length >= data.Length)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// I2C 버스에서 지정된 길이만큼 데이터를 읽어옵니다.
        /// </summary>
        public byte[] I2cRead(byte slaveAddress7bit, int length)
        {
            ThrowIfDisposed();
            EnsureOpen();

            if (length <= 0)
                return Array.Empty<byte>();

            _queue.Clear();
            _numBytesToRead = 0;

            I2C_SetStart();
            I2C_SetWriteByte((byte)((slaveAddress7bit << 1) | 0x01)); // 주소 + Read 비트(1)
            for (int i = 0; i < length; i++)
            {
                I2C_SetReadByte(i == length - 1); // 마지막 바이트는 NAK 응답
            }
            I2C_SetStop();

            if (SendCommand(true))
            {
                var recv = GetReceivedBytes(_numBytesToRead);
                if (recv != null && recv.Length >= length)
                {
                    var result = new byte[length];
                    // 수신 데이터 순서 뒤집기
                    for (int i = 0; i < length; i++)
                    {
                        result[length - 1 - i] = recv[recv.Length - 1 - i];
                    }
                    return result;
                }
            }

            return Array.Empty<byte>();
        }

        /// <summary>
        /// I2C 트랜잭션에서 연속적으로 데이터를 쓰고 바로 다시 읽어옵니다. (Repeated Start 사용)
        /// </summary>
        public byte[] I2cWriteAndRead(byte slaveAddress7bit, byte[] writeData, int numBytesToWrite, int numBytesToRead)
        {
            ThrowIfDisposed();
            EnsureOpen();

            if (numBytesToRead <= 0)
                return Array.Empty<byte>();

            if (writeData == null || numBytesToWrite <= 0)
                numBytesToWrite = 0;

            _queue.Clear();
            _numBytesToRead = 0;

            I2C_SetStart();
            I2C_SetWriteByte((byte)((slaveAddress7bit << 1) | 0x00));
            for (int i = 0; i < numBytesToWrite; i++)
            {
                I2C_SetWriteByte(writeData[i]);
            }
            I2C_SetStop(); // 일부 칩셋 호환을 위해 Stop 후 딜레이

            // Repeated Start 대신 Stop 후 딜레이 확보
            for (int j = 0; j < 10; j++)
            {
                GPIOL_SetPins(_lowPinsDirections, (byte)((_lowPinsStates & 0xF0u) | 0x00u), false);
            }

            I2C_SetStart();
            I2C_SetWriteByte((byte)((slaveAddress7bit << 1) | 0x01));
            for (int k = 0; k < numBytesToRead; k++)
            {
                I2C_SetReadByte(k == numBytesToRead - 1);
            }
            I2C_SetStop();

            if (SendCommand(true))
            {
                var recv = GetReceivedBytes(_numBytesToRead);
                if (recv != null && recv.Length >= numBytesToRead)
                {
                    var result = new byte[numBytesToRead];
                    for (int i = 0; i < numBytesToRead; i++)
                    {
                        result[numBytesToRead - 1 - i] = recv[recv.Length - 1 - i];
                    }
                    return result;
                }
            }

            return Array.Empty<byte>();
        }

        /// <summary>
        /// UM232H의 MPSSE 엔진을 SPI 마스터 모드로 설정합니다.
        /// </summary>
        public bool SpiInit(int mode, int clockKHz, bool lsbFirst)
        {
            ThrowIfDisposed();
            EnsureOpen();

            _queue.Clear();
            _queue.Set(Command.AdaptiveClocking.Disable);
            _queue.Set(Command.ThreePhase.Disable);
            if (!SendCommand())
                return false;

            if (!SetClock(clockKHz))
                return false;

            Thread.Sleep(20);

            _spiMode = mode;
            _spiBitFirst = lsbFirst ? Command.BitFirst.LSB : Command.BitFirst.MSB;
            _spiEdge = (mode == 0 || mode == 3)
                ? Command.ClockEdge.OutRising
                : Command.ClockEdge.InFalling;

            // SPI 모드(Clock Polarity)에 따라 초기 핀 상태 결정
            if (_spiMode < 2)
            {
                GPIOL_SetPins(0xFB, 0xF8, false);
            }
            else
            {
                GPIOL_SetPins(0xFB, 0xF9, false);
            }

            GPIOH_SetPins(0xFF, 0xFE);
            SendCommand();
            Thread.Sleep(30);

            return true;
        }

        /// <summary>
        /// SPI 통신으로 데이터를 전송합니다.
        /// </summary>
        public void SpiWrite(byte[] data, bool endTransaction)
        {
            ThrowIfDisposed();
            EnsureOpen();

            if (data == null || data.Length == 0)
                return;

            _queue.Clear();
            _numBytesToRead = 0;

            SPI_SetStart();
            SPI_SetBytes(data, (ushort)data.Length, Command.PinConfig.Write);
            SPI_SetStop();

            SendCommand(endTransaction);
        }

        /// <summary>
        /// SPI 통신으로 데이터를 수신합니다.
        /// </summary>
        public byte[] SpiRead(int length)
        {
            ThrowIfDisposed();
            EnsureOpen();

            if (length <= 0)
                return Array.Empty<byte>();

            _queue.Clear();
            _numBytesToRead = 0;

            SPI_SetStart();
            SPI_SetBytes(null, (ushort)length, Command.PinConfig.Read);
            SPI_SetStop();

            if (SendCommand(true))
            {
                var recv = GetReceivedBytes(length);
                return recv ?? Array.Empty<byte>();
            }

            return Array.Empty<byte>();
        }

        /// <summary>
        /// SPI 통신에서 데이터를 씀과 동시에 읽어옵니다. (Full-Duplex)
        /// </summary>
        public byte[] SpiReadWrite(byte[] writeData, int length)
        {
            ThrowIfDisposed();
            EnsureOpen();

            if (length <= 0)
                return Array.Empty<byte>();
            if (writeData == null || writeData.Length < length)
                throw new ArgumentException("writeData length must be >= length", nameof(writeData));

            _queue.Clear();
            _numBytesToRead = 0;

            SPI_SetStart();
            SPI_SetBytes(writeData, (ushort)length, Command.PinConfig.ReadWrite);
            SPI_SetStop();

            if (SendCommand(true))
            {
                var recv = GetReceivedBytes(length);
                return recv ?? Array.Empty<byte>();
            }

            return Array.Empty<byte>();
        }

        #region Chicago-Specific Methods
        /// <summary>
        /// Chicago 타겟 전용: SPI 읽기/쓰기 완료 후 대기 상태(Idle High)를 강제 적용합니다.
        /// </summary>
        public void SetGpioLHighForChicago()
        {
            ThrowIfDisposed();
            EnsureOpen();

            _queue.Clear();
            _numBytesToRead = 0;

            GPIOL_SetPins(0xFB, 0xFF, false);
            SendCommand();
        }

        /// <summary>
        /// Chicago 타겟 전용: SPI 읽기/쓰기 복합 동작 수행 후, Data Out(DO) 핀을 입력으로 전환하고 Idle High를 유지합니다.
        /// </summary>
        public byte[] SpiWriteAndReadForChicago(byte[] writeBytes, int numBytesToRead)
        {
            ThrowIfDisposed();
            EnsureOpen();

            if (writeBytes == null)
                throw new ArgumentNullException(nameof(writeBytes));
            if (numBytesToRead <= 0)
                return Array.Empty<byte>();

            _queue.Clear();
            _numBytesToRead = 0;

            SPI_SetStart();
            SPI_SetBytes(writeBytes, (ushort)writeBytes.Length, Command.PinConfig.Write);

            // Chicago 전용: Half-Duplex 전환 (DO 핀을 Input으로 설정)
            GPIOL_SetPins(0xF9, (byte)((_lowPinsStates & 0xF0u) | 0x00u), false);

            SPI_SetBytes(null, (ushort)numBytesToRead, Command.PinConfig.Read);
            SPI_SetStop();

            SendCommand(false);
            var recv = GetReceivedBytes(numBytesToRead) ?? Array.Empty<byte>();

            // Idle High 상태 복구 유지
            _queue.Clear();
            GPIOL_SetPins(0xFB, 0xFF, false);
            SendCommand();

            return recv;
        }
        #endregion

        #region Hardware Layer & Low-level MPSSE Methods
        /// <summary>
        /// MPSSE 통신 클럭 분주비를 계산하고 디바이스에 설정합니다.
        /// </summary>
        private bool SetClock(int clockKHz)
        {
            double baseKHz = 60000.0;
            ushort divisor = (ushort)(baseKHz / (2.0 * clockKHz) - 1.0);

            _queue.Set(Command.Clock.SetDivisor);
            _queue.Set((byte)(divisor & 0xFF));
            _queue.Set((byte)((divisor >> 8) & 0xFF));

            if (SendCommand())
            {
                _clockDivisor = divisor;
                return true;
            }
            return false;
        }

        private double GetClockKHz()
        {
            if (!_isOpen)
                return 0.0;

            double baseKHz = 60000.0;
            return baseKHz / ((1.0 + _clockDivisor) * 2.0);
        }

        /// <summary>
        /// ADBUS (GPIOL, 하위 8핀)의 방향과 상태를 설정하는 MPSSE 명령을 큐에 추가합니다.
        /// </summary>
        private void GPIOL_SetPins(byte directions, byte states, bool protect)
        {
            if (protect)
            {
                var current = GPIOL_GetPins();
                _lowPinsStates = (byte)((states & 0xF0u) | (current & 0x0Fu));
                _lowPinsDirections = (byte)((directions & 0xF0u) | (_lowPinsDirections & 0x0Fu));
            }
            else
            {
                _lowPinsStates = states;
                _lowPinsDirections = directions;
            }

            _queue.Set(Command.GPIO.SetGPIOL);
            _queue.Set(_lowPinsStates);
            _queue.Set(_lowPinsDirections);
        }

        private byte GPIOL_GetPins()
        {
            byte bitMode = 0;
            WaitBusyEvent(200, "GPIOL_GetPins");
            _ftdi.GetPinStates(ref bitMode);
            _lowPinsStates = bitMode;
            _busyEvent.Set();
            return _lowPinsStates;
        }

        /// <summary>
        /// ACBUS (GPIOH, 상위 8핀)의 방향과 상태를 설정하는 MPSSE 명령을 큐에 추가합니다.
        /// </summary>
        private void GPIOH_SetPins(byte directions, byte states)
        {
            _highPinsStates = states;
            _highPinsDirections = directions;

            _queue.Set(Command.GPIO.SetGPIOH);
            _queue.Set(_highPinsStates);
            _queue.Set(_highPinsDirections);
        }

        /// <summary>
        /// 소프트웨어적으로 I2C Start 파형을 생성합니다. (SDA 하강 -> SCL 하강)
        /// </summary>
        private void I2C_SetStart()
        {
            byte directions = (byte)((_lowPinsDirections & 0xF0u) | 0x03u);
            for (int i = 0; i < 20; i++)
            {
                GPIOL_SetPins(directions, (byte)((_lowPinsStates & 0xF0u) | 0x03u), false);
            }
            for (int j = 0; j < 20; j++)
            {
                GPIOL_SetPins(directions, (byte)((_lowPinsStates & 0xF0u) | 0x01u), false); // SDA 하강
            }
            GPIOL_SetPins(directions, (byte)((_lowPinsStates & 0xF0u) | 0x00u), false); // SCL 하강
        }

        /// <summary>
        /// 소프트웨어적으로 I2C Stop 파형을 생성합니다. (SCL 상승 -> SDA 상승)
        /// </summary>
        private void I2C_SetStop()
        {
            byte directions0 = (byte)((_lowPinsDirections & 0xF0u) | 0x00u);
            byte directions3 = (byte)((_lowPinsDirections & 0xF0u) | 0x03u);

            for (int i = 0; i < 20; i++)
            {
                GPIOL_SetPins(directions3, (byte)((_lowPinsStates & 0xF0u) | 0x01u), false); // SCL 상승 준비
            }
            for (int j = 0; j < 20; j++)
            {
                GPIOL_SetPins(directions3, (byte)((_lowPinsStates & 0xF0u) | 0x03u), false); // SDA 상승 (Stop)
            }

            GPIOL_SetPins(directions0, (byte)((_lowPinsStates & 0xF0u) | 0x00u), false);
        }

        private void I2C_SetWriteByte(byte data)
        {
            byte directions = (byte)((_lowPinsDirections & 0xF0u) | 0x03u);
            SSC_SetBits(data, 8, Command.PinConfig.Write, Command.BitFirst.MSB, Command.ClockEdge.OutRising);

            for (int i = 0; i < 20; i++)
            {
                GPIOL_SetPins(directions, (byte)((_lowPinsStates & 0xF0u) | 0x02u), false);
            }

            // ACK 비트 수신
            SSC_SetBits(0, 1, Command.PinConfig.Read, Command.BitFirst.MSB, Command.ClockEdge.OutFalling);

            for (int j = 0; j < 100; j++)
            {
                GPIOL_SetPins(directions, (byte)((_lowPinsStates & 0xF0u) | 0x02u), false);
            }
        }

        private void I2C_SetReadByte(bool nak)
        {
            byte directions = (byte)((_lowPinsDirections & 0xF0u) | 0x03u);

            for (int i = 0; i < 50; i++)
            {
                GPIOL_SetPins(directions, (byte)((_lowPinsStates & 0xF0u) | 0x02u), false);
            }

            // 8비트 데이터 수신
            SSC_SetBits(0, 8, Command.PinConfig.Read, Command.BitFirst.MSB, Command.ClockEdge.OutFalling);
            // ACK/NAK 송신
            SSC_SetBits((byte)(nak ? 0xFF : 0x00), 1, Command.PinConfig.Write, Command.BitFirst.MSB, Command.ClockEdge.OutRising);
        }

        private void SSC_SetBits(byte sendBits, byte length, Command.PinConfig config, Command.BitFirst first, Command.ClockEdge edge)
        {
            if (length > 8 || length < 1)
                return;

            _queue.Set(Command.Get(config, edge, Command.DataUnit.Bit, first));
            _queue.Set((byte)(length - 1));

            if (config == Command.PinConfig.Write || config == Command.PinConfig.ReadWrite)
            {
                _queue.Set(sendBits);
            }
            if (config == Command.PinConfig.Read || config == Command.PinConfig.ReadWrite)
            {
                _numBytesToRead++;
            }
        }

        private void SSC_SetBytes(byte[] bytes, ushort length, Command.PinConfig config, Command.BitFirst first, Command.ClockEdge edge)
        {
            if (config == Command.PinConfig.TMS_Write ||
                config == Command.PinConfig.TMS_ReadWrite ||
                ((config == Command.PinConfig.Write || config == Command.PinConfig.ReadWrite) &&
                 (bytes == null || bytes.Length < length)))
            {
                return;
            }

            _queue.Set(Command.Get(config, edge, Command.DataUnit.Byte, first));
            _queue.Set((byte)((length - 1) & 0xFF));
            _queue.Set((byte)(((length - 1) >> 8) & 0xFF));

            if (config == Command.PinConfig.Write || config == Command.PinConfig.ReadWrite)
            {
                for (int i = 0; i < length; i++)
                {
                    _queue.Set(bytes[i]);
                }
            }
            if (config == Command.PinConfig.Read || config == Command.PinConfig.ReadWrite)
            {
                _numBytesToRead += length;
            }
        }

        private void SPI_SetBytes(byte[] bytes, ushort length, Command.PinConfig config)
        {
            SSC_SetBytes(bytes, length, config, _spiBitFirst, _spiEdge);
        }

        private void SPI_SetStart()
        {
            if (_spiMode < 2)
            {
                for (int i = 0; i < 10; i++)
                    GPIOL_SetPins(_lowPinsDirections, (byte)((_lowPinsStates & 0xF0u) | 0x08u), false);
                GPIOL_SetPins(_lowPinsDirections, (byte)((_lowPinsStates & 0xF0u) | 0x00u), false); // CS 하강
            }
            else
            {
                for (int i = 0; i < 10; i++)
                    GPIOL_SetPins(_lowPinsDirections, (byte)((_lowPinsStates & 0xF0u) | 0x09u), false);
                GPIOL_SetPins(_lowPinsDirections, (byte)((_lowPinsStates & 0xF0u) | 0x01u), false); // CS 하강
            }
        }

        private void SPI_SetStop()
        {
            if (_spiMode < 2)
            {
                for (int i = 0; i < 10; i++)
                    GPIOL_SetPins(_lowPinsDirections, (byte)((_lowPinsStates & 0xF0u) | 0x00u), false);
                GPIOL_SetPins(_lowPinsDirections, (byte)((_lowPinsStates & 0xF0u) | 0x08u), false); // CS 상승
            }
            else
            {
                for (int i = 0; i < 10; i++)
                    GPIOL_SetPins(_lowPinsDirections, (byte)((_lowPinsStates & 0xF0u) | 0x01u), false);
                GPIOL_SetPins(_lowPinsDirections, (byte)((_lowPinsStates & 0xF0u) | 0x09u), false); // CS 상승
            }
        }
        #endregion

        #region GPIO API (Public Methods)
        public void SetGpioHDirection(int bitIndex, bool output)
        {
            ThrowIfDisposed();
            EnsureOpen();

            if (bitIndex < 0 || bitIndex > 7)
                throw new ArgumentOutOfRangeException(nameof(bitIndex));

            byte mask = (byte)(1 << bitIndex);
            if (output)
                _highPinsDirections |= mask;
            else
                _highPinsDirections &= (byte)~mask;

            GPIOH_SetPins(_highPinsDirections, _highPinsStates);
            SendCommand();
        }

        public void SetGpioHPin(int bitIndex, bool high)
        {
            ThrowIfDisposed();
            EnsureOpen();

            if (bitIndex < 0 || bitIndex > 7)
                throw new ArgumentOutOfRangeException(nameof(bitIndex));

            byte mask = (byte)(1 << bitIndex);
            if (high)
                _highPinsStates |= mask;
            else
                _highPinsStates &= (byte)~mask;

            GPIOH_SetPins(_highPinsDirections, _highPinsStates);
            SendCommand();
        }

        public bool GetGpioHPin(int bitIndex)
        {
            ThrowIfDisposed();
            EnsureOpen();

            if (bitIndex < 0 || bitIndex > 7)
                throw new ArgumentOutOfRangeException(nameof(bitIndex));

            _queue.Clear();
            _queue.Set(Command.GPIO.GetGPIOH);

            if (SendCommand(sendAnswerBackImmediately: true))
            {
                var recv = GetReceivedBytes(1);
                if (recv != null && recv.Length > 0)
                    return (recv[0] & (1 << bitIndex)) != 0;
            }
            return false;
        }

        public void SetGpioLDirection(int bitIndex, bool output)
        {
            ThrowIfDisposed();
            EnsureOpen();

            if (bitIndex < 4 || bitIndex > 7)
                throw new ArgumentOutOfRangeException(nameof(bitIndex), "Only ADBUS 4~7 are allowed for GPIO.");

            byte mask = (byte)(1 << bitIndex);
            if (output)
                _lowPinsDirections |= mask;
            else
                _lowPinsDirections &= (byte)~mask;

            GPIOL_SetPins(_lowPinsDirections, _lowPinsStates, protect: true);
            SendCommand();
        }

        public void SetGpioLPin(int bitIndex, bool high)
        {
            ThrowIfDisposed();
            EnsureOpen();

            if (bitIndex < 4 || bitIndex > 7)
                throw new ArgumentOutOfRangeException(nameof(bitIndex), "Only ADBUS 4~7 are allowed for GPIO.");

            byte mask = (byte)(1 << bitIndex);
            if (high)
                _lowPinsStates |= mask;
            else
                _lowPinsStates &= (byte)~mask;

            GPIOL_SetPins(_lowPinsDirections, _lowPinsStates, protect: true);
            SendCommand();
        }

        public bool GetGpioLPin(int bitIndex)
        {
            ThrowIfDisposed();
            EnsureOpen();

            if (bitIndex < 4 || bitIndex > 7)
                throw new ArgumentOutOfRangeException(nameof(bitIndex), "Only ADBUS 4~7 are allowed for GPIO.");

            _queue.Clear();
            _queue.Set(Command.GPIO.GetGPIOL);

            if (SendCommand(sendAnswerBackImmediately: true))
            {
                var recv = GetReceivedBytes(1);
                if (recv != null && recv.Length > 0)
                    return (recv[0] & (1 << bitIndex)) != 0;
            }
            return false;
        }

        public void SetGpioHLow()
        {
            ThrowIfDisposed();
            EnsureOpen();

            _queue.Clear();
            GPIOH_SetPins(0xFF, 0x00);
            SendCommand();
        }
        #endregion

        #region Communication & Utilities
        /// <summary>
        /// 큐에 누적된 MPSSE 명령어들을 FTDI 칩으로 일괄 전송합니다.
        /// </summary>
        private bool SendCommand(bool sendAnswerBackImmediately = false)
        {
            uint written = 0;
            if (sendAnswerBackImmediately)
            {
                _queue.Set(Command.SendAnswerBackImmediately);
            }

            var bytes = _queue.GetBytes(_queue.Count);
            if (bytes.Length == 0)
                return true;

            WaitBusyEvent(200, "SendCommand");
            var status = _ftdi.Write(bytes, bytes.Length, ref written);
            _busyEvent.Set();

            return status == FTD2XX_NET.FTDI.FT_STATUS.FT_OK;
        }

        /// <summary>
        /// FTDI 칩의 수신 버퍼에서 데이터를 읽어옵니다.
        /// </summary>
        private byte[] GetReceivedBytes(int length)
        {
            uint rxQueue = 0;
            uint read = 0;
            int tries = 0;
            byte[] buf = null;

            WaitBusyEvent(200, "GetReceivedBytes");
            FTD2XX_NET.FTDI.FT_STATUS status;
            do
            {
                Thread.Sleep(1);
                status = _ftdi.GetRxBytesAvailable(ref rxQueue);
                tries++;
            }
            while (rxQueue < length && tries < 500);

            if (status == FTD2XX_NET.FTDI.FT_STATUS.FT_OK && length <= rxQueue)
            {
                buf = new byte[rxQueue];
                _ftdi.Read(buf, rxQueue, ref read);
                _numBytesToRead = 0;
            }

            _busyEvent.Set();
            return buf;
        }

        private bool WaitBusyEvent(int timeoutMs, string debug)
        {
            if (_busyEvent.WaitOne(timeoutMs))
            {
                _busyEvent.Reset();
                return true;
            }

            Console.WriteLine($"{DateTime.Now:HH:mm:ss.fff}> UM232H:BDF:{debug}");
            return false;
        }

        private void EnsureOpen()
        {
            if (!_isOpen)
                throw new InvalidOperationException("UM232H device is not open.");
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(UM232H));
        }

        public void Dispose()
        {
            if (_disposed)
                return;
            Close();
            _disposed = true;
        }
        #endregion

        #region Inner Classes (Queue & MPSSE Opcodes)
        /// <summary>
        /// FTDI 칩으로 전송할 여러 개의 MPSSE 명령어(Opcode 및 Data)를 버퍼링하는 큐입니다.
        /// 한 번의 Write 호출로 오버헤드를 줄이기 위해 사용됩니다.
        /// </summary>
        private sealed class CommandQueue
        {
            private readonly List<byte> _buffer;
            public int Count => _buffer.Count;

            public CommandQueue(int capacity)
            {
                _buffer = new List<byte>(capacity);
            }

            public void Clear() => _buffer.Clear();
            public void Set(byte value) => _buffer.Add(value);
            public byte[] GetBytes(int count) => _buffer.ToArray();
        }

        /// <summary>
        /// FTDI MPSSE 모드에서 사용되는 명령어(Opcode) 상수 모음입니다.
        /// </summary>
        private static class Command
        {
            public static class Clock
            {
                public const byte SetDivisor = 0x86;
                public const byte Disable5Divisor = 0x8A;
                public const byte Enable5Divisor = 0x8B;
            }

            public static class GPIO
            {
                public const byte SetGPIOL = 0x80;
                public const byte GetGPIOL = 0x81;
                public const byte SetGPIOH = 0x82;
                public const byte GetGPIOH = 0x83;
            }

            public static class Loopback
            {
                public const byte Enable = 0x84;
                public const byte Disable = 0x85;
            }

            public static class ThreePhase
            {
                public const byte Enable = 0x8C;
                public const byte Disable = 0x8D;
            }

            public static class AdaptiveClocking
            {
                public const byte Enable = 0x96;
                public const byte Disable = 0x97;
            }

            public enum PinConfig
            {
                Write = 0x10,
                Read = 0x20,
                ReadWrite = 0x30,
                TMS_Write = 0x40,
                TMS_ReadWrite = 0x60
            }

            public enum DataUnit
            {
                Byte = 0x00,
                Bit = 0x02
            }

            public enum BitFirst
            {
                MSB = 0x00,
                LSB = 0x08
            }

            public enum ClockEdge
            {
                OutRising = 0x01,
                OutFalling = 0x00,
                InRising = 0x00,
                InFalling = 0x04
            }

            public const byte BadCommand = 0xFA;
            public const byte SendAnswerBackImmediately = 0x87;
            public const byte DriveOnlyZero = 0x9E;

            /// <summary>
            /// 핀 동작, 클럭 엣지, 데이터 단위(Bit/Byte), LSB/MSB 설정값을 OR 연산하여 최종 Opcode를 생성합니다.
            /// </summary>
            public static byte Get(PinConfig config, ClockEdge edge, DataUnit unit, BitFirst first)
            {
                if ((config == PinConfig.TMS_Write || config == PinConfig.TMS_ReadWrite) && unit == DataUnit.Byte)
                {
                    unit = DataUnit.Bit;
                }
                return (byte)((byte)config | (byte)edge | (byte)unit | (byte)first);
            }
        }
        #endregion
    }
}