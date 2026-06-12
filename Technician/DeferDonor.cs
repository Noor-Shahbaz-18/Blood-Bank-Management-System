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
    public partial class DeferDonor : BaseTechnicianForm
    {
        private Panel pnlFormCard;
        private DataGridView dgvDeferredDonors;
        private TextBox txtDonorID, txtDonorName, txtContact, txtReason;
        private ComboBox cmbBloodGroup, cmbDeferralType, cmbDeferralPeriod;
        private DateTimePicker dtpDeferralDate;
        private Button btnDefer, btnClear, btnRefresh, btnSearchDonor;
        private Label lblStatus;
        private DataTable deferredData;
        private bool isEditMode = false;
        private int currentDeferralID = 0;

        public DeferDonor()
        {
            this.Text = "Blood Bank Management System – Defer Donor";
            BuildLayout();
            BuildSidebar("Defer Donor");
            BuildTopBar("Defer Donor Management");
            BuildContentArea();

            // Load data from database
            LoadDeferredDonors();
            EnsureDeferralTableExists();
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
                Width = 200,
                Font = new Font("Segoe UI", 10),
                BorderStyle = BorderStyle.FixedSingle
            };
            pnlSearch.Controls.Add(txtSearchDonor);

            btnSearchDonor = new Button
            {
                Text = "🔍 Search",
                Location = new Point(345, 10),
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
                Height = 380,
                BackColor = C_CardBg,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            SetRoundedRegion(pnlFormCard, 15);
            AddDropShadow(pnlFormCard);
            pnlContent.Controls.Add(pnlFormCard);

            pnlFormCard.Controls.Add(new Label
            {
                Text = "🚫 Defer Donor",
                Font = new Font("Segoe UI", 14f, FontStyle.Bold),
                ForeColor = C_TextDark,
                Location = new Point(25, 20),
                AutoSize = true
            });

            int leftX = 25, rightX = 420, fieldY = 70, spacingY = 45;

            // Left Column
            AddLabelTextBox("Donor ID:", leftX, fieldY, out txtDonorID, 180);
            txtDonorID.TextChanged += TxtDonorID_TextChanged;

            AddLabelTextBox("Donor Name:", leftX, fieldY + spacingY, out txtDonorName, 180);
            txtDonorName.ReadOnly = true;
            txtDonorName.BackColor = Color.FromArgb(245, 245, 250);

            AddLabelTextBox("Contact Number:", leftX, fieldY + spacingY * 2, out txtContact, 180);
            txtContact.ReadOnly = true;
            txtContact.BackColor = Color.FromArgb(245, 245, 250);

            AddLabelCombo("Blood Group:", leftX, fieldY + spacingY * 3, new[] { "A+", "A-", "B+", "B-", "AB+", "AB-", "O+", "O-" }, out cmbBloodGroup, 150);
            cmbBloodGroup.Enabled = false;

            // Right Column
            AddLabelCombo("Deferral Type:", rightX, fieldY, new[] { "Temporary", "Permanent", "Medical Reason", "Behavioral" }, out cmbDeferralType, 180);

            AddLabelCombo("Deferral Period:", rightX, fieldY + spacingY, new[] { "3 months", "6 months", "12 months", "Permanent", "Indefinite" }, out cmbDeferralPeriod, 180);

            pnlFormCard.Controls.Add(new Label
            {
                Text = "Deferral Date:",
                Font = new Font("Segoe UI", 10),
                ForeColor = C_TextMid,
                Location = new Point(rightX, fieldY + spacingY * 2),
                AutoSize = true
            });
            dtpDeferralDate = new DateTimePicker
            {
                Location = new Point(rightX + 130, fieldY + spacingY * 2 - 3),
                Width = 150,
                Format = DateTimePickerFormat.Short,
                Value = DateTime.Now
            };
            pnlFormCard.Controls.Add(dtpDeferralDate);

            AddLabelTextBox("Reason for Deferral:", rightX, fieldY + spacingY * 3, out txtReason, 250);
            txtReason.Multiline = true;
            txtReason.Height = 60;
            txtReason.MaxLength = 500;

            // Buttons
            int btnY = fieldY + spacingY * 4 + 10;

            btnDefer = new Button
            {
                Text = "🚫 Defer Donor",
                Location = new Point(rightX, btnY),
                Size = new Size(150, 40),
                BackColor = brickRed,
                ForeColor = C_White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnDefer.FlatAppearance.BorderSize = 0;
            SetRoundedRegion(btnDefer, 8);
            btnDefer.Click += BtnDefer_Click;
            pnlFormCard.Controls.Add(btnDefer);

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
            btnRefresh.Click += (s, e) => LoadDeferredDonors();
            pnlFormCard.Controls.Add(btnRefresh);

            y += pnlFormCard.Height + 30;

            // Deferred Donors List Header
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
                Text = "📋 Deferred Donors List",
                Font = new Font("Segoe UI", 14f, FontStyle.Bold),
                ForeColor = C_TextDark,
                Location = new Point(0, 5),
                AutoSize = true
            };
            headerPanel.Controls.Add(lblHistory);
            y += 35;

            dgvDeferredDonors = CreateStyledGrid();
            dgvDeferredDonors.Location = new Point(0, y);
            dgvDeferredDonors.Height = 280;
            dgvDeferredDonors.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            dgvDeferredDonors.RowPrePaint += DgvDeferredDonors_RowPrePaint;
            dgvDeferredDonors.SelectionChanged += DgvDeferredDonors_SelectionChanged;
            WrapInCard(dgvDeferredDonors, pnlContent, new Point(0, y), 280);
            FixDataGridView(dgvDeferredDonors);

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
        // ENSURE DEFERRAL TABLE EXISTS
        // =========================================================
        private void EnsureDeferralTableExists()
        {
            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();

                    string createTableSql = @"
                        IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='DonorDeferrals' AND xtype='U')
                        CREATE TABLE DonorDeferrals (
                            DeferralID INT IDENTITY(1,1) PRIMARY KEY,
                            DonorID NVARCHAR(50) NOT NULL,
                            DonorName NVARCHAR(200) NOT NULL,
                            Contact NVARCHAR(20),
                            BloodGroup NVARCHAR(10),
                            DeferralType NVARCHAR(50),
                            DeferralPeriod NVARCHAR(50),
                            DeferralDate DATE,
                            Reason NVARCHAR(500),
                            Status NVARCHAR(50) DEFAULT 'Active',
                            DeferredBy INT,
                            CreatedAt DATETIME DEFAULT GETDATE(),
                            UpdatedAt DATETIME,
                            ReactivatedDate DATE
                        )";

                    using (var cmd = new SqlCommand(createTableSql, conn))
                    {
                        cmd.ExecuteNonQuery();
                    }

                    // Create indexes
                    string createIndexes = @"
                        IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name='IX_DonorDeferrals_DonorID')
                        CREATE INDEX IX_DonorDeferrals_DonorID ON DonorDeferrals(DonorID);
                        
                        IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name='IX_DonorDeferrals_Status')
                        CREATE INDEX IX_DonorDeferrals_Status ON DonorDeferrals(Status)";

                    using (var cmd = new SqlCommand(createIndexes, conn))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"EnsureDeferralTableExists Error: {ex.Message}");
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
                            d.Phone,
                            d.BloodGroup,
                            u.Email
                        FROM Donors d
                        LEFT JOIN Users u ON d.UserID = u.UserID
                        WHERE d.DonorID = @SearchTerm 
                        OR d.FullName LIKE @SearchPattern
                        OR d.CNIC = @SearchTerm
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
                                txtContact.Text = reader["Phone"].ToString();

                                string bloodGroup = reader["BloodGroup"].ToString();
                                int index = cmbBloodGroup.Items.IndexOf(bloodGroup);
                                if (index >= 0)
                                    cmbBloodGroup.SelectedIndex = index;

                                UpdateStatus($"✅ Donor found: {txtDonorName.Text}", Color.FromArgb(16, 185, 129));

                                // Check if donor is already deferred
                                CheckDonorDeferralStatus(txtDonorID.Text);
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

        private void CheckDonorDeferralStatus(string donorId)
        {
            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();
                    string query = @"
                        SELECT TOP 1 DeferralID, Status, DeferralPeriod, DeferralDate, Reason
                        FROM DonorDeferrals 
                        WHERE DonorID = @DonorID AND Status = 'Active'
                        ORDER BY DeferralDate DESC";

                    using (var cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@DonorID", donorId);
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string status = reader["Status"].ToString();
                                string period = reader["DeferralPeriod"].ToString();
                                DateTime deferralDate = Convert.ToDateTime(reader["DeferralDate"]);

                                // Check if deferral period has expired
                                bool isExpired = IsDeferralExpired(deferralDate, period);

                                if (isExpired)
                                {
                                    // Auto-reactivate donor
                                    AutoReactivateDonor(donorId);
                                }
                                else
                                {
                                    UpdateStatus($"⚠️ Donor is currently deferred until {GetExpiryDate(deferralDate, period):dd-MMM-yyyy}",
                                        Color.FromArgb(245, 158, 11));
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"CheckDonorDeferralStatus Error: {ex.Message}");
            }
        }

        private bool IsDeferralExpired(DateTime deferralDate, string period)
        {
            DateTime expiryDate = GetExpiryDate(deferralDate, period);
            return expiryDate <= DateTime.Today;
        }

        private DateTime GetExpiryDate(DateTime deferralDate, string period)
        {
            switch (period)
            {
                case "3 months": return deferralDate.AddMonths(3);
                case "6 months": return deferralDate.AddMonths(6);
                case "12 months": return deferralDate.AddMonths(12);
                case "Permanent":
                case "Indefinite":
                    return DateTime.MaxValue;
                default: return deferralDate.AddMonths(3);
            }
        }

        private void AutoReactivateDonor(string donorId)
        {
            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();
                    string sql = @"
                        UPDATE DonorDeferrals 
                        SET Status = 'Expired', 
                            ReactivatedDate = GETDATE(),
                            UpdatedAt = GETDATE()
                        WHERE DonorID = @DonorID AND Status = 'Active'";

                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@DonorID", donorId);
                        int rows = cmd.ExecuteNonQuery();

                        if (rows > 0)
                        {
                            UpdateStatus($"✅ Donor deferral period has expired. Donor is now eligible.",
                                Color.FromArgb(16, 185, 129));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"AutoReactivateDonor Error: {ex.Message}");
            }
        }

        // =========================================================
        // LOAD DEFERRED DONORS FROM DATABASE
        // =========================================================
        private void LoadDeferredDonors()
        {
            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();

                    // Check if table exists
                    string checkTable = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'DonorDeferrals'";
                    using (var checkCmd = new SqlCommand(checkTable, conn))
                    {
                        int tableExists = Convert.ToInt32(checkCmd.ExecuteScalar());
                        if (tableExists == 0)
                        {
                            dgvDeferredDonors.DataSource = null;
                            UpdateStatus("⚠️ Table 'DonorDeferrals' not found", Color.FromArgb(245, 158, 11));
                            return;
                        }
                    }

                    string query = @"
                        SELECT 
                            DeferralID,
                            DonorID as [Donor ID],
                            DonorName as [Donor Name],
                            Contact,
                            BloodGroup as [Blood Group],
                            DeferralType as [Deferral Type],
                            DeferralPeriod as [Deferral Period],
                            FORMAT(DeferralDate, 'dd-MMM-yyyy') as [Deferral Date],
                            Reason,
                            Status,
                            FORMAT(CreatedAt, 'dd-MMM-yyyy HH:mm') as [Recorded At]
                        FROM DonorDeferrals 
                        WHERE Status = 'Active'
                        ORDER BY DeferralDate DESC, DeferralID DESC";

                    SqlDataAdapter da = new SqlDataAdapter(query, conn);
                    deferredData = new DataTable();
                    da.Fill(deferredData);

                    dgvDeferredDonors.DataSource = deferredData;
                    FormatHistoryGrid();
                    NoSort(dgvDeferredDonors);

                    UpdateStatus($"✅ Loaded {deferredData.Rows.Count} deferred donor(s)", Color.FromArgb(16, 185, 129));
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LoadDeferredDonors Error: {ex.Message}");
                dgvDeferredDonors.DataSource = null;
                UpdateStatus($"❌ Database error: {ex.Message}", Color.FromArgb(220, 38, 38));
            }
        }

        private void FormatHistoryGrid()
        {
            if (dgvDeferredDonors.Columns.Count == 0) return;

            if (dgvDeferredDonors.Columns.Contains("DeferralID"))
                dgvDeferredDonors.Columns["DeferralID"].Visible = false;

            if (dgvDeferredDonors.Columns.Contains("Donor ID"))
                dgvDeferredDonors.Columns["Donor ID"].FillWeight = 10;
            if (dgvDeferredDonors.Columns.Contains("Donor Name"))
                dgvDeferredDonors.Columns["Donor Name"].FillWeight = 15;
            if (dgvDeferredDonors.Columns.Contains("Blood Group"))
                dgvDeferredDonors.Columns["Blood Group"].FillWeight = 8;
            if (dgvDeferredDonors.Columns.Contains("Deferral Type"))
                dgvDeferredDonors.Columns["Deferral Type"].FillWeight = 12;
            if (dgvDeferredDonors.Columns.Contains("Deferral Period"))
                dgvDeferredDonors.Columns["Deferral Period"].FillWeight = 10;
            if (dgvDeferredDonors.Columns.Contains("Deferral Date"))
                dgvDeferredDonors.Columns["Deferral Date"].FillWeight = 10;
            if (dgvDeferredDonors.Columns.Contains("Reason"))
                dgvDeferredDonors.Columns["Reason"].FillWeight = 20;
            if (dgvDeferredDonors.Columns.Contains("Status"))
                dgvDeferredDonors.Columns["Status"].FillWeight = 8;
        }

        // =========================================================
        // LOAD SELECTED RECORD FOR VIEWING
        // =========================================================
        private void DgvDeferredDonors_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvDeferredDonors.SelectedRows.Count > 0 && dgvDeferredDonors.SelectedRows[0].DataBoundItem != null)
            {
                DataRowView row = dgvDeferredDonors.SelectedRows[0].DataBoundItem as DataRowView;
                if (row != null)
                {
                    currentDeferralID = Convert.ToInt32(row["DeferralID"]);

                    txtDonorID.Text = row["Donor ID"]?.ToString();
                    txtDonorName.Text = row["Donor Name"]?.ToString();
                    txtContact.Text = row["Contact"]?.ToString();

                    SetComboValue(cmbBloodGroup, row["Blood Group"]?.ToString());
                    SetComboValue(cmbDeferralType, row["Deferral Type"]?.ToString());
                    SetComboValue(cmbDeferralPeriod, row["Deferral Period"]?.ToString());

                    txtReason.Text = row["Reason"]?.ToString();

                    if (DateTime.TryParse(row["Deferral Date"]?.ToString(), out DateTime deferralDate))
                        dtpDeferralDate.Value = deferralDate;

                    UpdateStatus("📋 Viewing deferral record", Color.FromArgb(59, 130, 246));
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
        // DEFER DONOR - SAVE TO DATABASE
        // =========================================================
        private async void BtnDefer_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtDonorID.Text))
            {
                MessageBox.Show("Please search and select a donor first.", "No Donor Selected",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtReason.Text))
            {
                MessageBox.Show("Please provide a reason for deferral.", "Reason Required",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                ShowLoading("Saving deferral record...");

                using (var conn = DatabaseHelper.GetConnection())
                {
                    await conn.OpenAsync();

                    // Check if donor already has active deferral
                    string checkSql = "SELECT COUNT(*) FROM DonorDeferrals WHERE DonorID = @DonorID AND Status = 'Active'";
                    using (var checkCmd = new SqlCommand(checkSql, conn))
                    {
                        checkCmd.Parameters.AddWithValue("@DonorID", txtDonorID.Text);
                        int existingCount = Convert.ToInt32(await checkCmd.ExecuteScalarAsync());

                        if (existingCount > 0)
                        {
                            DialogResult result = MessageBox.Show(
                                "This donor already has an active deferral.\n\nDo you want to update it?",
                                "Existing Deferral", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                            if (result != DialogResult.Yes)
                                return;

                            // Update existing deferral
                            string updateSql = @"
                                UPDATE DonorDeferrals SET 
                                    DeferralType = @DeferralType,
                                    DeferralPeriod = @DeferralPeriod,
                                    DeferralDate = @DeferralDate,
                                    Reason = @Reason,
                                    UpdatedAt = GETDATE()
                                WHERE DonorID = @DonorID AND Status = 'Active'";

                            using (var cmd = new SqlCommand(updateSql, conn))
                            {
                                cmd.Parameters.AddWithValue("@DonorID", txtDonorID.Text);
                                cmd.Parameters.AddWithValue("@DeferralType", cmbDeferralType.SelectedItem?.ToString());
                                cmd.Parameters.AddWithValue("@DeferralPeriod", cmbDeferralPeriod.SelectedItem?.ToString());
                                cmd.Parameters.AddWithValue("@DeferralDate", dtpDeferralDate.Value);
                                cmd.Parameters.AddWithValue("@Reason", txtReason.Text);

                                await cmd.ExecuteNonQueryAsync();
                            }

                            MessageBox.Show($"Deferral record for {txtDonorName.Text} updated successfully!",
                                "Update Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            // Insert new deferral
                            string insertSql = @"
                                INSERT INTO DonorDeferrals 
                                (DonorID, DonorName, Contact, BloodGroup, DeferralType, 
                                 DeferralPeriod, DeferralDate, Reason, Status, DeferredBy, CreatedAt)
                                VALUES 
                                (@DonorID, @DonorName, @Contact, @BloodGroup, @DeferralType,
                                 @DeferralPeriod, @DeferralDate, @Reason, 'Active', @DeferredBy, GETDATE())";

                            using (var cmd = new SqlCommand(insertSql, conn))
                            {
                                cmd.Parameters.AddWithValue("@DonorID", txtDonorID.Text);
                                cmd.Parameters.AddWithValue("@DonorName", txtDonorName.Text);
                                cmd.Parameters.AddWithValue("@Contact", txtContact.Text ?? "");
                                cmd.Parameters.AddWithValue("@BloodGroup", cmbBloodGroup.SelectedItem?.ToString() ?? "");
                                cmd.Parameters.AddWithValue("@DeferralType", cmbDeferralType.SelectedItem?.ToString());
                                cmd.Parameters.AddWithValue("@DeferralPeriod", cmbDeferralPeriod.SelectedItem?.ToString());
                                cmd.Parameters.AddWithValue("@DeferralDate", dtpDeferralDate.Value);
                                cmd.Parameters.AddWithValue("@Reason", txtReason.Text);
                                cmd.Parameters.AddWithValue("@DeferredBy", SessionManager.CurrentUserID);

                                await cmd.ExecuteNonQueryAsync();
                            }

                            MessageBox.Show($"Donor {txtDonorName.Text} (ID: {txtDonorID.Text}) has been deferred successfully!",
                                "Deferral Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                }

                // Log the activity
                AuditHelper.Log("Defer Donor", "Donor",
                    $"Donor {txtDonorName.Text} ({txtDonorID.Text}) deferred - Type: {cmbDeferralType.SelectedItem}, Period: {cmbDeferralPeriod.SelectedItem}");

                // Refresh the list
                LoadDeferredDonors();
                ClearForm();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"BtnDefer_Click Error: {ex.Message}");
                MessageBox.Show($"Error saving deferral record: {ex.Message}",
                    "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                UpdateStatus($"❌ Save failed: {ex.Message}", Color.FromArgb(220, 38, 38));
            }
            finally
            {
                HideLoading();
            }
        }

        private void ClearForm()
        {
            isEditMode = false;
            currentDeferralID = 0;

            txtDonorID.Clear();
            txtDonorName.Clear();
            txtContact.Clear();
            txtReason.Clear();

            cmbBloodGroup.SelectedIndex = 0;
            cmbDeferralType.SelectedIndex = 0;
            cmbDeferralPeriod.SelectedIndex = 0;

            dtpDeferralDate.Value = DateTime.Now;

            UpdateStatus("✅ Ready - Search donor to defer", Color.FromArgb(16, 185, 129));
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

        private void DgvDeferredDonors_RowPrePaint(object sender, DataGridViewRowPrePaintEventArgs e)
        {
            if (e.RowIndex >= 0 && dgvDeferredDonors.Rows[e.RowIndex].Cells["Status"].Value != null)
            {
                string status = dgvDeferredDonors.Rows[e.RowIndex].Cells["Status"].Value.ToString();
                if (status == "Active")
                    dgvDeferredDonors.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.FromArgb(255, 235, 235);
                else if (status == "Expired")
                    dgvDeferredDonors.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.FromArgb(220, 240, 220);
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
            foreach (DataGridViewColumn col in dgv.Columns)
                col.SortMode = DataGridViewColumnSortMode.NotSortable;
        }
    }
}