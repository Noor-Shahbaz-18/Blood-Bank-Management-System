using BloodBankManagementSystem.Classes.Database;
using BloodBankManagementSystem.Classes.Helpers;
using System;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace BloodBankManagementSystem.Forms.Admin
{
    public class AdminDashboard : Form
    {
        private Panel sidebarPanel;
        private Panel topbarPanel;
        private Panel mainPanel;
        private Panel cardsPanel;
        private Panel bottomSection;
        private Timer refreshTimer;

        public AdminDashboard()
        {
            this.Text = "Blood Bank Dashboard";
            this.WindowState = FormWindowState.Maximized;
            this.BackColor = Color.FromArgb(245, 247, 250);
            this.MinimumSize = new Size(1100, 650);
            this.StartPosition = FormStartPosition.CenterScreen;

            BuildMainPanel();
            BuildSidebar();
            BuildTopbar();
            LoadDashboardData();

            refreshTimer = new Timer();
            refreshTimer.Interval = 30000;
            refreshTimer.Tick += (s, e) => LoadDashboardData();
            refreshTimer.Start();
        }

        private void LoadDashboardData()
        {
            try
            {
                LoadCardStats();
                LoadRecentRequests();  // ✅ Now shows only PENDING requests
                LoadRecentActivity();
            }
            catch (Exception ex)
            {
                Logger.Error("Dashboard load error", ex);
            }
        }

        private void LoadCardStats()
        {
            try
            {
                int totalDonors = CommonDAL.GetTotalDonors();
                int totalHospitals = GetTotalHospitals();
                int totalBloodUnits = CommonDAL.GetTotalBloodBags();
                int pendingRequests = RequisitionDAL.GetPendingRequisitionCount();  // ✅ Updated

                if (cardsPanel != null && cardsPanel.Controls.Count > 0)
                {
                    foreach (Control control in cardsPanel.Controls)
                    {
                        if (control is TableLayoutPanel table)
                        {
                            int colIndex = 0;
                            foreach (Control cell in table.Controls)
                            {
                                if (cell is Panel cardPanel)
                                {
                                    foreach (Control inner in cardPanel.Controls)
                                    {
                                        if (inner is Label lbl && lbl.Font.Bold && lbl.Font.Size >= 14)
                                        {
                                            switch (colIndex)
                                            {
                                                case 0: lbl.Text = FormatNumber(totalDonors); break;
                                                case 1: lbl.Text = FormatNumber(totalHospitals); break;
                                                case 2: lbl.Text = FormatNumber(totalBloodUnits); break;
                                                case 3: lbl.Text = FormatNumber(pendingRequests); break;
                                            }
                                            break;
                                        }
                                    }
                                    colIndex++;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("LoadCardStats error", ex);
            }
        }

        private int GetTotalHospitals()
        {
            try
            {
                DataTable dt = HospitalDAL.GetAllHospitals();
                return dt?.Rows.Count ?? 0;
            }
            catch
            {
                return 0;
            }
        }

        private string FormatNumber(int number)
        {
            if (number >= 1000)
                return (number / 1000.0).ToString("0.#") + "k";
            return number.ToString();
        }

        private void LoadRecentRequests()
        {
            try
            {
                if (bottomSection == null) return;

                DataGridView dgv = FindDataGridView(bottomSection);
                if (dgv == null) return;

                dgv.Rows.Clear();

                // ✅ ONLY CHANGE: Get Pending Requisitions
                DataTable dt = RequisitionDAL.GetPendingRequisitions();

                if (dt != null && dt.Rows.Count > 0)
                {
                    int count = 0;
                    foreach (DataRow row in dt.Rows)
                    {
                        if (count >= 8) break;

                        string bloodGroup = row["BloodGroup"]?.ToString() ?? "N/A";
                        string units = row["UnitsNeeded"]?.ToString() ?? "0";
                        string hospital = row["Hospital"]?.ToString() ?? "N/A";
                        string status = "Pending";
                        string urgency = row["Urgency"]?.ToString() ?? "Normal";

                        int rowIndex = dgv.Rows.Add(bloodGroup, $"{units} Unit(s)", hospital, status);

                        // Color coding by urgency
                        if (urgency == "Emergency")
                        {
                            dgv.Rows[rowIndex].DefaultCellStyle.BackColor = Color.FromArgb(255, 220, 220);
                            dgv.Rows[rowIndex].DefaultCellStyle.ForeColor = Color.FromArgb(180, 30, 30);
                        }
                        else if (urgency == "Urgent")
                        {
                            dgv.Rows[rowIndex].DefaultCellStyle.BackColor = Color.FromArgb(255, 245, 220);
                        }
                        count++;
                    }
                }

                if (dgv.Rows.Count == 0)
                {
                    dgv.Rows.Add("---", "---", "No pending requests", "---");
                }
            }
            catch (Exception ex)
            {
                Logger.Error("LoadRecentRequests error", ex);
            }
        }

        private void LoadRecentActivity()
        {
            try
            {
                if (bottomSection == null) return;

                FlowLayoutPanel actFlow = FindFlowLayoutPanel(bottomSection);
                if (actFlow == null) return;

                actFlow.Controls.Clear();

                DataTable dt = AuditLogDAL.GetAllLogs();

                if (dt != null && dt.Rows.Count > 0)
                {
                    int count = 0;
                    foreach (DataRow row in dt.Rows)
                    {
                        if (count >= 7) break;

                        string action = row["Action"]?.ToString() ?? "";
                        string entity = row["EntityType"]?.ToString() ?? "";
                        string username = row["Username"]?.ToString() ?? "";

                        string activityText = $"{action} {entity} by {username}";
                        if (activityText.Length > 40)
                            activityText = activityText.Substring(0, 37) + "...";

                        Panel activityRow = CreateActivityRow(activityText);
                        actFlow.Controls.Add(activityRow);
                        count++;
                    }
                }

                if (actFlow.Controls.Count == 0)
                {
                    Panel emptyRow = CreateActivityRow("No recent activity");
                    actFlow.Controls.Add(emptyRow);
                }
            }
            catch (Exception ex)
            {
                Logger.Error("LoadRecentActivity error", ex);
            }
        }

        private Panel CreateActivityRow(string text)
        {
            Panel row = new Panel();
            row.Width = 260;
            row.Height = 38;
            row.BackColor = Color.Transparent;

            Panel dot = new Panel();
            dot.Size = new Size(24, 24);
            dot.Location = new Point(4, 7);
            dot.BackColor = Color.FromArgb(209, 250, 229);
            dot.Paint += (s, e) =>
            {
                var p = (Panel)s;
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var gp = new GraphicsPath())
                {
                    gp.AddEllipse(0, 0, 24, 24);
                    p.Region = new Region(gp);
                    using (var br = new SolidBrush(p.BackColor))
                        e.Graphics.FillPath(br, gp);
                }
                using (var pen = new Pen(Color.FromArgb(6, 95, 70), 2f))
                {
                    e.Graphics.DrawLine(pen, 6, 12, 10, 16);
                    e.Graphics.DrawLine(pen, 10, 16, 18, 8);
                }
            };

            Label lbl = new Label();
            lbl.Text = text;
            lbl.Font = new Font("Segoe UI", 8);
            lbl.ForeColor = Color.FromArgb(45, 45, 55);
            lbl.Location = new Point(38, 10);
            lbl.Size = new Size(220, 18);
            lbl.AutoSize = false;
            lbl.TextAlign = ContentAlignment.MiddleLeft;

            row.Controls.Add(dot);
            row.Controls.Add(lbl);
            return row;
        }

        private DataGridView FindDataGridView(Control parent)
        {
            foreach (Control ctrl in parent.Controls)
            {
                if (ctrl is DataGridView dgv) return dgv;
                if (ctrl.HasChildren)
                {
                    var found = FindDataGridView(ctrl);
                    if (found != null) return found;
                }
            }
            return null;
        }

        private FlowLayoutPanel FindFlowLayoutPanel(Control parent)
        {
            foreach (Control ctrl in parent.Controls)
            {
                if (ctrl is FlowLayoutPanel flp) return flp;
                if (ctrl.HasChildren)
                {
                    var found = FindFlowLayoutPanel(ctrl);
                    if (found != null) return found;
                }
            }
            return null;
        }

        private void BuildTopbar()
        {
            topbarPanel = new Panel();
            topbarPanel.Dock = DockStyle.Top;
            topbarPanel.Height = 60;
            topbarPanel.BackColor = Color.White;

            Label title = new Label();
            title.Text = "Admin Dashboard";
            title.Font = new Font("Segoe UI", 18, FontStyle.Bold);
            title.ForeColor = Color.FromArgb(25, 25, 25);
            title.AutoSize = true;
            title.Location = new Point(25, 18);

            Label sessionLabel = new Label();
            sessionLabel.Text = $"👋 Welcome, {SessionManager.CurrentFullName} | Role: {SessionManager.CurrentRole}";
            sessionLabel.Font = new Font("Segoe UI", 9);
            sessionLabel.ForeColor = Color.Gray;
            sessionLabel.AutoSize = true;

            topbarPanel.Controls.Add(title);
            topbarPanel.Controls.Add(sessionLabel);

            EventHandler reposition = (s, e) =>
            {
                sessionLabel.Location = new Point(topbarPanel.ClientSize.Width - sessionLabel.Width - 20, 22);
            };
            topbarPanel.Resize += reposition;
            topbarPanel.HandleCreated += reposition;

            this.Controls.Add(topbarPanel);
        }

        private void BuildSidebar()
        {
            sidebarPanel = new Panel();
            sidebarPanel.Dock = DockStyle.Left;
            sidebarPanel.Width = 220;
            sidebarPanel.BackColor = Color.FromArgb(5, 31, 64);

            Panel logoPanel = new Panel();
            logoPanel.Dock = DockStyle.Top;
            logoPanel.Height = 70;
            logoPanel.BackColor = Color.Transparent;

            Label bloodIcon = new Label();
            bloodIcon.Text = "🩸";
            bloodIcon.Font = new Font("Segoe UI Emoji", 22);
            bloodIcon.ForeColor = Color.FromArgb(220, 50, 50);
            bloodIcon.BackColor = Color.Transparent;
            bloodIcon.AutoSize = true;
            bloodIcon.Location = new Point(15, 18);

            Label logoText = new Label();
            logoText.Text = "Blood Bank";
            logoText.Font = new Font("Segoe UI", 16, FontStyle.Bold);
            logoText.ForeColor = Color.White;
            logoText.BackColor = Color.Transparent;
            logoText.AutoSize = true;
            logoText.Location = new Point(66, 22);

            logoPanel.Controls.Add(bloodIcon);
            logoPanel.Controls.Add(logoText);

            FlowLayoutPanel menuPanel = new FlowLayoutPanel();
            menuPanel.Dock = DockStyle.Fill;
            menuPanel.FlowDirection = FlowDirection.TopDown;
            menuPanel.WrapContents = false;
            menuPanel.AutoScroll = true;
            menuPanel.Padding = new Padding(8, 6, 8, 8);
            menuPanel.BackColor = Color.Transparent;

            AddMenuItem(menuPanel, "🏠", "Dashboard", true, null);
            AddMenuItem(menuPanel, "👥", "User Management", false, () =>
            {
                using (UserManagement userMgmt = new UserManagement())
                    userMgmt.ShowDialog();
                LoadDashboardData();
            });
            AddMenuItem(menuPanel, "🔐", "Roles Permissions", false, () =>
            {
                using (RolesPermissions rolesForm = new RolesPermissions())
                    rolesForm.ShowDialog();
            });
            AddMenuItem(menuPanel, "🏥", "Hospitals", false, () =>
            {
                using (HospitalManagement hospitalForm = new HospitalManagement())
                    hospitalForm.ShowDialog();
                LoadDashboardData();
            });
            AddMenuItem(menuPanel, "🩸", "Blood Components", false, () =>
            {
                using (BloodComponents bloodForm = new BloodComponents())
                    bloodForm.ShowDialog();
                LoadDashboardData();
            });
            AddMenuItem(menuPanel, "📊", "Reports", false, () =>
            {
                using (Reports reportsForm = new Reports())
                    reportsForm.ShowDialog();
            });
            AddMenuItem(menuPanel, "📋", "Audit Logs", false, () =>
            {
                using (AuditLog auditForm = new AuditLog())
                    auditForm.ShowDialog();
            });
            AddMenuItem(menuPanel, "📢", "Send Broadcast", false, () =>
            {
                using (BroadcastNotifications broadcastForm = new BroadcastNotifications())
                    broadcastForm.ShowDialog();
            });
            AddMenuItem(menuPanel, "🚨", "Emergency Alert", false, () =>
            {
                using (EmergencyAlert emergencyForm = new EmergencyAlert())
                    emergencyForm.ShowDialog();
            });
            AddMenuItem(menuPanel, "⚙", "Settings", false, () =>
            {
                using (Settings settingsForm = new Settings())
                    settingsForm.ShowDialog();
            });
            AddMenuItem(menuPanel, "🚪", "Logout", false, () =>
            {
                Logout logoutForm = new Logout();
                DialogResult result = logoutForm.ShowDialog();
                if (result == DialogResult.Yes)
                {
                    SessionManager.Logout();
                    Application.Exit();
                }
            });

            sidebarPanel.Controls.Add(menuPanel);
            sidebarPanel.Controls.Add(logoPanel);
            this.Controls.Add(sidebarPanel);
        }

        private void AddMenuItem(FlowLayoutPanel panel, string icon, string text, bool active, Action clickAction)
        {
            Panel item = new Panel();
            item.Size = new Size(200, 40);
            item.Margin = new Padding(0, 1, 0, 1);
            item.Cursor = Cursors.Hand;
            item.BackColor = active ? Color.FromArgb(230, 57, 70) : Color.Transparent;

            Label iconLbl = new Label();
            iconLbl.Text = icon;
            iconLbl.Font = new Font("Segoe UI Emoji", 12);
            iconLbl.ForeColor = Color.White;
            iconLbl.Location = new Point(12, 9);
            iconLbl.AutoSize = true;

            Label textLbl = new Label();
            textLbl.Text = text;
            textLbl.Font = new Font("Segoe UI", 9);
            textLbl.ForeColor = Color.White;
            textLbl.Location = new Point(44, 11);
            textLbl.AutoSize = true;

            EventHandler enter = (s, e) => item.BackColor = Color.FromArgb(200, 50, 60);
            EventHandler leave = (s, e) => { if (!active) item.BackColor = Color.Transparent; };

            if (clickAction != null)
            {
                EventHandler click = (s, e) => clickAction();
                item.Click += click;
                iconLbl.Click += click;
                textLbl.Click += click;
            }

            item.MouseEnter += enter;
            item.MouseLeave += leave;

            item.Controls.Add(iconLbl);
            item.Controls.Add(textLbl);
            panel.Controls.Add(item);
        }

        private void BuildMainPanel()
        {
            mainPanel = new Panel();
            mainPanel.Dock = DockStyle.Fill;
            mainPanel.BackColor = Color.FromArgb(245, 247, 250);
            mainPanel.Padding = new Padding(14, 12, 14, 12);

            Panel wrapper = new Panel();
            wrapper.Dock = DockStyle.Fill;
            wrapper.BackColor = Color.Transparent;

            BuildCardsSection();
            BuildBottomSection();

            wrapper.Controls.Add(bottomSection);
            wrapper.Controls.Add(cardsPanel);

            mainPanel.Controls.Add(wrapper);
            this.Controls.Add(mainPanel);
        }

        private void BuildCardsSection()
        {
            cardsPanel = new Panel();
            cardsPanel.Dock = DockStyle.Top;
            cardsPanel.Height = 110;
            cardsPanel.BackColor = Color.Transparent;

            TableLayoutPanel tbl = new TableLayoutPanel();
            tbl.Dock = DockStyle.Fill;
            tbl.ColumnCount = 4;
            tbl.RowCount = 1;
            tbl.Padding = new Padding(0, 0, 0, 10);
            tbl.BackColor = Color.Transparent;

            for (int c = 0; c < 4; c++)
                tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));
            tbl.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

            string[] labels = { "Total Donors", "Total Hospitals", "Blood Units", "Pending Requests" };
            string[] icons = { "👥", "🏥", "🩸", "⏳" };
            Color[] colors =
            {
                Color.FromArgb(63, 131, 248),
                Color.FromArgb(34, 197, 94),
                Color.FromArgb(239, 68, 68),
                Color.FromArgb(251, 146, 60)
            };

            for (int i = 0; i < 4; i++)
            {
                Panel card = CreateCard(labels[i], "0", icons[i], colors[i]);
                tbl.Controls.Add(card, i, 0);
            }

            cardsPanel.Controls.Add(tbl);
        }

        private Panel CreateCard(string title, string value, string icon, Color color)
        {
            Panel card = new Panel();
            card.Dock = DockStyle.Fill;
            card.Margin = new Padding(5, 0, 5, 0);
            card.BackColor = Color.White;
            card.Paint += (s, e) =>
            {
                var p = (Panel)s;
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var gp = RoundRect(p.ClientRectangle, 8))
                {
                    p.Region = new Region(gp);
                    using (var br = new SolidBrush(Color.White))
                        e.Graphics.FillPath(br, gp);
                }
            };

            Panel iconBox = new Panel();
            iconBox.Size = new Size(40, 40);
            iconBox.Location = new Point(14, 16);
            iconBox.BackColor = color;
            iconBox.Paint += (s, e) =>
            {
                var p = (Panel)s;
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var gp = RoundRect(p.ClientRectangle, 8))
                {
                    p.Region = new Region(gp);
                    using (var br = new SolidBrush(p.BackColor))
                        e.Graphics.FillPath(br, gp);
                }
            };

            Label iconLbl = new Label();
            iconLbl.Text = icon;
            iconLbl.Font = new Font("Segoe UI Emoji", 14);
            iconLbl.ForeColor = Color.White;
            iconLbl.Location = new Point(5, 6);
            iconLbl.AutoSize = true;
            iconBox.Controls.Add(iconLbl);

            Label valLbl = new Label();
            valLbl.Text = value;
            valLbl.Font = new Font("Segoe UI", 16, FontStyle.Bold);
            valLbl.ForeColor = Color.FromArgb(20, 20, 30);
            valLbl.AutoSize = true;
            valLbl.Location = new Point(66, 12);

            Label titLbl = new Label();
            titLbl.Text = title;
            titLbl.Font = new Font("Segoe UI", 8);
            titLbl.ForeColor = Color.Gray;
            titLbl.AutoSize = true;
            titLbl.Location = new Point(68, 40);

            card.Controls.Add(iconBox);
            card.Controls.Add(valLbl);
            card.Controls.Add(titLbl);

            return card;
        }

        private void BuildBottomSection()
        {
            bottomSection = new Panel();
            bottomSection.Dock = DockStyle.Fill;
            bottomSection.BackColor = Color.Transparent;

            TableLayoutPanel layout = new TableLayoutPanel();
            layout.Dock = DockStyle.Fill;
            layout.ColumnCount = 2;
            layout.RowCount = 1;
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70f));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30f));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            layout.BackColor = Color.Transparent;

            // LEFT: Blood Requests Table (shows PENDING only)
            Panel left = new Panel();
            left.Dock = DockStyle.Fill;
            left.Margin = new Padding(0, 0, 6, 0);
            left.BackColor = Color.White;
            left.Padding = new Padding(12, 10, 12, 8);
            left.Paint += (s, e) => PaintCard((Panel)s, e.Graphics);

            Label tblTitle = new Label();
            tblTitle.Text = "Recent Blood Requests";
            tblTitle.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            tblTitle.ForeColor = Color.FromArgb(20, 20, 20);
            tblTitle.Dock = DockStyle.Top;
            tblTitle.Height = 30;
            tblTitle.TextAlign = ContentAlignment.MiddleLeft;

            DataGridView dgv = new DataGridView();
            dgv.Name = "dgvRequests";
            dgv.Dock = DockStyle.Fill;
            dgv.BackgroundColor = Color.White;
            dgv.BorderStyle = BorderStyle.None;
            dgv.RowHeadersVisible = false;
            dgv.AllowUserToAddRows = false;
            dgv.AllowUserToResizeRows = false;
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgv.ColumnHeadersHeight = 30;
            dgv.RowTemplate.Height = 32;
            dgv.GridColor = Color.FromArgb(235, 235, 238);
            dgv.EnableHeadersVisualStyles = false;

            dgv.DefaultCellStyle.Font = new Font("Segoe UI", 9);
            dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(238, 242, 255);
            dgv.DefaultCellStyle.SelectionForeColor = Color.FromArgb(35, 35, 40);
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(248, 249, 251);
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(90, 90, 100);

            dgv.Columns.Add("BloodGroup", "Blood Group");
            dgv.Columns.Add("Units", "Units");
            dgv.Columns.Add("Hospital", "Hospital");
            dgv.Columns.Add("Status", "Status");

            dgv.CellFormatting += (s, e) =>
            {
                if (e.ColumnIndex == 3 && e.Value != null)
                {
                    bool pending = e.Value.ToString() == "Pending";
                    e.CellStyle.ForeColor = pending ? Color.FromArgb(146, 64, 14) : Color.FromArgb(6, 95, 70);
                    e.CellStyle.BackColor = pending ? Color.FromArgb(254, 243, 199) : Color.FromArgb(209, 250, 229);
                    e.CellStyle.SelectionForeColor = e.CellStyle.ForeColor;
                    e.CellStyle.SelectionBackColor = e.CellStyle.BackColor;
                }
            };

            left.Controls.Add(dgv);
            left.Controls.Add(tblTitle);

            // RIGHT: Recent Activity
            Panel right = new Panel();
            right.Dock = DockStyle.Fill;
            right.Margin = new Padding(6, 0, 0, 0);
            right.BackColor = Color.White;
            right.Padding = new Padding(10, 10, 10, 8);
            right.Paint += (s, e) => PaintCard((Panel)s, e.Graphics);

            Label actTitle = new Label();
            actTitle.Text = "Recent Activity";
            actTitle.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            actTitle.ForeColor = Color.FromArgb(20, 20, 20);
            actTitle.Dock = DockStyle.Top;
            actTitle.Height = 30;
            actTitle.TextAlign = ContentAlignment.MiddleLeft;

            FlowLayoutPanel actFlow = new FlowLayoutPanel();
            actFlow.Name = "actFlow";
            actFlow.Dock = DockStyle.Fill;
            actFlow.FlowDirection = FlowDirection.TopDown;
            actFlow.WrapContents = false;
            actFlow.AutoScroll = true;
            actFlow.Padding = new Padding(0, 4, 0, 0);

            right.Controls.Add(actFlow);
            right.Controls.Add(actTitle);

            layout.Controls.Add(left, 0, 0);
            layout.Controls.Add(right, 1, 0);

            bottomSection.Controls.Add(layout);
        }

        private void PaintCard(Panel p, Graphics g)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using (GraphicsPath gp = RoundRect(p.ClientRectangle, 8))
            using (SolidBrush brush = new SolidBrush(Color.White))
            using (Pen border = new Pen(Color.FromArgb(18, 0, 0, 0), 1f))
            {
                g.FillPath(brush, gp);
                g.DrawPath(border, gp);
                p.Region = new Region(gp);
            }
        }

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