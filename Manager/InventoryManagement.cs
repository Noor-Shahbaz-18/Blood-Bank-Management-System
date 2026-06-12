using BloodBankManagementSystem.Classes.Database;
using BloodBankManagementSystem.Classes.Helpers;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace BloodBankManagementSystem.Forms.Manager
{
    public partial class InventoryManagement : Form
    {
        private TableLayoutPanel mainLayout;
        private Panel topPanel;
        private Panel cardsPanel;
        private Label lblTableHeading;
        private DataGridView dgv;
        private FlowLayoutPanel buttonPanel;
        private TextBox txtSearch;
        private ComboBox cmbBloodGroupFilter;
        private ComboBox cmbStatusFilter;
        private DataTable inventoryData;
        private Label lblStatus;

        public InventoryManagement()
        {
            InitializeComponent();
            this.Text = "Inventory Management";
            this.WindowState = FormWindowState.Maximized;
            this.BackColor = Color.FromArgb(245, 247, 250);
            this.MinimumSize = new Size(1000, 650);
            this.StartPosition = FormStartPosition.CenterScreen;

            BuildUI();
            LoadInventoryFromDatabase();
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
            Label titleLabel = new Label
            {
                Text = "Inventory Management",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(25, 18)
            };
            topPanel.Controls.Add(titleLabel);

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
            txtSearch.TextChanged += (s, e) => SearchInventory();
            filterPanel.Controls.Add(txtSearch);

            Label bloodGroupLabel = new Label
            {
                Text = "Blood Group:",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.Gray,
                Location = new Point(280, 12),
                AutoSize = true
            };
            filterPanel.Controls.Add(bloodGroupLabel);

            cmbBloodGroupFilter = new ComboBox
            {
                Location = new Point(365, 8),
                Size = new Size(100, 25),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9)
            };
            cmbBloodGroupFilter.Items.AddRange(new[] { "All", "A+", "A-", "B+", "B-", "AB+", "AB-", "O+", "O-" });
            cmbBloodGroupFilter.SelectedIndex = 0;
            cmbBloodGroupFilter.SelectedIndexChanged += (s, e) => SearchInventory();
            filterPanel.Controls.Add(cmbBloodGroupFilter);

            Label statusLabel = new Label
            {
                Text = "Status:",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.Gray,
                Location = new Point(485, 12),
                AutoSize = true
            };
            filterPanel.Controls.Add(statusLabel);

            cmbStatusFilter = new ComboBox
            {
                Location = new Point(540, 8),
                Size = new Size(110, 25),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9)
            };
            cmbStatusFilter.Items.AddRange(new[] { "All", "Available", "In Lab", "Issued", "Expired", "Discarded", "Quarantined" });
            cmbStatusFilter.SelectedIndex = 0;
            cmbStatusFilter.SelectedIndexChanged += (s, e) => SearchInventory();
            filterPanel.Controls.Add(cmbStatusFilter);

            Button btnRefresh = new Button
            {
                Text = "🔄 Refresh",
                Location = new Point(670, 6),
                Size = new Size(90, 28),
                BackColor = Color.FromArgb(60, 70, 80),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnRefresh.FlatAppearance.BorderSize = 0;
            btnRefresh.Click += (s, e) => LoadInventoryFromDatabase();
            filterPanel.Controls.Add(btnRefresh);
            mainLayout.Controls.Add(filterPanel, 0, 2);

            // Heading
            lblTableHeading = new Label
            {
                Text = "📋 Blood Bags Inventory",
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

            Button btnAdd = CreateButton("➕ Add Stock", Color.FromArgb(34, 197, 94));
            btnAdd.Click += BtnAddStock_Click;

            Button btnExport = CreateButton("📊 Export", Color.FromArgb(59, 130, 246));
            btnExport.Click += BtnExport_Click;

            buttonPanel.Controls.Add(btnAdd);
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

        private void LoadInventoryFromDatabase()
        {
            try
            {
                UpdateStatus("🔄 Loading inventory...", Color.FromArgb(59, 130, 246));

                using (var conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();

                    string query = @"
                        SELECT 
                            BagID as [Bag ID],
                            ISNULL(DonorName, 'N/A') as [Donor Name],
                            BloodGroup as [Blood Group],
                            ISNULL(ComponentType, 'Whole Blood') as [Component],
                            Volume as [Vol (mL)],
                            FORMAT(CollectionDate, 'dd-MMM-yyyy') as [Collection Date],
                            FORMAT(ExpiryDate, 'dd-MMM-yyyy') as [Expiry Date],
                            DATEDIFF(DAY, GETDATE(), ExpiryDate) as [Days Left],
                            Status,
                            ISNULL(StorageLocation, 'Not Assigned') as [Location]
                        FROM BloodBags 
                        ORDER BY CollectionDate DESC, BagID DESC";

                    SqlDataAdapter da = new SqlDataAdapter(query, conn);
                    inventoryData = new DataTable();
                    da.Fill(inventoryData);

                    dgv.DataSource = inventoryData;
                    UpdateStatistics();

                    UpdateStatus($"✅ Loaded {inventoryData.Rows.Count} bag(s)", Color.FromArgb(16, 185, 129));
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LoadInventoryFromDatabase Error: {ex.Message}");
                dgv.DataSource = null;
                UpdateStatus($"❌ Error: {ex.Message}", Color.FromArgb(220, 38, 38));
            }
        }

        private void UpdateStatistics()
        {
            if (inventoryData == null) return;

            int totalUnits = 0;
            int availableBags = 0;
            int expiringSoon = 0;
            int expired = 0;

            foreach (DataRow row in inventoryData.Rows)
            {
                totalUnits++;
                string status = row["Status"]?.ToString() ?? "";
                int daysLeft = row["Days Left"] != DBNull.Value ? Convert.ToInt32(row["Days Left"]) : 999;

                if (status == "Available") availableBags++;
                if (daysLeft >= 0 && daysLeft <= 7) expiringSoon++;
                if (daysLeft < 0) expired++;
            }

            UpdateCards(totalUnits, availableBags, expiringSoon, expired);
        }

        private void UpdateCards(int total, int available, int expiring, int expired)
        {
            cardsPanel.Controls.Clear();

            Panel card1 = CreateStatCard("Total Bags", total.ToString(), "🩸", Color.FromArgb(239, 68, 68), 20);
            Panel card2 = CreateStatCard("Available", available.ToString(), "✅", Color.FromArgb(34, 197, 94), 340);
            Panel card3 = CreateStatCard("Expiring (7d)", expiring.ToString(), "⚠️", Color.FromArgb(245, 158, 11), 660);
            Panel card4 = CreateStatCard("Expired", expired.ToString(), "❌", Color.FromArgb(220, 38, 38), 980);

            cardsPanel.Controls.Add(card1);
            cardsPanel.Controls.Add(card2);
            cardsPanel.Controls.Add(card3);
            cardsPanel.Controls.Add(card4);
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

        private void SearchInventory()
        {
            if (inventoryData == null) return;

            string searchTerm = txtSearch.Text.Trim().ToLower();
            string bloodGroup = cmbBloodGroupFilter.SelectedItem?.ToString();
            string status = cmbStatusFilter.SelectedItem?.ToString();

            DataTable filteredDt = inventoryData.Clone();

            foreach (DataRow row in inventoryData.Rows)
            {
                bool match = true;

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    string bagID = row["Bag ID"]?.ToString().ToLower() ?? "";
                    string donor = row["Donor Name"]?.ToString().ToLower() ?? "";
                    if (!bagID.Contains(searchTerm) && !donor.Contains(searchTerm))
                        match = false;
                }

                if (match && bloodGroup != "All")
                {
                    string bg = row["Blood Group"]?.ToString() ?? "";
                    if (bg != bloodGroup) match = false;
                }

                if (match && status != "All")
                {
                    string stat = row["Status"]?.ToString() ?? "";
                    if (stat != status) match = false;
                }

                if (match) filteredDt.ImportRow(row);
            }

            dgv.DataSource = filteredDt;
            UpdateStatus($"🔍 Found {filteredDt.Rows.Count} matching bag(s)", Color.FromArgb(59, 130, 246));
        }

        private void Dgv_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.ColumnIndex >= 0 && dgv.Columns[e.ColumnIndex].HeaderText == "Status" && e.Value != null)
            {
                string status = e.Value.ToString();
                if (status == "Available")
                    e.CellStyle.ForeColor = Color.FromArgb(16, 185, 129);
                else if (status == "Expired")
                    e.CellStyle.ForeColor = Color.FromArgb(220, 38, 38);
                else if (status == "Expiring Soon")
                    e.CellStyle.ForeColor = Color.FromArgb(245, 158, 11);
                else if (status == "Issued")
                    e.CellStyle.ForeColor = Color.FromArgb(59, 130, 246);
                else if (status == "Quarantined")
                    e.CellStyle.ForeColor = Color.FromArgb(139, 92, 246);
            }

            if (e.ColumnIndex >= 0 && dgv.Columns[e.ColumnIndex].HeaderText == "Days Left" && e.Value != null)
            {
                int daysLeft = Convert.ToInt32(e.Value);
                if (daysLeft < 0)
                    e.CellStyle.ForeColor = Color.FromArgb(220, 38, 38);
                else if (daysLeft <= 7)
                    e.CellStyle.ForeColor = Color.FromArgb(245, 158, 11);
                else
                    e.CellStyle.ForeColor = Color.FromArgb(16, 185, 129);
            }
        }

        private void BtnAddStock_Click(object sender, EventArgs e)
        {
            using (var form = new AddEditBloodBag())
            {
                if (form.ShowDialog() == DialogResult.OK)
                    LoadInventoryFromDatabase();
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
                    FileName = $"Inventory_Report_{DateTime.Now:yyyyMMdd_HHmmss}"
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
                    MessageBox.Show($"Inventory exported successfully!\n\n{sfd.FileName}",
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