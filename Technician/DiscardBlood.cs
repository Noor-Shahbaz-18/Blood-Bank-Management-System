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
    public partial class DiscardBlood : BaseTechnicianForm
    {
        private Panel pnlFormCard;
        private DataGridView dgvDiscardedBags;
        private TextBox txtBagID, txtDonorName, txtQuantity, txtReason;
        private ComboBox cmbBloodGroup, cmbDiscardReason, cmbStatus;
        private DateTimePicker dtpExpiryDate, dtpDiscardDate;
        private Button btnDiscard, btnClear, btnRefresh, btnSearchBag;
        private Label lblStatus;
        private DataTable discardedData;
        private bool isEditMode = false;
        private int currentDiscardID = 0;

        public DiscardBlood()
        {
            this.Text = "Blood Bank Management System – Discard Blood";
            BuildLayout();
            BuildSidebar("Discard Blood");
            BuildTopBar("Discard Blood Management");
            BuildContentArea();

            // Load data from database
            LoadDiscardedHistory();
            EnsureDiscardTableExists();
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

            // Search Bag Panel
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

            Label lblSearchBag = new Label
            {
                Text = "Search Bag ID:",
                Font = new Font("Segoe UI", 10),
                ForeColor = C_TextMid,
                Location = new Point(20, 15),
                AutoSize = true
            };
            pnlSearch.Controls.Add(lblSearchBag);

            TextBox txtSearchBag = new TextBox
            {
                Location = new Point(130, 12),
                Width = 180,
                Font = new Font("Segoe UI", 10),
                BorderStyle = BorderStyle.FixedSingle
            };
            pnlSearch.Controls.Add(txtSearchBag);

            btnSearchBag = new Button
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
            btnSearchBag.FlatAppearance.BorderSize = 0;
            SetRoundedRegion(btnSearchBag, 6);
            btnSearchBag.Click += (s, e) => SearchBag(txtSearchBag.Text);
            pnlSearch.Controls.Add(btnSearchBag);

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
                Text = "🗑️ Discard Blood Bag",
                Font = new Font("Segoe UI", 14f, FontStyle.Bold),
                ForeColor = C_TextDark,
                Location = new Point(25, 20),
                AutoSize = true
            });

            int leftX = 25, rightX = 420, fieldY = 70, spacingY = 45;

            // Left Column
            AddLabelTextBox("Bag ID:", leftX, fieldY, out txtBagID, 180);
            txtBagID.TextChanged += TxtBagID_TextChanged;

            AddLabelTextBox("Donor Name:", leftX, fieldY + spacingY, out txtDonorName, 180);
            txtDonorName.ReadOnly = true;
            txtDonorName.BackColor = Color.FromArgb(245, 245, 250);

            AddLabelTextBox("Quantity (mL):", leftX, fieldY + spacingY * 2, out txtQuantity, 180);
            txtQuantity.ReadOnly = true;
            txtQuantity.BackColor = Color.FromArgb(245, 245, 250);

            AddLabelCombo("Blood Group:", leftX, fieldY + spacingY * 3, new[] { "A+", "A-", "B+", "B-", "AB+", "AB-", "O+", "O-" }, out cmbBloodGroup, 150);
            cmbBloodGroup.Enabled = false;

            // Right Column
            AddLabelCombo("Discard Reason:", rightX, fieldY, new[] { "Expired", "Contaminated", "Hemolyzed", "Leakage", "Abnormal Color", "Wrong Label", "TTI Reactive", "Other" }, out cmbDiscardReason, 200);

            pnlFormCard.Controls.Add(new Label
            {
                Text = "Expiry Date:",
                Font = new Font("Segoe UI", 10),
                ForeColor = C_TextMid,
                Location = new Point(rightX, fieldY + spacingY),
                AutoSize = true
            });
            dtpExpiryDate = new DateTimePicker
            {
                Location = new Point(rightX + 120, fieldY + spacingY - 3),
                Width = 150,
                Format = DateTimePickerFormat.Short,
                Enabled = false,
                BackColor = Color.FromArgb(245, 245, 250)
            };
            pnlFormCard.Controls.Add(dtpExpiryDate);

            pnlFormCard.Controls.Add(new Label
            {
                Text = "Discard Date:",
                Font = new Font("Segoe UI", 10),
                ForeColor = C_TextMid,
                Location = new Point(rightX, fieldY + spacingY * 2),
                AutoSize = true
            });
            dtpDiscardDate = new DateTimePicker
            {
                Location = new Point(rightX + 120, fieldY + spacingY * 2 - 3),
                Width = 150,
                Format = DateTimePickerFormat.Short,
                Value = DateTime.Now
            };
            pnlFormCard.Controls.Add(dtpDiscardDate);

            AddLabelCombo("Status:", rightX, fieldY + spacingY * 3, new[] { "Discarded", "Disposed" }, out cmbStatus, 150);

            // Reason Details
            AddLabelTextBox("Reason Details:", leftX, fieldY + spacingY * 4, out txtReason, 350);
            txtReason.Multiline = true;
            txtReason.Height = 50;
            txtReason.MaxLength = 500;

            // Buttons
            int btnY = fieldY + spacingY * 5 + 10;

            btnDiscard = new Button
            {
                Text = "🗑️ Discard Bag",
                Location = new Point(rightX, btnY),
                Size = new Size(150, 40),
                BackColor = Color.FromArgb(220, 38, 38),
                ForeColor = C_White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnDiscard.FlatAppearance.BorderSize = 0;
            SetRoundedRegion(btnDiscard, 8);
            btnDiscard.Click += BtnDiscard_Click;
            pnlFormCard.Controls.Add(btnDiscard);

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
            btnRefresh.Click += (s, e) => LoadDiscardedHistory();
            pnlFormCard.Controls.Add(btnRefresh);

            y += pnlFormCard.Height + 30;

            // Discarded History Header
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
                Text = "📋 Discarded Bags History",
                Font = new Font("Segoe UI", 14f, FontStyle.Bold),
                ForeColor = C_TextDark,
                Location = new Point(0, 5),
                AutoSize = true
            };
            headerPanel.Controls.Add(lblHistory);
            y += 35;

            dgvDiscardedBags = CreateStyledGrid();
            dgvDiscardedBags.Location = new Point(0, y);
            dgvDiscardedBags.Height = 280;
            dgvDiscardedBags.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            dgvDiscardedBags.RowPrePaint += DgvDiscardedBags_RowPrePaint;
            dgvDiscardedBags.SelectionChanged += DgvDiscardedBags_SelectionChanged;
            WrapInCard(dgvDiscardedBags, pnlContent, new Point(0, y), 280);
            FixDataGridView(dgvDiscardedBags);

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
        // ENSURE DISCARD TABLE EXISTS
        // =========================================================
        private void EnsureDiscardTableExists()
        {
            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();

                    string createTableSql = @"
                        IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='DiscardedBags' AND xtype='U')
                        CREATE TABLE DiscardedBags (
                            DiscardID INT IDENTITY(1,1) PRIMARY KEY,
                            BagID NVARCHAR(50) NOT NULL,
                            DonorName NVARCHAR(200),
                            BloodGroup NVARCHAR(10),
                            Quantity INT,
                            DiscardReason NVARCHAR(100),
                            ReasonDetails NVARCHAR(500),
                            ExpiryDate DATE,
                            DiscardDate DATE,
                            Status NVARCHAR(50),
                            DiscardedBy INT,
                            CreatedAt DATETIME DEFAULT GETDATE()
                        )";

                    using (var cmd = new SqlCommand(createTableSql, conn))
                    {
                        cmd.ExecuteNonQuery();
                    }

                    // Create indexes
                    string createIndexes = @"
                        IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name='IX_DiscardedBags_BagID')
                        CREATE INDEX IX_DiscardedBags_BagID ON DiscardedBags(BagID);
                        
                        IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name='IX_DiscardedBags_DiscardDate')
                        CREATE INDEX IX_DiscardedBags_DiscardDate ON DiscardedBags(DiscardDate)";

                    using (var cmd = new SqlCommand(createIndexes, conn))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"EnsureDiscardTableExists Error: {ex.Message}");
            }
        }

        // =========================================================
        // SEARCH BAG FROM DATABASE
        // =========================================================
        private void SearchBag(string bagId)
        {
            if (string.IsNullOrWhiteSpace(bagId))
            {
                MessageBox.Show("Please enter Bag ID to search.", "Search",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();

                    string query = @"
                        SELECT 
                            BagID,
                            ISNULL(DonorName, 'N/A') as DonorName,
                            BloodGroup,
                            Volume,
                            ExpiryDate,
                            Status
                        FROM BloodBags 
                        WHERE BagID = @BagID";

                    using (var cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@BagID", bagId);

                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                txtBagID.Text = reader["BagID"].ToString();
                                txtDonorName.Text = reader["DonorName"].ToString();
                                txtQuantity.Text = reader["Volume"].ToString();

                                string bloodGroup = reader["BloodGroup"].ToString();
                                int index = cmbBloodGroup.Items.IndexOf(bloodGroup);
                                if (index >= 0)
                                    cmbBloodGroup.SelectedIndex = index;

                                DateTime expiryDate = Convert.ToDateTime(reader["ExpiryDate"]);
                                dtpExpiryDate.Value = expiryDate;

                                string status = reader["Status"].ToString();

                                // Check if already discarded
                                if (status == "Discarded")
                                {
                                    UpdateStatus("⚠️ This bag has already been discarded", Color.FromArgb(245, 158, 11));
                                    btnDiscard.Enabled = false;
                                }
                                else
                                {
                                    UpdateStatus($"✅ Bag found: {bagId}", Color.FromArgb(16, 185, 129));
                                    btnDiscard.Enabled = true;
                                }
                            }
                            else
                            {
                                MessageBox.Show($"No bag found with ID: {bagId}",
                                    "Not Found", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                ClearForm();
                                UpdateStatus("⚠️ Bag not found", Color.FromArgb(245, 158, 11));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SearchBag Error: {ex.Message}");
                MessageBox.Show($"Error searching bag: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void TxtBagID_TextChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(txtBagID.Text))
            {
                SearchBag(txtBagID.Text);
            }
        }

        // =========================================================
        // LOAD DISCARDED HISTORY FROM DATABASE
        // =========================================================
        private void LoadDiscardedHistory()
        {
            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();

                    // Check if table exists
                    string checkTable = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'DiscardedBags'";
                    using (var checkCmd = new SqlCommand(checkTable, conn))
                    {
                        int tableExists = Convert.ToInt32(checkCmd.ExecuteScalar());
                        if (tableExists == 0)
                        {
                            dgvDiscardedBags.DataSource = null;
                            UpdateStatus("⚠️ Table 'DiscardedBags' not found", Color.FromArgb(245, 158, 11));
                            return;
                        }
                    }

                    string query = @"
                        SELECT 
                            DiscardID,
                            BagID as [Bag ID],
                            DonorName as [Donor Name],
                            BloodGroup as [Blood Group],
                            Quantity as [Quantity (mL)],
                            DiscardReason as [Discard Reason],
                            ReasonDetails as [Reason Details],
                            FORMAT(ExpiryDate, 'dd-MMM-yyyy') as [Expiry Date],
                            FORMAT(DiscardDate, 'dd-MMM-yyyy') as [Discard Date],
                            Status,
                            FORMAT(CreatedAt, 'dd-MMM-yyyy HH:mm') as [Recorded At]
                        FROM DiscardedBags 
                        ORDER BY DiscardDate DESC, DiscardID DESC";

                    SqlDataAdapter da = new SqlDataAdapter(query, conn);
                    discardedData = new DataTable();
                    da.Fill(discardedData);

                    dgvDiscardedBags.DataSource = discardedData;
                    FormatHistoryGrid();
                    NoSort(dgvDiscardedBags);

                    UpdateStatus($"✅ Loaded {discardedData.Rows.Count} discarded bag(s)", Color.FromArgb(16, 185, 129));
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LoadDiscardedHistory Error: {ex.Message}");
                dgvDiscardedBags.DataSource = null;
                UpdateStatus($"❌ Database error: {ex.Message}", Color.FromArgb(220, 38, 38));
            }
        }

        private void FormatHistoryGrid()
        {
            if (dgvDiscardedBags.Columns.Count == 0) return;

            if (dgvDiscardedBags.Columns.Contains("DiscardID"))
                dgvDiscardedBags.Columns["DiscardID"].Visible = false;

            if (dgvDiscardedBags.Columns.Contains("Bag ID"))
                dgvDiscardedBags.Columns["Bag ID"].FillWeight = 10;
            if (dgvDiscardedBags.Columns.Contains("Donor Name"))
                dgvDiscardedBags.Columns["Donor Name"].FillWeight = 12;
            if (dgvDiscardedBags.Columns.Contains("Blood Group"))
                dgvDiscardedBags.Columns["Blood Group"].FillWeight = 6;
            if (dgvDiscardedBags.Columns.Contains("Discard Reason"))
                dgvDiscardedBags.Columns["Discard Reason"].FillWeight = 12;
            if (dgvDiscardedBags.Columns.Contains("Reason Details"))
                dgvDiscardedBags.Columns["Reason Details"].FillWeight = 20;
            if (dgvDiscardedBags.Columns.Contains("Discard Date"))
                dgvDiscardedBags.Columns["Discard Date"].FillWeight = 8;
            if (dgvDiscardedBags.Columns.Contains("Status"))
                dgvDiscardedBags.Columns["Status"].FillWeight = 8;
        }

        // =========================================================
        // LOAD SELECTED RECORD FOR VIEWING
        // =========================================================
        private void DgvDiscardedBags_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvDiscardedBags.SelectedRows.Count > 0 && dgvDiscardedBags.SelectedRows[0].DataBoundItem != null)
            {
                DataRowView row = dgvDiscardedBags.SelectedRows[0].DataBoundItem as DataRowView;
                if (row != null)
                {
                    currentDiscardID = Convert.ToInt32(row["DiscardID"]);

                    txtBagID.Text = row["Bag ID"]?.ToString();
                    txtDonorName.Text = row["Donor Name"]?.ToString();
                    txtQuantity.Text = row["Quantity (mL)"]?.ToString();

                    SetComboValue(cmbBloodGroup, row["Blood Group"]?.ToString());
                    SetComboValue(cmbDiscardReason, row["Discard Reason"]?.ToString());
                    SetComboValue(cmbStatus, row["Status"]?.ToString());

                    txtReason.Text = row["Reason Details"]?.ToString();

                    if (DateTime.TryParse(row["Discard Date"]?.ToString(), out DateTime discardDate))
                        dtpDiscardDate.Value = discardDate;

                    btnDiscard.Enabled = false;
                    UpdateStatus("📋 Viewing discarded record", Color.FromArgb(59, 130, 246));
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
        // DISCARD BAG - SAVE TO DATABASE
        // =========================================================
        private async void BtnDiscard_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtBagID.Text))
            {
                MessageBox.Show("Please search and select a bag first.", "No Bag Selected",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(cmbDiscardReason.Text))
            {
                MessageBox.Show("Please select a discard reason.", "Reason Required",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                ShowLoading("Processing discard...");

                using (var conn = DatabaseHelper.GetConnection())
                {
                    await conn.OpenAsync();
                    using (var transaction = conn.BeginTransaction())
                    {
                        try
                        {
                            // 1. Insert into DiscardedBags table
                            string insertSql = @"
                                INSERT INTO DiscardedBags 
                                (BagID, DonorName, BloodGroup, Quantity, DiscardReason, 
                                 ReasonDetails, ExpiryDate, DiscardDate, Status, DiscardedBy, CreatedAt)
                                VALUES 
                                (@BagID, @DonorName, @BloodGroup, @Quantity, @DiscardReason,
                                 @ReasonDetails, @ExpiryDate, @DiscardDate, 'Discarded', @DiscardedBy, GETDATE())";

                            using (var cmd = new SqlCommand(insertSql, conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@BagID", txtBagID.Text);
                                cmd.Parameters.AddWithValue("@DonorName", txtDonorName.Text ?? "");
                                cmd.Parameters.AddWithValue("@BloodGroup", cmbBloodGroup.SelectedItem?.ToString() ?? "");
                                cmd.Parameters.AddWithValue("@Quantity", string.IsNullOrWhiteSpace(txtQuantity.Text) ? 0 : Convert.ToInt32(txtQuantity.Text));
                                cmd.Parameters.AddWithValue("@DiscardReason", cmbDiscardReason.SelectedItem?.ToString());
                                cmd.Parameters.AddWithValue("@ReasonDetails", txtReason.Text ?? "");
                                cmd.Parameters.AddWithValue("@ExpiryDate", dtpExpiryDate.Value);
                                cmd.Parameters.AddWithValue("@DiscardDate", dtpDiscardDate.Value);
                                cmd.Parameters.AddWithValue("@DiscardedBy", SessionManager.CurrentUserID);

                                await cmd.ExecuteNonQueryAsync();
                            }

                            // 2. Update the original blood bag status
                            string updateBagSql = @"
                                UPDATE BloodBags 
                                SET Status = 'Discarded', 
                                    Remarks = ISNULL(Remarks, '') + CHAR(10) + 'Discarded on ' + CAST(GETDATE() AS VARCHAR) + ' - Reason: ' + @DiscardReason,
                                    UpdatedAt = GETDATE()
                                WHERE BagID = @BagID";

                            using (var cmd = new SqlCommand(updateBagSql, conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@BagID", txtBagID.Text);
                                cmd.Parameters.AddWithValue("@DiscardReason", cmbDiscardReason.SelectedItem?.ToString());
                                await cmd.ExecuteNonQueryAsync();
                            }

                            transaction.Commit();
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            throw ex;
                        }
                    }
                }

                // Log the activity
                AuditHelper.Log("Discard Blood", "BloodBag",
                    $"Bag {txtBagID.Text} discarded - Reason: {cmbDiscardReason.SelectedItem}");

                MessageBox.Show($"Bag {txtBagID.Text} has been marked as discarded successfully!",
                    "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Refresh both lists
                LoadDiscardedHistory();
                ClearForm();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"BtnDiscard_Click Error: {ex.Message}");
                MessageBox.Show($"Error discarding bag: {ex.Message}",
                    "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                UpdateStatus($"❌ Discard failed: {ex.Message}", Color.FromArgb(220, 38, 38));
            }
            finally
            {
                HideLoading();
            }
        }

        private void ClearForm()
        {
            isEditMode = false;
            currentDiscardID = 0;

            txtBagID.Clear();
            txtDonorName.Clear();
            txtQuantity.Clear();
            txtReason.Clear();

            cmbBloodGroup.SelectedIndex = 0;
            cmbDiscardReason.SelectedIndex = 0;
            cmbStatus.SelectedIndex = 0;

            dtpExpiryDate.Value = DateTime.Today;
            dtpDiscardDate.Value = DateTime.Now;

            btnDiscard.Enabled = true;

            UpdateStatus("✅ Ready - Search bag to discard", Color.FromArgb(16, 185, 129));
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

        private void DgvDiscardedBags_RowPrePaint(object sender, DataGridViewRowPrePaintEventArgs e)
        {
            if (e.RowIndex >= 0 && dgvDiscardedBags.Rows[e.RowIndex].Cells["Status"].Value != null)
            {
                string status = dgvDiscardedBags.Rows[e.RowIndex].Cells["Status"].Value.ToString();
                if (status == "Discarded")
                    dgvDiscardedBags.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.FromArgb(255, 235, 235);
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