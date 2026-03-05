using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace SKAIChips_Verification_Tool.RegisterControl
{
    /// <summary>
    /// PC에 연결된 FTDI 기반 USB 장치 목록을 검색하고, 
    /// 하드웨어 통신(I2C/SPI)에 사용할 대상 장치를 선택하는 다이얼로그 폼입니다.
    /// FTD2XX.dll (FTDI 공식 드라이버 라이브러리)을 사용하여 장치를 열거합니다.
    /// </summary>
    public partial class FtdiSetupForm : Form
    {
        #region Properties

        /// <summary>
        /// 사용자가 목록에서 선택하고 [OK] 버튼을 눌렀을 때 생성되는 
        /// 최종 FTDI 장치 설정 정보(인덱스, 설명, 시리얼 등)입니다.
        /// </summary>
        public FtdiDeviceSettings Result
        {
            get; private set;
        }

        #endregion

        #region Constructor & Initialization

        /// <summary>
        /// FtdiSetupForm의 새 인스턴스를 초기화합니다.
        /// </summary>
        /// <param name="current">이전에 선택되어 있던 FTDI 장치 설정 (목록에서 해당 항목을 기본 선택하기 위함)</param>
        public FtdiSetupForm(FtdiDeviceSettings current = null)
        {
            InitializeComponent();

            // 폼 로드 시 PC에 연결된 FTDI 장치 목록을 스캔하여 ListView에 표시
            LoadDeviceList();

            // 이전 설정값이 있다면 해당 장치를 포커스
            if (current != null)
                ApplyCurrent(current);
        }

        #endregion

        #region UI Event Handlers

        /// <summary>
        /// [Refresh] 버튼 클릭 시, USB에 연결된 FTDI 장치 목록을 다시 스캔합니다.
        /// </summary>
        private void btnRefresh_Click(object sender, EventArgs e)
        {
            LoadDeviceList();
        }

        /// <summary>
        ///[OK] 버튼 클릭 시, ListView에서 선택된 항목을 파싱하여 Result 객체를 생성하고 폼을 닫습니다.
        /// </summary>
        private void btnOk_Click(object sender, EventArgs e)
        {
            var idx = GetSelectedDeviceIndex();
            if (idx < 0)
            {
                MessageBox.Show("연결할 장치를 목록에서 선택해 주세요.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var item = lvDevices.Items[idx];

            // ListView 항목의 SubItems 구조: [0] Index, [1] Description, [2] Serial, [3] Location
            Result = new FtdiDeviceSettings
            {
                DeviceIndex = int.Parse(item.SubItems[0].Text),
                Description = item.SubItems[1].Text,
                SerialNumber = item.SubItems[2].Text,
                Location = item.SubItems[3].Text
            };

            DialogResult = DialogResult.OK;
            Close();
        }

        /// <summary>
        /// [Cancel] 버튼 클릭 시 변경사항을 취소하고 폼을 닫습니다.
        /// </summary>
        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        #endregion

        #region UI Helper Methods

        /// <summary>
        /// ListView(lvDevices)에서 현재 선택된 아이템의 인덱스를 반환합니다.
        /// </summary>
        /// <returns>선택된 인덱스 번호 (선택된 항목이 없으면 -1)</returns>
        private int GetSelectedDeviceIndex()
        {
            if (lvDevices.SelectedItems.Count == 0)
                return -1;

            return lvDevices.SelectedItems[0].Index;
        }

        /// <summary>
        /// 기존에 설정된 장치 정보가 있을 경우, 해당 장치(DeviceIndex)를 ListView에서 찾아 자동으로 선택 상태로 만듭니다.
        /// </summary>
        /// <param name="current">이전에 설정된 FTDI 장치 정보</param>
        private void ApplyCurrent(FtdiDeviceSettings current)
        {
            for (var i = 0; i < lvDevices.Items.Count; i++)
            {
                var item = lvDevices.Items[i];
                if (!int.TryParse(item.SubItems[0].Text, out var devIdx))
                    continue;

                // 기존 장치의 인덱스와 일치하는 항목을 찾으면 선택 및 스크롤 이동
                if (devIdx == current.DeviceIndex)
                {
                    item.Selected = true;
                    item.Focused = true;
                    lvDevices.EnsureVisible(i);
                    break;
                }
            }
        }

        #endregion

        #region FTDI Device Enumeration Logic

        /// <summary>
        /// 네이티브 FTDI API(FTD2XX)를 호출하여 PC에 연결된 장치 개수와 세부 정보를 읽어와 ListView에 채웁니다.
        /// </summary>
        private void LoadDeviceList()
        {
            lvDevices.Items.Clear();

            uint numDevs = 0;

            // 1. 연결된 FTDI 장치 개수 확인
            var status = FT_CreateDeviceInfoList(ref numDevs);
            if (status != FT_STATUS.FT_OK)
            {
                MessageBox.Show($"FT_CreateDeviceInfoList 호출에 실패했습니다.\n에러 코드: {status}", "드라이버 오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // 2. 검색된 장치 개수만큼 반복하여 세부 정보 추출
            for (uint i = 0; i < numDevs; i++)
            {
                uint flags = 0;
                uint type = 0;
                uint id = 0;
                uint locId = 0;
                var serial = new byte[16];
                var desc = new byte[64];
                var handle = IntPtr.Zero;

                status = FT_GetDeviceInfoDetail(
                    i,
                    ref flags,
                    ref type,
                    ref id,
                    ref locId,
                    serial,
                    desc,
                    ref handle);

                if (status != FT_STATUS.FT_OK)
                    continue;

                // C-Style Byte Array(Null-Terminated)를 C# String으로 변환
                var serialStr = BytesToString(serial);
                var descStr = BytesToString(desc);
                var locStr = $"0x{locId:X8}";

                // ListView에 추가 [Index, Description, Serial, Location]
                var lvi = new ListViewItem(i.ToString());
                lvi.SubItems.Add(descStr);
                lvi.SubItems.Add(serialStr);
                lvi.SubItems.Add(locStr);

                lvDevices.Items.Add(lvi);
            }

            // 검색된 장치가 있다면 첫 번째 항목을 기본 선택
            if (lvDevices.Items.Count > 0)
                lvDevices.Items[0].Selected = true;
        }

        /// <summary>
        /// FTDI 드라이버에서 반환한 C-Style Null 문자('\0') 종단 바이트 배열을 C# 문자열(String)로 변환합니다.
        /// </summary>
        /// <param name="buf">ASCII 문자열 데이터가 담긴 바이트 배열</param>
        /// <returns>Null 문자 이후의 가비지가 제거된 깔끔한 문자열</returns>
        private static string BytesToString(byte[] buf)
        {
            if (buf == null || buf.Length == 0)
                return string.Empty;

            var s = Encoding.ASCII.GetString(buf);
            var idx = s.IndexOf('\0'); // 첫 번째 Null 문자의 위치 찾기

            if (idx >= 0)
                s = s.Substring(0, idx); // Null 문자 이전까지만 자르기

            return s.Trim();
        }

        #endregion

        #region FTD2XX Native Interop (P/Invoke)

        /// <summary>
        /// FTDI 공식 D2XX 드라이버에서 반환하는 상태 및 에러 코드 열거형입니다.
        /// </summary>
        private enum FT_STATUS : uint
        {
            FT_OK = 0,
            FT_INVALID_HANDLE,
            FT_DEVICE_NOT_FOUND,
            FT_DEVICE_NOT_OPENED,
            FT_IO_ERROR,
            FT_INSUFFICIENT_RESOURCES,
            FT_INVALID_PARAMETER,
            FT_INVALID_BAUD_RATE,

            FT_DEVICE_NOT_OPENED_FOR_ERASE = 0x10,
            FT_DEVICE_NOT_OPENED_FOR_WRITE,
            FT_FAILED_TO_WRITE_DEVICE,
            FT_EEPROM_READ_FAILED,
            FT_EEPROM_WRITE_FAILED,
            FT_EEPROM_ERASE_FAILED,
            FT_EEPROM_NOT_PRESENT,
            FT_EEPROM_NOT_PROGRAMMED,
            FT_INVALID_ARGS,
            FT_NOT_SUPPORTED,
            FT_OTHER_ERROR
        }

        /// <summary>
        /// 시스템에 연결된 FTDI 장치의 내부 목록을 작성하고 연결된 장치의 개수를 반환합니다.
        /// </summary>
        /// <param name="numDevs">검색된 FTDI 장치의 개수가 저장될 포인터 변수</param>
        /// <returns>함수 실행 결과 상태값 (FT_OK 시 성공)</returns>
        [DllImport("FTD2XX.dll")]
        private static extern FT_STATUS FT_CreateDeviceInfoList(ref uint numDevs);

        /// <summary>
        /// 특정 인덱스의 장치에 대한 세부 식별 정보(타입, 시리얼, 설명 등)를 가져옵니다.
        /// </summary>
        /// <param name="index">조회할 장치의 인덱스 (0 부터 numDevs - 1 까지)</param>
        /// <param name="flags">장치 상태 플래그</param>
        /// <param name="type">장치 타입 식별자</param>
        /// <param name="id">Vendor ID 및 Product ID 정보</param>
        /// <param name="locId">장치의 물리적 USB 위치 ID</param>
        /// <param name="serialNumber">시리얼 번호가 저장될 배열 포인터 (16바이트 할당 권장)</param>
        /// <param name="description">장치 설명 문구가 저장될 배열 포인터 (64바이트 할당 권장)</param>
        /// <param name="ftHandle">장치 제어 핸들 (열려 있지 않으면 IntPtr.Zero)</param>
        /// <returns>함수 실행 결과 상태값</returns>[DllImport("FTD2XX.dll")]
        private static extern FT_STATUS FT_GetDeviceInfoDetail(
            uint index,
            ref uint flags,
            ref uint type,
            ref uint id,
            ref uint locId, [Out] byte[] serialNumber,
            [Out] byte[] description,
            ref IntPtr ftHandle);

        #endregion
    }
}