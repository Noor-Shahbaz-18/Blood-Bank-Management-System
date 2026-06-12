using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using BloodBankManagementSystem.Classes.Database;
using BloodBankManagementSystem.Classes.Helpers;

namespace BloodBankManagementSystem.Forms.Manager
{
    public partial class RareDonorManagement : Form
    {
        private TableLayoutPanel mainLayout;
        private Panel topPanel;
        private Panel cardsPanel;
        private Label lblTableHeading;
        private DataGridView dgv;
        private FlowLayoutPanel buttonPanel;
        private TextBox txtSearch;
        private ComboBox cmbStatusFilter;
        private DataTable donorData;
        private Label lblStatus;
        private int selectedDonorID = 0;
        private string selectedDonorName = "";

        public RareDonorManagement()
        {
            InitializeComponent();
            this.Text = "Rare Donor Management";
            this.WindowState = FormWindowState.Maximized;
            this.BackColor = Color.FromArgb(245, 247, 250);
            this.MinimumSize = new Size(1000, 650);
            this.StartPosition = FormStartPosition.CenterScreen;

            BuildUI();
            LoadRareDonorsFromDatabase();
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
                Text = "Rare Donor Management",
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
                Size = new Size(200, 25),
                Location = new Point(80, 8),
                BorderStyle = BorderStyle.FixedSingle
            };
            txtSearch.TextChanged += (s, e) => SearchDonors();
            filterPanel.Controls.Add(txtSearch);

            Label statusLabel = new Label
            {
                Text = "Status:",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.Gray,
                Location = new Point(300, 12),
                AutoSize = true
            };
            filterPanel.Controls.Add(statusLabel);

            cmbStatusFilter = new ComboBox
            {
                Location = new Point(355, 8),
                Size = new Size(100, 25),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9)
            };
            cmbStatusFilter.Items.AddRange(new[] { "All", "Active", "Inactive" });
            cmbStatusFilter.SelectedIndex = 0;
            cmbStatusFilter.SelectedIndexChanged += (s, e) => SearchDonors();
            filterPanel.Controls.Add(cmbStatusFilter);

            Button btnRefresh = new Button
            {
                Text = "🔄 Refresh",
                Location = new Point(480, 6),
                Size = new Size(90, 28),
                BackColor = Color.FromArgb(60, 70, 80),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnRefresh.FlatAppearance.BorderSize = 0;
            btnRefresh.Click += (s, e) => LoadRareDonorsFromDatabase();
            filterPanel.Controls.Add(btnRefresh);
            mainLayout.Controls.Add(filterPanel, 0, 2);

            // Heading
            lblTableHeading = new Label
            {
                Text = "⭐ Rare Donor Registry",
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
            dgv.SelectionChanged += Dgv_SelectionChanged;
            mainLayout.Controls.Add(dgv, 0, 4);

            // Buttons panel
            buttonPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                FlowDirection = FlowDirection.LeftToRight,
                Padding = new Padding(20, 10, 20, 10)
            };

            Button btnAdd = CreateButton("➕ Add Donor", Color.FromArgb(34, 197, 94));
            btnAdd.Click += BtnAdd_Click;

            Button btnEdit = CreateButton("✏️ Edit Donor", Color.FromArgb(251, 146, 60));
            btnEdit.Click += BtnEdit_Click;  // ← Edit button click event

            Button btnDelete = CreateButton("🗑️ Delete Donor", Color.FromArgb(220, 38, 38));
            btnDelete.Click += BtnDelete_Click;

            Button btnExport = CreateButton("📊 Export", Color.FromArgb(59, 130, 246));
            btnExport.Click += BtnExport_Click;

            buttonPanel.Controls.Add(btnAdd);
            buttonPanel.Controls.Add(btnEdit);
            buttonPanel.Controls.Add(btnDelete);
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

        private void LoadRareDonorsFromDatabase()
        {
            try
            {
                UpdateStatus("🔄 Loading rare donors...", Color.FromArgb(59, 130, 246));

                donorData = RareDonorDAL.GetAllRareDonors();

                if (donorData != null && donorData.Rows.Count > 0)
                {
                    dgv.DataSource = donorData;
                    FormatGrid();
                }
                else
                {
                    dgv.DataSource = null;
                    dgv.Rows.Clear();
                    dgv.Columns.Clear();
                    dgv.Columns.Add("Message", "Information");
                    dgv.Rows.Add("No rare donors found in database. Click 'Add Donor' to register.");
                }

                UpdateStatistics();
                UpdateStatus($"✅ Loaded {(donorData?.Rows.Count ?? 0)} rare donor(s)", Color.FromArgb(16, 185, 129));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LoadRareDonorsFromDatabase Error: {ex.Message}");
                dgv.DataSource = null;
                UpdateStatus($"❌ Error: {ex.Message}", Color.FromArgb(220, 38, 38));
            }
        }

        private void FormatGrid()
        {
            if (dgv.Columns.Count == 0) return;

            if (dgv.Columns.Contains("DonorID"))
                dgv.Columns["DonorID"].Visible = false;

            if (dgv.Columns.Contains("FullName"))
                dgv.Columns["FullName"].HeaderText = "Donor Name";

            if (dgv.Columns.Contains("BloodGroup"))
                dgv.Columns["BloodGroup"].HeaderText = "Blood Group";

            if (dgv.Columns.Contains("RareBloodType"))
                dgv.Columns["RareBloodType"].HeaderText = "Rare Type";

            if (dgv.Columns.Contains("Phone"))
                dgv.Columns["Phone"].HeaderText = "Contact";

            if (dgv.Columns.Contains("City"))
                dgv.Columns["City"].HeaderText = "City";

            if (dgv.Columns.Contains("Status"))
                dgv.Columns["Status"].HeaderText = "Status";

            if (dgv.Columns.Contains("LastDonationDate"))
                dgv.Columns["LastDonationDate"].HeaderText = "Last Donation";

            if (dgv.Columns.Contains("TotalDonations"))
                dgv.Columns["TotalDonations"].HeaderText = "Donations";
        }

        private void UpdateStatistics()
        {
            if (donorData == null)
            {
                UpdateCards(0, 0, 0);
                return;
            }

            int total = donorData.Rows.Count;
            int active = 0;
            int rareTypes = 0;

            var rareTypesSet = new System.Collections.Generic.HashSet<string>();

            foreach (DataRow row in donorData.Rows)
            {
                string status = row["Status"]?.ToString() ?? "";
                if (status == "Active") active++;

                string rareType = row["RareBloodType"]?.ToString() ?? "";
                if (!string.IsNullOrEmpty(rareType))
                    rareTypesSet.Add(rareType);
            }

            rareTypes = rareTypesSet.Count;
            UpdateCards(total, active, rareTypes);
        }

        private void UpdateCards(int total, int active, int rareTypes)
        {
            cardsPanel.Controls.Clear();

            Panel card1 = CreateStatCard("Total Rare Donors", total.ToString(), "🩸", Color.FromArgb(239, 68, 68), 20);
            Panel card2 = CreateStatCard("Active Donors", active.ToString(), "✅", Color.FromArgb(34, 197, 94), 340);
            Panel card3 = CreateStatCard("Rare Blood Types", rareTypes.ToString(), "⭐", Color.FromArgb(59, 130, 246), 660);

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

        private void SearchDonors()
        {
            if (donorData == null) return;

            string searchTerm = txtSearch.Text.Trim().ToLower();
            string statusFilter = cmbStatusFilter.SelectedItem?.ToString();

            DataTable filteredDt = donorData.Clone();

            foreach (DataRow row in donorData.Rows)
            {
                bool match = true;

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    string name = row["FullName"]?.ToString().ToLower() ?? "";
                    string cnic = row["CNIC"]?.ToString().ToLower() ?? "";
                    string rareType = row["RareBloodType"]?.ToString().ToLower() ?? "";
                    if (!name.Contains(searchTerm) && !cnic.Contains(searchTerm) && !rareType.Contains(searchTerm))
                        match = false;
                }

                if (match && statusFilter != "All")
                {
                    string status = row["Status"]?.ToString() ?? "";
                    if (status != statusFilter) match = false;
                }

                if (match) filteredDt.ImportRow(row);
            }

            dgv.DataSource = filteredDt;
            UpdateStatus($"🔍 Found {filteredDt.Rows.Count} matching donor(s)", Color.FromArgb(59, 130, 246));
        }

        private void Dgv_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.ColumnIndex >= 0 && dgv.Columns[e.ColumnIndex].HeaderText == "Status" && e.Value != null)
            {
                string status = e.Value.ToString();
                if (status == "Active")
                    e.CellStyle.ForeColor = Color.FromArgb(16, 185, 129);
                else
                    e.CellStyle.ForeColor = Color.FromArgb(220, 38, 38);
            }
        }

        private void Dgv_SelectionChanged(object sender, EventArgs e)
        {
            if (dgv.SelectedRows.Count > 0 && dgv.SelectedRows[0].DataBoundItem != null)
            {
                DataRowView row = dgv.SelectedRows[0].DataBoundItem as DataRowView;
                if (row != null)
                {
                    if (row.Row.Table.Columns.Contains("DonorID"))
                        selectedDonorID = Convert.ToInt32(row["DonorID"]);
                    if (row.Row.Table.Columns.Contains("FullName"))
                        selectedDonorName = row["FullName"]?.ToString() ?? "";
                }
            }
        }

        // =========================================================
        // ADD DONOR BUTTON
        // =========================================================
        private void BtnAdd_Click(object sender, EventArgs e)
        {
            using (var form = new AddRareDonor())  // No ID = Add mode
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    LoadRareDonorsFromDatabase();
                }
            }
        }

        // =========================================================
        // EDIT DONOR BUTTON - IMPORTANT!
        // =========================================================
        private void BtnEdit_Click(object sender, EventArgs e)
        {
            if (selectedDonorID == 0)
            {
                MessageBox.Show("Please select a donor to edit.", "No Selection",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Pass the selected donor ID to the form for edit mode
            using (var form = new AddRareDonor(selectedDonorID))
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    LoadRareDonorsFromDatabase(); // Refresh the list
                    UpdateStatus($"✅ Donor '{selectedDonorName}' updated successfully", Color.FromArgb(16, 185, 129));
                }
            }
        }

        // =========================================================
        // DELETE DONOR BUTTON
        // =========================================================
        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (selectedDonorID == 0)
            {
                MessageBox.Show("Please select a donor to delete.", "No Selection",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            DialogResult result = MessageBox.Show($"Are you sure you want to delete donor:\n{selectedDonorName}?\n\nThis action cannot be undone.",
                "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                try
                {
                    if (RareDonorDAL.Delete(selectedDonorID))
                    {
                        UpdateStatus("✅ Donor deleted successfully", Color.FromArgb(16, 185, 129));
                        LoadRareDonorsFromDatabase();
                        selectedDonorID = 0;
                        selectedDonorName = "";
                    }
                    else
                    {
                        UpdateStatus("❌ Failed to delete donor", Color.FromArgb(220, 38, 38));
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"BtnDelete_Click Error: {ex.Message}");
                    UpdateStatus($"Error: {ex.Message}", Color.FromArgb(220, 38, 38));
                }
            }
        }

        // =========================================================
        // EXPORT BUTTON
        // =========================================================
        private void BtnExport_Click(object sender, EventArgs e)
        {
            try
            {
                SaveFileDialog sfd = new SaveFileDialog
                {
                    Filter = "CSV files (*.csv)|*.csv|Excel files (*.xls)|*.xls",
                    DefaultExt = "csv",
                    FileName = $"RareDonors_Report_{DateTime.Now:yyyyMMdd_HHmmss}"
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
                    MessageBox.Show($"Rare donors exported successfully!\n\n{sfd.FileName}",
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