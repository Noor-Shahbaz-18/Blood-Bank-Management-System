using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.Threading.Tasks;
using BloodBankManagementSystem.Classes.Database;

namespace BloodBankManagementSystem.Forms.Technician
{
    public partial class AssignStorageLocation : BaseTechnicianForm
    {
        private DataGridView dgvUnassignedBags;
        private DataGridView dgvAssignmentHistory;
        private TextBox txtSearch, txtStorageLocation, txtBagId, txtNotes;
        private Button btnAssign, btnRefresh;
        private DataTable unassignedData;
        private DataTable historyData;
        private Label lblUnassignedCount, lblHistoryCount;

        public AssignStorageLocation()
        {
            this.Text = "Blood Bank Management System – Assign Storage Location";
            BuildLayout();
            BuildSidebar("Assign Storage");
            BuildTopBar("Assign Storage Location");
            BuildContentArea();
            LoadUnassignedBags();
            LoadAssignmentHistory();
        }

        private void BuildContentArea()
        {
            int y = 20;

            // =========================================================
            // SECTION 1: Search and Unassigned Bags
            // =========================================================
            Label lblSearch = new Label
            {
                Text = "Search Bag ID / Donor",
                Font = new Font("Segoe UI", 10),
                ForeColor = C_TextMid,
                AutoSize = true,
                Location = new Point(0, y)
            };
            pnlContent.Controls.Add(lblSearch);

            txtSearch = new TextBox
            {
                Location = new Point(180, y - 2),
                Width = 250,
                Font = new Font("Segoe UI", 10),
                BackColor = C_White,
                BorderStyle = BorderStyle.FixedSingle
            };
            txtSearch.TextChanged += (s, e) => FilterBags();
            pnlContent.Controls.Add(txtSearch);
            y += 45;

            // Unassigned Bags Grid with count
            Panel unassignedHeader = new Panel
            {
                Location = new Point(0, y),
                Width = pnlContent.ClientSize.Width - 60,
                Height = 35,
                BackColor = Color.Transparent
            };
            pnlContent.Controls.Add(unassignedHeader);

            Label lblUnassigned = new Label
            {
                Text = "📦 Unassigned Blood Bags",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = C_TextDark,
                Location = new Point(0, 5),
                AutoSize = true
            };
            unassignedHeader.Controls.Add(lblUnassigned);

            lblUnassignedCount = new Label
            {
                Text = "",
                Font = new Font("Segoe UI", 9),
                ForeColor = C_TextMid,
                Location = new Point(200, 8),
                AutoSize = true
            };
            unassignedHeader.Controls.Add(lblUnassignedCount);
            y += 35;

            dgvUnassignedBags = CreateStyledGrid();
            dgvUnassignedBags.Location = new Point(0, y);
            dgvUnassignedBags.Height = 250;
            dgvUnassignedBags.Width = pnlContent.ClientSize.Width - 60;
            dgvUnassignedBags.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            dgvUnassignedBags.SelectionChanged += DgvUnassignedBags_SelectionChanged;
            WrapInCard(dgvUnassignedBags, pnlContent, new Point(0, y), 250);
            FixDataGridView(dgvUnassignedBags);
            y += 290;

            // =========================================================
            // SECTION 2: Assignment Form
            // =========================================================
            Panel assignGroup = new Panel
            {
                Location = new Point(0, y),
                Width = 550,
                Height = 220,
                BackColor = C_CardBg
            };
            SetRoundedRegion(assignGroup, 12);
            AddDropShadow(assignGroup);
            pnlContent.Controls.Add(assignGroup);

            assignGroup.Controls.Add(new Label
            {
                Text = "✏️ Assign Storage Location",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = C_TextDark,
                Location = new Point(20, 15),
                AutoSize = true
            });

            assignGroup.Controls.Add(new Label
            {
                Text = "Selected Bag ID:",
                Location = new Point(20, 55),
                AutoSize = true,
                ForeColor = C_TextMid
            });
            txtBagId = new TextBox
            {
                Location = new Point(150, 52),
                Width = 200,
                ReadOnly = true,
                BackColor = Color.FromArgb(245, 245, 250),
                BorderStyle = BorderStyle.FixedSingle
            };
            assignGroup.Controls.Add(txtBagId);

            assignGroup.Controls.Add(new Label
            {
                Text = "Storage Location:",
                Location = new Point(20, 90),
                AutoSize = true,
                ForeColor = C_TextMid
            });
            txtStorageLocation = new TextBox
            {
                Location = new Point(150, 87),
                Width = 350,
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.Gray,
                Text = "e.g., Rack A-12, Shelf 3"
            };
            txtStorageLocation.GotFocus += (s, e) =>
            {
                if (txtStorageLocation.Text == "e.g., Rack A-12, Shelf 3")
                {
                    txtStorageLocation.Text = "";
                    txtStorageLocation.ForeColor = C_TextDark;
                }
            };
            txtStorageLocation.LostFocus += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(txtStorageLocation.Text))
                {
                    txtStorageLocation.Text = "e.g., Rack A-12, Shelf 3";
                    txtStorageLocation.ForeColor = Color.Gray;
                }
            };
            assignGroup.Controls.Add(txtStorageLocation);

            assignGroup.Controls.Add(new Label
            {
                Text = "Notes (optional):",
                Location = new Point(20, 125),
                AutoSize = true,
                ForeColor = C_TextMid
            });
            txtNotes = new TextBox
            {
                Location = new Point(150, 122),
                Width = 350,
                Height = 40,
                Multiline = true,
                BorderStyle = BorderStyle.FixedSingle
            };
            assignGroup.Controls.Add(txtNotes);

            btnAssign = new Button
            {
                Text = "✓ Assign Location",
                Location = new Point(150, 175),
                Size = new Size(140, 35),
                BackColor = brickRed,
                ForeColor = C_White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnAssign.FlatAppearance.BorderSize = 0;
            SetRoundedRegion(btnAssign, 8);
            btnAssign.Click += BtnAssign_Click;
            assignGroup.Controls.Add(btnAssign);

            btnRefresh = new Button
            {
                Text = "⟳ Refresh Lists",
                Location = new Point(310, 175),
                Size = new Size(140, 35),
                BackColor = Color.FromArgb(100, 110, 125),
                ForeColor = C_White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnRefresh.FlatAppearance.BorderSize = 0;
            SetRoundedRegion(btnRefresh, 8);
            btnRefresh.Click += (s, e) =>
            {
                LoadUnassignedBags();
                LoadAssignmentHistory();
            };
            assignGroup.Controls.Add(btnRefresh);

            y += 240;

            // =========================================================
            // SECTION 3: Assignment History
            // =========================================================
            Panel historyHeader = new Panel
            {
                Location = new Point(0, y),
                Width = pnlContent.ClientSize.Width - 60,
                Height = 35,
                BackColor = Color.Transparent
            };
            pnlContent.Controls.Add(historyHeader);

            Label lblHistory = new Label
            {
                Text = "📋 Assignment History",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = C_TextDark,
                Location = new Point(0, 5),
                AutoSize = true
            };
            historyHeader.Controls.Add(lblHistory);

            lblHistoryCount = new Label
            {
                Text = "",
                Font = new Font("Segoe UI", 9),
                ForeColor = C_TextMid,
                Location = new Point(200, 8),
                AutoSize = true
            };
            historyHeader.Controls.Add(lblHistoryCount);
            y += 35;

            dgvAssignmentHistory = CreateStyledGrid();
            dgvAssignmentHistory.Location = new Point(0, y);
            dgvAssignmentHistory.Height = 200;
            dgvAssignmentHistory.Width = pnlContent.ClientSize.Width - 60;
            dgvAssignmentHistory.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            WrapInCard(dgvAssignmentHistory, pnlContent, new Point(0, y), 200);
            FixDataGridView(dgvAssignmentHistory);
            y += 240;

            Panel bottomSpacer = new Panel { Height = 30, BackColor = Color.Transparent, Dock = DockStyle.Bottom };
            pnlContent.Controls.Add(bottomSpacer);
        }

        // =========================================================
        // DATABASE: Load unassigned bags (ONLY from database - NO sample data)
        // =========================================================
        private void LoadUnassignedBags()
        {
            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();

                    // Check if table exists first
                    string checkTable = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'BloodBags'";
                    using (var checkCmd = new SqlCommand(checkTable, conn))
                    {
                        int tableExists = Convert.ToInt32(checkCmd.ExecuteScalar());
                        if (tableExists == 0)
                        {
                            dgvUnassignedBags.DataSource = null;
                            if (lblUnassignedCount != null)
                                lblUnassignedCount.Text = "⚠️ Table not found";
                            return;
                        }
                    }

                    string query = @"
                        SELECT 
                            BagID,
                            ISNULL(BloodGroup, 'N/A') as BloodGroup,
                            FORMAT(CollectionDate, 'dd-MMM-yyyy') as CollectionDate,
                            Status,
                            ISNULL(DonorName, 'N/A') as Donor
                        FROM BloodBags 
                        WHERE Status IN ('Available', 'In Lab')
                        ORDER BY CollectionDate DESC";

                    SqlDataAdapter da = new SqlDataAdapter(query, conn);
                    unassignedData = new DataTable();
                    da.Fill(unassignedData);

                    // Directly show database data - NO modification, NO sample data
                    dgvUnassignedBags.DataSource = unassignedData;

                    FormatUnassignedGrid();
                    NoSort(dgvUnassignedBags);

                    // Update count label
                    if (lblUnassignedCount != null)
                    {
                        if (unassignedData.Rows.Count == 0)
                            lblUnassignedCount.Text = "(No unassigned bags found)";
                        else
                            lblUnassignedCount.Text = $"({unassignedData.Rows.Count} bags waiting for assignment)";
                    }

                    if (txtSearch != null)
                        txtSearch.Text = "";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LoadUnassignedBags Error: {ex.Message}");
                dgvUnassignedBags.DataSource = null;
                if (lblUnassignedCount != null)
                    lblUnassignedCount.Text = "❌ Error loading data";

                MessageBox.Show("Error loading bags from database: " + ex.Message,
                    "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // =========================================================
        // DATABASE: Load assignment history (ONLY from database - NO sample data)
        // =========================================================
        private void LoadAssignmentHistory()
        {
            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();

                    // Check if table exists
                    string checkTable = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'BloodBags'";
                    using (var checkCmd = new SqlCommand(checkTable, conn))
                    {
                        int tableExists = Convert.ToInt32(checkCmd.ExecuteScalar());
                        if (tableExists == 0)
                        {
                            dgvAssignmentHistory.DataSource = null;
                            if (lblHistoryCount != null)
                                lblHistoryCount.Text = "⚠️ Table not found";
                            return;
                        }
                    }

                    string query = @"
                        SELECT 
                            BagID,
                            ISNULL(BloodGroup, 'N/A') as BloodGroup,
                            ISNULL(StorageLocation, 'Not Assigned') as StorageLocation,
                            Status,
                            ISNULL(DonorName, 'N/A') as Donor,
                            FORMAT(CollectionDate, 'dd-MMM-yyyy') as CollectionDate,
                            ISNULL(Remarks, '') as Remarks
                        FROM BloodBags 
                        WHERE StorageLocation IS NOT NULL 
                        AND StorageLocation != ''
                        ORDER BY CollectionDate DESC";

                    SqlDataAdapter da = new SqlDataAdapter(query, conn);
                    historyData = new DataTable();
                    da.Fill(historyData);

                    // Directly show database data - NO modification, NO sample data
                    dgvAssignmentHistory.DataSource = historyData;
                    FormatHistoryGrid();
                    NoSort(dgvAssignmentHistory);

                    // Update count label
                    if (lblHistoryCount != null)
                    {
                        if (historyData.Rows.Count == 0)
                            lblHistoryCount.Text = "(No assignment history found)";
                        else
                            lblHistoryCount.Text = $"({historyData.Rows.Count} records)";
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LoadAssignmentHistory Error: {ex.Message}");
                // Show NOTHING - no sample data
                dgvAssignmentHistory.DataSource = null;
                if (lblHistoryCount != null)
                    lblHistoryCount.Text = "❌ Error loading history";
            }
        }

        private void FormatUnassignedGrid()
        {
            if (dgvUnassignedBags.Columns.Count == 0) return;

            if (dgvUnassignedBags.Columns.Contains("BagID"))
            {
                dgvUnassignedBags.Columns["BagID"].HeaderText = "Bag ID";
                dgvUnassignedBags.Columns["BagID"].FillWeight = 25;
            }
            if (dgvUnassignedBags.Columns.Contains("BloodGroup"))
            {
                dgvUnassignedBags.Columns["BloodGroup"].HeaderText = "Blood Group";
                dgvUnassignedBags.Columns["BloodGroup"].FillWeight = 15;
            }
            if (dgvUnassignedBags.Columns.Contains("CollectionDate"))
            {
                dgvUnassignedBags.Columns["CollectionDate"].HeaderText = "Collection Date";
                dgvUnassignedBags.Columns["CollectionDate"].FillWeight = 20;
            }
            if (dgvUnassignedBags.Columns.Contains("Status"))
            {
                dgvUnassignedBags.Columns["Status"].HeaderText = "Status";
                dgvUnassignedBags.Columns["Status"].FillWeight = 15;
            }
            if (dgvUnassignedBags.Columns.Contains("Donor"))
            {
                dgvUnassignedBags.Columns["Donor"].HeaderText = "Donor Name";
                dgvUnassignedBags.Columns["Donor"].FillWeight = 25;
            }
        }

        private void FormatHistoryGrid()
        {
            if (dgvAssignmentHistory.Columns.Count == 0) return;

            if (dgvAssignmentHistory.Columns.Contains("BagID"))
            {
                dgvAssignmentHistory.Columns["BagID"].HeaderText = "Bag ID";
                dgvAssignmentHistory.Columns["BagID"].FillWeight = 20;
            }
            if (dgvAssignmentHistory.Columns.Contains("BloodGroup"))
            {
                dgvAssignmentHistory.Columns["BloodGroup"].HeaderText = "Blood Group";
                dgvAssignmentHistory.Columns["BloodGroup"].FillWeight = 10;
            }
            if (dgvAssignmentHistory.Columns.Contains("StorageLocation"))
            {
                dgvAssignmentHistory.Columns["StorageLocation"].HeaderText = "Storage Location";
                dgvAssignmentHistory.Columns["StorageLocation"].FillWeight = 20;
            }
            if (dgvAssignmentHistory.Columns.Contains("Status"))
            {
                dgvAssignmentHistory.Columns["Status"].HeaderText = "Status";
                dgvAssignmentHistory.Columns["Status"].FillWeight = 12;
            }
            if (dgvAssignmentHistory.Columns.Contains("Donor"))
            {
                dgvAssignmentHistory.Columns["Donor"].HeaderText = "Donor Name";
                dgvAssignmentHistory.Columns["Donor"].FillWeight = 18;
            }
            if (dgvAssignmentHistory.Columns.Contains("CollectionDate"))
            {
                dgvAssignmentHistory.Columns["CollectionDate"].HeaderText = "Collection Date";
                dgvAssignmentHistory.Columns["CollectionDate"].FillWeight = 12;
            }
            if (dgvAssignmentHistory.Columns.Contains("Remarks"))
            {
                dgvAssignmentHistory.Columns["Remarks"].HeaderText = "Remarks";
                dgvAssignmentHistory.Columns["Remarks"].FillWeight = 8;
            }
        }

        private void FilterBags()
        {
            if (unassignedData == null) return;

            string filter = txtSearch.Text.Trim();
            if (string.IsNullOrEmpty(filter))
            {
                dgvUnassignedBags.DataSource = unassignedData;
            }
            else
            {
                DataTable filtered = unassignedData.Clone();
                foreach (DataRow row in unassignedData.Rows)
                {
                    string bagId = row["BagID"].ToString();
                    string donor = row["Donor"].ToString();
                    if (bagId.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0 ||
                        donor.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        filtered.ImportRow(row);
                    }
                }
                dgvUnassignedBags.DataSource = filtered;
            }
        }

        private void DgvUnassignedBags_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvUnassignedBags.SelectedRows.Count > 0 && dgvUnassignedBags.SelectedRows[0].DataBoundItem != null)
            {
                DataRowView row = dgvUnassignedBags.SelectedRows[0].DataBoundItem as DataRowView;
                if (row != null)
                {
                    txtBagId.Text = row["BagID"].ToString();

                    // Auto-fill storage location suggestion based on blood group
                    string bloodGroup = row["BloodGroup"].ToString();
                    txtStorageLocation.Text = GetSuggestedLocation(bloodGroup);
                    txtStorageLocation.ForeColor = C_TextDark;
                }
            }
        }

        private string GetSuggestedLocation(string bloodGroup)
        {
            switch (bloodGroup)
            {
                case "A+": return "Rack A-1 (Section A)";
                case "A-": return "Rack A-2 (Section B)";
                case "B+": return "Rack B-1 (Section C)";
                case "B-": return "Rack B-2 (Section D)";
                case "AB+": return "Rack AB-1 (Section E)";
                case "AB-": return "Rack AB-2 (Section F)";
                case "O+": return "Rack O-1 (Section G)";
                case "O-": return "Rack O-2 (Section H - Universal)";
                default: return "Rack X (General Storage)";
            }
        }

        // =========================================================
        // DATABASE: Update storage location
        // =========================================================
        private async void BtnAssign_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtBagId.Text))
            {
                MessageBox.Show("Please select a bag from the list.", "No Bag Selected",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string location = txtStorageLocation.Text;
            if (string.IsNullOrWhiteSpace(location) || location == "e.g., Rack A-12, Shelf 3")
            {
                MessageBox.Show("Please enter a valid storage location.", "Location Required",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Check if bag exists in database before updating
            bool bagExists = await CheckBagExistsAsync(txtBagId.Text);
            if (!bagExists)
            {
                MessageBox.Show($"Bag ID '{txtBagId.Text}' does not exist in the system.",
                    "Bag Not Found", MessageBoxButtons.OK, MessageBoxIcon.Error);
                LoadUnassignedBags();
                return;
            }

            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    await conn.OpenAsync();

                    string query = @"
                        UPDATE BloodBags 
                        SET StorageLocation = @Location, 
                            Status = 'Available',
                            Remarks = ISNULL(Remarks, '') + CHAR(10) + @Remarks
                        WHERE BagID = @BagID";

                    using (var cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Location", location);
                        cmd.Parameters.AddWithValue("@Remarks", $"[{DateTime.Now:dd-MMM-yyyy HH:mm}] {txtNotes.Text.Trim()}");
                        cmd.Parameters.AddWithValue("@BagID", txtBagId.Text);

                        int rowsAffected = await cmd.ExecuteNonQueryAsync();

                        if (rowsAffected > 0)
                        {
                            MessageBox.Show($"✅ Bag {txtBagId.Text} assigned to location: {location}\n\nNotes: {txtNotes.Text}",
                                "Assignment Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);

                            // Refresh both grids from database
                            LoadUnassignedBags();
                            LoadAssignmentHistory();

                            // Clear form
                            txtBagId.Clear();
                            txtStorageLocation.Text = "e.g., Rack A-12, Shelf 3";
                            txtStorageLocation.ForeColor = Color.Gray;
                            txtNotes.Clear();
                            txtSearch.Clear();
                        }
                        else
                        {
                            MessageBox.Show("Failed to update bag location. Please try again.",
                                "Update Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"BtnAssign_Click Error: {ex.Message}");
                MessageBox.Show($"Error updating database: {ex.Message}", "Database Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // =========================================================
        // Helper: Check if bag exists in database
        // =========================================================
        private async Task<bool> CheckBagExistsAsync(string bagId)
        {
            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    await conn.OpenAsync();
                    string query = "SELECT COUNT(*) FROM BloodBags WHERE BagID = @BagID";
                    using (var cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@BagID", bagId);
                        int count = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                        return count > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"CheckBagExistsAsync Error: {ex.Message}");
                return false;
            }
        }

        private DataGridView CreateStyledGrid()
        {
            var dgv = new DataGridView
            {
                AllowUserToAddRows = false,
                ReadOnly = true,
                BackgroundColor = C_White,
                BorderStyle = BorderStyle.None,
                RowHeadersVisible = false,
                EnableHeadersVisualStyles = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                GridColor = C_Border,
                RowTemplate = { Height = 42 },
                ScrollBars = ScrollBars.Both,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            dgv.ColumnHeadersDefaultCellStyle.BackColor = brickRed;
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = C_White;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            dgv.ColumnHeadersHeight = 45;
            dgv.DefaultCellStyle.Font = new Font("Segoe UI", 9f);
            dgv.DefaultCellStyle.SelectionBackColor = brickRedLight;
            dgv.DefaultCellStyle.SelectionForeColor = brickRed;
            dgv.DefaultCellStyle.Padding = new Padding(10, 5, 10, 5);
            dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(252, 252, 252);

            // Color code status cells
            dgv.CellFormatting += (s, e) =>
            {
                if (e.RowIndex >= 0 && e.ColumnIndex >= 0 && dgv.Columns[e.ColumnIndex].HeaderText == "Status" && e.Value != null)
                {
                    string status = e.Value.ToString();
                    if (status == "Available")
                        e.CellStyle.ForeColor = Color.FromArgb(16, 185, 129);
                    else if (status == "In Lab")
                        e.CellStyle.ForeColor = Color.FromArgb(245, 158, 11);
                    else if (status == "Issued")
                        e.CellStyle.ForeColor = Color.FromArgb(59, 130, 246);
                }
            };

            return dgv;
        }

        private void NoSort(DataGridView dgv)
        {
            if (dgv.Columns != null)
            {
                foreach (DataGridViewColumn col in dgv.Columns)
                    col.SortMode = DataGridViewColumnSortMode.NotSortable;
            }
        }
    }
}