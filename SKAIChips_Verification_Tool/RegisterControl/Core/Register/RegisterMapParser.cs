using System;

namespace SKAIChips_Verification_Tool.RegisterControl
{
    /// <summary>
    /// 엑셀(Excel) 문서에서 읽어온 2차원 문자열 배열 데이터를 분석하여 
    /// 내부 레지스터 맵 자료구조(RegisterGroup, RegisterDetail, RegisterItem)로 파싱(Parsing)하는 유틸리티 클래스입니다.
    /// </summary>
    public static class RegisterMapParser
    {
        // 파싱 시 기준점이 되는 엑셀 헤더 키워드들
        private const string HeaderBit = "Bit";
        private const string HeaderName = "Name";
        private const string HeaderDefault = "Default";

        // 16진수 문자열 처리를 위한 접두사
        private const string HexPrefix = "0x";

        // 파싱에서 무시할(Don't care) 특수 값들
        private const string IgnoreValueX = "X";
        private const string IgnoreValueDash = "-";

        /// <summary>
        /// 주어진 2차원 문자열 배열(엑셀 데이터)을 분석하여 하나의 완성된 레지스터 그룹(RegisterGroup) 객체를 생성합니다.
        /// </summary>
        /// <param name="groupName">생성할 레지스터 그룹의 이름 (보통 엑셀 시트 이름 사용)</param>
        /// <param name="regData">엑셀에서 읽어온 2차원 문자열 배열 데이터 (행, 열)</param>
        /// <returns>파싱이 완료된 RegisterGroup 인스턴스</returns>
        public static RegisterGroup MakeRegisterGroup(string groupName, string[,] regData)
        {
            var rg = new RegisterGroup(groupName);

            if (regData == null)
                return rg;

            var rowCount = regData.GetLength(0);
            var colCount = regData.GetLength(1);

            // 엑셀의 왼쪽 끝(열 0~2)을 탐색하며 "Bit", "Name", "Default" 헤더 블록을 찾습니다.
            for (var xStart = 0; xStart < 3 && xStart < colCount; xStart++)
            {
                for (var row = 0; row < rowCount; row++)
                {
                    // 헤더 3줄(Bit, Name, Default)이 들어갈 공간이 없으면 스킵
                    if (row < 1 || row + 2 >= rowCount)
                        continue;

                    // 헤더 패턴 일치 여부 확인
                    if (regData[row, xStart] != HeaderBit ||
                        regData[row + 1, xStart] != HeaderName ||
                        regData[row + 2, xStart] != HeaderDefault)
                        continue;

                    // 헤더 위쪽 행에서 레지스터의 주소(Address) 추출 시도
                    string strAddr = null;
                    if (xStart + 1 < colCount)
                        strAddr = regData[row - 1, xStart + 1];

                    // "0x" 접두사가 있으면 제거
                    if (!string.IsNullOrWhiteSpace(strAddr) &&
                        strAddr.StartsWith(HexPrefix, StringComparison.OrdinalIgnoreCase))
                    {
                        strAddr = strAddr.Substring(HexPrefix.Length);
                    }

                    // 16진수 주소 문자열을 정수(uint)로 변환
                    if (!uint.TryParse(
                            strAddr,
                            System.Globalization.NumberStyles.HexNumber,
                            null,
                            out var address))
                    {
                        continue; // 주소 변환 실패 시 해당 블록 스킵
                    }

                    // 레지스터 이름 추출 및 레지스터 객체 생성
                    var regName = xStart + 2 < colCount ? regData[row - 1, xStart + 2] : null;
                    var reg = rg.AddRegister(regName, address);

                    uint resetValue = 0;
                    int maxBit = 0;

                    // 가로 방향으로 이동하며 개별 비트 필드(RegisterItem) 파싱
                    for (var column = xStart + 1; column < colCount; column++)
                    {
                        var defText = regData[row + 2, column]; // 기본값
                        var nameText = regData[row + 1, column]; // 비트 필드 이름

                        // 유효하지 않은 값("X", "-")이거나 비어있으면 스킵
                        if (defText == IgnoreValueX ||
                            defText == IgnoreValueDash ||
                            defText == null ||
                            nameText == null)
                        {
                            continue;
                        }

                        var itemName = nameText;
                        string itemDesc = null;

                        // 비트 위치(MSB) 추출
                        if (!int.TryParse(regData[row, column], out var upperBit))
                            continue;

                        // 레지스터 전체의 최대 비트 폭을 계산하기 위해 갱신
                        if (upperBit > maxBit)
                            maxBit = upperBit;

                        var lowerBit = upperBit;
                        uint itemValue = defText == "0" ? 0u : 1u;

                        // 셀 병합(Merge) 등으로 인해 이름이 비어있는 우측 셀들을 탐색하여 LSB 및 멀티 비트 기본값 계산
                        for (var x = column + 1; x < colCount; x++)
                        {
                            // 다음 비트 필드의 이름이 시작되면 탐색 중단
                            if (regData[row + 1, x] != null)
                                break;

                            var bitText = regData[row, x];
                            if (bitText == null)
                                continue;

                            // LSB 갱신
                            if (!int.TryParse(bitText, out var bit))
                                continue;

                            lowerBit = bit;

                            // 비트를 하나씩 시프트(Shift)하며 전체 기본값(itemValue) 조합
                            var bitDef = regData[row + 2, x];
                            itemValue = (itemValue << 1) | (bitDef == "0" ? 0u : 1u);
                        }

                        // 아래쪽 행들을 탐색하여 해당 비트 필드의 상세 설명(Description) 추출
                        for (var y = row; y < rowCount; y++)
                        {
                            if (regData[y, xStart] != itemName)
                                continue;

                            if (xStart + 1 < colCount)
                                itemDesc = regData[y, xStart + 1];

                            // 여러 줄에 걸친 상세 설명을 합침 (예: "0=Disable", "1=Enable")
                            for (var descRow = y + 1; descRow < rowCount; descRow++)
                            {
                                // 다른 항목의 설명이 시작되면 중단
                                if (regData[descRow, xStart] != null)
                                    break;

                                var col3 = xStart + 3 < colCount ? regData[descRow, xStart + 3] : null;
                                var col4 = xStart + 4 < colCount ? regData[descRow, xStart + 4] : null;

                                var lineDesc = string.Empty;

                                if (col3 != null && col4 != null)
                                    lineDesc = "\n" + col3 + "=" + col4;
                                else if (col4 != null)
                                    lineDesc = "\n" + col4;
                                else if (col3 != null)
                                    lineDesc = "\n" + col3 + "=";

                                if (!string.IsNullOrEmpty(lineDesc))
                                    itemDesc += lineDesc;
                            }

                            break;
                        }

                        // 레지스터에 비트 필드(Item) 추가
                        reg.AddItem(itemName, upperBit, lowerBit, itemValue, itemDesc);

                        // 각 비트 필드의 기본값들을 조합하여 레지스터 전체의 32비트 초기화(Reset) 값을 완성
                        var width = upperBit - lowerBit + 1;
                        if (width <= 0)
                            continue;

                        var mask = width >= 32 ? 0xFFFFFFFFu : ((1u << width) - 1u);
                        var val = itemValue & mask;

                        resetValue &= ~(mask << lowerBit); // 해당 영역 0으로 초기화
                        resetValue |= val << lowerBit;     // 값 삽입
                    }

                    // 파싱된 최대 비트 인덱스(maxBit)를 기준으로 레지스터의 전체 비트 폭(BitWidth) 설정
                    if (maxBit <= 7)
                        reg.BitWidth = 8;
                    else if (maxBit <= 15)
                        reg.BitWidth = 16;
                    else if (maxBit <= 23)
                        reg.BitWidth = 24;
                    else
                        reg.BitWidth = 32;

                    // 완성된 레지스터 초기값 할당
                    reg.ResetValue = resetValue;
                }
            }

            return rg;
        }
    }
}