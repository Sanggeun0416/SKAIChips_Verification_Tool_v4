namespace SKAIChips_Verification_Tool.RegisterControl
{
    /// <summary>
    /// 리포트 문서(예: 엑셀 워크시트)의 개별 시트를 제어하기 위한 추상 인터페이스입니다.
    /// 특정 시트 내에서 데이터의 읽기/쓰기, 서식 설정(폰트, 정렬, 테두리 등), 레이아웃 조정을 담당합니다.
    /// </summary>
    public interface IReportSheet
    {
        /// <summary>
        /// 현재 제어 중인 워크시트의 이름을 가져옵니다.
        /// </summary>
        string Name
        {
            get;
        }

        /// <summary>
        /// 지정된 특정 셀(행, 열)에 단일 값을 씁니다.
        /// </summary>
        /// <param name="row">행 번호 (1부터 시작)</param>
        /// <param name="col">열 번호 (1부터 시작)</param>
        /// <param name="value">입력할 데이터 값</param>
        void Write(int row, int col, object value);

        /// <summary>
        /// 지정된 행의 특정 열부터 여러 데이터를 가로 방향(Row)으로 이어서 씁니다.
        /// </summary>
        /// <param name="row">행 번호</param>
        /// <param name="startCol">데이터 쓰기를 시작할 열 번호</param>
        /// <param name="values">순차적으로 입력할 값들의 가변 배열</param>
        void WriteRow(int row, int startCol, params object[] values);

        /// <summary>
        /// 지정된 특정 셀의 현재 데이터를 읽어옵니다.
        /// </summary>
        /// <param name="row">행 번호</param>
        /// <param name="col">열 번호</param>
        /// <returns>셀에 포함된 데이터 (object 타입)</returns>
        object Read(int row, int col);

        /// <summary>
        /// 2차원 배열 형태의 데이터를 특정 시작 위치를 기준으로 대량 입력합니다.
        /// 단일 셀 입력 반복보다 성능상 유리합니다.
        /// </summary>
        /// <param name="startRow">시작 행 번호</param>
        /// <param name="startCol">시작 열 번호</param>
        /// <param name="values">입력할 데이터가 담긴 2차원 배열</param>
        void WriteRange(int startRow, int startCol, object[,] values);

        /// <summary>
        /// 엑셀 화면의 포커스(스크롤)를 특정 셀 위치로 이동시킵니다.
        /// </summary>
        /// <param name="row">행 번호</param>
        /// <param name="col">열 번호</param>
        void Focus(int row, int col);

        /// <summary>
        /// 지정된 범위 내의 셀들을 하나로 병합합니다.
        /// </summary>
        /// <param name="r1">시작 행</param>
        /// <param name="c1">시작 열</param>
        /// <param name="r2">종료 행</param>
        /// <param name="c2">종료 열</param>
        void Merge(int r1, int c1, int r2, int c2);

        /// <summary>
        /// 특정 범위 셀들의 폰트 스타일(글꼴, 크기, 굵게)을 설정합니다.
        /// </summary>
        /// <param name="r1">시작 행</param>
        /// <param name="c1">시작 열</param>
        /// <param name="r2">종료 행</param>
        /// <param name="c2">종료 열</param>
        /// <param name="fontName">글꼴 이름 (기본값: null, 변경 없음)</param>
        /// <param name="fontSize">글꼴 크기 (기본값: null, 변경 없음)</param>
        /// <param name="bold">굵게 표시 여부 (기본값: null, 변경 없음)</param>
        void SetFont(int r1, int c1, int r2, int c2, string fontName = null, double? fontSize = null, bool? bold = null);

        /// <summary>
        /// 현재 시트 전체 셀의 기본 폰트 스타일을 일괄 설정합니다.
        /// </summary>
        /// <param name="fontName">글꼴 이름</param>
        /// <param name="fontSize">글꼴 크기</param>
        /// <param name="bold">굵게 표시 여부</param>
        void SetSheetFont(string fontName = null, double? fontSize = null, bool? bold = null);

        /// <summary>
        /// 특정 범위 셀들의 숫자 또는 날짜 표시 형식(Format)을 지정합니다.
        /// </summary>
        /// <param name="r1">시작 행</param>
        /// <param name="c1">시작 열</param>
        /// <param name="r2">종료 행</param>
        /// <param name="c2">종료 열</param>
        /// <param name="format">엑셀 표시 형식 문자열 (예: "0.00", "YYYY-MM-DD")</param>
        void SetNumberFormat(int r1, int c1, int r2, int c2, string format);

        /// <summary>
        /// 특정 범위 셀들의 텍스트를 가로/세로 가운데 정렬로 설정합니다.
        /// </summary>
        /// <param name="r1">시작 행</param>
        /// <param name="c1">시작 열</param>
        /// <param name="r2">종료 행</param>
        /// <param name="c2">종료 열</param>
        /// <param name="wrapText">셀 크기에 맞게 텍스트 자동 줄 바꿈 여부 (기본값: false)</param>
        void SetAlignmentCenter(int r1, int c1, int r2, int c2, bool wrapText = false);

        /// <summary>
        /// 현재 시트의 모든 활성 셀을 일괄적으로 가운데 정렬합니다.
        /// </summary>
        void SetAlignmentCenterAll();

        /// <summary>
        /// 특정 범위 셀들의 모든 외곽선과 내부선에 테두리를 그립니다.
        /// </summary>
        /// <param name="r1">시작 행</param>
        /// <param name="c1">시작 열</param>
        /// <param name="r2">종료 행</param>
        /// <param name="c2">종료 열</param>
        void SetBorderAll(int r1, int c1, int r2, int c2);

        /// <summary>
        /// 입력된 데이터의 길이에 맞춰 열 너비와 행 높이를 자동으로 조정합니다.
        /// </summary>
        /// <param name="columns">열 너비 자동 맞춤 여부 (기본값: true)</param>
        /// <param name="rows">행 높이 자동 맞춤 여부 (기본값: false)</param>
        void AutoFit(bool columns = true, bool rows = false);
    }
}