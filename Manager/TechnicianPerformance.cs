using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using BloodBankManagementSystem.Classes.Database;
using BloodBankManagementSystem.Classes.Helpers;

namespace BloodBankManagementSystem.Forms.Manager
{
    public partial class TechnicianPerformance : Form
    {
        private TableLayoutPanel mainLayout;
        private Panel topPanel;
        private Panel cardsPanel;
        private Label lblTableHeading;
        private DataGridView dgv;
        private FlowLayoutPanel buttonPanel;
        private TextBox txtSearch;
        private DataTable technicianData;
        private Label lblStatus;
        private int selectedTechnicianID = 0;
        private string selectedTechnicianName = "";

        public TechnicianPerformance()
        {
            InitializeComponent();
            this.Text = "Technician Performance";
            this.WindowState = FormWindowState.Maximized;
            this.BackColor = Color.FromArgb(245, 247, 250);
            this.MinimumSize = new Size(1000, 650);
            this.StartPosition = FormStartPosition.CenterScreen;

            BuildUI();
            LoadTechniciansFromDatabase();
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
                Text = "Technician Performance",
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
            txtSearch.TextChanged += (s, e) => SearchTechnicians();
            filterPanel.Controls.Add(txtSearch);

            Button btnRefresh = new Button
            {
                Text = "🔄 Refresh",
                Location = new Point(300, 6),
                Size = new Size(90, 28),
                BackColor = Color.FromArgb(60, 70, 80),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnRefresh.FlatAppearance.BorderSize = 0;
            btnRefresh.Click += (s, e) => LoadTechniciansFromDatabase();
            filterPanel.Controls.Add(btnRefresh);
            mainLayout.Controls.Add(filterPanel, 0, 2);

            // Heading
            lblTableHeading = new Label
            {
                Text = "👨‍🔬 Technician Performance Dashboard",
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

            // ✅ ADD TECHNICIAN BUTTON
            Button btnAdd = CreateButton("➕ Add Technician", Color.FromArgb(34, 197, 94));
            btnAdd.Click += BtnAdd_Click;

            // ✅ EDIT TECHNICIAN BUTTON
            Button btnEdit = CreateButton("✏️ Edit Technician", Color.FromArgb(251, 146, 60));
            btnEdit.Click += BtnEdit_Click;

            // ✅ DELETE TECHNICIAN BUTTON
            Button btnDelete = CreateButton("🗑️ Delete Technician", Color.FromArgb(220, 38, 38));
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

        private void LoadTechniciansFromDatabase()
        {
            try
            {
                UpdateStatus("🔄 Loading technicians...", Color.FromArgb(59, 130, 246));
                Cursor = Cursors.WaitCursor;

                technicianData = TechnicianDAL.GetAll();

                if (technicianData != null && technicianData.Rows.Count > 0)
                {
                    dgv.DataSource = technicianData;
                    FormatGrid();
                    UpdateStatus($"✅ Loaded {technicianData.Rows.Count} technician(s)", Color.FromArgb(16, 185, 129));
                }
                else
                {
                    dgv.DataSource = null;
                    dgv.Rows.Clear();
                    dgv.Columns.Clear();
                    dgv.Columns.Add("Message", "Information");
                    dgv.Rows.Add("No technicians found in database. Click 'Add Technician' to register.");
                    UpdateStatus("⚠️ No technicians found", Color.FromArgb(245, 158, 11));
                }

                UpdateStatistics();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LoadTechniciansFromDatabase Error: {ex.Message}");
                dgv.DataSource = null;
                UpdateStatus($"❌ Database Error: {ex.Message}", Color.FromArgb(220, 38, 38));
                MessageBox.Show($"Error loading technicians: {ex.Message}\n\nPlease check database connection.",
                    "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void FormatGrid()
        {
            if (dgv.Columns.Count == 0) return;

            // Hide ID columns
            if (dgv.Columns.Contains("TechnicianID"))
                dgv.Columns["TechnicianID"].Visible = false;
            if (dgv.Columns.Contains("UserID"))
                dgv.Columns["UserID"].Visible = false;

            // Set headers
            if (dgv.Columns.Contains("FullName"))
                dgv.Columns["FullName"].HeaderText = "Name";
            if (dgv.Columns.Contains("Designation"))
                dgv.Columns["Designation"].HeaderText = "Role";
            if (dgv.Columns.Contains("Specialization"))
                dgv.Columns["Specialization"].HeaderText = "Specialization";
            if (dgv.Columns.Contains("TestsPerformed"))
                dgv.Columns["TestsPerformed"].HeaderText = "Tests";
            if (dgv.Columns.Contains("BagsProcessed"))
                dgv.Columns["BagsProcessed"].HeaderText = "Bags";
            if (dgv.Columns.Contains("Phone"))
                dgv.Columns["Phone"].HeaderText = "Contact";
            if (dgv.Columns.Contains("Email"))
                dgv.Columns["Email"].HeaderText = "Email";
            if (dgv.Columns.Contains("Shift"))
                dgv.Columns["Shift"].HeaderText = "Shift";
            if (dgv.Columns.Contains("IsActive"))
            {
                dgv.Columns["IsActive"].HeaderText = "Status";
                // Convert bool to string for display
                dgv.CellFormatting += (s, e) =>
                {
                    if (e.ColumnIndex == dgv.Columns["IsActive"].Index && e.Value != null)
                    {
                        e.Value = Convert.ToBoolean(e.Value) ? "Active" : "Inactive";
                        e.CellStyle.ForeColor = Convert.ToBoolean(e.Value) ? Color.FromArgb(16, 185, 129) : Color.FromArgb(220, 38, 38);
                        e.FormattingApplied = true;
                    }
                };
            }
        }

        private void UpdateStatistics()
        {
            if (technicianData == null)
            {
                UpdateCards(0, 0, 0);
                return;
            }

            int total = technicianData.Rows.Count;
            int active = 0;
            double totalScore = 0;
            int scoreCount = 0;

            foreach (DataRow row in technicianData.Rows)
            {
                bool isActive = row["IsActive"] != DBNull.Value && Convert.ToBoolean(row["IsActive"]);
                if (isActive) active++;

                int tests = row["TestsPerformed"] != DBNull.Value ? Convert.ToInt32(row["TestsPerformed"]) : 0;
                int bags = row["BagsProcessed"] != DBNull.Value ? Convert.ToInt32(row["BagsProcessed"]) : 0;
                double score = (tests + (bags * 2)) / 100.0;
                if (score > 5) score = 5;
                totalScore += score;
                scoreCount++;
            }

            double avgScore = scoreCount > 0 ? totalScore / scoreCount : 0;
            UpdateCards(total, active, Math.Round(avgScore, 1));
        }

        private void UpdateCards(int total, int active, double avgRating)
        {
            cardsPanel.Controls.Clear();

            Panel card1 = CreateStatCard("Total Technicians", total.ToString(), "👥", Color.FromArgb(59, 130, 246), 20);
            Panel card2 = CreateStatCard("Active Staff", active.ToString(), "✅", Color.FromArgb(34, 197, 94), 340);
            Panel card3 = CreateStatCard("Avg Performance", avgRating.ToString("0.0") + "/5", "⭐", Color.FromArgb(245, 158, 11), 660);

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

        private void SearchTechnicians()
        {
            if (technicianData == null) return;

            string searchTerm = txtSearch.Text.Trim().ToLower();

            DataTable filteredDt = technicianData.Clone();

            foreach (DataRow row in technicianData.Rows)
            {
                bool match = true;

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    string name = row["FullName"]?.ToString().ToLower() ?? "";
                    string designation = row["Designation"]?.ToString().ToLower() ?? "";
                    string specialization = row["Specialization"]?.ToString().ToLower() ?? "";
                    if (!name.Contains(searchTerm) && !designation.Contains(searchTerm) && !specialization.Contains(searchTerm))
                        match = false;
                }

                if (match) filteredDt.ImportRow(row);
            }

            dgv.DataSource = filteredDt;
            UpdateStatus($"🔍 Found {filteredDt.Rows.Count} matching technician(s)", Color.FromArgb(59, 130, 246));
        }

        private void Dgv_SelectionChanged(object sender, EventArgs e)
        {
            if (dgv.SelectedRows.Count > 0 && dgv.SelectedRows[0].DataBoundItem != null)
            {
                DataRowView row = dgv.SelectedRows[0].DataBoundItem as DataRowView;
                if (row != null)
                {
                    if (row.Row.Table.Columns.Contains("TechnicianID"))
                        selectedTechnicianID = Convert.ToInt32(row["TechnicianID"]);
                    if (row.Row.Table.Columns.Contains("FullName"))
                        selectedTechnicianName = row["FullName"]?.ToString() ?? "";
                }
            }
        }

        private void Dgv_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            // This is handled in FormatGrid now
        }

        // =========================================================
        // ADD TECHNICIAN BUTTON
        // =========================================================
        private void BtnAdd_Click(object sender, EventArgs e)
        {
            using (var form = new AddEditTechnician())
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    LoadTechniciansFromDatabase();
                    UpdateStatus("✅ Technician added successfully", Color.FromArgb(16, 185, 129));
                }
            }
        }

        // =========================================================
        // EDIT TECHNICIAN BUTTON
        // =========================================================
        private void BtnEdit_Click(object sender, EventArgs e)
        {
            if (selectedTechnicianID == 0)
            {
                MessageBox.Show("Please select a technician to edit.", "No Selection",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (var form = new AddEditTechnician(selectedTechnicianID))
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    LoadTechniciansFromDatabase();
                    UpdateStatus($"✅ Technician '{selectedTechnicianName}' updated successfully", Color.FromArgb(16, 185, 129));
                }
            }
        }

        // =========================================================
        // DELETE TECHNICIAN BUTTON
        // =========================================================
        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (selectedTechnicianID == 0)
            {
                MessageBox.Show("Please select a technician to delete.", "No Selection",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            DialogResult result = MessageBox.Show($"Are you sure you want to delete technician:\n{selectedTechnicianName}?\n\nThis action cannot be undone.",
                "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                try
                {
                    // First check if technician exists in database
                    var tech = TechnicianDAL.GetByTechnicianID(selectedTechnicianID);
                    if (tech == null)
                    {
                        MessageBox.Show("Technician not found!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    // Soft delete - update IsActive to false
                    bool success = TechnicianDAL.DeleteTechnician(selectedTechnicianID);

                    if (success)
                    {
                        UpdateStatus("✅ Technician deactivated successfully", Color.FromArgb(16, 185, 129));
                        LoadTechniciansFromDatabase();
                        selectedTechnicianID = 0;
                        selectedTechnicianName = "";
                    }
                    else
                    {
                        UpdateStatus("❌ Failed to delete technician", Color.FromArgb(220, 38, 38));
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"BtnDelete_Click Error: {ex.Message}");
                    MessageBox.Show($"Error deleting technician: {ex.Message}", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                    FileName = $"Technician_Performance_{DateTime.Now:yyyyMMdd_HHmmss}"
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
                    MessageBox.Show($"Technician data exported successfully!\n\n{sfd.FileName}",
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
                if (lblStatus.InvokeRequired)
                {
                    lblStatus.Invoke(new Action(() => { lblStatus.Text = message; lblStatus.ForeColor = color; }));
                }
                else
                {
                    lblStatus.Text = message;
                    lblStatus.ForeColor = color;
                }
            }
        }
    }
}