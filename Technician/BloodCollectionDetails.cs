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
    public partial class BloodCollectionDetails : BaseTechnicianForm
    {
        private Panel pnlFormCard;
        private DataGridView dgvCollections;
        private TextBox txtBagID, txtDonorID, txtDonorName, txtVolume;
        private ComboBox cmbBloodGroup, cmbCollectionMethod, cmbStatus;
        private DateTimePicker dtpCollectionDate;
        private Button btnSave, btnClear, btnRefresh, btnDelete;
        private DataTable collectionData;
        private bool isEditMode = false;
        private Label lblStatus;
        private string currentBagID = "";

        public BloodCollectionDetails()
        {
            this.Text = "Blood Bank Management System – Blood Collection Details";
            BuildLayout();
            BuildSidebar("Blood Collection");
            BuildTopBar("Blood Collection Details");
            BuildContentArea();

            // Load data from database
            LoadCollectionHistory();
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
                Location = new Point(pnlContent.Width - 220, y),
                AutoSize = true,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            pnlContent.Controls.Add(lblStatus);

            pnlFormCard = new Panel
            {
                Location = new Point(0, y + 30),
                Width = pnlContent.ClientSize.Width - 60,
                Height = 350,
                BackColor = C_CardBg,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            SetRoundedRegion(pnlFormCard, 15);
            AddDropShadow(pnlFormCard);
            pnlContent.Controls.Add(pnlFormCard);

            pnlFormCard.Controls.Add(new Label
            {
                Text = "🩸 Record Blood Collection",
                Font = new Font("Segoe UI", 14f, FontStyle.Bold),
                ForeColor = C_TextDark,
                Location = new Point(25, 20),
                AutoSize = true
            });

            int leftX = 25, rightX = 380, fieldY = 70, spacingY = 45;

            // Left Column
            AddLabelTextBox("Bag ID:", leftX, fieldY, out txtBagID, 180);
            txtBagID.TextChanged += TxtBagID_TextChanged;

            AddLabelTextBox("Donor ID:", leftX, fieldY + spacingY, out txtDonorID, 180);
            txtDonorID.TextChanged += TxtDonorID_TextChanged;

            AddLabelTextBox("Donor Name:", leftX, fieldY + spacingY * 2, out txtDonorName, 180);
            txtDonorName.ReadOnly = true;
            txtDonorName.BackColor = Color.FromArgb(245, 245, 250);

            pnlFormCard.Controls.Add(new Label
            {
                Text = "Collection Date:",
                Font = new Font("Segoe UI", 10),
                ForeColor = C_TextMid,
                Location = new Point(leftX, fieldY + spacingY * 3),
                AutoSize = true
            });
            dtpCollectionDate = new DateTimePicker
            {
                Location = new Point(leftX + 130, fieldY + spacingY * 3 - 3),
                Width = 150,
                Format = DateTimePickerFormat.Short,
                Value = DateTime.Now
            };
            pnlFormCard.Controls.Add(dtpCollectionDate);

            // Right Column
            AddLabelTextBox("Collection Volume (mL):", rightX, fieldY, out txtVolume, 150);
            txtVolume.Text = "450";

            AddLabelCombo("Blood Group:", rightX, fieldY + spacingY, new[] { "A+", "A-", "B+", "B-", "AB+", "AB-", "O+", "O-" }, out cmbBloodGroup, 150);

            AddLabelCombo("Collection Method:", rightX, fieldY + spacingY * 2, new[] { "Whole Blood", "Apheresis", "Double Red Cell" }, out cmbCollectionMethod, 160);

            AddLabelCombo("Status:", rightX, fieldY + spacingY * 3, new[] { "Collected", "In Lab", "Screening Pending", "Ready" }, out cmbStatus, 150);

            // Buttons
            int btnY = fieldY + spacingY * 4 + 10;

            btnSave = new Button
            {
                Text = "💾 Save Collection",
                Location = new Point(leftX, btnY),
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
                Location = new Point(leftX + 170, btnY),
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

            btnDelete = new Button
            {
                Text = "🗑️ Delete Record",
                Location = new Point(leftX + 310, btnY),
                Size = new Size(130, 40),
                BackColor = Color.FromArgb(220, 38, 38),
                ForeColor = C_White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnDelete.FlatAppearance.BorderSize = 0;
            SetRoundedRegion(btnDelete, 8);
            btnDelete.Click += BtnDelete_Click;
            pnlFormCard.Controls.Add(btnDelete);

            btnRefresh = new Button
            {
                Text = "🔄 Refresh List",
                Location = new Point(rightX + 100, btnY),
                Size = new Size(130, 40),
                BackColor = Color.FromArgb(60, 70, 80),
                ForeColor = C_White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnRefresh.FlatAppearance.BorderSize = 0;
            SetRoundedRegion(btnRefresh, 8);
            btnRefresh.Click += (s, e) => LoadCollectionHistory();
            pnlFormCard.Controls.Add(btnRefresh);

            y += pnlFormCard.Height + 30;

            // History Section
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
                Text = "📋 Collection History",
                Font = new Font("Segoe UI", 14f, FontStyle.Bold),
                ForeColor = C_TextDark,
                Location = new Point(0, 5),
                AutoSize = true
            };
            historyHeader.Controls.Add(lblHistory);
            y += 35;

            dgvCollections = CreateStyledGrid();
            dgvCollections.Location = new Point(0, y);
            dgvCollections.Height = 250;
            dgvCollections.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            dgvCollections.CellFormatting += DgvCollections_CellFormatting;
            dgvCollections.SelectionChanged += DgvCollections_SelectionChanged;
            WrapInCard(dgvCollections, pnlContent, new Point(0, y), 250);
            FixDataGridView(dgvCollections);

            Panel bottomSpacer = new Panel { Height = 30, BackColor = Color.Transparent, Dock = DockStyle.Bottom };
            pnlContent.Controls.Add(bottomSpacer);

            // Resize event
            pnlContent.Resize += (s, e) =>
            {
                if (lblStatus != null)
                    lblStatus.Location = new Point(pnlContent.Width - 240, 20);
            };
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

                    // Check if BloodBags table exists
                    string checkTable = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'BloodBags'";
                    using (var checkCmd = new SqlCommand(checkTable, conn))
                    {
                        int tableExists = Convert.ToInt32(await checkCmd.ExecuteScalarAsync());
                        if (tableExists == 0)
                        {
                            txtBagID.Text = $"BB-{DateTime.Now:yyyyMMdd}-001";
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
        // AUTO FILL DONOR DETAILS FROM DONOR ID
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
                                if (!string.IsNullOrEmpty(bloodGroup))
                                {
                                    int index = cmbBloodGroup.Items.IndexOf(bloodGroup);
                                    if (index >= 0)
                                        cmbBloodGroup.SelectedIndex = index;
                                }
                            }
                            else
                            {
                                txtDonorName.Text = "";
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"TxtDonorID_TextChanged Error: {ex.Message}");
                txtDonorName.Text = "";
            }
        }

        // =========================================================
        // CHECK IF BAG ALREADY EXISTS
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
                            MessageBox.Show($"Bag ID '{txtBagID.Text}' already exists in the system.\n\nPlease use a unique Bag ID.",
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
        // LOAD COLLECTION HISTORY FROM DATABASE
        // =========================================================
        private async void LoadCollectionHistory()
        {
            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    await conn.OpenAsync();

                    // Check if BloodBags table exists
                    string checkTable = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'BloodBags'";
                    using (var checkCmd = new SqlCommand(checkTable, conn))
                    {
                        int tableExists = Convert.ToInt32(await checkCmd.ExecuteScalarAsync());
                        if (tableExists == 0)
                        {
                            dgvCollections.DataSource = null;
                            UpdateStatus("⚠️ Table 'BloodBags' not found", Color.FromArgb(245, 158, 11));
                            return;
                        }
                    }

                    string query = @"
                        SELECT 
                            BagID as [Bag ID],
                            DonorID as [Donor ID],
                            DonorName as [Donor Name],
                            BloodGroup as [Blood Group],
                            Volume as [Volume (mL)],
                            ComponentType as [Collection Method],
                            FORMAT(CollectionDate, 'dd-MMM-yyyy') as [Collection Date],
                            Status,
                            FORMAT(CreatedAt, 'dd-MMM-yyyy HH:mm') as [Recorded At]
                        FROM BloodBags 
                        WHERE ComponentType IN ('Whole Blood', 'Apheresis', 'Double Red Cell')
                        ORDER BY CollectionDate DESC, BagID DESC";

                    SqlDataAdapter da = new SqlDataAdapter(query, conn);
                    collectionData = new DataTable();
                    da.Fill(collectionData);

                    dgvCollections.DataSource = collectionData;
                    FormatHistoryGrid();
                    NoSort(dgvCollections);

                    if (collectionData.Rows.Count == 0)
                        UpdateStatus("✅ No collection records found. Add new record using the form above.", Color.FromArgb(16, 185, 129));
                    else
                        UpdateStatus($"✅ Loaded {collectionData.Rows.Count} collection record(s) from database", Color.FromArgb(16, 185, 129));
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LoadCollectionHistory Error: {ex.Message}");
                dgvCollections.DataSource = null;
                UpdateStatus($"❌ Database error: {ex.Message}", Color.FromArgb(220, 38, 38));
            }
        }

        private void FormatHistoryGrid()
        {
            if (dgvCollections.Columns.Count == 0) return;

            // Set column widths
            if (dgvCollections.Columns.Contains("Bag ID"))
                dgvCollections.Columns["Bag ID"].FillWeight = 12;
            if (dgvCollections.Columns.Contains("Donor ID"))
                dgvCollections.Columns["Donor ID"].FillWeight = 10;
            if (dgvCollections.Columns.Contains("Donor Name"))
                dgvCollections.Columns["Donor Name"].FillWeight = 15;
            if (dgvCollections.Columns.Contains("Blood Group"))
                dgvCollections.Columns["Blood Group"].FillWeight = 8;
            if (dgvCollections.Columns.Contains("Volume (mL)"))
                dgvCollections.Columns["Volume (mL)"].FillWeight = 8;
            if (dgvCollections.Columns.Contains("Collection Method"))
                dgvCollections.Columns["Collection Method"].FillWeight = 12;
            if (dgvCollections.Columns.Contains("Collection Date"))
                dgvCollections.Columns["Collection Date"].FillWeight = 10;
            if (dgvCollections.Columns.Contains("Status"))
                dgvCollections.Columns["Status"].FillWeight = 10;
            if (dgvCollections.Columns.Contains("Recorded At"))
                dgvCollections.Columns["Recorded At"].FillWeight = 15;
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
        // LOAD SELECTED RECORD FOR EDITING
        // =========================================================
        private void DgvCollections_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvCollections.SelectedRows.Count > 0 && dgvCollections.SelectedRows[0].DataBoundItem != null)
            {
                DataRowView row = dgvCollections.SelectedRows[0].DataBoundItem as DataRowView;
                if (row != null)
                {
                    isEditMode = true;
                    btnSave.Text = "✏️ Update Collection";

                    txtBagID.Text = row["Bag ID"]?.ToString();
                    txtDonorID.Text = row["Donor ID"]?.ToString();
                    txtDonorName.Text = row["Donor Name"]?.ToString();
                    txtVolume.Text = row["Volume (mL)"]?.ToString();

                    SetComboValue(cmbBloodGroup, row["Blood Group"]?.ToString());
                    SetComboValue(cmbCollectionMethod, row["Collection Method"]?.ToString());
                    SetComboValue(cmbStatus, row["Status"]?.ToString());

                    if (DateTime.TryParse(row["Collection Date"]?.ToString(), out DateTime collectionDate))
                        dtpCollectionDate.Value = collectionDate;

                    currentBagID = txtBagID.Text;
                    UpdateStatus("✏️ Editing existing record", Color.FromArgb(59, 130, 246));
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
        // SAVE COLLECTION RECORD (INSERT OR UPDATE)
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

            if (!int.TryParse(txtVolume.Text, out int volume) || volume <= 0)
            {
                MessageBox.Show("Please enter a valid volume (mL).", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    await conn.OpenAsync();

                    if (isEditMode)
                    {
                        // UPDATE existing record
                        string sql = @"
                            UPDATE BloodBags SET 
                                DonorID = @DonorID,
                                DonorName = @DonorName,
                                BloodGroup = @BloodGroup,
                                Volume = @Volume,
                                ComponentType = @ComponentType,
                                CollectionDate = @CollectionDate,
                                Status = @Status,
                                UpdatedAt = GETDATE()
                            WHERE BagID = @BagID";

                        using (var cmd = new SqlCommand(sql, conn))
                        {
                            cmd.Parameters.AddWithValue("@BagID", txtBagID.Text);
                            cmd.Parameters.AddWithValue("@DonorID", txtDonorID.Text);
                            cmd.Parameters.AddWithValue("@DonorName", txtDonorName.Text);
                            cmd.Parameters.AddWithValue("@BloodGroup", cmbBloodGroup.SelectedItem?.ToString() ?? "");
                            cmd.Parameters.AddWithValue("@Volume", volume);
                            cmd.Parameters.AddWithValue("@ComponentType", cmbCollectionMethod.SelectedItem?.ToString() ?? "Whole Blood");
                            cmd.Parameters.AddWithValue("@CollectionDate", dtpCollectionDate.Value);
                            cmd.Parameters.AddWithValue("@Status", cmbStatus.SelectedItem?.ToString() ?? "Collected");

                            await cmd.ExecuteNonQueryAsync();
                        }

                        MessageBox.Show($"Blood collection record for Bag {txtBagID.Text} updated successfully!",
                            "Update Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        // INSERT new record
                        // Calculate expiry date (35 days from collection)
                        DateTime expiryDate = dtpCollectionDate.Value.AddDays(35);

                        string sql = @"
                            INSERT INTO BloodBags 
                            (BagID, DonorID, DonorName, BloodGroup, Volume, ComponentType, 
                             CollectionDate, ExpiryDate, Status, Quantity, CreatedAt, CreatedBy)
                            VALUES 
                            (@BagID, @DonorID, @DonorName, @BloodGroup, @Volume, @ComponentType,
                             @CollectionDate, @ExpiryDate, @Status, 1, GETDATE(), @CreatedBy)";

                        using (var cmd = new SqlCommand(sql, conn))
                        {
                            cmd.Parameters.AddWithValue("@BagID", txtBagID.Text);
                            cmd.Parameters.AddWithValue("@DonorID", txtDonorID.Text);
                            cmd.Parameters.AddWithValue("@DonorName", txtDonorName.Text);
                            cmd.Parameters.AddWithValue("@BloodGroup", cmbBloodGroup.SelectedItem?.ToString() ?? "");
                            cmd.Parameters.AddWithValue("@Volume", volume);
                            cmd.Parameters.AddWithValue("@ComponentType", cmbCollectionMethod.SelectedItem?.ToString() ?? "Whole Blood");
                            cmd.Parameters.AddWithValue("@CollectionDate", dtpCollectionDate.Value);
                            cmd.Parameters.AddWithValue("@ExpiryDate", expiryDate);
                            cmd.Parameters.AddWithValue("@Status", cmbStatus.SelectedItem?.ToString() ?? "Collected");
                            cmd.Parameters.AddWithValue("@CreatedBy", SessionManager.CurrentUserID);

                            await cmd.ExecuteNonQueryAsync();
                        }

                        MessageBox.Show($"Blood collection record for Bag {txtBagID.Text} saved successfully!\n\nExpiry Date: {expiryDate:dd-MMM-yyyy}",
                            "Save Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        // Update donor's donation count
                        await UpdateDonorDonationCountAsync(txtDonorID.Text);
                    }
                }

                // Refresh the grid from database
                LoadCollectionHistory();
                ClearForm();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"BtnSave_Click Error: {ex.Message}");
                MessageBox.Show($"Error saving to database: {ex.Message}",
                    "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                UpdateStatus($"❌ Save failed: {ex.Message}", Color.FromArgb(220, 38, 38));
            }
        }

        // =========================================================
        // UPDATE DONOR'S DONATION COUNT
        // =========================================================
        private async Task UpdateDonorDonationCountAsync(string donorId)
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
                        cmd.Parameters.AddWithValue("@DonorID", donorId);
                        cmd.Parameters.AddWithValue("@DonationDate", dtpCollectionDate.Value);
                        await cmd.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"UpdateDonorDonationCountAsync Error: {ex.Message}");
            }
        }

        // =========================================================
        // DELETE COLLECTION RECORD
        // =========================================================
        private async void BtnDelete_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtBagID.Text) || !isEditMode)
            {
                MessageBox.Show("Please select a record to delete from the list first.", "No Selection",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DialogResult result = MessageBox.Show($"Are you sure you want to delete record for Bag {txtBagID.Text}?\n\nThis action cannot be undone.",
                "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (result != DialogResult.Yes) return;

            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    await conn.OpenAsync();

                    // Check if bag exists
                    string checkSql = "SELECT COUNT(*) FROM BloodBags WHERE BagID = @BagID";
                    using (var checkCmd = new SqlCommand(checkSql, conn))
                    {
                        checkCmd.Parameters.AddWithValue("@BagID", txtBagID.Text);
                        int exists = Convert.ToInt32(await checkCmd.ExecuteScalarAsync());

                        if (exists == 0)
                        {
                            MessageBox.Show($"Bag ID '{txtBagID.Text}' not found in database.",
                                "Not Found", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            LoadCollectionHistory();
                            ClearForm();
                            return;
                        }
                    }

                    string sql = "DELETE FROM BloodBags WHERE BagID = @BagID";
                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@BagID", txtBagID.Text);
                        int rowsAffected = await cmd.ExecuteNonQueryAsync();

                        if (rowsAffected > 0)
                        {
                            MessageBox.Show($"Record for Bag {txtBagID.Text} deleted successfully.",
                                "Deleted", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            LoadCollectionHistory();
                            ClearForm();
                            UpdateStatus("🗑️ Record deleted", Color.FromArgb(16, 185, 129));
                        }
                        else
                        {
                            MessageBox.Show("Record not found or could not be deleted.",
                                "Delete Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"BtnDelete_Click Error: {ex.Message}");
                MessageBox.Show($"Error deleting record: {ex.Message}",
                    "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // =========================================================
        // CLEAR FORM
        // =========================================================
        private void ClearForm()
        {
            isEditMode = false;
            btnSave.Text = "💾 Save Collection";

            txtDonorID.Clear();
            txtDonorName.Clear();
            txtVolume.Text = "450";

            cmbBloodGroup.SelectedIndex = 0;
            cmbCollectionMethod.SelectedIndex = 0;
            cmbStatus.SelectedIndex = 0;

            dtpCollectionDate.Value = DateTime.Now;

            GenerateBagID();
            UpdateStatus("✅ Ready - Fill form to add new collection record", Color.FromArgb(16, 185, 129));
        }

        // =========================================================
        // CELL FORMATTING FOR GRID
        // =========================================================
        private void DgvCollections_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.Value != null && dgvCollections.Columns[e.ColumnIndex].HeaderText == "Status")
            {
                string status = e.Value.ToString();
                if (status == "Ready")
                    e.CellStyle.ForeColor = Color.FromArgb(16, 185, 129);
                else if (status == "Collected")
                    e.CellStyle.ForeColor = Color.FromArgb(59, 130, 246);
                else if (status == "In Lab")
                    e.CellStyle.ForeColor = Color.FromArgb(245, 158, 11);
                else if (status == "Screening Pending")
                    e.CellStyle.ForeColor = Color.FromArgb(239, 68, 68);
            }
        }

        // =========================================================
        // HELPER METHODS FOR UI
        // =========================================================
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