// ============================================================
// FILE: TechnicianDashboard.cs
// ============================================================

using BloodBankManagementSystem.Classes.Database;
using BloodBankManagementSystem.Classes.Helpers;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BloodBankManagementSystem.Forms.Technician
{
    public partial class TechnicianDashboard : Form
    {
        private static TechnicianDashboard _instance;
        private Panel pnlSidebar, pnlTopBar, pnlContent;
        private Panel pnlStats;
        private DataGridView dgvRecent;
        private Timer refreshTimer;
        private Label lblLastUpdated;
        private Label[] statLabels;
        private bool isLoading = false;

        // Colors
        private readonly Color brickRed = Color.FromArgb(120, 22, 27);
        private readonly Color C_White = Color.White;
        private readonly Color C_ContentBg = Color.FromArgb(245, 247, 250);
        private readonly Color C_CardBg = Color.White;
        private readonly Color C_TextDark = Color.FromArgb(31, 41, 55);
        private readonly Color C_TextMid = Color.FromArgb(107, 114, 128);
        private readonly Color C_SidebarBg = Color.FromArgb(22, 33, 62);
        private readonly Color C_SidebarHov = Color.FromArgb(50, 65, 85);

        public static TechnicianDashboard Instance
        {
            get
            {
                if (_instance == null || _instance.IsDisposed)
                    _instance = new TechnicianDashboard();
                return _instance;
            }
        }

        public TechnicianDashboard()
        {
            this.Text = "Blood Bank Management System – Lab Technician Dashboard";
            this.BackColor = C_ContentBg;
            this.WindowState = FormWindowState.Maximized;
            this.MinimumSize = new Size(900, 600);

            BuildLayout();
            BuildSidebar();
            BuildTopBar();
            BuildContentArea();

            this.Shown += (s, e) => RecalcContentWidths();

            _ = LoadAllDashboardDataAsync();

            refreshTimer = new Timer { Interval = 60000 };
            refreshTimer.Tick += async (s, e) => await RefreshDashboardAsync();
            refreshTimer.Start();
        }

        // =========================================================
        // LAYOUT
        // =========================================================

        private void BuildLayout()
        {
            // Sidebar — DockStyle.Left, added FIRST
            pnlSidebar = new Panel
            {
                Dock = DockStyle.Left,
                Width = 220,
                BackColor = C_SidebarBg,
                AutoScroll = true
            };
            this.Controls.Add(pnlSidebar);

            // Right-side container
            Panel pnlRight = new Panel { Dock = DockStyle.Fill, BackColor = C_ContentBg };
            this.Controls.Add(pnlRight);

            // Fill content — added FIRST inside pnlRight
            pnlContent = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = C_ContentBg,
                AutoScroll = true,
                Padding = new Padding(0)
            };
            pnlRight.Controls.Add(pnlContent);

            // Separator
            Panel pnlSep = new Panel
            {
                Dock = DockStyle.Top,
                Height = 1,
                BackColor = Color.FromArgb(229, 231, 235)
            };
            pnlRight.Controls.Add(pnlSep);

            // Top bar — added LAST so it docks at top
            pnlTopBar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = C_White
            };
            pnlRight.Controls.Add(pnlTopBar);
        }

        // =========================================================
        // SIDEBAR
        // =========================================================

        private void BuildSidebar()
        {
            int y = 0;

            // ── Logo block — full width, brick red background ────
            Panel logoPanel = new Panel
            {
                Size = new Size(220, 80),
                Location = new Point(0, y),
                BackColor = brickRed
            };
            pnlSidebar.Controls.Add(logoPanel);

            // Blood drop icon label
            Label lblIcon = new Label
            {
                Text = "🩸",
                Font = new Font("Segoe UI", 20),
                ForeColor = C_White,
                AutoSize = true,
                Location = new Point(18, 22),
                BackColor = Color.Transparent
            };
            logoPanel.Controls.Add(lblIcon);

            // Title stacked
            Label lblTitle1 = new Label
            {
                Text = "Blood Bank",
                Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                ForeColor = C_White,
                AutoSize = true,
                Location = new Point(58, 18),
                BackColor = Color.Transparent
            };
            logoPanel.Controls.Add(lblTitle1);

            Label lblTitle2 = new Label
            {
                Text = "Management System",
                Font = new Font("Segoe UI", 8f),
                ForeColor = Color.FromArgb(255, 200, 200),
                AutoSize = true,
                Location = new Point(58, 42),
                BackColor = Color.Transparent
            };
            logoPanel.Controls.Add(lblTitle2);

            y += 90;

            // ── Section label ────────────────────────────────────
            pnlSidebar.Controls.Add(new Label
            {
                Text = "MAIN MENU",
                Location = new Point(32, y),
                AutoSize = true,
                ForeColor = Color.FromArgb(148, 163, 184),
                Font = new Font("Segoe UI", 7.5f, FontStyle.Bold)
            });
            y += 26;

            // ── Menu items ───────────────────────────────────────
            var menuItems = new (string text, string icon, Type formType)[]
            {
                ("Dashboard",        "🏠", null),
                ("Assign Storage",   "📦", typeof(AssignStorageLocation)),
                ("Medical Screening","🩺", typeof(DonorMedicalScreening)),
                ("Receive Bag",      "📥", typeof(ReceiveBag)),
                ("Component Prep",   "⚙️", typeof(ComponentPreparation)),
                ("Blood Collection", "🩸", typeof(BloodCollectionDetails)),
                ("Defer Donor",      "🚫", typeof(DeferDonor)),
                ("Discard Blood",    "🗑️", typeof(DiscardBlood)),
                ("Print Labels",     "🖨️", typeof(PrintBarcodeLabels)),
                ("Quality Control",  "🔬", typeof(QualityControl)),
                ("TTI Screening",    "🧪", typeof(TTIScreeningResults)),
                ("Cross Matching",   "🔄", typeof(TechnicianCrossMatching)),
                ("Bag Tracking",     "📊", typeof(BloodBagTracking)),
                ("Expiry Alerts",    "⚠️", typeof(ExpiryAlerts)),
                ("Inventory",        "📋", typeof(RecentInventoryStatus))
            };

            bool isFirst = true;
            foreach (var item in menuItems)
            {
                bool isDashboard = isFirst;
                Button btn = new Button
                {
                    Text = $"   {item.icon}   {item.text}",
                    Size = new Size(190, 38),
                    Location = new Point(14, y),
                    FlatStyle = FlatStyle.Flat,
                    FlatAppearance = { BorderSize = 0 },
                    ForeColor = isDashboard ? C_White : Color.FromArgb(203, 213, 225),
                    BackColor = isDashboard ? C_SidebarHov : Color.Transparent,
                    TextAlign = ContentAlignment.MiddleLeft,
                    Font = new Font("Segoe UI", 9f),
                    Cursor = Cursors.Hand,
                    Padding = new Padding(6, 0, 0, 0)
                };

                btn.MouseEnter += (s, e) => { btn.BackColor = C_SidebarHov; btn.ForeColor = C_White; };
                btn.MouseLeave += (s, e) =>
                {
                    if (!isDashboard)
                    {
                        btn.BackColor = Color.Transparent;
                        btn.ForeColor = Color.FromArgb(203, 213, 225);
                    }
                };

                if (item.formType != null)
                    btn.Click += (s, e) => OpenForm(item.formType);

                pnlSidebar.Controls.Add(btn);
                y += 42;
                isFirst = false;
            }

            // ── Divider ──────────────────────────────────────────
            y += 10;
            pnlSidebar.Controls.Add(new Panel
            {
                Size = new Size(200, 1),
                Location = new Point(10, y),
                BackColor = Color.FromArgb(50, 65, 85)
            });
            y += 18;

            // ── Logout ───────────────────────────────────────────
            Button btnLogout = new Button
            {
                Text = "   ⏻   Logout",
                Size = new Size(190, 38),
                Location = new Point(14, y),
                FlatStyle = FlatStyle.Flat,
                FlatAppearance = { BorderSize = 0 },
                ForeColor = Color.FromArgb(252, 165, 165),
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Padding = new Padding(6, 0, 0, 0)
            };
            btnLogout.MouseEnter += (s, e) => { btnLogout.BackColor = Color.FromArgb(100, 30, 30); btnLogout.ForeColor = C_White; };
            btnLogout.MouseLeave += (s, e) => { btnLogout.BackColor = Color.Transparent; btnLogout.ForeColor = Color.FromArgb(252, 165, 165); };
            btnLogout.Click += (s, e) =>
            {
                SessionManager.Logout();
                var loginForm = new Shared.LoginForm();
                loginForm.Show();
                this.Close();
            };
            pnlSidebar.Controls.Add(btnLogout);
        }

        [System.Runtime.InteropServices.DllImport("Gdi32.dll")]
        private static extern IntPtr CreateRoundRectRgn(
            int nLeftRect, int nTopRect, int nRightRect, int nBottomRect,
            int nWidthEllipse, int nHeightEllipse);

        // =========================================================
        // TOP BAR
        // =========================================================

        private void BuildTopBar()
        {
            Label lblPage = new Label
            {
                Text = "Dashboard",
                Font = new Font("Segoe UI", 14f, FontStyle.Bold),
                ForeColor = C_TextDark,
                AutoSize = true,
                Location = new Point(28, 18)
            };
            pnlTopBar.Controls.Add(lblPage);

            Label lblDate = new Label
            {
                Text = DateTime.Now.ToString("dddd, dd MMMM yyyy"),
                Font = new Font("Segoe UI", 9f),
                ForeColor = C_TextMid,
                AutoSize = true,
                Location = new Point(pnlTopBar.Width - 210, 26)
            };
            pnlTopBar.Controls.Add(lblDate);

            Button btnRefresh = new Button
            {
                Text = "🔄  Refresh",
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                BackColor = brickRed,
                ForeColor = C_White,
                Size = new Size(90, 30),
                Location = new Point(pnlTopBar.Width - 330, 18),
                Cursor = Cursors.Hand
            };
            btnRefresh.FlatAppearance.BorderSize = 0;
            btnRefresh.Click += async (s, e) => await RefreshDashboardAsync();
            pnlTopBar.Controls.Add(btnRefresh);

            pnlTopBar.Resize += (s, e) =>
            {
                lblDate.Location = new Point(pnlTopBar.Width - 210, 26);
                btnRefresh.Location = new Point(pnlTopBar.Width - 330, 18);
            };
        }

        // =========================================================
        // CONTENT AREA
        // =========================================================

        private void BuildContentArea()
        {
            const int MX = 20;
            int y = 20;

            string fullName = SessionManager.CurrentFullName ?? "Technician";

            lblLastUpdated = new Label
            {
                Text = "🕐 Loading...",
                Font = new Font("Segoe UI", 8.5f),
                ForeColor = C_TextMid,
                Location = new Point(MX, y),
                AutoSize = true,
                Visible = false
            };
            pnlContent.Controls.Add(lblLastUpdated);

            // Stats panel — width set properly in RecalcContentWidths
            pnlStats = new Panel
            {
                Location = new Point(MX, y),
                Height = 68,
                Width = 200, // placeholder; fixed in RecalcContentWidths
                BackColor = Color.Transparent
            };
            pnlContent.Controls.Add(pnlStats);
            y += 90;

            // Recent Activities label
            Label lblRecent = new Label
            {
                Text = "Recent Activities",
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                ForeColor = C_TextDark,
                Location = new Point(MX, y),
                AutoSize = true
            };
            pnlContent.Controls.Add(lblRecent);
            y += 28;

            dgvRecent = CreateStyledGrid();
            dgvRecent.Location = new Point(MX, y);
            dgvRecent.Height = 300;
            dgvRecent.Width = 200; // placeholder; fixed in RecalcContentWidths
            pnlContent.Controls.Add(dgvRecent);

            // Resize: recalculate widths
            pnlContent.Resize += (s, e) => RecalcContentWidths();
        }

        /// <summary>
        /// Calculates correct widths for stats panel and grid based on pnlContent's actual client width.
        /// Called from Shown event and every Resize.
        /// </summary>
        private void RecalcContentWidths()
        {
            if (pnlContent == null || pnlContent.IsDisposed) return;

            int w = pnlContent.ClientSize.Width - 40; // 20px margin each side
            if (w < 100) return;

            if (pnlStats != null && !pnlStats.IsDisposed)
            {
                pnlStats.Width = w;
                CreateStatCards();
            }
            if (dgvRecent != null && !dgvRecent.IsDisposed)
                dgvRecent.Width = w;
        }

        // =========================================================
        // STAT CARDS
        // =========================================================

        private string[] _cachedStatValues = new string[] { "—", "—", "—", "—", "—", "—", "—" };

        private void CreateStatCards()
        {
            if (pnlStats == null || pnlStats.IsDisposed) return;
            pnlStats.Controls.Clear();

            string[] titles = {
                "TTI Screenings", "Available Bags", "Pending Reqs",
                "Expiring (7d)", "Total Donors"
            };

            int[] valueIndices = { 1, 2, 3, 4, 6 };

            Color[] accentColors = {
                Color.FromArgb(139, 92,  246),
                Color.FromArgb(16,  185, 129),
                Color.FromArgb(245, 158, 11),
                Color.FromArgb(239, 68,  68),
                brickRed
            };

            int count = titles.Length;
            int spacing = 12;
            int cardH = 68;

            int panelW = pnlStats.Width;
            if (panelW < 100) return;

            int cardW = (panelW - spacing * (count - 1)) / count;
            cardW = Math.Max(80, cardW);

            pnlStats.Height = cardH;

            statLabels = new Label[7];

            for (int i = 0; i < count; i++)
            {
                int xPos = i * (cardW + spacing);
                int vi = valueIndices[i];

                Panel card = new Panel
                {
                    Size = new Size(cardW, cardH),
                    Location = new Point(xPos, 0),
                    BackColor = C_White,
                    BorderStyle = BorderStyle.FixedSingle
                };

                Panel accentBar = new Panel
                {
                    Dock = DockStyle.Left,
                    Width = 4,
                    BackColor = accentColors[i]
                };
                card.Controls.Add(accentBar);

                Label valueLbl = new Label
                {
                    Text = _cachedStatValues[vi],
                    Font = new Font("Segoe UI", 18, FontStyle.Bold),
                    ForeColor = accentColors[i],
                    Location = new Point(14, 8),
                    AutoSize = true
                };
                card.Controls.Add(valueLbl);
                statLabels[vi] = valueLbl;

                Label titleLbl = new Label
                {
                    Text = titles[i],
                    Font = new Font("Segoe UI", 7.5f),
                    ForeColor = C_TextMid,
                    Location = new Point(14, 44),
                    AutoSize = true
                };
                card.Controls.Add(titleLbl);

                pnlStats.Controls.Add(card);
            }
        }

        // =========================================================
        // DATABASE METHODS
        // =========================================================

        private async Task LoadAllDashboardDataAsync()
        {
            if (isLoading) return;
            isLoading = true;

            try
            {
                SafeSetText(lblLastUpdated, "🕐 Loading data...");

                var stats = await GetDashboardStatsAsync();

                _cachedStatValues = new string[] {
                    stats.BagsReceivedToday.ToString(),
                    stats.TTIScreeningsToday.ToString(),
                    stats.AvailableBags.ToString(),
                    stats.PendingRequisitions.ToString(),
                    stats.ExpiringSoon.ToString(),
                    stats.TodayAppointments.ToString(),
                    stats.TotalDonors.ToString()
                };

                if (statLabels != null && statLabels.Length >= 7)
                {
                    for (int i = 0; i < 7; i++)
                        SafeSetText(statLabels[i], _cachedStatValues[i]);
                }

                var recentData = await GetRecentActivitiesAsync(15);
                if (dgvRecent != null && !dgvRecent.IsDisposed)
                {
                    if (dgvRecent.InvokeRequired)
                        dgvRecent.Invoke(new Action(() => { dgvRecent.DataSource = recentData; FormatRecentGrid(); }));
                    else
                    { dgvRecent.DataSource = recentData; FormatRecentGrid(); }
                }

                SafeSetText(lblLastUpdated, $"🕐 Last Updated: {DateTime.Now:dd-MMM-yyyy  HH:mm:ss}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LoadAllDashboardDataAsync Error: {ex.Message}");
                SafeSetText(lblLastUpdated, "⚠️ Error loading data. Click Refresh to retry.");
            }
            finally
            {
                isLoading = false;
            }
        }

        private void SafeSetText(Label lbl, string text)
        {
            if (lbl == null || lbl.IsDisposed) return;
            if (lbl.InvokeRequired)
                lbl.Invoke(new Action(() => lbl.Text = text));
            else
                lbl.Text = text;
        }

        private async Task<DashboardStatsData> GetDashboardStatsAsync()
        {
            var stats = new DashboardStatsData();

            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    await conn.OpenAsync();

                    string sql = @"
                        SELECT
                            (SELECT COUNT(*) FROM BloodBags
                             WHERE CAST(CollectionDate AS DATE) = CAST(GETDATE() AS DATE))          AS BagsReceivedToday,
                            (SELECT COUNT(*) FROM TTIScreeningResults
                             WHERE CAST(TestDate AS DATE) = CAST(GETDATE() AS DATE))                AS TTIScreeningsToday,
                            (SELECT COUNT(*) FROM BloodBags WHERE Status = 'Available')             AS AvailableBags,
                            (SELECT COUNT(*) FROM Requisitions WHERE Status = 'Pending')            AS PendingRequisitions,
                            (SELECT COUNT(*) FROM BloodBags
                             WHERE ExpiryDate BETWEEN GETDATE() AND DATEADD(DAY,7,GETDATE())
                               AND Status = 'Available')                                            AS ExpiringSoon,
                            (SELECT COUNT(*) FROM Appointments
                             WHERE CAST(AppointmentDate AS DATE) = CAST(GETDATE() AS DATE)
                               AND Status = 'Confirmed')                                            AS TodayAppointments,
                            (SELECT COUNT(*) FROM Donors WHERE IsActive = 1)                        AS TotalDonors";

                    using (var cmd = new SqlCommand(sql, conn))
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            stats.BagsReceivedToday = GetInt(reader, "BagsReceivedToday");
                            stats.TTIScreeningsToday = GetInt(reader, "TTIScreeningsToday");
                            stats.AvailableBags = GetInt(reader, "AvailableBags");
                            stats.PendingRequisitions = GetInt(reader, "PendingRequisitions");
                            stats.ExpiringSoon = GetInt(reader, "ExpiringSoon");
                            stats.TodayAppointments = GetInt(reader, "TodayAppointments");
                            stats.TotalDonors = GetInt(reader, "TotalDonors");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetDashboardStatsAsync Error: {ex.Message}");
            }

            return stats;
        }

        private async Task<DataTable> GetRecentActivitiesAsync(int topRows = 15)
        {
            DataTable dt = new DataTable();

            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    await conn.OpenAsync();

                    string sql = @"
                        SELECT TOP (@TopRows)
                            'Blood Bag'    AS Type,
                            BagID          AS ID,
                            ISNULL(DonorName,'N/A') AS Name,
                            BloodGroup,
                            Status,
                            FORMAT(CollectionDate,'dd-MMM HH:mm') AS ActivityDateTime
                        FROM BloodBags

                        UNION ALL

                        SELECT TOP (@TopRows)
                            'TTI Screening',
                            CAST(TestID AS NVARCHAR),
                            ISNULL(DonorName,'N/A'),
                            OverallResult,
                            OverallResult,
                            FORMAT(TestDate,'dd-MMM HH:mm')
                        FROM TTIScreeningResults

                        UNION ALL

                        SELECT TOP (@TopRows)
                            'Requisition',
                            RequisitionNumber,
                            PatientName,
                            BloodGroup,
                            Status,
                            FORMAT(RequestDate,'dd-MMM HH:mm')
                        FROM Requisitions

                        ORDER BY ActivityDateTime DESC";

                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@TopRows", topRows);
                        using (var da = new SqlDataAdapter(cmd))
                            da.Fill(dt);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetRecentActivitiesAsync Error: {ex.Message}");
                foreach (string col in new[] { "Type", "ID", "Name", "BloodGroup", "Status", "ActivityDateTime" })
                    dt.Columns.Add(col);
            }

            return dt;
        }

        private int GetInt(SqlDataReader reader, string columnName)
        {
            try
            {
                int ordinal = reader.GetOrdinal(columnName);
                return reader.IsDBNull(ordinal) ? 0 : reader.GetInt32(ordinal);
            }
            catch { return 0; }
        }

        private async Task RefreshDashboardAsync()
        {
            await LoadAllDashboardDataAsync();
        }

        // =========================================================
        // GRID HELPERS
        // =========================================================

        private void FormatRecentGrid()
        {
            if (dgvRecent == null || dgvRecent.Columns.Count == 0) return;

            try
            {
                var headers = new System.Collections.Generic.Dictionary<string, string>
                {
                    { "Type",             "Type"        },
                    { "ID",               "ID"          },
                    { "Name",             "Name"        },
                    { "BloodGroup",       "Blood Group" },
                    { "Status",           "Status"      },
                    { "ActivityDateTime", "Date / Time" }
                };

                foreach (var kv in headers)
                    if (dgvRecent.Columns.Contains(kv.Key))
                        dgvRecent.Columns[kv.Key].HeaderText = kv.Value;

                dgvRecent.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"FormatRecentGrid Error: {ex.Message}");
            }
        }

        private DataGridView CreateStyledGrid()
        {
            var dgv = new DataGridView
            {
                AllowUserToAddRows = false,
                ReadOnly = true,
                BackgroundColor = C_White,
                BorderStyle = BorderStyle.FixedSingle,
                RowHeadersVisible = false,
                EnableHeadersVisualStyles = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                RowTemplate = { Height = 34 },
                ScrollBars = ScrollBars.Both,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };

            dgv.ColumnHeadersDefaultCellStyle.BackColor = brickRed;
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = C_White;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            dgv.ColumnHeadersDefaultCellStyle.Padding = new Padding(8, 0, 8, 0);
            dgv.ColumnHeadersHeight = 36;

            dgv.DefaultCellStyle.Font = new Font("Segoe UI", 9.5f);
            dgv.DefaultCellStyle.Padding = new Padding(8, 4, 8, 4);
            dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(220, 38, 38);
            dgv.DefaultCellStyle.SelectionForeColor = C_White;

            dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(249, 250, 251);

            return dgv;
        }

        // =========================================================
        // NAVIGATION
        // =========================================================

        private void OpenForm(Type formType)
        {
            try
            {
                Form form = (Form)Activator.CreateInstance(formType);
                form.StartPosition = FormStartPosition.CenterScreen;
                form.ShowDialog();
                _ = LoadAllDashboardDataAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening form: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // =========================================================
        // CLEANUP
        // =========================================================

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            refreshTimer?.Stop();
            refreshTimer?.Dispose();
            base.OnFormClosing(e);
        }
    }

    // ── Dashboard stats data model ────────────────────────────────
    public class DashboardStatsData
    {
        public int BagsReceivedToday { get; set; }
        public int TTIScreeningsToday { get; set; }
        public int AvailableBags { get; set; }
        public int PendingRequisitions { get; set; }
        public int ExpiringSoon { get; set; }
        public int TodayAppointments { get; set; }
        public int TotalDonors { get; set; }
    }
}