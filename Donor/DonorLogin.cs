using System;
using System.Drawing;
using System.Windows.Forms;
using BloodBankManagementSystem.Classes.BusinessLogic;
using BloodBankManagementSystem.Classes.Enums;
using BloodBankManagementSystem.Classes.Models;
using BloodBankManagementSystem.Classes.Helpers;
using BloodBankManagementSystem.Forms.Shared;

namespace BloodBankManagementSystem.Forms.Donor
{
    public partial class DonorLogin : Form
    {
        public DonorLogin()
        {
            InitializeComponent();
            BuildUI();
        }

        private void BuildUI()
        {
            this.Text = "Donor Login - Blood Bank";
            this.Size = new Size(450, 400);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.White;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            Panel mainPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(30) };
            this.Controls.Add(mainPanel);

            // Logo
            Label lblLogo = new Label
            {
                Text = "🩸",
                Font = new Font("Segoe UI Emoji", 48),
                ForeColor = Color.FromArgb(120, 22, 27),
                Dock = DockStyle.Top,
                Height = 70,
                TextAlign = ContentAlignment.MiddleCenter
            };
            mainPanel.Controls.Add(lblLogo);

            Label lblTitle = new Label
            {
                Text = "Donor Login",
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                ForeColor = Color.FromArgb(120, 22, 27),
                Dock = DockStyle.Top,
                Height = 45,
                TextAlign = ContentAlignment.MiddleCenter
            };
            mainPanel.Controls.Add(lblTitle);

            int y = 150, leftX = 20, fieldWidth = 340;

            // Username
            mainPanel.Controls.Add(new Label { Text = "Username", Location = new Point(leftX, y), AutoSize = true, Font = new Font("Segoe UI", 10) });
            TextBox txtUsername = new TextBox { Location = new Point(leftX, y + 22), Width = fieldWidth, Font = new Font("Segoe UI", 11), BorderStyle = BorderStyle.FixedSingle };
            mainPanel.Controls.Add(txtUsername);
            y += 70;

            // Password
            mainPanel.Controls.Add(new Label { Text = "Password", Location = new Point(leftX, y), AutoSize = true, Font = new Font("Segoe UI", 10) });
            TextBox txtPassword = new TextBox { Location = new Point(leftX, y + 22), Width = fieldWidth, Font = new Font("Segoe UI", 11), UseSystemPasswordChar = true, BorderStyle = BorderStyle.FixedSingle };
            mainPanel.Controls.Add(txtPassword);
            y += 70;

            // Login Button
            Button btnLogin = new Button
            {
                Text = "Login",
                Location = new Point(leftX + 120, y),
                Size = new Size(100, 40),
                BackColor = Color.FromArgb(120, 22, 27),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnLogin.FlatAppearance.BorderSize = 0;
            mainPanel.Controls.Add(btnLogin);
            y += 55;

            // Register Link
            LinkLabel lnkRegister = new LinkLabel
            {
                Text = "New donor? Register here",
                Location = new Point(leftX + 120, y),
                AutoSize = true,
                Font = new Font("Segoe UI", 9),
                LinkColor = Color.FromArgb(120, 22, 27)
            };
            lnkRegister.Click += (s, e) =>
            {
                DonorRegistration regForm = new DonorRegistration();
                regForm.ShowDialog();
            };
            mainPanel.Controls.Add(lnkRegister);

            // Forgot Password Link
            LinkLabel lnkForgot = new LinkLabel
            {
                Text = "Forgot Password?",
                Location = new Point(leftX + 250, y),
                AutoSize = true,
                Font = new Font("Segoe UI", 9),
                LinkColor = Color.Gray
            };
            lnkForgot.Click += (s, e) =>
            {
                ForgotPassword forgotForm = new ForgotPassword();
                forgotForm.ShowDialog();
            };
            mainPanel.Controls.Add(lnkForgot);

            btnLogin.Click += (s, e) =>
            {
                string username = txtUsername.Text.Trim();
                string password = txtPassword.Text;

                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                {
                    MessageBox.Show("Please enter username and password.", "Login Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var user = AuthenticationManager.Login(username, password);

                if (user == null || user.Role != UserRole.Donor)
                {
                    MessageBox.Show("Invalid username or password.", "Login Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                this.Hide();
                DonorDashboard dashboard = new DonorDashboard();
                dashboard.ShowDialog();
                this.Close();
            };
        }
    }
}