using BloodBankManagementSystem.Classes.Database;
using BloodBankManagementSystem.Classes.Helpers;
using System;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace BloodBankManagementSystem.Forms.Doctor
{
    public partial class DoctorTransfusionHistory : UserControl
    {
        private readonly Color brickRed = Color.FromArgb(120, 22, 27);
        private readonly Color lightBrickRed = Color.FromArgb(254, 226, 226);

        private DataTable allTransfusions;
        private DataTable filteredTransfusions;
        private int selectedColumnIndex = -1;
        private bool isLoading = false;

        private DataGridView dgv;
        private TextBox txtSearch;
        private ComboBox cmbStatus;
        private Label[] valLabels = new Label[4];

        private Panel[] cards = new Panel[4];
        private Panel cardsContainer;
        private Panel tableCard;

        private const int CARD_H = 105;

        public DoctorTransfusionHistory()
        {
            InitializeComponent();
            BuildUI();
            LoadDataFromDatabase();
        }

        // ── DATA ─────────────────────────────────────────────────────────────
        private async void LoadDataFromDatabase()
        {
            if (isLoading) return;
            isLoading = true;
            this.Cursor = Cursors.WaitCursor;

            try
            {
                await System.Threading.Tasks.Task.Delay(80);

                string query = @"SELECT 
                                    th.TransfusionID, th.PatientName, th.BloodGroup,
                                    th.Units, th.Hospital, th.TransfusionDate,
                                    th.Status, th.ReactionType, th.DoctorName,
                                    r.RequisitionNumber
                                FROM TransfusionHistory th
                                LEFT JOIN Requisitions r ON th.RequisitionID = r.RequisitionID
                                WHERE th.DoctorName = @DoctorName OR @DoctorName = ''
                                ORDER BY th.TransfusionDate DESC";

                var parameters = new System.Data.SqlClient.SqlParameter[]
                {
                    new System.Data.SqlClient.SqlParameter("@DoctorName", SessionManager.CurrentFullName ?? "")
                };

                allTransfusions = CommonDAL.ExecuteReader(query, parameters);

                if (allTransfusions == null || allTransfusions.Rows.Count == 0)
                    allTransfusions = CreateEmptyTable();

                filteredTransfusions = allTransfusions.Copy();
                UpdateCards();
                PopulateGrid();
            }
            catch (Exception ex)
            {
                Logger.Error("Error loading transfusion history", ex);
                MessageBox.Show($"Error loading data: {ex.Message}", "Database Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                allTransfusions = CreateEmptyTable();
                filteredTransfusions = allTransfusions.Copy();
                UpdateCards();
                PopulateGrid();
            }
            finally
            {
                this.Cursor = Cursors.Default;
                isLoading = false;
            }
        }

        private DataTable CreateEmptyTable()
        {
            var dt = new DataTable();
            dt.Columns.Add("TransfusionID", typeof(int));
            dt.Columns.Add("PatientName", typeof(string));
            dt.Columns.Add("BloodGroup", typeof(string));
            dt.Columns.Add("Units", typeof(int));
            dt.Columns.Add("Hospital", typeof(string));
            dt.Columns.Add("TransfusionDate", typeof(DateTime));
            dt.Columns.Add("Status", typeof(string));
            dt.Columns.Add("ReactionType", typeof(string));
            dt.Columns.Add("DoctorName", typeof(string));
            dt.Columns.Add("RequisitionNumber", typeof(string));
            return dt;
        }

        // ── BUILD UI ──────────────────────────────────────────────────────────
        private void BuildUI()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.FromArgb(245, 247, 250);
            this.Padding = new Padding(28, 20, 28, 20);

            // ── PAGE TITLE ────────────────────────────────────────────────────
            Panel titlePanel = new Panel();
            titlePanel.Dock = DockStyle.Top;
            titlePanel.Height = 56;
            titlePanel.BackColor = Color.Transparent;
            this.Controls.Add(titlePanel);

            Label pageTitle = new Label();
            pageTitle.Text = "🩸  Transfusion History";
            pageTitle.Font = new Font("Segoe UI", 20, FontStyle.Bold);
            pageTitle.ForeColor = brickRed;
            pageTitle.AutoSize = true;
            pageTitle.Location = new Point(0, 8);
            titlePanel.Controls.Add(pageTitle);

            // ── STATS CARDS ───────────────────────────────────────────────────
            cardsContainer = new Panel();
            cardsContainer.Dock = DockStyle.Top;
            cardsContainer.Height = CARD_H + 12;
            cardsContainer.BackColor = Color.Transparent;
            this.Controls.Add(cardsContainer);

            string[] cardTitles = { "Total Transfusions", "Completed", "Pending Review", "Total Units" };
            Color[] cardColors =
            {
                brickRed,
                Color.FromArgb(34, 197, 94),
                Color.FromArgb(245, 158, 11),
                Color.FromArgb(59, 130, 246)
            };

            for (int i = 0; i < 4; i++)
            {
                int ci = i;
                Panel card = new Panel();
                card.BackColor = Color.White;
                card.Paint += (s, pe) =>
                {
                    var c = (Panel)s;
                    pe.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    using (var b = new SolidBrush(cardColors[ci]))
                        pe.Graphics.FillRectangle(b, 0, 0, 5, c.Height);
                    using (var p = new Pen(Color.FromArgb(229, 231, 235), 1))
                        pe.Graphics.DrawRectangle(p, 0, 0, c.Width - 1, c.Height - 1);
                };
                cardsContainer.Controls.Add(card);
                cards[i] = card;

                // Number label — top 58%
                Label lblVal = new Label();
                lblVal.Text = "0";
                lblVal.Font = new Font("Segoe UI", 24, FontStyle.Bold);
                lblVal.ForeColor = cardColors[i];
                lblVal.TextAlign = ContentAlignment.BottomCenter;
                card.Controls.Add(lblVal);
                valLabels[i] = lblVal;

                // Title label — bottom 42%
                Label lblTit = new Label();
                lblTit.Text = cardTitles[i];
                lblTit.Font = new Font("Segoe UI", 10);
                lblTit.ForeColor = Color.FromArgb(107, 114, 128);
                lblTit.TextAlign = ContentAlignment.TopCenter;
                card.Controls.Add(lblTit);

                // Layout labels on card resize
                var lv = lblVal; var lt = lblTit;
                card.Resize += (s, e) =>
                {
                    var c = (Panel)s;
                    int w = c.ClientSize.Width;
                    int h = c.ClientSize.Height;
                    int sp = (int)(h * 0.56);
                    lv.SetBounds(5, 0, w - 10, sp);
                    lt.SetBounds(5, sp, w - 10, h - sp);
                };
            }

            cardsContainer.Resize += (s, e) =>
            {
                int gap = 14;
                int cw = (cardsContainer.ClientSize.Width - gap * 3) / 4;
                if (cw < 100) cw = 100;
                int ch = CARD_H;
                for (int i = 0; i < 4; i++)
                    cards[i].SetBounds(i * (cw + gap), 4, cw, ch);
            };

            // ── TABLE CARD ────────────────────────────────────────────────────
            tableCard = new Panel();
            tableCard.Dock = DockStyle.Fill;
            tableCard.BackColor = Color.White;
            tableCard.Padding = new Padding(0);
            tableCard.Paint += (s, pe) =>
            {
                using (var p = new Pen(Color.FromArgb(229, 231, 235), 1))
                    pe.Graphics.DrawRectangle(p, 0, 0, tableCard.Width - 1, tableCard.Height - 1);
            };
            this.Controls.Add(tableCard);

            // ── TABLE TOOLBAR ─────────────────────────────────────────────────
            Panel toolbar = new Panel();
            toolbar.Dock = DockStyle.Top;
            toolbar.Height = 58;
            toolbar.BackColor = Color.White;
            tableCard.Controls.Add(toolbar);

            Label lblTableTitle = new Label();
            lblTableTitle.Text = "Transfusion Records";
            lblTableTitle.Font = new Font("Segoe UI", 13, FontStyle.Bold);
            lblTableTitle.ForeColor = Color.FromArgb(30, 30, 30);
            lblTableTitle.AutoSize = true;
            lblTableTitle.Location = new Point(18, 16);
            toolbar.Controls.Add(lblTableTitle);

            // Refresh
            Button btnRefresh = new Button();
            btnRefresh.Text = "🔄 Refresh";
            btnRefresh.Font = new Font("Segoe UI Semibold", 9);
            btnRefresh.BackColor = Color.FromArgb(59, 130, 246);
            btnRefresh.ForeColor = Color.White;
            btnRefresh.FlatStyle = FlatStyle.Flat;
            btnRefresh.FlatAppearance.BorderSize = 0;
            btnRefresh.Size = new Size(95, 34);
            btnRefresh.Cursor = Cursors.Hand;
            btnRefresh.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnRefresh.Click += (s, e) => LoadDataFromDatabase();
            toolbar.Controls.Add(btnRefresh);

            // Export
            Button btnExport = new Button();
            btnExport.Text = "📥 Export";
            btnExport.Font = new Font("Segoe UI Semibold", 9);
            btnExport.BackColor = brickRed;
            btnExport.ForeColor = Color.White;
            btnExport.FlatStyle = FlatStyle.Flat;
            btnExport.FlatAppearance.BorderSize = 0;
            btnExport.Size = new Size(90, 34);
            btnExport.Cursor = Cursors.Hand;
            btnExport.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnExport.Click += BtnExport_Click;
            toolbar.Controls.Add(btnExport);

            // Status filter
            cmbStatus = new ComboBox();
            cmbStatus.Items.AddRange(new string[] { "All Status", "Completed", "Pending Review", "Cancelled" });
            cmbStatus.SelectedIndex = 0;
            cmbStatus.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbStatus.Font = new Font("Segoe UI", 9);
            cmbStatus.Size = new Size(135, 34);
            cmbStatus.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            cmbStatus.SelectedIndexChanged += (s, e) => FilterData();
            toolbar.Controls.Add(cmbStatus);

            // Search
            txtSearch = new TextBox();
            txtSearch.Text = "Search patient or record...";
            txtSearch.ForeColor = Color.Gray;
            txtSearch.Font = new Font("Segoe UI", 9);
            txtSearch.Size = new Size(200, 34);
            txtSearch.BorderStyle = BorderStyle.FixedSingle;
            txtSearch.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            txtSearch.GotFocus += (s, e) =>
            {
                if (txtSearch.Text.StartsWith("Search"))
                { txtSearch.Text = ""; txtSearch.ForeColor = Color.Black; }
            };
            txtSearch.LostFocus += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(txtSearch.Text))
                { txtSearch.Text = "Search patient or record..."; txtSearch.ForeColor = Color.Gray; }
            };
            txtSearch.TextChanged += (s, e) => FilterData();
            toolbar.Controls.Add(txtSearch);

            // Reposition toolbar controls right-to-left on resize
            toolbar.Resize += (s, e) =>
            {
                int right = toolbar.ClientSize.Width - 12;
                int y = 12;
                btnRefresh.Location = new Point(right - btnRefresh.Width, y);
                btnExport.Location = new Point(btnRefresh.Left - btnExport.Width - 8, y);
                cmbStatus.Location = new Point(btnExport.Left - cmbStatus.Width - 8, y);
                txtSearch.Location = new Point(cmbStatus.Left - txtSearch.Width - 8, y);
            };

            // Divider
            Panel divider = new Panel();
            divider.Dock = DockStyle.Top;
            divider.Height = 1;
            divider.BackColor = Color.FromArgb(229, 231, 235);
            tableCard.Controls.Add(divider);

            // ── DATAGRIDVIEW ──────────────────────────────────────────────────
            dgv = new DataGridView();
            dgv.Dock = DockStyle.Fill;
            dgv.BackgroundColor = Color.White;
            dgv.BorderStyle = BorderStyle.None;
            dgv.AllowUserToAddRows = false;
            dgv.AllowUserToDeleteRows = false;
            dgv.AllowUserToResizeRows = false;
            dgv.AllowUserToResizeColumns = false;
            dgv.ReadOnly = true;
            dgv.MultiSelect = false;
            dgv.RowHeadersVisible = false;
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgv.EnableHeadersVisualStyles = false;
            dgv.ColumnHeadersHeight = 44;
            dgv.RowTemplate.Height = 48;
            dgv.GridColor = Color.FromArgb(229, 231, 235);
            dgv.CellBorderStyle = DataGridViewCellBorderStyle.Single;

            dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(55, 65, 81);
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI Semibold", 10);
            dgv.DefaultCellStyle.Font = new Font("Segoe UI", 10);
            dgv.DefaultCellStyle.ForeColor = Color.FromArgb(55, 65, 81);
            dgv.DefaultCellStyle.SelectionBackColor = lightBrickRed;
            dgv.DefaultCellStyle.SelectionForeColor = brickRed;
            dgv.DefaultCellStyle.Padding = new Padding(4, 0, 4, 0);

            tableCard.Controls.Add(dgv);

            // Columns
            dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "id", HeaderText = "Record ID", FillWeight = 60, Visible = false });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "patient", HeaderText = "Patient Name", FillWeight = 130 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "blood", HeaderText = "Blood Group", FillWeight = 75 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "units", HeaderText = "Units", FillWeight = 50 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "hospital", HeaderText = "Hospital", FillWeight = 130 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "date", HeaderText = "Date", FillWeight = 90 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "status", HeaderText = "Status", FillWeight = 110 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "reaction", HeaderText = "Reaction", FillWeight = 85 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "view", HeaderText = "Details", FillWeight = 80 });

            dgv.CellPainting += Dgv_CellPainting;
            dgv.CellClick += Dgv_CellClick;
        }

        // ── CARDS UPDATE ──────────────────────────────────────────────────────
        private void UpdateCards()
        {
            if (allTransfusions == null) return;

            int total = allTransfusions.Rows.Count;
            int completed = 0, pending = 0, totalUnits = 0;

            foreach (DataRow row in allTransfusions.Rows)
            {
                string st = row["Status"]?.ToString() ?? "";
                if (st == "Completed") completed++;
                if (st == "Pending Review") pending++;
                if (row["Units"] != DBNull.Value)
                    totalUnits += Convert.ToInt32(row["Units"]);
            }

            if (valLabels[0] != null) valLabels[0].Text = total.ToString();
            if (valLabels[1] != null) valLabels[1].Text = completed.ToString();
            if (valLabels[2] != null) valLabels[2].Text = pending.ToString();
            if (valLabels[3] != null) valLabels[3].Text = totalUnits.ToString();
        }

        // ── POPULATE GRID ─────────────────────────────────────────────────────
        private void PopulateGrid()
        {
            if (dgv == null) return;
            dgv.Rows.Clear();

            if (filteredTransfusions == null || filteredTransfusions.Rows.Count == 0)
            {
                // Empty state — single centered message row, NO status badge, NO view button
                dgv.Rows.Add("__empty__", "No records found", "", "", "", "", "__empty__", "", "__empty__");
                dgv.Rows[0].DefaultCellStyle.ForeColor = Color.FromArgb(156, 163, 175);
                dgv.Rows[0].DefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Italic);
                dgv.Rows[0].DefaultCellStyle.BackColor = Color.White;
                dgv.Rows[0].DefaultCellStyle.SelectionBackColor = Color.White;
                dgv.Rows[0].DefaultCellStyle.SelectionForeColor = Color.FromArgb(156, 163, 175);
                return;
            }

            foreach (DataRow row in filteredTransfusions.Rows)
            {
                string date = row["TransfusionDate"] != DBNull.Value
                    ? Convert.ToDateTime(row["TransfusionDate"]).ToString("dd-MMM-yyyy")
                    : "-";

                dgv.Rows.Add(
                    row["TransfusionID"],
                    row["PatientName"] ?? "-",
                    row["BloodGroup"] ?? "-",
                    row["Units"] ?? 0,
                    row["Hospital"] ?? "-",
                    date,
                    row["Status"] ?? "Pending",
                    row["ReactionType"] ?? "None",
                    ""
                );
            }
        }

        private bool IsEmptyRow(int rowIndex)
        {
            if (rowIndex < 0 || rowIndex >= dgv.Rows.Count) return false;
            return dgv.Rows[rowIndex].Cells["id"].Value?.ToString() == "__empty__";
        }

        // ── FILTER ────────────────────────────────────────────────────────────
        private void FilterData()
        {
            if (allTransfusions == null) return;

            string search = txtSearch.Text.Trim().ToLower();
            if (search.StartsWith("search")) search = "";
            string statusF = cmbStatus.SelectedItem?.ToString() ?? "All Status";

            filteredTransfusions = allTransfusions.Clone();

            foreach (DataRow row in allTransfusions.Rows)
            {
                bool matchSearch = string.IsNullOrEmpty(search) ||
                    row["PatientName"].ToString().ToLower().Contains(search) ||
                    row["TransfusionID"].ToString().Contains(search) ||
                    row["Hospital"].ToString().ToLower().Contains(search);

                bool matchStatus = statusF == "All Status" ||
                    row["Status"].ToString() == statusF;

                if (matchSearch && matchStatus)
                    filteredTransfusions.ImportRow(row);
            }

            PopulateGrid();
        }

        // ── EXPORT ────────────────────────────────────────────────────────────
        private void BtnExport_Click(object sender, EventArgs e)
        {
            if (filteredTransfusions == null || filteredTransfusions.Rows.Count == 0)
            {
                MessageBox.Show("No data to export.", "Export",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (SaveFileDialog sfd = new SaveFileDialog
            {
                Filter = "CSV Files (*.csv)|*.csv|Excel Files (*.xlsx)|*.xlsx",
                DefaultExt = "csv",
                FileName = $"TransfusionHistory_{DateTime.Now:yyyyMMdd_HHmmss}"
            })
            {
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        if (sfd.FilterIndex == 2)
                            ExportHelper.ExportToExcel(filteredTransfusions, sfd.FileName);
                        else
                            ExportHelper.ExportToCSV(filteredTransfusions, sfd.FileName);

                        MessageBox.Show($"Exported successfully!\n\n{sfd.FileName}",
                            "Export Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Export failed: {ex.Message}", "Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        // ── CELL PAINTING ─────────────────────────────────────────────────────
        private void Dgv_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            // Header
            if (e.RowIndex == -1 && e.ColumnIndex >= 0)
            {
                bool sel = e.ColumnIndex == selectedColumnIndex;
                using (var br = new SolidBrush(sel ? lightBrickRed : Color.FromArgb(248, 250, 252)))
                    e.Graphics.FillRectangle(br, e.CellBounds);
                TextRenderer.DrawText(e.Graphics, e.FormattedValue?.ToString(),
                    new Font("Segoe UI Semibold", 10), e.CellBounds,
                    sel ? brickRed : Color.FromArgb(55, 65, 81),
                    TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter);
                e.Handled = true;
                return;
            }

            if (e.RowIndex < 0) return;

            // Empty row — paint white, skip badges & buttons
            if (IsEmptyRow(e.RowIndex))
            {
                using (var br = new SolidBrush(Color.White))
                    e.Graphics.FillRectangle(br, e.CellBounds);

                // Only draw text in "patient" column (index 1)
                if (e.ColumnIndex == 1)
                {
                    TextRenderer.DrawText(e.Graphics, "No records found",
                        new Font("Segoe UI", 11, FontStyle.Italic),
                        e.CellBounds, Color.FromArgb(180, 180, 180),
                        TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.LeftAndRightPadding);
                }
                e.Handled = true;
                return;
            }

            // Alternating row
            bool isAlt = e.RowIndex % 2 != 0;
            Color rowBg = dgv.Rows[e.RowIndex].Selected
                ? lightBrickRed
                : (isAlt ? Color.FromArgb(250, 250, 252) : Color.White);

            // ── Status badge (col 6) ─────────────────────────────────────────
            if (e.ColumnIndex == 6)
            {
                using (var br = new SolidBrush(rowBg))
                    e.Graphics.FillRectangle(br, e.CellBounds);

                string st = e.Value?.ToString() ?? "";
                Color fc = st == "Completed" ? Color.FromArgb(22, 163, 74) :
                           st == "Pending Review" ? Color.FromArgb(245, 158, 11) :
                           st == "Cancelled" ? Color.FromArgb(220, 38, 38) :
                                                    Color.FromArgb(107, 114, 128);

                Size ts = TextRenderer.MeasureText(st, new Font("Segoe UI Semibold", 9));
                var badge = new Rectangle(
                    e.CellBounds.X + 8,
                    e.CellBounds.Y + (e.CellBounds.Height - 22) / 2,
                    ts.Width + 16, 22);
                using (var bb = new SolidBrush(Color.FromArgb(30, fc.R, fc.G, fc.B)))
                    e.Graphics.FillRectangle(bb, badge);
                TextRenderer.DrawText(e.Graphics, st, new Font("Segoe UI Semibold", 9),
                    badge, fc, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                e.Handled = true;
                return;
            }

            // ── Reaction (col 7) ─────────────────────────────────────────────
            if (e.ColumnIndex == 7)
            {
                using (var br = new SolidBrush(rowBg))
                    e.Graphics.FillRectangle(br, e.CellBounds);

                string rn = e.Value?.ToString() ?? "";
                Color fc = rn == "None" ? Color.FromArgb(22, 163, 74) :
                           rn == "Mild" ? Color.FromArgb(245, 158, 11) :
                           rn == "Severe" ? Color.FromArgb(220, 38, 38) :
                                            Color.FromArgb(107, 114, 128);
                TextRenderer.DrawText(e.Graphics, rn, new Font("Segoe UI Semibold", 9),
                    e.CellBounds, fc,
                    TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.LeftAndRightPadding);
                e.Handled = true;
                return;
            }

            // ── View Details button (col 8) ───────────────────────────────────
            if (e.ColumnIndex == 8)
            {
                using (var br = new SolidBrush(rowBg))
                    e.Graphics.FillRectangle(br, e.CellBounds);

                const int bW = 88, bH = 28;
                var rect = new Rectangle(
                    e.CellBounds.Left + (e.CellBounds.Width - bW) / 2,
                    e.CellBounds.Top + (e.CellBounds.Height - bH) / 2,
                    bW, bH);

                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var gp = RoundedPath(rect, 4))
                using (var sb = new SolidBrush(brickRed))
                    e.Graphics.FillPath(sb, gp);

                TextRenderer.DrawText(e.Graphics, "View Details",
                    new Font("Segoe UI Semibold", 8), rect, Color.White,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                e.Handled = true;
            }
        }

        private GraphicsPath RoundedPath(Rectangle r, int radius)
        {
            int d = radius * 2;
            var gp = new GraphicsPath();
            gp.AddArc(r.Left, r.Top, d, d, 180, 90);
            gp.AddArc(r.Right - d, r.Top, d, d, 270, 90);
            gp.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            gp.AddArc(r.Left, r.Bottom - d, d, d, 90, 90);
            gp.CloseFigure();
            return gp;
        }

        // ── CELL CLICK ────────────────────────────────────────────────────────
        private void Dgv_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;

            selectedColumnIndex = e.ColumnIndex;
            dgv.Invalidate();

            if (IsEmptyRow(e.RowIndex)) return;

            if (e.ColumnIndex == 8)
            {
                var cells = dgv.Rows[e.RowIndex].Cells;
                MessageBox.Show(
                    $"📋 TRANSFUSION DETAILS\n\n" +
                    $"Record ID   : {cells["id"].Value}\n" +
                    $"Patient     : {cells["patient"].Value}\n" +
                    $"Blood Group : {cells["blood"].Value}\n" +
                    $"Units       : {cells["units"].Value}\n" +
                    $"Hospital    : {cells["hospital"].Value}\n" +
                    $"Date        : {cells["date"].Value}\n" +
                    $"Status      : {cells["status"].Value}\n" +
                    $"Reaction    : {cells["reaction"].Value}\n" +
                    $"Doctor      : {SessionManager.CurrentFullName}",
                    "Transfusion Details",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}