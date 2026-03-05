using System.Globalization;

namespace SKAIChips_Verification_Tool.RegisterControl
{
    internal class ExcelDataConverter
    {
        public static string[,] ToStringArrayFromValue2(object? value2, bool trim = true, bool emptyToNull = true)
        {
            if (value2 == null)
                return new string[0, 0];

            if (value2 is object[,] matrix)
                return FromObjectMatrix(matrix, trim, emptyToNull);

            var single = new string[1, 1];
            single[0, 0] = ConvertCell(value2, trim, emptyToNull);
            return single;
        }

        public static string[,] FromObjectMatrix(object[,] values, bool trim = true, bool emptyToNull = true)
        {
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
                    result[r - r0, c - c0] = ConvertCell(v, trim, emptyToNull);
                }
            }

            return result;
        }

        public static string? ConvertCell(object? value2, bool trim = true, bool emptyToNull = true)
        {
            if (value2 == null)
                return null;

            string s;

            if (value2 is string str)
            {
                s = str;
            }
            else if (value2 is double d)
            {
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
                s = bo ? "TRUE" : "FALSE";
            }
            else if (value2 is DateTime dt)
            {
                s = dt.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
            }
            else
            {
                s = Convert.ToString(value2, CultureInfo.InvariantCulture);
            }

            if (s == null)
                return null;

            if (trim)
                s = s.Trim();

            if (emptyToNull && s.Length == 0)
                return null;

            return s;
        }
    }
}
