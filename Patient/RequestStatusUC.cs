using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace BloodBankManagementSystem.Forms.Patient
{
    public partial class RequestStatusUC : UserControl
    {
        private readonly Color brickRed = Color.FromArgb(120, 22, 27);
        private FlowLayoutPanel mainFlow;

        public RequestStatusUC()
        {
            InitializeComponent();
            BuildUI();
        }

        // =====================================================
        // UI BUILD
        // =====================================================
        private void BuildUI()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.FromArgb(245, 247, 250);
            this.Padding = new Padding(10, 15, 10, 15);

            // ================= MAIN FLOW =================
            mainFlow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                WrapContents = true,
                FlowDirection = FlowDirection.LeftToRight,

                // 🔥 INCREASED SPACING (FIX 1)
                Padding = new Padding(20, 10, 20, 10),

                BackColor = Color.Transparent
            };

            this.Controls.Add(mainFlow);

            // SAMPLE DATA
            AddRequestCard("REQ-2024-000123", "12 May 2024, 10:30 AM", "Cross-Matching in Progress", "2 - 3 Hours", false);
            AddRequestCard("REQ-2024-000118", "08 May 2024, 09:10 AM", "Completed", "Completed", false);
            AddRequestCard("REQ-2024-000110", "02 May 2024, 02:45 PM", "Cancelled", "Cancelled", true);

            // ================= RESPONSIVE =================
            mainFlow.SizeChanged += (s, e) =>
            {
                foreach (Control c in mainFlow.Controls)
                {
                    c.Width = (mainFlow.ClientSize.Width / 3) - 25;
                }
            };
        }

        // =====================================================
        // CARD
        // =====================================================
        private void AddRequestCard(
            string requestId,
            string requestDate,
            string currentStatus,
            string estimatedTime,
            bool isCancelled)
        {
            Panel card = CreateRoundedPanel();

            // ================= 3 CARDS PER ROW =================
            card.Width = (mainFlow.ClientSize.Width / 3) - 25;
            card.MinimumSize = new Size(300, 0);

            card.AutoSize = true;
            card.AutoSizeMode = AutoSizeMode.GrowAndShrink;

            card.Padding = new Padding(20);

            // 🔥 INCREASED CARD SPACING (FIX 2)
            card.Margin = new Padding(15, 15, 15, 25);

            card.BackColor = Color.White;

            mainFlow.Controls.Add(card);

            FlowLayoutPanel layout = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoSize = true
            };

            card.Controls.Add(layout);

            // ================= REQUEST ID =================
            layout.Controls.Add(new Label
            {
                Text = "Request ID",
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.Gray,
                AutoSize = true
            });

            layout.Controls.Add(new Label
            {
                Text = requestId,
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.FromArgb(31, 41, 55),
                AutoSize = true
            });

            layout.Controls.Add(new Label
            {
                Text = requestDate,
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.Gray,
                AutoSize = true
            });

            // ================= STATUS =================
            layout.Controls.Add(new Label
            {
                Text = "Status",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.FromArgb(31, 41, 55),
                AutoSize = true,
                Margin = new Padding(0, 15, 0, 5)
            });

            Panel statusBox = new Panel
            {
                Width = card.Width - 40,
                Height = 90,
                BackColor = isCancelled
                    ? Color.FromArgb(254, 242, 242)
                    : Color.FromArgb(239, 246, 255)
            };

            layout.Controls.Add(statusBox);

            Label statusLabel = new Label
            {
                Text = currentStatus,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = isCancelled ? Color.Red :
                           currentStatus == "Completed" ? Color.Green :
                           Color.FromArgb(37, 99, 235),
                AutoSize = true,
                Location = new Point(15, 15)
            };

            Label desc = new Label
            {
                Text = isCancelled
                    ? "This request has been cancelled."
                    : currentStatus == "Completed"
                    ? "This request is completed successfully."
                    : "Your request is currently being processed.",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.Gray,
                AutoSize = true,
                Location = new Point(15, 45)
            };

            statusBox.Controls.Add(statusLabel);
            statusBox.Controls.Add(desc);

            // ================= TIME =================
            layout.Controls.Add(new Label
            {
                Text = "Estimated Processing Time",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(31, 41, 55),
                AutoSize = true,
                Margin = new Padding(0, 15, 0, 5)
            });

            Label estimatedValue = new Label
            {
                Text = estimatedTime,
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = brickRed,
                AutoSize = true
            };

            layout.Controls.Add(estimatedValue);

            // ================= BUTTON =================
            Button btnCancel = new Button
            {
                Text =
                    isCancelled ? "Cancelled" :
                    currentStatus == "Completed" ? "Completed" :
                    "Cancel Request",

                Size = new Size(170, 40),

                BackColor =
                    isCancelled ? Color.Gray :
                    currentStatus == "Completed" ? Color.DarkGreen :
                    brickRed,

                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,

                Enabled = !isCancelled && currentStatus != "Completed",
                Margin = new Padding(0, 10, 0, 0)
            };

            btnCancel.FlatAppearance.BorderSize = 0;

            layout.Controls.Add(btnCancel);

            // ================= CLICK =================
            btnCancel.Click += (s, e) =>
            {
                if (currentStatus == "Completed")
                {
                    MessageBox.Show("This request is already completed and cannot be cancelled.");
                    return;
                }

                DialogResult result = MessageBox.Show(
                    "Are you sure you want to cancel this request?",
                    "Confirm",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    statusLabel.Text = "Cancelled";
                    statusLabel.ForeColor = Color.Red;

                    desc.Text = "This request has been cancelled.";
                    estimatedValue.Text = "Cancelled";
                    estimatedValue.ForeColor = Color.Red;

                    btnCancel.Text = "Cancelled";
                    btnCancel.BackColor = Color.Gray;
                    btnCancel.Enabled = false;

                    statusBox.BackColor = Color.FromArgb(254, 242, 242);
                }
            };
        }

        // =====================================================
        // ROUNDED PANEL
        // =====================================================
        private Panel CreateRoundedPanel()
        {
            Panel panel = new Panel();

            panel.Paint += (s, e) =>
            {
                GraphicsPath path = new GraphicsPath();
                int r = 18;

                path.AddArc(0, 0, r, r, 180, 90);
                path.AddArc(panel.Width - r, 0, r, r, 270, 90);
                path.AddArc(panel.Width - r, panel.Height - r, r, r, 0, 90);
                path.AddArc(0, panel.Height - r, r, r, 90, 90);
                path.CloseFigure();

                panel.Region = new Region(path);

                using (Pen pen = new Pen(Color.FromArgb(230, 230, 230)))
                {
                    e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    e.Graphics.DrawPath(pen, path);
                }
            };

            return panel;
        }
    }
}