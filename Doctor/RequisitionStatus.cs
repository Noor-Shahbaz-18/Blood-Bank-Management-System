using System;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using BloodBankManagementSystem.Classes.Database;
using BloodBankManagementSystem.Classes.Helpers;

namespace BloodBankManagementSystem.Forms.Doctor
{
    public partial class RequisitionStatus : UserControl
    {
        // ── Colours ──────────────────────────────────────────────────────────────
        private readonly Color brickRed = Color.FromArgb(120, 22, 27);
        private readonly Color lightBrickRed = Color.FromArgb(254, 226, 226);

        // ── State ────────────────────────────────────────────────────────────────
        private DataTable allRequisitions;
        private DataTable filteredRequisitions;
        private int selectedColumnIndex = -1;

        // ── Controls ─────────────────────────────────────────────────────────────
        private DataGridView dgv;
        private TextBox txtSearch;
        private ComboBox cmbStatusFilter;
        private Label[] valLabels = new Label[4];
        private Label[] titLabels = new Label[4];   // ← kept for Reflow
        private Panel tablePanel;
        private FlowLayoutPanel cardsRow;                 // ← stored for Reflow
        private FlowLayoutPanel stack;                    // ← stored for Reflow

        // card metrics (constant)
        private const int CARD_H = 130;   // card pixel height
        private const int ROW_H = CARD_H + 8;   // cardsRow height

        // ── Constructor ──────────────────────────────────────────────────────────
        public RequisitionStatus()
        {
            InitializeComponent();
            BuildUI();
            LoadData();
        }

        // =========================================================
        // BUILD UI
        // =========================================================
        private void BuildUI()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.FromArgb(245, 247, 250);

            // ── Outer scroll wrapper ──────────────────────────────────────────────
            Panel scroll = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = Color.FromArgb(245, 247, 250),
                Padding = new Padding(25, 15, 25, 20)
            };
            this.Controls.Add(scroll);

            // ── Inner stack ───────────────────────────────────────────────────────
            stack = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                BackColor = Color.Transparent,
                Padding = new Padding(0)
            };
            scroll.Controls.Add(stack);

            // ── Page title ────────────────────────────────────────────────────────
            Label pageTitle = new Label
            {
                Text = "📋  Requisition Status",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = brickRed,
                AutoSize = false,
                Height = 48,
                Margin = new Padding(0, 0, 0, 10),
                TextAlign = ContentAlignment.MiddleLeft
            };
            stack.Controls.Add(pageTitle);

            // ── Cards row ─────────────────────────────────────────────────────────
            string[] titles = { "Total Requisitions", "Pending", "Approved", "Cross Matching" };
            Color[] colors =
            {
                brickRed,
                Color.FromArgb(245, 158,  11),
                Color.FromArgb( 34, 197,  94),
                Color.FromArgb( 59, 130, 246)
            };

            cardsRow = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                AutoSize = false,
                Height = ROW_H,
                Margin = new Padding(0, 0, 0, 14),
                BackColor = Color.Transparent
            };
            stack.Controls.Add(cardsRow);

            for (int i = 0; i < 4; i++)
            {
                int ci = i;

                Panel card = new Panel
                {
                    Size = new Size(200, CARD_H),
                    BackColor = Color.White,
                    Margin = new Padding(0, 0, 14, 0)
                };

                card.Paint += (s, e) =>
                {
                    e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    using (SolidBrush b = new SolidBrush(colors[ci]))
                        e.Graphics.FillRectangle(b, 0, 0, 5, card.Height);
                    using (Pen p = new Pen(Color.FromArgb(220, 220, 220), 1))
                        e.Graphics.DrawRectangle(p, 0, 0, card.Width - 1, card.Height - 1);
                };

                // ── big number (top 65 px) ────────────────────────────────────────
                Label valLbl = new Label
                {
                    Text = "0",
                    Font = new Font("Segoe UI", 28, FontStyle.Bold),
                    ForeColor = colors[i],
                    AutoSize = false,
                    Size = new Size(190, 65),
                    Location = new Point(5, 8),
                    TextAlign = ContentAlignment.MiddleCenter
                };

                // ── title (below number, 55 px tall – room for 2 lines) ───────────
                Label titLbl = new Label
                {
                    Text = titles[i],
                    Font = new Font("Segoe UI", 10),
                    ForeColor = Color.FromArgb(75, 85, 99),
                    AutoSize = false,
                    Size = new Size(190, 55),
                    Location = new Point(5, 73),
                    TextAlign = ContentAlignment.TopCenter
                };

                card.Controls.Add(valLbl);
                card.Controls.Add(titLbl);
                cardsRow.Controls.Add(card);

                valLabels[i] = valLbl;
                titLabels[i] = titLbl;
            }

            // ── Table white card ──────────────────────────────────────────────────
            tablePanel = new Panel
            {
                BackColor = Color.White,
                AutoSize = false,
                Height = 480,
                Margin = new Padding(0)
            };
            tablePanel.Paint += (s, e) =>
            {
                using (Pen p = new Pen(Color.FromArgb(220, 220, 220), 1))
                    e.Graphics.DrawRectangle(p, 0, 0, tablePanel.Width - 1, tablePanel.Height - 1);
            };
            stack.Controls.Add(tablePanel);

            // ─── Table header bar ─────────────────────────────────────────────────
            Label tblTitle = new Label
            {
                Text = "My Requisitions",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 30, 30),
                AutoSize = false,
                Size = new Size(260, 40),
                Location = new Point(20, 14),
                TextAlign = ContentAlignment.MiddleLeft
            };
            tablePanel.Controls.Add(tblTitle);

            // Refresh button
            Button btnRefresh = new Button
            {
                Text = "🔄  Refresh",
                Size = new Size(105, 36),
                BackColor = brickRed,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI Semibold", 9),
                Cursor = Cursors.Hand,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            btnRefresh.FlatAppearance.BorderSize = 0;
            btnRefresh.Click += (s, e) => LoadData();
            tablePanel.Controls.Add(btnRefresh);

            // Status filter combo
            cmbStatusFilter = new ComboBox
            {
                Font = new Font("Segoe UI", 10),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Size = new Size(130, 36),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            cmbStatusFilter.Items.AddRange(new string[]
                { "All", "Pending", "Approved", "Rejected", "Cross Matching", "Completed" });
            cmbStatusFilter.SelectedIndex = 0;
            cmbStatusFilter.SelectedIndexChanged += (s, e) => FilterData();
            tablePanel.Controls.Add(cmbStatusFilter);

            // Status label
            Label lblSt = new Label
            {
                Text = "Status:",
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.FromArgb(55, 65, 81),
                AutoSize = true,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            tablePanel.Controls.Add(lblSt);

            // Search box
            txtSearch = new TextBox
            {
                Text = "Search patient or requisition...",
                ForeColor = Color.Gray,
                Font = new Font("Segoe UI", 10),
                Size = new Size(210, 36),
                BorderStyle = BorderStyle.FixedSingle,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            txtSearch.TextChanged += (s, e) => FilterData();
            txtSearch.GotFocus += (s, e) =>
            {
                if (txtSearch.Text.StartsWith("Search"))
                { txtSearch.Text = ""; txtSearch.ForeColor = Color.Black; }
            };
            txtSearch.LostFocus += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(txtSearch.Text))
                { txtSearch.Text = "Search patient or requisition..."; txtSearch.ForeColor = Color.Gray; }
            };
            tablePanel.Controls.Add(txtSearch);

            // Divider
            Panel hdiv = new Panel
            {
                BackColor = Color.FromArgb(230, 230, 230),
                Height = 1,
                Location = new Point(0, 62)
            };
            tablePanel.Controls.Add(hdiv);

            // ─── DataGridView ─────────────────────────────────────────────────────
            dgv = new DataGridView
            {
                Location = new Point(0, 63),
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                ReadOnly = true,
                MultiSelect = false,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                EnableHeadersVisualStyles = false,
                ColumnHeadersHeight = 44,
                GridColor = Color.FromArgb(229, 231, 235),
                Anchor = AnchorStyles.Top | AnchorStyles.Left
                                        | AnchorStyles.Right | AnchorStyles.Bottom
            };
            dgv.RowTemplate.Height = 46;
            dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(55, 65, 81);
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI Semibold", 10);
            dgv.DefaultCellStyle.Font = new Font("Segoe UI", 10);
            dgv.DefaultCellStyle.ForeColor = Color.FromArgb(55, 65, 81);
            dgv.DefaultCellStyle.SelectionBackColor = lightBrickRed;
            dgv.DefaultCellStyle.SelectionForeColor = brickRed;

            dgv.Columns.Add("id", "Req ID");
            dgv.Columns.Add("number", "Requisition No");
            dgv.Columns.Add("patient", "Patient Name");
            dgv.Columns.Add("blood", "Blood Group");
            dgv.Columns.Add("units", "Units");
            dgv.Columns.Add("hospital", "Hospital");
            dgv.Columns.Add("date", "Date");
            dgv.Columns.Add("status", "Status");
            dgv.Columns.Add("urgency", "Urgency");
            dgv.Columns.Add("actions", "Actions");

            dgv.Columns["id"].FillWeight = 55;
            dgv.Columns["number"].FillWeight = 105;
            dgv.Columns["patient"].FillWeight = 120;
            dgv.Columns["blood"].FillWeight = 75;
            dgv.Columns["units"].FillWeight = 50;
            dgv.Columns["hospital"].FillWeight = 105;
            dgv.Columns["date"].FillWeight = 90;
            dgv.Columns["status"].FillWeight = 100;
            dgv.Columns["urgency"].FillWeight = 85;
            dgv.Columns["actions"].FillWeight = 85;

            dgv.CellPainting += Dgv_CellPainting;
            dgv.CellClick += Dgv_CellClick;

            tablePanel.Controls.Add(dgv);

            // ─── Reflow on resize ────────────────────────────────────────────────
            scroll.Resize += (s, e) => Reflow(scroll, pageTitle, btnRefresh, hdiv, lblSt);
            this.Load += (s, e) => Reflow(scroll, pageTitle, btnRefresh, hdiv, lblSt);
        }

        // =========================================================
        // REFLOW  – called on load & every resize
        // =========================================================
        private void Reflow(Panel scroll, Label pageTitle,
                            Button btnRefresh, Panel hdiv, Label lblSt)
        {
            int usable = scroll.ClientSize.Width - scroll.Padding.Horizontal;
            if (usable < 100) return;

            stack.Width = usable;
            pageTitle.Width = usable;

            // ── Cards row ─────────────────────────────────────────────────────────
            cardsRow.Width = usable;

            const int gap = 14;
            int cardW = (usable - gap * 3) / 4;
            cardW = Math.Max(cardW, 140);

            for (int i = 0; i < cardsRow.Controls.Count; i++)
            {
                Panel c = (Panel)cardsRow.Controls[i];
                c.Size = new Size(cardW, CARD_H);
                c.Margin = new Padding(0, 0, i < 3 ? gap : 0, 0);

                // stretch inner labels
                foreach (Control ctrl in c.Controls)
                    ctrl.Width = cardW - 10;
            }

            // ── Table panel ───────────────────────────────────────────────────────
            tablePanel.Width = usable;
            tablePanel.Height = Math.Max(380,
                scroll.ClientSize.Height
                - 48 - 10       // page title + margin
                - ROW_H - 14   // cards row + margin
                - 20);          // bottom pad

            // header toolbar – right-align
            int right = tablePanel.Width - 15;
            btnRefresh.Location = new Point(right - btnRefresh.Width, 14);
            cmbStatusFilter.Location = new Point(btnRefresh.Left - cmbStatusFilter.Width - 8, 14);
            lblSt.Location = new Point(cmbStatusFilter.Left - lblSt.Width - 4, 20);
            txtSearch.Location = new Point(lblSt.Left - txtSearch.Width - 12, 14);

            // divider & grid fill table panel
            hdiv.Width = tablePanel.Width;
            dgv.Width = tablePanel.Width;
            dgv.Height = tablePanel.Height - 63;
        }

        // =========================================================
        // DATA
        // =========================================================
        private void LoadData()
        {
            try
            {
                allRequisitions = RequisitionDAL.GetRequisitionsByDoctor(SessionManager.CurrentUserID)
                                  ?? EmptyTable();
                if (allRequisitions.Rows.Count == 0) allRequisitions = EmptyTable();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Database Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                allRequisitions = EmptyTable();
            }

            filteredRequisitions = allRequisitions.Copy();
            PopulateGrid();
            UpdateCards();
        }

        private DataTable EmptyTable()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("RequisitionID", typeof(int));
            dt.Columns.Add("RequisitionNumber", typeof(string));
            dt.Columns.Add("PatientName", typeof(string));
            dt.Columns.Add("BloodGroup", typeof(string));
            dt.Columns.Add("UnitsNeeded", typeof(int));
            dt.Columns.Add("RequestDate", typeof(DateTime));
            dt.Columns.Add("Status", typeof(string));
            dt.Columns.Add("Urgency", typeof(string));
            dt.Columns.Add("Hospital", typeof(string));
            return dt;
        }

        private void UpdateCards()
        {
            int total = allRequisitions.Rows.Count,
                pending = 0, approved = 0, cross = 0;

            foreach (DataRow r in allRequisitions.Rows)
            {
                string st = r["Status"]?.ToString() ?? "";
                if (st == "Pending") pending++;
                else if (st == "Approved") approved++;
                else if (st == "Cross Matching") cross++;
            }

            if (valLabels[0] != null) valLabels[0].Text = total.ToString();
            if (valLabels[1] != null) valLabels[1].Text = pending.ToString();
            if (valLabels[2] != null) valLabels[2].Text = approved.ToString();
            if (valLabels[3] != null) valLabels[3].Text = cross.ToString();
        }

        private void PopulateGrid()
        {
            dgv.Rows.Clear();
            if (filteredRequisitions == null) return;

            foreach (DataRow r in filteredRequisitions.Rows)
            {
                dgv.Rows.Add(
                    r["RequisitionID"],
                    r["RequisitionNumber"],
                    r["PatientName"],
                    r["BloodGroup"],
                    r["UnitsNeeded"],
                    r["Hospital"],
                    Convert.ToDateTime(r["RequestDate"]).ToString("dd-MMM-yyyy"),
                    r["Status"],
                    r["Urgency"],
                    "");
            }
        }

        private void FilterData()
        {
            if (allRequisitions == null) return;

            string search = txtSearch.Text.Trim().ToLower();
            if (search.StartsWith("search")) search = "";
            string statusF = cmbStatusFilter.SelectedItem?.ToString() ?? "All";

            filteredRequisitions = allRequisitions.Clone();
            foreach (DataRow r in allRequisitions.Rows)
            {
                bool ms = string.IsNullOrEmpty(search)
                    || r["PatientName"].ToString().ToLower().Contains(search)
                    || r["RequisitionNumber"].ToString().ToLower().Contains(search);
                bool mf = statusF == "All" || r["Status"].ToString() == statusF;
                if (ms && mf) filteredRequisitions.ImportRow(r);
            }
            PopulateGrid();
        }

        // =========================================================
        // CELL PAINTING
        // =========================================================
        private void Dgv_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            // ── Column header ─────────────────────────────────────────────────────
            if (e.RowIndex == -1 && e.ColumnIndex >= 0)
            {
                bool sel = e.ColumnIndex == selectedColumnIndex;
                e.PaintBackground(e.CellBounds, true);
                using (SolidBrush b = new SolidBrush(sel ? lightBrickRed : Color.FromArgb(248, 250, 252)))
                    e.Graphics.FillRectangle(b, e.CellBounds);
                TextRenderer.DrawText(e.Graphics, e.FormattedValue?.ToString(),
                    new Font("Segoe UI Semibold", 10), e.CellBounds,
                    sel ? brickRed : Color.FromArgb(55, 65, 81),
                    TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.LeftAndRightPadding);
                e.Handled = true;
                return;
            }

            if (e.RowIndex < 0) return;

            // ── Status pill (col 7) ───────────────────────────────────────────────
            if (e.ColumnIndex == 7)
            {
                e.PaintBackground(e.CellBounds, true);
                string st = e.Value?.ToString() ?? "";
                Color fc = st == "Approved" ? Color.FromArgb(34, 197, 94)
                          : st == "Pending" ? Color.FromArgb(245, 158, 11)
                          : st == "Cross Matching" ? Color.FromArgb(59, 130, 246)
                          : st == "Rejected" ? Color.FromArgb(220, 38, 38)
                          : Color.FromArgb(55, 65, 81);

                Rectangle pr = new Rectangle(
                    e.CellBounds.Left + 6,
                    e.CellBounds.Top + 9,
                    e.CellBounds.Width - 12,
                    e.CellBounds.Height - 18);

                using (SolidBrush pb = new SolidBrush(Color.FromArgb(30, fc)))
                    e.Graphics.FillRectangle(pb, pr);
                TextRenderer.DrawText(e.Graphics, st,
                    new Font("Segoe UI Semibold", 9), pr, fc,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                e.Handled = true;
                return;
            }

            // ── Urgency (col 8) ───────────────────────────────────────────────────
            if (e.ColumnIndex == 8)
            {
                e.PaintBackground(e.CellBounds, true);
                string ug = e.Value?.ToString() ?? "";
                Color fc = ug == "Emergency" ? Color.FromArgb(220, 38, 38)
                          : ug == "Urgent" ? Color.FromArgb(245, 158, 11)
                          : Color.FromArgb(34, 197, 94);
                TextRenderer.DrawText(e.Graphics, ug,
                    new Font("Segoe UI Semibold", 9), e.CellBounds, fc,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.LeftAndRightPadding);
                e.Handled = true;
                return;
            }

            // ── Actions button (col 9) ────────────────────────────────────────────
            if (e.ColumnIndex == 9)
            {
                e.PaintBackground(e.CellBounds, true);
                const int bW = 90, bH = 28;
                Rectangle br = new Rectangle(
                    e.CellBounds.Left + (e.CellBounds.Width - bW) / 2,
                    e.CellBounds.Top + (e.CellBounds.Height - bH) / 2,
                    bW, bH);

                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (GraphicsPath gp = RoundedPath(br, 4))
                using (SolidBrush sb = new SolidBrush(brickRed))
                    e.Graphics.FillPath(sb, gp);

                TextRenderer.DrawText(e.Graphics, "View Details",
                    new Font("Segoe UI Semibold", 8), br, Color.White,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                e.Handled = true;
                return;
            }
        }

        private GraphicsPath RoundedPath(Rectangle r, int radius)
        {
            int d = radius * 2;
            GraphicsPath gp = new GraphicsPath();
            gp.AddArc(r.Left, r.Top, d, d, 180, 90);
            gp.AddArc(r.Right - d, r.Top, d, d, 270, 90);
            gp.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            gp.AddArc(r.Left, r.Bottom - d, d, d, 90, 90);
            gp.CloseFigure();
            return gp;
        }

        // =========================================================
        // CELL CLICK
        // =========================================================
        private void Dgv_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                selectedColumnIndex = e.ColumnIndex;
                dgv.Invalidate();
            }

            if (e.ColumnIndex == 9 && e.RowIndex >= 0)
            {
                var cells = dgv.Rows[e.RowIndex].Cells;
                string details =
                    $"Requisition : {cells["number"].Value}\n" +
                    $"Patient     : {cells["patient"].Value}\n" +
                    $"Blood Group : {cells["blood"].Value}\n" +
                    $"Units       : {cells["units"].Value}\n" +
                    $"Hospital    : {cells["hospital"].Value}\n" +
                    $"Status      : {cells["status"].Value}\n" +
                    $"Urgency     : {cells["urgency"].Value}";

                MessageBox.Show(details, "Requisition Details",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}