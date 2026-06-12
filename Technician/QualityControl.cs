using BloodBankManagementSystem.Classes.Database;
using BloodBankManagementSystem.Classes.Helpers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BloodBankManagementSystem.Forms.Technician
{
    public partial class QualityControl : BaseTechnicianForm
    {
        private Panel pnlFormCard;
        private DataGridView dgvQCTests;
        private TextBox txtTestID, txtComponentID, txtTechnician, txtRemarks, txtSearchComponent;
        private ComboBox cmbComponentType, cmbTestType, cmbResult, cmbSearchResult;
        private NumericUpDown numTemperature, numVolume, numPH;
        private DateTimePicker dtpTestDate;
        private Button btnSave, btnClear, btnRefresh, btnSearch;
        private Label lblStatus;
        private DataTable qcData;
        private bool isEditMode = false;
        private int currentQCID = 0;

        // QC Standards by component type
        private readonly Dictionary<string, (decimal minTemp, decimal maxTemp, int minVolume, int maxVolume, decimal minPH, decimal maxPH)> standards;

        public QualityControl()
        {
            this.Text = "Blood Bank Management System – Quality Control";

            // Initialize QC standards
            standards = new Dictionary<string, (decimal, decimal, int, int, decimal, decimal)>
            {
                { "Whole Blood", (2.0m, 6.0m, 400, 500, 7.0m, 7.4m) },
                { "Plasma", (-30m, -20m, 200, 300, 7.0m, 7.4m) },
                { "Red Cells", (2.0m, 6.0m, 250, 350, 6.8m, 7.2m) },
                { "Platelets", (20.0m, 24.0m, 45, 55, 6.8m, 7.2m) },
                { "Cryoprecipitate", (-30m, -20m, 10, 25, 6.5m, 7.0m) }
            };

            BuildLayout();
            BuildSidebar("Quality Control");
            BuildTopBar("Quality Control Management");
            BuildContentArea();

            // Load data from database
            LoadQCHistory();
            EnsureQCTableExists();
            SetDefaultTechnician();
            GenerateTestID();
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
                Height = 45,
                BackColor = C_CardBg,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            SetRoundedRegion(pnlSearch, 12);
            AddDropShadow(pnlSearch);
            pnlContent.Controls.Add(pnlSearch);

            Label lblSearch = new Label
            {
                Text = "Search Component:",
                Font = new Font("Segoe UI", 10),
                ForeColor = C_TextMid,
                Location = new Point(20, 12),
                AutoSize = true
            };
            pnlSearch.Controls.Add(lblSearch);

            txtSearchComponent = new TextBox
            {
                Location = new Point(150, 9),
                Width = 180,
                Font = new Font("Segoe UI", 10),
                BorderStyle = BorderStyle.FixedSingle
            };
            pnlSearch.Controls.Add(txtSearchComponent);

            cmbSearchResult = new ComboBox
            {
                Location = new Point(350, 9),
                Width = 120,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9)
            };
            cmbSearchResult.Items.AddRange(new[] { "All", "Pass", "Fail", "Pending Review" });
            cmbSearchResult.SelectedIndex = 0;
            pnlSearch.Controls.Add(cmbSearchResult);

            btnSearch = new Button
            {
                Text = "🔍 Search",
                Location = new Point(490, 7),
                Size = new Size(90, 30),
                BackColor = brickRed,
                ForeColor = C_White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnSearch.FlatAppearance.BorderSize = 0;
            SetRoundedRegion(btnSearch, 6);
            btnSearch.Click += (s, e) => SearchQCTests();
            pnlSearch.Controls.Add(btnSearch);

            y += pnlSearch.Height + 20;

            // Form Card
            pnlFormCard = new Panel
            {
                Location = new Point(0, y),
                Width = pnlContent.ClientSize.Width - 60,
                Height = 420,
                BackColor = C_CardBg,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            SetRoundedRegion(pnlFormCard, 15);
            AddDropShadow(pnlFormCard);
            pnlContent.Controls.Add(pnlFormCard);

            pnlFormCard.Controls.Add(new Label
            {
                Text = "🔬 Quality Control Test",
                Font = new Font("Segoe UI", 14f, FontStyle.Bold),
                ForeColor = C_TextDark,
                Location = new Point(25, 20),
                AutoSize = true
            });

            int leftX = 25, rightX = 420, fieldY = 70, spacingY = 45;

            // Left Column
            AddLabelTextBox("Test ID:", leftX, fieldY, out txtTestID, 180);
            txtTestID.ReadOnly = true;
            txtTestID.BackColor = Color.FromArgb(245, 245, 250);

            AddLabelTextBox("Component ID:", leftX, fieldY + spacingY, out txtComponentID, 180);
            txtComponentID.TextChanged += TxtComponentID_TextChanged;

            AddLabelCombo("Component Type:", leftX, fieldY + spacingY * 2, new[] { "Whole Blood", "Plasma", "Red Cells", "Platelets", "Cryoprecipitate" }, out cmbComponentType, 180);
            cmbComponentType.SelectedIndexChanged += (s, e) => UpdateQCRanges();

            AddLabelCombo("Test Type:", leftX, fieldY + spacingY * 3, new[] { "Sterility Test", "Endotoxin Test", "pH Test", "Volume Test", "Hemoglobin Test", "Cell Count" }, out cmbTestType, 180);

            AddLabelTextBox("Technician:", leftX, fieldY + spacingY * 4, out txtTechnician, 200);

            // Right Column
            pnlFormCard.Controls.Add(new Label
            {
                Text = "Temperature (°C):",
                Font = new Font("Segoe UI", 10),
                ForeColor = C_TextMid,
                Location = new Point(rightX, fieldY),
                AutoSize = true
            });
            numTemperature = new NumericUpDown
            {
                Location = new Point(rightX + 150, fieldY - 3),
                Width = 100,
                Minimum = -80,
                Maximum = 50,
                DecimalPlaces = 1,
                Value = 4,
                Font = new Font("Segoe UI", 10)
            };
            numTemperature.ValueChanged += (s, e) => AutoEvaluateResult();
            pnlFormCard.Controls.Add(numTemperature);

            pnlFormCard.Controls.Add(new Label
            {
                Text = "Volume (mL):",
                Font = new Font("Segoe UI", 10),
                ForeColor = C_TextMid,
                Location = new Point(rightX, fieldY + spacingY),
                AutoSize = true
            });
            numVolume = new NumericUpDown
            {
                Location = new Point(rightX + 150, fieldY + spacingY - 3),
                Width = 100,
                Minimum = 0,
                Maximum = 1000,
                Value = 450,
                Font = new Font("Segoe UI", 10)
            };
            numVolume.ValueChanged += (s, e) => AutoEvaluateResult();
            pnlFormCard.Controls.Add(numVolume);

            pnlFormCard.Controls.Add(new Label
            {
                Text = "pH Level:",
                Font = new Font("Segoe UI", 10),
                ForeColor = C_TextMid,
                Location = new Point(rightX, fieldY + spacingY * 2),
                AutoSize = true
            });
            numPH = new NumericUpDown
            {
                Location = new Point(rightX + 150, fieldY + spacingY * 2 - 3),
                Width = 100,
                Minimum = 0,
                Maximum = 14,
                DecimalPlaces = 1,
                Value = 7.2m,
                Increment = 0.1m,
                Font = new Font("Segoe UI", 10)
            };
            numPH.ValueChanged += (s, e) => AutoEvaluateResult();
            pnlFormCard.Controls.Add(numPH);

            AddLabelCombo("Result:", rightX, fieldY + spacingY * 3, new[] { "Pass", "Fail", "Pending Review" }, out cmbResult, 150);

            pnlFormCard.Controls.Add(new Label
            {
                Text = "Test Date:",
                Font = new Font("Segoe UI", 10),
                ForeColor = C_TextMid,
                Location = new Point(rightX, fieldY + spacingY * 4),
                AutoSize = true
            });
            dtpTestDate = new DateTimePicker
            {
                Location = new Point(rightX + 150, fieldY + spacingY * 4 - 3),
                Width = 150,
                Format = DateTimePickerFormat.Short,
                Value = DateTime.Now
            };
            pnlFormCard.Controls.Add(dtpTestDate);

            // Remarks
            AddLabelTextBox("Remarks:", leftX, fieldY + spacingY * 5, out txtRemarks, 350);
            txtRemarks.Multiline = true;
            txtRemarks.Height = 50;
            txtRemarks.MaxLength = 500;

            // Buttons
            int btnY = fieldY + spacingY * 6 + 10;

            btnSave = new Button
            {
                Text = "💾 Save Test Result",
                Location = new Point(rightX, btnY),
                Size = new Size(150, 40),
                BackColor = brickRed,
                ForeColor = C_White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnSave.FlatAppearance.BorderSize = 0;
            SetRoundedRegion(btnSave, 8);
            btnSave.Click += BtnSave_Click;
            pnlFormCard.Controls.Add(btnSave);

            btnClear = new Button
            {
                Text = "⟳ Clear Form",
                Location = new Point(rightX + 170, btnY),
                Size = new Size(110, 40),
                BackColor = Color.FromArgb(100, 110, 125),
                ForeColor = C_White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnClear.FlatAppearance.BorderSize = 0;
            SetRoundedRegion(btnClear, 8);
            btnClear.Click += (s, e) => ClearForm();
            pnlFormCard.Controls.Add(btnClear);

            btnRefresh = new Button
            {
                Text = "🔄 Refresh List",
                Location = new Point(rightX + 300, btnY),
                Size = new Size(110, 40),
                BackColor = Color.FromArgb(60, 70, 80),
                ForeColor = C_White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnRefresh.FlatAppearance.BorderSize = 0;
            SetRoundedRegion(btnRefresh, 8);
            btnRefresh.Click += (s, e) => LoadQCHistory();
            pnlFormCard.Controls.Add(btnRefresh);

            y += pnlFormCard.Height + 30;

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
                Text = "📋 Quality Control Tests History",
                Font = new Font("Segoe UI", 14f, FontStyle.Bold),
                ForeColor = C_TextDark,
                Location = new Point(0, 5),
                AutoSize = true
            };
            headerPanel.Controls.Add(lblHistory);
            y += 35;

            dgvQCTests = CreateStyledGrid();
            dgvQCTests.Location = new Point(0, y);
            dgvQCTests.Height = 250;
            dgvQCTests.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            dgvQCTests.CellFormatting += DgvQCTests_CellFormatting;
            dgvQCTests.SelectionChanged += DgvQCTests_SelectionChanged;
            WrapInCard(dgvQCTests, pnlContent, new Point(0, y), 250);
            FixDataGridView(dgvQCTests);

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
        // ENSURE QC TABLE EXISTS
        // =========================================================
        private void EnsureQCTableExists()
        {
            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();

                    string createTableSql = @"
                        IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='QualityControlTests' AND xtype='U')
                        CREATE TABLE QualityControlTests (
                            QCID INT IDENTITY(1,1) PRIMARY KEY,
                            TestID NVARCHAR(50) UNIQUE NOT NULL,
                            ComponentID NVARCHAR(50),
                            ComponentType NVARCHAR(50),
                            TestType NVARCHAR(100),
                            Temperature DECIMAL(5,1),
                            Volume INT,
                            PH DECIMAL(4,1),
                            Result NVARCHAR(20),
                            Technician NVARCHAR(100),
                            TestDate DATE,
                            Remarks NVARCHAR(500),
                            PerformedBy INT,
                            CreatedAt DATETIME DEFAULT GETDATE(),
                            UpdatedAt DATETIME
                        )";

                    using (var cmd = new SqlCommand(createTableSql, conn))
                    {
                        cmd.ExecuteNonQuery();
                    }

                    // Create indexes
                    string createIndexes = @"
                        IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name='IX_QualityControlTests_TestID')
                        CREATE INDEX IX_QualityControlTests_TestID ON QualityControlTests(TestID);
                        
                        IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name='IX_QualityControlTests_TestDate')
                        CREATE INDEX IX_QualityControlTests_TestDate ON QualityControlTests(TestDate)";

                    using (var cmd = new SqlCommand(createIndexes, conn))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"EnsureQCTableExists Error: {ex.Message}");
            }
        }

        // =========================================================
        // GENERATE AUTO TEST ID
        // =========================================================
        private async void GenerateTestID()
        {
            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    await conn.OpenAsync();

                    string checkTable = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'QualityControlTests'";
                    using (var checkCmd = new SqlCommand(checkTable, conn))
                    {
                        int tableExists = Convert.ToInt32(await checkCmd.ExecuteScalarAsync());
                        if (tableExists == 0)
                        {
                            txtTestID.Text = $"QC-{DateTime.Now:yyyyMMdd}-001";
                            return;
                        }
                    }

                    string sql = "SELECT COUNT(*) FROM QualityControlTests";
                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        int count = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                        txtTestID.Text = $"QC-{DateTime.Now:yyyyMMdd}-{(count + 1):D3}";
                    }
                }
            }
            catch
            {
                txtTestID.Text = $"QC-{DateTime.Now:yyyyMMdd}-001";
            }
        }

        private void SetDefaultTechnician()
        {
            txtTechnician.Text = SessionManager.CurrentFullName;
        }

        // =========================================================
        // AUTO FILL COMPONENT DETAILS
        // =========================================================
        private async void TxtComponentID_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtComponentID.Text)) return;

            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    await conn.OpenAsync();

                    string sql = @"
                        SELECT 
                            ComponentType,
                            Volume
                        FROM BloodBags 
                        WHERE BagID = @ComponentID";

                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@ComponentID", txtComponentID.Text.Trim());
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            if (reader.Read())
                            {
                                string compType = reader["ComponentType"].ToString();
                                int index = cmbComponentType.Items.IndexOf(compType);
                                if (index >= 0)
                                    cmbComponentType.SelectedIndex = index;

                                if (reader["Volume"] != DBNull.Value)
                                    numVolume.Value = Convert.ToInt32(reader["Volume"]);

                                UpdateStatus($"✅ Component found: {compType}", Color.FromArgb(16, 185, 129));
                            }
                            else
                            {
                                UpdateStatus("⚠️ Component not found", Color.FromArgb(245, 158, 11));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"TxtComponentID_TextChanged Error: {ex.Message}");
            }
        }

        // =========================================================
        // UPDATE QC RANGES BASED ON COMPONENT TYPE
        // =========================================================
        private void UpdateQCRanges()
        {
            string compType = cmbComponentType.SelectedItem?.ToString();
            if (string.IsNullOrEmpty(compType)) return;

            if (standards.ContainsKey(compType))
            {
                var standard = standards[compType];
                numTemperature.Minimum = standard.minTemp;
                numTemperature.Maximum = standard.maxTemp;
                numVolume.Minimum = standard.minVolume;
                numVolume.Maximum = standard.maxVolume;
                numPH.Minimum = standard.minPH;
                numPH.Maximum = standard.maxPH;

                // Set default values to middle of range
                numTemperature.Value = (standard.minTemp + standard.maxTemp) / 2;
                numVolume.Value = (standard.minVolume + standard.maxVolume) / 2;
                numPH.Value = (standard.minPH + standard.maxPH) / 2;
            }
        }

        // =========================================================
        // AUTO EVALUATE RESULT
        // =========================================================
        private void AutoEvaluateResult()
        {
            string compType = cmbComponentType.SelectedItem?.ToString();
            if (string.IsNullOrEmpty(compType)) return;

            if (!standards.ContainsKey(compType)) return;

            var standard = standards[compType];

            bool tempOk = numTemperature.Value >= standard.minTemp && numTemperature.Value <= standard.maxTemp;
            bool volumeOk = numVolume.Value >= standard.minVolume && numVolume.Value <= standard.maxVolume;
            bool phOk = numPH.Value >= standard.minPH && numPH.Value <= standard.maxPH;

            if (tempOk && volumeOk && phOk)
            {
                cmbResult.SelectedItem = "Pass";
                UpdateStatus("✅ All parameters within acceptable range", Color.FromArgb(16, 185, 129));
            }
            else
            {
                cmbResult.SelectedItem = "Fail";
                string issues = "";
                if (!tempOk) issues += $"Temp: {numTemperature.Value}°C (should be {standard.minTemp}-{standard.maxTemp}), ";
                if (!volumeOk) issues += $"Volume: {numVolume.Value}mL (should be {standard.minVolume}-{standard.maxVolume}), ";
                if (!phOk) issues += $"pH: {numPH.Value} (should be {standard.minPH}-{standard.maxPH})";
                UpdateStatus($"⚠️ Failed: {issues.TrimEnd(',', ' ')}", Color.FromArgb(245, 158, 11));
            }
        }

        // =========================================================
        // LOAD QC HISTORY FROM DATABASE
        // =========================================================
        private async void LoadQCHistory()
        {
            try
            {
                UpdateStatus("🔄 Loading QC history...", Color.FromArgb(59, 130, 246));

                using (var conn = DatabaseHelper.GetConnection())
                {
                    await conn.OpenAsync();

                    string checkTable = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'QualityControlTests'";
                    using (var checkCmd = new SqlCommand(checkTable, conn))
                    {
                        int tableExists = Convert.ToInt32(await checkCmd.ExecuteScalarAsync());
                        if (tableExists == 0)
                        {
                            dgvQCTests.DataSource = null;
                            UpdateStatus("⚠️ Table 'QualityControlTests' not found", Color.FromArgb(245, 158, 11));
                            return;
                        }
                    }

                    string query = @"
                        SELECT 
                            QCID,
                            TestID as [Test ID],
                            ComponentID as [Component ID],
                            ComponentType as [Component Type],
                            TestType as [Test Type],
                            Temperature as [Temp (°C)],
                            Volume as [Volume (mL)],
                            PH as [pH],
                            Result,
                            Technician,
                            FORMAT(TestDate, 'dd-MMM-yyyy') as [Test Date],
                            Remarks,
                            FORMAT(CreatedAt, 'dd-MMM-yyyy HH:mm') as [Recorded At]
                        FROM QualityControlTests 
                        ORDER BY TestDate DESC, QCID DESC";

                    SqlDataAdapter da = new SqlDataAdapter(query, conn);
                    qcData = new DataTable();
                    da.Fill(qcData);

                    dgvQCTests.DataSource = qcData;
                    FormatHistoryGrid();
                    NoSort(dgvQCTests);

                    UpdateStatus($"✅ Loaded {qcData.Rows.Count} QC test(s)", Color.FromArgb(16, 185, 129));
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LoadQCHistory Error: {ex.Message}");
                dgvQCTests.DataSource = null;
                UpdateStatus($"❌ Database error: {ex.Message}", Color.FromArgb(220, 38, 38));
            }
        }

        private void SearchQCTests()
        {
            if (qcData == null) return;

            string searchTerm = txtSearchComponent.Text.Trim().ToLower();
            string resultFilter = cmbSearchResult.SelectedItem?.ToString();

            DataTable filteredDt = qcData.Clone();

            foreach (DataRow row in qcData.Rows)
            {
                bool include = true;

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    string compID = row["Component ID"]?.ToString().ToLower() ?? "";
                    string compType = row["Component Type"]?.ToString().ToLower() ?? "";
                    if (!compID.Contains(searchTerm) && !compType.Contains(searchTerm))
                        include = false;
                }

                if (include && resultFilter != "All")
                {
                    string result = row["Result"]?.ToString() ?? "";
                    if (result != resultFilter)
                        include = false;
                }

                if (include)
                    filteredDt.ImportRow(row);
            }

            dgvQCTests.DataSource = filteredDt;
            UpdateStatus($"🔍 Found {filteredDt.Rows.Count} matching test(s)", Color.FromArgb(59, 130, 246));
        }

        private void FormatHistoryGrid()
        {
            if (dgvQCTests.Columns.Count == 0) return;

            if (dgvQCTests.Columns.Contains("QCID"))
                dgvQCTests.Columns["QCID"].Visible = false;

            if (dgvQCTests.Columns.Contains("Test ID"))
                dgvQCTests.Columns["Test ID"].FillWeight = 8;
            if (dgvQCTests.Columns.Contains("Component ID"))
                dgvQCTests.Columns["Component ID"].FillWeight = 8;
            if (dgvQCTests.Columns.Contains("Component Type"))
                dgvQCTests.Columns["Component Type"].FillWeight = 10;
            if (dgvQCTests.Columns.Contains("Test Type"))
                dgvQCTests.Columns["Test Type"].FillWeight = 10;
            if (dgvQCTests.Columns.Contains("Temp (°C)"))
                dgvQCTests.Columns["Temp (°C)"].FillWeight = 6;
            if (dgvQCTests.Columns.Contains("Volume (mL)"))
                dgvQCTests.Columns["Volume (mL)"].FillWeight = 6;
            if (dgvQCTests.Columns.Contains("pH"))
                dgvQCTests.Columns["pH"].FillWeight = 5;
            if (dgvQCTests.Columns.Contains("Result"))
                dgvQCTests.Columns["Result"].FillWeight = 8;
            if (dgvQCTests.Columns.Contains("Technician"))
                dgvQCTests.Columns["Technician"].FillWeight = 10;
            if (dgvQCTests.Columns.Contains("Test Date"))
                dgvQCTests.Columns["Test Date"].FillWeight = 8;
            if (dgvQCTests.Columns.Contains("Remarks"))
                dgvQCTests.Columns["Remarks"].FillWeight = 15;
        }

        // =========================================================
        // LOAD SELECTED RECORD FOR EDITING
        // =========================================================
        private void DgvQCTests_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvQCTests.SelectedRows.Count > 0 && dgvQCTests.SelectedRows[0].DataBoundItem != null)
            {
                DataRowView row = dgvQCTests.SelectedRows[0].DataBoundItem as DataRowView;
                if (row != null)
                {
                    isEditMode = true;
                    currentQCID = Convert.ToInt32(row["QCID"]);
                    btnSave.Text = "✏️ Update Test";

                    txtTestID.Text = row["Test ID"]?.ToString();
                    txtComponentID.Text = row["Component ID"]?.ToString();
                    SetComboValue(cmbComponentType, row["Component Type"]?.ToString());
                    SetComboValue(cmbTestType, row["Test Type"]?.ToString());

                    if (decimal.TryParse(row["Temp (°C)"]?.ToString(), out decimal temp))
                        numTemperature.Value = temp;
                    if (int.TryParse(row["Volume (mL)"]?.ToString(), out int volume))
                        numVolume.Value = volume;
                    if (decimal.TryParse(row["pH"]?.ToString(), out decimal ph))
                        numPH.Value = ph;

                    SetComboValue(cmbResult, row["Result"]?.ToString());
                    txtTechnician.Text = row["Technician"]?.ToString();
                    txtRemarks.Text = row["Remarks"]?.ToString();

                    if (DateTime.TryParse(row["Test Date"]?.ToString(), out DateTime testDate))
                        dtpTestDate.Value = testDate;

                    UpdateStatus("✏️ Editing existing QC test", Color.FromArgb(59, 130, 246));
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
        // SAVE QC TEST RESULT - INSERT OR UPDATE
        // =========================================================
        private async void BtnSave_Click(object sender, EventArgs e)
        {
            // Validation
            if (string.IsNullOrWhiteSpace(txtComponentID.Text))
            {
                MessageBox.Show("Component ID is required.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string result = cmbResult.SelectedItem?.ToString() ?? "Pending Review";

            try
            {
                ShowLoading("Saving QC test result...");

                using (var conn = DatabaseHelper.GetConnection())
                {
                    await conn.OpenAsync();

                    if (isEditMode)
                    {
                        // UPDATE existing record
                        string updateSql = @"
                            UPDATE QualityControlTests SET 
                                ComponentID = @ComponentID,
                                ComponentType = @ComponentType,
                                TestType = @TestType,
                                Temperature = @Temperature,
                                Volume = @Volume,
                                PH = @PH,
                                Result = @Result,
                                Technician = @Technician,
                                TestDate = @TestDate,
                                Remarks = @Remarks,
                                UpdatedAt = GETDATE()
                            WHERE QCID = @QCID";

                        using (var cmd = new SqlCommand(updateSql, conn))
                        {
                            cmd.Parameters.AddWithValue("@QCID", currentQCID);
                            cmd.Parameters.AddWithValue("@ComponentID", txtComponentID.Text);
                            cmd.Parameters.AddWithValue("@ComponentType", cmbComponentType.SelectedItem?.ToString() ?? "");
                            cmd.Parameters.AddWithValue("@TestType", cmbTestType.SelectedItem?.ToString() ?? "");
                            cmd.Parameters.AddWithValue("@Temperature", numTemperature.Value);
                            cmd.Parameters.AddWithValue("@Volume", (int)numVolume.Value);
                            cmd.Parameters.AddWithValue("@PH", numPH.Value);
                            cmd.Parameters.AddWithValue("@Result", result);
                            cmd.Parameters.AddWithValue("@Technician", txtTechnician.Text);
                            cmd.Parameters.AddWithValue("@TestDate", dtpTestDate.Value);
                            cmd.Parameters.AddWithValue("@Remarks", txtRemarks.Text ?? "");

                            await cmd.ExecuteNonQueryAsync();
                        }

                        MessageBox.Show($"QC test {txtTestID.Text} updated successfully!",
                            "Update Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        // INSERT new record
                        string insertSql = @"
                            INSERT INTO QualityControlTests 
                            (TestID, ComponentID, ComponentType, TestType, Temperature, 
                             Volume, PH, Result, Technician, TestDate, Remarks, PerformedBy, CreatedAt)
                            VALUES 
                            (@TestID, @ComponentID, @ComponentType, @TestType, @Temperature,
                             @Volume, @PH, @Result, @Technician, @TestDate, @Remarks, @PerformedBy, GETDATE())";

                        using (var cmd = new SqlCommand(insertSql, conn))
                        {
                            cmd.Parameters.AddWithValue("@TestID", txtTestID.Text);
                            cmd.Parameters.AddWithValue("@ComponentID", txtComponentID.Text);
                            cmd.Parameters.AddWithValue("@ComponentType", cmbComponentType.SelectedItem?.ToString() ?? "");
                            cmd.Parameters.AddWithValue("@TestType", cmbTestType.SelectedItem?.ToString() ?? "");
                            cmd.Parameters.AddWithValue("@Temperature", numTemperature.Value);
                            cmd.Parameters.AddWithValue("@Volume", (int)numVolume.Value);
                            cmd.Parameters.AddWithValue("@PH", numPH.Value);
                            cmd.Parameters.AddWithValue("@Result", result);
                            cmd.Parameters.AddWithValue("@Technician", txtTechnician.Text);
                            cmd.Parameters.AddWithValue("@TestDate", dtpTestDate.Value);
                            cmd.Parameters.AddWithValue("@Remarks", txtRemarks.Text ?? "");
                            cmd.Parameters.AddWithValue("@PerformedBy", SessionManager.CurrentUserID);

                            await cmd.ExecuteNonQueryAsync();
                        }

                        MessageBox.Show($"Quality control test {txtTestID.Text} saved successfully!",
                            "Save Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }

                // If result is Fail, update component status
                if (result == "Fail")
                {
                    await UpdateComponentStatus();
                }

                // Log the activity
                AuditHelper.Log("Quality Control", "BloodComponent",
                    $"QC test {txtTestID.Text} for component {txtComponentID.Text} - Result: {result}");

                // Refresh the grid
                LoadQCHistory();
                ClearForm();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"BtnSave_Click Error: {ex.Message}");
                MessageBox.Show($"Error saving QC test: {ex.Message}",
                    "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                UpdateStatus($"❌ Save failed: {ex.Message}", Color.FromArgb(220, 38, 38));
            }
            finally
            {
                HideLoading();
            }
        }

        private async Task UpdateComponentStatus()
        {
            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    await conn.OpenAsync();

                    string sql = @"
                        UPDATE BloodBags 
                        SET Status = 'Quarantined', 
                            Remarks = ISNULL(Remarks, '') + CHAR(10) + 'Failed QC on ' + CAST(GETDATE() AS VARCHAR) + ' - Test: ' + @TestType
                        WHERE BagID = @ComponentID";

                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@ComponentID", txtComponentID.Text);
                        cmd.Parameters.AddWithValue("@TestType", cmbTestType.SelectedItem?.ToString() ?? "");
                        await cmd.ExecuteNonQueryAsync();
                    }
                }

                MessageBox.Show("⚠️ Component has been marked as QUARANTINED due to failed QC test.",
                    "Important", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"UpdateComponentStatus Error: {ex.Message}");
            }
        }

        private void ClearForm()
        {
            isEditMode = false;
            currentQCID = 0;
            btnSave.Text = "💾 Save Test Result";

            txtComponentID.Clear();
            txtRemarks.Clear();

            cmbComponentType.SelectedIndex = 0;
            cmbTestType.SelectedIndex = 0;
            cmbResult.SelectedIndex = 0;

            numTemperature.Value = 4;
            numVolume.Value = 450;
            numPH.Value = 7.2m;

            dtpTestDate.Value = DateTime.Now;

            SetDefaultTechnician();
            GenerateTestID();

            UpdateStatus("✅ Ready - Enter component ID to start QC test", Color.FromArgb(16, 185, 129));
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

        private void DgvQCTests_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.ColumnIndex >= 0 && dgvQCTests.Columns[e.ColumnIndex].Name == "Result" && e.Value != null)
            {
                string result = e.Value.ToString();
                if (result == "Pass")
                    e.CellStyle.ForeColor = Color.FromArgb(16, 185, 129);
                else if (result == "Fail")
                    e.CellStyle.ForeColor = Color.FromArgb(220, 38, 38);
                else if (result == "Pending Review")
                    e.CellStyle.ForeColor = Color.FromArgb(245, 158, 11);
            }
        }

        private void AddLabelTextBox(string labelText, int x, int y, out TextBox txt, int width)
        {
            pnlFormCard.Controls.Add(new Label
            {
                Text = labelText,
                Font = new Font("Segoe UI", 10),
                ForeColor = C_TextMid,
                Location = new Point(x, y),
                AutoSize = true
            });
            txt = new TextBox
            {
                Location = new Point(x + 130, y - 3),
                Width = width,
                Font = new Font("Segoe UI", 10),
                BorderStyle = BorderStyle.FixedSingle
            };
            pnlFormCard.Controls.Add(txt);
        }

        private void AddLabelCombo(string labelText, int x, int y, string[] items, out ComboBox cmb, int width)
        {
            pnlFormCard.Controls.Add(new Label
            {
                Text = labelText,
                Font = new Font("Segoe UI", 10),
                ForeColor = C_TextMid,
                Location = new Point(x, y),
                AutoSize = true
            });
            cmb = new ComboBox
            {
                Location = new Point(x + 130, y - 3),
                Width = width,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10)
            };
            cmb.Items.AddRange(items);
            if (items.Length > 0) cmb.SelectedIndex = 0;
            pnlFormCard.Controls.Add(cmb);
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