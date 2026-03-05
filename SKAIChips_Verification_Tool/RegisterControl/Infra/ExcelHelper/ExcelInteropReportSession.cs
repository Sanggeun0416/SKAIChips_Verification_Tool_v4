namespace SKAIChips_Verification_Tool.RegisterControl
{
    /// <summary>
    /// Excel Interop을 사용하여 테스트 결과 리포트 생성 및 저장 세션을 관리하는 클래스입니다.
    /// ITestReportSession 인터페이스를 구현하며, ExcelWriter를 통해 실제 엑셀 프로세스를 제어합니다.
    /// </summary>
    public sealed class ExcelInteropReportSession : ITestReportSession
    {
        private readonly ExcelWriter _xl;

        /// <summary>
        /// ExcelInteropReportSession 클래스의 새 인스턴스를 초기화하고, 지정된 경로의 엑셀 파일을 열거나 생성합니다.
        /// </summary>
        /// <param name="reportPath">생성 또는 열어볼 엑셀 파일의 절대 또는 상대 경로입니다.</param>
        /// <param name="visible">엑셀 애플리케이션 창의 화면 표시 여부입니다. (기본값: true)</param>
        public ExcelInteropReportSession(string reportPath, bool visible = true)
        {
            _xl = new ExcelWriter();
            _xl.OpenOrAttach(reportPath, visible: visible, readOnly: false, createIfMissing: true);
        }

        /// <summary>
        /// 지정된 이름의 새 워크시트를 생성하고, 해당 시트를 제어할 수 있는 객체를 반환합니다.
        /// </summary>
        /// <param name="sheetTitle">생성할 새 시트의 이름입니다.</param>
        /// <returns>생성된 시트를 제어하는 IReportSheet 인스턴스입니다.</returns>
        public IReportSheet CreateSheet(string sheetTitle)
        {
            var name = _xl.AddSheet(sheetTitle);
            return new ExcelInteropReportSheet(_xl, name);
        }

        /// <summary>
        /// 기존에 존재하는 워크시트를 선택하고, 해당 시트를 제어할 수 있는 객체를 반환합니다.
        /// </summary>
        /// <param name="sheetTitle">선택할 기존 시트의 이름입니다.</param>
        /// <returns>선택된 시트를 제어하는 IReportSheet 인스턴스입니다.</returns>
        public IReportSheet SelectSheet(string sheetTitle)
        {
            var name = _xl.SelectSheet(sheetTitle);
            return new ExcelInteropReportSheet(_xl, name);
        }

        /// <summary>
        /// 현재 세션에서 작업한 엑셀 파일의 변경 사항을 저장합니다.
        /// </summary>
        public void Save() => _xl.Save();

        /// <summary>
        /// 사용 중인 Excel Interop 관련 COM 리소스 및 백그라운드 프로세스를 해제합니다.
        /// </summary>
        public void Dispose() => _xl.Dispose();

        /// <summary>
        /// 특정 엑셀 워크시트 내에서의 데이터 입출력 및 셀 서식 설정을 담당하는 내부 구현 클래스입니다.
        /// </summary>
        private sealed class ExcelInteropReportSheet : IReportSheet
        {
            private readonly ExcelWriter _xl;

            /// <summary>
            /// 현재 제어 중인 엑셀 워크시트의 이름입니다.
            /// </summary>
            public string Name
            {
                get;
            }

            /// <summary>
            /// ExcelInteropReportSheet 클래스의 새 인스턴스를 초기화합니다.
            /// </summary>
            /// <param name="xl">부모 세션에서 사용하는 ExcelWriter 인스턴스입니다.</param>
            /// <param name="name">제어할 워크시트의 이름입니다.</param>
            public ExcelInteropReportSheet(ExcelWriter xl, string name)
            {
                _xl = xl;
                Name = name;
            }

            /// <summary>
            /// 지정된 특정 셀에 단일 값을 씁니다.
            /// </summary>
            /// <param name="row">행 번호 (1부터 시작)</param>
            /// <param name="col">열 번호 (1부터 시작)</param>
            /// <param name="value">셀에 입력할 값</param>
            public void Write(int row, int col, object value) => _xl.WriteCell(Name, row, col, value);

            /// <summary>
            /// 지정된 행(Row)의 특정 열(Column)부터 여러 값을 가로로 이어서 씁니다.
            /// </summary>
            /// <param name="row">행 번호 (1부터 시작)</param>
            /// <param name="startCol">시작 열 번호 (1부터 시작)</param>
            /// <param name="values">가로로 입력할 값들의 배열</param>
            public void WriteRow(int row, int startCol, params object[] values)
            {
                var arr = new object[1, values.Length];
                for (int i = 0; i < values.Length; i++)
                    arr[0, i] = values[i];
                _xl.WriteRange(Name, row, startCol, arr);
            }

            /// <summary>
            /// 지정된 셀의 값을 읽어옵니다.
            /// </summary>
            /// <param name="row">행 번호 (1부터 시작)</param>
            /// <param name="col">열 번호 (1부터 시작)</param>
            /// <returns>셀에 저장된 값 (object 타입)</returns>
            public object Read(int row, int col) => _xl.ReadCell(Name, row, col);

            /// <summary>
            /// 2차원 배열의 데이터를 지정된 시작 셀을 기준으로 일괄 입력합니다.
            /// </summary>
            /// <param name="startRow">시작 행 번호 (1부터 시작)</param>
            /// <param name="startCol">시작 열 번호 (1부터 시작)</param>
            /// <param name="values">입력할 2차원 데이터 배열</param>
            public void WriteRange(int startRow, int startCol, object[,] values) => _xl.WriteRange(Name, startRow, startCol, values);

            /// <summary>
            /// 지정된 셀로 화면 스크롤 및 포커스를 이동합니다.
            /// </summary>
            /// <param name="row">포커스할 행 번호</param>
            /// <param name="col">포커스할 열 번호</param>
            public void Focus(int row, int col)
            {
                _xl.Focus(Name, row, col);
            }

            /// <summary>
            /// 지정된 범위의 셀들을 하나로 병합합니다.
            /// </summary>
            /// <param name="r1">시작 행 번호</param>
            /// <param name="c1">시작 열 번호</param>
            /// <param name="r2">끝 행 번호</param>
            /// <param name="c2">끝 열 번호</param>
            public void Merge(int r1, int c1, int r2, int c2) => _xl.Merge(Name, r1, c1, r2, c2);

            /// <summary>
            /// 특정 셀 범위의 폰트 스타일을 설정합니다.
            /// </summary>
            /// <param name="r1">시작 행 번호</param>
            /// <param name="c1">시작 열 번호</param>
            /// <param name="r2">끝 행 번호</param>
            /// <param name="c2">끝 열 번호</param>
            /// <param name="fontName">폰트 이름 (예: "맑은 고딕"). null일 경우 변경하지 않음.</param>
            /// <param name="fontSize">폰트 크기. null일 경우 변경하지 않음.</param>
            /// <param name="bold">굵게 처리 여부. null일 경우 변경하지 않음.</param>
            public void SetFont(int r1, int c1, int r2, int c2, string fontName = null, double? fontSize = null, bool? bold = null)
                => _xl.SetFont(Name, r1, c1, r2, c2, fontName, fontSize, bold);

            /// <summary>
            /// 워크시트 전체의 기본 폰트 스타일을 설정합니다.
            /// </summary>
            /// <param name="fontName">폰트 이름 (예: "맑은 고딕")</param>
            /// <param name="fontSize">폰트 크기</param>
            /// <param name="bold">굵게 처리 여부</param>
            public void SetSheetFont(string fontName = null, double? fontSize = null, bool? bold = null)
                => _xl.SetSheetFont(Name, fontName, fontSize, bold);

            /// <summary>
            /// 특정 셀 범위의 숫자 표시 형식(포맷)을 설정합니다.
            /// </summary>
            /// <param name="r1">시작 행 번호</param>
            /// <param name="c1">시작 열 번호</param>
            /// <param name="r2">끝 행 번호</param>
            /// <param name="c2">끝 열 번호</param>
            /// <param name="format">적용할 엑셀 형식 문자열 (예: "0.00", "0%")</param>
            public void SetNumberFormat(int r1, int c1, int r2, int c2, string format)
                => _xl.SetNumberFormat(Name, r1, c1, r2, c2, format);

            /// <summary>
            /// 특정 셀 범위의 텍스트를 가운데 정렬합니다.
            /// </summary>
            /// <param name="r1">시작 행 번호</param>
            /// <param name="c1">시작 열 번호</param>
            /// <param name="r2">끝 행 번호</param>
            /// <param name="c2">끝 열 번호</param>
            /// <param name="wrapText">텍스트 줄 바꿈 적용 여부 (기본값: false)</param>
            public void SetAlignmentCenter(int r1, int c1, int r2, int c2, bool wrapText = false)
                => _xl.SetAlignment(Name, r1, c1, r2, c2, wrapText: wrapText);

            /// <summary>
            /// 워크시트 내의 모든 사용 중인 셀을 가운데 정렬합니다.
            /// </summary>
            public void SetAlignmentCenterAll()
                => _xl.SetAlignmentAll(Name);

            /// <summary>
            /// 특정 셀 범위의 테두리 선을 모두 표시하도록 설정합니다.
            /// </summary>
            /// <param name="r1">시작 행 번호</param>
            /// <param name="c1">시작 열 번호</param>
            /// <param name="r2">끝 행 번호</param>
            /// <param name="c2">끝 열 번호</param>
            public void SetBorderAll(int r1, int c1, int r2, int c2)
                => _xl.SetBorderAll(Name, r1, c1, r2, c2);

            /// <summary>
            /// 셀의 내용에 맞게 열 너비와 행 높이를 자동으로 조정합니다.
            /// </summary>
            /// <param name="columns">열 너비 자동 맞춤 여부 (기본값: true)</param>
            /// <param name="rows">행 높이 자동 맞춤 여부 (기본값: false)</param>
            public void AutoFit(bool columns = true, bool rows = false) => _xl.AutoFit(Name, columns, rows);
        }
    }
}