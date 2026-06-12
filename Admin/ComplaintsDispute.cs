using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using BloodBankManagementSystem.Classes.Database;

namespace BloodBankManagementSystem.Forms.Admin
{
    public partial class ComplaintsDispute : Form
    {
        private DataGridView dgvComplaints;
        private TextBox txtSearch;
        private ComboBox cmbStatus;
        private Panel pnlDetails;
        private int selectedComplaintId = 0;

        public ComplaintsDispute()
        {
            InitializeComponent();
            BuildUI();
            LoadComplaints();
        }

        private void BuildUI()
        {
            this.Text = "Complaints & Disputes - Blood Bank";
            this.WindowState = FormWindowState.Maximized;
            this.BackColor = Color.FromArgb(245, 247, 250);
            this.MinimumSize = new Size(1100, 650);

            Panel mainPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20), AutoScroll = true };
            this.Controls.Add(mainPanel);

            Label lblTitle = new Label
            {
                Text = "📋 Complaints & Disputes Management",
                Font = new Font("Segoe UI", 22, FontStyle.Bold),
                ForeColor = Color.FromArgb(120, 22, 27),
                Dock = DockStyle.Top,
                Height = 50,
                TextAlign = ContentAlignment.MiddleLeft
            };
            mainPanel.Controls.Add(lblTitle);

            // Filter Panel
            Panel filterPanel = new Panel { Dock = DockStyle.Top, Height = 70, BackColor = Color.White, Padding = new Padding(15) };
            mainPanel.Controls.Add(filterPanel);

            Label lblSearch = new Label { Text = "🔍 Search:", Location = new Point(10, 25), AutoSize = true };
            txtSearch = new TextBox { Location = new Point(85, 22), Width = 250, Font = new Font("Segoe UI", 10), BorderStyle = BorderStyle.FixedSingle };
            txtSearch.TextChanged += (s, e) => FilterComplaints();

            Label lblStatus = new Label { Text = "Status:", Location = new Point(360, 25), AutoSize = true };
            cmbStatus = new ComboBox { Location = new Point(415, 20), Width = 120, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10) };
            cmbStatus.Items.AddRange(new string[] { "All", "Pending", "InProgress", "Resolved", "Rejected" });
            cmbStatus.SelectedIndex = 0;
            cmbStatus.SelectedIndexChanged += (s, e) => FilterComplaints();

            Button btnRefresh = new Button
            {
                Text = "🔄 Refresh",
                Location = new Point(560, 18),
                Size = new Size(100, 32),
                BackColor = Color.FromArgb(120, 22, 27),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnRefresh.FlatAppearance.BorderSize = 0;
            btnRefresh.Click += (s, e) => LoadComplaints();

            filterPanel.Controls.Add(lblSearch);
            filterPanel.Controls.Add(txtSearch);
            filterPanel.Controls.Add(lblStatus);
            filterPanel.Controls.Add(cmbStatus);
            filterPanel.Controls.Add(btnRefresh);

            // Split Container for List and Details
            SplitContainer splitContainer = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Horizontal,
                SplitterDistance = 350
            };
            mainPanel.Controls.Add(splitContainer);

            // Top Panel - Complaints List
            dgvComplaints = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AllowUserToAddRows = false,
                ReadOnly = true,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };
            dgvComplaints.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(120, 22, 27);
            dgvComplaints.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvComplaints.EnableHeadersVisualStyles = false;
            dgvComplaints.CellClick += DgvComplaints_CellClick;
            splitContainer.Panel1.Controls.Add(dgvComplaints);

            // Bottom Panel - Complaint Details & Resolution
            pnlDetails = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(20), AutoScroll = true };
            splitContainer.Panel2.Controls.Add(pnlDetails);

            SetupDetailsPanel();
        }

        private void SetupDetailsPanel()
        {
            pnlDetails.Controls.Clear();

            Label lblDetailsTitle = new Label
            {
                Text = "Complaint Details & Resolution",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.FromArgb(120, 22, 27),
                Dock = DockStyle.Top,
                Height = 40,
                TextAlign = ContentAlignment.MiddleLeft
            };
            pnlDetails.Controls.Add(lblDetailsTitle);

            int y = 55, leftX = 20, labelWidth = 120, fieldWidth = 400;

            // Complaint Info (Read-only)
            Label lblComplaintId = new Label { Text = "Complaint ID:", Location = new Point(leftX, y), AutoSize = true, Font = new Font("Segoe UI", 9, FontStyle.Bold) };
            Label lblComplaintIdValue = new Label { Text = "-", Location = new Point(leftX + 120, y), AutoSize = true, ForeColor = Color.Gray };
            pnlDetails.Controls.Add(lblComplaintId);
            pnlDetails.Controls.Add(lblComplaintIdValue);
            y += 30;

            Label lblUserName = new Label { Text = "Submitted By:", Location = new Point(leftX, y), AutoSize = true, Font = new Font("Segoe UI", 9, FontStyle.Bold) };
            Label lblUserNameValue = new Label { Text = "-", Location = new Point(leftX + 120, y), AutoSize = true, ForeColor = Color.Gray };
            pnlDetails.Controls.Add(lblUserName);
            pnlDetails.Controls.Add(lblUserNameValue);
            y += 30;

            Label lblSubject = new Label { Text = "Subject:", Location = new Point(leftX, y), AutoSize = true, Font = new Font("Segoe UI", 9, FontStyle.Bold) };
            Label lblSubjectValue = new Label { Text = "-", Location = new Point(leftX + 120, y), AutoSize = true, ForeColor = Color.Gray };
            pnlDetails.Controls.Add(lblSubject);
            pnlDetails.Controls.Add(lblSubjectValue);
            y += 30;

            Label lblDescription = new Label { Text = "Description:", Location = new Point(leftX, y), AutoSize = true, Font = new Font("Segoe UI", 9, FontStyle.Bold) };
            TextBox txtDescription = new TextBox { Location = new Point(leftX + 120, y), Width = 500, Height = 80, Multiline = true, ReadOnly = true, BackColor = Color.FromArgb(250, 250, 252), BorderStyle = BorderStyle.FixedSingle };
            pnlDetails.Controls.Add(lblDescription);
            pnlDetails.Controls.Add(txtDescription);
            y += 100;

            // Resolution Section
            Label lblResolution = new Label { Text = "Resolution / Response:", Location = new Point(leftX, y), AutoSize = true, Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = Color.FromArgb(120, 22, 27) };
            pnlDetails.Controls.Add(lblResolution);
            y += 30;

            TextBox txtResolution = new TextBox { Location = new Point(leftX, y), Width = 600, Height = 80, Multiline = true, BorderStyle = BorderStyle.FixedSingle };
            pnlDetails.Controls.Add(txtResolution);
            y += 100;

            // Status Dropdown
            Label lblStatus = new Label { Text = "Update Status:", Location = new Point(leftX, y), AutoSize = true };
            ComboBox cmbUpdateStatus = new ComboBox { Location = new Point(leftX + 100, y - 3), Width = 130, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbUpdateStatus.Items.AddRange(new string[] { "Pending", "InProgress", "Resolved", "Rejected" });
            pnlDetails.Controls.Add(lblStatus);
            pnlDetails.Controls.Add(cmbUpdateStatus);
            y += 45;

            Button btnResolve = new Button
            {
                Text = "✅ Update & Resolve",
                Location = new Point(leftX, y),
                Size = new Size(160, 40),
                BackColor = Color.FromArgb(120, 22, 27),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnResolve.FlatAppearance.BorderSize = 0;
            btnResolve.Click += (s, e) =>
            {
                if (selectedComplaintId > 0)
                {
                    MessageBox.Show($"Complaint #{selectedComplaintId} updated to {cmbUpdateStatus.Text}!\n\nResponse: {txtResolution.Text}",
                        "Updated", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadComplaints();
                    ClearDetails();
                }
            };
            pnlDetails.Controls.Add(btnResolve);

            Button btnClose = new Button
            {
                Text = "❌ Close",
                Location = new Point(leftX + 180, y),
                Size = new Size(100, 40),
                BackColor = Color.LightGray,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnClose.Click += (s, e) => ClearDetails();
            pnlDetails.Controls.Add(btnClose);

            // Store references
            pnlDetails.Tag = new { lblComplaintIdValue, lblUserNameValue, lblSubjectValue, txtDescription, txtResolution, cmbUpdateStatus };
        }

        private void LoadComplaints()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("ComplaintID"); dt.Columns.Add("User"); dt.Columns.Add("Subject"); dt.Columns.Add("Type"); dt.Columns.Add("Date"); dt.Columns.Add("Status");

            dt.Rows.Add("1", "Ali Raza", "Staff Behavior", "Complaint", "12-May-2026", "Pending");
            dt.Rows.Add("2", "Sara Khan", "Blood Delay", "Dispute", "11-May-2026", "InProgress");
            dt.Rows.Add("3", "Ahmed Ali", "Wrong Blood Group", "Complaint", "10-May-2026", "Resolved");

            dgvComplaints.DataSource = dt;
            if (dgvComplaints.Columns.Contains("ComplaintID")) dgvComplaints.Columns["ComplaintID"].Width = 100;
            if (dgvComplaints.Columns.Contains("User")) dgvComplaints.Columns["User"].Width = 150;
            if (dgvComplaints.Columns.Contains("Subject")) dgvComplaints.Columns["Subject"].Width = 250;
        }

        private void FilterComplaints()
        {
            string filter = txtSearch.Text.ToLower();
            string statusFilter = cmbStatus.SelectedItem?.ToString();

            foreach (DataGridViewRow row in dgvComplaints.Rows)
            {
                bool match = true;
                if (!string.IsNullOrWhiteSpace(filter) && filter != "Search...")
                {
                    match = row.Cells["User"].Value.ToString().ToLower().Contains(filter) ||
                            row.Cells["Subject"].Value.ToString().ToLower().Contains(filter);
                }
                if (match && statusFilter != "All")
                {
                    match = row.Cells["Status"].Value.ToString() == statusFilter;
                }
                row.Visible = match;
            }
        }

        private void DgvComplaints_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                selectedComplaintId = Convert.ToInt32(dgvComplaints.Rows[e.RowIndex].Cells["ComplaintID"].Value);
                var data = pnlDetails.Tag as dynamic;
                if (data != null)
                {
                    data.lblComplaintIdValue.Text = dgvComplaints.Rows[e.RowIndex].Cells["ComplaintID"].Value.ToString();
                    data.lblUserNameValue.Text = dgvComplaints.Rows[e.RowIndex].Cells["User"].Value.ToString();
                    data.lblSubjectValue.Text = dgvComplaints.Rows[e.RowIndex].Cells["Subject"].Value.ToString();
                    data.txtDescription.Text = "Detailed complaint description would appear here...";
                    data.cmbUpdateStatus.Text = dgvComplaints.Rows[e.RowIndex].Cells["Status"].Value.ToString();
                }
            }
        }

        private void ClearDetails()
        {
            selectedComplaintId = 0;
            var data = pnlDetails.Tag as dynamic;
            if (data != null)
            {
                data.lblComplaintIdValue.Text = "-";
                data.lblUserNameValue.Text = "-";
                data.lblSubjectValue.Text = "-";
                data.txtDescription.Text = "";
                data.txtResolution.Text = "";
                data.cmbUpdateStatus.SelectedIndex = 0;
            }
        }
    }
}