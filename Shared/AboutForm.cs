using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace BloodBankManagementSystem.Forms.Shared
{
    public partial class AboutForm : Form
    {
        public AboutForm()
        {
            InitializeComponent();
            BuildUI();
        }

        private void BuildUI()
        {
            this.Text = "About - Blood Bank Management System";
            this.Size = new Size(550, 500);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.White;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            Panel mainPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(30) };
            this.Controls.Add(mainPanel);

            // Logo / Icon
            Label lblIcon = new Label
            {
                Text = "🩸",
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
                Text = "Blood Bank Management System",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.FromArgb(120, 22, 27),
                Dock = DockStyle.Top,
                Height = 45,
                TextAlign = ContentAlignment.MiddleCenter
            };
            mainPanel.Controls.Add(lblTitle);

            // Version
            Label lblVersion = new Label
            {
                Text = "Version 1.0.0",
                Font = new Font("Segoe UI", 11),
                ForeColor = Color.Gray,
                Dock = DockStyle.Top,
                Height = 30,
                TextAlign = ContentAlignment.MiddleCenter
            };
            mainPanel.Controls.Add(lblVersion);

            // Separator
            Panel separator = new Panel
            {
                Dock = DockStyle.Top,
                Height = 1,
                BackColor = Color.FromArgb(220, 220, 220),
                Margin = new Padding(0, 10, 0, 10)
            };
            mainPanel.Controls.Add(separator);

            // Info Panel
            Panel infoPanel = new Panel { Dock = DockStyle.Top, Height = 180, Padding = new Padding(10) };
            mainPanel.Controls.Add(infoPanel);

            string[] info = {
                "A complete Blood Bank Management System",
                "designed to efficiently manage blood donations,",
                "inventory, requisitions, and transfusions.",
                "",
                "© 2024 Blood Bank Management System",
                "All Rights Reserved."
            };

            int y = 10;
            foreach (string line in info)
            {
                Label lblLine = new Label
                {
                    Text = line,
                    Font = new Font("Segoe UI", 10),
                    ForeColor = Color.FromArgb(80, 80, 90),
                    Location = new Point(20, y),
                    AutoSize = true
                };
                infoPanel.Controls.Add(lblLine);
                y += 25;
            }

            // Features Panel
            Panel featuresPanel = new Panel { Dock = DockStyle.Top, Height = 100, Padding = new Padding(10) };
            mainPanel.Controls.Add(featuresPanel);

            Label lblFeatures = new Label
            {
                Text = "Key Features:",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.FromArgb(120, 22, 27),
                Location = new Point(20, 5),
                AutoSize = true
            };
            featuresPanel.Controls.Add(lblFeatures);

            string[] features = {
                "• Donor Management",
                "• Blood Inventory Tracking",
                "• Patient Requisitions",
                "• Cross Matching",
                "• Reports & Analytics"
            };

            int fy = 30;
            foreach (string feature in features)
            {
                Label lblFeature = new Label
                {
                    Text = feature,
                    Font = new Font("Segoe UI", 9),
                    ForeColor = Color.FromArgb(80, 80, 90),
                    Location = new Point(30, fy),
                    AutoSize = true
                };
                featuresPanel.Controls.Add(lblFeature);
                fy += 22;
            }

            // Close Button
            Button btnClose = new Button
            {
                Text = "Close",
                Size = new Size(100, 35),
                Location = new Point(mainPanel.Width / 2 - 50, mainPanel.Height - 60),
                BackColor = Color.FromArgb(120, 22, 27),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left
            };
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.Click += (s, e) => this.Close();
            mainPanel.Controls.Add(btnClose);

            mainPanel.Resize += (s, e) =>
            {
                btnClose.Location = new Point(mainPanel.Width / 2 - 50, mainPanel.Height - 60);
            };
        }
    }
}