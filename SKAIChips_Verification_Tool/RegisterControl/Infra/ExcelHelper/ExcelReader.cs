using System.Collections.Generic;
using System.IO;
using ClosedXML.Excel;

namespace SKAIChips_Verification_Tool.RegisterControl
{
    /// <summary>
    /// ClosedXML 라이브러리를 사용하여 엑셀 파일(.xlsx)의 데이터를 빠르고 안전하게 읽어오는 정적 유틸리티 클래스입니다.
    /// 백그라운드 엑셀 프로세스(COM Interop)를 사용하지 않으므로 자원 누수가 발생하지 않습니다.
    /// </summary>
    public static class ExcelReader
    {
        /// <summary>
        /// 지정된 엑셀 파일을 열고 포함된 모든 워크시트의 이름 목록을 반환합니다.
        /// (파일이 다른 프로그램에서 열려 있어도 읽을 수 있도록 FileShare.ReadWrite 권한을 사용합니다.)
        /// </summary>
        /// <param name="filePath">읽어올 엑셀 파일의 절대 또는 상대 경로입니다.</param>
        /// <returns>엑셀 파일에 포함된 워크시트 이름들의 리스트입니다.</returns>
        public static List<string> GetSheetNames(string filePath)
        {
            using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var wb = new XLWorkbook(fs);

            var list = new List<string>();
            foreach (var ws in wb.Worksheets)
            {
                list.Add(ws.Name);
            }

            return list;
        }

        /// <summary>
        /// 특정 워크시트에서 데이터가 입력된 전체 유효 영역(Used Range)을 찾아 2차원 문자열 배열로 반환합니다.
        /// </summary>
        /// <param name="filePath">읽어올 엑셀 파일의 경로입니다.</param>
        /// <param name="sheetName">데이터를 추출할 대상 워크시트의 이름입니다.</param>
        /// <returns>
        /// 데이터가 포함된 2차원 문자열 배열을 반환합니다. 
        /// 시트가 비어있을 경우 크기가 0인 배열을 반환하며, 값이 없는 빈 셀은 null로 처리됩니다.
        /// </returns>
        public static string[,] ReadUsedRangeAsStringArray(string filePath, string sheetName)
        {
            using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var wb = new XLWorkbook(fs);
            var ws = wb.Worksheet(sheetName);

            // 데이터가 있는 첫 번째 셀과 마지막 셀을 탐색
            var firstCell = ws.FirstCellUsed();
            var lastCell = ws.LastCellUsed();

            // 시트에 유효한 데이터가 전혀 없는 경우 빈 배열 반환
            if (firstCell == null || lastCell == null)
                return new string[0, 0];

            int firstRow = firstCell.Address.RowNumber;
            int firstCol = firstCell.Address.ColumnNumber;
            int lastRow = lastCell.Address.RowNumber;
            int lastCol = lastCell.Address.ColumnNumber;

            // 2차원 배열의 크기 계산
            int rows = lastRow - firstRow + 1;
            int cols = lastCol - firstCol + 1;

            var result = new string[rows, cols];

            // 유효 영역 내에서 실제로 값이 있는 셀들만 순회
            foreach (var cell in ws.Range(firstRow, firstCol, lastRow, lastCol).CellsUsed())
            {
                // 배열의 0,0 인덱스에 맞추기 위해 좌표 보정
                int r = cell.Address.RowNumber - firstRow;
                int c = cell.Address.ColumnNumber - firstCol;

                string val = cell.GetString();

                // 빈 문자열이거나 공백만 있는 경우 null로, 그 외에는 양쪽 공백을 제거하여 저장
                if (string.IsNullOrWhiteSpace(val))
                    result[r, c] = null;
                else
                    result[r, c] = val.Trim();
            }

            return result;
        }
    }
}