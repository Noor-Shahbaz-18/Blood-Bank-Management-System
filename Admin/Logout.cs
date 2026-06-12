using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace BloodBankManagementSystem.Forms.Admin
{
    public class Logout : Form
    {
        public Logout()
        {
            this.Text = "Logout - Blood Bank";
            this.Size = new Size(480, 450);
            this.BackColor = Color.FromArgb(245, 247, 251);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.None;
            this.AutoScaleMode = AutoScaleMode.Dpi;

            BuildUI();
        }

        private void BuildUI()
        {
            // Main Card Panel
            Panel cardPanel = new Panel();
            cardPanel.Size = new Size(420, 380);
            cardPanel.Location = new Point(30, 30);
            cardPanel.BackColor = Color.White;
            cardPanel.Paint += (s, e) =>
            {
                Panel p = (Panel)s;
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (GraphicsPath gp = RoundRect(p.ClientRectangle, 16))
                {
                    p.Region = new Region(gp);
                }
            };

            // Close Button (X)
            Label closeBtn = new Label();
            closeBtn.Text = "✕";
            closeBtn.Font = new Font("Segoe UI", 14, FontStyle.Bold);
            closeBtn.ForeColor = Color.FromArgb(150, 150, 160);
            closeBtn.BackColor = Color.Transparent;
            closeBtn.AutoSize = true;
            closeBtn.Location = new Point(380, 12);
            closeBtn.Cursor = Cursors.Hand;
            closeBtn.Click += (s, e) => this.Close();
            closeBtn.MouseEnter += (s, e) => closeBtn.ForeColor = Color.FromArgb(220, 53, 69);
            closeBtn.MouseLeave += (s, e) => closeBtn.ForeColor = Color.FromArgb(150, 150, 160);

            // Logout Icon
            Panel iconCircle = new Panel();
            iconCircle.Size = new Size(90, 90);
            iconCircle.Location = new Point(165, 50);
            iconCircle.BackColor = Color.FromArgb(255, 232, 234);
            iconCircle.Paint += (s, e) =>
            {
                Panel p = (Panel)s;
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (GraphicsPath gp = new GraphicsPath())
                {
                    gp.AddEllipse(0, 0, 90, 90);
                    p.Region = new Region(gp);
                }
            };

            Label iconLbl = new Label();
            iconLbl.Text = "🚪";
            iconLbl.Font = new Font("Segoe UI Emoji", 32);
            iconLbl.ForeColor = Color.FromArgb(220, 53, 69);
            iconLbl.BackColor = Color.Transparent;
            iconLbl.AutoSize = false;
            iconLbl.Size = new Size(90, 90);
            iconLbl.Location = new Point(0, 0);
            iconLbl.TextAlign = ContentAlignment.MiddleCenter;

            iconCircle.Controls.Add(iconLbl);

            // Title
            Label titleLbl = new Label();
            titleLbl.Text = "Logout Confirmation";
            titleLbl.Font = new Font("Segoe UI", 18, FontStyle.Bold);
            titleLbl.ForeColor = Color.FromArgb(34, 34, 34);
            titleLbl.BackColor = Color.Transparent;
            titleLbl.Size = new Size(250, 35);
            titleLbl.Location = new Point(85, 160);
            titleLbl.TextAlign = ContentAlignment.MiddleCenter;

            // Description
            Label descLbl = new Label();
            descLbl.Text = "Are you sure you want to logout from the\nBlood Bank Management System?";
            descLbl.Font = new Font("Segoe UI", 11);
            descLbl.ForeColor = Color.FromArgb(102, 102, 102);
            descLbl.BackColor = Color.Transparent;
            descLbl.AutoSize = false;
            descLbl.Size = new Size(340, 50);
            descLbl.Location = new Point(40, 210);
            descLbl.TextAlign = ContentAlignment.MiddleCenter;

            // Cancel Button
            Button cancelBtn = new Button();
            cancelBtn.Text = "Cancel";
            cancelBtn.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            cancelBtn.ForeColor = Color.FromArgb(51, 51, 51);
            cancelBtn.BackColor = Color.FromArgb(238, 241, 246);
            cancelBtn.FlatStyle = FlatStyle.Flat;
            cancelBtn.FlatAppearance.BorderSize = 0;
            cancelBtn.Size = new Size(140, 42);
            cancelBtn.Location = new Point(60, 285);
            cancelBtn.Cursor = Cursors.Hand;
            cancelBtn.Paint += (s, e) =>
            {
                Button btn = (Button)s;
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (GraphicsPath gp = RoundRect(btn.ClientRectangle, 8))
                {
                    btn.Region = new Region(gp);
                }
            };
            cancelBtn.Click += (s, e) =>
            {
                this.DialogResult = DialogResult.Cancel;
                this.Close();
            };
            cancelBtn.MouseEnter += (s, e) => cancelBtn.BackColor = Color.FromArgb(223, 229, 238);
            cancelBtn.MouseLeave += (s, e) => cancelBtn.BackColor = Color.FromArgb(238, 241, 246);

            // Logout Button
            Button logoutBtn = new Button();
            logoutBtn.Text = "Logout";
            logoutBtn.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            logoutBtn.ForeColor = Color.White;
            logoutBtn.BackColor = Color.FromArgb(220, 53, 69);
            logoutBtn.FlatStyle = FlatStyle.Flat;
            logoutBtn.FlatAppearance.BorderSize = 0;
            logoutBtn.Size = new Size(140, 42);
            logoutBtn.Location = new Point(220, 285);
            logoutBtn.Cursor = Cursors.Hand;
            logoutBtn.Paint += (s, e) =>
            {
                Button btn = (Button)s;
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (GraphicsPath gp = RoundRect(btn.ClientRectangle, 8))
                {
                    btn.Region = new Region(gp);
                }
            };
            logoutBtn.Click += (s, e) =>
            {
                this.DialogResult = DialogResult.Yes;
                this.Close();
            };
            logoutBtn.MouseEnter += (s, e) => logoutBtn.BackColor = Color.FromArgb(187, 45, 59);
            logoutBtn.MouseLeave += (s, e) => logoutBtn.BackColor = Color.FromArgb(220, 53, 69);

            cardPanel.Controls.Add(logoutBtn);
            cardPanel.Controls.Add(cancelBtn);
            cardPanel.Controls.Add(descLbl);
            cardPanel.Controls.Add(titleLbl);
            cardPanel.Controls.Add(iconCircle);
            cardPanel.Controls.Add(closeBtn);

            this.Controls.Add(cardPanel);
        }

        // =====================================================
        // HELPER
        // =====================================================
        private GraphicsPath RoundRect(Rectangle r, int radius)
        {
            int d = radius * 2;
            GraphicsPath gp = new GraphicsPath();
            gp.AddArc(r.X, r.Y, d, d, 180, 90);
            gp.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            gp.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            gp.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            gp.CloseFigure();
            return gp;
        }
    }
}