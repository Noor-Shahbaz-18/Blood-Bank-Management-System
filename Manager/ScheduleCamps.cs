using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using BloodBankManagementSystem.Classes.Database;
using BloodBankManagementSystem.Classes.Helpers;

namespace BloodBankManagementSystem.Forms.Manager
{
    public partial class ScheduleCamps : Form
    {
        private TableLayoutPanel mainLayout;
        private Panel topPanel;
        private Panel cardsPanel;
        private Label lblTableHeading;
        private DataGridView dgv;
        private FlowLayoutPanel buttonPanel;
        private TextBox txtSearch;
        private ComboBox cmbCityFilter;
        private DataTable campData;
        private Label lblStatus;

        public ScheduleCamps()
        {
            InitializeComponent();
            this.Text = "Schedule Donation Camps";
            this.WindowState = FormWindowState.Maximized;
            this.BackColor = Color.FromArgb(245, 247, 250);
            this.MinimumSize = new Size(1200, 700);
            this.StartPosition = FormStartPosition.CenterScreen;

            BuildUI();
            LoadCampsFromDatabase();
        }

        private void BuildUI()
        {
            mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 6,
                BackColor = Color.Transparent
            };
            mainLayout.RowStyles.Clear();
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 60));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 110));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 45));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 45));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 60));

            // Top panel
            topPanel = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(5, 31, 64) };
            Label title = new Label
            {
                Text = "Schedule Donation Camps",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(25, 18)
            };
            topPanel.Controls.Add(title);

            lblStatus = new Label
            {
                Text = "✅ Ready",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(16, 185, 129),
                AutoSize = true,
                Location = new Point(topPanel.Width - 220, 22)
            };
            topPanel.Controls.Add(lblStatus);
            topPanel.Resize += (s, e) => lblStatus.Location = new Point(topPanel.Width - 220, 22);
            mainLayout.Controls.Add(topPanel, 0, 0);

            // Cards panel
            cardsPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                Padding = new Padding(20, 10, 20, 10)
            };
            mainLayout.Controls.Add(cardsPanel, 0, 1);

            // Filter panel
            Panel filterPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                Padding = new Padding(20, 5, 20, 5)
            };

            Label searchLabel = new Label
            {
                Text = "Search:",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.Gray,
                Location = new Point(20, 12),
                AutoSize = true
            };
            filterPanel.Controls.Add(searchLabel);

            txtSearch = new TextBox
            {
                Font = new Font("Segoe UI", 9),
                Size = new Size(180, 25),
                Location = new Point(80, 8),
                BorderStyle = BorderStyle.FixedSingle
            };
            txtSearch.TextChanged += (s, e) => SearchCamps();
            filterPanel.Controls.Add(txtSearch);

            Label cityLabel = new Label
            {
                Text = "City:",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.Gray,
                Location = new Point(280, 12),
                AutoSize = true
            };
            filterPanel.Controls.Add(cityLabel);

            cmbCityFilter = new ComboBox
            {
                Location = new Point(320, 8),
                Size = new Size(120, 25),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9)
            };
            cmbCityFilter.Items.AddRange(new[] { "All", "Lahore", "Karachi", "Islamabad", "Rawalpindi", "Multan", "Faisalabad" });
            cmbCityFilter.SelectedIndex = 0;
            cmbCityFilter.SelectedIndexChanged += (s, e) => SearchCamps();
            filterPanel.Controls.Add(cmbCityFilter);

            Button btnRefresh = new Button
            {
                Text = "🔄 Refresh",
                Location = new Point(460, 6),
                Size = new Size(90, 28),
                BackColor = Color.FromArgb(60, 70, 80),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnRefresh.FlatAppearance.BorderSize = 0;
            btnRefresh.Click += (s, e) => LoadCampsFromDatabase();
            filterPanel.Controls.Add(btnRefresh);
            mainLayout.Controls.Add(filterPanel, 0, 2);

            // Heading
            lblTableHeading = new Label
            {
                Text = "🏕️ Donation Camps",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.FromArgb(40, 40, 50),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(20, 0, 0, 0),
                BackColor = Color.Transparent
            };
            mainLayout.Controls.Add(lblTableHeading, 0, 3);

            // DataGridView
            dgv = CreateModernDataGridView();
            dgv.CellFormatting += Dgv_CellFormatting;
            mainLayout.Controls.Add(dgv, 0, 4);

            // Buttons panel
            buttonPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                FlowDirection = FlowDirection.LeftToRight,
                Padding = new Padding(20, 10, 20, 10)
            };

            Button btnAdd = CreateButton("➕ New Camp", Color.FromArgb(34, 197, 94));
            btnAdd.Click += BtnAdd_Click;

            Button btnRefreshList = CreateButton("🔄 Refresh", Color.FromArgb(59, 130, 246));
            btnRefreshList.Click += (s, e) => LoadCampsFromDatabase();

            Button btnExport = CreateButton("📊 Export", Color.FromArgb(245, 158, 11));
            btnExport.Click += BtnExport_Click;

            buttonPanel.Controls.Add(btnAdd);
            buttonPanel.Controls.Add(btnRefreshList);
            buttonPanel.Controls.Add(btnExport);
            mainLayout.Controls.Add(buttonPanel, 0, 5);

            Controls.Add(mainLayout);
        }

        private Button CreateButton(string text, Color backColor)
        {
            return new Button
            {
                Text = text,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                BackColor = backColor,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(130, 38),
                Margin = new Padding(5, 0, 5, 0),
                Cursor = Cursors.Hand,
                FlatAppearance = { BorderSize = 0 }
            };
        }

        private DataGridView CreateModernDataGridView()
        {
            DataGridView dgv = new DataGridView
            {
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                AllowUserToAddRows = false,
                ReadOnly = true,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                EnableHeadersVisualStyles = false,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                GridColor = Color.FromArgb(229, 231, 235),
                RowTemplate = { Height = 38 },
                ColumnHeadersHeight = 40,
                Dock = DockStyle.Fill
            };

            dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(120, 22, 27);
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            dgv.DefaultCellStyle.Font = new Font("Segoe UI", 9);
            dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(254, 242, 242);
            dgv.DefaultCellStyle.SelectionForeColor = Color.FromArgb(120, 22, 27);
            dgv.DefaultCellStyle.Padding = new Padding(10, 5, 10, 5);
            dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(252, 252, 252);

            return dgv;
        }

        private void LoadCampsFromDatabase()
        {
            try
            {
                UpdateStatus("🔄 Loading camps...", Color.FromArgb(59, 130, 246));

                campData = DonationCampDAL.GetAllActiveCamps();

                if (campData != null && campData.Rows.Count > 0)
                {
                    dgv.DataSource = campData;
                    FormatGrid();
                }
                else
                {
                    dgv.DataSource = null;
                    dgv.Rows.Clear();
                    dgv.Columns.Clear();
                    dgv.Columns.Add("Message", "Information");
                    dgv.Rows.Add("No donation camps found. Click 'New Camp' to schedule.");
                }

                UpdateStatistics();
                UpdateStatus($"✅ Loaded {(campData?.Rows.Count ?? 0)} camp(s)", Color.FromArgb(16, 185, 129));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LoadCampsFromDatabase Error: {ex.Message}");
                dgv.DataSource = null;
                UpdateStatus($"❌ Error: {ex.Message}", Color.FromArgb(220, 38, 38));
            }
        }

        private void FormatGrid()
        {
            if (dgv.Columns.Count == 0) return;

            if (dgv.Columns.Contains("CampID"))
                dgv.Columns["CampID"].Visible = false;

            if (dgv.Columns.Contains("CampName"))
                dgv.Columns["CampName"].HeaderText = "Camp Name";

            if (dgv.Columns.Contains("Location"))
                dgv.Columns["Location"].HeaderText = "Location";

            if (dgv.Columns.Contains("City"))
                dgv.Columns["City"].HeaderText = "City";

            if (dgv.Columns.Contains("StartDate"))
                dgv.Columns["StartDate"].HeaderText = "Start Date";

            if (dgv.Columns.Contains("EndDate"))
                dgv.Columns["EndDate"].HeaderText = "End Date";

            if (dgv.Columns.Contains("Organizer"))
                dgv.Columns["Organizer"].HeaderText = "Organizer";

            if (dgv.Columns.Contains("TargetDonors"))
                dgv.Columns["TargetDonors"].HeaderText = "Target";

            if (dgv.Columns.Contains("DonorsCollected"))
                dgv.Columns["DonorsCollected"].HeaderText = "Collected";

            if (dgv.Columns.Contains("Status"))
                dgv.Columns["Status"].HeaderText = "Status";
        }

        private void UpdateStatistics()
        {
            if (campData == null)
            {
                UpdateCards(0, 0, 0);
                return;
            }

            int upcoming = 0;
            int completed = 0;
            int totalUnits = 0;

            foreach (DataRow row in campData.Rows)
            {
                string status = row["Status"]?.ToString() ?? "";
                if (status == "Upcoming") upcoming++;
                else if (status == "Completed") completed++;

                int donors = row["DonorsCollected"] != DBNull.Value ? Convert.ToInt32(row["DonorsCollected"]) : 0;
                totalUnits += donors;
            }

            UpdateCards(upcoming, completed, totalUnits);
        }

        private void UpdateCards(int upcoming, int completed, int totalUnits)
        {
            cardsPanel.Controls.Clear();

            Panel card1 = CreateStatCard("Upcoming Camps", upcoming.ToString(), "📅", Color.FromArgb(59, 130, 246), 20);
            Panel card2 = CreateStatCard("Completed Camps", completed.ToString(), "✅", Color.FromArgb(34, 197, 94), 340);
            Panel card3 = CreateStatCard("Total Donors", totalUnits.ToString(), "🩸", Color.FromArgb(239, 68, 68), 660);

            cardsPanel.Controls.Add(card1);
            cardsPanel.Controls.Add(card2);
            cardsPanel.Controls.Add(card3);
        }

        private Panel CreateStatCard(string title, string value, string icon, Color color, int leftMargin)
        {
            Panel card = new Panel
            {
                Size = new Size(300, 85),
                Location = new Point(leftMargin, 10),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            Panel iconBox = new Panel
            {
                Size = new Size(45, 45),
                Location = new Point(12, 20),
                BackColor = color
            };

            Label iconLbl = new Label
            {
                Text = icon,
                Font = new Font("Segoe UI", 16),
                ForeColor = Color.White,
                Location = new Point(8, 8),
                AutoSize = true
            };
            iconBox.Controls.Add(iconLbl);

            Label valLbl = new Label
            {
                Text = value,
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.FromArgb(20, 20, 30),
                Location = new Point(70, 18),
                AutoSize = true
            };

            Label titLbl = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.Gray,
                Location = new Point(70, 48),
                AutoSize = true
            };

            card.Controls.Add(iconBox);
            card.Controls.Add(valLbl);
            card.Controls.Add(titLbl);
            return card;
        }

        private void SearchCamps()
        {
            if (campData == null) return;

            string searchTerm = txtSearch.Text.Trim().ToLower();
            string city = cmbCityFilter.SelectedItem?.ToString();

            DataTable filteredDt = campData.Clone();

            foreach (DataRow row in campData.Rows)
            {
                bool match = true;

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    string name = row["CampName"]?.ToString().ToLower() ?? "";
                    string location = row["Location"]?.ToString().ToLower() ?? "";
                    if (!name.Contains(searchTerm) && !location.Contains(searchTerm))
                        match = false;
                }

                if (match && city != "All")
                {
                    string rowCity = row["City"]?.ToString() ?? "";
                    if (rowCity != city) match = false;
                }

                if (match) filteredDt.ImportRow(row);
            }

            dgv.DataSource = filteredDt;
            UpdateStatus($"🔍 Found {filteredDt.Rows.Count} matching camp(s)", Color.FromArgb(59, 130, 246));
        }

        private void Dgv_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.ColumnIndex >= 0 && dgv.Columns[e.ColumnIndex].HeaderText == "Status" && e.Value != null)
            {
                string status = e.Value.ToString();
                if (status == "Upcoming")
                    e.CellStyle.ForeColor = Color.FromArgb(245, 158, 11);
                else if (status == "Completed")
                    e.CellStyle.ForeColor = Color.FromArgb(16, 185, 129);
                else if (status == "Cancelled")
                    e.CellStyle.ForeColor = Color.FromArgb(220, 38, 38);
            }
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            using (var form = new AddEditCamp())
            {
                if (form.ShowDialog() == DialogResult.OK)
                    LoadCampsFromDatabase();
            }
        }

        private void BtnExport_Click(object sender, EventArgs e)
        {
            try
            {
                SaveFileDialog sfd = new SaveFileDialog
                {
                    Filter = "CSV files (*.csv)|*.csv|Excel files (*.xls)|*.xls",
                    DefaultExt = "csv",
                    FileName = $"DonationCamps_{DateTime.Now:yyyyMMdd_HHmmss}"
                };

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    DataTable exportData = dgv.DataSource as DataTable;
                    if (exportData == null || exportData.Rows.Count == 0)
                    {
                        MessageBox.Show("No data to export.", "Export",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    ExportHelper.ExportToCSV(exportData, sfd.FileName);
                    MessageBox.Show($"Camps exported successfully!\n\n{sfd.FileName}",
                        "Export Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Export failed: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdateStatus(string message, Color color)
        {
            if (lblStatus != null && !lblStatus.IsDisposed)
            {
                lblStatus.Text = message;
                lblStatus.ForeColor = color;
            }
        }
    }
}