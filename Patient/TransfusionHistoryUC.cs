using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace BloodBankManagementSystem.Forms.Patient
{
    public partial class TransfusionHistoryUC : UserControl
    {
        private readonly Color brickRed = Color.FromArgb(120, 22, 27);

        private Panel mainCard;
        private FlowLayoutPanel historyPanel;

        public TransfusionHistoryUC()
        {
            InitializeComponent();
            BuildUI();
        }

        // =====================================================
        // BUILD UI
        // =====================================================
        private void BuildUI()
        {
            this.Dock = DockStyle.Fill;

            this.BackColor = Color.FromArgb(245, 247, 250);

            // LEFT & RIGHT SPACE
            this.Padding = new Padding(8, 15, 8, 15);

            // =====================================================
            // MAIN CARD
            // =====================================================
            mainCard = CreateRoundedPanel();

            mainCard.Location = new Point(0, 0);

            mainCard.Size = new Size(
                this.Width - 16,
                this.Height - 30);

            this.Controls.Add(mainCard);

            // =====================================================
            // HISTORY PANEL
            // =====================================================
            historyPanel = new FlowLayoutPanel();

            historyPanel.Location = new Point(15, 15);

            historyPanel.Size = new Size(
                mainCard.Width - 30,
                mainCard.Height - 30);

            historyPanel.FlowDirection =
                FlowDirection.TopDown;

            historyPanel.WrapContents = false;

            historyPanel.AutoScroll = true;

            historyPanel.Padding =
                new Padding(0, 0, 3, 10);

            historyPanel.BackColor =
                Color.Transparent;

            historyPanel.SizeChanged +=
                (s, e) =>
                {
                    HideVerticalScrollBar(historyPanel);

                    foreach (Control c in historyPanel.Controls)
                    {
                        c.Width =
                            historyPanel.ClientSize.Width - 5;
                    }
                };

            historyPanel.HandleCreated +=
                (s, e) =>
                {
                    HideVerticalScrollBar(historyPanel);
                };

            mainCard.Controls.Add(historyPanel);

            // =====================================================
            // ITEMS
            // =====================================================
            AddHistoryItem(
                "10 May 2024",
                "O+ (PRBC)",
                "General Hospital, Lahore",
                "1 Unit");

            AddHistoryItem(
                "28 Apr 2024",
                "O+ (PRBC)",
                "General Hospital, Lahore",
                "1 Unit");

            AddHistoryItem(
                "15 Apr 2024",
                "A+ (Whole Blood)",
                "Mayo Hospital, Lahore",
                "2 Units");

            AddHistoryItem(
                "02 Apr 2024",
                "B+ (Platelets)",
                "Services Hospital, Lahore",
                "1 Unit");

            // =====================================================
            // RESIZE
            // =====================================================
            this.Resize += (s, e) =>
            {
                mainCard.Size = new Size(
                    this.Width - 16,
                    this.Height - 30);

                historyPanel.Size = new Size(
                    mainCard.Width - 30,
                    mainCard.Height - 30);

                foreach (Control c in historyPanel.Controls)
                {
                    c.Width =
                        historyPanel.ClientSize.Width - 5;

                    Button btn =
                        c.Controls["btnView"] as Button;

                    if (btn != null)
                    {
                        // LEFT MOVE
                        btn.Location = new Point(
                            c.Width - 160,
                            c.Height - 58);
                    }
                }

                HideVerticalScrollBar(historyPanel);
            };
        }

        // =====================================================
        // HISTORY ITEM
        // =====================================================
        private void AddHistoryItem(
            string date,
            string bloodGroup,
            string hospital,
            string units)
        {
            Panel item = CreateRoundedPanel();

            item.Size = new Size(
                historyPanel.Width - 10,
                155);

            item.Margin =
                new Padding(0, 0, 0, 15);

            item.BackColor =
                Color.FromArgb(249, 250, 251);

            historyPanel.Controls.Add(item);

            // =====================================================
            // LEFT BORDER
            // =====================================================
            Panel border = new Panel
            {
                Dock = DockStyle.Left,
                Width = 6,
                BackColor = brickRed
            };

            item.Controls.Add(border);

            // =====================================================
            // CONTENT
            // =====================================================
            int y = 20;

            AddRow(
                item,
                "Date",
                date,
                ref y);

            AddRow(
                item,
                "Blood Group",
                bloodGroup,
                ref y);

            AddRow(
                item,
                "Hospital",
                hospital,
                ref y);

            AddRow(
                item,
                "Units",
                units,
                ref y);

            // =====================================================
            // BUTTON
            // =====================================================
            Button btnView = new Button
            {
                Name = "btnView",

                Text = "View Details",

                Size = new Size(110, 34),

                // LEFT MOVE
                Location = new Point(
                    item.Width - 160,
                    item.Height - 58),

                BackColor = brickRed,

                ForeColor = Color.White,

                FlatStyle = FlatStyle.Flat,

                Font = new Font(
                    "Segoe UI Semibold",
                    9),

                Cursor = Cursors.Hand
            };

            btnView.FlatAppearance.BorderSize = 0;

            btnView.Click += (s, e) =>
            {
                MessageBox.Show(
                    $"Date: {date}\nBlood Group: {bloodGroup}\nHospital: {hospital}\nUnits: {units}",
                    "Transfusion Details",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            };

            item.Controls.Add(btnView);

            // =====================================================
            // RESPONSIVE
            // =====================================================
            item.Resize += (s, e) =>
            {
                btnView.Location = new Point(
                    item.Width - 160,
                    item.Height - 58);
            };
        }

        // =====================================================
        // ROWS
        // =====================================================
        private void AddRow(
            Panel parent,
            string label,
            string value,
            ref int y)
        {
            Label lblTitle = new Label
            {
                Text = label,

                Font = new Font(
                    "Segoe UI Semibold",
                    9),

                ForeColor = Color.Gray,

                AutoSize = true,

                Location = new Point(28, y)
            };

            parent.Controls.Add(lblTitle);

            Label lblValue = new Label
            {
                Text = value,

                Font = new Font(
                    "Segoe UI",
                    10,
                    FontStyle.Bold),

                ForeColor =
                    Color.FromArgb(31, 41, 55),

                AutoSize = true,

                Location =
                    new Point(150, y - 1)
            };

            parent.Controls.Add(lblValue);

            y += 25;
        }

        // =====================================================
        // ROUNDED PANEL
        // =====================================================
        private Panel CreateRoundedPanel()
        {
            Panel panel = new Panel();

            panel.BackColor = Color.White;

            panel.Paint += (s, e) =>
            {
                GraphicsPath path =
                    new GraphicsPath();

                int radius = 18;

                path.AddArc(
                    0,
                    0,
                    radius,
                    radius,
                    180,
                    90);

                path.AddArc(
                    panel.Width - radius,
                    0,
                    radius,
                    radius,
                    270,
                    90);

                path.AddArc(
                    panel.Width - radius,
                    panel.Height - radius,
                    radius,
                    radius,
                    0,
                    90);

                path.AddArc(
                    0,
                    panel.Height - radius,
                    radius,
                    radius,
                    90,
                    90);

                path.CloseFigure();

                panel.Region =
                    new Region(path);

                using (Pen pen = new Pen(
                    Color.FromArgb(230, 230, 230),
                    1))
                {
                    e.Graphics.SmoothingMode =
                        SmoothingMode.AntiAlias;

                    e.Graphics.DrawPath(
                        pen,
                        path);
                }
            };

            return panel;
        }

        // =====================================================
        // HIDE SCROLLBAR
        // =====================================================
        private const int SB_VERT = 1;

        [DllImport("user32.dll")]
        private static extern bool ShowScrollBar(
            IntPtr hWnd,
            int wBar,
            bool bShow);

        private void HideVerticalScrollBar(
            Control c)
        {
            if (c == null) return;

            if (c.IsHandleCreated)
            {
                ShowScrollBar(
                    c.Handle,
                    SB_VERT,
                    false);
            }
        }
    }
}