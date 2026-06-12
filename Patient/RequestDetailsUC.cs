using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace BloodBankManagementSystem.Forms.Patient
{
    public partial class RequestDetailsUC : UserControl
    {
        private readonly Color brickRed = Color.FromArgb(120, 22, 27);

        public RequestDetailsUC()
        {
            InitializeComponent();
            BuildUI();
        }

        private void BuildUI()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.FromArgb(245, 247, 250);
            this.Padding = new Padding(30);

            Panel mainCard = CreateCard(new Rectangle(0, 0, 600, 550));
            mainCard.Location = new Point((this.Width - 600) / 2, 30);
            this.Controls.Add(mainCard);

            int y = 25;
            AddDetailRow(mainCard, "Request ID", "REQ-2024-000123", ref y);
            AddDetailRow(mainCard, "Blood Group / Component", "O+ / Packed Red Cells (PRBC)", ref y);
            AddDetailRow(mainCard, "Hospital", "General Hospital, Lahore", ref y);
            AddDetailRow(mainCard, "Requested On", "12 May 2024, 10:30 AM", ref y);

            y += 20;
            mainCard.Controls.Add(new Label
            {
                Text = "Estimated Processing Time",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Location = new Point(30, y),
                AutoSize = true
            });
            y += 30;
            mainCard.Controls.Add(new Label
            {
                Text = "2 - 3 Hours",
                Font = new Font("Segoe UI", 24, FontStyle.Bold),
                ForeColor = brickRed,
                Location = new Point(30, y),
                AutoSize = true
            });

            y += 60;
            Panel noteBox = new Panel
            {
                Size = new Size(540, 60),
                Location = new Point(30, y),
                BackColor = Color.FromArgb(239, 246, 255)
            };
            DrawRoundedRect(noteBox, 10, noteBox.BackColor);
            mainCard.Controls.Add(noteBox);
            noteBox.Controls.Add(new Label
            {
                Text = "ℹ",
                Font = new Font("Segoe UI", 14),
                Location = new Point(15, 15),
                AutoSize = true
            });
            noteBox.Controls.Add(new Label
            {
                Text = "Note",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(45, 15),
                AutoSize = true
            });
            noteBox.Controls.Add(new Label
            {
                Text = "Processing time may vary depending on availability and cross-matching.",
                Font = new Font("Segoe UI", 9),
                Location = new Point(45, 35),
                Size = new Size(480, 20)
            });

            this.Resize += (s, e) => mainCard.Location = new Point((this.Width - 600) / 2, 30);
        }

        private void AddDetailRow(Panel parent, string label, string value, ref int y)
        {
            parent.Controls.Add(new Label
            {
                Text = label,
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.Gray,
                Location = new Point(30, y),
                AutoSize = true
            });
            y += 25;
            parent.Controls.Add(new Label
            {
                Text = value,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Location = new Point(30, y),
                AutoSize = true
            });
            y += 40;
        }

        private Panel CreateCard(Rectangle bounds)
        {
            Panel card = new Panel { Bounds = bounds, BackColor = Color.White };
            card.Paint += (s, e) => DrawRoundedRect(card, 15, Color.White);
            return card;
        }

        private void DrawRoundedRect(Control ctrl, int radius, Color color)
        {
            GraphicsPath path = new GraphicsPath();
            path.AddArc(0, 0, radius, radius, 180, 90);
            path.AddArc(ctrl.Width - radius, 0, radius, radius, 270, 90);
            path.AddArc(ctrl.Width - radius, ctrl.Height - radius, radius, radius, 0, 90);
            path.AddArc(0, ctrl.Height - radius, radius, radius, 90, 90);
            path.CloseFigure();
            ctrl.Region = new Region(path);
        }
    }
}