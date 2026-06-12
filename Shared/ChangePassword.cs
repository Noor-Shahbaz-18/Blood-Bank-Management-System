using System;
using System.Drawing;
using System.Windows.Forms;
using BloodBankManagementSystem.Classes.Database;
using BloodBankManagementSystem.Classes.Helpers;

namespace BloodBankManagementSystem.Forms.Shared
{
    public partial class ChangePassword : Form
    {
        private int _userId;
        private string _currentPasswordHash;

        public ChangePassword(int userId, string currentPasswordHash)
        {
            _userId = userId;
            _currentPasswordHash = currentPasswordHash;
            InitializeComponent();
            BuildUI();
        }

        private void BuildUI()
        {
            this.Text = "Change Password";
            this.Size = new Size(450, 350);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.White;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            Panel mainPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(25) };
            this.Controls.Add(mainPanel);

            // Title
            Label lblTitle = new Label
            {
                Text = "🔒 Change Password",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.FromArgb(120, 22, 27),
                Dock = DockStyle.Top,
                Height = 45,
                TextAlign = ContentAlignment.MiddleCenter
            };
            mainPanel.Controls.Add(lblTitle);

            int y = 70, leftX = 20, fieldWidth = 350;

            // Current Password
            mainPanel.Controls.Add(new Label { Text = "Current Password:", Location = new Point(leftX, y), AutoSize = true, Font = new Font("Segoe UI", 10) });
            TextBox txtCurrentPassword = new TextBox
            {
                Location = new Point(leftX, y + 25),
                Width = fieldWidth,
                Font = new Font("Segoe UI", 11),
                UseSystemPasswordChar = true,
                BorderStyle = BorderStyle.FixedSingle
            };
            mainPanel.Controls.Add(txtCurrentPassword);
            y += 70;

            // New Password
            mainPanel.Controls.Add(new Label { Text = "New Password:", Location = new Point(leftX, y), AutoSize = true, Font = new Font("Segoe UI", 10) });
            TextBox txtNewPassword = new TextBox
            {
                Location = new Point(leftX, y + 25),
                Width = fieldWidth,
                Font = new Font("Segoe UI", 11),
                UseSystemPasswordChar = true,
                BorderStyle = BorderStyle.FixedSingle
            };
            mainPanel.Controls.Add(txtNewPassword);
            y += 70;

            // Confirm Password
            mainPanel.Controls.Add(new Label { Text = "Confirm Password:", Location = new Point(leftX, y), AutoSize = true, Font = new Font("Segoe UI", 10) });
            TextBox txtConfirmPassword = new TextBox
            {
                Location = new Point(leftX, y + 25),
                Width = fieldWidth,
                Font = new Font("Segoe UI", 11),
                UseSystemPasswordChar = true,
                BorderStyle = BorderStyle.FixedSingle
            };
            mainPanel.Controls.Add(txtConfirmPassword);
            y += 75;

            // Buttons
            Button btnSave = new Button
            {
                Text = "✓ Update Password",
                Location = new Point(leftX + 60, y),
                Size = new Size(130, 40),
                BackColor = Color.FromArgb(120, 22, 27),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnSave.FlatAppearance.BorderSize = 0;
            mainPanel.Controls.Add(btnSave);

            Button btnCancel = new Button
            {
                Text = "✗ Cancel",
                Location = new Point(leftX + 210, y),
                Size = new Size(100, 40),
                BackColor = Color.LightGray,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnCancel.Click += (s, e) => this.Close();
            mainPanel.Controls.Add(btnCancel);

            btnSave.Click += (s, e) =>
            {
                string currentPassword = txtCurrentPassword.Text;
                string newPassword = txtNewPassword.Text;
                string confirmPassword = txtConfirmPassword.Text;

                // Validation
                if (string.IsNullOrEmpty(currentPassword))
                {
                    MessageBox.Show("Please enter current password.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtCurrentPassword.Focus();
                    return;
                }

                if (!EncryptionHelper.Verify(currentPassword, _currentPasswordHash))
                {
                    MessageBox.Show("Current password is incorrect.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtCurrentPassword.Focus();
                    return;
                }

                if (string.IsNullOrEmpty(newPassword))
                {
                    MessageBox.Show("Please enter new password.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtNewPassword.Focus();
                    return;
                }

                if (newPassword.Length < 6)
                {
                    MessageBox.Show("New password must be at least 6 characters.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtNewPassword.Focus();
                    return;
                }

                if (newPassword != confirmPassword)
                {
                    MessageBox.Show("New password and confirm password do not match.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtConfirmPassword.Focus();
                    return;
                }

                // Update password
                bool success = AdminDAL.ChangePassword(_userId, newPassword);

                if (success)
                {
                    MessageBox.Show("Password changed successfully! Please login again.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Failed to change password. Please try again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };
        }
    }
}