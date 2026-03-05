using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace SKAIChips_Verification_Tool.RegisterControl
{
    /// <summary>
    /// 여러 바이트의 Hex(16진수) 데이터를 스프레드시트(Grid) 형태로 편집하거나 조회할 수 있는 다이얼로그 폼입니다.
    /// 메모리 맵핑, 버퍼 데이터 입력, 멀티 바이트 레지스터 제어 시 UI 도구로 사용됩니다.
    /// </summary>
    public class HexWriteForm : Form
    {
        #region Constants & Fields

        private const int TOTAL_COLS = 32;
        private const int MAX_ROWS = 10;
        private const int COL_WIDTH = 38;
        private const int ROW_HEADER_WIDTH = 55;

        /// <summary>그리드에서 사용할 고정 폭 글꼴입니다.</summary>
        private readonly Font GridFont = new Font("Consolas", 10f);

        // UI Controls
        private DataGridView grid;
        private Panel buttonPanel;
        private Button btnOK;
        private Button btnCancel;

        // Data Properties
        private readonly int totalBytes;
        private readonly int segmentsPerGrid;
        private readonly int numRows;
        private readonly byte[] initialData;

        /// <summary>
        /// 사용자가 편집을 완료하고 [Write/OK] 버튼을 눌렀을 때 반환되는 최종 바이트 배열입니다.
        /// </summary>
        public byte[] ResultData
        {
            get; private set;
        }

        #endregion

        #region Constructor & Initialization

        /// <summary>
        /// HexWriteForm의 새 인스턴스를 초기화합니다.
        /// </summary>
        /// <param name="totalBytes">입력받을 또는 표시할 총 바이트 수</param>
        /// <param name="segmentsPerGrid">그리드 한 행(Row)에 표시할 바이트 개수 (열 수)</param>
        /// <param name="initial">초기화에 사용할 바이트 배열 (없을 경우 null)</param>
        public HexWriteForm(int totalBytes, int segmentsPerGrid, byte[] initial = null)
        {
            this.totalBytes = totalBytes;
            this.segmentsPerGrid = segmentsPerGrid;
            this.initialData = initial;

            // 표시해야 할 행(Row) 개수 계산 (최대 MAX_ROWS로 제한)
            numRows = segmentsPerGrid > 0
                ? Math.Min((int)Math.Ceiling((double)totalBytes / segmentsPerGrid), MAX_ROWS)
                : 0;

            InitializeFormProperties();

            BuildGrid();
            ApplyColumnVisibility();
            BuildButtons();

            Controls.Add(grid);
            Controls.Add(buttonPanel);

            // 데이터 바인딩 및 활성 영역 스타일 지정
            ApplyEditableRange(initialData);

            // 데이터 길이에 맞게 창 크기 자동 조절
            FitWindowToGrid();

            // 이벤트 핸들러 등록
            grid.EditingControlShowing += Grid_EditingControlShowing;
            grid.KeyDown += Grid_KeyDown;

            AcceptButton = btnOK;
            CancelButton = btnCancel;
        }

        /// <summary>
        /// 폼의 기본 외형과 동작 속성을 설정합니다.
        /// </summary>
        private void InitializeFormProperties()
        {
            Text = "Hex Data Editor";
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
        }

        /// <summary>
        /// 디자이너 초기화를 위한 빈 메서드 (수동으로 UI를 구성하므로 비워둠)
        /// </summary>
        private void InitializeComponent()
        {
        }

        #endregion

        #region UI Construction & Styling

        /// <summary>
        /// DataGridView 컨트롤을 생성하고 행/열의 기본 속성 및 스타일을 설정합니다.
        /// </summary>
        private void BuildGrid()
        {
            grid = new DataGridView
            {
                Dock = DockStyle.Top,
                BorderStyle = BorderStyle.FixedSingle,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeColumns = false,
                AllowUserToResizeRows = false,
                MultiSelect = true,
                SelectionMode = DataGridViewSelectionMode.CellSelect,
                RowHeadersVisible = true,
                RowHeadersWidth = ROW_HEADER_WIDTH,
                AutoGenerateColumns = false,
                ScrollBars = ScrollBars.None,
                Font = GridFont
            };

            grid.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            grid.DefaultCellStyle.Font = GridFont;
            grid.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            grid.ColumnHeadersDefaultCellStyle.Font = GridFont;
            grid.RowHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            grid.RowHeadersDefaultCellStyle.Font = GridFont;

            // 최대치(TOTAL_COLS) 만큼 열 생성 후 숨김 처리
            grid.Columns.Clear();
            for (int c = 0; c < TOTAL_COLS; c++)
            {
                grid.Columns.Add(new DataGridViewTextBoxColumn
                {
                    HeaderText = $"S{c + 1}",
                    SortMode = DataGridViewColumnSortMode.NotSortable,
                    Width = COL_WIDTH,
                    Visible = false
                });
            }

            // 필요한 개수만큼 행 생성
            grid.Rows.Clear();
            if (numRows > 0)
            {
                grid.Rows.Add(numRows);
                for (int r = 0; r < numRows; r++)
                {
                    grid.Rows[r].HeaderCell.Value = $"G{r + 1}";
                    grid.Rows[r].Height = 24;
                }
            }

            // 전체 셀을 읽기 전용/비활성화 상태 스타일로 초기화
            foreach (DataGridViewRow row in grid.Rows)
            {
                foreach (DataGridViewCell cell in row.Cells)
                {
                    cell.Value = "--";
                    cell.ReadOnly = true;
                    cell.Style.BackColor = SystemColors.Control;
                    cell.Style.ForeColor = SystemColors.GrayText;
                }
            }
        }

        /// <summary>
        /// 설정된 열(segmentsPerGrid) 개수만큼만 그리드 컬럼을 표시하도록 설정합니다.
        /// </summary>
        private void ApplyColumnVisibility()
        {
            for (int c = 0; c < TOTAL_COLS; c++)
            {
                if (grid.Columns.Count > c)
                    grid.Columns[c].Visible = c < segmentsPerGrid;
            }
        }

        /// <summary>
        /// 하단 패널 및 OK, Cancel 버튼을 생성하고 리사이즈 이벤트를 매핑합니다.
        /// </summary>
        private void BuildButtons()
        {
            buttonPanel = new Panel { Dock = DockStyle.Bottom, Height = 50, Padding = new Padding(10) };

            btnOK = new Button { Text = "Write", DialogResult = DialogResult.OK, Width = 90, Height = 30 };
            btnOK.Click += BtnOK_Click;

            btnCancel = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel, Width = 90, Height = 30 };

            buttonPanel.Controls.Add(btnOK);
            buttonPanel.Controls.Add(btnCancel);

            // 창 크기 변경 시 버튼을 항상 우측 정렬
            buttonPanel.Resize += (s, e) =>
            {
                int spacing = 8;
                btnCancel.Location = new Point(buttonPanel.Width - btnCancel.Width - 12, (buttonPanel.Height - btnCancel.Height) / 2);
                btnOK.Location = new Point(btnCancel.Left - btnOK.Width - spacing, btnCancel.Top);
            };
        }

        /// <summary>
        /// 활성화된 그리드의 행/열 개수에 맞춰 폼의 전체 창 크기를 동적으로 축소/확장합니다.
        /// </summary>
        private void FitWindowToGrid()
        {
            int totalW = grid.RowHeadersWidth + 2;
            foreach (DataGridViewColumn col in grid.Columns)
            {
                if (col.Visible)
                    totalW += col.Width;
            }

            int headerH = grid.ColumnHeadersVisible ? grid.ColumnHeadersHeight : 0;
            int rowsH = 0;
            foreach (DataGridViewRow row in grid.Rows)
            {
                rowsH += row.Height;
            }
            int totalH = headerH + rowsH + 2;

            grid.Size = new Size(totalW, totalH);
            ClientSize = new Size(totalW, totalH + buttonPanel.Height);
        }

        #endregion

        #region Data Mapping & Mode Settings

        /// <summary>
        /// 요구된 바이트 개수(totalBytes) 영역만큼만 입력 가능한(흰색 배경) 셀로 변경하고, 초기값을 매핑합니다.
        /// </summary>
        /// <param name="initialRange">셀에 채워넣을 초기 바이트 배열</param>
        private void ApplyEditableRange(byte[] initialRange)
        {
            if (totalBytes <= 0 || segmentsPerGrid == 0)
                return;

            for (int idx = 0; idx < totalBytes; idx++)
            {
                int r = idx / segmentsPerGrid;
                int c = idx % segmentsPerGrid;

                // 초기값이 있으면 Hex 텍스트로, 없으면 "00"으로 채움
                string value = (initialRange != null && idx < initialRange.Length)
                    ? initialRange[idx].ToString("X2")
                    : "00";

                if (r < grid.Rows.Count && c < grid.Columns.Count)
                {
                    var cell = grid.Rows[r].Cells[c];
                    cell.Value = value;
                    cell.ReadOnly = false;
                    cell.Style.BackColor = Color.White;
                    cell.Style.ForeColor = Color.Black;
                }
            }

            // 편집 가능 영역의 첫 번째 셀을 활성화(포커스)
            if (grid.Rows.Count > 0 && grid.Columns.Count > 0)
            {
                grid.ClearSelection();
                grid.CurrentCell = grid.Rows[0].Cells[0];
                grid.Rows[0].Cells[0].Selected = true;
            }
        }

        /// <summary>
        /// 이 폼을 데이터를 수정할 수 없는 "읽기 전용 모드"로 전환합니다.
        /// </summary>
        public void SetReadOnlyMode()
        {
            grid.ReadOnly = true;

            for (int idx = 0; idx < totalBytes; idx++)
            {
                if (segmentsPerGrid == 0)
                    continue;

                int r = idx / segmentsPerGrid;
                int c = idx % segmentsPerGrid;

                if (r < grid.Rows.Count && c < grid.Columns.Count)
                {
                    var cell = grid.Rows[r].Cells[c];
                    // 편집 불가 상태임을 나타내는 회색 배경 적용
                    cell.Style.BackColor = SystemColors.Control;
                    cell.Style.ForeColor = SystemColors.GrayText;
                }
            }

            // Write 버튼을 숨기고 Cancel 버튼의 역할을 Close(닫기)로 변경
            if (btnOK != null)
            {
                btnOK.Enabled = false;
                btnOK.Visible = false;
            }

            if (btnCancel != null)
                btnCancel.Text = "Close";
        }

        #endregion

        #region Grid Event Handlers (Input Validation & Clipboard)

        /// <summary>
        /// 문자열이 1~2자리의 올바른 16진수 포맷인지 검사합니다.
        /// </summary>
        private static bool IsHexByte(string s) => Regex.IsMatch(s, @"\A[0-9A-Fa-f]{1,2}\z");

        /// <summary>
        /// 사용자가 셀 편집(타이핑)을 시작할 때 발생하는 이벤트입니다. 
        /// 대문자로 강제하고 입력 가능한 길이를 제한하며 문자 필터링 이벤트를 부착합니다.
        /// </summary>
        private void Grid_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            if (e.Control is TextBox tb)
            {
                tb.CharacterCasing = CharacterCasing.Upper;
                tb.MaxLength = 2; // 한 바이트(Hex)는 최대 2글자

                // 이벤트 중복 등록 방지 후 연결
                tb.KeyPress -= Tb_KeyPressHexOnly;
                tb.KeyPress += Tb_KeyPressHexOnly;
            }
        }

        /// <summary>
        /// 셀 텍스트박스 입력 시 16진수에 해당하는 문자(0-9, A-F, a-f)와 제어 문자만 허용합니다.
        /// </summary>
        private void Tb_KeyPressHexOnly(object sender, KeyPressEventArgs e)
        {
            char ch = e.KeyChar;

            // 백스페이스 등의 제어 문자는 허용
            if (char.IsControl(ch))
                return;

            bool isHexChar = (ch >= '0' && ch <= '9') ||
                             (ch >= 'A' && ch <= 'F') ||
                             (ch >= 'a' && ch <= 'f');

            // 16진수 문자가 아니면 이벤트 핸들링 처리하여 입력 무시
            e.Handled = !isHexChar;
        }

        /// <summary>
        /// 그리드에서 발생하는 키 이벤트를 처리합니다. (단축키 지원)
        /// Ctrl+A(전체 선택), Ctrl+V(클립보드 붙여넣기) 기능을 제공합니다.
        /// </summary>
        private void Grid_KeyDown(object sender, KeyEventArgs e)
        {
            if (segmentsPerGrid == 0)
                return;

            // [Ctrl + A] : 데이터가 있는 활성 영역(totalBytes) 전체 선택
            if (e.Control && e.KeyCode == Keys.A)
            {
                grid.ClearSelection();
                for (int idx = 0; idx < totalBytes; idx++)
                {
                    int r = idx / segmentsPerGrid;
                    int c = idx % segmentsPerGrid;
                    grid.Rows[r].Cells[c].Selected = true;
                }
                e.SuppressKeyPress = true;
                return;
            }

            // [Ctrl + V] : 클립보드 텍스트 붙여넣기 기능
            if (e.Control && e.KeyCode == Keys.V)
            {
                string text = Clipboard.GetText();
                if (string.IsNullOrWhiteSpace(text))
                    return;

                // 공백, 쉼표, 세미콜론 등을 기준으로 문자열 분리
                var tokens = Regex.Split(text.Trim(), @"[\s,;]+").Where(t => !string.IsNullOrEmpty(t)).ToList();
                if (tokens.Count == 0)
                    return;

                var startCell = grid.CurrentCell;
                if (startCell == null)
                    return;

                var selectedCells = grid.SelectedCells.Cast<DataGridViewCell>().ToList();

                // 경우 1: 클립보드에 단일 값이 있고 여러 셀이 선택된 경우 (선택된 셀을 동일한 값으로 일괄 채움)
                if (selectedCells.Count > 1 && tokens.Count == 1 && IsHexByte(tokens[0]))
                {
                    string v = tokens[0].PadLeft(2, '0').ToUpper();
                    foreach (var cell in selectedCells)
                    {
                        if (cell.ReadOnly)
                            continue;
                        cell.Value = v;
                    }
                }
                // 경우 2: 다중 값을 시작 셀부터 순서대로 붙여넣음
                else
                {
                    int startIdx = startCell.RowIndex * segmentsPerGrid + startCell.ColumnIndex;
                    int currentIdx = startIdx;
                    int tokenIdx = 0;

                    while (tokenIdx < tokens.Count && currentIdx < totalBytes)
                    {
                        int r = currentIdx / segmentsPerGrid;
                        int c = currentIdx % segmentsPerGrid;
                        var cell = grid.Rows[r].Cells[c];

                        if (!cell.ReadOnly && IsHexByte(tokens[tokenIdx]))
                        {
                            cell.Value = tokens[tokenIdx].PadLeft(2, '0').ToUpper();
                            tokenIdx++;
                        }
                        currentIdx++;
                    }
                }
                e.SuppressKeyPress = true;
            }
        }

        #endregion

        #region OK/Apply Result Handler

        /// <summary>
        /// [Write] (OK) 버튼 클릭 시, 그리드의 모든 셀 데이터를 검증하고 바이트 배열로 변환하여 ResultData에 저장합니다.
        /// </summary>
        private void BtnOK_Click(object sender, EventArgs e)
        {
            if (totalBytes <= 0 || segmentsPerGrid == 0)
            {
                ResultData = Array.Empty<byte>();
                return;
            }

            var byteList = new List<byte>(totalBytes);

            for (int idx = 0; idx < totalBytes; idx++)
            {
                int r = idx / segmentsPerGrid;
                int c = idx % segmentsPerGrid;
                var cell = grid.Rows[r].Cells[c];

                string valueStr = (cell.Value?.ToString() ?? "00").Trim();

                // 최종 변환 전 유효한 16진수 문자열인지 다시 한 번 확인
                if (!IsHexByte(valueStr))
                {
                    MessageBox.Show($"잘못된 입력값: '{valueStr}' (데이터 위치: {idx}번째)\n올바른 16진수를 입력해주세요.",
                                    "입력 오류", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                    // 에러가 났으므로 창이 닫히지 않도록 DialogResult 롤백
                    DialogResult = DialogResult.None;
                    return;
                }

                byteList.Add(Convert.ToByte(valueStr, 16));
            }

            ResultData = byteList.ToArray();
        }

        #endregion
    }
}