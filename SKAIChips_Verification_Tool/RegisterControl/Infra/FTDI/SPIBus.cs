namespace SKAIChips_Verification_Tool.RegisterControl
{
    public sealed class SpiBus : ISpiBus, IChicagoSpiBus, IGpioController
    {
        private readonly uint _deviceIndex;
        private readonly ProtocolSettings _settings;

        private FT4222H? _ft4222;
        private UM232H? _um232h;

        private readonly FT4222H.GPIO_Direction[] _ft4222GpioDirs =
            Enumerable.Repeat(FT4222H.GPIO_Direction.INPUT, 4).ToArray();

        public bool IsConnected
        {
            get; private set;
        }

        public SpiBus(uint deviceIndex, ProtocolSettings settings)
        {
            _deviceIndex = deviceIndex;
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

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
                // GPIO 0~3
                if (pinIndex < 0 || pinIndex > 3)
                    throw new ArgumentOutOfRangeException(nameof(pinIndex), "FT4222 GPIO Pin must be 0-3");

                _ft4222GpioDirs[pinIndex] = isOutput ? FT4222H.GPIO_Direction.OUTPUT : FT4222H.GPIO_Direction.INPUT;
                _ft4222!.GpioInit(_ft4222GpioDirs);
            }
        }

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

        public void Dispose() => Disconnect();

        private void EnsureConnected()
        {
            if (!IsConnected)
                throw new InvalidOperationException("SPI bus is not connected.");
        }

        public void ChicagoWrite(ReadOnlySpan<byte> tx)
        {
            EnsureConnected();

            if (_settings.DeviceKind != DeviceKind.UM232H)
                throw new NotSupportedException("Chicago requires UM232H.");

            _um232h!.SpiWrite(tx.ToArray(), endTransaction: true);
            _um232h.SetGpioLHighForChicago();
        }

        public void ChicagoWriteRead(ReadOnlySpan<byte> tx, Span<byte> rx)
        {
            EnsureConnected();

            if (_settings.DeviceKind != DeviceKind.UM232H)
                throw new NotSupportedException("Chicago requires UM232H.");

            var r = _um232h!.SpiWriteAndReadForChicago(tx.ToArray(), rx.Length);
            r.AsSpan(0, Math.Min(r.Length, rx.Length)).CopyTo(rx);
        }

        public void ChicagoForceIdleHigh()
        {
            EnsureConnected();

            if (_settings.DeviceKind != DeviceKind.UM232H)
                return;

            _um232h!.SetGpioLHighForChicago();
        }
    }
}
