using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Threading.Tasks;

namespace BloodBankManagementSystem.Forms.Shared
{
    public partial class SplashScreen : Form
    {
        private Timer loadingTimer;
        private int progressValue = 0;
        private Panel progressBar;

        public SplashScreen()
        {
            InitializeComponent();
            BuildUI();
            StartLoading();
        }

        private void BuildUI()
        {
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Size = new Size(500, 400);
            this.BackColor = Color.FromArgb(120, 22, 27);
            this.ShowInTaskbar = false;
            this.TopMost = true;

            // Rounded corners
            this.Paint += (s, e) =>
            {
                GraphicsPath path = new GraphicsPath();
                int radius = 20;
                path.AddArc(0, 0, radius, radius, 180, 90);
                path.AddArc(this.Width - radius, 0, radius, radius, 270, 90);
                path.AddArc(this.Width - radius, this.Height - radius, radius, radius, 0, 90);
                path.AddArc(0, this.Height - radius, radius, radius, 90, 90);
                this.Region = new Region(path);
            };

            // Logo / Icon
            Label lblIcon = new Label
            {
                Text = "🩸",
                Font = new Font("Segoe UI Emoji", 64),
                ForeColor = Color.White,
                Dock = DockStyle.Top,
                Height = 100,
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent
            };
            this.Controls.Add(lblIcon);

            // Title
            Label lblTitle = new Label
            {
                Text = "Blood Bank Management System",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(50, 120),
                AutoSize = true,
                BackColor = Color.Transparent
            };
            this.Controls.Add(lblTitle);

            // Subtitle
            Label lblSubtitle = new Label
            {
                Text = "Saving Lives, One Drop at a Time",
                Font = new Font("Segoe UI", 11),
                ForeColor = Color.FromArgb(255, 200, 200),
                Location = new Point(110, 155),
                AutoSize = true,
                BackColor = Color.Transparent
            };
            this.Controls.Add(lblSubtitle);

            // Progress Bar Container
            Panel progressContainer = new Panel
            {
                Size = new Size(400, 8),
                Location = new Point(50, 220),
                BackColor = Color.FromArgb(80, 15, 18),
                BorderStyle = BorderStyle.None
            };
            this.Controls.Add(progressContainer);

            // Progress Bar
            progressBar = new Panel
            {
                Size = new Size(0, 8),
                BackColor = Color.White,
                Location = new Point(0, 0)
            };
            progressContainer.Controls.Add(progressBar);

            // Loading Text
            Label lblLoading = new Label
            {
                Text = "Loading... 0%",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(255, 200, 200),
                Location = new Point(50, 240),
                AutoSize = true,
                BackColor = Color.Transparent
            };
            this.Controls.Add(lblLoading);

            // Version
            Label lblVersion = new Label
            {
                Text = "Version 1.0.0",
                Font = new Font("Segoe UI", 8),
                ForeColor = Color.FromArgb(200, 150, 150),
                Location = new Point(50, 360),
                AutoSize = true,
                BackColor = Color.Transparent
            };
            this.Controls.Add(lblVersion);

            // Copyright
            Label lblCopyright = new Label
            {
                Text = "© 2024 Blood Bank Management System",
                Font = new Font("Segoe UI", 8),
                ForeColor = Color.FromArgb(200, 150, 150),
                Location = new Point(280, 360),
                AutoSize = true,
                BackColor = Color.Transparent
            };
            this.Controls.Add(lblCopyright);

            loadingTimer = new Timer();
            loadingTimer.Interval = 30;
            loadingTimer.Tick += (s, e) =>
            {
                progressValue++;
                if (progressValue <= 100)
                {
                    int width = (progressContainer.Width * progressValue) / 100;
                    progressBar.Width = width;
                    lblLoading.Text = $"Loading... {progressValue}%";
                }
                else
                {
                    loadingTimer.Stop();
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
            };
        }

        private void StartLoading()
        {
            loadingTimer.Start();
        }

        public static void ShowSplash()
        {
            using (SplashScreen splash = new SplashScreen())
            {
                splash.ShowDialog();
            }
        }

        public static async Task ShowSplashAsync(int durationMs = 3000)
        {
            using (SplashScreen splash = new SplashScreen())
            {
                var task = Task.Run(() => splash.ShowDialog());
                await Task.Delay(durationMs);
                splash.Close();
                await task;
            }
        }
    }
}