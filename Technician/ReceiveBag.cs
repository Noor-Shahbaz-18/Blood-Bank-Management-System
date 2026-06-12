using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.Threading.Tasks;
using BloodBankManagementSystem.Classes.Database;
using BloodBankManagementSystem.Classes.Helpers;

namespace BloodBankManagementSystem.Forms.Technician
{
    public partial class ReceiveBag : BaseTechnicianForm
    {
        private DataGridView dgvBags;
        private TabControl tcLabType;
        private TextBox txtBagID, txtBagName, txtQuantity, txtDonorID, txtDonorName, txtSearchBagID, txtSearchDonor, txtRemarks;
        private ComboBox cmbBagType, cmbBagColor, cmbStatus, cmbBloodGroup, cmbSearchLabType;
        private DateTimePicker dtpDeliveryDate, dtpDeliveryTime, dtpExpiryDate;
        private Button btnSave, btnClear, btnSearch, btnRefresh;
        private Label lblStatus;
        private DataTable receivedBagsData;
        private bool isEditMode = false;
        private string currentBagID = "";

        public ReceiveBag()
        {
            this.Text = "Blood Bank Management System – Receive Bag";
            BuildLayout();
            BuildSidebar("Receive Bag");
            BuildTopBar("Receive Bag");
            BuildContentArea();

            // Load data from database
            LoadReceivedBagsFromDatabase();
            EnsureBloodBagsTableExists();
            GenerateBagID();
        }

        private void BuildContentArea()
        {
            int y = 20;

            // Status label
            lblStatus = new Label
            {
                Text = "✅ Ready",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(16, 185, 129),
                Location = new Point(pnlContent.Width - 250, y),
                AutoSize = true,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            pnlContent.Controls.Add(lblStatus);

            // Search Panel
            Panel pnlSearch = new Panel
            {
                Location = new Point(0, y + 25),
                Width = pnlContent.ClientSize.Width - 60,
                Height = 50,
                BackColor = C_CardBg,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            SetRoundedRegion(pnlSearch, 12);
            AddDropShadow(pnlSearch);
            pnlContent.Controls.Add(pnlSearch);

            Label lblSearch = new Label
            {
                Text = "Search:",
                Font = new Font("Segoe UI", 10),
                ForeColor = C_TextMid,
                Location = new Point(20, 15),
                AutoSize = true
            };
            pnlSearch.Controls.Add(lblSearch);

            txtSearchBagID = new TextBox
            {
                Location = new Point(80, 12),
                Width = 130,
                Font = new Font("Segoe UI", 10),
                BorderStyle = BorderStyle.FixedSingle
            };
            pnlSearch.Controls.Add(txtSearchBagID);

            txtSearchDonor = new TextBox
            {
                Location = new Point(225, 12),
                Width = 130,
                Font = new Font("Segoe UI", 10),
                BorderStyle = BorderStyle.FixedSingle
            };
            pnlSearch.Controls.Add(txtSearchDonor);

            cmbSearchLabType = new ComboBox
            {
                Location = new Point(370, 12),
                Width = 100,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9)
            };
            cmbSearchLabType.Items.AddRange(new[] { "All", "In Lab", "Out Lab" });
            cmbSearchLabType.SelectedIndex = 0;
            pnlSearch.Controls.Add(cmbSearchLabType);

            btnSearch = new Button
            {
                Text = "🔍 Search",
                Location = new Point(490, 10),
                Size = new Size(90, 30),
                BackColor = brickRed,
                ForeColor = C_White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnSearch.FlatAppearance.BorderSize = 0;
            SetRoundedRegion(btnSearch, 6);
            btnSearch.Click += (s, e) => SearchReceivedBags();
            pnlSearch.Controls.Add(btnSearch);

            btnRefresh = new Button
            {
                Text = "🔄 Refresh",
                Location = new Point(590, 10),
                Size = new Size(90, 30),
                BackColor = Color.FromArgb(60, 70, 80),
                ForeColor = C_White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnRefresh.FlatAppearance.BorderSize = 0;
            SetRoundedRegion(btnRefresh, 6);
            btnRefresh.Click += (s, e) => LoadReceivedBagsFromDatabase();
            pnlSearch.Controls.Add(btnRefresh);

            y += pnlSearch.Height + 20;

            // Tab Control for Lab Type
            tcLabType = new TabControl
            {
                Location = new Point(0, y),
                Size = new Size(pnlContent.ClientSize.Width - 60, 40),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            tcLabType.TabPages.Add("In Lab");
            tcLabType.TabPages.Add("Out Lab");
            pnlContent.Controls.Add(tcLabType);
            y += 50;

            // Form Card
            Panel formCard = new Panel
            {
                Location = new Point(0, y),
                Width = pnlContent.ClientSize.Width - 60,
                Height = 420,
                BackColor = C_CardBg,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            SetRoundedRegion(formCard, 15);
            AddDropShadow(formCard);
            pnlContent.Controls.Add(formCard);

            formCard.Controls.Add(new Label
            {
                Text = "📦 Receive New Blood Bag",
                Font = new Font("Segoe UI", 14f, FontStyle.Bold),
                ForeColor = C_TextDark,
                Location = new Point(25, 20),
                AutoSize = true
            });

            int leftX = 25, rightX = 420, fieldY = 70, spacingY = 45;

            // Left Column
            AddLabelTextBox(formCard, "Bag ID:", leftX, fieldY, out txtBagID, 180);
            txtBagID.TextChanged += TxtBagID_TextChanged;

            AddLabelTextBox(formCard, "Donor ID:", leftX, fieldY + spacingY, out txtDonorID, 180);
            txtDonorID.TextChanged += TxtDonorID_TextChanged;

            AddLabelTextBox(formCard, "Donor Name:", leftX, fieldY + spacingY * 2, out txtDonorName, 180);
            txtDonorName.ReadOnly = true;
            txtDonorName.BackColor = Color.FromArgb(245, 245, 250);

            AddLabelCombo(formCard, "Blood Group:", leftX, fieldY + spacingY * 3, new[] { "A+", "A-", "B+", "B-", "AB+", "AB-", "O+", "O-" }, out cmbBloodGroup, 150);

            // Right Column
            AddLabelCombo(formCard, "Bag Type:", rightX, fieldY, new[] { "Whole Blood", "Platelet", "Plasma", "Red Cells" }, out cmbBagType, 160);
            cmbBagType.SelectedIndexChanged += (s, e) => UpdateExpiryDate();

            AddLabelTextBox(formCard, "Quantity:", rightX, fieldY + spacingY, out txtQuantity, 120);
            txtQuantity.Text = "1";

            AddLabelCombo(formCard, "Status:", rightX, fieldY + spacingY * 2, new[] { "Available", "In Lab", "Quarantined" }, out cmbStatus, 160);

            // Delivery Date & Time
            formCard.Controls.Add(new Label
            {
                Text = "Delivery Date:",
                Font = new Font("Segoe UI", 10),
                ForeColor = C_TextMid,
                Location = new Point(rightX, fieldY + spacingY * 3),
                AutoSize = true
            });
            dtpDeliveryDate = new DateTimePicker
            {
                Location = new Point(rightX + 100, fieldY + spacingY * 3 - 3),
                Width = 140,
                Format = DateTimePickerFormat.Short,
                Value = DateTime.Now
            };
            dtpDeliveryDate.ValueChanged += (s, e) => UpdateExpiryDate();
            formCard.Controls.Add(dtpDeliveryDate);

            formCard.Controls.Add(new Label
            {
                Text = "Delivery Time:",
                Font = new Font("Segoe UI", 10),
                ForeColor = C_TextMid,
                Location = new Point(rightX, fieldY + spacingY * 4),
                AutoSize = true
            });
            dtpDeliveryTime = new DateTimePicker
            {
                Location = new Point(rightX + 100, fieldY + spacingY * 4 - 3),
                Width = 140,
                Format = DateTimePickerFormat.Time,
                ShowUpDown = true,
                Value = DateTime.Now
            };
            formCard.Controls.Add(dtpDeliveryTime);

            // Expiry Date (auto-calculated)
            formCard.Controls.Add(new Label
            {
                Text = "Expiry Date:",
                Font = new Font("Segoe UI", 10),
                ForeColor = C_TextMid,
                Location = new Point(leftX, fieldY + spacingY * 4),
                AutoSize = true
            });
            dtpExpiryDate = new DateTimePicker
            {
                Location = new Point(leftX + 100, fieldY + spacingY * 4 - 3),
                Width = 140,
                Format = DateTimePickerFormat.Short,
                Enabled = false,
                BackColor = Color.FromArgb(245, 245, 250)
            };
            formCard.Controls.Add(dtpExpiryDate);

            // Remarks
            AddLabelTextBox(formCard, "Remarks:", leftX, fieldY + spacingY * 5, out txtRemarks, 350);
            txtRemarks.Multiline = true;
            txtRemarks.Height = 50;
            txtRemarks.MaxLength = 500;

            // Buttons
            int btnY = fieldY + spacingY * 6 + 10;

            btnSave = new Button
            {
                Text = "💾 Save Bag",
                Location = new Point(rightX + 100, btnY),
                Size = new Size(130, 40),
                BackColor = brickRed,
                ForeColor = C_White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnSave.FlatAppearance.BorderSize = 0;
            SetRoundedRegion(btnSave, 8);
            btnSave.Click += BtnSave_Click;
            formCard.Controls.Add(btnSave);

            btnClear = new Button
            {
                Text = "⟳ Clear Form",
                Location = new Point(rightX + 250, btnY),
                Size = new Size(110, 40),
                BackColor = Color.FromArgb(100, 110, 125),
                ForeColor = C_White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnClear.FlatAppearance.BorderSize = 0;
            SetRoundedRegion(btnClear, 8);
            btnClear.Click += (s, e) => ClearForm();
            formCard.Controls.Add(btnClear);

            y += formCard.Height + 30;

            // History Header
            Panel headerPanel = new Panel
            {
                Location = new Point(0, y),
                Width = pnlContent.ClientSize.Width - 60,
                Height = 35,
                BackColor = Color.Transparent
            };
            pnlContent.Controls.Add(headerPanel);

            Label lblHistory = new Label
            {
                Text = "📋 Recently Received Bags",
                Font = new Font("Segoe UI", 14f, FontStyle.Bold),
                ForeColor = C_TextDark,
                Location = new Point(0, 5),
                AutoSize = true
            };
            headerPanel.Controls.Add(lblHistory);
            y += 35;

            dgvBags = CreateStyledGrid();
            dgvBags.Location = new Point(0, y);
            dgvBags.Height = 250;
            dgvBags.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
            dgvBags.SelectionChanged += DgvBags_SelectionChanged;
            WrapInCard(dgvBags, pnlContent, new Point(0, y), 250);
            FixDataGridView(dgvBags);

            Panel bottomSpacer = new Panel { Height = 30, BackColor = Color.Transparent, Dock = DockStyle.Bottom };
            pnlContent.Controls.Add(bottomSpacer);

            // Resize event
            pnlContent.Resize += (s, e) =>
            {
                if (lblStatus != null)
                    lblStatus.Location = new Point(pnlContent.Width - 270, 20);
            };
        }

        // =========================================================
        // ENSURE BLOOD BAGS TABLE EXISTS
        // =========================================================
        private void EnsureBloodBagsTableExists()
        {
            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();

                    string createTableSql = @"
                        IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='BloodBags' AND xtype='U')
                        CREATE TABLE BloodBags (
                            BloodBagID INT IDENTITY(1,1) PRIMARY KEY,
                            BagID NVARCHAR(50) UNIQUE NOT NULL,
                            DonorID NVARCHAR(50),
                            DonorName NVARCHAR(200),
                            BloodGroup NVARCHAR(10),
                            ComponentType NVARCHAR(50),
                            Quantity INT DEFAULT 1,
                            Volume INT,
                            CollectionDate DATE,
                            ExpiryDate DATE,
                            StorageLocation NVARCHAR(200),
                            Status NVARCHAR(50),
                            LabType NVARCHAR(20),
                            Remarks NVARCHAR(500),
                            CreatedAt DATETIME DEFAULT GETDATE(),
                            CreatedBy INT,
                            UpdatedAt DATETIME
                        )";

                    using (var cmd = new SqlCommand(createTableSql, conn))
                    {
                        cmd.ExecuteNonQuery();
                    }

                    // Create indexes
                    string createIndexes = @"
                        IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name='IX_BloodBags_BagID')
                        CREATE INDEX IX_BloodBags_BagID ON BloodBags(BagID);
                        
                        IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name='IX_BloodBags_Status')
                        CREATE INDEX IX_BloodBags_Status ON BloodBags(Status)";

                    using (var cmd = new SqlCommand(createIndexes, conn))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"EnsureBloodBagsTableExists Error: {ex.Message}");
            }
        }

        // =========================================================
        // GENERATE AUTO BAG ID
        // =========================================================
        private async void GenerateBagID()
        {
            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    await conn.OpenAsync();

                    string checkTable = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'BloodBags'";
                    using (var checkCmd = new SqlCommand(checkTable, conn))
                    {
                        int tableExists = Convert.ToInt32(await checkCmd.ExecuteScalarAsync());
                        if (tableExists == 0)
                        {
                            txtBagID.Text = $"BB-{DateTime.Now:yyyyMMdd}-001";
                            currentBagID = txtBagID.Text;
                            return;
                        }
                    }

                    string sql = "SELECT COUNT(*) FROM BloodBags";
                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        int count = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                        txtBagID.Text = $"BB-{DateTime.Now:yyyyMMdd}-{(count + 1):D3}";
                        currentBagID = txtBagID.Text;
                    }
                }
            }
            catch
            {
                txtBagID.Text = $"BB-{DateTime.Now:yyyyMMdd}-001";
                currentBagID = txtBagID.Text;
            }
        }

        // =========================================================
        // CHECK FOR DUPLICATE BAG ID
        // =========================================================
        private async void TxtBagID_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtBagID.Text)) return;
            if (txtBagID.Text == currentBagID) return;

            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    await conn.OpenAsync();

                    string sql = "SELECT COUNT(*) FROM BloodBags WHERE BagID = @BagID";
                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@BagID", txtBagID.Text.Trim());
                        int count = Convert.ToInt32(await cmd.ExecuteScalarAsync());

                        if (count > 0)
                        {
                            MessageBox.Show($"Bag ID '{txtBagID.Text}' already exists.\n\nA new unique ID will be generated.",
                                "Duplicate Bag ID", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            GenerateBagID();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"TxtBagID_TextChanged Error: {ex.Message}");
            }
        }

        // =========================================================
        // AUTO FILL DONOR DETAILS
        // =========================================================
        private async void TxtDonorID_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtDonorID.Text)) return;

            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    await conn.OpenAsync();

                    string sql = @"
                        SELECT 
                            FullName,
                            BloodGroup
                        FROM Donors 
                        WHERE DonorID = @DonorID OR UserID = @DonorID";

                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@DonorID", txtDonorID.Text.Trim());
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                txtDonorName.Text = reader["FullName"].ToString();
                                string bloodGroup = reader["BloodGroup"].ToString();
                                int index = cmbBloodGroup.Items.IndexOf(bloodGroup);
                                if (index >= 0)
                                    cmbBloodGroup.SelectedIndex = index;

                                UpdateStatus($"✅ Donor found: {txtDonorName.Text}", Color.FromArgb(16, 185, 129));
                            }
                            else
                            {
                                txtDonorName.Text = "";
                                UpdateStatus("⚠️ Donor not found", Color.FromArgb(245, 158, 11));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"TxtDonorID_TextChanged Error: {ex.Message}");
            }
        }

        // =========================================================
        // UPDATE EXPIRY DATE BASED ON BAG TYPE
        // =========================================================
        private void UpdateExpiryDate()
        {
            string bagType = cmbBagType.SelectedItem?.ToString();
            DateTime collectionDate = dtpDeliveryDate.Value;
            DateTime expiryDate;

            switch (bagType)
            {
                case "Whole Blood":
                case "Red Cells":
                    expiryDate = collectionDate.AddDays(35);
                    break;
                case "Platelet":
                    expiryDate = collectionDate.AddDays(5);
                    break;
                case "Plasma":
                    expiryDate = collectionDate.AddDays(365);
                    break;
                default:
                    expiryDate = collectionDate.AddDays(35);
                    break;
            }

            dtpExpiryDate.Value = expiryDate;
        }

        // =========================================================
        // LOAD RECEIVED BAGS FROM DATABASE
        // =========================================================
        private async void LoadReceivedBagsFromDatabase()
        {
            try
            {
                UpdateStatus("🔄 Loading received bags...", Color.FromArgb(59, 130, 246));

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
                            BagID as [Bag ID],
                            DonorName as [Donor Name],
                            BloodGroup as [Blood Group],
                            ComponentType as [Bag Type],
                            Quantity,
                            Status,
                            LabType as [Lab Type],
                            FORMAT(CollectionDate, 'dd-MMM-yyyy') as [Delivery Date],
                            FORMAT(ExpiryDate, 'dd-MMM-yyyy') as [Expiry Date],
                            FORMAT(CreatedAt, 'dd-MMM-yyyy HH:mm') as [Received At]
                        FROM BloodBags 
                        WHERE ComponentType IN ('Whole Blood', 'Platelet', 'Plasma', 'Red Cells')
                        ORDER BY CreatedAt DESC, BagID DESC";

                    SqlDataAdapter da = new SqlDataAdapter(query, conn);
                    receivedBagsData = new DataTable();
                    da.Fill(receivedBagsData);

                    dgvBags.DataSource = receivedBagsData;
                    FormatHistoryGrid();
                    NoSort(dgvBags);

                    UpdateStatus($"✅ Loaded {receivedBagsData.Rows.Count} received bag(s)", Color.FromArgb(16, 185, 129));
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LoadReceivedBagsFromDatabase Error: {ex.Message}");
                dgvBags.DataSource = null;
                UpdateStatus($"❌ Database error: {ex.Message}", Color.FromArgb(220, 38, 38));
            }
        }

        private void SearchReceivedBags()
        {
            if (receivedBagsData == null) return;

            string bagID = txtSearchBagID.Text.Trim().ToLower();
            string donor = txtSearchDonor.Text.Trim().ToLower();
            string labType = cmbSearchLabType.SelectedItem?.ToString();

            DataTable filteredDt = receivedBagsData.Clone();

            foreach (DataRow row in receivedBagsData.Rows)
            {
                bool include = true;

                if (!string.IsNullOrEmpty(bagID))
                {
                    string rowBagID = row["Bag ID"]?.ToString().ToLower() ?? "";
                    if (!rowBagID.Contains(bagID)) include = false;
                }

                if (include && !string.IsNullOrEmpty(donor))
                {
                    string rowDonor = row["Donor Name"]?.ToString().ToLower() ?? "";
                    if (!rowDonor.Contains(donor)) include = false;
                }

                if (include && labType != "All")
                {
                    string rowLabType = row["Lab Type"]?.ToString() ?? "";
                    if (rowLabType != labType) include = false;
                }

                if (include)
                    filteredDt.ImportRow(row);
            }

            dgvBags.DataSource = filteredDt;
            UpdateStatus($"🔍 Found {filteredDt.Rows.Count} matching bag(s)", Color.FromArgb(59, 130, 246));
        }

        private void FormatHistoryGrid()
        {
            if (dgvBags.Columns.Count == 0) return;

            if (dgvBags.Columns.Contains("Bag ID"))
                dgvBags.Columns["Bag ID"].FillWeight = 10;
            if (dgvBags.Columns.Contains("Donor Name"))
                dgvBags.Columns["Donor Name"].FillWeight = 15;
            if (dgvBags.Columns.Contains("Blood Group"))
                dgvBags.Columns["Blood Group"].FillWeight = 8;
            if (dgvBags.Columns.Contains("Bag Type"))
                dgvBags.Columns["Bag Type"].FillWeight = 10;
            if (dgvBags.Columns.Contains("Quantity"))
                dgvBags.Columns["Quantity"].FillWeight = 5;
            if (dgvBags.Columns.Contains("Status"))
                dgvBags.Columns["Status"].FillWeight = 10;
            if (dgvBags.Columns.Contains("Lab Type"))
                dgvBags.Columns["Lab Type"].FillWeight = 8;
            if (dgvBags.Columns.Contains("Delivery Date"))
                dgvBags.Columns["Delivery Date"].FillWeight = 10;
            if (dgvBags.Columns.Contains("Expiry Date"))
                dgvBags.Columns["Expiry Date"].FillWeight = 10;
            if (dgvBags.Columns.Contains("Received At"))
                dgvBags.Columns["Received At"].FillWeight = 14;
        }

        // =========================================================
        // LOAD SELECTED RECORD FOR VIEWING
        // =========================================================
        private void DgvBags_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvBags.SelectedRows.Count > 0 && dgvBags.SelectedRows[0].DataBoundItem != null)
            {
                DataRowView row = dgvBags.SelectedRows[0].DataBoundItem as DataRowView;
                if (row != null)
                {
                    isEditMode = true;
                    btnSave.Text = "✏️ Update Bag";

                    txtBagID.Text = row["Bag ID"]?.ToString();
                    txtDonorName.Text = row["Donor Name"]?.ToString();
                    SetComboValue(cmbBloodGroup, row["Blood Group"]?.ToString());
                    SetComboValue(cmbBagType, row["Bag Type"]?.ToString());
                    txtQuantity.Text = row["Quantity"]?.ToString();
                    SetComboValue(cmbStatus, row["Status"]?.ToString());

                    string labType = row["Lab Type"]?.ToString();
                    if (labType == "In Lab")
                        tcLabType.SelectedIndex = 0;
                    else if (labType == "Out Lab")
                        tcLabType.SelectedIndex = 1;

                    if (DateTime.TryParse(row["Delivery Date"]?.ToString(), out DateTime deliveryDate))
                        dtpDeliveryDate.Value = deliveryDate;

                    UpdateStatus("📋 Viewing existing bag record", Color.FromArgb(59, 130, 246));
                }
            }
        }

        private void SetComboValue(ComboBox cmb, string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                int index = cmb.Items.IndexOf(value);
                if (index >= 0)
                    cmb.SelectedIndex = index;
            }
        }

        // =========================================================
        // SAVE BAG - INSERT OR UPDATE
        // =========================================================
        private async void BtnSave_Click(object sender, EventArgs e)
        {
            // Validation
            if (string.IsNullOrWhiteSpace(txtBagID.Text))
            {
                MessageBox.Show("Bag ID is required.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtDonorID.Text))
            {
                MessageBox.Show("Donor ID is required.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string labType = tcLabType.SelectedTab.Text;
            int quantity = string.IsNullOrWhiteSpace(txtQuantity.Text) ? 1 : Convert.ToInt32(txtQuantity.Text);
            int volume = quantity * 450; // Standard bag volume

            try
            {
                ShowLoading("Saving bag record...");

                using (var conn = DatabaseHelper.GetConnection())
                {
                    await conn.OpenAsync();

                    if (isEditMode)
                    {
                        // UPDATE existing record
                        string updateSql = @"
                            UPDATE BloodBags SET 
                                DonorID = @DonorID,
                                DonorName = @DonorName,
                                BloodGroup = @BloodGroup,
                                ComponentType = @ComponentType,
                                Quantity = @Quantity,
                                Volume = @Volume,
                                Status = @Status,
                                LabType = @LabType,
                                CollectionDate = @CollectionDate,
                                ExpiryDate = @ExpiryDate,
                                Remarks = @Remarks,
                                UpdatedAt = GETDATE()
                            WHERE BagID = @BagID";

                        using (var cmd = new SqlCommand(updateSql, conn))
                        {
                            cmd.Parameters.AddWithValue("@BagID", txtBagID.Text);
                            cmd.Parameters.AddWithValue("@DonorID", txtDonorID.Text);
                            cmd.Parameters.AddWithValue("@DonorName", txtDonorName.Text);
                            cmd.Parameters.AddWithValue("@BloodGroup", cmbBloodGroup.SelectedItem?.ToString() ?? "");
                            cmd.Parameters.AddWithValue("@ComponentType", cmbBagType.SelectedItem?.ToString() ?? "");
                            cmd.Parameters.AddWithValue("@Quantity", quantity);
                            cmd.Parameters.AddWithValue("@Volume", volume);
                            cmd.Parameters.AddWithValue("@Status", cmbStatus.SelectedItem?.ToString() ?? "Available");
                            cmd.Parameters.AddWithValue("@LabType", labType);
                            cmd.Parameters.AddWithValue("@CollectionDate", dtpDeliveryDate.Value);
                            cmd.Parameters.AddWithValue("@ExpiryDate", dtpExpiryDate.Value);
                            cmd.Parameters.AddWithValue("@Remarks", txtRemarks.Text ?? "");

                            await cmd.ExecuteNonQueryAsync();
                        }

                        MessageBox.Show($"Bag {txtBagID.Text} updated successfully!",
                            "Update Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        // INSERT new record
                        string insertSql = @"
                            INSERT INTO BloodBags 
                            (BagID, DonorID, DonorName, BloodGroup, ComponentType, 
                             Quantity, Volume, Status, LabType, CollectionDate, ExpiryDate, Remarks, CreatedAt, CreatedBy)
                            VALUES 
                            (@BagID, @DonorID, @DonorName, @BloodGroup, @ComponentType,
                             @Quantity, @Volume, @Status, @LabType, @CollectionDate, @ExpiryDate, @Remarks, GETDATE(), @CreatedBy)";

                        using (var cmd = new SqlCommand(insertSql, conn))
                        {
                            cmd.Parameters.AddWithValue("@BagID", txtBagID.Text);
                            cmd.Parameters.AddWithValue("@DonorID", txtDonorID.Text);
                            cmd.Parameters.AddWithValue("@DonorName", txtDonorName.Text);
                            cmd.Parameters.AddWithValue("@BloodGroup", cmbBloodGroup.SelectedItem?.ToString() ?? "");
                            cmd.Parameters.AddWithValue("@ComponentType", cmbBagType.SelectedItem?.ToString() ?? "");
                            cmd.Parameters.AddWithValue("@Quantity", quantity);
                            cmd.Parameters.AddWithValue("@Volume", volume);
                            cmd.Parameters.AddWithValue("@Status", cmbStatus.SelectedItem?.ToString() ?? "Available");
                            cmd.Parameters.AddWithValue("@LabType", labType);
                            cmd.Parameters.AddWithValue("@CollectionDate", dtpDeliveryDate.Value);
                            cmd.Parameters.AddWithValue("@ExpiryDate", dtpExpiryDate.Value);
                            cmd.Parameters.AddWithValue("@Remarks", txtRemarks.Text ?? "");
                            cmd.Parameters.AddWithValue("@CreatedBy", SessionManager.CurrentUserID);

                            await cmd.ExecuteNonQueryAsync();
                        }

                        MessageBox.Show($"Bag {txtBagID.Text} received successfully in {labType}!\n\nExpiry Date: {dtpExpiryDate.Value:dd-MMM-yyyy}",
                            "Save Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        // Update donor's donation count
                        await UpdateDonorDonationCountAsync();
                    }
                }

                // Log the activity
                AuditHelper.Log(isEditMode ? "Update Bag" : "Receive Bag", "BloodBag",
                    $"Bag {txtBagID.Text} - Donor: {txtDonorName.Text} - Lab: {labType}");

                // Refresh the grid
                LoadReceivedBagsFromDatabase();
                ClearForm();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"BtnSave_Click Error: {ex.Message}");
                MessageBox.Show($"Error saving bag record: {ex.Message}",
                    "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                UpdateStatus($"❌ Save failed: {ex.Message}", Color.FromArgb(220, 38, 38));
            }
            finally
            {
                HideLoading();
            }
        }

        private async Task UpdateDonorDonationCountAsync()
        {
            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    await conn.OpenAsync();

                    string sql = @"
                        UPDATE Donors SET 
                            TotalDonations = ISNULL(TotalDonations, 0) + 1,
                            LastDonationDate = @DonationDate
                        WHERE DonorID = @DonorID OR UserID = @DonorID";

                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@DonorID", txtDonorID.Text);
                        cmd.Parameters.AddWithValue("@DonationDate", dtpDeliveryDate.Value);
                        await cmd.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"UpdateDonorDonationCountAsync Error: {ex.Message}");
            }
        }

        private void ClearForm()
        {
            isEditMode = false;
            btnSave.Text = "💾 Save Bag";

            txtDonorID.Clear();
            txtDonorName.Clear();
            txtQuantity.Text = "1";
            txtRemarks.Clear();

            cmbBagType.SelectedIndex = 0;
            cmbBloodGroup.SelectedIndex = 0;
            cmbStatus.SelectedIndex = 0;

            dtpDeliveryDate.Value = DateTime.Now;
            dtpDeliveryTime.Value = DateTime.Now;
            tcLabType.SelectedIndex = 0;

            GenerateBagID();
            UpdateExpiryDate();

            UpdateStatus("✅ Ready - Enter donor ID to receive bag", Color.FromArgb(16, 185, 129));
        }

        private void UpdateStatus(string message, Color color)
        {
            if (lblStatus != null && !lblStatus.IsDisposed)
            {
                lblStatus.Text = message;
                lblStatus.ForeColor = color;
            }
        }

        private void ShowLoading(string message)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action<string>(ShowLoading), message);
                return;
            }
            Cursor = Cursors.WaitCursor;
            UpdateStatus($"⏳ {message}", Color.FromArgb(59, 130, 246));
        }

        private void HideLoading()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(HideLoading));
                return;
            }
            Cursor = Cursors.Default;
        }

        private void AddLabelTextBox(Panel parent, string labelText, int x, int y, out TextBox txt, int width)
        {
            parent.Controls.Add(new Label
            {
                Text = labelText,
                Font = new Font("Segoe UI", 10),
                ForeColor = C_TextMid,
                Location = new Point(x, y),
                AutoSize = true
            });
            txt = new TextBox
            {
                Location = new Point(x + 90, y - 3),
                Width = width,
                Font = new Font("Segoe UI", 10),
                BorderStyle = BorderStyle.FixedSingle
            };
            parent.Controls.Add(txt);
        }

        private void AddLabelCombo(Panel parent, string labelText, int x, int y, string[] items, out ComboBox cmb, int width)
        {
            parent.Controls.Add(new Label
            {
                Text = labelText,
                Font = new Font("Segoe UI", 10),
                ForeColor = C_TextMid,
                Location = new Point(x, y),
                AutoSize = true
            });
            cmb = new ComboBox
            {
                Location = new Point(x + 90, y - 3),
                Width = width,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10)
            };
            cmb.Items.AddRange(items);
            if (items.Length > 0) cmb.SelectedIndex = 0;
            parent.Controls.Add(cmb);
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