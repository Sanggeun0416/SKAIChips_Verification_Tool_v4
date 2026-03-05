using System;

namespace SKAIChips_Verification_Tool.RegisterControl
{
    /// <summary>
    /// 하드웨어 검증 결과 리포트 파일의 전체 세션(Session)을 관리하는 인터페이스입니다.
    /// 리포트 파일의 생성, 저장 및 종료(Dispose)를 담당하며, 개별 워크시트(IReportSheet)를 생성하거나 선택하는 기능을 제공합니다.
    /// </summary>
    public interface ITestReportSession : IDisposable
    {
        /// <summary>
        /// 리포트 문서 내에 새로운 워크시트를 생성하고, 해당 시트를 제어할 수 있는 객체를 반환합니다.
        /// </summary>
        /// <param name="sheetTitle">새로 생성할 시트의 이름 (중복될 경우 구현체에 따라 이름이 조정될 수 있음)</param>
        /// <returns>생성된 시트 제어를 위한 IReportSheet 인스턴스</returns>
        IReportSheet CreateSheet(string sheetTitle);

        /// <summary>
        /// 이미 존재하는 워크시트를 이름으로 찾아 해당 시트를 제어할 수 있는 객체를 반환합니다.
        /// </summary>
        /// <param name="sheetTitle">찾고자 하는 시트의 이름</param>
        /// <returns>선택된 시트 제어를 위한 IReportSheet 인스턴스</returns>
        IReportSheet SelectSheet(string sheetTitle);

        /// <summary>
        /// 현재 세션에서 작업 중인 리포트 파일의 변경 사항을 파일 시스템에 즉시 저장합니다.
        /// </summary>
        void Save();
    }
}