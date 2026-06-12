using System;
using System.Drawing;
using System.Windows.Forms;
using BloodBankManagementSystem.Classes.Database;
using BloodBankManagementSystem.Classes.Helpers;
using TechnicianModel = BloodBankManagementSystem.Classes.Models.Technician;
using BloodBankManagementSystem.Classes.Models;
using BloodBankManagementSystem.Classes.Enums;

namespace BloodBankManagementSystem.Forms.Manager
{
    public partial class AddEditTechnician : Form
    {
        private int _userId = 0;
        private int _technicianId = 0;
        private bool _isEditMode = false;

        private TextBox txtFullName, txtUsername, txtEmail, txtPhone, txtEmployeeID, txtDesignation, txtSpecialization, txtQualification, txtCertification, txtWorkingLocation, txtNotes;
        private ComboBox cmbShift, cmbStatus;
        private NumericUpDown numExperience;
        private DateTimePicker dtpJoiningDate;
        private Button btnSave, btnCancel;

        public AddEditTechnician(int technicianId = 0)
        {
            _technicianId = technicianId;
            _isEditMode = technicianId > 0;
            this.Text = _isEditMode ? "✏️ Edit Technician" : "👨‍🔬 Add New Technician";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.White;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Size = new Size(950, 700);

            BuildUI();
            if (_isEditMode) LoadTechnicianData();
        }

        private void BuildUI()
        {
            Panel main = new Panel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = Color.White };
            this.Controls.Add(main);

            int leftX = 30;
            int labelW = 145;
            int fieldX = leftX + labelW + 10;
            int fieldW = 300;
            int y = 20;

            Label lblTitle = new Label
            {
                Text = _isEditMode ? "✏️ Edit Technician" : "👨‍🔬 Add New Technician",
                Font = new Font("Segoe UI", 17, FontStyle.Bold),
                ForeColor = Color.FromArgb(120, 22, 27),
                Location = new Point(leftX, y),
                AutoSize = true
            };
            main.Controls.Add(lblTitle);
            y += 55;

            // Full Name
            main.Controls.Add(new Label
            {
                Text = "Full Name:",
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

            // Username
            main.Controls.Add(new Label
            {
                Text = "Username:",
                Location = new Point(leftX, y + 5),
                Size = new Size(labelW, 24),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(60, 60, 70)
            });
            txtUsername = new TextBox
            {
                Location = new Point(fieldX, y),
                Size = new Size(fieldW, 32),
                Font = new Font("Segoe UI", 11),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(250, 250, 252)
            };
            main.Controls.Add(txtUsername);
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

            // Phone
            main.Controls.Add(new Label
            {
                Text = "Phone:",
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

            // Employee ID
            main.Controls.Add(new Label
            {
                Text = "Employee ID:",
                Location = new Point(leftX, y + 5),
                Size = new Size(labelW, 24),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(60, 60, 70)
            });
            txtEmployeeID = new TextBox
            {
                Location = new Point(fieldX, y),
                Size = new Size(fieldW, 32),
                Font = new Font("Segoe UI", 11),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(250, 250, 252)
            };
            main.Controls.Add(txtEmployeeID);
            y += 50;

            // Designation
            main.Controls.Add(new Label
            {
                Text = "Designation:",
                Location = new Point(leftX, y + 5),
                Size = new Size(labelW, 24),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(60, 60, 70)
            });
            txtDesignation = new TextBox
            {
                Location = new Point(fieldX, y),
                Size = new Size(fieldW, 32),
                Font = new Font("Segoe UI", 11),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(250, 250, 252)
            };
            main.Controls.Add(txtDesignation);
            y += 50;

            // Right column - second column
            int rightX = leftX + labelW + 10 + fieldW + 40;
            int ry = 75;

            // Specialization
            main.Controls.Add(new Label
            {
                Text = "Specialization:",
                Location = new Point(rightX, ry + 5),
                Size = new Size(labelW, 24),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(60, 60, 70)
            });
            txtSpecialization = new TextBox
            {
                Location = new Point(rightX + labelW, ry),
                Size = new Size(fieldW, 32),
                Font = new Font("Segoe UI", 11),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(250, 250, 252)
            };
            main.Controls.Add(txtSpecialization);
            ry += 50;

            // Qualification
            main.Controls.Add(new Label
            {
                Text = "Qualification:",
                Location = new Point(rightX, ry + 5),
                Size = new Size(labelW, 24),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(60, 60, 70)
            });
            txtQualification = new TextBox
            {
                Location = new Point(rightX + labelW, ry),
                Size = new Size(fieldW, 32),
                Font = new Font("Segoe UI", 11),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(250, 250, 252)
            };
            main.Controls.Add(txtQualification);
            ry += 50;

            // Experience
            main.Controls.Add(new Label
            {
                Text = "Experience (years):",
                Location = new Point(rightX, ry + 5),
                Size = new Size(labelW, 24),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(60, 60, 70)
            });
            numExperience = new NumericUpDown
            {
                Location = new Point(rightX + labelW, ry),
                Size = new Size(80, 32),
                Minimum = 0,
                Maximum = 50,
                Value = 0,
                Font = new Font("Segoe UI", 11)
            };
            main.Controls.Add(numExperience);
            ry += 50;

            // Certification Number
            main.Controls.Add(new Label
            {
                Text = "Certification #:",
                Location = new Point(rightX, ry + 5),
                Size = new Size(labelW, 24),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(60, 60, 70)
            });
            txtCertification = new TextBox
            {
                Location = new Point(rightX + labelW, ry),
                Size = new Size(fieldW, 32),
                Font = new Font("Segoe UI", 11),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(250, 250, 252)
            };
            main.Controls.Add(txtCertification);
            ry += 50;

            // Working Location
            main.Controls.Add(new Label
            {
                Text = "Working Location:",
                Location = new Point(rightX, ry + 5),
                Size = new Size(labelW, 24),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(60, 60, 70)
            });
            txtWorkingLocation = new TextBox
            {
                Location = new Point(rightX + labelW, ry),
                Size = new Size(fieldW, 32),
                Font = new Font("Segoe UI", 11),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(250, 250, 252)
            };
            main.Controls.Add(txtWorkingLocation);
            ry += 50;

            // Shift
            main.Controls.Add(new Label
            {
                Text = "Shift:",
                Location = new Point(rightX, ry + 5),
                Size = new Size(labelW, 24),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(60, 60, 70)
            });
            cmbShift = new ComboBox
            {
                Location = new Point(rightX + labelW, ry),
                Size = new Size(120, 32),
                Font = new Font("Segoe UI", 11),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(250, 250, 252)
            };
            cmbShift.Items.AddRange(new[] { "Morning", "Evening", "Night" });
            cmbShift.SelectedIndex = 0;
            main.Controls.Add(cmbShift);
            ry += 50;

            // Joining Date
            main.Controls.Add(new Label
            {
                Text = "Joining Date:",
                Location = new Point(rightX, ry + 5),
                Size = new Size(labelW, 24),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(60, 60, 70)
            });
            dtpJoiningDate = new DateTimePicker
            {
                Location = new Point(rightX + labelW, ry),
                Size = new Size(140, 32),
                Format = DateTimePickerFormat.Short,
                Font = new Font("Segoe UI", 11),
                Value = DateTime.Now
            };
            main.Controls.Add(dtpJoiningDate);
            ry += 50;

            // Notes - full width
            y = Math.Max(y, ry) + 20;

            main.Controls.Add(new Label
            {
                Text = "Notes:",
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

            int totalW = 250;
            int btnX = (this.Width / 2) - (totalW / 2);

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
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtFullName.Text))
            {
                MessageBox.Show("Please enter full name.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                if (!_isEditMode)
                {
                    var user = new User
                    {
                        FullName = txtFullName.Text.Trim(),
                        Username = txtUsername.Text.Trim(),
                        Role = UserRole.Technician,
                        Email = txtEmail.Text.Trim(),
                        Phone = txtPhone.Text.Trim(),
                        IsActive = cmbStatus.SelectedItem?.ToString() == "Active"
                    };

                    string defaultPassword = "Tech@123";
                    bool userCreated = UserDAL.Insert(user, defaultPassword);

                    if (!userCreated)
                    {
                        MessageBox.Show("Failed to create user account.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    _userId = user.UserID;
                }

                var technician = new TechnicianModel
                {
                    UserID = _userId,
                    FullName = txtFullName.Text.Trim(),
                    EmployeeID = txtEmployeeID.Text.Trim(),
                    Designation = txtDesignation.Text.Trim(),
                    Specialization = txtSpecialization.Text.Trim(),
                    Qualification = txtQualification.Text.Trim(),
                    YearsOfExperience = (int)numExperience.Value,
                    Phone = txtPhone.Text.Trim(),
                    Email = txtEmail.Text.Trim(),
                    Shift = cmbShift.SelectedItem?.ToString(),
                    JoiningDate = dtpJoiningDate.Value,
                    CertificationNumber = txtCertification.Text.Trim(),
                    WorkingLocation = txtWorkingLocation.Text.Trim(),
                    IsActive = cmbStatus.SelectedItem?.ToString() == "Active",
                    Notes = txtNotes.Text.Trim()
                };

                bool success;
                if (_isEditMode)
                {
                    technician.TechnicianID = _technicianId;
                    success = TechnicianDAL.Update(technician);

                    var user = UserDAL.GetByID(_userId);
                    if (user != null)
                    {
                        user.FullName = txtFullName.Text.Trim();
                        user.Email = txtEmail.Text.Trim();
                        user.Phone = txtPhone.Text.Trim();
                        user.IsActive = cmbStatus.SelectedItem?.ToString() == "Active";
                        UserDAL.Update(user);
                    }
                }
                else
                {
                    success = TechnicianDAL.Insert(technician);
                }

                if (success)
                {
                    MessageBox.Show($"Technician {(_isEditMode ? "updated" : "added")} successfully!", "Success",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Failed to save technician.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadTechnicianData()
        {
            try
            {
                var tech = TechnicianDAL.GetByTechnicianID(_technicianId);
                if (tech == null) return;

                _userId = tech.UserID;
                txtFullName.Text = tech.FullName;
                txtEmployeeID.Text = tech.EmployeeID;
                txtDesignation.Text = tech.Designation;
                txtSpecialization.Text = tech.Specialization;
                txtQualification.Text = tech.Qualification;
                numExperience.Value = tech.YearsOfExperience;
                txtPhone.Text = tech.Phone;
                txtEmail.Text = tech.Email;
                cmbShift.Text = tech.Shift;
                dtpJoiningDate.Value = tech.JoiningDate;
                txtCertification.Text = tech.CertificationNumber;
                txtWorkingLocation.Text = tech.WorkingLocation;
                cmbStatus.Text = tech.IsActive ? "Active" : "Inactive";
                txtNotes.Text = tech.Notes;

                var user = UserDAL.GetByID(_userId);
                if (user != null)
                {
                    txtUsername.Text = user.Username;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}