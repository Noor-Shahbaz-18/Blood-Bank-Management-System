using System;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using BloodBankManagementSystem.Classes.Database;
using BloodBankManagementSystem.Classes.Helpers;

namespace BloodBankManagementSystem.Forms.Patient
{
    public partial class DashboardUC : UserControl
    {
        // ── Colors ───────────────────────────────────────────────
        private readonly Color clrRed = Color.FromArgb(120, 22, 27);
        private readonly Color clrBg = Color.FromArgb(245, 247, 250);
        private readonly Color clrCard = Color.White;
        private readonly Color clrText = Color.FromArgb(31, 41, 55);
        private readonly Color clrGray = Color.FromArgb(107, 114, 128);
        private readonly Color clrGreen = Color.FromArgb(34, 197, 94);
        private readonly Color clrAmber = Color.FromArgb(245, 158, 11);
        private readonly Color clrBlue = Color.FromArgb(59, 130, 246);

        // ── Layout ───────────────────────────────────────────────
        private Panel pnlScroll;
        private FlowLayoutPanel flpStats;
        private Panel pnlActivity;

        // ── Data ─────────────────────────────────────────────────
        private int totalRequests = 0;
        private int approvedRequests = 0;
        private int pendingRequests = 0;
        private int unreadNotifications = 0;

        public DashboardUC()
        {
            InitializeComponent();
            this.Dock = DockStyle.Fill;
            this.BackColor = clrBg;
            BuildUI();
            LoadDashboardData();
        }

        // ============================================================
        // LOAD REAL DATA FROM DATABASE
        // ============================================================
        private void LoadDashboardData()
        {
            try
            {
                int currentUserId = SessionManager.CurrentUserID;

                // Get patient's requisitions
                DataTable dtRequisitions = RequisitionDAL.GetRequisitionsByPatient(currentUserId);

                if (dtRequisitions != null)
                {
                    totalRequests = dtRequisitions.Rows.Count;

                    foreach (DataRow row in dtRequisitions.Rows)
                    {
                        string status = row["Status"]?.ToString() ?? "";
                        if (status == "Approved")
                            approvedRequests++;
                        else if (status == "Pending")
                            pendingRequests++;
                    }
                }

                // Get unread notifications count
                unreadNotifications = NotificationDAL.GetUnreadCount(currentUserId);

                // Update UI with real data
                UpdateStatCards();
                LoadRecentActivity();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LoadDashboardData Error: {ex.Message}");
                // Fallback to default values
                totalRequests = 0;
                approvedRequests = 0;
                pendingRequests = 0;
                unreadNotifications = 0;
                UpdateStatCards();
            }
        }

        private void UpdateStatCards()
        {
            if (flpStats == null || flpStats.Controls.Count < 4) return;

            // Update Blood Requests card (index 0)
            UpdateCardValue(flpStats.Controls[0] as Panel, totalRequests.ToString(), "Total submitted requests");

            // Update Approved card (index 1)
            UpdateCardValue(flpStats.Controls[1] as Panel, approvedRequests.ToString(), "Successfully approved");

            // Update Pending card (index 2)
            UpdateCardValue(flpStats.Controls[2] as Panel, pendingRequests.ToString(), "Awaiting response");

            // Update Notifications card (index 3)
            UpdateCardValue(flpStats.Controls[3] as Panel, unreadNotifications.ToString(), "Unread notifications");
        }

        private void UpdateCardValue(Panel card, string value, string subtitle)
        {
            if (card == null) return;

            foreach (Control ctrl in card.Controls)
            {
                if (ctrl is Label lbl)
                {
                    // Check if this is the value label (large font)
                    if (lbl.Font.Size >= 20)
                    {
                        lbl.Text = value;
                    }
                    // Check if this is the subtitle label
                    else if (lbl.Text.Contains("submitted") || lbl.Text.Contains("approved") ||
                             lbl.Text.Contains("Awaiting") || lbl.Text.Contains("Unread"))
                    {
                        lbl.Text = subtitle;
                    }
                }
            }
        }

        private void LoadRecentActivity()
        {
            if (pnlActivity == null) return;

            // Clear existing activity items (keep header and divider)
            var controlsToRemove = new System.Collections.Generic.List<Control>();
            foreach (Control ctrl in pnlActivity.Controls)
            {
                if (ctrl is Panel row && row.Height == 65)
                {
                    controlsToRemove.Add(ctrl);
                }
                else if (ctrl is Panel sep && sep.Height == 1)
                {
                    controlsToRemove.Add(ctrl);
                }
            }
            foreach (var ctrl in controlsToRemove)
            {
                pnlActivity.Controls.Remove(ctrl);
            }

            try
            {
                int currentUserId = SessionManager.CurrentUserID;

                // Get recent requisitions for activity
                DataTable dtRequisitions = RequisitionDAL.GetRequisitionsByPatient(currentUserId);

                if (dtRequisitions != null && dtRequisitions.Rows.Count > 0)
                {
                    int y = 58;
                    int count = 0;

                    foreach (DataRow row in dtRequisitions.Rows)
                    {
                        if (count >= 5) break; // Show only last 5 activities

                        string reqNumber = row["RequisitionNumber"]?.ToString() ?? "";
                        string status = row["Status"]?.ToString() ?? "Pending";
                        DateTime reqDate = Convert.ToDateTime(row["RequestDate"]);
                        string timeAgo = GetTimeAgo(reqDate);

                        string title = status == "Approved" ? "Blood Request Approved" :
                                      status == "Pending" ? "Request Submitted" :
                                      status == "Completed" ? "Request Completed" :
                                      "Request " + status;

                        string desc = status == "Approved" ? $"Your request {reqNumber} has been approved." :
                                    status == "Pending" ? $"Blood request {reqNumber} is pending approval." :
                                    status == "Completed" ? $"Request {reqNumber} has been completed." :
                                    $"Request {reqNumber} status: {status}";

                        Color dotColor = status == "Approved" ? clrGreen :
                                        status == "Pending" ? clrAmber :
                                        status == "Completed" ? clrBlue : clrGray;

                        AddActivityRow(pnlActivity, title, desc, timeAgo, dotColor, ref y);
                        count++;
                    }
                }

                if (pnlActivity.Controls.Count <= 2) // Only header and divider
                {
                    int y = 58;
                    AddActivityRow(pnlActivity, "No Activity Yet", "Your blood requests will appear here", "Just now", clrGray, ref y);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LoadRecentActivity Error: {ex.Message}");
                int y = 58;
                AddActivityRow(pnlActivity, "Unable to load activity", "Please refresh the page", "Now", clrGray, ref y);
            }
        }

        private string GetTimeAgo(DateTime date)
        {
            TimeSpan diff = DateTime.Now - date;
            if (diff.TotalMinutes < 1) return "Just now";
            if (diff.TotalMinutes < 60) return $"{(int)diff.TotalMinutes} min ago";
            if (diff.TotalHours < 24) return $"{(int)diff.TotalHours} hours ago";
            if (diff.TotalDays < 7) return $"{(int)diff.TotalDays} days ago";
            if (diff.TotalDays < 30) return $"{(int)(diff.TotalDays / 7)} weeks ago";
            return date.ToString("dd MMM yyyy");
        }

        private void AddActivityRow(Panel parent, string title, string desc, string time, Color dotColor, ref int y)
        {
            Panel row = new Panel
            {
                Left = 20,
                Top = y,
                Height = 65,
                Width = parent.Width - 40,
                BackColor = Color.Transparent
            };
            parent.Controls.Add(row);

            parent.Layout += (s, e) =>
            {
                row.Width = parent.Width - 40;
            };

            // DOT
            Panel dot = new Panel
            {
                Size = new Size(10, 10),
                Location = new Point(0, 12),
                BackColor = dotColor
            };
            dot.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (SolidBrush b = new SolidBrush(dotColor))
                    e.Graphics.FillEllipse(b, 0, 0, 9, 9);
            };
            row.Controls.Add(dot);

            // TITLE
            Label lblTitle = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = clrText,
                AutoSize = false,
                Height = 22,
                Location = new Point(20, 4)
            };
            row.Controls.Add(lblTitle);

            // DESCRIPTION
            Label lblDesc = new Label
            {
                Text = desc,
                Font = new Font("Segoe UI", 9),
                ForeColor = clrGray,
                AutoSize = false,
                Height = 35,
                Location = new Point(20, 26)
            };
            row.Controls.Add(lblDesc);

            // TIME
            Label lblTime = new Label
            {
                Text = time,
                Font = new Font("Segoe UI", 8),
                ForeColor = clrGray,
                AutoSize = true
            };
            row.Controls.Add(lblTime);

            row.Resize += (s, e) =>
            {
                lblTitle.Width = row.Width - 120;
                lblDesc.Width = row.Width - 40;
                lblTime.Location = new Point(row.Width - lblTime.Width - 5, 6);
            };

            // SEPARATOR
            Panel sep = new Panel
            {
                Left = 20,
                Top = y + 62,
                Height = 1,
                BackColor = Color.FromArgb(243, 244, 246),
                Width = parent.Width - 40
            };
            parent.Controls.Add(sep);

            parent.Layout += (s, e) =>
            {
                sep.Width = parent.Width - 40;
            };

            y += 68;
        }

        // ============================================================
        // BUILD UI
        // ============================================================
        private void BuildUI()
        {
            pnlScroll = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = Color.Transparent,
                Padding = new Padding(20, 18, 20, 20)
            };
            this.Controls.Add(pnlScroll);

            // =====================================================
            // STATS CARDS
            // =====================================================
            flpStats = new FlowLayoutPanel
            {
                Left = 0,
                Top = 0,
                Height = 150,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                AutoSize = false,
                BackColor = Color.Transparent
            };
            pnlScroll.Controls.Add(flpStats);

            BuildStatCard(flpStats, "Blood Requests", "0", "Total submitted requests", clrRed);
            BuildStatCard(flpStats, "Approved", "0", "Successfully approved", clrGreen);
            BuildStatCard(flpStats, "Pending", "0", "Awaiting response", clrAmber);
            BuildStatCard(flpStats, "Notifications", "0", "Unread notifications", clrBlue);

            // =====================================================
            // RECENT ACTIVITY
            // =====================================================
            pnlActivity = BuildSection("Recent Activity", 180, 340);
            pnlScroll.Controls.Add(pnlActivity);

            // =====================================================
            // RESPONSIVE
            // =====================================================
            pnlScroll.Resize += (s, e) => Relayout();
            this.HandleCreated += (s, e) => Relayout();
        }

        private void BuildStatCard(FlowLayoutPanel parent, string title, string value, string sub, Color accent)
        {
            Panel card = MakeCard(140);
            card.Margin = new Padding(0, 0, 14, 0);
            parent.Controls.Add(card);

            // TOP STRIPE
            Panel stripe = new Panel
            {
                Dock = DockStyle.Top,
                Height = 4,
                BackColor = accent
            };
            card.Controls.Add(stripe);

            // VALUE
            Label lblVal = new Label
            {
                Text = value,
                Font = new Font("Segoe UI", 28, FontStyle.Bold),
                ForeColor = accent,
                AutoSize = false,
                Width = card.Width,
                Height = 50,
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(0, 18)
            };
            card.Controls.Add(lblVal);

            // TITLE
            Label lblTitle = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = clrText,
                AutoSize = false,
                Width = card.Width,
                Height = 25,
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(0, 74)
            };
            card.Controls.Add(lblTitle);

            // SUBTITLE
            Label lblSub = new Label
            {
                Text = sub,
                Font = new Font("Segoe UI", 9),
                ForeColor = clrGray,
                AutoSize = false,
                Width = card.Width - 20,
                Height = 35,
                TextAlign = ContentAlignment.TopCenter,
                Location = new Point(10, 100)
            };
            card.Controls.Add(lblSub);

            card.Resize += (s, e) =>
            {
                lblVal.Width = card.Width;
                lblTitle.Width = card.Width;
                lblSub.Width = card.Width - 20;
            };
        }

        private Panel BuildSection(string title, int top, int height)
        {
            Panel card = MakeCard(height);
            card.Left = 0;
            card.Top = top;
            card.Padding = new Padding(20, 16, 20, 16);

            Label lbl = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 13, FontStyle.Bold),
                ForeColor = clrText,
                AutoSize = true,
                Location = new Point(20, 16)
            };
            card.Controls.Add(lbl);

            // DIVIDER
            Panel div = new Panel
            {
                Height = 1,
                BackColor = Color.FromArgb(229, 231, 235),
                Left = 20,
                Top = 44
            };
            card.Controls.Add(div);

            card.Layout += (s, e) =>
            {
                div.Width = card.Width - 40;
            };

            return card;
        }

        private void Relayout()
        {
            int avail = pnlScroll.ClientSize.Width - 40;
            if (avail < 100) return;

            // STATS
            flpStats.Width = avail;
            int count = flpStats.Controls.Count;
            if (count > 0)
            {
                int cardW = (avail - (14 * (count - 1))) / count;
                if (cardW < 180) cardW = 180;
                foreach (Control c in flpStats.Controls)
                {
                    c.Width = cardW;
                }
            }

            // ACTIVITY
            pnlActivity.Width = avail;
        }

        private Panel MakeCard(int height)
        {
            Panel card = new Panel
            {
                Height = height,
                BackColor = clrCard
            };
            card.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (GraphicsPath path = RoundRect(new Rectangle(0, 0, card.Width - 1, card.Height - 1), 14))
                {
                    card.Region = new Region(path);
                    using (Pen pen = new Pen(Color.FromArgb(229, 231, 235), 1))
                        e.Graphics.DrawPath(pen, path);
                }
            };
            return card;
        }

        private static GraphicsPath RoundRect(Rectangle r, int radius)
        {
            GraphicsPath p = new GraphicsPath();
            p.AddArc(r.X, r.Y, radius, radius, 180, 90);
            p.AddArc(r.Right - radius, r.Y, radius, radius, 270, 90);
            p.AddArc(r.Right - radius, r.Bottom - radius, radius, radius, 0, 90);
            p.AddArc(r.X, r.Bottom - radius, radius, radius, 90, 90);
            p.CloseFigure();
            return p;
        }
    }
}