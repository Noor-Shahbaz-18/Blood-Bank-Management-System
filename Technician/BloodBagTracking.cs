using BloodBankManagementSystem.Classes.Database;
using BloodBankManagementSystem.Classes.Helpers;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BloodBankManagementSystem.Forms.Technician
{
    public partial class BloodBagTracking : BaseTechnicianForm
    {
        private DataGridView dgvBloodBags;
        private TextBox txtSearchBagID, txtSearchDonor, txtSearchLocation;
        private ComboBox cmbSearchStatus, cmbSearchBloodGroup, cmbSearchComponentType;
        private Button btnSearch, btnReset, btnRefresh, btnExport;
        private Panel pnlFilterCard;
        private DataTable originalData;
        private Label lblStatus, lblTotalCount;

        public BloodBagTracking()
        {
            this.Text = "Blood Bank Management System – Blood Bag Tracking";
            BuildLayout();
            BuildSidebar("Bag Tracking");
            BuildTopBar("Blood Bag Tracking");
            BuildContentArea();
            LoadBloodBagsFromDatabase();
        }

        private void BuildContentArea()
        {
            int y = 20;

            // ── Status label (top-right) ─────────────────────────────
            lblStatus = new Label
            {
                Text = "✅ Loading data...",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(16, 185, 129),
                Location = new Point(pnlContent.Width - 320, y),
                AutoSize = true,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            pnlContent.Controls.Add(lblStatus);

            // ── Filter Card ──────────────────────────────────────────
            // Height increased to 150 so two rows + buttons fit comfortably
            pnlFilterCard = new Panel
            {
                Location = new Point(0, y + 25),
                Width = pnlContent.ClientSize.Width - 40,
                Height = 150,
                BackColor = C_CardBg,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            SetRoundedRegion(pnlFilterCard, 12);
            AddDropShadow(pnlFilterCard);
            pnlContent.Controls.Add(pnlFilterCard);

            pnlFilterCard.Controls.Add(new Label
            {
                Text = "🔍 Filter & Search",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = C_TextDark,
                Location = new Point(15, 10),
                AutoSize = true
            });

            // ── Row 1: Bag ID | Donor Name | Status ─────────────────
            int row1Y = 40;

            // Bag ID
            pnlFilterCard.Controls.Add(MakeLabel("Bag ID:", new Point(15, row1Y)));
            txtSearchBagID = new TextBox
            {
                Location = new Point(65, row1Y - 2),
                Width = 110,
                Font = new Font("Segoe UI", 9),
                BorderStyle = BorderStyle.FixedSingle
            };
            pnlFilterCard.Controls.Add(txtSearchBagID);

            // Donor Name
            pnlFilterCard.Controls.Add(MakeLabel("Donor:", new Point(190, row1Y)));
            txtSearchDonor = new TextBox
            {
                Location = new Point(240, row1Y - 2),
                Width = 130,
                Font = new Font("Segoe UI", 9),
                BorderStyle = BorderStyle.FixedSingle
            };
            pnlFilterCard.Controls.Add(txtSearchDonor);

            // Status
            pnlFilterCard.Controls.Add(MakeLabel("Status:", new Point(385, row1Y)));
            cmbSearchStatus = new ComboBox
            {
                Location = new Point(435, row1Y - 2),
                Width = 120,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9)
            };
            cmbSearchStatus.Items.AddRange(new[] { "All", "Available", "In Lab", "Issued", "Expired", "Discarded", "Quarantined" });
            cmbSearchStatus.SelectedIndex = 0;
            pnlFilterCard.Controls.Add(cmbSearchStatus);

            // ── Row 2: Blood Group | Component | Location ────────────
            int row2Y = 80;

            // Blood Group
            pnlFilterCard.Controls.Add(MakeLabel("Blood Group:", new Point(15, row2Y)));
            cmbSearchBloodGroup = new ComboBox
            {
                Location = new Point(100, row2Y - 2),
                Width = 90,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9)
            };
            cmbSearchBloodGroup.Items.AddRange(new[] { "All", "A+", "A-", "B+", "B-", "AB+", "AB-", "O+", "O-" });
            cmbSearchBloodGroup.SelectedIndex = 0;
            pnlFilterCard.Controls.Add(cmbSearchBloodGroup);

            // Component
            pnlFilterCard.Controls.Add(MakeLabel("Component:", new Point(205, row2Y)));
            cmbSearchComponentType = new ComboBox
            {
                Location = new Point(285, row2Y - 2),
                Width = 115,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9)
            };
            cmbSearchComponentType.Items.AddRange(new[] { "All", "Whole Blood", "Plasma", "Red Cells", "Platelets", "Cryoprecipitate" });
            cmbSearchComponentType.SelectedIndex = 0;
            pnlFilterCard.Controls.Add(cmbSearchComponentType);

            // Location
            pnlFilterCard.Controls.Add(MakeLabel("Location:", new Point(415, row2Y)));
            txtSearchLocation = new TextBox
            {
                Location = new Point(475, row2Y - 2),
                Width = 130,
                Font = new Font("Segoe UI", 9),
                BorderStyle = BorderStyle.FixedSingle
            };
            pnlFilterCard.Controls.Add(txtSearchLocation);

            // ── Row 3: Buttons ────────────────────────────────────────
            // Fixed: buttons placed on their own row so they never overlap fields
            int row3Y = 110;
            int btnRight = pnlFilterCard.Width - 15;

            // Reset  (rightmost)
            btnReset = MakeButton("⟳ Reset", Color.FromArgb(100, 110, 125));
            btnReset.Size = new Size(85, 30);
            btnReset.Location = new Point(btnRight - 85, row3Y);
            btnReset.Click += BtnReset_Click;
            pnlFilterCard.Controls.Add(btnReset);

            // Search
            btnSearch = MakeButton("🔍 Search", brickRed);
            btnSearch.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            btnSearch.Size = new Size(90, 30);
            btnSearch.Location = new Point(btnRight - 85 - 100, row3Y);
            btnSearch.Click += BtnSearch_Click;
            pnlFilterCard.Controls.Add(btnSearch);

            // Refresh
            btnRefresh = MakeButton("🔄 Refresh", Color.FromArgb(60, 70, 80));
            btnRefresh.Size = new Size(95, 30);
            btnRefresh.Location = new Point(btnRight - 85 - 100 - 105, row3Y);
            btnRefresh.Click += (s, e) => LoadBloodBagsFromDatabase();
            pnlFilterCard.Controls.Add(btnRefresh);

            // Export
            btnExport = MakeButton("📊 Export", Color.FromArgb(16, 185, 129));
            btnExport.Size = new Size(90, 30);
            btnExport.Location = new Point(btnRight - 85 - 100 - 105 - 100, row3Y);
            btnExport.Click += BtnExport_Click;
            pnlFilterCard.Controls.Add(btnExport);

            // ── Inventory header ──────────────────────────────────────
            y += pnlFilterCard.Height + 20;

            Panel headerPanel = new Panel
            {
                Location = new Point(0, y),
                Width = pnlContent.ClientSize.Width - 40,
                Height = 32,
                BackColor = Color.Transparent
            };
            pnlContent.Controls.Add(headerPanel);

            headerPanel.Controls.Add(new Label
            {
                Text = "📋 Blood Bags Inventory",
                Font = new Font("Segoe UI", 13f, FontStyle.Bold),
                ForeColor = C_TextDark,
                Location = new Point(0, 4),
                AutoSize = true
            });

            lblTotalCount = new Label
            {
                Text = "",
                Font = new Font("Segoe UI", 9),
                ForeColor = C_TextMid,
                Location = new Point(210, 8),
                AutoSize = true
            };
            headerPanel.Controls.Add(lblTotalCount);
            y += 32;

            // ── DataGridView ──────────────────────────────────────────
            dgvBloodBags = CreateStyledGrid();
            dgvBloodBags.RowPrePaint += DgvBloodBags_RowPrePaint;
            WrapInCard(dgvBloodBags, pnlContent, new Point(0, y), 420);
            FixDataGridView(dgvBloodBags);

            pnlContent.Controls.Add(new Panel
            {
                Height = 30,
                BackColor = Color.Transparent,
                Dock = DockStyle.Bottom
            });

            pnlContent.Resize += (s, e) =>
            {
                if (lblStatus != null)
                    lblStatus.Location = new Point(pnlContent.Width - 320, 20);
            };
        }

        // ── Small helpers to reduce repetition ───────────────────────
        private Label MakeLabel(string text, Point loc)
        {
            return new Label
            {
                Text = text,
                Font = new Font("Segoe UI", 9),
                ForeColor = C_TextMid,
                Location = loc,
                AutoSize = true
            };
        }

        private Button MakeButton(string text, Color backColor)
        {
            var btn = new Button
            {
                Text = text,
                BackColor = backColor,
                ForeColor = C_White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9),
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            SetRoundedRegion(btn, 6);
            return btn;
        }

        // =========================================================
        // LOAD FROM DATABASE
        // =========================================================
        private async void LoadBloodBagsFromDatabase()
        {
            try
            {
                UpdateStatus("🔄 Loading data from database...", Color.FromArgb(59, 130, 246));

                using (var conn = DatabaseHelper.GetConnection())
                {
                    await conn.OpenAsync();

                    string checkTable = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'BloodBags'";
                    using (var checkCmd = new SqlCommand(checkTable, conn))
                    {
                        int tableExists = Convert.ToInt32(await checkCmd.ExecuteScalarAsync());
                        if (tableExists == 0)
                        {
                            dgvBloodBags.DataSource = null;
                            UpdateStatus("⚠️ Table 'BloodBags' not found in database", Color.FromArgb(245, 158, 11));
                            if (lblTotalCount != null) lblTotalCount.Text = "Table not found";
                            return;
                        }
                    }

                    string query = @"
                        SELECT
                            BagID,
                            ISNULL(DonorName,      'N/A')          AS DonorName,
                            BloodGroup,
                            ComponentType,
                            Volume,
                            FORMAT(CollectionDate, 'dd-MMM-yyyy')  AS CollectionDate,
                            FORMAT(ExpiryDate,     'dd-MMM-yyyy')  AS ExpiryDate,
                            DATEDIFF(DAY, GETDATE(), ExpiryDate)   AS DaysLeft,
                            Status,
                            ISNULL(StorageLocation,'Not Assigned') AS Location,
                            ISNULL(Remarks,        '')              AS Remarks
                        FROM BloodBags
                        ORDER BY CollectionDate DESC, BagID DESC";

                    SqlDataAdapter da = new SqlDataAdapter(query, conn);
                    originalData = new DataTable();
                    da.Fill(originalData);

                    dgvBloodBags.DataSource = originalData;
                    FormatGridColumns();
                    NoSort(dgvBloodBags);

                    if (lblTotalCount != null)
                        lblTotalCount.Text = originalData.Rows.Count == 0
                            ? "(No records found)"
                            : $"({originalData.Rows.Count} bags total)";

                    UpdateStatus($"✅ Loaded {originalData.Rows.Count} bag(s) from database",
                                 Color.FromArgb(16, 185, 129));
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LoadBloodBagsFromDatabase Error: {ex.Message}");
                dgvBloodBags.DataSource = null;
                UpdateStatus($"❌ Database error: {ex.Message}", Color.FromArgb(220, 38, 38));
                if (lblTotalCount != null) lblTotalCount.Text = "Error loading data";
            }
        }

        // ── Column formatting ─────────────────────────────────────────
        // KEY FIX: AutoSizeColumnsMode = Fill + explicit FillWeight values
        // so every column gets proportional space and nothing is cut off.
        private void FormatGridColumns()
        {
            if (dgvBloodBags.Columns.Count == 0) return;

            // Make sure scrollbar is visible for wide tables
            dgvBloodBags.ScrollBars = ScrollBars.Both;
            dgvBloodBags.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            void Col(string name, string header, int weight)
            {
                if (!dgvBloodBags.Columns.Contains(name)) return;
                dgvBloodBags.Columns[name].HeaderText = header;
                dgvBloodBags.Columns[name].FillWeight = weight;
                dgvBloodBags.Columns[name].MinimumWidth = 60;
                // Centre-align short columns
                if (weight <= 7)
                    dgvBloodBags.Columns[name].DefaultCellStyle.Alignment =
                        DataGridViewContentAlignment.MiddleCenter;
            }

            Col("BagID", "Bag ID", 10);
            Col("DonorName", "Donor Name", 14);
            Col("BloodGroup", "Blood Group", 7);
            Col("ComponentType", "Component", 11);
            Col("Volume", "Vol (mL)", 7);
            Col("CollectionDate", "Collection Date", 11);
            Col("ExpiryDate", "Expiry Date", 11);
            Col("DaysLeft", "Days Left", 7);
            Col("Status", "Status", 10);
            Col("Location", "Location", 12);
            Col("Remarks", "Remarks", 10);
        }

        private void UpdateStatus(string message, Color color)
        {
            if (lblStatus != null && !lblStatus.IsDisposed)
            {
                lblStatus.Text = message;
                lblStatus.ForeColor = color;
            }
        }

        // =========================================================
        // SEARCH / FILTER
        // =========================================================
        private void BtnSearch_Click(object sender, EventArgs e)
        {
            if (originalData == null) return;

            DataTable filteredDt = originalData.Clone();
            string bagID = txtSearchBagID.Text.Trim().ToLower();
            string donor = txtSearchDonor.Text.Trim().ToLower();
            string location = txtSearchLocation.Text.Trim().ToLower();
            string status = cmbSearchStatus.SelectedItem?.ToString();
            string bloodGroup = cmbSearchBloodGroup.SelectedItem?.ToString();
            string componentType = cmbSearchComponentType.SelectedItem?.ToString();

            foreach (DataRow row in originalData.Rows)
            {
                bool match = true;

                if (!string.IsNullOrEmpty(bagID)
                    && !row["BagID"]?.ToString().ToLower().Contains(bagID) == true) match = false;

                if (match && !string.IsNullOrEmpty(donor)
                    && !row["DonorName"]?.ToString().ToLower().Contains(donor) == true) match = false;

                if (match && !string.IsNullOrEmpty(location)
                    && !row["Location"]?.ToString().ToLower().Contains(location) == true) match = false;

                if (match && status != "All" && !string.IsNullOrEmpty(status)
                    && row["Status"]?.ToString() != status) match = false;

                if (match && bloodGroup != "All" && !string.IsNullOrEmpty(bloodGroup)
                    && row["BloodGroup"]?.ToString() != bloodGroup) match = false;

                if (match && componentType != "All" && !string.IsNullOrEmpty(componentType)
                    && row["ComponentType"]?.ToString() != componentType) match = false;

                if (match) filteredDt.ImportRow(row);
            }

            dgvBloodBags.DataSource = filteredDt;

            if (lblTotalCount != null)
                lblTotalCount.Text = $"({filteredDt.Rows.Count} of {originalData.Rows.Count} bags)";

            UpdateStatus($"🔍 Found {filteredDt.Rows.Count} matching bag(s)",
                         Color.FromArgb(59, 130, 246));
        }

        private void BtnReset_Click(object sender, EventArgs e)
        {
            txtSearchBagID.Clear();
            txtSearchDonor.Clear();
            txtSearchLocation.Clear();
            cmbSearchStatus.SelectedIndex = 0;
            cmbSearchBloodGroup.SelectedIndex = 0;
            cmbSearchComponentType.SelectedIndex = 0;

            dgvBloodBags.DataSource = originalData?.Copy();

            if (lblTotalCount != null && originalData != null)
                lblTotalCount.Text = $"({originalData.Rows.Count} bags total)";

            UpdateStatus("✅ Filters reset", Color.FromArgb(16, 185, 129));
        }

        // =========================================================
        // EXPORT
        // =========================================================
        private void BtnExport_Click(object sender, EventArgs e)
        {
            try
            {
                var sfd = new SaveFileDialog
                {
                    Filter = "CSV files (*.csv)|*.csv|Excel files (*.xls)|*.xls",
                    DefaultExt = "csv",
                    FileName = $"BloodBags_Tracking_{DateTime.Now:yyyyMMdd_HHmmss}"
                };

                if (sfd.ShowDialog() != DialogResult.OK) return;

                DataTable exportData = dgvBloodBags.DataSource as DataTable;
                if (exportData == null || exportData.Rows.Count == 0)
                {
                    MessageBox.Show("No data to export.", "Export",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                if (sfd.FileName.EndsWith(".csv"))
                    ExportToCSV(exportData, sfd.FileName);
                else
                    ExportToExcel(exportData, sfd.FileName);

                MessageBox.Show($"Data exported successfully to:\n{sfd.FileName}",
                    "Export Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Export failed: {ex.Message}", "Export Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ExportToCSV(DataTable dt, string filePath)
        {
            using (var sw = new System.IO.StreamWriter(filePath))
            {
                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    sw.Write(dt.Columns[i].ColumnName);
                    if (i < dt.Columns.Count - 1) sw.Write(",");
                }
                sw.WriteLine();

                foreach (DataRow row in dt.Rows)
                {
                    for (int i = 0; i < dt.Columns.Count; i++)
                    {
                        sw.Write(row[i].ToString().Replace(",", ";"));
                        if (i < dt.Columns.Count - 1) sw.Write(",");
                    }
                    sw.WriteLine();
                }
            }
        }

        private void ExportToExcel(DataTable dt, string filePath)
        {
            var html = new StringBuilder();
            html.AppendLine("<html><head><meta charset='UTF-8'></head><body>");
            html.AppendLine("<table border='1' cellpadding='5' cellspacing='0' style='border-collapse:collapse;'>");
            html.AppendLine("<tr style='background-color:#78161b; color:white;'>");
            foreach (DataColumn col in dt.Columns)
                html.AppendLine($"<th>{col.ColumnName}</th>");
            html.AppendLine("</tr>");
            foreach (DataRow row in dt.Rows)
            {
                html.AppendLine("<tr>");
                foreach (DataColumn col in dt.Columns)
                    html.AppendLine($"<td>{row[col]}</td>");
                html.AppendLine("</tr>");
            }
            html.AppendLine("</table>");
            html.AppendLine($"<p>Generated on: {DateTime.Now:dd-MMM-yyyy HH:mm:ss}</p>");
            html.AppendLine("</body></html>");
            System.IO.File.WriteAllText(filePath, html.ToString());
        }

        // =========================================================
        // ROW COLORING
        // =========================================================
        private void DgvBloodBags_RowPrePaint(object sender, DataGridViewRowPrePaintEventArgs e)
        {
            if (e.RowIndex < 0) return;
            if (!dgvBloodBags.Columns.Contains("Status")) return;

            string status = dgvBloodBags.Rows[e.RowIndex].Cells["Status"].Value?.ToString() ?? "";

            int daysLeft = 999;
            if (dgvBloodBags.Columns.Contains("DaysLeft"))
                int.TryParse(dgvBloodBags.Rows[e.RowIndex].Cells["DaysLeft"].Value?.ToString(), out daysLeft);

            Color bg = Color.Empty;

            switch (status)
            {
                case "Available":
                    bg = (daysLeft <= 7 && daysLeft > 0)
                        ? Color.FromArgb(255, 245, 200)   // yellow warning
                        : Color.FromArgb(220, 240, 220);  // light green
                    break;
                case "Expired":
                    bg = Color.FromArgb(255, 220, 220); break;  // light red
                case "Discarded":
                    bg = Color.FromArgb(230, 230, 230); break;  // gray
                case "In Lab":
                    bg = Color.FromArgb(255, 245, 220); break;  // light orange
                case "Issued":
                    bg = Color.FromArgb(220, 235, 255); break;  // light blue
                case "Quarantined":
                    bg = Color.FromArgb(255, 200, 200); break;  // pink-red
                default:
                    if (daysLeft < 0)
                        bg = Color.FromArgb(255, 220, 220);
                    break;
            }

            if (bg != Color.Empty)
                dgvBloodBags.Rows[e.RowIndex].DefaultCellStyle.BackColor = bg;
        }

        // =========================================================
        // HELPERS
        // =========================================================
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
                RowTemplate = { Height = 40 },
                ScrollBars = ScrollBars.Both,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                // KEY FIX: clip content instead of expanding columns
                Dock = DockStyle.None
            };

            dgv.ColumnHeadersDefaultCellStyle.BackColor = brickRed;
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = C_White;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            dgv.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgv.ColumnHeadersHeight = 42;

            dgv.DefaultCellStyle.Font = new Font("Segoe UI", 8.5f);
            dgv.DefaultCellStyle.SelectionBackColor = brickRedLight;
            dgv.DefaultCellStyle.SelectionForeColor = brickRed;
            dgv.DefaultCellStyle.Padding = new Padding(6, 4, 6, 4);

            dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(252, 252, 252);
            return dgv;
        }

        private void NoSort(DataGridView dgv)
        {
            foreach (DataGridViewColumn col in dgv.Columns)
                col.SortMode = DataGridViewColumnSortMode.NotSortable;
        }
    }
}