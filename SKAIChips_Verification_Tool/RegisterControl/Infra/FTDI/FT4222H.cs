using System;
using System.Runtime.InteropServices;

namespace SKAIChips_Verification_Tool.RegisterControl
{
    /// <summary>
    /// FTDI FT4222H USB-to-I2C/SPI/GPIO 브릿지 칩셋을 제어하기 위한 네이티브 라이브러리(FTD2XX, LibFT4222) 래퍼 클래스입니다.
    /// IDisposable을 구현하여 사용 후 하드웨어 리소스와 핸들을 안전하게 해제합니다.
    /// </summary>
    public sealed class FT4222H : IDisposable
    {
        private IntPtr _handle = IntPtr.Zero;
        private bool _disposed;

        /// <summary>
        /// FT4222H 디바이스가 현재 정상적으로 열려 있는지 여부를 가져옵니다.
        /// </summary>
        public bool IsOpen => _handle != IntPtr.Zero;

        /// <summary>
        /// 현재 설정된 I2C 통신 속도(Kbps)를 가져옵니다.
        /// </summary>
        public ushort CurrentI2cSpeedKbps
        {
            get; private set;
        }

        /// <summary>
        /// 현재 설정된 SPI 통신 클럭 속도(KHz)를 가져옵니다.
        /// </summary>
        public int CurrentSpiClockKHz
        {
            get; private set;
        }

        /// <summary>
        /// 지정된 디바이스 인덱스를 사용하여 FT4222H 장치를 엽니다.
        /// </summary>
        /// <param name="deviceIndex">열고자 하는 장치의 0 기반 인덱스입니다.</param>
        /// <returns>장치 연결 및 핸들 획득에 성공하면 true, 실패하면 false를 반환합니다.</returns>
        public bool Open(uint deviceIndex)
        {
            ThrowIfDisposed();

            if (IsOpen)
                return true;

            uint deviceCount = 0;
            var status = FT_CreateDeviceInfoList(ref deviceCount);
            if (status != FT_STATUS.FT_OK || deviceCount == 0 || deviceIndex >= deviceCount)
                return false;

            status = FT_Open(deviceIndex, out _handle);
            if (status != FT_STATUS.FT_OK || _handle == IntPtr.Zero)
            {
                _handle = IntPtr.Zero;
                return false;
            }

            return true;
        }

        /// <summary>
        /// 열려 있는 FT4222H 장치의 I2C/SPI 상태를 초기화하고 연결을 닫습니다.
        /// </summary>
        public void Close()
        {
            ThrowIfDisposed();

            if (_handle != IntPtr.Zero)
            {
                FT4222_I2CMaster_Reset(_handle);
                FT4222_SPI_Reset(_handle);
                FT4222_UnInitialize(_handle);
                FT_Close(_handle);

                _handle = IntPtr.Zero;
                CurrentI2cSpeedKbps = 0;
                CurrentSpiClockKHz = 0;
            }
        }

        /// <summary>
        /// FT4222H를 I2C 마스터 모드로 초기화하고 통신 속도를 설정합니다.
        /// </summary>
        /// <param name="kbps">I2C 통신 속도 (단위: Kbps, 예: 100, 400)</param>
        /// <returns>초기화에 성공하면 true, 실패하면 false를 반환합니다.</returns>
        public bool I2cInit(ushort kbps)
        {
            ThrowIfDisposed();
            EnsureOpen();

            var status = FT4222_I2CMaster_Init(_handle, kbps);
            if (status != FT4222_STATUS.FT4222_OK)
                return false;

            FT4222_I2CMaster_Reset(_handle);
            CurrentI2cSpeedKbps = kbps;
            return true;
        }

        /// <summary>
        /// 지정된 I2C 슬레이브 주소로 데이터를 전송합니다.
        /// </summary>
        /// <param name="slaveAddress">I2C 슬레이브 디바이스의 7비트 주소입니다.</param>
        /// <param name="data">전송할 바이트 배열입니다.</param>
        /// <returns>실제로 전송된 바이트 수를 반환합니다.</returns>
        /// <exception cref="InvalidOperationException">I2C 쓰기 작업이 실패했을 때 발생합니다.</exception>
        public ushort I2cWrite(ushort slaveAddress, byte[] data)
        {
            ThrowIfDisposed();
            EnsureOpen();

            if (data == null || data.Length == 0)
                return 0;

            ushort transferred = 0;
            var status = FT4222_I2CMaster_Write(
                _handle,
                slaveAddress,
                data,
                (ushort)data.Length,
                ref transferred);

            if (status != FT4222_STATUS.FT4222_OK)
                throw new InvalidOperationException($"FT4222 I2C write failed. Status={status}, Written={transferred}");

            return transferred;
        }

        /// <summary>
        /// 지정된 I2C 슬레이브 주소로부터 데이터를 읽어옵니다.
        /// </summary>
        /// <param name="slaveAddress">I2C 슬레이브 디바이스의 7비트 주소입니다.</param>
        /// <param name="buffer">읽어온 데이터를 저장할 버퍼 배열입니다.</param>
        /// <returns>실제로 읽어온 바이트 수를 반환합니다.</returns>
        /// <exception cref="InvalidOperationException">I2C 읽기 작업이 실패했을 때 발생합니다.</exception>
        public ushort I2cRead(ushort slaveAddress, byte[] buffer)
        {
            ThrowIfDisposed();
            EnsureOpen();

            if (buffer == null || buffer.Length == 0)
                return 0;

            ushort transferred = 0;
            var status = FT4222_I2CMaster_Read(
                _handle,
                slaveAddress,
                buffer,
                (ushort)buffer.Length,
                ref transferred);

            if (status != FT4222_STATUS.FT4222_OK)
                throw new InvalidOperationException($"FT4222 I2C read failed. Status={status}, Read={transferred}");

            return transferred;
        }

        /// <summary>
        /// FT4222H를 SPI 마스터 모드로 초기화하고 클럭 속도 및 SPI 모드(0~3)를 설정합니다.
        /// </summary>
        /// <param name="sckRateKHz">목표 SPI 클럭 속도 (KHz 단위)</param>
        /// <param name="mode">SPI 통신 모드 (0, 1, 2, 3)</param>
        /// <returns>초기화 성공 시 true, 실패 시 false를 반환합니다.</returns>
        public bool Master(int sckRateKHz, int mode)
        {
            ThrowIfDisposed();
            EnsureOpen();

            GetSpiClockParameter(sckRateKHz, out SystemClock sysClk, out SPI_ClockDivider clkDiv);

            // SPI 모드에 따른 극성(CPOL) 및 위상(CPHA) 설정
            var cpol = (mode == 2 || mode == 3) ? SPI_ClkPolarity.ACTIVE_HIGH : SPI_ClkPolarity.ACTIVE_LOW;
            var cpha = (mode == 1 || mode == 3) ? SPI_ClkPhase.CLK_TRAILING : SPI_ClkPhase.CLK_LEADING;

            var st = FT4222_SetClock(_handle, sysClk);
            if (st != FT4222_STATUS.FT4222_OK)
                return false;

            st = FT4222_SPIMaster_Init(
                _handle,
                SPI_Mode.SPI_IO_SINGLE,
                clkDiv,
                cpol,
                cpha,
                0x01);

            if (st != FT4222_STATUS.FT4222_OK)
                return false;

            FT4222_SPI_SetDrivingStrength(
                _handle,
                SPI_DrivingStrength.DS_4MA,
                SPI_DrivingStrength.DS_4MA,
                SPI_DrivingStrength.DS_4MA);

            FT4222_SetSuspendOut(_handle, false);
            FT4222_SetWakeUpInterrupt(_handle, false);

            // 실제 설정된 클럭 속도 계산 및 저장
            switch (sysClk)
            {
                case SystemClock.SYS_CLK_80:
                    CurrentSpiClockKHz = 80000 / (1 << (int)clkDiv);
                    break;
                case SystemClock.SYS_CLK_60:
                    CurrentSpiClockKHz = 60000 / (1 << (int)clkDiv);
                    break;
                case SystemClock.SYS_CLK_48:
                    CurrentSpiClockKHz = 48000 / (1 << (int)clkDiv);
                    break;
                case SystemClock.SYS_CLK_24:
                    CurrentSpiClockKHz = 24000 / (1 << (int)clkDiv);
                    break;
            }

            return true;
        }

        /// <summary>
        /// SPI 버스를 통해 데이터를 전송합니다.
        /// </summary>
        /// <param name="writeBuf">전송할 데이터가 담긴 배열</param>
        /// <param name="endTransaction">전송 완료 후 CS(Chip Select) 핀을 비활성화(High)할지 여부</param>
        /// <returns>전송된 바이트 수</returns>
        public ushort SpiWriteBytes(byte[] writeBuf, bool endTransaction)
        {
            ThrowIfDisposed();
            EnsureOpen();

            if (writeBuf == null || writeBuf.Length == 0)
                return 0;

            ushort sizeTransferred = 0;
            var st = FT4222_SPIMaster_SingleWrite(
                _handle,
                ref writeBuf[0],
                (ushort)writeBuf.Length,
                ref sizeTransferred,
                endTransaction);

            if (st != FT4222_STATUS.FT4222_OK)
                throw new InvalidOperationException($"FT4222 SPI write failed. Status={st}, Written={sizeTransferred}");

            return sizeTransferred;
        }

        /// <summary>
        /// SPI 버스를 통해 데이터를 읽어옵니다.
        /// </summary>
        /// <param name="readBuf">읽어올 데이터를 저장할 배열</param>
        /// <param name="endTransaction">읽기 완료 후 CS(Chip Select) 핀을 비활성화(High)할지 여부</param>
        /// <returns>읽어온 바이트 수</returns>
        public ushort SpiReadBytes(byte[] readBuf, bool endTransaction)
        {
            ThrowIfDisposed();
            EnsureOpen();

            if (readBuf == null || readBuf.Length == 0)
                return 0;

            ushort sizeRead = 0;
            var st = FT4222_SPIMaster_SingleRead(
                _handle,
                ref readBuf[0],
                (ushort)readBuf.Length,
                ref sizeRead,
                endTransaction);

            if (st != FT4222_STATUS.FT4222_OK)
                throw new InvalidOperationException($"FT4222 SPI read failed. Status={st}, Read={sizeRead}");

            return sizeRead;
        }

        /// <summary>
        /// 전이중(Full-Duplex) 방식으로 SPI 버스에서 데이터를 동시에 쓰고 읽습니다.
        /// </summary>
        /// <param name="readBuf">수신된 데이터를 저장할 버퍼 (writeBuf 길이 이상이어야 함)</param>
        /// <param name="writeBuf">전송할 데이터가 담긴 버퍼</param>
        /// <param name="endTransaction">통신 완료 후 CS 핀을 비활성화(High)할지 여부</param>
        /// <returns>전송 및 수신된 바이트 수</returns>
        public ushort SpiReadWriteBytes(byte[] readBuf, byte[] writeBuf, bool endTransaction)
        {
            ThrowIfDisposed();
            EnsureOpen();

            if (writeBuf == null || writeBuf.Length == 0)
                return 0;
            if (readBuf == null || readBuf.Length < writeBuf.Length)
                throw new ArgumentException("readBuf length must be >= writeBuf length");

            ushort sizeTransferred = 0;
            var st = FT4222_SPIMaster_SingleReadWrite(
                _handle,
                ref readBuf[0],
                ref writeBuf[0],
                (ushort)writeBuf.Length,
                ref sizeTransferred,
                endTransaction);

            if (st != FT4222_STATUS.FT4222_OK)
                throw new InvalidOperationException($"FT4222 SPI read/write failed. Status={st}, Transferred={sizeTransferred}");

            return sizeTransferred;
        }

        /// <summary>
        /// 목표 SPI 클럭 속도에 가장 근접한 시스템 클럭과 디바이더(분주비) 값을 계산합니다.
        /// </summary>
        private void GetSpiClockParameter(int sckRateKHz, out SystemClock sysClk, out SPI_ClockDivider clkDiv)
        {
            sysClk = SystemClock.SYS_CLK_80;
            clkDiv = SPI_ClockDivider.CLK_DIV_8;

            for (int i = 1; i < 8; i++)
            {
                if (sckRateKHz >= 80000 / (1 << i))
                {
                    sysClk = SystemClock.SYS_CLK_80;
                    clkDiv = (SPI_ClockDivider)i;
                    break;
                }
                if (sckRateKHz >= 60000 / (1 << i))
                {
                    sysClk = SystemClock.SYS_CLK_60;
                    clkDiv = (SPI_ClockDivider)i;
                    break;
                }
                if (sckRateKHz >= 48000 / (1 << i))
                {
                    sysClk = SystemClock.SYS_CLK_48;
                    clkDiv = (SPI_ClockDivider)i;
                    break;
                }
            }
        }

        /// <summary>
        /// 4개의 핀에 대한 GPIO 입출력 방향을 초기화합니다.
        /// </summary>
        /// <param name="directions">포트 0~3에 대한 방향(INPUT/OUTPUT)을 담은 길이 4의 배열</param>
        public bool GpioInit(GPIO_Direction[] directions)
        {
            ThrowIfDisposed();
            EnsureOpen();

            if (directions == null || directions.Length != 4)
                throw new ArgumentException("GPIO directions array must have exactly 4 elements.");

            var status = FT4222_GPIO_Init(_handle, directions);
            return status == FT4222_STATUS.FT4222_OK;
        }

        /// <summary>
        /// 지정된 GPIO 포트에 디지털 값(High/Low)을 출력합니다.
        /// </summary>
        /// <param name="port">출력할 GPIO 포트 번호</param>
        /// <param name="value">출력할 상태값 (true: High, false: Low)</param>
        public bool GpioWrite(GPIO_Port port, bool value)
        {
            ThrowIfDisposed();
            EnsureOpen();

            var status = FT4222_GPIO_Write(_handle, port, value);
            return status == FT4222_STATUS.FT4222_OK;
        }

        /// <summary>
        /// 지정된 GPIO 포트의 디지털 입력 값(High/Low)을 읽어옵니다.
        /// </summary>
        /// <param name="port">읽어올 GPIO 포트 번호</param>
        /// <param name="value">읽어온 상태값이 저장될 변수</param>
        public bool GpioRead(GPIO_Port port, out bool value)
        {
            ThrowIfDisposed();
            EnsureOpen();

            bool val = false;
            var status = FT4222_GPIO_Read(_handle, port, ref val);
            value = val;

            return status == FT4222_STATUS.FT4222_OK;
        }

        /// <summary>
        /// 특정 GPIO 포트의 입력 트리거 조건을 설정합니다.
        /// </summary>
        public bool GpioSetInputTrigger(GPIO_Port port, GPIO_Trigger trigger)
        {
            ThrowIfDisposed();
            EnsureOpen();

            var status = FT4222_GPIO_SetInputTrigger(_handle, port, trigger);
            return status == FT4222_STATUS.FT4222_OK;
        }

        private void EnsureOpen()
        {
            if (!IsOpen)
                throw new InvalidOperationException("FT4222H device is not open.");
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(FT4222H));
        }

        /// <summary>
        /// 클래스 인스턴스가 삭제될 때 연결된 하드웨어 리소스를 안전하게 닫습니다.
        /// </summary>
        public void Dispose()
        {
            if (_disposed)
                return;

            Close();
            _disposed = true;
        }

        #region Native FTDI Structs and Enums
        // 내부 P/Invoke 및 네이티브 구조체 선언부
        private enum FT_STATUS : uint
        {
            FT_OK = 0,
            FT_INVALID_HANDLE,
            FT_DEVICE_NOT_FOUND,
            FT_DEVICE_NOT_OPENED,
            FT_IO_ERROR,
            FT_INSUFFICIENT_RESOURCES,
            FT_INVALID_PARAMETER,
            FT_INVALID_BAUD_RATE,
            FT_DEVICE_NOT_OPENED_FOR_ERASE,
            FT_DEVICE_NOT_OPENED_FOR_WRITE,
            FT_FAILED_TO_WRITE_DEVICE,
            FT_EEPROM_READ_FAILED,
            FT_EEPROM_WRITE_FAILED,
            FT_EEPROM_ERASE_FAILED,
            FT_EEPROM_NOT_PRESENT,
            FT_EEPROM_NOT_PROGRAMMED,
            FT_INVALID_ARGS,
            FT_NOT_SUPPORTED,
            FT_OTHER_ERROR,
            FT_DEVICE_LIST_NOT_READY
        }

        private enum FT4222_STATUS
        {
            FT4222_OK = 0,
            FT4222_INVALID_HANDLE = 1,
            FT4222_DEVICE_NOT_FOUND = 2,
            FT4222_DEVICE_NOT_OPENED = 3,
            FT4222_IO_ERROR = 4,
            FT4222_INSUFFICIENT_RESOURCES = 5,
            FT4222_INVALID_PARAMETER = 6,
            FT4222_INVALID_BAUD_RATE = 7,
            FT4222_DEVICE_NOT_OPENED_FOR_ERASE = 8,
            FT4222_DEVICE_NOT_OPENED_FOR_WRITE = 9,
            FT4222_FAILED_TO_WRITE_DEVICE = 10,
            FT4222_EEPROM_READ_FAILED = 11,
            FT4222_EEPROM_WRITE_FAILED = 12,
            FT4222_EEPROM_ERASE_FAILED = 13,
            FT4222_EEPROM_NOT_PRESENT = 14,
            FT4222_EEPROM_NOT_PROGRAMMED = 15,
            FT4222_INVALID_ARGS = 16,
            FT4222_NOT_SUPPORTED = 17,
            FT4222_OTHER_ERROR = 18,
            FT4222_DEVICE_LIST_NOT_READY = 19
        }

        private enum SystemClock
        {
            SYS_CLK_60,
            SYS_CLK_24,
            SYS_CLK_48,
            SYS_CLK_80
        }

        private enum SPI_Mode
        {
            SPI_IO_NONE = 0,
            SPI_IO_SINGLE = 1,
            SPI_IO_DUAL = 2,
            SPI_IO_QUAD = 4
        }

        private enum SPI_ClockDivider
        {
            CLK_NONE,
            CLK_DIV_2,
            CLK_DIV_4,
            CLK_DIV_8,
            CLK_DIV_16,
            CLK_DIV_32,
            CLK_DIV_64,
            CLK_DIV_128,
            CLK_DIV_256,
            CLK_DIV_512
        }

        private enum SPI_ClkPolarity
        {
            ACTIVE_LOW,
            ACTIVE_HIGH
        }

        private enum SPI_ClkPhase
        {
            CLK_LEADING,
            CLK_TRAILING
        }

        private enum SPI_DrivingStrength
        {
            DS_4MA,
            DS_8MA,
            DS_12MA,
            DS_16MA
        }

        /// <summary>
        /// FT4222H의 GPIO 핀 번호 열거형입니다.
        /// </summary>
        public enum GPIO_Port
        {
            PORT0 = 0,
            PORT1 = 1,
            PORT2 = 2,
            PORT3 = 3
        }

        /// <summary>
        /// GPIO 핀의 데이터 흐름 방향 설정 열거형입니다.
        /// </summary>
        public enum GPIO_Direction
        {
            OUTPUT = 0,
            INPUT = 1
        }

        /// <summary>
        /// GPIO 입력 상태 변화 감지 조건(트리거) 열거형입니다.
        /// </summary>
        public enum GPIO_Trigger
        {
            RISING = 1,
            FALLING = 2,
            LEVEL_HIGH = 4,
            LEVEL_LOW = 8
        }
        #endregion

        #region DllImports for Native API Calls
        [DllImport("FTD2XX.dll")]
        private static extern FT_STATUS FT_CreateDeviceInfoList(ref uint numDevices);

        [DllImport("FTD2XX.dll")]
        private static extern FT_STATUS FT_Open(uint deviceIndex, out IntPtr ftHandle);

        [DllImport("FTD2XX.dll")]
        private static extern FT_STATUS FT_Close(IntPtr ftHandle);

        [DllImport("LibFT4222-64.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern FT4222_STATUS FT4222_UnInitialize(IntPtr ftHandle);

        [DllImport("LibFT4222-64.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern FT4222_STATUS FT4222_SetClock(IntPtr ftHandle, SystemClock clk);

        [DllImport("LibFT4222-64.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern FT4222_STATUS FT4222_SPI_SetDrivingStrength(IntPtr ftHandle, SPI_DrivingStrength clkStrength, SPI_DrivingStrength ioStrength, SPI_DrivingStrength ssoStrength);

        [DllImport("LibFT4222-64.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern FT4222_STATUS FT4222_SetSuspendOut(IntPtr ftHandle, bool enable);

        [DllImport("LibFT4222-64.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern FT4222_STATUS FT4222_SetWakeUpInterrupt(IntPtr ftHandle, bool enable);

        [DllImport("LibFT4222-64.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern FT4222_STATUS FT4222_SPIMaster_Init(IntPtr ftHandle, SPI_Mode ioLine, SPI_ClockDivider clock, SPI_ClkPolarity cpol, SPI_ClkPhase cpha, byte ssoMap);

        [DllImport("LibFT4222-64.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern FT4222_STATUS FT4222_SPIMaster_SingleRead(IntPtr ftHandle, ref byte buffer, ushort bytesToRead, ref ushort sizeOfRead, bool isEndTransaction);

        [DllImport("LibFT4222-64.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern FT4222_STATUS FT4222_SPIMaster_SingleWrite(IntPtr ftHandle, ref byte buffer, ushort bytesToWrite, ref ushort sizeTransferred, bool isEndTransaction);

        [DllImport("LibFT4222-64.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern FT4222_STATUS FT4222_SPIMaster_SingleReadWrite(IntPtr ftHandle, ref byte readBuffer, ref byte writeBuffer, ushort bufferSize, ref ushort sizeTransferred, bool isEndTransaction);

        [DllImport("LibFT4222-64.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern FT4222_STATUS FT4222_SPI_Reset(IntPtr ftHandle);

        [DllImport("LibFT4222-64.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern FT4222_STATUS FT4222_I2CMaster_Init(IntPtr ftHandle, ushort kbps);

        [DllImport("LibFT4222-64.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern FT4222_STATUS FT4222_I2CMaster_Reset(IntPtr ftHandle);

        [DllImport("LibFT4222-64.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern FT4222_STATUS FT4222_I2CMaster_Read(IntPtr ftHandle, ushort deviceAddress, [Out] byte[] buffer, ushort sizeToTransfer, ref ushort sizeTransferred);

        [DllImport("LibFT4222-64.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern FT4222_STATUS FT4222_I2CMaster_Write(IntPtr ftHandle, ushort deviceAddress, byte[] buffer, ushort sizeToTransfer, ref ushort sizeTransferred);

        [DllImport("LibFT4222-64.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern FT4222_STATUS FT4222_GPIO_Init(IntPtr ftHandle, GPIO_Direction[] GpioDir);

        [DllImport("LibFT4222-64.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern FT4222_STATUS FT4222_GPIO_Read(IntPtr ftHandle, GPIO_Port PortNum, ref bool Value);

        [DllImport("LibFT4222-64.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern FT4222_STATUS FT4222_GPIO_Write(IntPtr ftHandle, GPIO_Port PortNum, bool Value);

        [DllImport("LibFT4222-64.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern FT4222_STATUS FT4222_GPIO_SetInputTrigger(IntPtr ftHandle, GPIO_Port PortNum, GPIO_Trigger Trigger);
        #endregion
    }
}