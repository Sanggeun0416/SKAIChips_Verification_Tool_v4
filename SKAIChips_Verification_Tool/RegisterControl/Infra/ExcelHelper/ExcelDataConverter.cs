using System;
using System.Globalization;

namespace SKAIChips_Verification_Tool.RegisterControl
{
    /// <summary>
    /// Excel의 COM 객체(Range.Value2) 데이터를 C#에서 다루기 쉬운 2차원 문자열 배열(string[,])로 변환하는 내부 유틸리티 클래스입니다.
    /// Excel Interop 특유의 1-based 인덱싱 및 타입 불일치 문제를 해결합니다.
    /// </summary>
    internal class ExcelDataConverter
    {
        /// <summary>
        /// Excel의 단일 셀 값 또는 다중 셀 범위(2D 배열) 객체를 0-based 2차원 문자열 배열로 변환합니다.
        /// </summary>
        /// <param name="value2">Excel에서 읽어온 원시 데이터 (Range.Value2)</param>
        /// <param name="trim">변환된 문자열의 앞뒤 공백을 제거할지 여부입니다. (기본값: true)</param>
        /// <param name="emptyToNull">빈 문자열("")을 null로 치환할지 여부입니다. (기본값: true)</param>
        /// <returns>정규화된 0-based 2차원 문자열 배열</returns>
        public static string[,] ToStringArrayFromValue2(object? value2, bool trim = true, bool emptyToNull = true)
        {
            // 데이터가 아예 없는 경우 빈 배열 반환
            if (value2 == null)
                return new string[0, 0];

            // 데이터가 다중 셀(범위) 형태인 경우 배열 변환기 호출
            if (value2 is object[,] matrix)
                return FromObjectMatrix(matrix, trim, emptyToNull);

            // 데이터가 단일 셀 값인 경우 1x1 배열로 감싸서 반환
            var single = new string[1, 1];
            single[0, 0] = ConvertCell(value2, trim, emptyToNull);
            return single;
        }

        /// <summary>
        /// Excel Interop 특유의 1-based 2차원 객체 배열을 C# 표준인 0-based 2차원 문자열 배열로 변환합니다.
        /// </summary>
        /// <param name="values">Excel에서 읽어온 1-based 2차원 객체 배열</param>
        /// <param name="trim">문자열 앞뒤 공백 제거 여부</param>
        /// <param name="emptyToNull">빈 문자열의 null 치환 여부</param>
        /// <returns>0부터 인덱싱이 시작되는 2차원 문자열 배열</returns>
        public static string[,] FromObjectMatrix(object[,] values, bool trim = true, bool emptyToNull = true)
        {
            // Excel 배열은 대체로 인덱스가 1부터 시작하므로 GetLowerBound/GetUpperBound를 통해 정확한 범위를 구합니다.
            int r0 = values.GetLowerBound(0);
            int r1 = values.GetUpperBound(0);
            int c0 = values.GetLowerBound(1);
            int c1 = values.GetUpperBound(1);

            int rows = r1 - r0 + 1;
            int cols = c1 - c0 + 1;

            var result = new string[rows, cols];

            for (int r = r0; r <= r1; r++)
            {
                for (int c = c0; c <= c1; c++)
                {
                    var v = values[r, c];
                    // 배열 인덱스를 0부터 시작하도록 (r - r0, c - c0) 보정하여 저장합니다.
                    result[r - r0, c - c0] = ConvertCell(v, trim, emptyToNull);
                }
            }

            return result;
        }

        /// <summary>
        /// 개별 Excel 셀의 원시 데이터(Object) 타입을 판별하여 가장 적절한 문자열 포맷으로 변환합니다.
        /// 부동소수점 오차 제거 및 문화권(Culture)에 독립적인 포맷팅을 지원합니다.
        /// </summary>
        /// <param name="value2">단일 셀의 값 (Excel 원시 데이터)</param>
        /// <param name="trim">문자열 앞뒤 공백 제거 여부</param>
        /// <param name="emptyToNull">빈 문자열의 null 치환 여부</param>
        /// <returns>포맷팅이 완료된 문자열 또는 null</returns>
        public static string? ConvertCell(object? value2, bool trim = true, bool emptyToNull = true)
        {
            if (value2 == null)
                return null;

            string? s;

            // 데이터 타입별로 최적의 문자열 변환 수행
            if (value2 is string str)
            {
                s = str;
            }
            else if (value2 is double d)
            {
                // Excel에서 정수형 숫자가 Double로 읽혀오는 경우가 많습니다. (예: 1.0)
                // 이를 해결하기 위해 소수점 이하 값이 1e-9 이하라면 정수(long) 형태로 변환합니다.
                var rd = Math.Round(d);
                if (Math.Abs(d - rd) < 1e-9)
                    s = ((long)rd).ToString(CultureInfo.InvariantCulture);
                else
                    s = d.ToString(CultureInfo.InvariantCulture);
            }
            else if (value2 is float f)
            {
                s = f.ToString(CultureInfo.InvariantCulture);
            }
            else if (value2 is decimal m)
            {
                s = m.ToString(CultureInfo.InvariantCulture);
            }
            else if (value2 is int i)
            {
                s = i.ToString(CultureInfo.InvariantCulture);
            }
            else if (value2 is long l)
            {
                s = l.ToString(CultureInfo.InvariantCulture);
            }
            else if (value2 is short sh)
            {
                s = sh.ToString(CultureInfo.InvariantCulture);
            }
            else if (value2 is byte b)
            {
                s = b.ToString(CultureInfo.InvariantCulture);
            }
            else if (value2 is bool bo)
            {
                // 엑셀의 불리언 값은 TRUE / FALSE 대문자로 변환하여 직관성을 높입니다.
                s = bo ? "TRUE" : "FALSE";
            }
            else if (value2 is DateTime dt)
            {
                // 날짜 데이터의 포맷을 표준 "yyyy-MM-dd HH:mm:ss" 형태로 고정합니다.
                s = dt.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
            }
            else
            {
                s = Convert.ToString(value2, CultureInfo.InvariantCulture);
            }

            // 변환 실패 시 null 반환
            if (s == null)
                return null;

            // 옵션: 공백 제거
            if (trim)
                s = s.Trim();

            // 옵션: 비어 있는 문자열을 null로 취급
            if (emptyToNull && s.Length == 0)
                return null;

            return s;
        }
    }
}