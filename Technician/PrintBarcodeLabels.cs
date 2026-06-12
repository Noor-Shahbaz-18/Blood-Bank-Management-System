using System;
using System.Data;
using System.Drawing;
using System.Drawing.Printing;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;
using BloodBankManagementSystem.Classes.Database;
using BloodBankManagementSystem.Classes.Helpers;

namespace BloodBankManagementSystem.Forms.Technician
{
    public partial class PrintBarcodeLabels : BaseTechnicianForm
    {
        private DataGridView dgvBags;
        private ListBox lstSelectedBags;
        private ComboBox cmbLabelType, cmbLabelSize, cmbSearchBloodGroup, cmbSearchStatus;
        private NumericUpDown numCopies;
        private Button btnGenerate, btnPrint, btnAddToList, btnRemoveFromList, btnClear, btnSearch, btnRefresh;
        private TextBox txtSearchBagID, txtSearchDonor;
        private PrintDocument printDocument;
        private PrintPreviewDialog printPreviewDialog;
        private DataTable labelData;
        private DataTable availableBagsData;
        private int currentLabelIndex;
        private Panel pnlPreview;
        private Label lblStatus;
        private Timer refreshTimer;

        public PrintBarcodeLabels()
        {
            this.Text = "Blood Bank Management System – Print Barcode Labels";
            BuildLayout();
            BuildSidebar("Print Labels");
            BuildTopBar("Print Barcode Labels");
            BuildContentArea();
            LoadAvailableBagsFromDatabase();

            refreshTimer = new Timer { Interval = 120000 };
            refreshTimer.Tick += (s, e) => LoadAvailableBagsFromDatabase();
            refreshTimer.Start();
        }

        private void BuildContentArea()
        {
            int y = 20;

            // ── Status label ─────────────────────────────────────────
            lblStatus = new Label
            {
                Text = "✅ Loading bags...",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(16, 185, 129),
                Location = new Point(pnlContent.Width - 310, y),
                AutoSize = true,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            pnlContent.Controls.Add(lblStatus);

            // ── Settings Panel ────────────────────────────────────────
            // Height = 160 to fit 3 clean rows:
            //   Row 1: Label Type | Label Size | Copies
            //   Row 2: Search fields
            //   Row 3: Action buttons
            Panel pnlSettings = new Panel
            {
                Location = new Point(0, y + 25),
                Width = pnlContent.ClientSize.Width - 40,
                Height = 168,
                BackColor = C_CardBg,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            SetRoundedRegion(pnlSettings, 12);
            AddDropShadow(pnlSettings);
            pnlContent.Controls.Add(pnlSettings);

            pnlSettings.Controls.Add(new Label
            {
                Text = "🖨️ Label Settings",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = C_TextDark,
                Location = new Point(15, 10),
                AutoSize = true
            });

            // ── ROW 1: Label Type | Label Size | Copies ───────────────
            int r1Y = 40;

            pnlSettings.Controls.Add(MkLbl("Label Type:", new Point(15, r1Y)));
            cmbLabelType = MkCombo(new Point(95, r1Y - 2), 150,
                new[] { "Blood Bag Label", "Component Label", "Donor Label", "Patient Label" });
            pnlSettings.Controls.Add(cmbLabelType);

            pnlSettings.Controls.Add(MkLbl("Label Size:", new Point(260, r1Y)));
            cmbLabelSize = MkCombo(new Point(340, r1Y - 2), 145,
                new[] { "Small (2x1 inch)", "Medium (3x2 inch)", "Large (4x3 inch)" });
            cmbLabelSize.SelectedIndex = 1;
            pnlSettings.Controls.Add(cmbLabelSize);

            pnlSettings.Controls.Add(MkLbl("Copies:", new Point(500, r1Y)));
            numCopies = new NumericUpDown
            {
                Location = new Point(558, r1Y - 2),
                Width = 55,
                Minimum = 1,
                Maximum = 10,
                Value = 1,
                Font = new Font("Segoe UI", 9)
            };
            pnlSettings.Controls.Add(numCopies);

            // ── ROW 2: Search fields ──────────────────────────────────
            int r2Y = 82;

            pnlSettings.Controls.Add(MkLbl("Search Bag ID:", new Point(15, r2Y)));
            txtSearchBagID = new TextBox
            {
                Location = new Point(105, r2Y - 2),
                Width = 110,
                Font = new Font("Segoe UI", 9),
                BorderStyle = BorderStyle.FixedSingle
            };
            pnlSettings.Controls.Add(txtSearchBagID);

            pnlSettings.Controls.Add(MkLbl("Donor:", new Point(230, r2Y)));
            txtSearchDonor = new TextBox
            {
                Location = new Point(275, r2Y - 2),
                Width = 110,
                Font = new Font("Segoe UI", 9),
                BorderStyle = BorderStyle.FixedSingle
            };
            pnlSettings.Controls.Add(txtSearchDonor);

            pnlSettings.Controls.Add(MkLbl("Blood Group:", new Point(400, r2Y)));
            cmbSearchBloodGroup = MkCombo(new Point(480, r2Y - 2), 90,
                new[] { "All", "A+", "A-", "B+", "B-", "AB+", "AB-", "O+", "O-" });
            pnlSettings.Controls.Add(cmbSearchBloodGroup);

            pnlSettings.Controls.Add(MkLbl("Status:", new Point(585, r2Y)));
            cmbSearchStatus = MkCombo(new Point(630, r2Y - 2), 100,
                new[] { "All", "Available", "In Lab", "Issued" });
            pnlSettings.Controls.Add(cmbSearchStatus);

            // ── ROW 3: Buttons ────────────────────────────────────────
            // All 4 buttons on their own row — no overlapping with fields
            int r3Y = 124;

            btnSearch = MkBtn("🔍 Search", brickRed, bold: true);
            btnSearch.Location = new Point(15, r3Y);
            btnSearch.Size = new Size(95, 30);
            btnSearch.Click += (s, e) => SearchBags();
            pnlSettings.Controls.Add(btnSearch);

            btnRefresh = MkBtn("🔄 Refresh", Color.FromArgb(60, 70, 80));
            btnRefresh.Location = new Point(120, r3Y);
            btnRefresh.Size = new Size(95, 30);
            btnRefresh.Click += (s, e) => LoadAvailableBagsFromDatabase();
            pnlSettings.Controls.Add(btnRefresh);

            btnGenerate = MkBtn("🔍 Generate Preview", Color.FromArgb(30, 130, 200), bold: true);
            btnGenerate.Location = new Point(225, r3Y);
            btnGenerate.Size = new Size(150, 30);
            btnGenerate.Click += BtnGenerate_Click;
            pnlSettings.Controls.Add(btnGenerate);

            btnPrint = MkBtn("🖨️ Print Labels", brickRed, bold: true);
            btnPrint.Location = new Point(385, r3Y);
            btnPrint.Size = new Size(130, 30);
            btnPrint.Click += BtnPrint_Click;
            pnlSettings.Controls.Add(btnPrint);

            y += pnlSettings.Height + 18;

            // ── Available Blood Bags ──────────────────────────────────
            AddSectionHeader("📋 Available Blood Bags", ref y);

            dgvBags = CreateStyledGrid();
            WrapInCard(dgvBags, pnlContent, new Point(0, y), 210);
            FixDataGridView(dgvBags);
            y += 228;

            // ── Selected Bags for Printing ────────────────────────────
            AddSectionHeader("📋 Selected Bags for Printing", ref y);

            Panel selectionPanel = new Panel
            {
                Location = new Point(0, y),
                Width = pnlContent.ClientSize.Width - 40,
                Height = 160,
                BackColor = Color.Transparent
            };
            pnlContent.Controls.Add(selectionPanel);

            lstSelectedBags = new ListBox
            {
                Location = new Point(0, 0),
                Width = selectionPanel.Width - 160,
                Height = 145,
                Font = new Font("Segoe UI", 10),
                BorderStyle = BorderStyle.FixedSingle,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            selectionPanel.Controls.Add(lstSelectedBags);

            int btnColX = selectionPanel.Width - 145;

            btnAddToList = MkBtn(">> Add Selected", brickRed, bold: true);
            btnAddToList.Location = new Point(btnColX, 5);
            btnAddToList.Size = new Size(135, 38);
            btnAddToList.Click += BtnAddToList_Click;
            selectionPanel.Controls.Add(btnAddToList);

            btnRemoveFromList = MkBtn("<< Remove", Color.FromArgb(100, 110, 125));
            btnRemoveFromList.Location = new Point(btnColX, 53);
            btnRemoveFromList.Size = new Size(135, 38);
            btnRemoveFromList.Click += BtnRemoveFromList_Click;
            selectionPanel.Controls.Add(btnRemoveFromList);

            btnClear = MkBtn("⟳ Clear All", Color.FromArgb(60, 70, 80));
            btnClear.Location = new Point(btnColX, 101);
            btnClear.Size = new Size(135, 38);
            btnClear.Click += (s, e) => lstSelectedBags.Items.Clear();
            selectionPanel.Controls.Add(btnClear);

            y += 175;

            // ── Preview Panel (hidden until Generate clicked) ─────────
            pnlPreview = new Panel
            {
                Location = new Point(0, y),
                Width = pnlContent.ClientSize.Width - 40,
                Height = 220,
                BackColor = C_CardBg,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                Visible = false
            };
            SetRoundedRegion(pnlPreview, 12);
            AddDropShadow(pnlPreview);
            pnlContent.Controls.Add(pnlPreview);

            pnlContent.Controls.Add(new Panel
            {
                Height = 30,
                BackColor = Color.Transparent,
                Dock = DockStyle.Bottom
            });

            pnlContent.Resize += (s, e) =>
            {
                if (lblStatus != null)
                    lblStatus.Location = new Point(pnlContent.Width - 310, 20);
            };
        }

        // ── Section header helper ─────────────────────────────────────
        private void AddSectionHeader(string text, ref int y)
        {
            Panel hdr = new Panel
            {
                Location = new Point(0, y),
                Width = pnlContent.ClientSize.Width - 40,
                Height = 30,
                BackColor = Color.Transparent
            };
            hdr.Controls.Add(new Label
            {
                Text = text,
                Font = new Font("Segoe UI", 13f, FontStyle.Bold),
                ForeColor = C_TextDark,
                Location = new Point(0, 3),
                AutoSize = true
            });
            pnlContent.Controls.Add(hdr);
            y += 30;
        }

        // ── Tiny factory helpers ──────────────────────────────────────
        private Label MkLbl(string text, Point loc) => new Label
        {
            Text = text,
            Font = new Font("Segoe UI", 9),
            ForeColor = C_TextMid,
            Location = loc,
            AutoSize = true
        };

        private ComboBox MkCombo(Point loc, int width, string[] items)
        {
            var c = new ComboBox
            {
                Location = loc,
                Width = width,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9)
            };
            c.Items.AddRange(items);
            c.SelectedIndex = 0;
            return c;
        }

        private Button MkBtn(string text, Color backColor, bool bold = false)
        {
            var b = new Button
            {
                Text = text,
                BackColor = backColor,
                ForeColor = C_White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, bold ? FontStyle.Bold : FontStyle.Regular),
                Cursor = Cursors.Hand
            };
            b.FlatAppearance.BorderSize = 0;
            SetRoundedRegion(b, 6);
            return b;
        }

        // =========================================================
        // LOAD AVAILABLE BAGS FROM DATABASE
        // =========================================================
        private async void LoadAvailableBagsFromDatabase()
        {
            try
            {
                UpdateStatus("🔄 Loading available bags...", Color.FromArgb(59, 130, 246));

                using (var conn = DatabaseHelper.GetConnection())
                {
                    await conn.OpenAsync();

                    string checkTable = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'BloodBags'";
                    using (var checkCmd = new SqlCommand(checkTable, conn))
                    {
                        int tableExists = Convert.ToInt32(await checkCmd.ExecuteScalarAsync());
                        if (tableExists == 0)
                        {
                            dgvBags.DataSource = null;
                            UpdateStatus("⚠️ Table 'BloodBags' not found", Color.FromArgb(245, 158, 11));
                            return;
                        }
                    }

                    string query = @"
                        SELECT 
                            BagID                                   AS [Bag ID],
                            ISNULL(DonorName, 'N/A')                AS [Donor Name],
                            BloodGroup                              AS [Blood Group],
                            ComponentType                           AS [Component],
                            Volume                                  AS [Vol (mL)],
                            FORMAT(CollectionDate,'dd-MMM-yyyy')    AS [Collection Date],
                            FORMAT(ExpiryDate,    'dd-MMM-yyyy')    AS [Expiry Date],
                            Status,
                            ISNULL(StorageLocation,'Not Assigned')  AS [Location]
                        FROM BloodBags
                        WHERE Status IN ('Available','In Lab')
                        ORDER BY CollectionDate DESC, BagID DESC";

                    SqlDataAdapter da = new SqlDataAdapter(query, conn);
                    availableBagsData = new DataTable();
                    da.Fill(availableBagsData);

                    dgvBags.DataSource = availableBagsData;
                    FormatBagsGrid();
                    NoSort(dgvBags);

                    UpdateStatus($"✅ Loaded {availableBagsData.Rows.Count} available bag(s)",
                                 Color.FromArgb(16, 185, 129));
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LoadAvailableBagsFromDatabase Error: {ex.Message}");
                dgvBags.DataSource = null;
                UpdateStatus($"❌ Database error: {ex.Message}", Color.FromArgb(220, 38, 38));
            }
        }

        private void FormatBagsGrid()
        {
            if (dgvBags.Columns.Count == 0) return;

            dgvBags.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            void Col(string name, int weight, bool center = false)
            {
                if (!dgvBags.Columns.Contains(name)) return;
                dgvBags.Columns[name].FillWeight = weight;
                dgvBags.Columns[name].MinimumWidth = 55;
                if (center)
                    dgvBags.Columns[name].DefaultCellStyle.Alignment =
                        DataGridViewContentAlignment.MiddleCenter;
            }

            Col("Bag ID", 11);
            Col("Donor Name", 15);
            Col("Blood Group", 8, center: true);
            Col("Component", 11);
            Col("Vol (mL)", 7, center: true);
            Col("Collection Date", 11);
            Col("Expiry Date", 11);
            Col("Status", 10);
            Col("Location", 12);
        }

        // =========================================================
        // SEARCH
        // =========================================================
        private void SearchBags()
        {
            if (availableBagsData == null) return;

            string bagID = txtSearchBagID.Text.Trim().ToLower();
            string donor = txtSearchDonor.Text.Trim().ToLower();
            string bloodGroup = cmbSearchBloodGroup.SelectedItem?.ToString();
            string status = cmbSearchStatus.SelectedItem?.ToString();

            DataTable filteredDt = availableBagsData.Clone();

            foreach (DataRow row in availableBagsData.Rows)
            {
                bool match = true;

                if (!string.IsNullOrEmpty(bagID)
                    && !(row["Bag ID"]?.ToString().ToLower().Contains(bagID) ?? false))
                    match = false;

                if (match && !string.IsNullOrEmpty(donor)
                    && !(row["Donor Name"]?.ToString().ToLower().Contains(donor) ?? false))
                    match = false;

                if (match && bloodGroup != "All"
                    && row["Blood Group"]?.ToString() != bloodGroup)
                    match = false;

                if (match && status != "All"
                    && row["Status"]?.ToString() != status)
                    match = false;

                if (match) filteredDt.ImportRow(row);
            }

            dgvBags.DataSource = filteredDt;
            UpdateStatus($"🔍 Found {filteredDt.Rows.Count} matching bag(s)",
                         Color.FromArgb(59, 130, 246));
        }

        // =========================================================
        // ADD / REMOVE
        // =========================================================
        private void BtnAddToList_Click(object sender, EventArgs e)
        {
            if (dgvBags.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a bag from the grid first.", "No Selection",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DataGridViewRow row = dgvBags.SelectedRows[0];
            string bagId = row.Cells["Bag ID"].Value?.ToString();
            string donorName = row.Cells["Donor Name"].Value?.ToString();
            string bloodGroup = row.Cells["Blood Group"].Value?.ToString();
            string bagInfo = $"{bagId} - {donorName} - {bloodGroup}";

            if (!lstSelectedBags.Items.Contains(bagInfo))
            {
                lstSelectedBags.Items.Add(bagInfo);
                UpdateStatus($"✅ Added {bagId} to print list", Color.FromArgb(16, 185, 129));
            }
            else
            {
                UpdateStatus($"ℹ️ {bagId} already in list", Color.FromArgb(245, 158, 11));
            }
        }

        private void BtnRemoveFromList_Click(object sender, EventArgs e)
        {
            if (lstSelectedBags.SelectedIndex >= 0)
            {
                lstSelectedBags.Items.RemoveAt(lstSelectedBags.SelectedIndex);
                UpdateStatus("🗑️ Removed from print list", Color.FromArgb(16, 185, 129));
            }
        }

        // =========================================================
        // GENERATE PREVIEW
        // =========================================================
        private void BtnGenerate_Click(object sender, EventArgs e)
        {
            if (lstSelectedBags.Items.Count == 0)
            {
                MessageBox.Show("Please add at least one bag to the list first.", "No Bags Selected",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            pnlPreview.Visible = true;
            pnlPreview.Controls.Clear();

            pnlPreview.Controls.Add(new Label
            {
                Text = "🔍 Label Preview",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = C_TextDark,
                Location = new Point(15, 10),
                AutoSize = true
            });

            int previewY = 42;
            int maxPreview = Math.Min(lstSelectedBags.Items.Count, 5);

            for (int i = 0; i < maxPreview; i++)
            {
                string bagInfo = lstSelectedBags.Items[i].ToString();
                string bagId = bagInfo.Split('-')[0].Trim();

                Panel previewLabel = new Panel
                {
                    Location = new Point(15 + (i % 3) * 275, previewY + (i / 3) * 65),
                    Size = new Size(260, 55),
                    BackColor = Color.White,
                    BorderStyle = BorderStyle.FixedSingle
                };

                string capturedBagId = bagId;
                string capturedBagInfo = bagInfo;

                previewLabel.Paint += (s, pe) =>
                {
                    var g = pe.Graphics;
                    using var barcodeFont = new Font("Consolas", 10);
                    using var textFont = new Font("Segoe UI", 7);

                    // Simulated barcode bars
                    for (int x = 10; x < 250; x += 4)
                        pe.Graphics.DrawLine(Pens.Black, x, 4, x, 28);

                    g.DrawString(capturedBagId, barcodeFont, Brushes.Black, 10, 30);
                    string info = capturedBagInfo.Length > 38
                        ? capturedBagInfo.Substring(0, 35) + "..."
                        : capturedBagInfo;
                    g.DrawString(info, textFont, Brushes.DimGray, 10, 44);
                };
                pnlPreview.Controls.Add(previewLabel);
            }

            if (lstSelectedBags.Items.Count > maxPreview)
            {
                pnlPreview.Controls.Add(new Label
                {
                    Text = $"... and {lstSelectedBags.Items.Count - maxPreview} more labels",
                    Location = new Point(15, previewY + 135),
                    AutoSize = true,
                    ForeColor = C_TextMid
                });
            }

            UpdateStatus("🔍 Preview generated", Color.FromArgb(59, 130, 246));
        }

        // =========================================================
        // PRINT
        // =========================================================
        private void BtnPrint_Click(object sender, EventArgs e)
        {
            if (lstSelectedBags.Items.Count == 0)
            {
                MessageBox.Show("Please add at least one bag to the list first.", "No Bags Selected",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            labelData = new DataTable();
            labelData.Columns.Add("BagInfo");
            labelData.Columns.Add("BagID");
            labelData.Columns.Add("DonorName");
            labelData.Columns.Add("BloodGroup");

            foreach (var item in lstSelectedBags.Items)
            {
                string bagInfo = item.ToString();
                string[] parts = bagInfo.Split('-');
                string bagId = parts[0].Trim();
                string donorName = parts.Length > 1 ? parts[1].Trim() : "N/A";
                string bloodGroup = parts.Length > 2 ? parts[2].Trim() : "N/A";
                labelData.Rows.Add(bagInfo, bagId, donorName, bloodGroup);
            }

            currentLabelIndex = 0;
            printDocument = new PrintDocument();
            printDocument.PrintPage += PrintDocument_PrintPage;

            printPreviewDialog = new PrintPreviewDialog
            {
                Document = printDocument,
                WindowState = FormWindowState.Maximized
            };
            printPreviewDialog.ShowDialog();

            AuditHelper.Log("Print Barcode Labels", "Print",
                $"Printed {lstSelectedBags.Items.Count} label(s) - Type: {cmbLabelType.SelectedItem}");
        }

        private void PrintDocument_PrintPage(object sender, PrintPageEventArgs e)
        {
            Graphics g = e.Graphics;
            Font barcodeFont = new Font("Consolas", 10);
            Font textFont = new Font("Segoe UI", 8);
            Font titleFont = new Font("Segoe UI", 9, FontStyle.Bold);
            int printY = 50;
            int copies = (int)numCopies.Value;
            string labelType = cmbLabelType.SelectedItem?.ToString() ?? "Blood Bag Label";
            DateTime now = DateTime.Now;

            for (int copy = 0; copy < copies && currentLabelIndex < labelData.Rows.Count; copy++)
            {
                DataRow row = labelData.Rows[currentLabelIndex];
                string bagId = row["BagID"].ToString();
                string donorName = row["DonorName"].ToString();
                string bloodGroup = row["BloodGroup"].ToString();

                Rectangle labelRect = new Rectangle(50, printY, 280, 100);
                g.DrawRectangle(Pens.Black, labelRect);
                g.DrawString(labelType, titleFont, Brushes.Black, 60, printY + 5);

                for (int x = 60; x < 310; x += 3)
                    g.DrawLine(Pens.Black, x, printY + 20, x, printY + 50);

                g.DrawString(bagId, barcodeFont, Brushes.Black, 60, printY + 55);
                g.DrawString($"Donor: {donorName}", textFont, Brushes.Black, 60, printY + 72);
                g.DrawString($"Blood Group: {bloodGroup}", textFont, Brushes.Black, 60, printY + 83);
                g.DrawString($"Date: {now:dd-MMM-yyyy}", new Font("Segoe UI", 6), Brushes.Gray, 60, printY + 93);

                printY += 115;

                if (printY > e.MarginBounds.Height - 50)
                {
                    currentLabelIndex++;
                    e.HasMorePages = true;
                    return;
                }
            }

            currentLabelIndex++;
            e.HasMorePages = currentLabelIndex < labelData.Rows.Count;
        }

        // =========================================================
        // HELPERS
        // =========================================================
        private void UpdateStatus(string message, Color color)
        {
            if (lblStatus != null && !lblStatus.IsDisposed)
            {
                lblStatus.Text = message;
                lblStatus.ForeColor = color;
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
                RowTemplate = { Height = 40 },
                ScrollBars = ScrollBars.Both,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            dgv.ColumnHeadersDefaultCellStyle.BackColor = brickRed;
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = C_White;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            dgv.ColumnHeadersHeight = 42;
            dgv.DefaultCellStyle.Font = new Font("Segoe UI", 8.5f);
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