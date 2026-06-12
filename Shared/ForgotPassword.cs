using BloodBankManagementSystem.Classes.BusinessLogic;
using BloodBankManagementSystem.Classes.Database;
using BloodBankManagementSystem.Classes.Helpers;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace BloodBankManagementSystem.Forms.Shared
{
    public partial class ForgotPassword : Form
    {
        public ForgotPassword()
        {
            InitializeComponent();
            BuildUI();
        }

        private void BuildUI()
        {
            this.Text = "Forgot Password - Blood Bank";
            this.Size = new Size(500, 480);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.White;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            Panel mainPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(30) };
            this.Controls.Add(mainPanel);

            // Icon
            Label lblIcon = new Label
            {
                Text = "🔐",
                Font = new Font("Segoe UI Emoji", 48),
                ForeColor = Color.FromArgb(120, 22, 27),
                Dock = DockStyle.Top,
                Height = 80,
                TextAlign = ContentAlignment.MiddleCenter
            };
            mainPanel.Controls.Add(lblIcon);

            // Title
            Label lblTitle = new Label
            {
                Text = "Forgot Password?",
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                ForeColor = Color.FromArgb(120, 22, 27),
                Dock = DockStyle.Top,
                Height = 45,
                TextAlign = ContentAlignment.MiddleCenter
            };
            mainPanel.Controls.Add(lblTitle);

            // Description
            Label lblDesc = new Label
            {
                Text = "Enter your registered email address and we'll send you\na new password.",
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.Gray,
                Dock = DockStyle.Top,
                Height = 55,
                TextAlign = ContentAlignment.MiddleCenter
            };
            mainPanel.Controls.Add(lblDesc);

            int y = 210, leftX = 40, fieldWidth = 360;
            Label lblStatus = new Label();

            // Email
            mainPanel.Controls.Add(new Label
            {
                Text = "Email Address:",
                Location = new Point(leftX, y),
                AutoSize = true,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            });
            TextBox txtEmail = new TextBox
            {
                Location = new Point(leftX, y + 28),
                Width = fieldWidth,
                Font = new Font("Segoe UI", 11),
                BorderStyle = BorderStyle.FixedSingle
            };
            mainPanel.Controls.Add(txtEmail);
            y += 80;

            // Username (Optional)
            mainPanel.Controls.Add(new Label
            {
                Text = "Username (Optional):",
                Location = new Point(leftX, y),
                AutoSize = true,
                Font = new Font("Segoe UI", 10)
            });
            TextBox txtUsername = new TextBox
            {
                Location = new Point(leftX, y + 28),
                Width = fieldWidth,
                Font = new Font("Segoe UI", 11),
                BorderStyle = BorderStyle.FixedSingle
            };
            mainPanel.Controls.Add(txtUsername);
            y += 80;

            // Status Label
            lblStatus = new Label
            {
                Text = "",
                Location = new Point(leftX, y),
                AutoSize = true,
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.Gray
            };
            mainPanel.Controls.Add(lblStatus);
            y += 35;

            // Reset Button
            Button btnReset = new Button
            {
                Text = "📧 Send New Password",
                Location = new Point(leftX + 80, y),
                Size = new Size(200, 45),
                BackColor = Color.FromArgb(120, 22, 27),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnReset.FlatAppearance.BorderSize = 0;
            mainPanel.Controls.Add(btnReset);

            // Back to Login Link
            LinkLabel lnkBack = new LinkLabel
            {
                Text = "← Back to Login",
                Location = new Point(leftX + 150, y + 60),
                AutoSize = true,
                Font = new Font("Segoe UI", 9),
                LinkColor = Color.FromArgb(120, 22, 27)
            };
            lnkBack.Click += (s, e) => this.Close();
            mainPanel.Controls.Add(lnkBack);

            // Button Click Event
            btnReset.Click += async (s, e) =>
            {
                string email = txtEmail.Text.Trim();
                string username = txtUsername.Text.Trim();

                if (string.IsNullOrEmpty(email))
                {
                    lblStatus.Text = "❌ Please enter your email address";
                    lblStatus.ForeColor = Color.Red;
                    txtEmail.Focus();
                    return;
                }

                btnReset.Enabled = false;
                btnReset.Text = "⏳ Sending...";
                lblStatus.Text = "⏳ Checking records...";
                lblStatus.ForeColor = Color.Orange;
                Application.DoEvents();

                try
                {
                    // Find user by email
                    var user = GetUserByEmail(email);

                    if (user == null)
                    {
                        lblStatus.Text = "❌ No account found with this email address";
                        lblStatus.ForeColor = Color.Red;
                        btnReset.Enabled = true;
                        btnReset.Text = "📧 Send New Password";
                        return;
                    }

                    // Verify username if provided
                    if (!string.IsNullOrEmpty(username) && user.Username.ToLower() != username.ToLower())
                    {
                        lblStatus.Text = "❌ Username does not match our records";
                        lblStatus.ForeColor = Color.Red;
                        btnReset.Enabled = true;
                        btnReset.Text = "📧 Send New Password";
                        return;
                    }

                    // Generate new random password
                    string newPassword = GenerateRandomPassword(8);

                    // Update password in database
                    bool passwordUpdated = AdminDAL.ChangePassword(user.UserID, newPassword);

                    if (passwordUpdated)
                    {
                        // Prepare email body
                        string subject = "🩸 Blood Bank - Password Reset";
                        string body = $@"
                            <html>
                            <head>
                                <style>
                                    body {{ font-family: 'Segoe UI', Arial, sans-serif; }}
                                    .container {{ max-width: 550px; margin: auto; padding: 20px; border: 1px solid #ddd; border-radius: 10px; }}
                                    .header {{ background-color: #78161b; color: white; padding: 20px; text-align: center; border-radius: 8px 8px 0 0; }}
                                    .content {{ padding: 25px; }}
                                    .password-box {{ background-color: #f5f5f5; padding: 15px; text-align: center; border-radius: 8px; margin: 15px 0; }}
                                    .password {{ font-size: 24px; font-weight: bold; color: #78161b; letter-spacing: 2px; }}
                                    .footer {{ background-color: #f0f0f0; padding: 12px; text-align: center; font-size: 11px; border-radius: 0 0 8px 8px; color: #666; }}
                                    .note {{ font-size: 12px; color: #d9534f; }}
                                </style>
                            </head>
                            <body>
                                <div class='container'>
                                    <div class='header'>
                                        <h2>🩸 Blood Bank Management System</h2>
                                    </div>
                                    <div class='content'>
                                        <h3>Dear {user.FullName},</h3>
                                        <p>We received a request to reset your password.</p>
                                        <div class='password-box'>
                                            <p style='margin: 0; color: #666;'>Your New Password:</p>
                                            <p class='password'>{newPassword}</p>
                                        </div>
                                        <p><strong>Username:</strong> {user.Username}</p>
                                        <p class='note'>⚠️ For security reasons, please change this password after logging in.</p>
                                        <br/>
                                        <p>If you didn't request this, please ignore this email or contact support.</p>
                                        <br/>
                                        <p>Thank you,<br/>Blood Bank Team</p>
                                    </div>
                                    <div class='footer'>
                                        <p>© 2024 Blood Bank Management System | Saving Lives Together</p>
                                    </div>
                                </div>
                            </body>
                            </html>";

                        // Send email
                        bool emailSent = await EmailManager.SendEmailAsync(email, subject, body);

                        if (emailSent)
                        {
                            lblStatus.Text = "✅ New password sent to your email!";
                            lblStatus.ForeColor = Color.Green;

                            MessageBox.Show(
                                $"✅ Password Reset Successful!\n\n" +
                                $"A new password has been sent to:\n{email}\n\n" +
                                $"📧 Please check your inbox (and spam folder).\n\n" +
                                $"Username: {user.Username}",
                                "Password Reset",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Information);

                            this.DialogResult = DialogResult.OK;
                            this.Close();
                        }
                        else
                        {
                            lblStatus.Text = "❌ Failed to send email. Please try again.";
                            lblStatus.ForeColor = Color.Red;
                            MessageBox.Show(
                                "❌ Failed to send email.\n\nPlease make sure email settings are configured in Settings.\n\nContact administrator for help.",
                                "Email Error",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
                        }
                    }
                    else
                    {
                        lblStatus.Text = "❌ Failed to reset password. Please try again.";
                        lblStatus.ForeColor = Color.Red;
                    }
                }
                catch (Exception ex)
                {
                    lblStatus.Text = "❌ An error occurred. Please try again.";
                    lblStatus.ForeColor = Color.Red;
                    MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    btnReset.Enabled = true;
                    btnReset.Text = "📧 Send New Password";
                }
            };
        }

        private Classes.Models.User GetUserByEmail(string email)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"=== SEARCHING USER BY EMAIL ===");
                System.Diagnostics.Debug.WriteLine($"Search Email: '{email}'");

                using (var conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();

                    // Direct SQL query - case insensitive search
                    string sql = "SELECT UserID, FullName, Username, Email, Role, IsActive FROM Users WHERE LOWER(Email) = LOWER(@Email)";
                    var cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@Email", email);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string roleStr = reader.GetString(4);
                            Classes.Enums.UserRole role;
                            Enum.TryParse(roleStr, true, out role);

                            var user = new Classes.Models.User
                            {
                                UserID = reader.GetInt32(0),
                                FullName = reader.GetString(1),
                                Username = reader.GetString(2),
                                Email = reader.GetString(3),
                                Role = role,
                                IsActive = reader.GetBoolean(5)
                            };

                            System.Diagnostics.Debug.WriteLine($"✅ User Found!");
                            System.Diagnostics.Debug.WriteLine($"   UserID: {user.UserID}");
                            System.Diagnostics.Debug.WriteLine($"   Username: {user.Username}");
                            System.Diagnostics.Debug.WriteLine($"   Email: {user.Email}");

                            return user;
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine($"❌ No user found with email: '{email}'");

                            // Debug: Show all emails in database
                            var cmd2 = new SqlCommand("SELECT UserID, Username, Email FROM Users", conn);
                            using (var reader2 = cmd2.ExecuteReader())
                            {
                                System.Diagnostics.Debug.WriteLine("All users in database:");
                                while (reader2.Read())
                                {
                                    System.Diagnostics.Debug.WriteLine($"  ID: {reader2[0]}, Username: {reader2[1]}, Email: '{reader2[2]}'");
                                }
                            }
                        }
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetUserByEmail Error: {ex.Message}");
                return null;
            }
        }

        private string GenerateRandomPassword(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            Random random = new Random();
            char[] password = new char[length];
            for (int i = 0; i < length; i++)
            {
                password[i] = chars[random.Next(chars.Length)];
            }
            return new string(password);
        }
    }
}