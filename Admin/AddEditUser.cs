using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using BloodBankManagementSystem.Classes.Database;
using BloodBankManagementSystem.Classes.Enums;
using BloodBankManagementSystem.Classes.Models;
using BloodBankManagementSystem.Classes.Helpers;

namespace BloodBankManagementSystem.Forms.Admin
{
    public partial class AddEditUser : Form
    {
        private int _userId = 0;
        private bool _isEditMode = false;

        // Fields ko class level pe rakhein taake LoadUserData() access kar sake
        private TextBox txtFullName;
        private TextBox txtUsername;
        private TextBox txtPassword;
        private TextBox txtEmail;
        private TextBox txtPhone;
        private ComboBox cmbRole;

        public AddEditUser(int userId = 0)
        {
            InitializeComponent();
            _userId = userId;
            _isEditMode = userId > 0;
            BuildUI();
            if (_isEditMode)
            {
                LoadUserData();
                this.Text = "Edit User";
            }
            else
            {
                this.Text = "Add New User";
            }
        }

        private void BuildUI()
        {
            this.Size = new Size(560, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.White;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // ScrollablePanel - content clip na ho
            Panel mainPanel = new Panel
            {
                Location = new Point(0, 0),
                Size = new Size(560, 600),
                AutoScroll = true,
                BackColor = Color.White,
                Padding = new Padding(25, 15, 25, 15)
            };
            this.Controls.Add(mainPanel);

            int leftX = 25;
            int fieldWidth = 480;
            int y = 15;  // Title se shuru

            // ── Title ──────────────────────────────────────────
            Label lblTitle = new Label
            {
                Text = _isEditMode ? "✏️  Edit User" : "➕  Add New User",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.FromArgb(120, 22, 27),
                Location = new Point(leftX, y),
                Width = fieldWidth,
                Height = 45,
                TextAlign = ContentAlignment.MiddleCenter
            };
            mainPanel.Controls.Add(lblTitle);
            y += 55;  // Title ke neeche thoda space

            // ── Helper: ek field row banana ───────────────────
            // Returns the TextBox for later use
            TextBox AddField(string labelText, bool isPassword = false)
            {
                Label lbl = new Label
                {
                    Text = labelText,
                    Location = new Point(leftX, y),
                    Width = fieldWidth,
                    Height = 20,
                    Font = new Font("Segoe UI", 9, FontStyle.Bold),
                    ForeColor = Color.FromArgb(80, 80, 90)
                };
                mainPanel.Controls.Add(lbl);

                TextBox txt = new TextBox
                {
                    Location = new Point(leftX, y + 22),
                    Width = fieldWidth,
                    Height = 32,
                    Font = new Font("Segoe UI", 11),
                    BorderStyle = BorderStyle.FixedSingle,
                    BackColor = Color.FromArgb(250, 250, 252),
                    UseSystemPasswordChar = isPassword
                };
                mainPanel.Controls.Add(txt);

                y += 65;
                return txt;
            }

            // ── Fields ────────────────────────────────────────
            txtFullName = AddField("Full Name");
            txtUsername = AddField("Username");
            txtPassword = AddField("Password", isPassword: true);

            // Edit mode mein password field hide karein
            if (_isEditMode)
            {
                // Password label aur textbox dono visible = false
                // Lekin y already advance ho chuka — uski jagah wapas lo
                var controls = mainPanel.Controls;
                // Last 2 added controls password ke hain
                controls[controls.Count - 1].Visible = false; // TextBox
                controls[controls.Count - 2].Visible = false; // Label
                y -= 65; // Woh space wapas lo
            }

            txtEmail = AddField("Email");
            txtPhone = AddField("Phone");

            // ── Role ComboBox ──────────────────────────────────
            Label lblRole = new Label
            {
                Text = "Role",
                Location = new Point(leftX, y),
                Width = fieldWidth,
                Height = 20,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.FromArgb(80, 80, 90)
            };
            mainPanel.Controls.Add(lblRole);

            cmbRole = new ComboBox
            {
                Location = new Point(leftX, y + 22),
                Width = fieldWidth,
                Font = new Font("Segoe UI", 11),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(250, 250, 252)
            };
            cmbRole.Items.AddRange(new string[] { "Admin", "Manager", "Doctor", "Donor", "Patient", "Technician" });
            cmbRole.SelectedIndex = 0;
            mainPanel.Controls.Add(cmbRole);
            y += 70;

            // ── Buttons ───────────────────────────────────────
            y += 5; // thoda breathing room

            Button btnSave = new Button
            {
                Text = "💾  Save",
                Location = new Point(leftX + 100, y),
                Size = new Size(120, 42),
                BackColor = Color.FromArgb(120, 22, 27),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnSave.FlatAppearance.BorderSize = 0;
            mainPanel.Controls.Add(btnSave);

            Button btnCancel = new Button
            {
                Text = "❌  Cancel",
                Location = new Point(leftX + 240, y),
                Size = new Size(120, 42),
                BackColor = Color.FromArgb(220, 220, 225),
                ForeColor = Color.FromArgb(50, 50, 50),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11),
                Cursor = Cursors.Hand
            };
            btnCancel.FlatAppearance.BorderSize = 0;
            mainPanel.Controls.Add(btnCancel);

            // ── Form height adjust ────────────────────────────
            this.Height = y + 100; // content ke mutabiq height

            // ── Event Handlers ────────────────────────────────
            btnSave.Click += BtnSave_Click;
            btnCancel.Click += (s, e) => this.Close();
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtFullName.Text))
            {
                MessageBox.Show("Please enter full name.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtFullName.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtUsername.Text))
            {
                MessageBox.Show("Please enter username.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtUsername.Focus();
                return;
            }

            if (!_isEditMode && string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                MessageBox.Show("Please enter password.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtPassword.Focus();
                return;
            }

            User user = new User
            {
                FullName = txtFullName.Text.Trim(),
                Username = txtUsername.Text.Trim(),
                Role = (UserRole)Enum.Parse(typeof(UserRole), cmbRole.Text),
                Email = txtEmail.Text.Trim(),
                Phone = txtPhone.Text.Trim(),
                IsActive = true
            };

            bool success;
            if (_isEditMode)
            {
                user.UserID = _userId;
                success = AdminDAL.UpdateUser(user);
            }
            else
            {
                success = AdminDAL.InsertUser(user, txtPassword.Text);
            }

            if (success)
            {
                MessageBox.Show(
                    $"User {(_isEditMode ? "updated" : "added")} successfully!",
                    "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                MessageBox.Show("Failed to save user. Please try again.", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadUserData()
        {
            var user = AdminDAL.GetUserByID(_userId);
            if (user == null) return;

            // Ab seedha fields access karein (class-level variables)
            txtFullName.Text = user.FullName ?? "";
            txtUsername.Text = user.Username ?? "";
            txtEmail.Text = user.Email ?? "";
            txtPhone.Text = user.Phone ?? "";
            cmbRole.Text = user.Role.ToString();
        }
    }
}