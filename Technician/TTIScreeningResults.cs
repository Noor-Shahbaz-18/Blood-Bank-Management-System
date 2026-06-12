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
    public partial class TTIScreeningResults : BaseTechnicianForm
    {
        private Panel pnlFormCard;
        private DataGridView dgvScreeningResults;
        private TextBox txtTestID, txtBagID, txtDonorID, txtDonorName, txtRemarks;
        private ComboBox cmbHIV, cmbHepatitisB, cmbHepatitisC, cmbSyphilis, cmbMalaria, cmbOverallResult;
        private DateTimePicker dtpTestDate;
        private Button btnSave, btnClear, btnRefresh, btnDelete;
        private DataTable originalData;
        private bool isEditMode = false;
        private Label lblStatus;

        public TTIScreeningResults()
        {
            this.Text = "Blood Bank Management System – TTI Screening Results";
            BuildLayout();
            BuildSidebar("TTI Screening");
            BuildTopBar("TTI Screening Results");
            BuildContentArea();
            LoadScreeningHistory();
            GenerateTestID();
        }

        private void BuildContentArea()
        {
            int y = 20;

            // Status label at top right
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

            // Form Card
            pnlFormCard = new Panel
            {
                Location = new Point(0, y + 30),
                Width = pnlContent.ClientSize.Width - 40,
                Height = 480,
                BackColor = C_CardBg,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            SetRoundedRegion(pnlFormCard, 15);
            AddDropShadow(pnlFormCard);
            pnlContent.Controls.Add(pnlFormCard);

            pnlFormCard.Controls.Add(new Label
            {
                Text = "🧪 TTI Screening Test Results",
                Font = new Font("Segoe UI", 14f, FontStyle.Bold),
                ForeColor = C_TextDark,
                Location = new Point(25, 18),
                AutoSize = true
            });

            int leftX = 25, rightX = 430, fieldY = 68, spacingY = 42;

            // =========================================================
            // LEFT COLUMN
            // =========================================================

            // Test ID (read-only)
            AddLabel("Test ID:", leftX, fieldY);
            txtTestID = AddTextBox(leftX + 110, fieldY, 190);
            txtTestID.ReadOnly = true;
            txtTestID.BackColor = Color.FromArgb(245, 245, 250);

            // Bag ID
            AddLabel("Bag ID:", leftX, fieldY + spacingY);
            txtBagID = AddTextBox(leftX + 110, fieldY + spacingY, 190);
            txtBagID.TextChanged += TxtBagID_TextChanged;

            // Donor ID
            AddLabel("Donor ID:", leftX, fieldY + spacingY * 2);
            txtDonorID = AddTextBox(leftX + 110, fieldY + spacingY * 2, 190);
            txtDonorID.TextChanged += TxtDonorID_TextChanged;

            // Donor Name (read-only, auto-filled)
            AddLabel("Donor Name:", leftX, fieldY + spacingY * 3);
            txtDonorName = AddTextBox(leftX + 110, fieldY + spacingY * 3, 190);
            txtDonorName.ReadOnly = true;
            txtDonorName.BackColor = Color.FromArgb(245, 245, 250);

            // Malaria
            AddLabel("Malaria:", leftX, fieldY + spacingY * 4);
            cmbMalaria = AddComboBox(leftX + 110, fieldY + spacingY * 4, 170,
                new[] { "Negative", "Positive", "Pending" });
            cmbMalaria.SelectedIndexChanged += (s, e) => UpdateOverallResult();

            // Test Date
            AddLabel("Test Date:", leftX, fieldY + spacingY * 5);
            dtpTestDate = new DateTimePicker
            {
                Location = new Point(leftX + 110, fieldY + spacingY * 5 - 2),
                Width = 160,
                Format = DateTimePickerFormat.Short,
                Value = DateTime.Now,
                Font = new Font("Segoe UI", 10)
            };
            pnlFormCard.Controls.Add(dtpTestDate);

            // =========================================================
            // RIGHT COLUMN
            // =========================================================

            // HIV
            AddLabel("HIV 1/2:", rightX, fieldY);
            cmbHIV = AddComboBox(rightX + 175, fieldY, 185,
                new[] { "Non-Reactive", "Reactive", "Indeterminate", "Pending" });
            cmbHIV.SelectedIndexChanged += (s, e) => UpdateOverallResult();

            // Hepatitis B
            AddLabel("Hepatitis B (HBsAg):", rightX, fieldY + spacingY);
            cmbHepatitisB = AddComboBox(rightX + 175, fieldY + spacingY, 185,
                new[] { "Non-Reactive", "Reactive", "Indeterminate", "Pending" });
            cmbHepatitisB.SelectedIndexChanged += (s, e) => UpdateOverallResult();

            // Hepatitis C
            AddLabel("Hepatitis C:", rightX, fieldY + spacingY * 2);
            cmbHepatitisC = AddComboBox(rightX + 175, fieldY + spacingY * 2, 185,
                new[] { "Non-Reactive", "Reactive", "Indeterminate", "Pending" });
            cmbHepatitisC.SelectedIndexChanged += (s, e) => UpdateOverallResult();

            // Syphilis
            AddLabel("Syphilis:", rightX, fieldY + spacingY * 3);
            cmbSyphilis = AddComboBox(rightX + 175, fieldY + spacingY * 3, 185,
                new[] { "Non-Reactive", "Reactive", "Indeterminate", "Pending" });
            cmbSyphilis.SelectedIndexChanged += (s, e) => UpdateOverallResult();

            // Overall Result
            AddLabel("Overall Result:", rightX, fieldY + spacingY * 4);
            cmbOverallResult = AddComboBox(rightX + 175, fieldY + spacingY * 4, 185,
                new[] { "Non-Reactive", "Reactive", "Indeterminate", "Requires Retesting" });

            // =========================================================
            // REMARKS (full width)
            // =========================================================
            int remarksY = fieldY + spacingY * 5 + 38;
            AddLabel("Remarks:", leftX, remarksY + 3);
            txtRemarks = new TextBox
            {
                Location = new Point(leftX + 110, remarksY),
                Width = pnlFormCard.Width - leftX - 110 - 30,
                Height = 55,
                Multiline = true,
                Font = new Font("Segoe UI", 10),
                BorderStyle = BorderStyle.FixedSingle,
                MaxLength = 500,
                Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top
            };
            pnlFormCard.Controls.Add(txtRemarks);

            // =========================================================
            // BUTTONS ROW
            // =========================================================
            int btnY = remarksY + 68;

            btnSave = new Button
            {
                Text = "💾 Save Screening",
                Location = new Point(leftX, btnY),
                Size = new Size(155, 38),
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
                Location = new Point(leftX + 165, btnY),
                Size = new Size(120, 38),
                BackColor = Color.FromArgb(100, 110, 125),
                ForeColor = C_White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10),
                Cursor = Cursors.Hand
            };
            btnClear.FlatAppearance.BorderSize = 0;
            SetRoundedRegion(btnClear, 8);
            btnClear.Click += (s, e) => ClearForm();
            pnlFormCard.Controls.Add(btnClear);

            btnDelete = new Button
            {
                Text = "🗑️ Delete Record",
                Location = new Point(leftX + 298, btnY),
                Size = new Size(135, 38),
                BackColor = Color.FromArgb(220, 38, 38),
                ForeColor = C_White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10),
                Cursor = Cursors.Hand
            };
            btnDelete.FlatAppearance.BorderSize = 0;
            SetRoundedRegion(btnDelete, 8);
            btnDelete.Click += BtnDelete_Click;
            pnlFormCard.Controls.Add(btnDelete);

            btnRefresh = new Button
            {
                Text = "🔄 Refresh List",
                Location = new Point(leftX + 448, btnY),
                Size = new Size(130, 38),
                BackColor = Color.FromArgb(60, 70, 80),
                ForeColor = C_White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10),
                Cursor = Cursors.Hand
            };
            btnRefresh.FlatAppearance.BorderSize = 0;
            SetRoundedRegion(btnRefresh, 8);
            btnRefresh.Click += (s, e) => LoadScreeningHistory();
            pnlFormCard.Controls.Add(btnRefresh);

            y += pnlFormCard.Height + 28;

            // =========================================================
            // HISTORY SECTION HEADER
            // =========================================================
            Panel historyHeader = new Panel
            {
                Location = new Point(0, y),
                Width = pnlContent.ClientSize.Width - 40,
                Height = 32,
                BackColor = Color.Transparent
            };
            pnlContent.Controls.Add(historyHeader);

            historyHeader.Controls.Add(new Label
            {
                Text = "📋 TTI Screening History",
                Font = new Font("Segoe UI", 13f, FontStyle.Bold),
                ForeColor = C_TextDark,
                Location = new Point(0, 4),
                AutoSize = true
            });
            y += 32;

            // =========================================================
            // HISTORY GRID
            // =========================================================
            dgvScreeningResults = CreateStyledGrid();
            dgvScreeningResults.CellFormatting += DgvScreeningResults_CellFormatting;
            dgvScreeningResults.SelectionChanged += DgvScreeningResults_SelectionChanged;
            WrapInCard(dgvScreeningResults, pnlContent, new Point(0, y), 260);
            FixDataGridView(dgvScreeningResults);

            pnlContent.Controls.Add(new Panel
            {
                Height = 30,
                BackColor = Color.Transparent,
                Dock = DockStyle.Bottom
            });

            pnlContent.Resize += (s, e) =>
            {
                if (lblStatus != null)
                    lblStatus.Location = new Point(pnlContent.Width - 220, 20);
            };
        }

        // =========================================================
        // TINY HELPERS  (keep BuildContentArea readable)
        // =========================================================
        private void AddLabel(string text, int x, int y)
        {
            pnlFormCard.Controls.Add(new Label
            {
                Text = text,
                Font = new Font("Segoe UI", 10),
                ForeColor = C_TextMid,
                Location = new Point(x, y),
                AutoSize = true
            });
        }

        private TextBox AddTextBox(int x, int y, int width)
        {
            var txt = new TextBox
            {
                Location = new Point(x, y - 2),
                Width = width,
                Font = new Font("Segoe UI", 10),
                BorderStyle = BorderStyle.FixedSingle
            };
            pnlFormCard.Controls.Add(txt);
            return txt;
        }

        private ComboBox AddComboBox(int x, int y, int width, string[] items)
        {
            var cmb = new ComboBox
            {
                Location = new Point(x, y - 2),
                Width = width,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10)
            };
            cmb.Items.AddRange(items);
            if (items.Length > 0) cmb.SelectedIndex = 0;
            pnlFormCard.Controls.Add(cmb);
            return cmb;
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

                    string checkTable = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'TTIScreeningResults'";
                    using (var checkCmd = new SqlCommand(checkTable, conn))
                    {
                        int tableExists = Convert.ToInt32(await checkCmd.ExecuteScalarAsync());
                        if (tableExists == 0)
                        {
                            txtTestID.Text = $"TTI-{DateTime.Now:yyyyMMdd}-001";
                            return;
                        }
                    }

                    string sql = "SELECT COUNT(*) FROM TTIScreeningResults";
                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        int count = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                        txtTestID.Text = $"TTI-{DateTime.Now:yyyyMMdd}-{(count + 1):D3}";
                    }
                }
            }
            catch
            {
                txtTestID.Text = $"TTI-{DateTime.Now:yyyyMMdd}-001";
            }
        }

        // =========================================================
        // AUTO-FILL DONOR DETAILS FROM BAG ID
        // =========================================================
        private async void TxtBagID_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtBagID.Text)) return;

            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    await conn.OpenAsync();

                    string checkTable = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'BloodBags'";
                    using (var checkCmd = new SqlCommand(checkTable, conn))
                    {
                        int tableExists = Convert.ToInt32(await checkCmd.ExecuteScalarAsync());
                        if (tableExists == 0) { txtDonorID.Text = ""; txtDonorName.Text = ""; return; }
                    }

                    string sql = @"
                        SELECT ISNULL(DonorID,   'N/A') AS DonorID,
                               ISNULL(DonorName, 'N/A') AS DonorName
                        FROM   BloodBags
                        WHERE  BagID = @BagID";

                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@BagID", txtBagID.Text.Trim());
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                txtDonorID.Text = reader["DonorID"].ToString();
                                txtDonorName.Text = reader["DonorName"].ToString();
                            }
                            else
                            {
                                txtDonorID.Text = "";
                                txtDonorName.Text = "";
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"TxtBagID_TextChanged Error: {ex.Message}");
                txtDonorID.Text = ""; txtDonorName.Text = "";
            }
        }

        // =========================================================
        // AUTO-FILL DONOR NAME FROM DONOR ID
        // =========================================================
        private async void TxtDonorID_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtDonorID.Text)) return;

            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    await conn.OpenAsync();

                    string checkTable = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Donors'";
                    using (var checkCmd = new SqlCommand(checkTable, conn))
                    {
                        int tableExists = Convert.ToInt32(await checkCmd.ExecuteScalarAsync());
                        if (tableExists == 0) { txtDonorName.Text = ""; return; }
                    }

                    string sql = "SELECT FullName FROM Donors WHERE DonorID = @DonorID OR UserID = @DonorID";
                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@DonorID", txtDonorID.Text.Trim());
                        var result = await cmd.ExecuteScalarAsync();
                        txtDonorName.Text = result != null ? result.ToString() : "";
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
        // AUTO CALCULATE OVERALL RESULT
        // =========================================================
        private void UpdateOverallResult()
        {
            string[] results =
            {
                cmbHIV.SelectedItem?.ToString(),
                cmbHepatitisB.SelectedItem?.ToString(),
                cmbHepatitisC.SelectedItem?.ToString(),
                cmbSyphilis.SelectedItem?.ToString(),
                cmbMalaria.SelectedItem?.ToString()
            };

            if (Array.Exists(results, r => r == "Reactive" || r == "Positive"))
                cmbOverallResult.SelectedItem = "Reactive";
            else if (Array.Exists(results, r => r == "Indeterminate"))
                cmbOverallResult.SelectedItem = "Indeterminate";
            else if (Array.Exists(results, r => r == "Pending"))
                cmbOverallResult.SelectedItem = "Requires Retesting";
            else
                cmbOverallResult.SelectedItem = "Non-Reactive";
        }

        // =========================================================
        // LOAD SCREENING HISTORY FROM DATABASE
        // =========================================================
        private async void LoadScreeningHistory()
        {
            try
            {
                UpdateStatus("🔄 Loading records...", Color.FromArgb(59, 130, 246));

                using (var conn = DatabaseHelper.GetConnection())
                {
                    await conn.OpenAsync();

                    string checkTable = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'TTIScreeningResults'";
                    using (var checkCmd = new SqlCommand(checkTable, conn))
                    {
                        int tableExists = Convert.ToInt32(await checkCmd.ExecuteScalarAsync());
                        if (tableExists == 0)
                        {
                            dgvScreeningResults.DataSource = null;
                            UpdateStatus("⚠️ Table 'TTIScreeningResults' not found. Run the SQL fix script first.",
                                         Color.FromArgb(245, 158, 11));
                            return;
                        }
                    }

                    // Build column list dynamically based on what exists
                    // This avoids "Invalid column name" errors on older schemas
                    string query = BuildSafeSelectQuery(conn);

                    SqlDataAdapter da = new SqlDataAdapter(query, conn);
                    originalData = new DataTable();
                    da.Fill(originalData);

                    dgvScreeningResults.DataSource = originalData;
                    FormatHistoryGrid();
                    NoSort(dgvScreeningResults);

                    UpdateStatus(originalData.Rows.Count == 0
                        ? "✅ No records yet. Fill the form above to add one."
                        : $"✅ Loaded {originalData.Rows.Count} record(s)",
                        Color.FromArgb(16, 185, 129));
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LoadScreeningHistory Error: {ex.Message}");
                dgvScreeningResults.DataSource = null;
                UpdateStatus($"❌ {ex.Message}", Color.FromArgb(220, 38, 38));
            }
        }

        // Builds a SELECT that only references columns that actually exist
        private string BuildSafeSelectQuery(SqlConnection conn)
        {
            // Get actual column names from DB
            var existingCols = new System.Collections.Generic.HashSet<string>(StringComparer.OrdinalIgnoreCase);
            using (var cmd = new SqlCommand(
                "SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'TTIScreeningResults'", conn))
            using (var reader = cmd.ExecuteReader())
                while (reader.Read())
                    existingCols.Add(reader.GetString(0));

            string Col(string name, string alias = null)
            {
                if (!existingCols.Contains(name)) return $"NULL AS {alias ?? name}";
                return alias != null ? $"{name} AS {alias}" : name;
            }

            return $@"
                SELECT
                    {Col("ScreeningID")},
                    {Col("TestID")},
                    {Col("BagID")},
                    {Col("DonorID")},
                    {Col("DonorName")},
                    {Col("HIV")},
                    {Col("HepatitisB")},
                    {Col("HepatitisC")},
                    {Col("Syphilis")},
                    {Col("Malaria")},
                    {Col("OverallResult")},
                    {(existingCols.Contains("TestDate")
                        ? "FORMAT(TestDate, 'dd-MMM-yyyy') AS TestDate"
                        : "NULL AS TestDate")},
                    {Col("Remarks")},
                    {(existingCols.Contains("CreatedAt")
                        ? "FORMAT(CreatedAt, 'dd-MMM-yyyy HH:mm') AS CreatedAt"
                        : "NULL AS CreatedAt")}
                FROM TTIScreeningResults
                ORDER BY {(existingCols.Contains("TestDate") ? "TestDate DESC," : "")} ScreeningID DESC";
        }

        private void FormatHistoryGrid()
        {
            if (dgvScreeningResults.Columns.Count == 0) return;

            void Fmt(string col, string header, int weight, bool center = false)
            {
                if (!dgvScreeningResults.Columns.Contains(col)) return;
                dgvScreeningResults.Columns[col].HeaderText = header;
                dgvScreeningResults.Columns[col].FillWeight = weight;
                dgvScreeningResults.Columns[col].MinimumWidth = 55;
                if (center)
                    dgvScreeningResults.Columns[col].DefaultCellStyle.Alignment =
                        DataGridViewContentAlignment.MiddleCenter;
            }

            if (dgvScreeningResults.Columns.Contains("ScreeningID"))
                dgvScreeningResults.Columns["ScreeningID"].Visible = false;

            Fmt("TestID", "Test ID", 11);
            Fmt("BagID", "Bag ID", 9);
            Fmt("DonorID", "Donor ID", 9);
            Fmt("DonorName", "Donor Name", 14);
            Fmt("HIV", "HIV", 8, true);
            Fmt("HepatitisB", "Hep B", 8, true);
            Fmt("HepatitisC", "Hep C", 8, true);
            Fmt("Syphilis", "Syphilis", 8, true);
            Fmt("Malaria", "Malaria", 8, true);
            Fmt("OverallResult", "Result", 10);
            Fmt("TestDate", "Test Date", 10);
            Fmt("Remarks", "Remarks", 10);
            Fmt("CreatedAt", "Created", 10);
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
        // LOAD SELECTED ROW INTO FORM FOR EDITING
        // =========================================================
        private void DgvScreeningResults_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvScreeningResults.SelectedRows.Count == 0) return;
            var row = dgvScreeningResults.SelectedRows[0].DataBoundItem as DataRowView;
            if (row == null) return;

            isEditMode = true;
            btnSave.Text = "✏️ Update Result";

            txtTestID.Text = row["TestID"]?.ToString();
            txtBagID.Text = row["BagID"]?.ToString();
            txtDonorID.Text = row["DonorID"]?.ToString();
            txtDonorName.Text = row["DonorName"]?.ToString();
            txtRemarks.Text = row["Remarks"]?.ToString();

            SetCombo(cmbHIV, row["HIV"]?.ToString());
            SetCombo(cmbHepatitisB, row["HepatitisB"]?.ToString());
            SetCombo(cmbHepatitisC, row["HepatitisC"]?.ToString());
            SetCombo(cmbSyphilis, row["Syphilis"]?.ToString());
            SetCombo(cmbMalaria, row["Malaria"]?.ToString());
            SetCombo(cmbOverallResult, row["OverallResult"]?.ToString());

            if (DateTime.TryParse(row["TestDate"]?.ToString(), out DateTime d))
                dtpTestDate.Value = d;

            UpdateStatus("✏️ Editing record – click Update to save changes",
                         Color.FromArgb(59, 130, 246));
        }

        private void SetCombo(ComboBox cmb, string value)
        {
            if (string.IsNullOrEmpty(value)) return;
            int idx = cmb.Items.IndexOf(value);
            if (idx >= 0) cmb.SelectedIndex = idx;
        }

        // =========================================================
        // SAVE (INSERT OR UPDATE)
        // =========================================================
        private async void BtnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtBagID.Text))
            {
                MessageBox.Show("Bag ID is required.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            bool bagExists = await CheckBagExistsAsync(txtBagID.Text.Trim());
            if (!bagExists)
            {
                MessageBox.Show($"Bag ID '{txtBagID.Text}' does not exist.\n\nPlease enter a valid Bag ID.",
                    "Invalid Bag ID", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                UpdateStatus("💾 Saving...", Color.FromArgb(59, 130, 246));

                using (var conn = DatabaseHelper.GetConnection())
                {
                    await conn.OpenAsync();

                    string sql = isEditMode
                        ? @"UPDATE TTIScreeningResults SET
                                BagID         = @BagID,
                                DonorID       = @DonorID,
                                DonorName     = @DonorName,
                                HIV           = @HIV,
                                HepatitisB    = @HepatitisB,
                                HepatitisC    = @HepatitisC,
                                Syphilis      = @Syphilis,
                                Malaria       = @Malaria,
                                OverallResult = @OverallResult,
                                TestDate      = @TestDate,
                                Remarks       = @Remarks,
                                UpdatedAt     = GETDATE()
                            WHERE TestID = @TestID"
                        : @"INSERT INTO TTIScreeningResults
                            (TestID, BagID, DonorID, DonorName,
                             HIV, HepatitisB, HepatitisC, Syphilis, Malaria,
                             OverallResult, TestDate, Remarks, CreatedAt, CreatedBy)
                            VALUES
                            (@TestID, @BagID, @DonorID, @DonorName,
                             @HIV, @HepatitisB, @HepatitisC, @Syphilis, @Malaria,
                             @OverallResult, @TestDate, @Remarks, GETDATE(), @CreatedBy)";

                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@TestID", txtTestID.Text);
                        cmd.Parameters.AddWithValue("@BagID", txtBagID.Text.Trim());
                        cmd.Parameters.AddWithValue("@DonorID", txtDonorID.Text ?? "");
                        cmd.Parameters.AddWithValue("@DonorName", txtDonorName.Text ?? "");
                        cmd.Parameters.AddWithValue("@HIV", cmbHIV.SelectedItem?.ToString() ?? "Pending");
                        cmd.Parameters.AddWithValue("@HepatitisB", cmbHepatitisB.SelectedItem?.ToString() ?? "Pending");
                        cmd.Parameters.AddWithValue("@HepatitisC", cmbHepatitisC.SelectedItem?.ToString() ?? "Pending");
                        cmd.Parameters.AddWithValue("@Syphilis", cmbSyphilis.SelectedItem?.ToString() ?? "Pending");
                        cmd.Parameters.AddWithValue("@Malaria", cmbMalaria.SelectedItem?.ToString() ?? "Pending");
                        cmd.Parameters.AddWithValue("@OverallResult", cmbOverallResult.SelectedItem?.ToString() ?? "Pending");
                        cmd.Parameters.AddWithValue("@TestDate", dtpTestDate.Value);
                        cmd.Parameters.AddWithValue("@Remarks", txtRemarks.Text ?? "");
                        if (!isEditMode)
                            cmd.Parameters.AddWithValue("@CreatedBy", SessionManager.CurrentUserID);

                        await cmd.ExecuteNonQueryAsync();
                    }
                }

                MessageBox.Show(
                    $"Record {txtTestID.Text} {(isEditMode ? "updated" : "saved")} successfully!",
                    "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                LoadScreeningHistory();
                ClearForm();

                if (cmbOverallResult.SelectedItem?.ToString() == "Reactive")
                    await UpdateBloodBagStatusAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"BtnSave_Click Error: {ex.Message}");
                MessageBox.Show($"Error saving record:\n{ex.Message}",
                    "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                UpdateStatus($"❌ Save failed: {ex.Message}", Color.FromArgb(220, 38, 38));
            }
        }

        // =========================================================
        // DELETE
        // =========================================================
        private async void BtnDelete_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtTestID.Text))
            {
                MessageBox.Show("Please select a record from the grid to delete.",
                    "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (MessageBox.Show(
                $"Delete record {txtTestID.Text}?\n\nThis cannot be undone.",
                "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                return;

            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    await conn.OpenAsync();
                    using (var cmd = new SqlCommand(
                        "DELETE FROM TTIScreeningResults WHERE TestID = @TestID", conn))
                    {
                        cmd.Parameters.AddWithValue("@TestID", txtTestID.Text);
                        int rows = await cmd.ExecuteNonQueryAsync();

                        if (rows > 0)
                        {
                            MessageBox.Show($"Record {txtTestID.Text} deleted.",
                                "Deleted", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            LoadScreeningHistory();
                            ClearForm();
                            UpdateStatus("🗑️ Record deleted", Color.FromArgb(16, 185, 129));
                        }
                        else
                        {
                            MessageBox.Show("Record not found.", "Delete Failed",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"BtnDelete_Click Error: {ex.Message}");
                MessageBox.Show($"Error deleting record:\n{ex.Message}",
                    "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // =========================================================
        // QUARANTINE BLOOD BAG IF REACTIVE
        // =========================================================
        private async Task UpdateBloodBagStatusAsync()
        {
            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    await conn.OpenAsync();
                    string sql = @"
                        UPDATE BloodBags
                        SET Status  = 'Quarantined',
                            Remarks = ISNULL(Remarks,'') + CHAR(10) +
                                      'TTI Reactive - Quarantined on ' + CONVERT(VARCHAR, GETDATE(), 106)
                        WHERE BagID = @BagID";

                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@BagID", txtBagID.Text.Trim());
                        await cmd.ExecuteNonQueryAsync();
                    }
                }

                MessageBox.Show(
                    "⚠️ Blood bag has been marked QUARANTINED due to reactive TTI results.\n\n" +
                    "This bag must NOT be used for transfusion.",
                    "Important", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"UpdateBloodBagStatusAsync Error: {ex.Message}");
            }
        }

        // =========================================================
        // CHECK BAG EXISTS
        // =========================================================
        private async Task<bool> CheckBagExistsAsync(string bagId)
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
                        if (tableExists == 0) return false;
                    }

                    string sql = "SELECT COUNT(*) FROM BloodBags WHERE BagID = @BagID";
                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@BagID", bagId);
                        return Convert.ToInt32(await cmd.ExecuteScalarAsync()) > 0;
                    }
                }
            }
            catch { return false; }
        }

        // =========================================================
        // CLEAR FORM
        // =========================================================
        private void ClearForm()
        {
            isEditMode = false;
            btnSave.Text = "💾 Save Screening";

            txtBagID.Clear();
            txtDonorID.Clear();
            txtDonorName.Clear();
            txtRemarks.Clear();

            cmbHIV.SelectedIndex = 0;
            cmbHepatitisB.SelectedIndex = 0;
            cmbHepatitisC.SelectedIndex = 0;
            cmbSyphilis.SelectedIndex = 0;
            cmbMalaria.SelectedIndex = 0;
            cmbOverallResult.SelectedIndex = 0;

            dtpTestDate.Value = DateTime.Now;

            GenerateTestID();
            UpdateStatus("✅ Ready – fill form to add new record", Color.FromArgb(16, 185, 129));
        }

        // =========================================================
        // GRID CELL FORMATTING (colour-code Overall Result)
        // =========================================================
        private void DgvScreeningResults_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.Value == null) return;
            if (dgvScreeningResults.Columns[e.ColumnIndex].HeaderText != "Result") return;

            switch (e.Value.ToString())
            {
                case "Reactive":
                    e.CellStyle.ForeColor = Color.FromArgb(220, 38, 38);
                    e.CellStyle.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
                    break;
                case "Non-Reactive":
                    e.CellStyle.ForeColor = Color.FromArgb(16, 185, 129);
                    break;
                case "Indeterminate":
                case "Requires Retesting":
                    e.CellStyle.ForeColor = Color.FromArgb(245, 158, 11);
                    break;
            }
        }

        // =========================================================
        // STYLED GRID
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
    }
}