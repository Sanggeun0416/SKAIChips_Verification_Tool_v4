using System.Text.Json;

namespace SKAIChips_Verification_Tool.Instrument
{
    public sealed class InstrumentRegistry
    {
        public static InstrumentRegistry Instance { get; } = new InstrumentRegistry();

        readonly object _sync = new object();
        readonly List<InstrumentInfo> _infos = new();
        readonly Dictionary<string, IScpiClient> _clients =
            new(StringComparer.OrdinalIgnoreCase);

        InstrumentRegistry()
        {
        }

        public void Update(IEnumerable<InstrumentInfo> instruments)
        {
            if (instruments == null)
                throw new ArgumentNullException(nameof(instruments));

            lock (_sync)
            {
                _infos.Clear();
                _infos.AddRange(instruments.Select(Clone));

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

        static InstrumentInfo Clone(InstrumentInfo src) => new()
        {
            Type = src.Type,
            Enabled = src.Enabled,
            VisaAddress = src.VisaAddress,
            Name = src.Name
        };

        public IScpiClient GetByType(string type)
        {
            if (string.IsNullOrWhiteSpace(type))
                throw new ArgumentNullException(nameof(type));

            type = type.Trim();

            lock (_sync)
            {
                if (_clients.TryGetValue(type, out var cached))
                    return cached;

                var info = _infos
                    .Where(x => x.Enabled)
                    .FirstOrDefault(x => string.Equals(x.Type, type, StringComparison.OrdinalIgnoreCase));

                if (info == null)
                    throw new InvalidOperationException($"Instrument(Type='{type}') not found or not enabled.");

                if (string.IsNullOrWhiteSpace(info.VisaAddress))
                    throw new InvalidOperationException($"Instrument(Type='{type}') has empty VISA address.");

                var client = new VisaScpiClient(info.VisaAddress);
                if (!client.Open())
                {
                    client.Dispose();
                    throw new InvalidOperationException($"Failed to open instrument(Type='{type}', Addr='{info.VisaAddress}').");
                }

                _clients[type] = client;
                return client;
            }
        }

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
