using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace BloodBankManagementSystem.Forms.Common
{
    public partial class CustomMessageBox : Form
    {
        private string _message;
        private string _title;
        private MessageBoxButtons _buttons;
        private MessageBoxIcon _icon;
        private DialogResult _result = DialogResult.None;

        public CustomMessageBox(string message, string title = "Message",
            MessageBoxButtons buttons = MessageBoxButtons.OK,
            MessageBoxIcon icon = MessageBoxIcon.Information)
        {
            _message = message;
            _title = title;
            _buttons = buttons;
            _icon = icon;
            InitializeComponent();
            BuildUI();
        }

        public static DialogResult Show(string message, string title = "Message",
            MessageBoxButtons buttons = MessageBoxButtons.OK,
            MessageBoxIcon icon = MessageBoxIcon.Information)
        {
            using (var msgBox = new CustomMessageBox(message, title, buttons, icon))
            {
                return msgBox.ShowDialog();
            }
        }

        private void BuildUI()
        {
            this.Text = _title;
            this.Size = new Size(450, 220);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = Color.White;

            // Rounded corners
            this.Paint += (s, e) =>
            {
                GraphicsPath path = new GraphicsPath();
                int radius = 15;
                path.AddArc(0, 0, radius, radius, 180, 90);
                path.AddArc(this.Width - radius, 0, radius, radius, 270, 90);
                path.AddArc(this.Width - radius, this.Height - radius, radius, radius, 0, 90);
                path.AddArc(0, this.Height - radius, radius, radius, 90, 90);
                this.Region = new Region(path);
            };

            Panel mainPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20) };
            this.Controls.Add(mainPanel);

            // Icon
            string iconText;
            Color iconColor;
            if (_icon == MessageBoxIcon.Information)
            {
                iconText = "ℹ️";
                iconColor = Color.FromArgb(59, 130, 246);
            }
            else if (_icon == MessageBoxIcon.Warning)
            {
                iconText = "⚠️";
                iconColor = Color.FromArgb(245, 158, 11);
            }
            else if (_icon == MessageBoxIcon.Error)
            {
                iconText = "❌";
                iconColor = Color.FromArgb(220, 38, 38);
            }
            else if (_icon == MessageBoxIcon.Question)
            {
                iconText = "❓";
                iconColor = Color.FromArgb(120, 22, 27);
            }
            else
            {
                iconText = "ℹ️";
                iconColor = Color.FromArgb(59, 130, 246);
            }

            Label lblIcon = new Label
            {
                Text = iconText,
                Font = new Font("Segoe UI Emoji", 28),
                ForeColor = iconColor,
                Location = new Point(20, 20),
                AutoSize = true
            };
            mainPanel.Controls.Add(lblIcon);

            // Title
            Label lblTitle = new Label
            {
                Text = _title,
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.FromArgb(34, 34, 34),
                Location = new Point(80, 22),
                AutoSize = true
            };
            mainPanel.Controls.Add(lblTitle);

            // Message
            Label lblMessage = new Label
            {
                Text = _message,
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.FromArgb(80, 80, 90),
                Location = new Point(80, 55),
                Size = new Size(310, 60),
                AutoSize = false
            };
            mainPanel.Controls.Add(lblMessage);

            // Buttons Panel
            Panel buttonPanel = new Panel { Location = new Point(0, 130), Size = new Size(410, 50), BackColor = Color.FromArgb(248, 249, 250) };
            mainPanel.Controls.Add(buttonPanel);

            int btnX = 210;
            Button btnOK = new Button
            {
                Text = "OK",
                Size = new Size(80, 32),
                Location = new Point(btnX, 9),
                BackColor = Color.FromArgb(120, 22, 27),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnOK.FlatAppearance.BorderSize = 0;
            btnOK.Click += (s, e) => { _result = DialogResult.OK; this.Close(); };

            Button btnYes = new Button
            {
                Text = "Yes",
                Size = new Size(80, 32),
                Location = new Point(btnX - 90, 9),
                BackColor = Color.FromArgb(120, 22, 27),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Visible = false
            };
            btnYes.FlatAppearance.BorderSize = 0;
            btnYes.Click += (s, e) => { _result = DialogResult.Yes; this.Close(); };

            Button btnNo = new Button
            {
                Text = "No",
                Size = new Size(80, 32),
                Location = new Point(btnX, 9),
                BackColor = Color.LightGray,
                ForeColor = Color.Black,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Visible = false
            };
            btnNo.Click += (s, e) => { _result = DialogResult.No; this.Close(); };

            Button btnCancel = new Button
            {
                Text = "Cancel",
                Size = new Size(80, 32),
                Location = new Point(btnX + 90, 9),
                BackColor = Color.LightGray,
                ForeColor = Color.Black,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Visible = false
            };
            btnCancel.Click += (s, e) => { _result = DialogResult.Cancel; this.Close(); };

            switch (_buttons)
            {
                case MessageBoxButtons.OK:
                    btnOK.Visible = true;
                    btnOK.Location = new Point(165, 9);
                    break;
                case MessageBoxButtons.OKCancel:
                    btnOK.Visible = true;
                    btnCancel.Visible = true;
                    btnOK.Location = new Point(120, 9);
                    btnCancel.Location = new Point(210, 9);
                    break;
                case MessageBoxButtons.YesNo:
                    btnYes.Visible = true;
                    btnNo.Visible = true;
                    btnYes.Location = new Point(120, 9);
                    btnNo.Location = new Point(210, 9);
                    break;
                case MessageBoxButtons.YesNoCancel:
                    btnYes.Visible = true;
                    btnNo.Visible = true;
                    btnCancel.Visible = true;
                    btnYes.Location = new Point(60, 9);
                    btnNo.Location = new Point(150, 9);
                    btnCancel.Location = new Point(240, 9);
                    break;
            }

            buttonPanel.Controls.Add(btnOK);
            buttonPanel.Controls.Add(btnYes);
            buttonPanel.Controls.Add(btnNo);
            buttonPanel.Controls.Add(btnCancel);
        }
    }
}