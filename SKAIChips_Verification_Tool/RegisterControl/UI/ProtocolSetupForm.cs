using System.Globalization;

namespace SKAIChips_Verification_Tool.RegisterControl
{
    public partial class ProtocolSetupForm : Form
    {
        private readonly IChipProject _project;
        private bool _suppressProtocolChanged;

        public ProtocolSettings? Result
        {
            get; private set;
        }

        public ProtocolSetupForm(IChipProject project, ProtocolSettings current)
        {
            _project = project ?? throw new ArgumentNullException(nameof(project));

            InitializeComponent();
            InitProtocolCombo();
            InitSpiModeCombo();

            _suppressProtocolChanged = true;
            try
            {
                if (current != null)
                    ApplyCurrent(current);
                else
                    ApplyDefault();
            }
            finally
            {
                _suppressProtocolChanged = false;
            }
        }

        private void InitProtocolCombo()
        {
            comboProtocol.Items.Clear();

            foreach (var p in _project.SupportedProtocols)
                comboProtocol.Items.Add(p);

            if (comboProtocol.Items.Count > 0)
                comboProtocol.SelectedIndex = 0;
        }

        private void InitSpiModeCombo()
        {
            comboSpiMode.Items.Clear();

            comboSpiMode.Items.Add(new SpiModeItem(0, 0, 0));
            comboSpiMode.Items.Add(new SpiModeItem(1, 0, 1));
            comboSpiMode.Items.Add(new SpiModeItem(2, 1, 0));
            comboSpiMode.Items.Add(new SpiModeItem(3, 1, 1));

            comboSpiMode.SelectedIndex = 0;
        }

        private void ApplyDefault()
        {
            ApplyDefaultSpeedForSelectedProtocol();
            txtSlaveAddr.Text = $"0x{_project.DeviceAddress:X2}";
            comboSpiMode.SelectedIndex = 0;
            UpdateControlsEnabled();
        }

        private void ApplyDefaultSpeedForSelectedProtocol()
        {
            if (comboProtocol.SelectedItem is not ProtocolRegLogType pt)
                return;

            SetNumericWithinRange(numSpeed, _project.ComFrequency);
        }

        private void ApplyCurrent(ProtocolSettings current)
        {

            for (var i = 0; i < comboProtocol.Items.Count; i++)
            {
                if (comboProtocol.Items[i] is ProtocolRegLogType pt && pt == current.ProtocolRegLogType)
                {
                    comboProtocol.SelectedIndex = i;
                    break;
                }
            }

            if (current.ProtocolRegLogType == ProtocolRegLogType.I2C)
            {
                if (current.SpeedKbps > 0 && current.SpeedKbps >= (int)numSpeed.Minimum && current.SpeedKbps <= (int)numSpeed.Maximum)
                    numSpeed.Value = current.SpeedKbps;

                txtSlaveAddr.Text = $"0x{current.I2cSlaveAddress:X2}";
            }

            else if (current.ProtocolRegLogType == ProtocolRegLogType.SPI)
            {
                if (current.SpiClockKHz > 0 && current.SpiClockKHz >= (int)numSpeed.Minimum && current.SpiClockKHz <= (int)numSpeed.Maximum)
                    numSpeed.Value = current.SpiClockKHz;

                for (int i = 0; i < comboSpiMode.Items.Count; i++)
                {
                    if (comboSpiMode.Items[i] is SpiModeItem it && it.Value == current.SpiMode)
                    {
                        comboSpiMode.SelectedIndex = i;
                        break;
                    }
                }
            }

            UpdateControlsEnabled();
        }

        private void comboProtocol_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_suppressProtocolChanged)
                return;

            ApplyDefaultSpeedForSelectedProtocol();
            UpdateControlsEnabled();
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            if (comboProtocol.SelectedItem is not ProtocolRegLogType protocol)
            {
                MessageBox.Show("Protocol을 선택하세요.");
                return;
            }

            var settings = new ProtocolSettings
            {
                ProtocolRegLogType = protocol
            };

            if (protocol == ProtocolRegLogType.I2C)
            {
                settings.SpeedKbps = (int)numSpeed.Value;

                if (!TryParseHexByte(txtSlaveAddr.Text, out var slave))
                {
                    MessageBox.Show("I2C Slave Address 형식이 잘못되었습니다. 예: 0x52");
                    return;
                }

                settings.I2cSlaveAddress = slave;
            }
            else if (protocol == ProtocolRegLogType.SPI)
            {
                settings.SpiClockKHz = (int)numSpeed.Value;

                if (comboSpiMode.SelectedItem is SpiModeItem it)
                    settings.SpiMode = it.Value;
                else
                {
                    MessageBox.Show("SPI Mode를 선택하세요.");
                    return;
                }

                settings.SpiLsbFirst = false;
            }

            Result = settings;
            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void UpdateControlsEnabled()
        {
            var selected = comboProtocol.SelectedItem;
            var isI2c = selected is ProtocolRegLogType pt1 && pt1 == ProtocolRegLogType.I2C;
            var isSpi = selected is ProtocolRegLogType pt2 && pt2 == ProtocolRegLogType.SPI;

            lblSpeed.Visible = isI2c || isSpi;
            numSpeed.Visible = isI2c || isSpi;

            lblSlaveAddr.Visible = isI2c;
            txtSlaveAddr.Visible = isI2c;

            lblSpiMode.Visible = isSpi;
            comboSpiMode.Visible = isSpi;
        }

        private static bool TryParseHexByte(string text, out byte value)
        {
            value = 0;

            if (string.IsNullOrWhiteSpace(text))
                return false;

            text = text.Trim();

            if (text.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                text = text[2..];

            return byte.TryParse(text, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out value);
        }

        private static void SetNumericWithinRange(NumericUpDown n, decimal value)
        {
            if (value < n.Minimum)
                value = n.Minimum;
            if (value > n.Maximum)
                value = n.Maximum;
            n.Value = value;
        }

        private sealed class SpiModeItem
        {
            public int Value
            {
                get;
            }
            public string Text
            {
                get;
            }

            public SpiModeItem(int value, int cpol, int cpha)
            {
                Value = value;
                Text = $"{value} (CPOL={cpol}, CPHA={cpha})";
            }

            public override string ToString() => Text;
        }
    }
}
