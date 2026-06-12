using System;
using System.Drawing;
using System.Windows.Forms;
using BloodBankManagementSystem.Classes.Database;
using BloodBankManagementSystem.Classes.Helpers;

namespace BloodBankManagementSystem.Forms.Manager
{
    public partial class AddEditBloodBag : Form
    {
        private string _bagId = "";
        private bool _isEditMode = false;

        private TextBox txtBagID, txtDonorID, txtDonorName, txtVolume, txtStorageLocation, txtRemarks;
        private ComboBox cmbBloodGroup, cmbComponentType, cmbStatus;
        private DateTimePicker dtpCollectionDate, dtpExpiryDate;
        private Button btnSave, btnCancel;

        public AddEditBloodBag(string bagId = "")
        {
            _bagId = bagId;
            _isEditMode = !string.IsNullOrEmpty(bagId);
            BuildUI();
            if (_isEditMode) LoadBagData();
        }

        private void BuildUI()
        {
            this.Text = _isEditMode ? "✏️ Edit Blood Bag" : "🩸 Add New Blood Bag";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.White;

            // FixedDialog ki jagah Sizable — AutoScroll ke liye zaroori
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            int leftX = 30;
            int labelW = 145;
            int fieldX = leftX + labelW + 10;
            int fieldW = 340;
            int y = 20;

            // Scrollable panel
            Panel main = new Panel
            {
                Location = new Point(0, 0),
                BackColor = Color.White,
                AutoScroll = true,
                Dock = DockStyle.Fill   // form ke saath resize ho
            };
            this.Controls.Add(main);

            // Inner panel — content yahan rakho, scroll is par hoga
            Panel inner = new Panel
            {
                Location = new Point(0, 0),
                BackColor = Color.White,
                AutoSize = false
            };
            main.Controls.Add(inner);

            // Title
            Label lblTitle = new Label
            {
                Text = _isEditMode ? "✏️ Edit Blood Bag" : "🩸 Add New Blood Bag",
                Font = new Font("Segoe UI", 17, FontStyle.Bold),
                ForeColor = Color.FromArgb(120, 22, 27),
                Location = new Point(leftX, y),
                Width = labelW + 10 + fieldW,
                Height = 42,
                TextAlign = ContentAlignment.MiddleCenter
            };
            inner.Controls.Add(lblTitle);
            y += 55;

            void AddLabel(string text, int row_y)
            {
                inner.Controls.Add(new Label
                {
                    Text = text,
                    Location = new Point(leftX, row_y + 5),
                    Size = new Size(labelW, 24),
                    Font = new Font("Segoe UI", 10, FontStyle.Bold),
                    ForeColor = Color.FromArgb(60, 60, 70)
                });
            }

            // Bag ID
            AddLabel("Bag ID:", y);
            txtBagID = new TextBox
            {
                Location = new Point(fieldX, y),
                Size = new Size(fieldW, 32),
                Font = new Font("Segoe UI", 11),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = _isEditMode ? Color.FromArgb(240, 240, 240) : Color.FromArgb(250, 250, 252),
                ReadOnly = _isEditMode
            };
            if (!_isEditMode) txtBagID.Text = GenerateBagID();
            inner.Controls.Add(txtBagID);
            y += 55;

            // Donor ID
            AddLabel("Donor ID:", y);
            txtDonorID = new TextBox
            {
                Location = new Point(fieldX, y),
                Size = new Size(fieldW, 32),
                Font = new Font("Segoe UI", 11),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(250, 250, 252)
            };
            txtDonorID.TextChanged += TxtDonorID_TextChanged;
            inner.Controls.Add(txtDonorID);
            y += 55;

            // Donor Name
            AddLabel("Donor Name:", y);
            txtDonorName = new TextBox
            {
                Location = new Point(fieldX, y),
                Size = new Size(fieldW, 32),
                Font = new Font("Segoe UI", 11),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(240, 240, 240),
                ReadOnly = true
            };
            inner.Controls.Add(txtDonorName);
            y += 55;

            // Blood Group
            AddLabel("Blood Group:", y);
            cmbBloodGroup = new ComboBox
            {
                Location = new Point(fieldX, y),
                Size = new Size(150, 32),
                Font = new Font("Segoe UI", 11),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(250, 250, 252)
            };
            cmbBloodGroup.Items.AddRange(new[] { "A+", "A-", "B+", "B-", "AB+", "AB-", "O+", "O-" });
            cmbBloodGroup.SelectedIndex = 0;
            inner.Controls.Add(cmbBloodGroup);
            y += 55;

            // Component Type
            AddLabel("Component Type:", y);
            cmbComponentType = new ComboBox
            {
                Location = new Point(fieldX, y),
                Size = new Size(200, 32),
                Font = new Font("Segoe UI", 11),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(250, 250, 252)
            };
            cmbComponentType.Items.AddRange(new[] { "Whole Blood", "Red Cells", "Plasma", "Platelets", "Cryoprecipitate" });
            cmbComponentType.SelectedIndex = 0;
            cmbComponentType.SelectedIndexChanged += (s, e) => UpdateExpiryDate();
            inner.Controls.Add(cmbComponentType);
            y += 55;

            // Volume
            AddLabel("Volume (mL):", y);
            txtVolume = new TextBox
            {
                Location = new Point(fieldX, y),
                Size = new Size(100, 32),
                Font = new Font("Segoe UI", 11),
                BorderStyle = BorderStyle.FixedSingle,
                Text = "450",
                BackColor = Color.FromArgb(250, 250, 252)
            };
            inner.Controls.Add(txtVolume);
            y += 55;

            // Collection Date
            AddLabel("Collection Date:", y);
            dtpCollectionDate = new DateTimePicker
            {
                Location = new Point(fieldX, y),
                Size = new Size(200, 32),
                Format = DateTimePickerFormat.Short,
                Font = new Font("Segoe UI", 11),
                Value = DateTime.Now
            };
            dtpCollectionDate.ValueChanged += (s, e) => UpdateExpiryDate();
            inner.Controls.Add(dtpCollectionDate);
            y += 55;

            // Expiry Date
            AddLabel("Expiry Date:", y);
            dtpExpiryDate = new DateTimePicker
            {
                Location = new Point(fieldX, y),
                Size = new Size(200, 32),
                Format = DateTimePickerFormat.Short,
                Font = new Font("Segoe UI", 11),
                Enabled = false,
                BackColor = Color.FromArgb(240, 240, 240)
            };
            inner.Controls.Add(dtpExpiryDate);
            y += 55;

            // Storage Location
            AddLabel("Storage Location:", y);
            txtStorageLocation = new TextBox
            {
                Location = new Point(fieldX, y),
                Size = new Size(fieldW, 32),
                Font = new Font("Segoe UI", 11),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(250, 250, 252)
            };
            inner.Controls.Add(txtStorageLocation);
            y += 55;

            // Status
            AddLabel("Status:", y);
            cmbStatus = new ComboBox
            {
                Location = new Point(fieldX, y),
                Size = new Size(150, 32),
                Font = new Font("Segoe UI", 11),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(250, 250, 252)
            };
            cmbStatus.Items.AddRange(new[] { "Available", "In Lab", "Issued", "Quarantined", "Expired", "Discarded" });
            cmbStatus.SelectedIndex = 0;
            inner.Controls.Add(cmbStatus);
            y += 55;

            // Remarks
            AddLabel("Remarks:", y);
            txtRemarks = new TextBox
            {
                Location = new Point(fieldX, y),
                Size = new Size(fieldW, 60),
                Multiline = true,
                Font = new Font("Segoe UI", 11),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(250, 250, 252)
            };
            inner.Controls.Add(txtRemarks);
            y += 75;
            y += 18;

            // Buttons
            int totalW = 250;
            int btnX = (leftX + labelW + 10 + fieldW / 2) - totalW / 2;

            btnSave = new Button
            {
                Text = "💾 Save",
                Location = new Point(btnX, y),
                Size = new Size(115, 42),
                BackColor = Color.FromArgb(120, 22, 27),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += BtnSave_Click;
            inner.Controls.Add(btnSave);

            btnCancel = new Button
            {
                Text = "❌ Cancel",
                Location = new Point(btnX + 130, y),
                Size = new Size(115, 42),
                BackColor = Color.FromArgb(220, 220, 225),
                ForeColor = Color.FromArgb(50, 50, 50),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11),
                Cursor = Cursors.Hand
            };
            btnCancel.FlatAppearance.BorderSize = 0;
            btnCancel.Click += (s, e) => this.Close();
            inner.Controls.Add(btnCancel);

            // Content ki total height
            int contentH = y + 42 + 30;
            int formW = leftX + labelW + 10 + fieldW + leftX + SystemInformation.VerticalScrollBarWidth;

            // Inner panel ko poora content ki size do
            inner.Size = new Size(formW, contentH);

            // Form ki height — screen ka 85% se zyada nahi
            int screenH = Screen.PrimaryScreen.WorkingArea.Height;
            int formH = Math.Min(contentH, (int)(screenH * 0.85));

            this.ClientSize = new Size(formW, formH);

            UpdateExpiryDate();
        }

        private string GenerateBagID()
        {
            return $"BB-{DateTime.Now:yyyyMMdd}-{new Random().Next(1000, 9999)}";
        }

        private async void TxtDonorID_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtDonorID.Text)) return;

            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    await conn.OpenAsync();
                    string sql = "SELECT FullName FROM Donors WHERE DonorID = @DonorID OR UserID = @DonorID";
                    using (var cmd = new System.Data.SqlClient.SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@DonorID", txtDonorID.Text.Trim());
                        var result = await cmd.ExecuteScalarAsync();
                        txtDonorName.Text = result?.ToString() ?? "";
                    }
                }
            }
            catch { txtDonorName.Text = ""; }
        }

        private void UpdateExpiryDate()
        {
            string component = cmbComponentType.SelectedItem?.ToString();
            DateTime collection = dtpCollectionDate.Value;

            if (component == "Platelets")
                dtpExpiryDate.Value = collection.AddDays(5);
            else if (component == "Plasma" || component == "Cryoprecipitate")
                dtpExpiryDate.Value = collection.AddDays(365);
            else
                dtpExpiryDate.Value = collection.AddDays(35);
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtBagID.Text))
            {
                MessageBox.Show("Bag ID is required.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                var bag = new BloodBankManagementSystem.Classes.Models.BloodBag
                {
                    BagID = txtBagID.Text.Trim(),
                    DonorID = txtDonorID.Text.Trim(),
                    DonorName = txtDonorName.Text.Trim(),
                    BloodGroup = cmbBloodGroup.SelectedItem?.ToString(),
                    ComponentType = cmbComponentType.SelectedItem?.ToString(),
                    Volume = int.TryParse(txtVolume.Text, out int v) ? v : 450,
                    CollectionDate = dtpCollectionDate.Value,
                    ExpiryDate = dtpExpiryDate.Value,
                    StorageLocation = txtStorageLocation.Text.Trim(),
                    Status = cmbStatus.SelectedItem?.ToString(),
                    Remarks = txtRemarks.Text.Trim()
                };

                bool success;
                if (_isEditMode)
                    success = BloodBagDAL.Update(bag);
                else
                    success = BloodBagDAL.Insert(bag);

                if (success)
                {
                    MessageBox.Show($"Blood bag {(_isEditMode ? "updated" : "added")} successfully!", "Success",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Failed to save blood bag.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadBagData()
        {
            try
            {
                var bag = BloodBagDAL.GetByID(_bagId);
                if (bag == null) return;

                txtBagID.Text = bag.BagID;
                txtDonorID.Text = bag.DonorID;
                txtDonorName.Text = bag.DonorName;
                cmbBloodGroup.Text = bag.BloodGroup;
                cmbComponentType.Text = bag.ComponentType;
                txtVolume.Text = bag.Volume.ToString();
                dtpCollectionDate.Value = bag.CollectionDate;
                dtpExpiryDate.Value = bag.ExpiryDate;
                txtStorageLocation.Text = bag.StorageLocation;
                cmbStatus.Text = bag.Status;
                txtRemarks.Text = bag.Remarks;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}