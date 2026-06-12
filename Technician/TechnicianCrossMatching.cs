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
    public partial class TechnicianCrossMatching : BaseTechnicianForm
    {
        private Panel pnlFormCard;
        private DataGridView dgvCrossMatching;
        private TextBox txtRequestID, txtPatientName, txtPatientID, txtDonorBagID, txtDonorName, txtHospital, txtRemarks;
        private ComboBox cmbPatientBloodGroup, cmbDonorBloodGroup, cmbCompatibility, cmbStatus;
        private DateTimePicker dtpRequestDate, dtpCrossMatchDate;
        private Button btnSave, btnClear, btnRefresh, btnSearchDonor, btnSearchBag;
        private Label lblStatus;
        private DataTable crossMatchData;
        private bool isEditMode = false;
        private int currentCrossMatchID = 0;

        // Blood group compatibility rules
        private readonly Dictionary<string, List<string>> compatibilityRules = new Dictionary<string, List<string>>
        {
            { "O-", new List<string> { "O-" } },
            { "O+", new List<string> { "O+", "O-", "A+", "B+", "AB+" } },
            { "A-", new List<string> { "A-", "O-" } },
            { "A+", new List<string> { "A+", "A-", "O+", "O-" } },
            { "B-", new List<string> { "B-", "O-" } },
            { "B+", new List<string> { "B+", "B-", "O+", "O-" } },
            { "AB-", new List<string> { "AB-", "A-", "B-", "O-" } },
            { "AB+", new List<string> { "AB+", "AB-", "A+", "A-", "B+", "B-", "O+", "O-" } }
        };

        public TechnicianCrossMatching()
        {
            this.Text = "Blood Bank Management System – Cross Matching";
            BuildLayout();
            BuildSidebar("Cross Matching");
            BuildTopBar("Cross Matching");
            BuildContentArea();

            // Load data from database
            LoadCrossMatchHistory();
            EnsureCrossMatchTableExists();
            GenerateRequestID();
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

            Label lblSearchReq = new Label { Text = "Search Request:", Font = new Font("Segoe UI", 9), ForeColor = C_TextMid, Location = new Point(15, 16), AutoSize = true };
            pnlSearch.Controls.Add(lblSearchReq);

            TextBox txtSearchRequest = new TextBox { Location = new Point(110, 13), Width = 150, Font = new Font("Segoe UI", 9), BorderStyle = BorderStyle.FixedSingle };
            pnlSearch.Controls.Add(txtSearchRequest);

            Button btnSearch = new Button
            {
                Text = "🔍 Search",
                Location = new Point(275, 11),
                Size = new Size(80, 28),
                BackColor = brickRed,
                ForeColor = C_White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnSearch.FlatAppearance.BorderSize = 0;
            SetRoundedRegion(btnSearch, 6);
            btnSearch.Click += (s, e) => SearchCrossMatch(txtSearchRequest.Text);
            pnlSearch.Controls.Add(btnSearch);

            y += pnlSearch.Height + 20;

            // Form Card
            pnlFormCard = new Panel
            {
                Location = new Point(0, y),
                Width = pnlContent.ClientSize.Width - 60,
                Height = 450,
                BackColor = C_CardBg,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            SetRoundedRegion(pnlFormCard, 15);
            AddDropShadow(pnlFormCard);
            pnlContent.Controls.Add(pnlFormCard);

            pnlFormCard.Controls.Add(new Label { Text = "🔄 Cross Matching Test", Font = new Font("Segoe UI", 14f, FontStyle.Bold), ForeColor = C_TextDark, Location = new Point(25, 20), AutoSize = true });

            int leftX = 25, rightX = 420, fieldY = 70, spacingY = 45;

            // Section Titles
            pnlFormCard.Controls.Add(new Label { Text = "Patient Information", Font = new Font("Segoe UI", 11, FontStyle.Bold), ForeColor = brickRed, Location = new Point(leftX, fieldY - 25), AutoSize = true });
            pnlFormCard.Controls.Add(new Label { Text = "Donor Information", Font = new Font("Segoe UI", 11, FontStyle.Bold), ForeColor = brickRed, Location = new Point(rightX, fieldY - 25), AutoSize = true });

            // Patient Section
            AddLabelTextBox("Request ID:", leftX, fieldY, out txtRequestID, 180);
            txtRequestID.ReadOnly = true;
            txtRequestID.BackColor = Color.FromArgb(245, 245, 250);

            AddLabelTextBox("Patient ID:", leftX, fieldY + spacingY, out txtPatientID, 180);
            txtPatientID.TextChanged += TxtPatientID_TextChanged;

            AddLabelTextBox("Patient Name:", leftX, fieldY + spacingY * 2, out txtPatientName, 180);
            txtPatientName.ReadOnly = true;
            txtPatientName.BackColor = Color.FromArgb(245, 245, 250);

            AddLabelCombo("Patient Blood Group:", leftX, fieldY + spacingY * 3, new[] { "A+", "A-", "B+", "B-", "AB+", "AB-", "O+", "O-" }, out cmbPatientBloodGroup, 180);
            cmbPatientBloodGroup.SelectedIndexChanged += (s, e) => AutoCheckCompatibility();

            AddLabelTextBox("Hospital:", leftX, fieldY + spacingY * 4, out txtHospital, 180);

            // Donor Section
            AddLabelTextBox("Donor Bag ID:", rightX, fieldY, out txtDonorBagID, 200);
            txtDonorBagID.TextChanged += TxtDonorBagID_TextChanged;

            AddLabelTextBox("Donor Name:", rightX, fieldY + spacingY, out txtDonorName, 200);
            txtDonorName.ReadOnly = true;
            txtDonorName.BackColor = Color.FromArgb(245, 245, 250);

            AddLabelCombo("Donor Blood Group:", rightX, fieldY + spacingY * 2, new[] { "A+", "A-", "B+", "B-", "AB+", "AB-", "O+", "O-" }, out cmbDonorBloodGroup, 200);
            cmbDonorBloodGroup.SelectedIndexChanged += (s, e) => AutoCheckCompatibility();

            AddLabelCombo("Compatibility:", rightX, fieldY + spacingY * 3, new[] { "Compatible", "Incompatible", "Cross-match Pending" }, out cmbCompatibility, 200);

            AddLabelCombo("Status:", rightX, fieldY + spacingY * 4, new[] { "Completed", "Pending", "Cancelled" }, out cmbStatus, 180);

            // Dates
            pnlFormCard.Controls.Add(new Label { Text = "Request Date:", Font = new Font("Segoe UI", 10), ForeColor = C_TextMid, Location = new Point(leftX, fieldY + spacingY * 5), AutoSize = true });
            dtpRequestDate = new DateTimePicker { Location = new Point(leftX + 130, fieldY + spacingY * 5 - 3), Width = 150, Format = DateTimePickerFormat.Short, Value = DateTime.Now };
            pnlFormCard.Controls.Add(dtpRequestDate);

            pnlFormCard.Controls.Add(new Label { Text = "Cross-match Date:", Font = new Font("Segoe UI", 10), ForeColor = C_TextMid, Location = new Point(rightX, fieldY + spacingY * 5), AutoSize = true });
            dtpCrossMatchDate = new DateTimePicker { Location = new Point(rightX + 150, fieldY + spacingY * 5 - 3), Width = 150, Format = DateTimePickerFormat.Short, Value = DateTime.Now };
            pnlFormCard.Controls.Add(dtpCrossMatchDate);

            // Remarks
            AddLabelTextBox("Remarks:", leftX, fieldY + spacingY * 6, out txtRemarks, 400);
            txtRemarks.Multiline = true;
            txtRemarks.Height = 50;
            txtRemarks.MaxLength = 500;

            // Buttons
            int btnY = fieldY + spacingY * 7 + 10;

            btnSave = new Button { Text = "💾 Save Cross-match Result", Location = new Point(rightX, btnY), Size = new Size(160, 40), BackColor = brickRed, ForeColor = C_White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10, FontStyle.Bold), Cursor = Cursors.Hand };
            btnSave.FlatAppearance.BorderSize = 0;
            SetRoundedRegion(btnSave, 8);
            btnSave.Click += BtnSave_Click;
            pnlFormCard.Controls.Add(btnSave);

            btnClear = new Button { Text = "⟳ Clear Form", Location = new Point(rightX + 180, btnY), Size = new Size(110, 40), BackColor = Color.FromArgb(100, 110, 125), ForeColor = C_White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            btnClear.FlatAppearance.BorderSize = 0;
            SetRoundedRegion(btnClear, 8);
            btnClear.Click += (s, e) => ClearForm();
            pnlFormCard.Controls.Add(btnClear);

            btnRefresh = new Button { Text = "🔄 Refresh List", Location = new Point(rightX + 310, btnY), Size = new Size(110, 40), BackColor = Color.FromArgb(60, 70, 80), ForeColor = C_White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            btnRefresh.FlatAppearance.BorderSize = 0;
            SetRoundedRegion(btnRefresh, 8);
            btnRefresh.Click += (s, e) => LoadCrossMatchHistory();
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
                Text = "📋 Cross Matching History",
                Font = new Font("Segoe UI", 14f, FontStyle.Bold),
                ForeColor = C_TextDark,
                Location = new Point(0, 5),
                AutoSize = true
            };
            headerPanel.Controls.Add(lblHistory);
            y += 35;

            dgvCrossMatching = CreateStyledGrid();
            dgvCrossMatching.Location = new Point(0, y);
            dgvCrossMatching.Height = 250;
            dgvCrossMatching.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            dgvCrossMatching.CellFormatting += DgvCrossMatching_CellFormatting;
            dgvCrossMatching.SelectionChanged += DgvCrossMatching_SelectionChanged;
            WrapInCard(dgvCrossMatching, pnlContent, new Point(0, y), 250);
            FixDataGridView(dgvCrossMatching);

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
        // ENSURE CROSS MATCH TABLE EXISTS
        // =========================================================
        private void EnsureCrossMatchTableExists()
        {
            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();

                    string createTableSql = @"
                        IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='CrossMatching' AND xtype='U')
                        CREATE TABLE CrossMatching (
                            CrossMatchID INT IDENTITY(1,1) PRIMARY KEY,
                            RequestID NVARCHAR(50) UNIQUE NOT NULL,
                            PatientID NVARCHAR(50),
                            PatientName NVARCHAR(200),
                            PatientBloodGroup NVARCHAR(10),
                            DonorBagID NVARCHAR(50),
                            DonorName NVARCHAR(200),
                            DonorBloodGroup NVARCHAR(10),
                            Compatibility NVARCHAR(50),
                            Status NVARCHAR(50),
                            Hospital NVARCHAR(200),
                            RequestDate DATE,
                            CrossMatchDate DATE,
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
                        IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name='IX_CrossMatching_RequestID')
                        CREATE INDEX IX_CrossMatching_RequestID ON CrossMatching(RequestID);
                        
                        IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name='IX_CrossMatching_PatientID')
                        CREATE INDEX IX_CrossMatching_PatientID ON CrossMatching(PatientID)";

                    using (var cmd = new SqlCommand(createIndexes, conn))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"EnsureCrossMatchTableExists Error: {ex.Message}");
            }
        }

        // =========================================================
        // GENERATE AUTO REQUEST ID
        // =========================================================
        private async void GenerateRequestID()
        {
            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    await conn.OpenAsync();

                    string checkTable = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'CrossMatching'";
                    using (var checkCmd = new SqlCommand(checkTable, conn))
                    {
                        int tableExists = Convert.ToInt32(await checkCmd.ExecuteScalarAsync());
                        if (tableExists == 0)
                        {
                            txtRequestID.Text = $"XM-{DateTime.Now:yyyyMMdd}-001";
                            return;
                        }
                    }

                    string sql = "SELECT COUNT(*) FROM CrossMatching";
                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        int count = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                        txtRequestID.Text = $"XM-{DateTime.Now:yyyyMMdd}-{(count + 1):D3}";
                    }
                }
            }
            catch
            {
                txtRequestID.Text = $"XM-{DateTime.Now:yyyyMMdd}-001";
            }
        }

        // =========================================================
        // AUTO FILL PATIENT DETAILS
        // =========================================================
        private async void TxtPatientID_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtPatientID.Text)) return;

            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    await conn.OpenAsync();

                    string sql = @"
                        SELECT 
                            FullName,
                            BloodGroup
                        FROM Patients 
                        WHERE PatientID = @PatientID OR UserID = @PatientID";

                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@PatientID", txtPatientID.Text.Trim());
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                txtPatientName.Text = reader["FullName"].ToString();
                                string bloodGroup = reader["BloodGroup"].ToString();
                                int index = cmbPatientBloodGroup.Items.IndexOf(bloodGroup);
                                if (index >= 0)
                                    cmbPatientBloodGroup.SelectedIndex = index;

                                UpdateStatus($"✅ Patient found: {txtPatientName.Text}", Color.FromArgb(16, 185, 129));
                            }
                            else
                            {
                                txtPatientName.Text = "";
                                UpdateStatus("⚠️ Patient not found", Color.FromArgb(245, 158, 11));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"TxtPatientID_TextChanged Error: {ex.Message}");
            }
        }

        // =========================================================
        // AUTO FILL DONOR DETAILS FROM BAG ID
        // =========================================================
        private async void TxtDonorBagID_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtDonorBagID.Text)) return;

            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    await conn.OpenAsync();

                    string sql = @"
                        SELECT 
                            ISNULL(DonorName, 'N/A') as DonorName,
                            BloodGroup
                        FROM BloodBags 
                        WHERE BagID = @BagID AND Status = 'Available'";

                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@BagID", txtDonorBagID.Text.Trim());
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                txtDonorName.Text = reader["DonorName"].ToString();
                                string bloodGroup = reader["BloodGroup"].ToString();
                                int index = cmbDonorBloodGroup.Items.IndexOf(bloodGroup);
                                if (index >= 0)
                                    cmbDonorBloodGroup.SelectedIndex = index;

                                UpdateStatus($"✅ Donor bag found: {txtDonorBagID.Text}", Color.FromArgb(16, 185, 129));
                            }
                            else
                            {
                                txtDonorName.Text = "";
                                UpdateStatus("⚠️ Donor bag not found or not available", Color.FromArgb(245, 158, 11));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"TxtDonorBagID_TextChanged Error: {ex.Message}");
            }
        }

        // =========================================================
        // AUTO CHECK COMPATIBILITY
        // =========================================================
        private void AutoCheckCompatibility()
        {
            string patientBlood = cmbPatientBloodGroup.SelectedItem?.ToString();
            string donorBlood = cmbDonorBloodGroup.SelectedItem?.ToString();

            if (string.IsNullOrEmpty(patientBlood) || string.IsNullOrEmpty(donorBlood)) return;

            if (compatibilityRules.ContainsKey(donorBlood) && compatibilityRules[donorBlood].Contains(patientBlood))
            {
                cmbCompatibility.SelectedItem = "Compatible";
                UpdateStatus("✅ Compatible - Donor blood can be given to patient", Color.FromArgb(16, 185, 129));
            }
            else
            {
                cmbCompatibility.SelectedItem = "Incompatible";
                UpdateStatus("⚠️ Incompatible - Donor blood cannot be given to patient", Color.FromArgb(220, 38, 38));
            }
        }

        // =========================================================
        // SEARCH CROSS MATCH RECORDS
        // =========================================================
        private void SearchCrossMatch(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                LoadCrossMatchHistory();
                return;
            }

            if (crossMatchData == null) return;

            DataTable filteredDt = crossMatchData.Clone();

            foreach (DataRow row in crossMatchData.Rows)
            {
                string requestID = row["Request ID"]?.ToString() ?? "";
                string patientID = row["Patient ID"]?.ToString() ?? "";
                string patientName = row["Patient Name"]?.ToString() ?? "";

                if (requestID.ToLower().Contains(searchTerm.ToLower()) ||
                    patientID.ToLower().Contains(searchTerm.ToLower()) ||
                    patientName.ToLower().Contains(searchTerm.ToLower()))
                {
                    filteredDt.ImportRow(row);
                }
            }

            dgvCrossMatching.DataSource = filteredDt;
            UpdateStatus($"🔍 Found {filteredDt.Rows.Count} matching record(s)", Color.FromArgb(59, 130, 246));
        }

        // =========================================================
        // LOAD CROSS MATCH HISTORY FROM DATABASE
        // =========================================================
        private async void LoadCrossMatchHistory()
        {
            try
            {
                UpdateStatus("🔄 Loading cross-match history...", Color.FromArgb(59, 130, 246));

                using (var conn = DatabaseHelper.GetConnection())
                {
                    await conn.OpenAsync();

                    string checkTable = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'CrossMatching'";
                    using (var checkCmd = new SqlCommand(checkTable, conn))
                    {
                        int tableExists = Convert.ToInt32(await checkCmd.ExecuteScalarAsync());
                        if (tableExists == 0)
                        {
                            dgvCrossMatching.DataSource = null;
                            UpdateStatus("⚠️ Table 'CrossMatching' not found", Color.FromArgb(245, 158, 11));
                            return;
                        }
                    }

                    string query = @"
                        SELECT 
                            CrossMatchID,
                            RequestID as [Request ID],
                            PatientID as [Patient ID],
                            PatientName as [Patient Name],
                            PatientBloodGroup as [Patient Blood Group],
                            DonorBagID as [Donor Bag ID],
                            DonorName as [Donor Name],
                            DonorBloodGroup as [Donor Blood Group],
                            Compatibility,
                            Status,
                            Hospital,
                            FORMAT(RequestDate, 'dd-MMM-yyyy') as [Request Date],
                            FORMAT(CrossMatchDate, 'dd-MMM-yyyy') as [Cross-match Date],
                            Remarks,
                            FORMAT(CreatedAt, 'dd-MMM-yyyy HH:mm') as [Recorded At]
                        FROM CrossMatching 
                        ORDER BY CrossMatchDate DESC, RequestID DESC";

                    SqlDataAdapter da = new SqlDataAdapter(query, conn);
                    crossMatchData = new DataTable();
                    da.Fill(crossMatchData);

                    dgvCrossMatching.DataSource = crossMatchData;
                    FormatHistoryGrid();
                    NoSort(dgvCrossMatching);

                    UpdateStatus($"✅ Loaded {crossMatchData.Rows.Count} cross-match record(s)", Color.FromArgb(16, 185, 129));
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LoadCrossMatchHistory Error: {ex.Message}");
                dgvCrossMatching.DataSource = null;
                UpdateStatus($"❌ Database error: {ex.Message}", Color.FromArgb(220, 38, 38));
            }
        }

        private void FormatHistoryGrid()
        {
            if (dgvCrossMatching.Columns.Count == 0) return;

            if (dgvCrossMatching.Columns.Contains("CrossMatchID"))
                dgvCrossMatching.Columns["CrossMatchID"].Visible = false;

            if (dgvCrossMatching.Columns.Contains("Request ID"))
                dgvCrossMatching.Columns["Request ID"].FillWeight = 8;
            if (dgvCrossMatching.Columns.Contains("Patient ID"))
                dgvCrossMatching.Columns["Patient ID"].FillWeight = 8;
            if (dgvCrossMatching.Columns.Contains("Patient Name"))
                dgvCrossMatching.Columns["Patient Name"].FillWeight = 10;
            if (dgvCrossMatching.Columns.Contains("Patient Blood Group"))
                dgvCrossMatching.Columns["Patient Blood Group"].FillWeight = 8;
            if (dgvCrossMatching.Columns.Contains("Donor Bag ID"))
                dgvCrossMatching.Columns["Donor Bag ID"].FillWeight = 10;
            if (dgvCrossMatching.Columns.Contains("Donor Name"))
                dgvCrossMatching.Columns["Donor Name"].FillWeight = 10;
            if (dgvCrossMatching.Columns.Contains("Donor Blood Group"))
                dgvCrossMatching.Columns["Donor Blood Group"].FillWeight = 8;
            if (dgvCrossMatching.Columns.Contains("Compatibility"))
                dgvCrossMatching.Columns["Compatibility"].FillWeight = 10;
            if (dgvCrossMatching.Columns.Contains("Status"))
                dgvCrossMatching.Columns["Status"].FillWeight = 8;
            if (dgvCrossMatching.Columns.Contains("Cross-match Date"))
                dgvCrossMatching.Columns["Cross-match Date"].FillWeight = 10;
            if (dgvCrossMatching.Columns.Contains("Recorded At"))
                dgvCrossMatching.Columns["Recorded At"].FillWeight = 10;
        }

        // =========================================================
        // LOAD SELECTED RECORD FOR EDITING
        // =========================================================
        private void DgvCrossMatching_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvCrossMatching.SelectedRows.Count > 0 && dgvCrossMatching.SelectedRows[0].DataBoundItem != null)
            {
                DataRowView row = dgvCrossMatching.SelectedRows[0].DataBoundItem as DataRowView;
                if (row != null)
                {
                    isEditMode = true;
                    currentCrossMatchID = Convert.ToInt32(row["CrossMatchID"]);
                    btnSave.Text = "✏️ Update Result";

                    txtRequestID.Text = row["Request ID"]?.ToString();
                    txtPatientID.Text = row["Patient ID"]?.ToString();
                    txtPatientName.Text = row["Patient Name"]?.ToString();
                    SetComboValue(cmbPatientBloodGroup, row["Patient Blood Group"]?.ToString());
                    txtDonorBagID.Text = row["Donor Bag ID"]?.ToString();
                    txtDonorName.Text = row["Donor Name"]?.ToString();
                    SetComboValue(cmbDonorBloodGroup, row["Donor Blood Group"]?.ToString());
                    SetComboValue(cmbCompatibility, row["Compatibility"]?.ToString());
                    SetComboValue(cmbStatus, row["Status"]?.ToString());
                    txtHospital.Text = row["Hospital"]?.ToString();
                    txtRemarks.Text = row["Remarks"]?.ToString();

                    if (DateTime.TryParse(row["Request Date"]?.ToString(), out DateTime reqDate))
                        dtpRequestDate.Value = reqDate;
                    if (DateTime.TryParse(row["Cross-match Date"]?.ToString(), out DateTime cmDate))
                        dtpCrossMatchDate.Value = cmDate;

                    UpdateStatus("✏️ Editing existing cross-match record", Color.FromArgb(59, 130, 246));
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
        // SAVE CROSS MATCH RESULT - INSERT OR UPDATE
        // =========================================================
        private async void BtnSave_Click(object sender, EventArgs e)
        {
            // Validation
            if (string.IsNullOrWhiteSpace(txtRequestID.Text))
            {
                MessageBox.Show("Request ID is required.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtPatientID.Text))
            {
                MessageBox.Show("Patient ID is required.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtDonorBagID.Text))
            {
                MessageBox.Show("Donor Bag ID is required.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                ShowLoading("Saving cross-match record...");

                using (var conn = DatabaseHelper.GetConnection())
                {
                    await conn.OpenAsync();

                    if (isEditMode)
                    {
                        // UPDATE existing record
                        string updateSql = @"
                            UPDATE CrossMatching SET 
                                PatientID = @PatientID,
                                PatientName = @PatientName,
                                PatientBloodGroup = @PatientBloodGroup,
                                DonorBagID = @DonorBagID,
                                DonorName = @DonorName,
                                DonorBloodGroup = @DonorBloodGroup,
                                Compatibility = @Compatibility,
                                Status = @Status,
                                Hospital = @Hospital,
                                RequestDate = @RequestDate,
                                CrossMatchDate = @CrossMatchDate,
                                Remarks = @Remarks,
                                UpdatedAt = GETDATE()
                            WHERE RequestID = @RequestID";

                        using (var cmd = new SqlCommand(updateSql, conn))
                        {
                            cmd.Parameters.AddWithValue("@RequestID", txtRequestID.Text);
                            cmd.Parameters.AddWithValue("@PatientID", txtPatientID.Text);
                            cmd.Parameters.AddWithValue("@PatientName", txtPatientName.Text);
                            cmd.Parameters.AddWithValue("@PatientBloodGroup", cmbPatientBloodGroup.SelectedItem?.ToString() ?? "");
                            cmd.Parameters.AddWithValue("@DonorBagID", txtDonorBagID.Text);
                            cmd.Parameters.AddWithValue("@DonorName", txtDonorName.Text);
                            cmd.Parameters.AddWithValue("@DonorBloodGroup", cmbDonorBloodGroup.SelectedItem?.ToString() ?? "");
                            cmd.Parameters.AddWithValue("@Compatibility", cmbCompatibility.SelectedItem?.ToString() ?? "Pending");
                            cmd.Parameters.AddWithValue("@Status", cmbStatus.SelectedItem?.ToString() ?? "Pending");
                            cmd.Parameters.AddWithValue("@Hospital", txtHospital.Text ?? "");
                            cmd.Parameters.AddWithValue("@RequestDate", dtpRequestDate.Value);
                            cmd.Parameters.AddWithValue("@CrossMatchDate", dtpCrossMatchDate.Value);
                            cmd.Parameters.AddWithValue("@Remarks", txtRemarks.Text ?? "");

                            await cmd.ExecuteNonQueryAsync();
                        }

                        MessageBox.Show($"Cross-match record {txtRequestID.Text} updated successfully!",
                            "Update Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        // INSERT new record
                        string insertSql = @"
                            INSERT INTO CrossMatching 
                            (RequestID, PatientID, PatientName, PatientBloodGroup, DonorBagID, 
                             DonorName, DonorBloodGroup, Compatibility, Status, Hospital, 
                             RequestDate, CrossMatchDate, Remarks, PerformedBy, CreatedAt)
                            VALUES 
                            (@RequestID, @PatientID, @PatientName, @PatientBloodGroup, @DonorBagID,
                             @DonorName, @DonorBloodGroup, @Compatibility, @Status, @Hospital,
                             @RequestDate, @CrossMatchDate, @Remarks, @PerformedBy, GETDATE())";

                        using (var cmd = new SqlCommand(insertSql, conn))
                        {
                            cmd.Parameters.AddWithValue("@RequestID", txtRequestID.Text);
                            cmd.Parameters.AddWithValue("@PatientID", txtPatientID.Text);
                            cmd.Parameters.AddWithValue("@PatientName", txtPatientName.Text);
                            cmd.Parameters.AddWithValue("@PatientBloodGroup", cmbPatientBloodGroup.SelectedItem?.ToString() ?? "");
                            cmd.Parameters.AddWithValue("@DonorBagID", txtDonorBagID.Text);
                            cmd.Parameters.AddWithValue("@DonorName", txtDonorName.Text);
                            cmd.Parameters.AddWithValue("@DonorBloodGroup", cmbDonorBloodGroup.SelectedItem?.ToString() ?? "");
                            cmd.Parameters.AddWithValue("@Compatibility", cmbCompatibility.SelectedItem?.ToString() ?? "Pending");
                            cmd.Parameters.AddWithValue("@Status", cmbStatus.SelectedItem?.ToString() ?? "Pending");
                            cmd.Parameters.AddWithValue("@Hospital", txtHospital.Text ?? "");
                            cmd.Parameters.AddWithValue("@RequestDate", dtpRequestDate.Value);
                            cmd.Parameters.AddWithValue("@CrossMatchDate", dtpCrossMatchDate.Value);
                            cmd.Parameters.AddWithValue("@Remarks", txtRemarks.Text ?? "");
                            cmd.Parameters.AddWithValue("@PerformedBy", SessionManager.CurrentUserID);

                            await cmd.ExecuteNonQueryAsync();
                        }

                        MessageBox.Show($"Cross-match record {txtRequestID.Text} saved successfully!",
                            "Save Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }

                // If compatible, update blood bag status
                if (cmbCompatibility.SelectedItem?.ToString() == "Compatible" && cmbStatus.SelectedItem?.ToString() == "Completed")
                {
                    await UpdateBloodBagStatus();
                }

                // Log the activity
                AuditHelper.Log("Cross Matching", "BloodBag",
                    $"Cross-match {txtRequestID.Text} - Patient: {txtPatientName.Text} - Donor Bag: {txtDonorBagID.Text} - Result: {cmbCompatibility.SelectedItem}");

                // Refresh the grid
                LoadCrossMatchHistory();
                ClearForm();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"BtnSave_Click Error: {ex.Message}");
                MessageBox.Show($"Error saving cross-match record: {ex.Message}",
                    "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                UpdateStatus($"❌ Save failed: {ex.Message}", Color.FromArgb(220, 38, 38));
            }
            finally
            {
                HideLoading();
            }
        }

        private async Task UpdateBloodBagStatus()
        {
            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    await conn.OpenAsync();

                    string sql = @"
                        UPDATE BloodBags 
                        SET Status = 'Issued', 
                            IssuedDate = GETDATE(),
                            IssuedTo = @PatientName,
                            Remarks = ISNULL(Remarks, '') + CHAR(10) + 'Cross-matched and issued to patient on ' + CAST(GETDATE() AS VARCHAR)
                        WHERE BagID = @BagID";

                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@BagID", txtDonorBagID.Text);
                        cmd.Parameters.AddWithValue("@PatientName", txtPatientName.Text);
                        await cmd.ExecuteNonQueryAsync();
                    }
                }

                UpdateStatus("✅ Blood bag marked as ISSUED", Color.FromArgb(16, 185, 129));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"UpdateBloodBagStatus Error: {ex.Message}");
            }
        }

        private void ClearForm()
        {
            isEditMode = false;
            currentCrossMatchID = 0;
            btnSave.Text = "💾 Save Cross-match Result";

            txtPatientID.Clear();
            txtPatientName.Clear();
            txtDonorBagID.Clear();
            txtDonorName.Clear();
            txtHospital.Clear();
            txtRemarks.Clear();

            cmbPatientBloodGroup.SelectedIndex = 0;
            cmbDonorBloodGroup.SelectedIndex = 0;
            cmbCompatibility.SelectedIndex = 0;
            cmbStatus.SelectedIndex = 0;

            dtpRequestDate.Value = DateTime.Now;
            dtpCrossMatchDate.Value = DateTime.Now;

            GenerateRequestID();

            UpdateStatus("✅ Ready - Enter patient ID and donor bag ID", Color.FromArgb(16, 185, 129));
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

        private void DgvCrossMatching_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (dgvCrossMatching.Columns[e.ColumnIndex].Name == "Compatibility" && e.Value != null)
            {
                string result = e.Value.ToString();
                if (result == "Compatible")
                    e.CellStyle.ForeColor = Color.FromArgb(16, 185, 129);
                else if (result == "Incompatible")
                    e.CellStyle.ForeColor = Color.FromArgb(220, 38, 38);
                else if (result == "Cross-match Pending")
                    e.CellStyle.ForeColor = Color.FromArgb(245, 158, 11);
            }

            if (dgvCrossMatching.Columns[e.ColumnIndex].Name == "Status" && e.Value != null)
            {
                string status = e.Value.ToString();
                if (status == "Completed")
                    e.CellStyle.ForeColor = Color.FromArgb(16, 185, 129);
                else if (status == "Cancelled")
                    e.CellStyle.ForeColor = Color.FromArgb(220, 38, 38);
                else if (status == "Pending")
                    e.CellStyle.ForeColor = Color.FromArgb(245, 158, 11);
            }
        }

        private void AddLabelTextBox(string labelText, int x, int y, out TextBox txt, int width)
        {
            pnlFormCard.Controls.Add(new Label { Text = labelText, Font = new Font("Segoe UI", 10), ForeColor = C_TextMid, Location = new Point(x, y), AutoSize = true });
            txt = new TextBox { Location = new Point(x + 130, y - 3), Width = width, Font = new Font("Segoe UI", 10), BorderStyle = BorderStyle.FixedSingle };
            pnlFormCard.Controls.Add(txt);
        }

        private void AddLabelCombo(string labelText, int x, int y, string[] items, out ComboBox cmb, int width)
        {
            pnlFormCard.Controls.Add(new Label { Text = labelText, Font = new Font("Segoe UI", 10), ForeColor = C_TextMid, Location = new Point(x, y), AutoSize = true });
            cmb = new ComboBox { Location = new Point(x + 130, y - 3), Width = width, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10) };
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
            foreach (DataGridViewColumn col in dgv.Columns)
                col.SortMode = DataGridViewColumnSortMode.NotSortable;
        }
    }
}