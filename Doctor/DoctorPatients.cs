using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using BloodBankManagementSystem.Classes.Database;
using BloodBankManagementSystem.Classes.Helpers;

namespace BloodBankManagementSystem.Forms.Doctor
{
    public partial class DoctorPatients : UserControl
    {
        private readonly Color brickRed = Color.FromArgb(120, 22, 27);
        private readonly Color lightBrickRed = Color.FromArgb(254, 226, 226);
        private DataTable allPatients;
        private DataTable filteredPatients;
        private DataGridView dgv;
        private TextBox txtSearch;
        private ComboBox cmbBloodFilter;
        private int selectedColumnIndex = -1;
        private Panel[] cards;

        public DoctorPatients()
        {
            InitializeComponent();
            BuildUI();
            LoadPatientsFromDatabase();
        }

        private void LoadPatientsFromDatabase()
        {
            try
            {
                // Sirf active patients jo doctor ne treat kiye hain (requisitions ke through)
                string query = @"
                    SELECT DISTINCT 
                        p.PatientID, 
                        p.FullName, 
                        p.Age, 
                        p.Gender, 
                        p.BloodGroup, 
                        p.Phone, 
                        p.Email,
                        COALESCE(p.Disease, 'Not Specified') AS Disease,
                        COALESCE(p.Status, 'Active') AS Status
                    FROM Patients p
                    INNER JOIN Requisitions r ON r.PatientName = p.FullName
                    WHERE r.DoctorID = @DoctorID
                    ORDER BY p.PatientID DESC";

                var parameters = new System.Data.SqlClient.SqlParameter[]
                {
                    new System.Data.SqlClient.SqlParameter("@DoctorID", SessionManager.CurrentUserID)
                };

                allPatients = CommonDAL.ExecuteReader(query, parameters);

                if (allPatients == null || allPatients.Rows.Count == 0)
                {
                    MessageBox.Show("No patients found.\n\nWhen you create requisitions for patients, they will appear here.",
                        "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    allPatients = new DataTable();
                    allPatients.Columns.Add("PatientID", typeof(int));
                    allPatients.Columns.Add("FullName", typeof(string));
                    allPatients.Columns.Add("Age", typeof(int));
                    allPatients.Columns.Add("Gender", typeof(string));
                    allPatients.Columns.Add("BloodGroup", typeof(string));
                    allPatients.Columns.Add("Phone", typeof(string));
                    allPatients.Columns.Add("Email", typeof(string));
                    allPatients.Columns.Add("Disease", typeof(string));
                    allPatients.Columns.Add("Status", typeof(string));
                }

                filteredPatients = allPatients.Copy();
                LoadTableData();
                UpdateSummaryCards();
            }
            catch (Exception ex)
            {
                Logger.Error("Error loading patients", ex);
                MessageBox.Show($"Error loading patients: {ex.Message}",
                    "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                allPatients = new DataTable();
                allPatients.Columns.Add("PatientID", typeof(int));
                allPatients.Columns.Add("FullName", typeof(string));
                allPatients.Columns.Add("Age", typeof(int));
                allPatients.Columns.Add("Gender", typeof(string));
                allPatients.Columns.Add("BloodGroup", typeof(string));
                allPatients.Columns.Add("Phone", typeof(string));
                allPatients.Columns.Add("Email", typeof(string));
                allPatients.Columns.Add("Disease", typeof(string));
                allPatients.Columns.Add("Status", typeof(string));

                filteredPatients = allPatients.Copy();
                LoadTableData();
                UpdateSummaryCards();
            }
        }

        private void BuildUI()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.FromArgb(245, 247, 250);
            this.Padding = new Padding(30, 25, 30, 25);

            // ── HEADER ───────────────────────────────────────────────────────
            Panel headerPanel = new Panel();
            headerPanel.Dock = DockStyle.Top;
            headerPanel.Height = 60;
            headerPanel.BackColor = Color.Transparent;
            this.Controls.Add(headerPanel);

            Label lblTitle = new Label();
            lblTitle.Text = "Patients";
            lblTitle.Font = new Font("Segoe UI", 22, FontStyle.Bold);
            lblTitle.ForeColor = Color.FromArgb(31, 41, 55);
            lblTitle.AutoSize = true;
            lblTitle.Location = new Point(0, 0);
            headerPanel.Controls.Add(lblTitle);

            Label lblSub = new Label();
            lblSub.Text = "Manage and view all registered patients.";
            lblSub.Font = new Font("Segoe UI", 10);
            lblSub.ForeColor = Color.Gray;
            lblSub.AutoSize = true;
            lblSub.Location = new Point(2, 36);
            headerPanel.Controls.Add(lblSub);

            // ── STATS CARDS ───────────────────────────────────────────────────
            Panel cardsContainer = new Panel();
            cardsContainer.Dock = DockStyle.Top;
            cardsContainer.Height = 115;
            cardsContainer.BackColor = Color.Transparent;
            cardsContainer.Padding = new Padding(0, 10, 0, 5);
            this.Controls.Add(cardsContainer);

            cards = new Panel[4];
            for (int i = 0; i < 4; i++)
            {
                Panel card = new Panel();
                card.BackColor = Color.White;
                card.Paint += (s, pe) =>
                {
                    var p = (Panel)s;
                    ControlPaint.DrawBorder(pe.Graphics, p.ClientRectangle,
                        Color.FromArgb(229, 231, 235), 1, ButtonBorderStyle.Solid,
                        Color.FromArgb(229, 231, 235), 1, ButtonBorderStyle.Solid,
                        Color.FromArgb(229, 231, 235), 1, ButtonBorderStyle.Solid,
                        Color.FromArgb(229, 231, 235), 1, ButtonBorderStyle.Solid);
                };
                cardsContainer.Controls.Add(card);
                cards[i] = card;
            }

            // ── TABLE PANEL ───────────────────────────────────────────────────
            Panel tablePanel = new Panel();
            tablePanel.Dock = DockStyle.Fill;
            tablePanel.BackColor = Color.White;
            tablePanel.Padding = new Padding(20, 15, 20, 15);
            this.Controls.Add(tablePanel);

            // Toolbar inside tablePanel
            Panel toolbar = new Panel();
            toolbar.Dock = DockStyle.Top;
            toolbar.Height = 52;
            toolbar.BackColor = Color.White;
            tablePanel.Controls.Add(toolbar);

            // Refresh Button
            Button btnRefresh = new Button();
            btnRefresh.Text = "🔄 Refresh";
            btnRefresh.Font = new Font("Segoe UI Semibold", 9);
            btnRefresh.BackColor = brickRed;
            btnRefresh.ForeColor = Color.White;
            btnRefresh.FlatStyle = FlatStyle.Flat;
            btnRefresh.FlatAppearance.BorderSize = 0;
            btnRefresh.Size = new Size(105, 36);
            btnRefresh.Location = new Point(0, 8);
            btnRefresh.Cursor = Cursors.Hand;
            btnRefresh.Click += (s, e) => LoadPatientsFromDatabase();
            toolbar.Controls.Add(btnRefresh);

            // Search box
            txtSearch = new TextBox();
            txtSearch.Text = "Search patients...";
            txtSearch.ForeColor = Color.Gray;
            txtSearch.Font = new Font("Segoe UI", 10);
            txtSearch.Size = new Size(260, 36);
            txtSearch.BorderStyle = BorderStyle.FixedSingle;
            txtSearch.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            txtSearch.TextChanged += (s, e) => FilterData();
            txtSearch.GotFocus += (s, e) =>
            {
                if (txtSearch.Text == "Search patients...")
                { txtSearch.Text = ""; txtSearch.ForeColor = Color.Black; }
            };
            txtSearch.LostFocus += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(txtSearch.Text))
                { txtSearch.Text = "Search patients..."; txtSearch.ForeColor = Color.Gray; }
            };
            toolbar.Controls.Add(txtSearch);

            // Blood Group Filter
            cmbBloodFilter = new ComboBox();
            cmbBloodFilter.Items.AddRange(new string[]
                { "All Blood Groups", "A+", "A-", "B+", "B-", "AB+", "AB-", "O+", "O-" });
            cmbBloodFilter.SelectedIndex = 0;
            cmbBloodFilter.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbBloodFilter.Font = new Font("Segoe UI", 10);
            cmbBloodFilter.Size = new Size(165, 36);
            cmbBloodFilter.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            cmbBloodFilter.SelectedIndexChanged += (s, e) => FilterData();
            toolbar.Controls.Add(cmbBloodFilter);

            // Position toolbar controls on resize
            toolbar.Resize += (s, e) =>
            {
                int right = toolbar.ClientSize.Width;
                cmbBloodFilter.Location = new Point(right - cmbBloodFilter.Width, 8);
                txtSearch.Location = new Point(right - cmbBloodFilter.Width - txtSearch.Width - 10, 8);
            };

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
            dgv.RowHeadersVisible = false;
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgv.EnableHeadersVisualStyles = false;
            dgv.ColumnHeadersHeight = 46;
            dgv.RowTemplate.Height = 52;
            dgv.GridColor = Color.FromArgb(229, 231, 235);
            dgv.CellBorderStyle = DataGridViewCellBorderStyle.Single;

            dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(55, 65, 81);
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI Semibold", 10);
            dgv.ColumnHeadersDefaultCellStyle.SelectionBackColor = lightBrickRed;
            dgv.ColumnHeadersDefaultCellStyle.SelectionForeColor = brickRed;

            dgv.DefaultCellStyle.Font = new Font("Segoe UI", 10);
            dgv.DefaultCellStyle.ForeColor = Color.FromArgb(55, 65, 81);
            dgv.DefaultCellStyle.SelectionBackColor = lightBrickRed;
            dgv.DefaultCellStyle.SelectionForeColor = brickRed;
            dgv.DefaultCellStyle.Padding = new Padding(4, 0, 4, 0);

            tablePanel.Controls.Add(dgv);

            // Columns
            dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "id", HeaderText = "Patient ID", FillWeight = 80 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "name", HeaderText = "Name", FillWeight = 130 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "age", HeaderText = "Age", FillWeight = 50 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "gender", HeaderText = "Gender", FillWeight = 70 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "blood", HeaderText = "Blood Group", FillWeight = 80 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "disease", HeaderText = "Disease", FillWeight = 100 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "status", HeaderText = "Status", FillWeight = 100 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "actions", HeaderText = "Actions", FillWeight = 140 });

            dgv.CellPainting += Dgv_CellPainting;
            dgv.CellClick += Dgv_CellClick;

            // Reposition cards on resize
            cardsContainer.Resize += (s, e) =>
            {
                int gap = 14;
                int cw = (cardsContainer.ClientSize.Width - gap * 3) / 4;
                int ch = cardsContainer.ClientSize.Height - cardsContainer.Padding.Top - cardsContainer.Padding.Bottom;
                for (int i = 0; i < 4; i++)
                    cards[i].SetBounds(i * (cw + gap), cardsContainer.Padding.Top, cw, ch);
            };
        }

        private void UpdateSummaryCards()
        {
            if (cards == null) return;

            int total = 0, critical = 0, underTreatment = 0, stable = 0;

            if (allPatients != null)
            {
                total = allPatients.Rows.Count;
                foreach (DataRow row in allPatients.Rows)
                {
                    string status = row["Status"]?.ToString() ?? "";
                    if (status == "Critical") critical++;
                    else if (status == "Under Treatment" || status == "Under Observation") underTreatment++;
                    else if (status == "Stable") stable++;
                }
            }

            string[] titles = { "Total Patients", "Critical Patients", "Under Treatment", "Stable" };
            int[] values = { total, critical, underTreatment, stable };
            Color[] colors = {
                Color.FromArgb(120, 22, 27),
                Color.FromArgb(220, 38, 38),
                Color.FromArgb(59, 130, 246),
                Color.FromArgb(22, 163, 74)
            };

            for (int i = 0; i < 4; i++)
            {
                var card = cards[i];
                card.Controls.Clear();

                int cardH = card.Height > 0 ? card.Height : 95;

                Label lblTitle = new Label();
                lblTitle.Text = titles[i];
                lblTitle.Font = new Font("Segoe UI", 10);
                lblTitle.ForeColor = Color.FromArgb(107, 114, 128);
                lblTitle.TextAlign = ContentAlignment.BottomCenter;
                lblTitle.Size = new Size(card.Width > 0 ? card.Width : 200, cardH / 2);
                lblTitle.Location = new Point(0, 0);
                lblTitle.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
                card.Controls.Add(lblTitle);

                Label lblValue = new Label();
                lblValue.Text = values[i].ToString();
                lblValue.Font = new Font("Segoe UI", 26, FontStyle.Bold);
                lblValue.ForeColor = colors[i];
                lblValue.TextAlign = ContentAlignment.TopCenter;
                lblValue.Size = new Size(card.Width > 0 ? card.Width : 200, cardH / 2);
                lblValue.Location = new Point(0, cardH / 2);
                lblValue.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
                card.Controls.Add(lblValue);

                card.Resize += (s, e) =>
                {
                    var c = (Panel)s;
                    int h = c.Height;
                    int w = c.Width;
                    lblTitle.Size = new Size(w, h / 2);
                    lblValue.Size = new Size(w, h / 2);
                    lblValue.Location = new Point(0, h / 2);
                };
            }
        }

        private void FilterData()
        {
            if (allPatients == null) return;

            string search = txtSearch.Text.Trim().ToLower();
            string bloodFilter = cmbBloodFilter.SelectedItem?.ToString();

            filteredPatients = allPatients.Clone();

            foreach (DataRow row in allPatients.Rows)
            {
                bool matchSearch = string.IsNullOrWhiteSpace(search) ||
                                   search == "search patients..." ||
                                   row["FullName"].ToString().ToLower().Contains(search) ||
                                   row["PatientID"].ToString().Contains(search) ||
                                   row["Phone"].ToString().Contains(search);

                bool matchBlood = bloodFilter == "All Blood Groups" ||
                                  row["BloodGroup"].ToString() == bloodFilter;

                if (matchSearch && matchBlood)
                    filteredPatients.ImportRow(row);
            }

            LoadTableData();
        }

        private void LoadTableData()
        {
            if (dgv == null || filteredPatients == null) return;

            dgv.Rows.Clear();

            if (filteredPatients.Rows.Count == 0)
            {
                dgv.Rows.Add("", "No patients found", "", "", "", "", "", "");
                return;
            }

            foreach (DataRow row in filteredPatients.Rows)
            {
                string gender = row["Gender"]?.ToString();
                if (string.IsNullOrWhiteSpace(gender)) gender = "N/A";

                dgv.Rows.Add(
                    row["PatientID"],
                    row["FullName"],
                    row["Age"],
                    gender,
                    row["BloodGroup"],
                    row["Disease"],
                    row["Status"],
                    ""
                );
            }
        }

        private void Dgv_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex == -1 && e.ColumnIndex >= 0)
            {
                bool isSel = (e.ColumnIndex == selectedColumnIndex);
                Color bg = isSel ? lightBrickRed : Color.FromArgb(248, 250, 252);
                Color fg = isSel ? brickRed : Color.FromArgb(55, 65, 81);
                using (SolidBrush br = new SolidBrush(bg))
                    e.Graphics.FillRectangle(br, e.CellBounds);
                TextRenderer.DrawText(e.Graphics, e.FormattedValue?.ToString(),
                    new Font("Segoe UI Semibold", 10), e.CellBounds, fg,
                    TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter);
                e.Handled = true;
                return;
            }

            if (e.RowIndex < 0) return;

            // Skip painting for "No patients found" row
            if (dgv.Rows.Count > 0 && e.RowIndex < dgv.Rows.Count)
            {
                var firstCell = dgv.Rows[e.RowIndex].Cells[0].Value?.ToString();
                if (firstCell == "No patients found")
                {
                    e.PaintBackground(e.CellBounds, true);
                    e.Handled = true;
                    return;
                }
            }

            bool isAlt = e.RowIndex % 2 == 0;
            Color rowBg = dgv.Rows[e.RowIndex].Selected
                ? lightBrickRed
                : (isAlt ? Color.White : Color.FromArgb(250, 250, 252));

            // Status column (6)
            if (e.ColumnIndex == 6)
            {
                using (SolidBrush br = new SolidBrush(rowBg))
                    e.Graphics.FillRectangle(br, e.CellBounds);

                string status = e.Value?.ToString() ?? "";
                Color fg = status == "Critical" ? Color.FromArgb(220, 38, 38) :
                           status == "Stable" ? Color.FromArgb(22, 163, 74) :
                           status == "Under Treatment" ? Color.FromArgb(59, 130, 246) :
                           status == "Active" ? Color.FromArgb(245, 158, 11) :
                           Color.FromArgb(107, 114, 128);

                Size textSize = TextRenderer.MeasureText(status, new Font("Segoe UI Semibold", 9));
                Rectangle badge = new Rectangle(
                    e.CellBounds.X + 8,
                    e.CellBounds.Y + (e.CellBounds.Height - 22) / 2,
                    textSize.Width + 16, 22);

                Color badgeBg = Color.FromArgb(30, fg.R, fg.G, fg.B);
                using (SolidBrush bb = new SolidBrush(badgeBg))
                    e.Graphics.FillRectangle(bb, badge);

                TextRenderer.DrawText(e.Graphics, status, new Font("Segoe UI Semibold", 9),
                    badge, fg, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                e.Handled = true;
            }

            // Actions column (7)
            if (e.ColumnIndex == 7)
            {
                using (SolidBrush br = new SolidBrush(rowBg))
                    e.Graphics.FillRectangle(br, e.CellBounds);

                int btnW = 62;
                int btnH = 28;
                int gap = 6;
                int total = btnW * 3 + gap * 2;
                int sx = e.CellBounds.X + (e.CellBounds.Width - total) / 2;
                int sy = e.CellBounds.Y + (e.CellBounds.Height - btnH) / 2;

                DrawButton(e.Graphics, "View", new Rectangle(sx, sy, btnW, btnH), Color.FromArgb(59, 130, 246));
                DrawButton(e.Graphics, "Edit", new Rectangle(sx + btnW + gap, sy, btnW, btnH), Color.FromArgb(16, 185, 129));
                DrawButton(e.Graphics, "History", new Rectangle(sx + (btnW + gap) * 2, sy, btnW, btnH), brickRed);
                e.Handled = true;
            }
        }

        private void DrawButton(Graphics g, string text, Rectangle rect, Color color)
        {
            using (SolidBrush br = new SolidBrush(color))
                g.FillRectangle(br, rect);
            TextRenderer.DrawText(g, text, new Font("Segoe UI Semibold", 9), rect, Color.White,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        }

        private void Dgv_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;

            // Skip for "No patients found" row
            if (dgv.Rows.Count > 0 && e.RowIndex < dgv.Rows.Count)
            {
                var firstCell = dgv.Rows[e.RowIndex].Cells[0].Value?.ToString();
                if (firstCell == "No patients found") return;
            }

            selectedColumnIndex = e.ColumnIndex;
            dgv.Invalidate();

            string patientId = dgv.Rows[e.RowIndex].Cells["id"].Value?.ToString();
            string patientName = dgv.Rows[e.RowIndex].Cells["name"].Value?.ToString();

            if (e.ColumnIndex == 7)
            {
                Rectangle cellRect = dgv.GetCellDisplayRectangle(e.ColumnIndex, e.RowIndex, false);
                Point cursor = dgv.PointToClient(Cursor.Position);
                int clickX = cursor.X - cellRect.X;

                int btnW = 62;
                int gap = 6;
                int total = btnW * 3 + gap * 2;
                int sx = (cellRect.Width - total) / 2;

                if (clickX >= sx && clickX < sx + btnW)
                    ShowPatientDetails(patientId);
                else if (clickX >= sx + btnW + gap && clickX < sx + btnW * 2 + gap)
                    EditPatient(patientId);
                else if (clickX >= sx + (btnW + gap) * 2)
                    ShowPatientHistory(patientName);
            }
        }

        private void ShowPatientDetails(string patientId)
        {
            if (string.IsNullOrEmpty(patientId)) return;

            foreach (DataRow row in allPatients.Rows)
            {
                if (row["PatientID"].ToString() == patientId)
                {
                    string details =
                        $"📋 PATIENT DETAILS\n\n" +
                        $"Patient ID:  {row["PatientID"]}\n" +
                        $"Name:        {row["FullName"]}\n" +
                        $"Age:         {row["Age"]}\n" +
                        $"Gender:      {row["Gender"]}\n" +
                        $"Blood Group: {row["BloodGroup"]}\n" +
                        $"Disease:     {row["Disease"]}\n" +
                        $"Status:      {row["Status"]}\n" +
                        $"Phone:       {row["Phone"]}\n" +
                        $"Email:       {row["Email"]}";

                    MessageBox.Show(details, "Patient Details",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
            }
        }

        private void EditPatient(string patientId)
        {
            MessageBox.Show($"Edit patient: {patientId}\n\nFeature coming soon.",
                "Edit Patient", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ShowPatientHistory(string patientName)
        {
            MessageBox.Show($"Medical history for: {patientName}\n\nFeature coming soon.",
                "Patient History", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}