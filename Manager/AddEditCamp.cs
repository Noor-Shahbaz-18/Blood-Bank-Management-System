using System;
using System.Drawing;
using System.Windows.Forms;
using BloodBankManagementSystem.Classes.Database;
using BloodBankManagementSystem.Classes.Helpers;

namespace BloodBankManagementSystem.Forms.Manager
{
    public partial class AddEditCamp : Form
    {
        private int _campId = 0;
        private bool _isEditMode = false;

        private TextBox txtCampName, txtLocation, txtCity, txtOrganizer, txtContact, txtTargetDonors, txtRemarks;
        private DateTimePicker dtpStartDate, dtpEndDate;
        private ComboBox cmbStatus;
        private Button btnSave, btnCancel;

        public AddEditCamp(int campId = 0)
        {
            _campId = campId;
            _isEditMode = campId > 0;
            this.Text = _isEditMode ? "✏️ Edit Donation Camp" : "🏕️ Schedule New Donation Camp";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.White;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Size = new Size(600, 650);

            BuildUI();
            if (_isEditMode) LoadCampData();
        }

        private void BuildUI()
        {
            Panel main = new Panel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = Color.White };
            this.Controls.Add(main);

            int leftX = 30;
            int labelW = 130;
            int fieldX = leftX + labelW + 10;
            int fieldW = 280;
            int y = 20;

            Label lblTitle = new Label
            {
                Text = _isEditMode ? "✏️ Edit Donation Camp" : "🏕️ Schedule New Donation Camp",
                Font = new Font("Segoe UI", 17, FontStyle.Bold),
                ForeColor = Color.FromArgb(120, 22, 27),
                Location = new Point(leftX, y),
                AutoSize = true
            };
            main.Controls.Add(lblTitle);
            y += 55;

            // Camp Name
            main.Controls.Add(new Label
            {
                Text = "Camp Name:",
                Location = new Point(leftX, y + 5),
                Size = new Size(labelW, 24),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(60, 60, 70)
            });
            txtCampName = new TextBox
            {
                Location = new Point(fieldX, y),
                Size = new Size(fieldW, 32),
                Font = new Font("Segoe UI", 11),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(250, 250, 252)
            };
            main.Controls.Add(txtCampName);
            y += 50;

            // Location
            main.Controls.Add(new Label
            {
                Text = "Location:",
                Location = new Point(leftX, y + 5),
                Size = new Size(labelW, 24),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(60, 60, 70)
            });
            txtLocation = new TextBox
            {
                Location = new Point(fieldX, y),
                Size = new Size(fieldW, 32),
                Font = new Font("Segoe UI", 11),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(250, 250, 252)
            };
            main.Controls.Add(txtLocation);
            y += 50;

            // City
            main.Controls.Add(new Label
            {
                Text = "City:",
                Location = new Point(leftX, y + 5),
                Size = new Size(labelW, 24),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(60, 60, 70)
            });
            txtCity = new TextBox
            {
                Location = new Point(fieldX, y),
                Size = new Size(fieldW, 32),
                Font = new Font("Segoe UI", 11),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(250, 250, 252)
            };
            main.Controls.Add(txtCity);
            y += 50;

            // Start Date
            main.Controls.Add(new Label
            {
                Text = "Start Date:",
                Location = new Point(leftX, y + 5),
                Size = new Size(labelW, 24),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(60, 60, 70)
            });
            dtpStartDate = new DateTimePicker
            {
                Location = new Point(fieldX, y),
                Size = new Size(180, 32),
                Format = DateTimePickerFormat.Short,
                Font = new Font("Segoe UI", 11),
                Value = DateTime.Now.AddDays(7)
            };
            main.Controls.Add(dtpStartDate);
            y += 50;

            // End Date
            main.Controls.Add(new Label
            {
                Text = "End Date:",
                Location = new Point(leftX, y + 5),
                Size = new Size(labelW, 24),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(60, 60, 70)
            });
            dtpEndDate = new DateTimePicker
            {
                Location = new Point(fieldX, y),
                Size = new Size(180, 32),
                Format = DateTimePickerFormat.Short,
                Font = new Font("Segoe UI", 11),
                Value = DateTime.Now.AddDays(8)
            };
            main.Controls.Add(dtpEndDate);
            y += 50;

            // Organizer
            main.Controls.Add(new Label
            {
                Text = "Organizer:",
                Location = new Point(leftX, y + 5),
                Size = new Size(labelW, 24),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(60, 60, 70)
            });
            txtOrganizer = new TextBox
            {
                Location = new Point(fieldX, y),
                Size = new Size(fieldW, 32),
                Font = new Font("Segoe UI", 11),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(250, 250, 252)
            };
            main.Controls.Add(txtOrganizer);
            y += 50;

            // Contact Number
            main.Controls.Add(new Label
            {
                Text = "Contact #:",
                Location = new Point(leftX, y + 5),
                Size = new Size(labelW, 24),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(60, 60, 70)
            });
            txtContact = new TextBox
            {
                Location = new Point(fieldX, y),
                Size = new Size(fieldW, 32),
                Font = new Font("Segoe UI", 11),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(250, 250, 252)
            };
            main.Controls.Add(txtContact);
            y += 50;

            // Target Donors
            main.Controls.Add(new Label
            {
                Text = "Target Donors:",
                Location = new Point(leftX, y + 5),
                Size = new Size(labelW, 24),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(60, 60, 70)
            });
            txtTargetDonors = new TextBox
            {
                Location = new Point(fieldX, y),
                Size = new Size(100, 32),
                Font = new Font("Segoe UI", 11),
                BorderStyle = BorderStyle.FixedSingle,
                Text = "100",
                BackColor = Color.FromArgb(250, 250, 252)
            };
            main.Controls.Add(txtTargetDonors);
            y += 50;

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
                Location = new Point(fieldX, y),
                Size = new Size(120, 32),
                Font = new Font("Segoe UI", 11),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(250, 250, 252)
            };
            cmbStatus.Items.AddRange(new[] { "Upcoming", "Ongoing", "Completed", "Cancelled" });
            cmbStatus.SelectedIndex = 0;
            main.Controls.Add(cmbStatus);
            y += 50;

            // Remarks
            main.Controls.Add(new Label
            {
                Text = "Remarks:",
                Location = new Point(leftX, y + 5),
                Size = new Size(labelW, 24),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(60, 60, 70)
            });
            txtRemarks = new TextBox
            {
                Location = new Point(fieldX, y),
                Size = new Size(fieldW, 60),
                Multiline = true,
                Font = new Font("Segoe UI", 11),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(250, 250, 252)
            };
            main.Controls.Add(txtRemarks);
            y += 75;

            y += 20;

            int totalW = 250;
            int btnX = (this.Width / 2) - (totalW / 2);

            btnSave = new Button
            {
                Text = "💾 Save Camp",
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
            if (string.IsNullOrWhiteSpace(txtCampName.Text))
            {
                MessageBox.Show("Please enter camp name.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!int.TryParse(txtTargetDonors.Text, out int target))
            {
                MessageBox.Show("Please enter valid target donors number.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                bool success;
                if (_isEditMode)
                {
                    success = DonationCampDAL.UpdateCamp(
                        _campId,
                        txtCampName.Text.Trim(),
                        txtLocation.Text.Trim(),
                        txtCity.Text.Trim(),
                        dtpStartDate.Value,
                        dtpEndDate.Value,
                        txtOrganizer.Text.Trim(),
                        txtContact.Text.Trim(),
                        target,
                        cmbStatus.SelectedItem?.ToString(),
                        txtRemarks.Text.Trim()
                    );
                }
                else
                {
                    success = DonationCampDAL.InsertCamp(
                        txtCampName.Text.Trim(),
                        txtLocation.Text.Trim(),
                        txtCity.Text.Trim(),
                        dtpStartDate.Value,
                        dtpEndDate.Value,
                        txtOrganizer.Text.Trim(),
                        txtContact.Text.Trim(),
                        target,
                        SessionManager.CurrentUserID
                    );
                }

                if (success)
                {
                    MessageBox.Show($"Donation camp {(_isEditMode ? "updated" : "scheduled")} successfully!", "Success",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Failed to save camp.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadCampData()
        {
            try
            {
                var row = DonationCampDAL.GetCampByID(_campId);
                if (row == null) return;

                txtCampName.Text = row["CampName"]?.ToString();
                txtLocation.Text = row["Location"]?.ToString();
                txtCity.Text = row["City"]?.ToString();
                dtpStartDate.Value = Convert.ToDateTime(row["StartDate"]);
                dtpEndDate.Value = Convert.ToDateTime(row["EndDate"]);
                txtOrganizer.Text = row["Organizer"]?.ToString();
                txtContact.Text = row["ContactNumber"]?.ToString();
                txtTargetDonors.Text = row["TargetDonors"]?.ToString();
                cmbStatus.Text = row["Status"]?.ToString();
                txtRemarks.Text = row["Remarks"]?.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}