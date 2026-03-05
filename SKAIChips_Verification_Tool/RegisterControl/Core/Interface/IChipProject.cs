using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SKAIChips_Verification_Tool.RegisterControl
{
    /// <summary>
    /// SKAIChips에서 검증할 개별 칩(Chip) 프로젝트가 공통적으로 가져야 할 기본 정보와 동작을 정의하는 최상위 인터페이스입니다.
    /// </summary>
    public interface IChipProject
    {
        /// <summary>
        /// 칩 프로젝트의 이름 (예: "SKAI1000")
        /// </summary>
        string Name
        {
            get;
        }

        /// <summary>
        /// 검색이나 필터링에 사용될 프로젝트 관련 키워드 목록
        /// </summary>
        IEnumerable<string> ProjectKeywords
        {
            get;
        }

        /// <summary>
        /// 해당 칩이 지원하는 통신 프로토콜 목록 (I2C, SPI 등)
        /// </summary>
        IEnumerable<ProtocolRegLogType> SupportedProtocols
        {
            get;
        }

        /// <summary>
        /// 메인 UI의 커스텀 테스트 슬롯(버튼)에 바인딩할 사용자 정의 동작(Action) 목록을 반환합니다.
        /// </summary>
        TestSlotAction[] GetTestSlotActions();

        /// <summary>
        /// 칩과 통신할 때 사용할 기본 통신 주파수 (예: I2C의 경우 400KHz -> 400)
        /// </summary>
        uint ComFrequency
        {
            get;
        }

        /// <summary>
        /// 칩의 하드웨어 디바이스 주소 (예: I2C Slave Address)
        /// </summary>
        byte DeviceAddress
        {
            get;
        }
    }

    /// <summary>
    /// I2C 통신 프로토콜을 사용하는 칩 프로젝트를 위한 인터페이스입니다.
    /// </summary>
    public interface II2cChipProject : IChipProject
    {
        /// <summary>
        /// 주어진 I2C 버스와 설정을 사용하여 실제 레지스터 제어가 가능한 칩 인스턴스를 생성합니다.
        /// </summary>
        /// <param name="bus">I2C 통신 버스 인스턴스</param>
        /// <param name="settings">통신 프로토콜 설정</param>
        /// <returns>생성된 I2C 레지스터 제어 칩 인스턴스</returns>
        IRegisterChip CreateChip(II2cBus bus, ProtocolSettings settings);
    }

    /// <summary>
    /// SPI 통신 프로토콜을 사용하는 칩 프로젝트를 위한 인터페이스입니다.
    /// </summary>
    public interface ISpiChipProject : IChipProject
    {
        /// <summary>
        /// 주어진 SPI 버스와 설정을 사용하여 실제 레지스터 제어가 가능한 칩 인스턴스를 생성합니다.
        /// </summary>
        /// <param name="bus">SPI 통신 버스 인스턴스</param>
        /// <param name="settings">통신 프로토콜 설정</param>
        /// <returns>생성된 SPI 레지스터 제어 칩 인스턴스</returns>
        IRegisterChip CreateChip(ISpiBus bus, ProtocolSettings settings);
    }

    /// <summary>
    /// 단순 레지스터 읽기/쓰기를 넘어, 자동화된 검증 시나리오(Test Suite)를 포함하는 칩 프로젝트 인터페이스입니다.
    /// </summary>
    public interface IChipProjectWithTests : IChipProject
    {
        /// <summary>
        /// 해당 칩에 대해 수행할 수 있는 자동화 테스트 스위트를 생성합니다.
        /// </summary>
        /// <param name="chip">테스트 대상이 되는 칩 인스턴스</param>
        /// <returns>테스트 시나리오들을 관리하고 실행하는 테스트 스위트 인스턴스</returns>
        IChipTestSuite CreateTestSuite(IRegisterChip chip);
    }

    /// <summary>
    /// 특정 칩에 대해 수행할 수 있는 자동화된 하드웨어 검증 테스트 시나리오들을 정의하고 실행하는 인터페이스입니다.
    /// </summary>
    public interface IChipTestSuite
    {
        /// <summary>
        /// 이 테스트 스위트에 포함된 모든 개별 테스트 항목들의 메타데이터 목록입니다.
        /// </summary>
        IReadOnlyList<ChipTestInfo> Tests
        {
            get;
        }

        /// <summary>
        /// 특정 테스트를 실행하기 전, UI 컨텍스트를 통해 초기화나 사전 준비 작업을 수행합니다.
        /// </summary>
        /// <param name="testId">준비할 테스트의 고유 ID</param>
        /// <param name="uiContext">진행 상황 표시 등을 위한 UI 컨텍스트</param>
        /// <returns>준비가 성공적으로 완료되었으면 true, 그렇지 않으면 false</returns>
        bool PrepareTest(string testId, ITestUiContext uiContext);

        /// <summary>
        /// 지정된 ID의 테스트 시나리오를 비동기적으로 실행합니다.
        /// </summary>
        /// <param name="testId">실행할 테스트의 고유 ID</param>
        /// <param name="log">테스트 진행 상황이나 결과를 UI에 출력하기 위한 로깅 콜백 함수</param>
        /// <param name="cancellationToken">사용자가 테스트를 강제 취소할 때 사용하는 토큰</param>
        Task Run_TEST(string testId, Func<string, string, Task> log, CancellationToken cancellationToken);
    }

    /// <summary>
    /// 개별 검증 테스트 시나리오의 메타데이터(ID, 이름, 설명, 분류)를 담는 불변(Immutable) 데이터 클래스입니다.
    /// </summary>
    public sealed class ChipTestInfo
    {
        /// <summary>
        /// 테스트를 식별하는 고유 ID
        /// </summary>
        public string Id
        {
            get;
        }

        /// <summary>
        /// UI에 표시될 테스트의 짧은 이름
        /// </summary>
        public string Name
        {
            get;
        }

        /// <summary>
        /// 테스트가 어떤 항목을 검증하는지에 대한 상세 설명
        /// </summary>
        public string Description
        {
            get;
        }

        /// <summary>
        /// 테스트들을 그룹화하기 위한 카테고리 (예: "전원부 검증", "통신 검증")
        /// </summary>
        public string Category
        {
            get;
        }

        /// <summary>
        /// ChipTestInfo 인스턴스를 초기화합니다.
        /// </summary>
        public ChipTestInfo(string id, string name, string description, string category)
        {
            Id = id;
            Name = name;
            Description = description;
            Category = category;
        }
    }
}