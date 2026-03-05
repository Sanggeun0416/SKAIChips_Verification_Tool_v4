namespace SKAIChips_Verification_Tool.RegisterControl
{
    /// <summary>
    /// 하드웨어 검증 테스트 시나리오가 실행되는 동안 유지되는 실행 컨텍스트 정보를 담는 클래스입니다.
    /// 테스트 실행 로직과 리포트 생성 세션을 연결하여, 테스트 결과가 올바른 문서에 기록될 수 있도록 지원합니다.
    /// </summary>
    public sealed class RunTestContext
    {
        /// <summary>
        /// 현재 실행 중인 테스트의 결과를 기록할 리포트 세션 객체를 가져옵니다.
        /// 이를 통해 테스트 도중 실시간으로 데이터를 엑셀 등에 작성할 수 있습니다.
        /// </summary>
        public ITestReportSession Report
        {
            get;
        }

        /// <summary>
        /// RunTestContext 클래스의 새 인스턴스를 초기화합니다.
        /// </summary>
        /// <param name="report">테스트와 연결될 리포트 보고 세션 인스턴스</param>
        public RunTestContext(ITestReportSession report)
        {
            Report = report;
        }
    }
}