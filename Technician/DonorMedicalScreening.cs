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
    public partial class DonorMedicalScreening : BaseTechnicianForm
    {
        private Panel pnlFormCard;
        private DataGridView dgvHistory;
        private TextBox txtDonorID, txtDonorName, txtWeight, txtBP, txtHb, txtTemp, txtPulse;
        private ComboBox cmbBloodGroup;
        private RadioButton radFit, radDefer;
        private Button btnSave, btnClear, btnSearchDonor, btnRefresh;
        private Label lblStatus;
        private DataTable screeningData;
        private bool isEditMode = false;
        private int currentScreeningID = 0;

        // Screening thresholds
        private const int MIN_WEIGHT = 50;
        private const int MIN_HB = 12;
        private const decimal MAX_TEMP = 37.5m;
        private const int MAX_PULSE = 100;
        private const int MIN_PULSE = 60;

        public DonorMedicalScreening()
        {
            this.Text = "Blood Bank Management System – Donor Medical Screening";
            BuildLayout();
            BuildSidebar("Medical Screening");
            BuildTopBar("Donor Medical Screening");
            BuildContentArea();

            // Load data from database
            LoadScreeningHistory();
            EnsureScreeningTableExists();
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

            // Search Donor Panel
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

            Label lblSearchDonor = new Label
            {
                Text = "Search Donor:",
                Font = new Font("Segoe UI", 10),
                ForeColor = C_TextMid,
                Location = new Point(20, 15),
                AutoSize = true
            };
            pnlSearch.Controls.Add(lblSearchDonor);

            TextBox txtSearchDonor = new TextBox
            {
                Location = new Point(130, 12),
                Width = 180,
                Font = new Font("Segoe UI", 10),
                BorderStyle = BorderStyle.FixedSingle
            };
            pnlSearch.Controls.Add(txtSearchDonor);

            btnSearchDonor = new Button
            {
                Text = "🔍 Search",
                Location = new Point(325, 10),
                Size = new Size(100, 30),
                BackColor = brickRed,
                ForeColor = C_White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnSearchDonor.FlatAppearance.BorderSize = 0;
            SetRoundedRegion(btnSearchDonor, 6);
            btnSearchDonor.Click += (s, e) => SearchDonor(txtSearchDonor.Text);
            pnlSearch.Controls.Add(btnSearchDonor);

            y += pnlSearch.Height + 20;

            // Form Card
            pnlFormCard = new Panel
            {
                Location = new Point(0, y),
                Width = pnlContent.ClientSize.Width - 60,
                Height = 400,
                BackColor = C_CardBg,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            SetRoundedRegion(pnlFormCard, 15);
            AddDropShadow(pnlFormCard);
            pnlContent.Controls.Add(pnlFormCard);

            pnlFormCard.Controls.Add(new Label
            {
                Text = "📋 Donor Medical Screening",
                Font = new Font("Segoe UI", 14f, FontStyle.Bold),
                ForeColor = C_TextDark,
                Location = new Point(25, 20),
                AutoSize = true
            });

            int leftX = 25, rightX = 420, fieldY = 70, spacingY = 45;

            // Left Column
            AddLabelTextBox("Donor ID:", leftX, fieldY, out txtDonorID, 160);
            txtDonorID.TextChanged += TxtDonorID_TextChanged;

            AddLabelTextBox("Donor Name:", leftX, fieldY + spacingY, out txtDonorName, 160);
            txtDonorName.ReadOnly = true;
            txtDonorName.BackColor = Color.FromArgb(245, 245, 250);

            AddLabelCombo("Blood Group:", leftX, fieldY + spacingY * 2, new[] { "A+", "A-", "B+", "B-", "AB+", "AB-", "O+", "O-" }, out cmbBloodGroup, 160);

            AddLabelTextBox("Temperature (°C):", leftX, fieldY + spacingY * 3, out txtTemp, 160);
            txtTemp.TextChanged += (s, e) => AutoEvaluateResult();

            AddLabelTextBox("Pulse (bpm):", leftX, fieldY + spacingY * 4, out txtPulse, 160);
            txtPulse.TextChanged += (s, e) => AutoEvaluateResult();

            // Right Column
            AddLabelTextBox("Weight (kg):", rightX, fieldY, out txtWeight, 160);
            txtWeight.TextChanged += (s, e) => AutoEvaluateResult();

            AddLabelTextBox("BP (mmHg):", rightX, fieldY + spacingY, out txtBP, 160);
            txtBP.TextChanged += (s, e) => AutoEvaluateResult();

            AddLabelTextBox("Hemoglobin (g/dL):", rightX, fieldY + spacingY * 2, out txtHb, 160);
            txtHb.TextChanged += (s, e) => AutoEvaluateResult();

            // Screening Result
            pnlFormCard.Controls.Add(new Label
            {
                Text = "Screening Result:",
                Font = new Font("Segoe UI", 10),
                ForeColor = C_TextMid,
                Location = new Point(rightX, fieldY + spacingY * 3),
                AutoSize = true
            });

            radFit = new RadioButton
            {
                Text = "✓ Fit to Donate",
                Location = new Point(rightX + 120, fieldY + spacingY * 3 - 2),
                AutoSize = true,
                ForeColor = Color.FromArgb(16, 185, 129),
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };
            radDefer = new RadioButton
            {
                Text = "✗ Defer",
                Location = new Point(rightX + 240, fieldY + spacingY * 3 - 2),
                AutoSize = true,
                ForeColor = Color.FromArgb(245, 158, 11)
            };
            radFit.Checked = true;
            pnlFormCard.Controls.Add(radFit);
            pnlFormCard.Controls.Add(radDefer);

            // Buttons
            int btnY = fieldY + spacingY * 5;

            btnSave = new Button
            {
                Text = "💾 Save Screening",
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
                Size = new Size(120, 40),
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
                Location = new Point(rightX + 310, btnY),
                Size = new Size(110, 40),
                BackColor = Color.FromArgb(60, 70, 80),
                ForeColor = C_White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnRefresh.FlatAppearance.BorderSize = 0;
            SetRoundedRegion(btnRefresh, 8);
            btnRefresh.Click += (s, e) => LoadScreeningHistory();
            pnlFormCard.Controls.Add(btnRefresh);

            y += pnlFormCard.Height + 30;

            // Screening History Header
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
                Text = "📋 Screening History",
                Font = new Font("Segoe UI", 14f, FontStyle.Bold),
                ForeColor = C_TextDark,
                Location = new Point(0, 5),
                AutoSize = true
            };
            headerPanel.Controls.Add(lblHistory);
            y += 35;

            dgvHistory = CreateStyledGrid();
            dgvHistory.Location = new Point(0, y);
            dgvHistory.Height = 280;
            dgvHistory.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            dgvHistory.CellFormatting += DgvHistory_CellFormatting;
            dgvHistory.SelectionChanged += DgvHistory_SelectionChanged;
            WrapInCard(dgvHistory, pnlContent, new Point(0, y), 280);
            FixDataGridView(dgvHistory);

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
        // ENSURE SCREENING TABLE EXISTS
        // =========================================================
        private void EnsureScreeningTableExists()
        {
            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();

                    string createTableSql = @"
                        IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='DonorScreenings' AND xtype='U')
                        CREATE TABLE DonorScreenings (
                            ScreeningID INT IDENTITY(1,1) PRIMARY KEY,
                            DonorID NVARCHAR(50) NOT NULL,
                            DonorName NVARCHAR(200),
                            BloodGroup NVARCHAR(10),
                            Weight DECIMAL(5,1),
                            BloodPressure NVARCHAR(20),
                            Hemoglobin DECIMAL(4,1),
                            Temperature DECIMAL(4,1),
                            Pulse INT,
                            ScreeningResult NVARCHAR(20),
                            ScreeningDate DATETIME DEFAULT GETDATE(),
                            ScreenedBy INT,
                            CreatedAt DATETIME DEFAULT GETDATE()
                        )";

                    using (var cmd = new SqlCommand(createTableSql, conn))
                    {
                        cmd.ExecuteNonQuery();
                    }

                    // Create indexes
                    string createIndexes = @"
                        IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name='IX_DonorScreenings_DonorID')
                        CREATE INDEX IX_DonorScreenings_DonorID ON DonorScreenings(DonorID);
                        
                        IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name='IX_DonorScreenings_ScreeningDate')
                        CREATE INDEX IX_DonorScreenings_ScreeningDate ON DonorScreenings(ScreeningDate)";

                    using (var cmd = new SqlCommand(createIndexes, conn))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"EnsureScreeningTableExists Error: {ex.Message}");
            }
        }

        // =========================================================
        // SEARCH DONOR FROM DATABASE
        // =========================================================
        private void SearchDonor(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                MessageBox.Show("Please enter Donor ID or Name to search.", "Search",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();

                    string query = @"
                        SELECT TOP 1 
                            d.DonorID,
                            d.FullName,
                            d.BloodGroup,
                            d.Weight,
                            d.Age
                        FROM Donors d
                        WHERE (d.DonorID = @SearchTerm 
                        OR d.FullName LIKE @SearchPattern
                        OR d.CNIC = @SearchTerm)
                        AND d.IsActive = 1";

                    using (var cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@SearchTerm", searchTerm);
                        cmd.Parameters.AddWithValue("@SearchPattern", "%" + searchTerm + "%");

                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                txtDonorID.Text = reader["DonorID"].ToString();
                                txtDonorName.Text = reader["FullName"].ToString();

                                string bloodGroup = reader["BloodGroup"].ToString();
                                int index = cmbBloodGroup.Items.IndexOf(bloodGroup);
                                if (index >= 0)
                                    cmbBloodGroup.SelectedIndex = index;

                                // Pre-fill weight if available
                                if (reader["Weight"] != DBNull.Value)
                                {
                                    txtWeight.Text = reader["Weight"].ToString();
                                }

                                UpdateStatus($"✅ Donor found: {txtDonorName.Text}", Color.FromArgb(16, 185, 129));
                                AutoEvaluateResult();
                            }
                            else
                            {
                                MessageBox.Show($"No donor found with ID/Name: {searchTerm}",
                                    "Not Found", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                ClearForm();
                                UpdateStatus("⚠️ Donor not found", Color.FromArgb(245, 158, 11));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SearchDonor Error: {ex.Message}");
                MessageBox.Show($"Error searching donor: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void TxtDonorID_TextChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(txtDonorID.Text))
            {
                SearchDonor(txtDonorID.Text);
            }
        }

        // =========================================================
        // AUTO EVALUATE SCREENING RESULT
        // =========================================================
        private void AutoEvaluateResult()
        {
            // Parse values
            decimal weight = ParseDecimal(txtWeight.Text);
            decimal hemoglobin = ParseDecimal(txtHb.Text);
            decimal temperature = ParseDecimal(txtTemp.Text);
            int pulse = ParseInt(txtPulse.Text);

            // Check BP format (e.g., "120/80")
            bool bpValid = IsValidBloodPressure(txtBP.Text);

            bool isFit = true;
            string deferReason = "";

            if (weight > 0 && weight < MIN_WEIGHT)
            {
                isFit = false;
                deferReason = $"Weight below {MIN_WEIGHT}kg";
            }

            if (hemoglobin > 0 && hemoglobin < MIN_HB)
            {
                isFit = false;
                deferReason = $"Hemoglobin below {MIN_HB}g/dL";
            }

            if (temperature > 0 && temperature > MAX_TEMP)
            {
                isFit = false;
                deferReason = $"Temperature above {MAX_TEMP}°C";
            }

            if (pulse > 0 && (pulse > MAX_PULSE || pulse < MIN_PULSE))
            {
                isFit = false;
                deferReason = $"Pulse out of normal range ({MIN_PULSE}-{MAX_PULSE} bpm)";
            }

            if (!bpValid && !string.IsNullOrWhiteSpace(txtBP.Text))
            {
                isFit = false;
                deferReason = "Invalid blood pressure reading";
            }

            if (isFit)
            {
                radFit.Checked = true;
                UpdateStatus("✅ Donor meets all criteria - Fit to donate", Color.FromArgb(16, 185, 129));
            }
            else if (!string.IsNullOrWhiteSpace(txtWeight.Text) || !string.IsNullOrWhiteSpace(txtHb.Text))
            {
                radDefer.Checked = true;
                UpdateStatus($"⚠️ Donor deferred: {deferReason}", Color.FromArgb(245, 158, 11));
            }
        }

        private decimal ParseDecimal(string value)
        {
            if (decimal.TryParse(value, out decimal result))
                return result;
            return 0;
        }

        private int ParseInt(string value)
        {
            if (int.TryParse(value, out int result))
                return result;
            return 0;
        }

        private bool IsValidBloodPressure(string bp)
        {
            if (string.IsNullOrWhiteSpace(bp)) return true; // Optional field

            string[] parts = bp.Split('/');
            if (parts.Length != 2) return false;

            if (int.TryParse(parts[0], out int systolic) && int.TryParse(parts[1], out int diastolic))
            {
                return systolic >= 90 && systolic <= 180 && diastolic >= 60 && diastolic <= 120;
            }
            return false;
        }

        // =========================================================
        // LOAD SCREENING HISTORY FROM DATABASE
        // =========================================================
        private void LoadScreeningHistory()
        {
            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();

                    // Check if table exists
                    string checkTable = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'DonorScreenings'";
                    using (var checkCmd = new SqlCommand(checkTable, conn))
                    {
                        int tableExists = Convert.ToInt32(checkCmd.ExecuteScalar());
                        if (tableExists == 0)
                        {
                            dgvHistory.DataSource = null;
                            UpdateStatus("⚠️ Table 'DonorScreenings' not found", Color.FromArgb(245, 158, 11));
                            return;
                        }
                    }

                    string query = @"
                        SELECT 
                            ScreeningID,
                            DonorID as [Donor ID],
                            DonorName as [Donor Name],
                            BloodGroup as [Blood Group],
                            Weight as [Weight (kg)],
                            BloodPressure as [BP (mmHg)],
                            Hemoglobin as [Hb (g/dL)],
                            Temperature as [Temp (°C)],
                            Pulse as [Pulse (bpm)],
                            ScreeningResult as [Result],
                            FORMAT(ScreeningDate, 'dd-MMM-yyyy HH:mm') as [Date/Time]
                        FROM DonorScreenings 
                        ORDER BY ScreeningDate DESC, ScreeningID DESC";

                    SqlDataAdapter da = new SqlDataAdapter(query, conn);
                    screeningData = new DataTable();
                    da.Fill(screeningData);

                    dgvHistory.DataSource = screeningData;
                    FormatHistoryGrid();
                    NoSort(dgvHistory);

                    UpdateStatus($"✅ Loaded {screeningData.Rows.Count} screening record(s)", Color.FromArgb(16, 185, 129));
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LoadScreeningHistory Error: {ex.Message}");
                dgvHistory.DataSource = null;
                UpdateStatus($"❌ Database error: {ex.Message}", Color.FromArgb(220, 38, 38));
            }
        }

        private void FormatHistoryGrid()
        {
            if (dgvHistory.Columns.Count == 0) return;

            if (dgvHistory.Columns.Contains("ScreeningID"))
                dgvHistory.Columns["ScreeningID"].Visible = false;

            // Set column widths
            if (dgvHistory.Columns.Contains("Donor ID"))
                dgvHistory.Columns["Donor ID"].FillWeight = 8;
            if (dgvHistory.Columns.Contains("Donor Name"))
                dgvHistory.Columns["Donor Name"].FillWeight = 12;
            if (dgvHistory.Columns.Contains("Blood Group"))
                dgvHistory.Columns["Blood Group"].FillWeight = 6;
            if (dgvHistory.Columns.Contains("Weight (kg)"))
                dgvHistory.Columns["Weight (kg)"].FillWeight = 6;
            if (dgvHistory.Columns.Contains("BP (mmHg)"))
                dgvHistory.Columns["BP (mmHg)"].FillWeight = 8;
            if (dgvHistory.Columns.Contains("Hb (g/dL)"))
                dgvHistory.Columns["Hb (g/dL)"].FillWeight = 6;
            if (dgvHistory.Columns.Contains("Temp (°C)"))
                dgvHistory.Columns["Temp (°C)"].FillWeight = 6;
            if (dgvHistory.Columns.Contains("Pulse (bpm)"))
                dgvHistory.Columns["Pulse (bpm)"].FillWeight = 6;
            if (dgvHistory.Columns.Contains("Result"))
                dgvHistory.Columns["Result"].FillWeight = 8;
            if (dgvHistory.Columns.Contains("Date/Time"))
                dgvHistory.Columns["Date/Time"].FillWeight = 12;
        }

        // =========================================================
        // LOAD SELECTED RECORD FOR VIEWING
        // =========================================================
        private void DgvHistory_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvHistory.SelectedRows.Count > 0 && dgvHistory.SelectedRows[0].DataBoundItem != null)
            {
                DataRowView row = dgvHistory.SelectedRows[0].DataBoundItem as DataRowView;
                if (row != null)
                {
                    isEditMode = true;
                    currentScreeningID = Convert.ToInt32(row["ScreeningID"]);

                    txtDonorID.Text = row["Donor ID"]?.ToString();
                    txtDonorName.Text = row["Donor Name"]?.ToString();
                    txtWeight.Text = row["Weight (kg)"]?.ToString();
                    txtBP.Text = row["BP (mmHg)"]?.ToString();
                    txtHb.Text = row["Hb (g/dL)"]?.ToString();
                    txtTemp.Text = row["Temp (°C)"]?.ToString();
                    txtPulse.Text = row["Pulse (bpm)"]?.ToString();

                    SetComboValue(cmbBloodGroup, row["Blood Group"]?.ToString());

                    string result = row["Result"]?.ToString();
                    if (result == "Fit")
                        radFit.Checked = true;
                    else if (result == "Defer")
                        radDefer.Checked = true;

                    UpdateStatus("📋 Viewing existing screening record", Color.FromArgb(59, 130, 246));
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
        // SAVE SCREENING RECORD - INSERT OR UPDATE (FIXED)
        // =========================================================
        private async void BtnSave_Click(object sender, EventArgs e)
        {
            // Validation
            if (string.IsNullOrWhiteSpace(txtDonorID.Text))
            {
                MessageBox.Show("Please search and select a donor first.", "No Donor Selected",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtWeight.Text) || string.IsNullOrWhiteSpace(txtHb.Text))
            {
                MessageBox.Show("Weight and Hemoglobin are required fields.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Define result variable OUTSIDE the using block
            string result = radFit.Checked ? "Fit" : "Defer";

            try
            {
                ShowLoading("Saving screening record...");

                using (var conn = DatabaseHelper.GetConnection())
                {
                    await conn.OpenAsync();

                    if (isEditMode)
                    {
                        // UPDATE existing record
                        string updateSql = @"
                            UPDATE DonorScreenings SET 
                                Weight = @Weight,
                                BloodPressure = @BloodPressure,
                                Hemoglobin = @Hemoglobin,
                                Temperature = @Temperature,
                                Pulse = @Pulse,
                                ScreeningResult = @Result,
                                ScreeningDate = GETDATE()
                            WHERE ScreeningID = @ScreeningID";

                        using (var cmd = new SqlCommand(updateSql, conn))
                        {
                            cmd.Parameters.AddWithValue("@ScreeningID", currentScreeningID);
                            cmd.Parameters.AddWithValue("@Weight", ParseDecimal(txtWeight.Text));
                            cmd.Parameters.AddWithValue("@BloodPressure", txtBP.Text ?? "");
                            cmd.Parameters.AddWithValue("@Hemoglobin", ParseDecimal(txtHb.Text));
                            cmd.Parameters.AddWithValue("@Temperature", ParseDecimal(txtTemp.Text));
                            cmd.Parameters.AddWithValue("@Pulse", ParseInt(txtPulse.Text));
                            cmd.Parameters.AddWithValue("@Result", result);

                            await cmd.ExecuteNonQueryAsync();
                        }

                        MessageBox.Show("Screening record updated successfully!",
                            "Update Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        // INSERT new record
                        string insertSql = @"
                            INSERT INTO DonorScreenings 
                            (DonorID, DonorName, BloodGroup, Weight, BloodPressure, 
                             Hemoglobin, Temperature, Pulse, ScreeningResult, ScreenedBy, CreatedAt)
                            VALUES 
                            (@DonorID, @DonorName, @BloodGroup, @Weight, @BloodPressure,
                             @Hemoglobin, @Temperature, @Pulse, @Result, @ScreenedBy, GETDATE())";

                        using (var cmd = new SqlCommand(insertSql, conn))
                        {
                            cmd.Parameters.AddWithValue("@DonorID", txtDonorID.Text);
                            cmd.Parameters.AddWithValue("@DonorName", txtDonorName.Text);
                            cmd.Parameters.AddWithValue("@BloodGroup", cmbBloodGroup.SelectedItem?.ToString() ?? "");
                            cmd.Parameters.AddWithValue("@Weight", ParseDecimal(txtWeight.Text));
                            cmd.Parameters.AddWithValue("@BloodPressure", txtBP.Text ?? "");
                            cmd.Parameters.AddWithValue("@Hemoglobin", ParseDecimal(txtHb.Text));
                            cmd.Parameters.AddWithValue("@Temperature", ParseDecimal(txtTemp.Text));
                            cmd.Parameters.AddWithValue("@Pulse", ParseInt(txtPulse.Text));
                            cmd.Parameters.AddWithValue("@Result", result);
                            cmd.Parameters.AddWithValue("@ScreenedBy", SessionManager.CurrentUserID);

                            await cmd.ExecuteNonQueryAsync();
                        }

                        MessageBox.Show("Screening record saved successfully!",
                            "Save Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }

                // If donor is deferred, update donor status
                if (result == "Defer")
                {
                    await UpdateDonorDeferralStatus();
                }

                // Log the activity
                AuditHelper.Log("Medical Screening", "Donor",
                    $"Donor {txtDonorName.Text} ({txtDonorID.Text}) screened - Result: {result}");

                // Refresh the grid
                LoadScreeningHistory();
                ClearForm();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"BtnSave_Click Error: {ex.Message}");
                MessageBox.Show($"Error saving screening record: {ex.Message}",
                    "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                UpdateStatus($"❌ Save failed: {ex.Message}", Color.FromArgb(220, 38, 38));
            }
            finally
            {
                HideLoading();
            }
        }

        // =========================================================
        // UPDATE DONOR DEFERRAL STATUS
        // =========================================================
        private async Task UpdateDonorDeferralStatus()
        {
            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    await conn.OpenAsync();

                    // Check if donor already has active deferral
                    string checkSql = "SELECT COUNT(*) FROM DonorDeferrals WHERE DonorID = @DonorID AND Status = 'Active'";
                    using (var checkCmd = new SqlCommand(checkSql, conn))
                    {
                        checkCmd.Parameters.AddWithValue("@DonorID", txtDonorID.Text);
                        int count = Convert.ToInt32(await checkCmd.ExecuteScalarAsync());

                        if (count == 0)
                        {
                            // Create deferral record
                            string insertSql = @"
                                INSERT INTO DonorDeferrals 
                                (DonorID, DonorName, BloodGroup, DeferralType, DeferralPeriod, 
                                 DeferralDate, Reason, Status, DeferredBy, CreatedAt)
                                VALUES 
                                (@DonorID, @DonorName, @BloodGroup, 'Medical Reason', '3 months',
                                 @DeferralDate, @Reason, 'Active', @DeferredBy, GETDATE())";

                            using (var cmd = new SqlCommand(insertSql, conn))
                            {
                                cmd.Parameters.AddWithValue("@DonorID", txtDonorID.Text);
                                cmd.Parameters.AddWithValue("@DonorName", txtDonorName.Text);
                                cmd.Parameters.AddWithValue("@BloodGroup", cmbBloodGroup.SelectedItem?.ToString() ?? "");
                                cmd.Parameters.AddWithValue("@DeferralDate", DateTime.Now);
                                cmd.Parameters.AddWithValue("@Reason", $"Failed medical screening - Hb: {txtHb.Text}g/dL, BP: {txtBP.Text}");
                                cmd.Parameters.AddWithValue("@DeferredBy", SessionManager.CurrentUserID);

                                await cmd.ExecuteNonQueryAsync();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"UpdateDonorDeferralStatus Error: {ex.Message}");
            }
        }

        private void ClearForm()
        {
            isEditMode = false;
            currentScreeningID = 0;

            txtDonorID.Clear();
            txtDonorName.Clear();
            txtWeight.Clear();
            txtBP.Clear();
            txtHb.Clear();
            txtTemp.Clear();
            txtPulse.Clear();

            cmbBloodGroup.SelectedIndex = 0;
            radFit.Checked = true;

            UpdateStatus("✅ Ready - Search donor to start screening", Color.FromArgb(16, 185, 129));
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

        private void DgvHistory_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.ColumnIndex >= 0 && dgvHistory.Columns[e.ColumnIndex].Name == "Result" && e.Value != null)
            {
                string result = e.Value.ToString();
                if (result == "Fit")
                    e.CellStyle.ForeColor = Color.FromArgb(16, 185, 129);
                else if (result == "Defer")
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
                Location = new Point(x + 120, y - 3),
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
                Location = new Point(x + 120, y - 3),
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
            foreach (DataGridViewColumn col in dgv.Columns)
                col.SortMode = DataGridViewColumnSortMode.NotSortable;
        }
    }
}