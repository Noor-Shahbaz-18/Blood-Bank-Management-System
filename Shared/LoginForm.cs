using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Data.SqlClient;
using System.Windows.Forms;
using BloodBankManagementSystem.Classes.BusinessLogic;
using BloodBankManagementSystem.Classes.Enums;
using BloodBankManagementSystem.Classes.Models;
using BloodBankManagementSystem.Classes.Helpers;
using BloodBankManagementSystem.Classes.Database;
using BloodBankManagementSystem.Forms.Donor;

namespace BloodBankManagementSystem.Forms.Shared
{
    public partial class LoginForm : Form
    {
        private readonly Color brickRed = Color.FromArgb(120, 22, 27);
        private Panel overlay;
        private Panel loginPanel;
        private Panel textPanel;

        // Form controls
        private TextBox txtUsername;
        private TextBox txtPassword;
        private Button btnLogin;
        private LinkLabel lblForgotPassword;
        private LinkLabel lblRegister;

        public LoginForm()
        {
            InitializeComponent();

            this.SuspendLayout();

            this.Text = "Blood Bank Management System";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MinimumSize = new Size(1200, 700);
            this.FormBorderStyle = FormBorderStyle.Sizable;

            this.SetStyle(ControlStyles.OptimizedDoubleBuffer |
                          ControlStyles.AllPaintingInWmPaint |
                          ControlStyles.UserPaint |
                          ControlStyles.ResizeRedraw, true);

            this.UpdateStyles();
            this.DoubleBuffered = true;

            BuildUI();

            this.Load += LoginForm_Load;
            this.Resize += LoginForm_Resize;

            this.ResumeLayout(false);
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x02000000;
                return cp;
            }
        }

        private void BuildUI()
        {
            this.BackColor = Color.FromArgb(120, 22, 27);
            this.BackgroundImage = null;

            // MAIN OVERLAY
            overlay = new Panel();
            overlay.Dock = DockStyle.Fill;
            overlay.BackColor = Color.FromArgb(110, 0, 0, 0);
            this.Controls.Add(overlay);

            // LOGIN PANEL (Left side)
            loginPanel = new Panel();
            loginPanel.Size = new Size(500, 620);
            loginPanel.BackColor = Color.FromArgb(245, 245, 250);
            loginPanel.BorderStyle = BorderStyle.None;
            overlay.Controls.Add(loginPanel);

            loginPanel.Resize += LoginPanel_Resize;

            // RIGHT TEXT PANEL
            textPanel = new Panel();
            textPanel.Dock = DockStyle.Right;
            textPanel.Width = this.Width / 2 + 40;
            textPanel.BackColor = Color.Transparent;
            textPanel.Padding = new Padding(120, 40, 40, 40);
            overlay.Controls.Add(textPanel);

            // CONTENT CONTAINER for right panel
            TableLayoutPanel contentContainer = new TableLayoutPanel();
            contentContainer.Dock = DockStyle.Fill;
            contentContainer.ColumnCount = 1;
            contentContainer.RowCount = 4;
            contentContainer.RowStyles.Add(new RowStyle(SizeType.Percent, 25F));
            contentContainer.RowStyles.Add(new RowStyle(SizeType.Percent, 20F));
            contentContainer.RowStyles.Add(new RowStyle(SizeType.Percent, 40F));
            contentContainer.RowStyles.Add(new RowStyle(SizeType.Percent, 15F));
            textPanel.Controls.Add(contentContainer);

            // LOGO ROW
            Panel logoRow = new Panel();
            logoRow.Dock = DockStyle.Fill;
            logoRow.BackColor = Color.Transparent;
            contentContainer.Controls.Add(logoRow, 0, 0);

            TableLayoutPanel logoTable = new TableLayoutPanel();
            logoTable.ColumnCount = 2;
            logoTable.RowCount = 1;
            logoTable.BackColor = Color.Transparent;
            logoTable.AutoSize = true;
            logoTable.Anchor = AnchorStyles.None;
            logoTable.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            logoTable.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

            logoRow.Controls.Add(logoTable);
            logoRow.Resize += (s, e) =>
            {
                logoTable.Left = (logoRow.Width - logoTable.Width) / 2;
                logoTable.Top = (logoRow.Height - logoTable.Height) / 2;
            };

            PictureBox bloodLogo = new PictureBox();
            bloodLogo.Size = new Size(110, 110);
            bloodLogo.BackColor = Color.Transparent;
            bloodLogo.SizeMode = PictureBoxSizeMode.Zoom;
            bloodLogo.Image = CreateBloodDropImageWhite();
            bloodLogo.Margin = new Padding(0, 0, 10, 0);
            logoTable.Controls.Add(bloodLogo, 0, 0);

            Panel titleStack = new Panel();
            titleStack.BackColor = Color.Transparent;
            titleStack.AutoSize = true;
            logoTable.Controls.Add(titleStack, 1, 0);

            Label title = new Label();
            title.Text = "Blood Bank";
            title.Font = new Font("Segoe UI", 32, FontStyle.Bold);
            title.ForeColor = Color.White;
            title.AutoSize = true;
            title.Location = new Point(0, 20);
            title.BackColor = Color.Transparent;
            titleStack.Controls.Add(title);

            Label subtitle = new Label();
            subtitle.Text = "Management System";
            subtitle.Font = new Font("Segoe UI", 18);
            subtitle.ForeColor = Color.WhiteSmoke;
            subtitle.AutoSize = true;
            subtitle.Location = new Point(8, 75);
            subtitle.BackColor = Color.Transparent;
            titleStack.Controls.Add(subtitle);

            // HEADING
            Panel headingPanel = new Panel();
            headingPanel.Dock = DockStyle.Fill;
            headingPanel.BackColor = Color.Transparent;
            contentContainer.Controls.Add(headingPanel, 0, 1);

            Label line1 = new Label();
            line1.Text = "Donate Blood, Save Lives";
            line1.Font = new Font("Segoe UI", 36, FontStyle.Bold);
            line1.ForeColor = Color.White;
            line1.Dock = DockStyle.Fill;
            line1.TextAlign = ContentAlignment.MiddleCenter;
            line1.BackColor = Color.Transparent;
            headingPanel.Controls.Add(line1);

            // PARAGRAPH
            Panel paragraphPanel = new Panel();
            paragraphPanel.Dock = DockStyle.Fill;
            paragraphPanel.BackColor = Color.Transparent;
            contentContainer.Controls.Add(paragraphPanel, 0, 2);

            Label paragraph = new Label();
            paragraph.Text = "It is a modern blood bank management platform designed " +
                "to streamline donations, blood requisitions, emergency requests, " +
                "and transfusion workflows.\n\n" +
                "The system connects hospitals, donors, laboratories, and patients " +
                "in real-time to ensure that every blood unit reaches the right patient " +
                "without delay.\n\n" +
                "With secure healthcare data management, intelligent tracking, " +
                "and 24/7 emergency support, this system helps healthcare organizations " +
                "save lives faster, safer, and more efficiently.";

            paragraph.Font = new Font("Segoe UI", 14);
            paragraph.ForeColor = Color.Gainsboro;
            paragraph.Dock = DockStyle.Fill;
            paragraph.TextAlign = ContentAlignment.MiddleCenter;
            paragraph.Margin = new Padding(0, 0, 0, 0);
            paragraph.BackColor = Color.Transparent;
            paragraph.Padding = new Padding(10);
            paragraphPanel.Controls.Add(paragraph);

            // TAGLINE
            Panel taglinePanel = new Panel();
            taglinePanel.Dock = DockStyle.Fill;
            taglinePanel.BackColor = Color.Transparent;
            contentContainer.Controls.Add(taglinePanel, 0, 3);

            Label line2 = new Label();
            line2.Text = "Every drop counts. Be a hero today.";
            line2.Font = new Font("Segoe UI", 16, FontStyle.Italic);
            line2.ForeColor = Color.WhiteSmoke;
            line2.Dock = DockStyle.Fill;
            line2.TextAlign = ContentAlignment.MiddleCenter;
            line2.BackColor = Color.Transparent;
            taglinePanel.Controls.Add(line2);

            // ========== LOGIN PANEL CONTENT (Left side) ==========
            int centerX = 55;
            int currentY = 40;

            Label lblWelcome = new Label();
            lblWelcome.Text = "Welcome Back";
            lblWelcome.Font = new Font("Segoe UI", 30, FontStyle.Bold);
            lblWelcome.ForeColor = Color.FromArgb(31, 41, 55);
            lblWelcome.AutoSize = true;
            lblWelcome.Location = new Point(centerX + 50, currentY);
            lblWelcome.BackColor = Color.Transparent;
            loginPanel.Controls.Add(lblWelcome);
            currentY += 60;

            Label lblSub = new Label();
            lblSub.Text = "Please login to your account";
            lblSub.Font = new Font("Segoe UI", 12);
            lblSub.ForeColor = Color.Gray;
            lblSub.AutoSize = true;
            lblSub.Location = new Point(centerX + 85, currentY);
            lblSub.BackColor = Color.Transparent;
            loginPanel.Controls.Add(lblSub);
            currentY += 70;

            // Username Field Label
            Label lblUsername = new Label();
            lblUsername.Text = "Username";
            lblUsername.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            lblUsername.ForeColor = Color.FromArgb(55, 65, 81);
            lblUsername.AutoSize = true;
            lblUsername.Location = new Point(centerX, currentY);
            lblUsername.BackColor = Color.Transparent;
            loginPanel.Controls.Add(lblUsername);
            currentY += 28;

            // ✅ Username Container Panel (same style as password container)
            Panel usernameContainer = new Panel();
            usernameContainer.Size = new Size(390, 45);
            usernameContainer.Location = new Point(centerX, currentY);
            usernameContainer.BackColor = Color.White;
            usernameContainer.BorderStyle = BorderStyle.FixedSingle;
            loginPanel.Controls.Add(usernameContainer);

            // ✅ Username TextBox inside container
            txtUsername = new TextBox();
            txtUsername.Size = new Size(390, 43);
            txtUsername.Location = new Point(0, 1);
            txtUsername.Font = new Font("Segoe UI", 12);
            txtUsername.BorderStyle = BorderStyle.None;
            txtUsername.BackColor = Color.White;
            usernameContainer.Controls.Add(txtUsername);

            currentY += 65;

            // Password Field Label
            Label lblPassword = new Label();
            lblPassword.Text = "Password";
            lblPassword.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            lblPassword.ForeColor = Color.FromArgb(55, 65, 81);
            lblPassword.AutoSize = true;
            lblPassword.Location = new Point(centerX, currentY);
            lblPassword.BackColor = Color.Transparent;
            loginPanel.Controls.Add(lblPassword);
            currentY += 28;

            // Password Container Panel
            Panel pwdContainer = new Panel();
            pwdContainer.Size = new Size(390, 45);
            pwdContainer.Location = new Point(centerX, currentY);
            pwdContainer.BackColor = Color.White;
            pwdContainer.BorderStyle = BorderStyle.FixedSingle;
            loginPanel.Controls.Add(pwdContainer);

            // Password TextBox
            txtPassword = new TextBox();
            txtPassword.Size = new Size(344, 43);
            txtPassword.Location = new Point(0, 1);
            txtPassword.Font = new Font("Segoe UI", 12);
            txtPassword.BorderStyle = BorderStyle.None;
            txtPassword.UseSystemPasswordChar = true;
            txtPassword.BackColor = Color.White;
            pwdContainer.Controls.Add(txtPassword);

            // Eye Button
            Button btnEye = new Button();
            btnEye.Size = new Size(40, 43);
            btnEye.Location = new Point(348, 0);
            btnEye.Text = "👁";
            btnEye.Font = new Font("Segoe UI", 12);
            btnEye.FlatStyle = FlatStyle.Flat;
            btnEye.FlatAppearance.BorderSize = 0;
            btnEye.BackColor = Color.White;
            btnEye.Cursor = Cursors.Hand;
            btnEye.TabStop = false;
            btnEye.Click += (s, e) =>
            {
                txtPassword.UseSystemPasswordChar = !txtPassword.UseSystemPasswordChar;
                btnEye.Text = txtPassword.UseSystemPasswordChar ? "👁" : "🙈";
            };
            pwdContainer.Controls.Add(btnEye);

            currentY += 65;

            // Forgot Password Link
            lblForgotPassword = new LinkLabel();
            lblForgotPassword.Text = "Forgot Password?";
            lblForgotPassword.Location = new Point(centerX + 280, currentY);
            lblForgotPassword.AutoSize = true;
            lblForgotPassword.Font = new Font("Segoe UI", 9);
            lblForgotPassword.LinkColor = brickRed;
            lblForgotPassword.Click += (s, e) =>
            {
                ForgotPassword forgotForm = new ForgotPassword();
                forgotForm.ShowDialog();
            };
            loginPanel.Controls.Add(lblForgotPassword);
            currentY += 55;

            // Login Button
            btnLogin = new Button();
            btnLogin.Text = "LOGIN";
            btnLogin.Size = new Size(390, 55);
            btnLogin.Location = new Point(centerX, currentY);
            btnLogin.BackColor = brickRed;
            btnLogin.ForeColor = Color.White;
            btnLogin.FlatStyle = FlatStyle.Flat;
            btnLogin.FlatAppearance.BorderSize = 0;
            btnLogin.Font = new Font("Segoe UI", 14, FontStyle.Bold);
            btnLogin.Cursor = Cursors.Hand;
            btnLogin.Click += BtnLogin_Click;

            btnLogin.MouseEnter += (s, e) => btnLogin.BackColor = Color.FromArgb(145, 25, 30);
            btnLogin.MouseLeave += (s, e) => btnLogin.BackColor = brickRed;

            loginPanel.Controls.Add(btnLogin);
            currentY += 75;

            // Register Link
            Label lblNewUser = new Label();
            lblNewUser.Text = "Don't have an account? ";
            lblNewUser.Font = new Font("Segoe UI", 9);
            lblNewUser.ForeColor = Color.Gray;
            lblNewUser.AutoSize = true;
            lblNewUser.Location = new Point(centerX + 70, currentY);
            loginPanel.Controls.Add(lblNewUser);

            lblRegister = new LinkLabel();
            lblRegister.Text = "Register as Donor";
            lblRegister.AutoSize = true;
            lblRegister.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            lblRegister.LinkColor = brickRed;
            lblRegister.Location = new Point(centerX + 215, currentY);
            lblRegister.Click += (s, e) =>
            {
                DonorRegistration regForm = new DonorRegistration();
                regForm.ShowDialog();
            };
            loginPanel.Controls.Add(lblRegister);

            UpdateLayout();
        }

        private Image CreateBloodDropImageWhite()
        {
            Bitmap bmp = new Bitmap(110, 110);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.Clear(Color.Transparent);
                using (SolidBrush brush = new SolidBrush(Color.White))
                {
                    GraphicsPath path = new GraphicsPath();
                    path.AddEllipse(25, 45, 60, 60);
                    Point[] points = { new Point(55, 5), new Point(33, 55), new Point(77, 55) };
                    path.AddPolygon(points);
                    g.FillPath(brush, path);
                }
            }
            return bmp;
        }

        private void LoginPanel_Resize(object sender, EventArgs e)
        {
            GraphicsPath path = new GraphicsPath();
            int radius = 25;

            path.AddArc(0, 0, radius, radius, 180, 90);
            path.AddArc(loginPanel.Width - radius, 0, radius, radius, 270, 90);
            path.AddArc(loginPanel.Width - radius, loginPanel.Height - radius, radius, radius, 0, 90);
            path.AddArc(0, loginPanel.Height - radius, radius, radius, 90, 90);
            path.CloseFigure();

            loginPanel.Region = new Region(path);
        }

        private void LoginForm_Load(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Maximized;
            UpdateLayout();

            this.BeginInvoke(new Action(() =>
            {
                UpdateLayout();
                this.Refresh();
            }));
        }

        private void LoginForm_Resize(object sender, EventArgs e)
        {
            UpdateLayout();
        }

        private void UpdateLayout()
        {
            if (overlay == null || loginPanel == null || textPanel == null)
                return;

            loginPanel.Location = new Point(80, Math.Max(20, (overlay.Height - loginPanel.Height) / 2));
            textPanel.Width = overlay.Width / 2 + 150;
        }

        private void BtnLogin_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Please enter username and password.", "Login Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var user = AuthenticationManager.Login(username, password);

            if (user != null)
            {
                this.Hide();
                OpenDashboard(user.Role);
                this.Close();
                return;
            }

            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();
                    string query = "SELECT UserID, FullName, Username, PasswordHash, Role, IsActive FROM Users WHERE Username = @Username";
                    using (var cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Username", username);
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string storedHash = reader.GetString(3);
                                string role = reader.GetString(4);
                                bool isActive = reader.GetBoolean(5);
                                string computedHash = EncryptionHelper.HashPassword(password);

                                if (storedHash == computedHash && isActive)
                                {
                                    user = new User
                                    {
                                        UserID = reader.GetInt32(0),
                                        FullName = reader.GetString(1),
                                        Username = reader.GetString(2),
                                        Role = (UserRole)Enum.Parse(typeof(UserRole), role),
                                        IsActive = true
                                    };
                                    SessionManager.Login(user);
                                    this.Hide();
                                    OpenDashboard(user.Role);
                                    this.Close();
                                    return;
                                }
                                else
                                {
                                    string message = $"Login Failed!\n\n" +
                                                    $"Please check:\n" +
                                                    $"- Caps Lock is OFF\n" +
                                                    $"- Password is correct\n" +
                                                    $"- Account is active";
                                    MessageBox.Show(message, "Login Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                }
                            }
                            else
                            {
                                MessageBox.Show($"User '{username}' not found in database.\n\nPlease check username or contact administrator.",
                                    "Login Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Database error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OpenDashboard(UserRole role)
        {
            Form dashboard = null;
            switch (role)
            {
                case UserRole.Admin:
                    dashboard = new Admin.AdminDashboard();
                    break;
                case UserRole.Manager:
                    dashboard = new Manager.ManagerDashboard();
                    break;
                case UserRole.Doctor:
                    dashboard = new Doctor.DoctorDashboard();
                    break;
                case UserRole.Donor:
                    dashboard = new Donor.DonorDashboard();
                    break;
                case UserRole.Patient:
                    dashboard = new Patient.PatientDashboard();
                    break;
                case UserRole.Technician:
                    dashboard = new Technician.TechnicianDashboard();
                    break;
                default:
                    MessageBox.Show($"Unknown role: {role}", "Login Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
            }

            if (dashboard != null)
            {
                dashboard.ShowDialog();
            }
        }
    }
}