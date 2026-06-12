using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;
using BloodBankManagementSystem.Classes.Database;
using BloodBankManagementSystem.Classes.Helpers;

namespace BloodBankManagementSystem.Forms.Technician
{
    public partial class ExpiryAlerts : BaseTechnicianForm
    {
        private DataGridView dgvExpiryAlerts;
        private ComboBox cmbAlertType, cmbBloodGroup, cmbComponentType;
        private Button btnRefresh, btnExport;
        private Label lblTotalBags, lblExpiringSoon, lblExpired, lblTotalUnits, lblStatus;
        private Panel pnlFilterCard;
        private Panel statsPanel;
        private Panel card1, card2, card3, card4;
        private DataTable originalData;
        private Timer refreshTimer;

        public ExpiryAlerts()
        {
            this.Text = "Blood Bank Management System – Expiry Alerts";
            BuildLayout();
            BuildSidebar("Expiry Alerts");
            BuildTopBar("Expiry Alerts");
            BuildContentArea();

            LoadExpiryDataFromDatabase();

            refreshTimer = new Timer { Interval = 300000 };
            refreshTimer.Tick += (s, e) => LoadExpiryDataFromDatabase();
            refreshTimer.Start();
        }

        private void BuildContentArea()
        {
            const int MX = 20;
            int y = 20;

            // Status label (top-right)
            lblStatus = new Label
            {
                Text = "✅ Loading data...",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(16, 185, 129),
                AutoSize = true,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            pnlContent.Controls.Add(lblStatus);
            // Position after layout
            pnlContent.Resize += (s, e) =>
            {
                if (lblStatus != null && !lblStatus.IsDisposed)
                    lblStatus.Location = new Point(pnlContent.ClientSize.Width - lblStatus.Width - MX, 20);
            };

            // ── Stat Cards ──────────────────────────────────────────
            statsPanel = new Panel
            {
                Location = new Point(MX, y),
                Height = 90,
                BackColor = Color.Transparent,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            pnlContent.Controls.Add(statsPanel);

            card1 = CreateStatCard("Total Bags", "0", brickRed, 0, 0, 90);
            card2 = CreateStatCard("Expiring Soon", "0", Color.FromArgb(245, 158, 11), 0, 0, 90);
            card3 = CreateStatCard("Expired", "0", Color.FromArgb(220, 38, 38), 0, 0, 90);
            card4 = CreateStatCard("Total Units", "0", Color.FromArgb(16, 185, 129), 0, 0, 90);

            lblTotalBags = card1.Controls[2] as Label;
            lblExpiringSoon = card2.Controls[2] as Label;
            lblExpired = card3.Controls[2] as Label;
            lblTotalUnits = card4.Controls[2] as Label;

            statsPanel.Controls.Add(card1);
            statsPanel.Controls.Add(card2);
            statsPanel.Controls.Add(card3);
            statsPanel.Controls.Add(card4);

            y += statsPanel.Height + 16;

            // ── Filter Card ─────────────────────────────────────────
            pnlFilterCard = new Panel
            {
                Location = new Point(MX, y),
                Height = 90,
                Width = 900,   // large default; Shown + Resize will correct it
                BackColor = C_CardBg
            };
            SetRoundedRegion(pnlFilterCard, 12);
            AddDropShadow(pnlFilterCard);
            pnlContent.Controls.Add(pnlFilterCard);

            pnlFilterCard.Controls.Add(new Label
            {
                Text = "🔍 Filter Alerts",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = C_TextDark,
                Location = new Point(15, 12),
                AutoSize = true
            });

            int filterY = 48;

            // Alert Type
            pnlFilterCard.Controls.Add(new Label { Text = "Alert Type:", Font = new Font("Segoe UI", 9), ForeColor = C_TextMid, Location = new Point(15, filterY + 3), AutoSize = true });
            cmbAlertType = new ComboBox { Location = new Point(90, filterY), Width = 155, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 9) };
            cmbAlertType.Items.AddRange(new[] { "All", "Expiring Soon (≤7 days)", "Expired", "Normal (>7 days)" });
            cmbAlertType.SelectedIndex = 0;
            cmbAlertType.SelectedIndexChanged += (s, e) => ApplyFilter();
            pnlFilterCard.Controls.Add(cmbAlertType);

            // Blood Group
            pnlFilterCard.Controls.Add(new Label { Text = "Blood Group:", Font = new Font("Segoe UI", 9), ForeColor = C_TextMid, Location = new Point(265, filterY + 3), AutoSize = true });
            cmbBloodGroup = new ComboBox { Location = new Point(350, filterY), Width = 95, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 9) };
            cmbBloodGroup.Items.AddRange(new[] { "All", "A+", "A-", "B+", "B-", "AB+", "AB-", "O+", "O-" });
            cmbBloodGroup.SelectedIndex = 0;
            cmbBloodGroup.SelectedIndexChanged += (s, e) => ApplyFilter();
            pnlFilterCard.Controls.Add(cmbBloodGroup);

            // Component
            pnlFilterCard.Controls.Add(new Label { Text = "Component:", Font = new Font("Segoe UI", 9), ForeColor = C_TextMid, Location = new Point(465, filterY + 3), AutoSize = true });
            cmbComponentType = new ComboBox { Location = new Point(545, filterY), Width = 130, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 9) };
            cmbComponentType.Items.AddRange(new[] { "All", "Whole Blood", "Plasma", "Red Cells", "Platelets", "Cryoprecipitate" });
            cmbComponentType.SelectedIndex = 0;
            cmbComponentType.SelectedIndexChanged += (s, e) => ApplyFilter();
            pnlFilterCard.Controls.Add(cmbComponentType);

            // Buttons — always at fixed distance from right edge, repositioned on resize
            btnRefresh = new Button
            {
                Text = "🔄 Refresh",
                Size = new Size(95, 30),
                BackColor = Color.FromArgb(60, 70, 80),
                ForeColor = C_White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9),
                Cursor = Cursors.Hand
            };
            btnRefresh.FlatAppearance.BorderSize = 0;
            SetRoundedRegion(btnRefresh, 6);
            btnRefresh.Click += (s, e) => LoadExpiryDataFromDatabase();
            pnlFilterCard.Controls.Add(btnRefresh);

            btnExport = new Button
            {
                Text = "📊 Export",
                Size = new Size(85, 30),
                BackColor = brickRed,
                ForeColor = C_White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnExport.FlatAppearance.BorderSize = 0;
            SetRoundedRegion(btnExport, 6);
            btnExport.Click += (s, e) => ExportToExcel();
            pnlFilterCard.Controls.Add(btnExport);

            y += pnlFilterCard.Height + 16;

            // ── Grid Header ─────────────────────────────────────────
            Label lblTitle = new Label
            {
                Text = "⚠️ Blood Bags Expiry Alerts",
                Font = new Font("Segoe UI", 13f, FontStyle.Bold),
                ForeColor = C_TextDark,
                Location = new Point(MX, y),
                AutoSize = true
            };
            pnlContent.Controls.Add(lblTitle);
            y += 30;

            // ── Grid ────────────────────────────────────────────────
            dgvExpiryAlerts = CreateStyledGrid();
            dgvExpiryAlerts.Location = new Point(MX, y);
            dgvExpiryAlerts.Height = 350;
            dgvExpiryAlerts.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
            dgvExpiryAlerts.RowPrePaint += DgvExpiryAlerts_RowPrePaint;
            WrapInCard(dgvExpiryAlerts, pnlContent, new Point(MX, y), 350);
            FixDataGridView(dgvExpiryAlerts);

            // ── Resize handler ──────────────────────────────────────
            Action layoutAll = () =>
            {
                int w = pnlContent.ClientSize.Width - MX * 2 - 20; // extra 20px right margin
                if (w < 200) return;

                // Stats
                if (statsPanel != null && !statsPanel.IsDisposed)
                {
                    statsPanel.Width = w;
                    int sp = 10;
                    int cw = (w - sp * 3) / 4;
                    cw = Math.Max(80, cw);
                    card1.Width = cw; card1.Location = new Point(0, 0);
                    card2.Width = cw; card2.Location = new Point(cw + sp, 0);
                    card3.Width = cw; card3.Location = new Point((cw + sp) * 2, 0);
                    card4.Width = cw; card4.Location = new Point((cw + sp) * 3, 0);
                }

                // Filter card
                if (pnlFilterCard != null && !pnlFilterCard.IsDisposed)
                {
                    pnlFilterCard.Width = w;
                    if (btnExport != null) btnExport.Location = new Point(w - 90, filterY);
                    if (btnRefresh != null) btnRefresh.Location = new Point(w - 195, filterY);
                }

                // Grid
                if (dgvExpiryAlerts != null && !dgvExpiryAlerts.IsDisposed)
                    dgvExpiryAlerts.Width = w;

                // Status label
                if (lblStatus != null && !lblStatus.IsDisposed)
                    lblStatus.Location = new Point(pnlContent.ClientSize.Width - lblStatus.Width - MX, 20);
            };

            pnlContent.Resize += (s, e) => layoutAll();
            this.Shown += (s, e) => layoutAll();
        }

        private Panel CreateStatCard(string title, string value, Color color, int x, int y, int height)
        {
            Panel card = new Panel
            {
                Location = new Point(x, y),
                Size = new Size(100, height),   // width set by resize
                BackColor = C_CardBg,
                BorderStyle = BorderStyle.None
            };
            SetRoundedRegion(card, 10);
            AddDropShadow(card);

            Panel accentBar = new Panel
            {
                Location = new Point(0, 0),
                Size = new Size(5, height),
                BackColor = color
            };
            card.Controls.Add(accentBar);

            Label titleLbl = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 8.5f),
                ForeColor = C_TextMid,
                Location = new Point(14, 10),
                AutoSize = true
            };
            card.Controls.Add(titleLbl);

            Label valueLbl = new Label
            {
                Text = value,
                Font = new Font("Segoe UI", 22, FontStyle.Bold),
                ForeColor = color,
                Location = new Point(14, 35),
                AutoSize = true
            };
            card.Controls.Add(valueLbl);

            return card;
        }

        // =========================================================
        // LOAD EXPIRY DATA FROM DATABASE
        // =========================================================
        private async void LoadExpiryDataFromDatabase()
        {
            try
            {
                UpdateStatus("🔄 Loading expiry data from database...", Color.FromArgb(59, 130, 246));

                using (var conn = DatabaseHelper.GetConnection())
                {
                    await conn.OpenAsync();

                    string checkTable = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'BloodBags'";
                    using (var checkCmd = new SqlCommand(checkTable, conn))
                    {
                        int tableExists = Convert.ToInt32(await checkCmd.ExecuteScalarAsync());
                        if (tableExists == 0)
                        {
                            dgvExpiryAlerts.DataSource = null;
                            UpdateStatus("⚠️ Table 'BloodBags' not found", Color.FromArgb(245, 158, 11));
                            return;
                        }
                    }

                    string query = @"
                        SELECT 
                            BagID as [Bag ID],
                            BloodGroup as [Blood Group],
                            Volume as [Quantity (mL)],
                            ComponentType as [Component Type],
                            FORMAT(CollectionDate, 'dd-MMM-yyyy') as [Collection Date],
                            FORMAT(ExpiryDate, 'dd-MMM-yyyy') as [Expiry Date],
                            DATEDIFF(DAY, GETDATE(), ExpiryDate) as [Days Left],
                            CASE 
                                WHEN ExpiryDate < GETDATE() THEN 'Expired'
                                WHEN DATEDIFF(DAY, GETDATE(), ExpiryDate) <= 7 THEN 'Expiring Soon'
                                ELSE 'Good'
                            END as [Status],
                            ISNULL(StorageLocation, 'Not Assigned') as [Location],
                            ISNULL(DonorName, 'N/A') as [Donor Name]
                        FROM BloodBags 
                        WHERE Status NOT IN ('Discarded', 'Quarantined')
                        ORDER BY [Days Left] ASC, ExpiryDate ASC";

                    SqlDataAdapter da = new SqlDataAdapter(query, conn);
                    originalData = new DataTable();
                    da.Fill(originalData);

                    dgvExpiryAlerts.DataSource = originalData;
                    FormatGridColumns();
                    NoSort(dgvExpiryAlerts);
                    UpdateStatistics();

                    UpdateStatus(originalData.Rows.Count == 0
                        ? "✅ No bags found in inventory"
                        : $"✅ Loaded {originalData.Rows.Count} bag(s) from database",
                        Color.FromArgb(16, 185, 129));
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LoadExpiryDataFromDatabase Error: {ex.Message}");
                dgvExpiryAlerts.DataSource = null;
                UpdateStatus($"❌ Database error: {ex.Message}", Color.FromArgb(220, 38, 38));
            }
        }

        private void FormatGridColumns()
        {
            if (dgvExpiryAlerts.Columns.Count == 0) return;

            if (dgvExpiryAlerts.Columns.Contains("Bag ID")) dgvExpiryAlerts.Columns["Bag ID"].FillWeight = 12;
            if (dgvExpiryAlerts.Columns.Contains("Blood Group")) dgvExpiryAlerts.Columns["Blood Group"].FillWeight = 8;
            if (dgvExpiryAlerts.Columns.Contains("Quantity (mL)")) dgvExpiryAlerts.Columns["Quantity (mL)"].FillWeight = 8;
            if (dgvExpiryAlerts.Columns.Contains("Component Type")) dgvExpiryAlerts.Columns["Component Type"].FillWeight = 10;
            if (dgvExpiryAlerts.Columns.Contains("Collection Date")) dgvExpiryAlerts.Columns["Collection Date"].FillWeight = 10;
            if (dgvExpiryAlerts.Columns.Contains("Expiry Date")) dgvExpiryAlerts.Columns["Expiry Date"].FillWeight = 10;
            if (dgvExpiryAlerts.Columns.Contains("Days Left")) dgvExpiryAlerts.Columns["Days Left"].FillWeight = 8;
            if (dgvExpiryAlerts.Columns.Contains("Status")) dgvExpiryAlerts.Columns["Status"].FillWeight = 10;
            if (dgvExpiryAlerts.Columns.Contains("Location")) dgvExpiryAlerts.Columns["Location"].FillWeight = 12;
            if (dgvExpiryAlerts.Columns.Contains("Donor Name")) dgvExpiryAlerts.Columns["Donor Name"].FillWeight = 12;
        }

        private void UpdateStatistics()
        {
            if (originalData == null) return;

            int totalBags = originalData.Rows.Count;
            int totalUnits = 0, expiringSoon = 0, expired = 0;

            foreach (DataRow row in originalData.Rows)
            {
                int daysLeft = Convert.ToInt32(row["Days Left"]);
                if (row["Quantity (mL)"] != DBNull.Value)
                    totalUnits += Convert.ToInt32(row["Quantity (mL)"]);
                if (daysLeft <= 0) expired++;
                else if (daysLeft <= 7) expiringSoon++;
            }

            if (lblTotalBags != null) lblTotalBags.Text = totalBags.ToString();
            if (lblTotalUnits != null) lblTotalUnits.Text = totalUnits.ToString();
            if (lblExpiringSoon != null) lblExpiringSoon.Text = expiringSoon.ToString();
            if (lblExpired != null) lblExpired.Text = expired.ToString();
        }

        private void ApplyFilter()
        {
            if (originalData == null) return;

            string alertType = cmbAlertType.SelectedItem?.ToString();
            string bloodGroup = cmbBloodGroup.SelectedItem?.ToString();
            string componentType = cmbComponentType.SelectedItem?.ToString();

            DataTable filteredDt = originalData.Clone();

            foreach (DataRow row in originalData.Rows)
            {
                int daysLeft = Convert.ToInt32(row["Days Left"]);
                string bg = row["Blood Group"]?.ToString() ?? "";
                string comp = row["Component Type"]?.ToString() ?? "";
                bool include = true;

                if (alertType != "All")
                {
                    if (alertType == "Expiring Soon (≤7 days)" && (daysLeft <= 0 || daysLeft > 7)) include = false;
                    else if (alertType == "Expired" && daysLeft > 0) include = false;
                    else if (alertType == "Normal (>7 days)" && (daysLeft <= 7 || daysLeft < 0)) include = false;
                }
                if (include && bloodGroup != "All" && bg != bloodGroup) include = false;
                if (include && componentType != "All" && comp != componentType) include = false;

                if (include) filteredDt.ImportRow(row);
            }

            dgvExpiryAlerts.DataSource = filteredDt;
            UpdateStatus($"🔍 Showing {filteredDt.Rows.Count} of {originalData.Rows.Count} bags", Color.FromArgb(59, 130, 246));
        }

        private void DgvExpiryAlerts_RowPrePaint(object sender, DataGridViewRowPrePaintEventArgs e)
        {
            if (e.RowIndex >= 0 && dgvExpiryAlerts.Rows[e.RowIndex].Cells["Days Left"].Value != null)
            {
                int daysLeft = Convert.ToInt32(dgvExpiryAlerts.Rows[e.RowIndex].Cells["Days Left"].Value);
                if (daysLeft <= 0) dgvExpiryAlerts.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.FromArgb(255, 220, 220);
                else if (daysLeft <= 3) dgvExpiryAlerts.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.FromArgb(255, 235, 220);
                else if (daysLeft <= 7) dgvExpiryAlerts.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.FromArgb(255, 250, 220);
            }
        }

        // =========================================================
        // EXPORT
        // =========================================================
        private void ExportToExcel()
        {
            try
            {
                SaveFileDialog sfd = new SaveFileDialog
                {
                    Filter = "CSV files (*.csv)|*.csv|Excel files (*.xls)|*.xls",
                    DefaultExt = "csv",
                    FileName = $"ExpiryAlerts_{DateTime.Now:yyyyMMdd_HHmmss}"
                };

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    DataTable exportData = dgvExpiryAlerts.DataSource as DataTable;
                    if (exportData == null || exportData.Rows.Count == 0)
                    {
                        MessageBox.Show("No data to export.", "Export", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }
                    if (sfd.FileName.EndsWith(".csv")) ExportToCSV(exportData, sfd.FileName);
                    else ExportToExcelHtml(exportData, sfd.FileName);

                    MessageBox.Show($"Data exported successfully to:\n{sfd.FileName}", "Export Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Export failed: {ex.Message}", "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ExportToCSV(DataTable dt, string filePath)
        {
            using (System.IO.StreamWriter sw = new System.IO.StreamWriter(filePath))
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

        private void ExportToExcelHtml(DataTable dt, string filePath)
        {
            StringBuilder html = new StringBuilder();
            html.AppendLine("<html><head><meta charset='UTF-8'></head><body>");
            html.AppendLine("<h2>Blood Bank - Expiry Alerts Report</h2>");
            html.AppendLine($"<p>Generated on: {DateTime.Now:dd-MMM-yyyy HH:mm:ss}</p>");
            html.AppendLine("<table border='1' cellpadding='5' cellspacing='0' style='border-collapse:collapse;'>");
            html.AppendLine("<tr style='background-color:#78161b; color:white;'>");
            foreach (DataColumn col in dt.Columns)
                html.AppendLine($"<th>{col.ColumnName}</th>");
            html.AppendLine("</tr>");
            foreach (DataRow row in dt.Rows)
            {
                html.AppendLine("<tr>");
                foreach (DataColumn col in dt.Columns)
                {
                    string value = row[col].ToString();
                    if (col.ColumnName == "Days Left" && int.TryParse(value, out int dl))
                        html.AppendLine(dl <= 0 ? $"<td style='background-color:#ffdddd;'>{value}</td>"
                                      : dl <= 7 ? $"<td style='background-color:#ffffcc;'>{value}</td>"
                                                : $"<td>{value}</td>");
                    else
                        html.AppendLine($"<td>{value}</td>");
                }
                html.AppendLine("</tr>");
            }
            html.AppendLine("</table></body></html>");
            System.IO.File.WriteAllText(filePath, html.ToString());
        }

        private void UpdateStatus(string message, Color color)
        {
            if (lblStatus != null && !lblStatus.IsDisposed)
            {
                if (lblStatus.InvokeRequired)
                    lblStatus.Invoke(new Action(() => { lblStatus.Text = message; lblStatus.ForeColor = color; }));
                else
                { lblStatus.Text = message; lblStatus.ForeColor = color; }
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
                RowTemplate = { Height = 38 },
                ScrollBars = ScrollBars.Both,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            dgv.ColumnHeadersDefaultCellStyle.BackColor = brickRed;
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = C_White;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            dgv.ColumnHeadersHeight = 40;
            dgv.DefaultCellStyle.Font = new Font("Segoe UI", 9f);
            dgv.DefaultCellStyle.SelectionBackColor = brickRedLight;
            dgv.DefaultCellStyle.SelectionForeColor = brickRed;
            dgv.DefaultCellStyle.Padding = new Padding(8, 4, 8, 4);
            dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(252, 252, 252);
            return dgv;
        }

        private void NoSort(DataGridView dgv)
        {
            foreach (DataGridViewColumn col in dgv.Columns)
                col.SortMode = DataGridViewColumnSortMode.NotSortable;
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            refreshTimer?.Stop();
            refreshTimer?.Dispose();
            base.OnFormClosing(e);
        }
    }
}