using BloodBankManagementSystem.Classes.Database;
using BloodBankManagementSystem.Classes.Helpers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BloodBankManagementSystem.Forms.Technician
{
    public partial class ComponentPreparation : BaseTechnicianForm
    {
        private Panel pnlParentBag, pnlFooter;
        private DataGridView dgvComponents;
        private TextBox txtParentBagID, txtBloodGroup, txtCollectionVolume;
        private TextBox txtPreparedBy;
        private DateTimePicker dtpDate, dtpTime;
        private Button btnPrepare, btnSearchBag, btnRefresh;
        private ComboBox cmbSearchBag;
        private Label lblStatus;
        private DataTable componentsData;
        private string currentParentBagID = "";

        // Component volume mapping
        private readonly Dictionary<string, int> componentVolumes = new Dictionary<string, int>
        {
            { "Plasma", 250 },
            { "Red Cells", 200 },
            { "White Cells", 150 },
            { "Platelets", 50 },
            { "Cryoprecipitate", 20 },
            { "Factor VIII", 10 },
            { "Albumin", 100 }
        };

        // All available components
        private readonly string[] allComponents = {
            "Plasma", "Red Cells", "White Cells", "Platelets",
            "Cryoprecipitate", "Factor VIII", "Albumin"
        };

        public ComponentPreparation()
        {
            this.Text = "Blood Bank Management System – Component Preparation";
            BuildLayout();
            BuildSidebar("Component Prep");
            BuildTopBar("Component Preparation");
            BuildContentArea();

            // Load data from database
            LoadAvailableBags();
            SetDefaultPreparedBy();
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

            // =========================================================
            // PARENT BAG SELECTION SECTION
            // =========================================================
            Panel pnlSelection = new Panel
            {
                Location = new Point(0, y + 25),
                Width = pnlContent.ClientSize.Width - 60,
                Height = 60,
                BackColor = C_CardBg,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            SetRoundedRegion(pnlSelection, 12);
            AddDropShadow(pnlSelection);
            pnlContent.Controls.Add(pnlSelection);

            Label lblSelectBag = new Label
            {
                Text = "Select Parent Bag:",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = C_TextDark,
                Location = new Point(20, 20),
                AutoSize = true
            };
            pnlSelection.Controls.Add(lblSelectBag);

            cmbSearchBag = new ComboBox
            {
                Location = new Point(150, 18),
                Width = 250,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10),
                BackColor = C_White
            };
            cmbSearchBag.SelectedIndexChanged += CmbSearchBag_SelectedIndexChanged;
            pnlSelection.Controls.Add(cmbSearchBag);

            btnRefresh = new Button
            {
                Text = "🔄 Refresh Bags",
                Location = new Point(420, 16),
                Size = new Size(130, 30),
                BackColor = Color.FromArgb(60, 70, 80),
                ForeColor = C_White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9),
                Cursor = Cursors.Hand
            };
            btnRefresh.FlatAppearance.BorderSize = 0;
            SetRoundedRegion(btnRefresh, 6);
            btnRefresh.Click += (s, e) => LoadAvailableBags();
            pnlSelection.Controls.Add(btnRefresh);

            y += pnlSelection.Height + 20;

            // =========================================================
            // PARENT BAG INFORMATION PANEL
            // =========================================================
            pnlParentBag = new Panel
            {
                Location = new Point(0, y),
                Width = pnlContent.ClientSize.Width - 60,
                Height = 160,
                BackColor = C_CardBg,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            SetRoundedRegion(pnlParentBag, 12);
            AddDropShadow(pnlParentBag);
            pnlContent.Controls.Add(pnlParentBag);

            Label lblParentTitle = new Label
            {
                Text = "📦 Parent Bag Information",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = C_TextDark,
                Location = new Point(20, 15),
                AutoSize = true
            };
            pnlParentBag.Controls.Add(lblParentTitle);

            int fieldY = 60, spacing = 220, startX = 20;
            AddReadOnlyField(pnlParentBag, "Bag ID:", startX, fieldY, out txtParentBagID, 180);
            AddReadOnlyField(pnlParentBag, "Blood Group:", startX + spacing, fieldY, out txtBloodGroup, 180);
            AddReadOnlyField(pnlParentBag, "Collection Volume (mL):", startX + spacing * 2, fieldY, out txtCollectionVolume, 180);

            y += pnlParentBag.Height + 25;

            // =========================================================
            // COMPONENTS SECTION
            // =========================================================
            Label lblComponents = new Label
            {
                Text = "🧪 Components to Prepare",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = C_TextDark,
                Location = new Point(0, y),
                AutoSize = true
            };
            pnlContent.Controls.Add(lblComponents);
            y += 35;

            dgvComponents = CreateComponentsGrid();
            dgvComponents.Location = new Point(0, y);
            dgvComponents.Height = 380;
            dgvComponents.Width = pnlContent.ClientSize.Width - 60;
            dgvComponents.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            pnlContent.Controls.Add(dgvComponents);
            FixDataGridView(dgvComponents);
            y += dgvComponents.Height + 25;

            // =========================================================
            // FOOTER PANEL
            // =========================================================
            pnlFooter = new Panel
            {
                Location = new Point(0, y),
                Width = pnlContent.ClientSize.Width - 60,
                Height = 100,
                BackColor = C_CardBg,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            SetRoundedRegion(pnlFooter, 12);
            AddDropShadow(pnlFooter);
            pnlContent.Controls.Add(pnlFooter);

            Label lblPreparedBy = new Label
            {
                Text = "Prepared By:",
                Font = new Font("Segoe UI", 10),
                ForeColor = C_TextMid,
                Location = new Point(20, 20),
                AutoSize = true
            };
            pnlFooter.Controls.Add(lblPreparedBy);

            txtPreparedBy = new TextBox
            {
                Location = new Point(120, 17),
                Width = 200,
                Font = new Font("Segoe UI", 10),
                BorderStyle = BorderStyle.FixedSingle
            };
            pnlFooter.Controls.Add(txtPreparedBy);

            Label lblDateLabel = new Label
            {
                Text = "Date:",
                Font = new Font("Segoe UI", 10),
                ForeColor = C_TextMid,
                Location = new Point(350, 20),
                AutoSize = true
            };
            pnlFooter.Controls.Add(lblDateLabel);

            dtpDate = new DateTimePicker
            {
                Location = new Point(400, 17),
                Width = 130,
                Format = DateTimePickerFormat.Short,
                Value = DateTime.Today
            };
            pnlFooter.Controls.Add(dtpDate);

            Label lblTimeLabel = new Label
            {
                Text = "Time:",
                Font = new Font("Segoe UI", 10),
                ForeColor = C_TextMid,
                Location = new Point(550, 20),
                AutoSize = true
            };
            pnlFooter.Controls.Add(lblTimeLabel);

            dtpTime = new DateTimePicker
            {
                Location = new Point(600, 17),
                Width = 110,
                Format = DateTimePickerFormat.Time,
                ShowUpDown = true,
                Value = DateTime.Now
            };
            pnlFooter.Controls.Add(dtpTime);

            btnPrepare = new Button
            {
                Text = "✅ Prepare Components",
                Location = new Point(20, 55),
                Size = new Size(220, 35),
                BackColor = brickRed,
                ForeColor = C_White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnPrepare.FlatAppearance.BorderSize = 0;
            SetRoundedRegion(btnPrepare, 8);
            btnPrepare.Click += BtnPrepare_Click;
            pnlFooter.Controls.Add(btnPrepare);

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
        // LOAD AVAILABLE BAGS FROM DATABASE
        // =========================================================
        private void LoadAvailableBags()
        {
            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();

                    string query = @"
                        SELECT 
                            BagID,
                            BloodGroup,
                            Volume,
                            CollectionDate,
                            Status
                        FROM BloodBags 
                        WHERE Status = 'Available' 
                        AND (ComponentType IS NULL OR ComponentType = 'Whole Blood')
                        ORDER BY CollectionDate DESC";

                    SqlDataAdapter da = new SqlDataAdapter(query, conn);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    cmbSearchBag.Items.Clear();
                    cmbSearchBag.DisplayMember = "DisplayText";
                    cmbSearchBag.ValueMember = "BagID";

                    foreach (DataRow row in dt.Rows)
                    {
                        var item = new
                        {
                            BagID = row["BagID"].ToString(),
                            DisplayText = $"{row["BagID"]} - {row["BloodGroup"]} ({row["Volume"]} mL)"
                        };
                        cmbSearchBag.Items.Add(item);
                    }

                    if (cmbSearchBag.Items.Count > 0)
                        cmbSearchBag.SelectedIndex = 0;

                    UpdateStatus($"✅ Loaded {cmbSearchBag.Items.Count} available bag(s)", Color.FromArgb(16, 185, 129));
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LoadAvailableBags Error: {ex.Message}");
                UpdateStatus($"❌ Error loading bags: {ex.Message}", Color.FromArgb(220, 38, 38));
            }
        }

        // =========================================================
        // LOAD SELECTED BAG DETAILS FROM DATABASE
        // =========================================================
        private void CmbSearchBag_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbSearchBag.SelectedItem == null) return;

            dynamic selected = cmbSearchBag.SelectedItem;
            string bagId = selected.BagID;
            currentParentBagID = bagId;

            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();

                    string query = @"
                        SELECT 
                            BagID,
                            BloodGroup,
                            Volume as CollectionVolume,
                            FORMAT(CollectionDate, 'dd-MMM-yyyy') as CollectionDate,
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
                                txtParentBagID.Text = reader["BagID"].ToString();
                                txtBloodGroup.Text = reader["BloodGroup"].ToString();
                                txtCollectionVolume.Text = reader["CollectionVolume"].ToString();
                            }
                        }
                    }
                }

                // Reset components grid
                ResetComponentsGrid();
                UpdateStatus($"✅ Loaded bag: {bagId}", Color.FromArgb(16, 185, 129));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"CmbSearchBag_SelectedIndexChanged Error: {ex.Message}");
                UpdateStatus($"❌ Error loading bag details: {ex.Message}", Color.FromArgb(220, 38, 38));
            }
        }

        private void ResetComponentsGrid()
        {
            dgvComponents.Rows.Clear();
            foreach (string comp in allComponents)
            {
                dgvComponents.Rows.Add(comp, "0", "0");
            }
        }

        private void SetDefaultPreparedBy()
        {
            txtPreparedBy.Text = SessionManager.CurrentFullName;
        }

        private void AddReadOnlyField(Panel parent, string labelText, int x, int y, out TextBox txt, int width)
        {
            Label lbl = new Label
            {
                Text = labelText,
                Font = new Font("Segoe UI", 10),
                ForeColor = C_TextMid,
                Location = new Point(x, y),
                AutoSize = true
            };
            parent.Controls.Add(lbl);

            txt = new TextBox
            {
                Location = new Point(x + 120, y - 3),
                Width = width,
                ReadOnly = true,
                BackColor = Color.FromArgb(245, 245, 250),
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 10)
            };
            parent.Controls.Add(txt);
        }

        private DataGridView CreateComponentsGrid()
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
                RowTemplate = { Height = 50 },
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

            dgv.Columns.Add("Component", "Component");
            dgv.Columns.Add("Units", "Units");
            dgv.Columns.Add("Estimated Volume (mL)", "Estimated Volume (mL)");

            dgv.Columns[0].FillWeight = 40;
            dgv.Columns[1].FillWeight = 30;
            dgv.Columns[2].FillWeight = 30;

            dgv.CellPainting += DgvComponents_CellPainting;
            dgv.CellClick += DgvComponents_CellClick;
            return dgv;
        }

        private void DgvComponents_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex == dgvComponents.Columns["Units"].Index)
            {
                e.PaintBackground(e.CellBounds, true);
                int btnW = 30, btnH = 26, gap = 5;
                int startX = e.CellBounds.Left + (e.CellBounds.Width - (btnW * 2 + gap)) / 2;
                int startY = e.CellBounds.Top + (e.CellBounds.Height - btnH) / 2;

                Rectangle minusRect = new Rectangle(startX, startY, btnW, btnH);
                using (SolidBrush brush = new SolidBrush(brickRed))
                    e.Graphics.FillRectangle(brush, minusRect);
                TextRenderer.DrawText(e.Graphics, "-", new Font("Segoe UI", 10, FontStyle.Bold), minusRect, Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

                Rectangle plusRect = new Rectangle(startX + btnW + gap, startY, btnW, btnH);
                using (SolidBrush brush = new SolidBrush(brickRed))
                    e.Graphics.FillRectangle(brush, plusRect);
                TextRenderer.DrawText(e.Graphics, "+", new Font("Segoe UI", 10, FontStyle.Bold), plusRect, Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

                string unitsValue = e.Value?.ToString() ?? "0";
                int textX = startX + btnW + gap + btnW + 10;
                TextRenderer.DrawText(e.Graphics, unitsValue, new Font("Segoe UI", 10), new Point(textX, startY + btnH / 2 - 8), C_TextDark);
                e.Handled = true;
            }
            else if (e.RowIndex >= 0 && e.ColumnIndex == dgvComponents.Columns["Estimated Volume (mL)"].Index)
            {
                e.PaintBackground(e.CellBounds, true);
                e.PaintContent(e.CellBounds);
                e.Handled = true;
            }
        }

        private void DgvComponents_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex == dgvComponents.Columns["Units"].Index)
            {
                Rectangle cellBounds = dgvComponents.GetCellDisplayRectangle(e.ColumnIndex, e.RowIndex, false);
                Point mousePos = dgvComponents.PointToClient(Cursor.Position);
                int btnW = 30, btnH = 26, gap = 5;
                int startX = cellBounds.Left + (cellBounds.Width - (btnW * 2 + gap)) / 2;
                int startY = cellBounds.Top + (cellBounds.Height - btnH) / 2;

                Rectangle minusRect = new Rectangle(startX, startY, btnW, btnH);
                Rectangle plusRect = new Rectangle(startX + btnW + gap, startY, btnW, btnH);

                string component = dgvComponents.Rows[e.RowIndex].Cells["Component"].Value.ToString();
                int currentUnits = Convert.ToInt32(dgvComponents.Rows[e.RowIndex].Cells["Units"].Value);
                int volumePerUnit = componentVolumes.ContainsKey(component) ? componentVolumes[component] : 0;

                if (minusRect.Contains(mousePos) && currentUnits > 0)
                    currentUnits--;
                else if (plusRect.Contains(mousePos))
                    currentUnits++;
                else
                    return;

                dgvComponents.Rows[e.RowIndex].Cells["Units"].Value = currentUnits.ToString();
                dgvComponents.Rows[e.RowIndex].Cells["Estimated Volume (mL)"].Value = (currentUnits * volumePerUnit).ToString();
                dgvComponents.InvalidateRow(e.RowIndex);
            }
        }

        // =========================================================
        // PREPARE COMPONENTS - SAVE TO DATABASE
        // =========================================================
        private async void BtnPrepare_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(currentParentBagID))
            {
                MessageBox.Show("Please select a parent bag first.", "No Bag Selected",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Check which components are selected
            var selectedComponents = new DataTable();
            selectedComponents.Columns.Add("ComponentName");
            selectedComponents.Columns.Add("Units");
            selectedComponents.Columns.Add("Volume");

            int totalVolumeUsed = 0;
            StringBuilder componentsList = new StringBuilder();

            foreach (DataGridViewRow row in dgvComponents.Rows)
            {
                string comp = row.Cells["Component"].Value.ToString();
                string unitsStr = row.Cells["Units"].Value.ToString();
                string volumeStr = row.Cells["Estimated Volume (mL)"].Value.ToString();

                if (unitsStr != "0")
                {
                    int units = int.Parse(unitsStr);
                    int volume = int.Parse(volumeStr);
                    totalVolumeUsed += volume;
                    selectedComponents.Rows.Add(comp, units, volume);
                    componentsList.AppendLine($"- {comp}: {units} unit(s) ({volume} mL)");
                }
            }

            if (selectedComponents.Rows.Count == 0)
            {
                MessageBox.Show("Please select at least one component to prepare.", "No Components",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Check if total volume exceeds collection volume
            int collectionVolume = int.Parse(txtCollectionVolume.Text);
            if (totalVolumeUsed > collectionVolume)
            {
                DialogResult result = MessageBox.Show(
                    $"Total component volume ({totalVolumeUsed} mL) exceeds collection volume ({collectionVolume} mL).\n\n" +
                    "Do you want to continue anyway?",
                    "Volume Exceeded", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if (result != DialogResult.Yes)
                    return;
            }

            try
            {
                ShowLoading("Preparing components...");

                using (var conn = DatabaseHelper.GetConnection())
                {
                    await conn.OpenAsync();
                    using (var transaction = conn.BeginTransaction())
                    {
                        try
                        {
                            // 1. Update parent bag status
                            string updateParentSql = @"
                                UPDATE BloodBags 
                                SET Status = 'Component Prep', 
                                    UpdatedAt = GETDATE(),
                                    Remarks = ISNULL(Remarks, '') + CHAR(10) + 'Components prepared on ' + CAST(GETDATE() AS VARCHAR)
                                WHERE BagID = @BagID";

                            using (var cmd = new SqlCommand(updateParentSql, conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@BagID", currentParentBagID);
                                await cmd.ExecuteNonQueryAsync();
                            }

                            // 2. Insert each component as a new blood bag
                            string insertComponentSql = @"
                                INSERT INTO BloodBags 
                                (BagID, ParentBagID, DonorID, DonorName, BloodGroup, ComponentType, 
                                 Quantity, Volume, CollectionDate, ExpiryDate, Status, 
                                 PreparedBy, CreatedAt, CreatedBy)
                                VALUES 
                                (@BagID, @ParentBagID, @DonorID, @DonorName, @BloodGroup, @ComponentType,
                                 1, @Volume, @CollectionDate, @ExpiryDate, 'Available',
                                 @PreparedBy, GETDATE(), @CreatedBy)";

                            DateTime collectionDate = dtpDate.Value;
                            DateTime preparationDateTime = dtpDate.Value.Date + dtpTime.Value.TimeOfDay;

                            foreach (DataRow row in selectedComponents.Rows)
                            {
                                string componentName = row["ComponentName"].ToString();
                                int volume = Convert.ToInt32(row["Volume"]);
                                string newBagID = GenerateComponentBagID(componentName);
                                DateTime expiryDate = CalculateExpiryDate(collectionDate, componentName);

                                using (var cmd = new SqlCommand(insertComponentSql, conn, transaction))
                                {
                                    cmd.Parameters.AddWithValue("@BagID", newBagID);
                                    cmd.Parameters.AddWithValue("@ParentBagID", currentParentBagID);
                                    cmd.Parameters.AddWithValue("@DonorID", DBNull.Value);
                                    cmd.Parameters.AddWithValue("@DonorName", DBNull.Value);
                                    cmd.Parameters.AddWithValue("@BloodGroup", txtBloodGroup.Text);
                                    cmd.Parameters.AddWithValue("@ComponentType", componentName);
                                    cmd.Parameters.AddWithValue("@Volume", volume);
                                    cmd.Parameters.AddWithValue("@CollectionDate", collectionDate);
                                    cmd.Parameters.AddWithValue("@ExpiryDate", expiryDate);
                                    cmd.Parameters.AddWithValue("@PreparedBy", txtPreparedBy.Text);
                                    cmd.Parameters.AddWithValue("@CreatedBy", SessionManager.CurrentUserID);

                                    await cmd.ExecuteNonQueryAsync();
                                }
                            }

                            transaction.Commit();

                            // Show success message
                            string successMessage = $"✅ Components prepared successfully!\n\n" +
                                $"Parent Bag: {currentParentBagID}\n" +
                                $"Blood Group: {txtBloodGroup.Text}\n" +
                                $"Collection Volume: {collectionVolume} mL\n" +
                                $"Total Components Volume: {totalVolumeUsed} mL\n\n" +
                                $"Components Prepared:\n{componentsList}\n" +
                                $"Prepared By: {txtPreparedBy.Text}\n" +
                                $"Date/Time: {dtpDate.Value:dd-MMM-yyyy} {dtpTime.Value:hh:mm tt}";

                            MessageBox.Show(successMessage, "Preparation Complete",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);

                            // Log the activity
                            AuditHelper.Log("Component Preparation", "BloodBag",
                                $"Components prepared from bag {currentParentBagID} by {txtPreparedBy.Text}");

                            // Reset form
                            ResetForm();

                            // Refresh available bags list
                            LoadAvailableBags();
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            throw ex;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"BtnPrepare_Click Error: {ex.Message}");
                MessageBox.Show($"Error preparing components: {ex.Message}",
                    "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                UpdateStatus($"❌ Preparation failed: {ex.Message}", Color.FromArgb(220, 38, 38));
            }
            finally
            {
                HideLoading();
            }
        }

        private string GenerateComponentBagID(string componentName)
        {
            string prefix = "";
            switch (componentName)
            {
                case "Plasma": prefix = "PL"; break;
                case "Red Cells": prefix = "RC"; break;
                case "White Cells": prefix = "WC"; break;
                case "Platelets": prefix = "PT"; break;
                case "Cryoprecipitate": prefix = "CR"; break;
                case "Factor VIII": prefix = "F8"; break;
                case "Albumin": prefix = "AL"; break;
                default: prefix = "CP"; break;
            }

            return $"{prefix}-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 4).ToUpper()}";
        }

        private DateTime CalculateExpiryDate(DateTime collectionDate, string componentType)
        {
            switch (componentType)
            {
                case "Red Cells":
                    return collectionDate.AddDays(35);
                case "Platelets":
                    return collectionDate.AddDays(5);
                case "Plasma":
                case "Cryoprecipitate":
                case "Factor VIII":
                case "Albumin":
                    return collectionDate.AddDays(365);
                default:
                    return collectionDate.AddDays(35);
            }
        }

        private void ResetForm()
        {
            currentParentBagID = "";
            txtParentBagID.Clear();
            txtBloodGroup.Clear();
            txtCollectionVolume.Clear();
            ResetComponentsGrid();
            dtpDate.Value = DateTime.Today;
            dtpTime.Value = DateTime.Now;
            SetDefaultPreparedBy();

            if (cmbSearchBag.Items.Count > 0)
                cmbSearchBag.SelectedIndex = 0;
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

        private void NoSort(DataGridView dgv)
        {
            foreach (DataGridViewColumn col in dgv.Columns)
                col.SortMode = DataGridViewColumnSortMode.NotSortable;
        }
    }
}