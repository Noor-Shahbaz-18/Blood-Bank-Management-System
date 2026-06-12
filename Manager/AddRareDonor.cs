using System;
using System.Drawing;
using System.Windows.Forms;
using BloodBankManagementSystem.Classes.Database;
using BloodBankManagementSystem.Classes.Helpers;
using BloodBankManagementSystem.Classes.Models;

namespace BloodBankManagementSystem.Forms.Manager
{
    public partial class AddRareDonor : Form
    {
        private int _donorId = 0;
        private bool _isEditMode = false;

        private Panel main;
        private TextBox txtFullName, txtCNIC, txtPhone, txtEmail, txtAddress, txtCity, txtRareBloodType, txtMedicalHistory, txtNotes;
        private ComboBox cmbBloodGroup, cmbGender, cmbStatus;
        private DateTimePicker dtpDateOfBirth;
        private NumericUpDown numWeight;
        private Button btnSave, btnCancel;
        private TextBox txtAge;
        private Label lblStatus;

        public AddRareDonor(int donorId = 0)
        {
            _donorId = donorId;
            _isEditMode = donorId > 0;
            this.Text = _isEditMode ? "✏️ Edit Rare Donor" : "⭐ Add Rare Donor";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.White;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Size = new Size(950, 750);

            BuildUI();
            if (_isEditMode) LoadDonorData();
        }

        private void BuildUI()
        {
            main = new Panel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = Color.White };
            this.Controls.Add(main);

            int leftX = 30;
            int labelW = 130;
            int fieldX = leftX + labelW + 10;
            int fieldW = 280;
            int y = 20;

            // Title
            Label lblTitle = new Label
            {
                Text = _isEditMode ? "✏️ Edit Rare Donor" : "⭐ Add Rare Donor",
                Font = new Font("Segoe UI", 17, FontStyle.Bold),
                ForeColor = Color.FromArgb(120, 22, 27),
                Location = new Point(leftX, y),
                AutoSize = true
            };
            main.Controls.Add(lblTitle);
            y += 55;

            // Status Label
            lblStatus = new Label
            {
                Text = "",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(16, 185, 129),
                Location = new Point(leftX, y),
                AutoSize = true,
                Visible = false
            };
            main.Controls.Add(lblStatus);
            y += 30;

            // Full Name
            main.Controls.Add(new Label
            {
                Text = "Full Name:*",
                Location = new Point(leftX, y + 5),
                Size = new Size(labelW, 24),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(60, 60, 70)
            });
            txtFullName = new TextBox
            {
                Location = new Point(fieldX, y),
                Size = new Size(fieldW, 32),
                Font = new Font("Segoe UI", 11),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(250, 250, 252)
            };
            main.Controls.Add(txtFullName);
            y += 50;

            // CNIC
            main.Controls.Add(new Label
            {
                Text = "CNIC:*",
                Location = new Point(leftX, y + 5),
                Size = new Size(labelW, 24),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(60, 60, 70)
            });
            txtCNIC = new TextBox
            {
                Location = new Point(fieldX, y),
                Size = new Size(fieldW, 32),
                Font = new Font("Segoe UI", 11),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(250, 250, 252)
            };
            main.Controls.Add(txtCNIC);
            y += 50;

            // Blood Group
            main.Controls.Add(new Label
            {
                Text = "Blood Group:",
                Location = new Point(leftX, y + 5),
                Size = new Size(labelW, 24),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(60, 60, 70)
            });
            cmbBloodGroup = new ComboBox
            {
                Location = new Point(fieldX, y),
                Size = new Size(120, 32),
                Font = new Font("Segoe UI", 11),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(250, 250, 252)
            };
            cmbBloodGroup.Items.AddRange(new[] { "A+", "A-", "B+", "B-", "AB+", "AB-", "O+", "O-" });
            cmbBloodGroup.SelectedIndex = 0;
            main.Controls.Add(cmbBloodGroup);
            y += 50;

            // Rare Blood Type
            main.Controls.Add(new Label
            {
                Text = "Rare Blood Type:*",
                Location = new Point(leftX, y + 5),
                Size = new Size(labelW, 24),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(60, 60, 70)
            });
            txtRareBloodType = new TextBox
            {
                Location = new Point(fieldX, y),
                Size = new Size(fieldW, 32),
                Font = new Font("Segoe UI", 11),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(250, 250, 252)
            };
            main.Controls.Add(txtRareBloodType);
            y += 50;

            // Phone
            main.Controls.Add(new Label
            {
                Text = "Phone:*",
                Location = new Point(leftX, y + 5),
                Size = new Size(labelW, 24),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(60, 60, 70)
            });
            txtPhone = new TextBox
            {
                Location = new Point(fieldX, y),
                Size = new Size(fieldW, 32),
                Font = new Font("Segoe UI", 11),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(250, 250, 252)
            };
            main.Controls.Add(txtPhone);
            y += 50;

            // Email
            main.Controls.Add(new Label
            {
                Text = "Email:",
                Location = new Point(leftX, y + 5),
                Size = new Size(labelW, 24),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(60, 60, 70)
            });
            txtEmail = new TextBox
            {
                Location = new Point(fieldX, y),
                Size = new Size(fieldW, 32),
                Font = new Font("Segoe UI", 11),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(250, 250, 252)
            };
            main.Controls.Add(txtEmail);
            y += 50;

            // Right Column
            int rightX = leftX + labelW + 10 + fieldW + 40;
            int ry = 75;

            // Date of Birth
            main.Controls.Add(new Label
            {
                Text = "Date of Birth:",
                Location = new Point(rightX, ry + 5),
                Size = new Size(labelW, 24),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(60, 60, 70)
            });
            dtpDateOfBirth = new DateTimePicker
            {
                Location = new Point(rightX + labelW, ry),
                Size = new Size(140, 32),
                Format = DateTimePickerFormat.Short,
                Font = new Font("Segoe UI", 11),
                Value = DateTime.Now.AddYears(-25)
            };
            dtpDateOfBirth.ValueChanged += (s, e) => CalculateAge();
            main.Controls.Add(dtpDateOfBirth);
            ry += 50;

            // Age
            main.Controls.Add(new Label
            {
                Text = "Age:",
                Location = new Point(rightX, ry + 5),
                Size = new Size(labelW, 24),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(60, 60, 70)
            });
            txtAge = new TextBox
            {
                Location = new Point(rightX + labelW, ry),
                Size = new Size(60, 32),
                Font = new Font("Segoe UI", 11),
                ReadOnly = true,
                BackColor = Color.FromArgb(240, 240, 240)
            };
            main.Controls.Add(txtAge);
            ry += 50;

            // Gender
            main.Controls.Add(new Label
            {
                Text = "Gender:",
                Location = new Point(rightX, ry + 5),
                Size = new Size(labelW, 24),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(60, 60, 70)
            });
            cmbGender = new ComboBox
            {
                Location = new Point(rightX + labelW, ry),
                Size = new Size(120, 32),
                Font = new Font("Segoe UI", 11),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(250, 250, 252)
            };
            cmbGender.Items.AddRange(new[] { "Male", "Female", "Other" });
            cmbGender.SelectedIndex = 0;
            main.Controls.Add(cmbGender);
            ry += 50;

            // Weight
            main.Controls.Add(new Label
            {
                Text = "Weight (kg):",
                Location = new Point(rightX, ry + 5),
                Size = new Size(labelW, 24),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(60, 60, 70)
            });
            numWeight = new NumericUpDown
            {
                Location = new Point(rightX + labelW, ry),
                Size = new Size(80, 32),
                Minimum = 30,
                Maximum = 200,
                Value = 70,
                Font = new Font("Segoe UI", 11)
            };
            main.Controls.Add(numWeight);
            ry += 50;

            // Address
            main.Controls.Add(new Label
            {
                Text = "Address:",
                Location = new Point(rightX, ry + 5),
                Size = new Size(labelW, 24),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(60, 60, 70)
            });
            txtAddress = new TextBox
            {
                Location = new Point(rightX + labelW, ry),
                Size = new Size(fieldW, 32),
                Font = new Font("Segoe UI", 11),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(250, 250, 252)
            };
            main.Controls.Add(txtAddress);
            ry += 50;

            // City
            main.Controls.Add(new Label
            {
                Text = "City:",
                Location = new Point(rightX, ry + 5),
                Size = new Size(labelW, 24),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(60, 60, 70)
            });
            txtCity = new TextBox
            {
                Location = new Point(rightX + labelW, ry),
                Size = new Size(150, 32),
                Font = new Font("Segoe UI", 11),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(250, 250, 252)
            };
            main.Controls.Add(txtCity);
            ry += 50;

            // Medical History
            y = Math.Max(y, ry) + 20;
            main.Controls.Add(new Label
            {
                Text = "Medical History:",
                Location = new Point(leftX, y + 5),
                Size = new Size(labelW, 24),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(60, 60, 70)
            });
            txtMedicalHistory = new TextBox
            {
                Location = new Point(leftX + labelW + 10, y),
                Size = new Size(560, 60),
                Multiline = true,
                Font = new Font("Segoe UI", 11),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(250, 250, 252)
            };
            main.Controls.Add(txtMedicalHistory);
            y += 75;

            // Notes
            main.Controls.Add(new Label
            {
                Text = "Additional Notes:",
                Location = new Point(leftX, y + 5),
                Size = new Size(labelW, 24),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(60, 60, 70)
            });
            txtNotes = new TextBox
            {
                Location = new Point(leftX + labelW + 10, y),
                Size = new Size(560, 60),
                Multiline = true,
                Font = new Font("Segoe UI", 11),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(250, 250, 252)
            };
            main.Controls.Add(txtNotes);
            y += 75;

            // Status
            main.Controls.Add(new Label
            {
                Text = "Status:",
                Location = new Point(leftX, y + 5),
                Size = new Size(labelW, 24),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(60, 60, 70)
            });
            cmbStatus = new ComboBox
            {
                Location = new Point(leftX + labelW + 10, y),
                Size = new Size(120, 32),
                Font = new Font("Segoe UI", 11),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(250, 250, 252)
            };
            cmbStatus.Items.AddRange(new[] { "Active", "Inactive" });
            cmbStatus.SelectedIndex = 0;
            main.Controls.Add(cmbStatus);
            y += 55;

            y += 20;

            // Buttons
            int btnX = (main.ClientSize.Width / 2) - 125;

            btnSave = new Button
            {
                Text = "💾 Save Donor",
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
            main.Controls.Add(btnSave);

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
            main.Controls.Add(btnCancel);

            main.AutoScrollMinSize = new Size(0, y + 150);
            CalculateAge();
        }

        private void CalculateAge()
        {
            DateTime dob = dtpDateOfBirth.Value;
            DateTime today = DateTime.Today;
            int age = today.Year - dob.Year;
            if (dob.Date > today.AddYears(-age)) age--;
            txtAge.Text = age.ToString();
        }

        private void ShowStatus(string message, Color color)
        {
            if (lblStatus != null)
            {
                lblStatus.Text = message;
                lblStatus.ForeColor = color;
                lblStatus.Visible = true;
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            // =========================================================
            // VALIDATION
            // =========================================================
            if (string.IsNullOrWhiteSpace(txtFullName.Text))
            {
                MessageBox.Show("Please enter donor name.", "Validation Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtFullName.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtRareBloodType.Text))
            {
                MessageBox.Show("Please enter rare blood type.", "Validation Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtRareBloodType.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtCNIC.Text))
            {
                MessageBox.Show("Please enter CNIC.", "Validation Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtCNIC.Focus();
                return;
            }

            if (!Validator.IsValidCNIC(txtCNIC.Text))
            {
                MessageBox.Show("Please enter valid CNIC (Format: 12345-1234567-1)", "Invalid CNIC", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtCNIC.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtPhone.Text))
            {
                MessageBox.Show("Please enter phone number.", "Validation Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtPhone.Focus();
                return;
            }

            // Check duplicate CNIC only for new donor
            if (!_isEditMode)
            {
                try
                {
                    bool exists = RareDonorDAL.IsCNICExists(txtCNIC.Text);
                    if (exists)
                    {
                        MessageBox.Show("A donor with this CNIC already exists!", "Duplicate CNIC", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        txtCNIC.Focus();
                        return;
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Check CNIC Error: {ex.Message}");
                    // Continue if check fails
                }
            }

            // =========================================================
            // SAVE DONOR
            // =========================================================
            try
            {
                ShowStatus("Saving donor...", Color.FromArgb(59, 130, 246));
                btnSave.Enabled = false;
                Cursor = Cursors.WaitCursor;

                // Calculate age from DOB
                DateTime dob = dtpDateOfBirth.Value;
                DateTime today = DateTime.Today;
                int age = today.Year - dob.Year;
                if (dob.Date > today.AddYears(-age)) age--;

                var donor = new RareDonor
                {
                    DonorID = _donorId,
                    FullName = txtFullName.Text.Trim(),
                    CNIC = txtCNIC.Text.Trim(),
                    BloodGroup = cmbBloodGroup.SelectedItem?.ToString(),
                    RareBloodType = txtRareBloodType.Text.Trim(),
                    Phone = txtPhone.Text.Trim(),
                    Email = string.IsNullOrWhiteSpace(txtEmail.Text) ? null : txtEmail.Text.Trim(),
                    Address = string.IsNullOrWhiteSpace(txtAddress.Text) ? null : txtAddress.Text.Trim(),
                    City = string.IsNullOrWhiteSpace(txtCity.Text) ? null : txtCity.Text.Trim(),
                    Gender = cmbGender.SelectedItem?.ToString(),
                    DateOfBirth = dob,
                    Age = age,
                    Weight = (int)numWeight.Value,
                    MedicalHistory = string.IsNullOrWhiteSpace(txtMedicalHistory.Text) ? null : txtMedicalHistory.Text.Trim(),
                    Notes = string.IsNullOrWhiteSpace(txtNotes.Text) ? null : txtNotes.Text.Trim(),
                    Status = cmbStatus.SelectedItem?.ToString() ?? "Active",
                    CreatedBy = SessionManager.CurrentUserID,
                    CreatedAt = DateTime.Now
                };

                // Debug output
                System.Diagnostics.Debug.WriteLine("=== Saving Donor ===");
                System.Diagnostics.Debug.WriteLine($"FullName: {donor.FullName}");
                System.Diagnostics.Debug.WriteLine($"CNIC: {donor.CNIC}");
                System.Diagnostics.Debug.WriteLine($"RareBloodType: {donor.RareBloodType}");
                System.Diagnostics.Debug.WriteLine($"Phone: {donor.Phone}");
                System.Diagnostics.Debug.WriteLine($"CreatedBy: {donor.CreatedBy}");
                System.Diagnostics.Debug.WriteLine($"IsEditMode: {_isEditMode}");

                bool success;

                if (_isEditMode)
                {
                    success = RareDonorDAL.Update(donor);
                    if (success)
                    {
                        MessageBox.Show($"Rare donor '{donor.FullName}' updated successfully!", "Success",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                        this.DialogResult = DialogResult.OK;
                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show("Failed to update donor. Please check the error log.", "Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        btnSave.Enabled = true;
                    }
                }
                else
                {
                    success = RareDonorDAL.Insert(donor);
                    if (success)
                    {
                        MessageBox.Show($"Rare donor '{donor.FullName}' added successfully!", "Success",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                        this.DialogResult = DialogResult.OK;
                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show("Failed to add donor. Please check the error log.\n\nCheck Output window (View → Output) for details.", "Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        btnSave.Enabled = true;
                    }
                }
            }
            catch (Exception ex)
            {
                string errorMsg = $"Error: {ex.Message}\n\n";
                errorMsg += $"Stack Trace: {ex.StackTrace}\n\n";
                if (ex.InnerException != null)
                {
                    errorMsg += $"Inner Error: {ex.InnerException.Message}";
                }

                MessageBox.Show(errorMsg, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                btnSave.Enabled = true;
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void LoadDonorData()
        {
            try
            {
                var donor = RareDonorDAL.GetByID(_donorId);
                if (donor == null)
                {
                    MessageBox.Show("Donor not found!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    this.Close();
                    return;
                }

                txtFullName.Text = donor.FullName;
                txtCNIC.Text = donor.CNIC;
                cmbBloodGroup.Text = donor.BloodGroup;
                txtRareBloodType.Text = donor.RareBloodType;
                txtPhone.Text = donor.Phone;
                txtEmail.Text = donor.Email;
                txtAddress.Text = donor.Address;
                txtCity.Text = donor.City;
                cmbGender.Text = donor.Gender;
                dtpDateOfBirth.Value = donor.DateOfBirth;
                numWeight.Value = donor.Weight;
                txtMedicalHistory.Text = donor.MedicalHistory;
                txtNotes.Text = donor.Notes;
                cmbStatus.Text = donor.Status;
                CalculateAge();

                ShowStatus("Donor data loaded for editing", Color.FromArgb(16, 185, 129));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        protected override void WndProc(ref Message m)
        {
            const int WM_MOUSEWHEEL = 0x020A;
            if (m.Msg == WM_MOUSEWHEEL && main != null)
            {
                int wParam = m.WParam.ToInt32();
                short delta = (short)((wParam >> 16) & 0xffff);

                Point screenPos = Cursor.Position;
                Point clientPos = this.PointToClient(screenPos);
                if (this.ClientRectangle.Contains(clientPos))
                {
                    int lines = SystemInformation.MouseWheelScrollLines;
                    int pixelsPerLine = 16;
                    int scrollAmount = (delta / 120) * lines * pixelsPerLine;

                    try
                    {
                        int newVal = main.VerticalScroll.Value - scrollAmount;
                        newVal = Math.Max(main.VerticalScroll.Minimum, Math.Min(main.VerticalScroll.Maximum, newVal));
                        main.VerticalScroll.Value = newVal;
                        main.PerformLayout();
                    }
                    catch
                    {
                        // ignore
                    }
                    return;
                }
            }
            base.WndProc(ref m);
        }
    }
}