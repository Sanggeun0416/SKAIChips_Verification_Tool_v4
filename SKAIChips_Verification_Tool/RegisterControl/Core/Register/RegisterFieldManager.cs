using System.Text.RegularExpressions;

namespace SKAIChips_Verification_Tool.RegisterControl
{
    public sealed class RegisterFieldManager
    {
        private readonly Func<IEnumerable<RegisterGroup>> _groupsProvider;

        public RegisterFieldManager(Func<IEnumerable<RegisterGroup>> groupsProvider)
        {
            _groupsProvider = groupsProvider ?? throw new ArgumentNullException(nameof(groupsProvider));
        }

        public IEnumerable<RegisterGroup> Groups => _groupsProvider();

        public RegisterField GetRegisterItem(IRegisterChip chip, string expr)
        {
            if (chip == null)
                throw new ArgumentNullException(nameof(chip));
            if (string.IsNullOrWhiteSpace(expr))
                throw new ArgumentException("expr is null/empty.", nameof(expr));

            var q = Parse(expr);

            if (TryFindByExactName(q.Original, out var reg1, out var item1))
            {
                var (u, l) = q.HasRange ? ValidateOrUseRange(item1.UpperBit, item1.LowerBit, q.Msb, q.Lsb, q.Original) : (item1.UpperBit, item1.LowerBit);
                return new RegisterField(chip, reg1.Address, q.HasRange ? $"{q.BaseName}[{u}:{l}]" : item1.Name, u, l);
            }

            var candidates = FindCandidatesByBaseName(q.BaseName);
            if (candidates.Count > 0)
            {
                (RegisterDetail reg, RegisterItem item, int u, int l)? picked = null;

                foreach (var (reg, item) in candidates)
                {
                    if (!q.HasRange)
                    {
                        if (picked != null)
                            throw new InvalidOperationException($"Ambiguous field name: '{q.BaseName}'. Add bit range like '{q.BaseName}[msb:lsb]'.");
                        picked = (reg, item, item.UpperBit, item.LowerBit);
                        continue;
                    }

                    if (RangeFits(item.UpperBit, item.LowerBit, q.Msb, q.Lsb))
                    {
                        var (u, l) = NormalizeRange(q.Msb, q.Lsb);

                        if (picked == null)
                            picked = (reg, item, u, l);
                        else
                        {
                            if (picked.Value.u == u && picked.Value.l == l)
                                throw new InvalidOperationException($"Ambiguous field: '{q.Original}' matches multiple items.");
                        }
                    }
                }

                if (picked == null)
                    throw new KeyNotFoundException($"Field not found with requested range: '{q.Original}'.");

                return new RegisterField(chip, picked.Value.reg.Address, q.Original, picked.Value.u, picked.Value.l);
            }

            if (TryFindRegisterByName(q.BaseName, out var regOnly))
            {
                return new RegisterField(chip, regOnly.Address, q.BaseName, 31, 0);
            }

            throw new KeyNotFoundException($"Field not found: '{q.Original}'.");
        }

        private bool TryFindByExactName(string name, out RegisterDetail reg, out RegisterItem item)
        {
            foreach (var g in _groupsProvider())
            {
                foreach (var r in g.Registers)
                {
                    foreach (var it in r.Items)
                    {
                        if (string.Equals(it.Name, name, StringComparison.OrdinalIgnoreCase))
                        {
                            reg = r;
                            item = it;
                            return true;
                        }
                    }
                }
            }

            reg = null!;
            item = null!;
            return false;
        }

        private List<(RegisterDetail reg, RegisterItem item)> FindCandidatesByBaseName(string baseName)
        {
            var list = new List<(RegisterDetail, RegisterItem)>();

            foreach (var g in _groupsProvider())
            {
                foreach (var r in g.Registers)
                {
                    foreach (var it in r.Items)
                    {
                        var itBase = StripRange(it.Name);
                        if (string.Equals(itBase, baseName, StringComparison.OrdinalIgnoreCase))
                            list.Add((r, it));
                    }
                }
            }

            return list;
        }

        private bool TryFindRegisterByName(string name, out RegisterDetail reg)
        {
            foreach (var g in _groupsProvider())
            {
                foreach (var r in g.Registers)
                {
                    if (string.Equals(r.Name, name, StringComparison.OrdinalIgnoreCase))
                    {
                        reg = r;
                        return true;
                    }
                }
            }

            reg = null!;
            return false;
        }

        private static (int u, int l) ValidateOrUseRange(int itemU, int itemL, int reqU, int reqL, string original)
        {
            if (!RangeFits(itemU, itemL, reqU, reqL))
                throw new InvalidOperationException($"Range '{original}' does not fit item bits ({itemU}:{itemL}).");

            return NormalizeRange(reqU, reqL);
        }

        private static bool RangeFits(int itemU, int itemL, int reqU, int reqL)
        {
            var (iu, il) = NormalizeRange(itemU, itemL);
            var (ru, rl) = NormalizeRange(reqU, reqL);
            return ru <= iu && rl >= il;
        }

        private static (int u, int l) NormalizeRange(int a, int b) => a >= b ? (a, b) : (b, a);

        private static string StripRange(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return name;
            var idx = name.IndexOf('[');
            return idx >= 0 ? name[..idx].Trim() : name.Trim();
        }

        private static ParsedExpr Parse(string expr)
        {
            var s = expr.Trim();

            var m = Regex.Match(s, @"^(?<n>.+?)\s*\[\s*(?<msb>\d+)\s*(?::\s*(?<lsb>\d+)\s*)?\]\s*$");
            if (!m.Success)
                return new ParsedExpr(s, StripRange(s), false, 0, 0);

            var name = m.Groups["n"].Value.Trim();
            var msb = int.Parse(m.Groups["msb"].Value);
            var lsb = m.Groups["lsb"].Success ? int.Parse(m.Groups["lsb"].Value) : msb;

            return new ParsedExpr(s, name, true, msb, lsb);
        }

        private readonly record struct ParsedExpr(string Original, string BaseName, bool HasRange, int Msb, int Lsb);

        public sealed class RegisterField
        {
            private readonly IRegisterChip _chip;
            private readonly uint _address;

            public string Name
            {
                get;
            }
            public int UpperBit
            {
                get;
            }
            public int LowerBit
            {
                get;
            }
            public uint Value
            {
                get; set;
            }
            public uint LastRegisterValue
            {
                get; private set;
            }

            public RegisterField(IRegisterChip chip, uint address, string name, int upperBit, int lowerBit)
            {
                _chip = chip;
                _address = address;
                Name = name;

                var (u, l) = upperBit >= lowerBit ? (upperBit, lowerBit) : (lowerBit, upperBit);
                if (u > 31 || l < 0)
                    throw new ArgumentOutOfRangeException(nameof(upperBit));
                UpperBit = u;
                LowerBit = l;
            }

            public uint Read()
            {
                var reg = _chip.ReadRegister(_address);
                LastRegisterValue = reg;

                Value = Extract(reg);
                return Value;
            }

            public void Write()
            {
                if (UpperBit == 31 && LowerBit == 0)
                {
                    _chip.WriteRegister(_address, Value);
                    return;
                }

                var reg = _chip.ReadRegister(_address);
                LastRegisterValue = reg;

                var newReg = Insert(reg, Value);
                _chip.WriteRegister(_address, newReg);
                LastRegisterValue = newReg;
            }

            public void Write(uint value)
            {
                Value = value;
                Write();
            }

            private uint Extract(uint reg)
            {
                var width = UpperBit - LowerBit + 1;
                var mask = width >= 32 ? 0xFFFFFFFFu : ((1u << width) - 1u);
                return (reg >> LowerBit) & mask;
            }

            private uint Insert(uint reg, uint fieldValue)
            {
                var width = UpperBit - LowerBit + 1;
                var mask = width >= 32 ? 0xFFFFFFFFu : ((1u << width) - 1u);
                var v = fieldValue & mask;

                reg &= ~(mask << LowerBit);
                reg |= v << LowerBit;
                return reg;
            }
        }
    }
}
