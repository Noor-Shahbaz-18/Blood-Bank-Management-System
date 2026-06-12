using System;
using System.Drawing;
using System.Windows.Forms;
using BloodBankManagementSystem.Classes.Database;
using BloodBankManagementSystem.Classes.Enums;
using BloodBankManagementSystem.Classes.Models;
using DonorModel = BloodBankManagementSystem.Classes.Models.Donor;
using BloodBankManagementSystem.Classes.Helpers;

namespace BloodBankManagementSystem.Forms.Donor
{
    public partial class DonorRegistration : Form
    {
        private TextBox txtFullName, txtCNIC, txtAge, txtWeight, txtPhone, txtEmail,
                        txtAddress, txtEmergencyContact, txtUsername, txtPassword, txtConfirmPassword;
        private ComboBox cmbGender, cmbBloodGroup, cmbCity;
        private DateTimePicker dtpDOB;

        private string[] cities = new string[]
        {
            "Select City", "Lahore", "Karachi", "Islamabad", "Rawalpindi",
            "Faisalabad", "Multan", "Gujranwala", "Peshawar", "Quetta",
            "Sialkot", "Bahawalpur", "Sargodha", "Sukkur", "Larkana",
            "Mardan", "Gujrat", "Rahim Yar Khan", "Jhang", "Dera Ghazi Khan", "Other"
        };

        public DonorRegistration()
        {
            InitializeComponent();
            BuildUI();
        }

        private void BuildUI()
        {
            this.Text = "Donor Registration - Blood Bank";
            this.Size = new Size(800, 780);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.White;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            // Use a scrollable panel — NO Dock on children except the panel itself
            Panel mainPanel = new Panel
            {
                Location = new Point(0, 0),
                Size = new Size(800, 740),
                AutoScroll = true,
                BackColor = Color.White
            };
            this.Controls.Add(mainPanel);

            // ── TITLE (fixed position, NOT Dock) ──────────────────────────
            Label lblTitle = new Label
            {
                Text = "🩸 Donor Registration",
                Font = new Font("Segoe UI", 22, FontStyle.Bold),
                ForeColor = Color.FromArgb(120, 22, 27),
                Location = new Point(0, 10),
                Size = new Size(780, 50),
                TextAlign = ContentAlignment.MiddleCenter
            };
            mainPanel.Controls.Add(lblTitle);

            // ── COLUMN SETTINGS ───────────────────────────────────────────
            int leftX = 30;
            int rightX = 415;
            int fldW = 330;
            int rowGap = 60;
            int leftY = 75;    // starts BELOW title
            int rightY = 75;

            // ══════════════════════════════════════════════════════════════
            // LEFT COLUMN
            // ══════════════════════════════════════════════════════════════
            AddField(mainPanel, "Full Name:", ref leftY, leftX, fldW, out txtFullName);
            leftY += rowGap;

            AddField(mainPanel, "CNIC (xxxxx-xxxxxxx-x):", ref leftY, leftX, fldW, out txtCNIC);
            leftY += rowGap;

            AddDateField(mainPanel, "Date of Birth:", ref leftY, leftX, fldW, out dtpDOB);
            leftY += rowGap;

            AddField(mainPanel, "Age:", ref leftY, leftX, fldW, out txtAge, readOnly: true);
            leftY += rowGap;

            AddComboField(mainPanel, "Gender:", ref leftY, leftX, fldW, out cmbGender,
                new string[] { "Male", "Female", "Other" });
            leftY += rowGap;

            AddComboField(mainPanel, "Blood Group:", ref leftY, leftX, fldW, out cmbBloodGroup,
                new string[] { "A+", "A-", "B+", "B-", "AB+", "AB-", "O+", "O-" });
            leftY += rowGap;

            AddField(mainPanel, "Password:", ref leftY, leftX, fldW, out txtPassword, isPassword: true);
            leftY += rowGap;

            // ══════════════════════════════════════════════════════════════
            // RIGHT COLUMN
            // ══════════════════════════════════════════════════════════════
            AddField(mainPanel, "Weight (kg):", ref rightY, rightX, fldW, out txtWeight);
            rightY += rowGap;

            AddField(mainPanel, "Phone:", ref rightY, rightX, fldW, out txtPhone);
            rightY += rowGap;

            AddField(mainPanel, "Email:", ref rightY, rightX, fldW, out txtEmail);
            rightY += rowGap;

            AddComboField(mainPanel, "City:", ref rightY, rightX, fldW, out cmbCity, cities);
            rightY += rowGap;

            AddField(mainPanel, "Emergency Contact:", ref rightY, rightX, fldW, out txtEmergencyContact);
            rightY += rowGap;

            AddField(mainPanel, "Username:", ref rightY, rightX, fldW, out txtUsername);
            rightY += rowGap;

            AddField(mainPanel, "Confirm Password:", ref rightY, rightX, fldW, out txtConfirmPassword, isPassword: true);
            rightY += rowGap;

            // ══════════════════════════════════════════════════════════════
            // FULL WIDTH — Address
            // ══════════════════════════════════════════════════════════════
            int belowY = Math.Max(leftY, rightY) + 10;
            AddField(mainPanel, "Address:", ref belowY, leftX, 715, out txtAddress);
            belowY += 65;

            // ── City info label ───────────────────────────────────────────
            Label lblCityInfo = new Label
            {
                Text = "ℹ️ Your city helps us find nearby donation camps for you.",
                Font = new Font("Segoe UI", 8, FontStyle.Italic),
                ForeColor = Color.Gray,
                Location = new Point(leftX, belowY),
                AutoSize = true
            };
            mainPanel.Controls.Add(lblCityInfo);
            belowY += 28;

            // ── Register Button ───────────────────────────────────────────
            Button btnRegister = new Button
            {
                Text = "📝 Register",
                Location = new Point(305, belowY),
                Size = new Size(190, 46),
                BackColor = Color.FromArgb(120, 22, 27),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnRegister.FlatAppearance.BorderSize = 0;
            mainPanel.Controls.Add(btnRegister);

            // ── EVENTS ────────────────────────────────────────────────────
            dtpDOB.ValueChanged += (s, e) =>
                txtAge.Text = DateTimeHelper.CalculateAge(dtpDOB.Value).ToString();

            txtAge.Text = DateTimeHelper.CalculateAge(dtpDOB.Value).ToString();

            // ── REGISTER CLICK ────────────────────────────────────────────
            btnRegister.Click += (sender, e) =>
            {
                if (string.IsNullOrWhiteSpace(txtFullName.Text))
                { MessageBox.Show("Please enter full name.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning); txtFullName.Focus(); return; }

                if (!Validator.IsValidCNIC(txtCNIC.Text))
                { MessageBox.Show("Please enter valid CNIC (xxxxx-xxxxxxx-x).", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning); txtCNIC.Focus(); return; }

                if (cmbCity.SelectedIndex == 0 || string.IsNullOrWhiteSpace(cmbCity.Text))
                { MessageBox.Show("Please select your city.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning); cmbCity.Focus(); return; }

                if (string.IsNullOrWhiteSpace(txtUsername.Text))
                { MessageBox.Show("Please enter username.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning); txtUsername.Focus(); return; }

                if (txtPassword.Text.Length < 6)
                { MessageBox.Show("Password must be at least 6 characters.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning); txtPassword.Focus(); return; }

                if (txtPassword.Text != txtConfirmPassword.Text)
                { MessageBox.Show("Passwords do not match.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning); txtConfirmPassword.Focus(); return; }

                User user = new User
                {
                    FullName = txtFullName.Text.Trim(),
                    Username = txtUsername.Text.Trim(),
                    Role = UserRole.Donor,
                    Email = txtEmail.Text.Trim(),
                    Phone = txtPhone.Text.Trim(),
                    IsActive = true
                };

                bool userSaved = UserDAL.Insert(user, txtPassword.Text);
                if (!userSaved)
                { MessageBox.Show("Username already exists. Please choose another.", "Registration Failed", MessageBoxButtons.OK, MessageBoxIcon.Error); return; }

                var newUser = UserDAL.GetByUsername(txtUsername.Text);
                if (newUser == null)
                { MessageBox.Show("Registration failed. Please try again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); return; }

                string fullAddress = txtAddress.Text.Trim();
                string selectedCity = cmbCity.SelectedItem?.ToString();

                if (selectedCity == "Other")
                {
                    string customCity = Microsoft.VisualBasic.Interaction.InputBox(
                        "Please enter your city name:", "City Name", "", -1, -1);
                    selectedCity = string.IsNullOrWhiteSpace(customCity) ? "" : customCity.Trim();
                }

                if (!string.IsNullOrEmpty(selectedCity) && selectedCity != "Select City")
                    fullAddress = string.IsNullOrEmpty(fullAddress) ? selectedCity : fullAddress + ", " + selectedCity;

                DonorModel donor = new DonorModel
                {
                    UserID = newUser.UserID,
                    FullName = txtFullName.Text.Trim(),
                    CNIC = txtCNIC.Text.Trim(),
                    BloodGroup = cmbBloodGroup.Text,
                    DateOfBirth = dtpDOB.Value,
                    Age = Convert.ToInt32(txtAge.Text),
                    Gender = cmbGender.Text,
                    Address = fullAddress,
                    Phone = txtPhone.Text.Trim(),
                    Email = txtEmail.Text.Trim(),
                    Weight = Convert.ToInt32(txtWeight.Text),
                    IsActive = true,
                    RegistrationDate = DateTime.Now,
                    EmergencyContact = txtEmergencyContact.Text.Trim(),
                    MedicalHistory = null
                };

                bool donorSaved = DonorDAL.Insert(donor);
                if (donorSaved)
                {
                    MessageBox.Show(
                        $"✅ Registration successful!\n\nWelcome {txtFullName.Text}!\nCity: {selectedCity}\n\nYou can now login and find nearby donation camps.",
                        "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                    MessageBox.Show("Registration failed. Please try again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            };
        }

        // ── HELPER METHODS ────────────────────────────────────────────────────

        private void AddField(Panel parent, string label, ref int y, int x, int width,
                              out TextBox textBox, bool readOnly = false, bool isPassword = false)
        {
            parent.Controls.Add(new Label
            {
                Text = label,
                Location = new Point(x, y),
                AutoSize = true,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.FromArgb(80, 80, 90)
            });
            textBox = new TextBox
            {
                Location = new Point(x, y + 22),
                Width = width,
                Font = new Font("Segoe UI", 10),
                BorderStyle = BorderStyle.FixedSingle,
                ReadOnly = readOnly
            };
            if (isPassword) textBox.UseSystemPasswordChar = true;
            parent.Controls.Add(textBox);
        }

        private void AddComboField(Panel parent, string label, ref int y, int x, int width,
                                   out ComboBox comboBox, string[] items)
        {
            parent.Controls.Add(new Label
            {
                Text = label,
                Location = new Point(x, y),
                AutoSize = true,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.FromArgb(80, 80, 90)
            });
            comboBox = new ComboBox
            {
                Location = new Point(x, y + 22),
                Width = width,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10)
            };
            comboBox.Items.AddRange(items);
            if (items.Length > 0) comboBox.SelectedIndex = 0;
            parent.Controls.Add(comboBox);
        }

        private void AddDateField(Panel parent, string label, ref int y, int x, int width,
                                  out DateTimePicker dtp)
        {
            parent.Controls.Add(new Label
            {
                Text = label,
                Location = new Point(x, y),
                AutoSize = true,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.FromArgb(80, 80, 90)
            });
            dtp = new DateTimePicker
            {
                Location = new Point(x, y + 22),
                Width = width,
                Format = DateTimePickerFormat.Short,
                Font = new Font("Segoe UI", 10),
                MaxDate = DateTime.Today.AddYears(-18),
                Value = DateTime.Today.AddYears(-25)
            };
            parent.Controls.Add(dtp);
        }
    }
}