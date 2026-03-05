using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace SKAIChips_Verification_Tool.RegisterControl
{
    /// <summary>
    /// 사용자가 입력한 문자열 수식(예: "CTRL_REG[3:0]")을 파싱하여, 
    /// 칩의 맵(RegisterMap)에서 해당 레지스터와 비트 필드를 찾아 제어 객체를 생성해주는 매니저 클래스입니다.
    /// </summary>
    public sealed class RegisterFieldManager
    {
        private readonly Func<IEnumerable<RegisterGroup>> _groupsProvider;

        /// <summary>
        /// RegisterFieldManager 클래스의 새 인스턴스를 초기화합니다.
        /// </summary>
        /// <param name="groupsProvider">현재 로드된 전체 레지스터 그룹(맵)을 제공하는 델리게이트 함수</param>
        public RegisterFieldManager(Func<IEnumerable<RegisterGroup>> groupsProvider)
        {
            _groupsProvider = groupsProvider ?? throw new ArgumentNullException(nameof(groupsProvider));
        }

        /// <summary>
        /// 현재 로드되어 있는 모든 레지스터 그룹의 목록을 가져옵니다.
        /// </summary>
        public IEnumerable<RegisterGroup> Groups => _groupsProvider();

        /// <summary>
        /// 사용자가 입력한 문자열 수식을 분석하여 실제 칩의 레지스터를 제어할 수 있는 필드 객체를 반환합니다.
        /// </summary>
        /// <param name="chip">제어 대상이 되는 하드웨어 칩 인터페이스</param>
        /// <param name="expr">검색할 필드 수식 (예: "ENABLE_TX", "SYS_CTRL[7:4]", "STATUS[0]")</param>
        /// <returns>해당 비트 영역을 읽고 쓸 수 있는 RegisterField 객체</returns>
        /// <exception cref="KeyNotFoundException">수식과 일치하는 레지스터나 필드를 찾지 못했을 때 발생합니다.</exception>
        /// <exception cref="InvalidOperationException">동일한 이름이 여러 개 존재하여 대상을 특정할 수 없을 때 발생합니다.</exception>
        public RegisterField GetRegisterItem(IRegisterChip chip, string expr)
        {
            if (chip == null)
                throw new ArgumentNullException(nameof(chip));
            if (string.IsNullOrWhiteSpace(expr))
                throw new ArgumentException("expr is null/empty.", nameof(expr));

            // 1. 수식 문자열(expr)을 이름과 비트 범위(Range)로 파싱합니다.
            var q = Parse(expr);

            // 2. 정확히 일치하는 비트 필드(Item) 이름이 있는지 검색합니다.
            if (TryFindByExactName(q.Original, out var reg1, out var item1))
            {
                var (u, l) = q.HasRange ? ValidateOrUseRange(item1.UpperBit, item1.LowerBit, q.Msb, q.Lsb, q.Original) : (item1.UpperBit, item1.LowerBit);
                return new RegisterField(chip, reg1.Address, q.HasRange ? $"{q.BaseName}[{u}:{l}]" : item1.Name, u, l);
            }

            // 3. 괄호([ ])를 제거한 기본 이름(BaseName)으로 후보군을 검색합니다.
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

            // 4. 비트 필드(Item)가 아닌 전체 레지스터(RegisterDetail)의 이름과 일치하는지 검색합니다.
            if (TryFindRegisterByName(q.BaseName, out var regOnly))
            {
                // 레지스터 전체를 지정한 경우 기본값으로 32비트(31~0) 전체 영역을 반환합니다.
                return new RegisterField(chip, regOnly.Address, q.BaseName, 31, 0);
            }

            throw new KeyNotFoundException($"Field not found: '{q.Original}'.");
        }

        /// <summary>
        /// 그룹과 레지스터를 순회하며 입력된 이름과 정확히 일치하는 비트 필드(Item)를 찾습니다.
        /// </summary>
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

        /// <summary>
        /// 범위 수식([x:y])을 떼어낸 기본 이름과 일치하는 모든 비트 필드 후보군을 찾습니다.
        /// </summary>
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

        /// <summary>
        /// 입력된 이름과 일치하는 레지스터(RegisterDetail) 자체를 찾습니다.
        /// </summary>
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

        /// <summary>
        /// 사용자가 요청한 비트 범위가 맵에 정의된 아이템의 비트 범위 내에 안전하게 속하는지 검증합니다.
        /// </summary>
        private static (int u, int l) ValidateOrUseRange(int itemU, int itemL, int reqU, int reqL, string original)
        {
            if (!RangeFits(itemU, itemL, reqU, reqL))
                throw new InvalidOperationException($"Range '{original}' does not fit item bits ({itemU}:{itemL}).");

            return NormalizeRange(reqU, reqL);
        }

        /// <summary>
        /// 요청된 범위(reqU~reqL)가 타겟 범위(itemU~itemL) 안에 포함되는지 확인합니다.
        /// </summary>
        private static bool RangeFits(int itemU, int itemL, int reqU, int reqL)
        {
            var (iu, il) = NormalizeRange(itemU, itemL);
            var (ru, rl) = NormalizeRange(reqU, reqL);
            return ru <= iu && rl >= il;
        }

        /// <summary>
        /// 두 비트 인덱스 중 큰 값을 MSB(Upper), 작은 값을 LSB(Lower)로 정렬하여 반환합니다.
        /// </summary>
        private static (int u, int l) NormalizeRange(int a, int b) => a >= b ? (a, b) : (b, a);

        /// <summary>
        /// 문자열에서 대괄호('['...']')로 표현된 비트 범위를 잘라내고 순수 이름만 반환합니다.
        /// </summary>
        private static string StripRange(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return name;
            var idx = name.IndexOf('[');
            return idx >= 0 ? name[..idx].Trim() : name.Trim();
        }

        /// <summary>
        /// 정규표현식을 사용하여 사용자가 입력한 문자열 수식에서 이름과 비트 범위(MSB, LSB)를 추출합니다.
        /// </summary>
        private static ParsedExpr Parse(string expr)
        {
            var s = expr.Trim();

            // 예: "CTRL_REG[7:4]", "STATUS[0]" 매칭 정규식
            var m = Regex.Match(s, @"^(?<n>.+?)\s*\[\s*(?<msb>\d+)\s*(?::\s*(?<lsb>\d+)\s*)?\]\s*$");
            if (!m.Success)
                return new ParsedExpr(s, StripRange(s), false, 0, 0);

            var name = m.Groups["n"].Value.Trim();
            var msb = int.Parse(m.Groups["msb"].Value);
            var lsb = m.Groups["lsb"].Success ? int.Parse(m.Groups["lsb"].Value) : msb;

            return new ParsedExpr(s, name, true, msb, lsb);
        }

        /// <summary>
        /// 파싱된 수식의 결과(기본 이름, 범위 포함 여부, 최상위/최하위 비트)를 담는 레코드 구조체입니다.
        /// </summary>
        private readonly record struct ParsedExpr(string Original, string BaseName, bool HasRange, int Msb, int Lsb);

        /// <summary>
        /// 특정 칩의 특정 레지스터 주소 내에서 지정된 비트 범위(MSB~LSB)만을 읽고 쓰기 위한 제어 객체입니다.
        /// </summary>
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

            /// <summary>
            /// 현재 메모리에서 가지고 있거나 쓸 예정인 비트 필드의 값입니다.
            /// </summary>
            public uint Value
            {
                get; set;
            }

            /// <summary>
            /// 가장 최근에 읽거나 쓴 전체 레지스터(32비트 등)의 원본 값입니다.
            /// </summary>
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

            /// <summary>
            /// 칩에서 레지스터 전체 값을 읽어온 뒤, 할당된 비트 범위만 추출(마스킹 및 시프트)하여 반환합니다.
            /// </summary>
            public uint Read()
            {
                var reg = _chip.ReadRegister(_address);
                LastRegisterValue = reg;

                Value = Extract(reg);
                return Value;
            }

            /// <summary>
            /// 현재 저장된 Value 값을 칩의 레지스터에 기록합니다.
            /// 전체 레지스터가 아닌 일부 비트만 수정해야 할 경우 Read-Modify-Write 동작을 수행합니다.
            /// </summary>
            public void Write()
            {
                // 32비트 전체를 덮어쓰는 경우 굳이 미리 읽어올 필요가 없음
                if (UpperBit == 31 && LowerBit == 0)
                {
                    _chip.WriteRegister(_address, Value);
                    return;
                }

                // Read-Modify-Write: 기존 값을 읽어와서 원하는 비트만 수정한 뒤 다시 씀
                var reg = _chip.ReadRegister(_address);
                LastRegisterValue = reg;

                var newReg = Insert(reg, Value);
                _chip.WriteRegister(_address, newReg);
                LastRegisterValue = newReg;
            }

            /// <summary>
            /// 새로운 값을 할당하고 즉시 칩에 기록합니다.
            /// </summary>
            public void Write(uint value)
            {
                Value = value;
                Write();
            }

            /// <summary>
            /// 32비트 레지스터 값에서 현재 필드가 차지하는 비트 영역만 추출하여 우측으로 시프트(Shift)합니다.
            /// </summary>
            private uint Extract(uint reg)
            {
                var width = UpperBit - LowerBit + 1;
                var mask = width >= 32 ? 0xFFFFFFFFu : ((1u << width) - 1u);
                return (reg >> LowerBit) & mask;
            }

            /// <summary>
            /// 32비트 레지스터 원본 값 중 현재 필드가 차지하는 비트 영역만 지정된 새 값(fieldValue)으로 교체합니다.
            /// </summary>
            private uint Insert(uint reg, uint fieldValue)
            {
                var width = UpperBit - LowerBit + 1;
                var mask = width >= 32 ? 0xFFFFFFFFu : ((1u << width) - 1u);
                var v = fieldValue & mask;

                reg &= ~(mask << LowerBit); // 기존 비트 영역 초기화(0)
                reg |= v << LowerBit;       // 새로운 값 삽입
                return reg;
            }
        }
    }
}