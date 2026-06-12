using BloodBankManagementSystem.Classes.Helpers;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace BloodBankManagementSystem.Forms.Admin
{
    public class RolesPermissions : Form
    {
        private Panel sidebarPanel;
        private Panel mainPanel;
        private CheckBox[,] permissionCheckboxes;
        private string[] modules = { "Dashboard", "User Management", "Hospitals", "Reports", "Settings", "Blood Components" };

        public RolesPermissions()
        {
            this.Text = "Roles & Permissions - Blood Bank";
            this.WindowState = FormWindowState.Maximized;
            this.BackColor = Color.FromArgb(244, 246, 249);
            this.MinimumSize = new Size(1100, 650);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.AutoScaleMode = AutoScaleMode.Dpi;

            BuildMainContent();
            BuildSidebar();
        }

        // =====================================================
        // SIDEBAR
        // =====================================================
        private void BuildSidebar()
        {
            sidebarPanel = new Panel();
            sidebarPanel.Dock = DockStyle.Left;
            sidebarPanel.Width = 220;
            sidebarPanel.BackColor = Color.FromArgb(5, 31, 64);

            // Logo Panel
            Panel logoPanel = new Panel();
            logoPanel.Dock = DockStyle.Top;
            logoPanel.Height = 70;
            logoPanel.BackColor = Color.FromArgb(5, 31, 64);

            Label bloodIcon = new Label();
            bloodIcon.Text = "🩸";
            bloodIcon.Font = new Font("Segoe UI Emoji", 22, FontStyle.Regular);
            bloodIcon.ForeColor = Color.FromArgb(220, 50, 50);
            bloodIcon.BackColor = Color.Transparent;
            bloodIcon.AutoSize = true;
            bloodIcon.Location = new Point(16, 18);
            bloodIcon.TextAlign = ContentAlignment.MiddleCenter;

            Label logoText = new Label();
            logoText.Text = "Blood Bank";
            logoText.Font = new Font("Segoe UI", 16, FontStyle.Bold);
            logoText.ForeColor = Color.White;
            logoText.BackColor = Color.Transparent;
            logoText.AutoSize = true;
            logoText.Location = new Point(66, 22);
            logoText.TextAlign = ContentAlignment.MiddleLeft;

            logoPanel.Controls.Add(bloodIcon);
            logoPanel.Controls.Add(logoText);

            // Menu Panel
            FlowLayoutPanel menuPanel = new FlowLayoutPanel();
            menuPanel.Dock = DockStyle.Fill;
            menuPanel.FlowDirection = FlowDirection.TopDown;
            menuPanel.WrapContents = false;
            menuPanel.AutoScroll = true;
            menuPanel.Padding = new Padding(8, 6, 8, 8);
            menuPanel.BackColor = Color.Transparent;



            // ============================================
            // BEST VERSION - WITH TRY-CATCH
            // ============================================
            AddSidebarMenuItem(menuPanel, "🏠", "Dashboard", false, () =>
            {
                AdminDashboard dashboard = new AdminDashboard();
                dashboard.Show();
                this.Close();
            });

            // User Management
            AddSidebarMenuItem(menuPanel, "👥", "User Management", false, () =>
            {
                using (UserManagement userMgmt = new UserManagement())
                {
                    this.Hide();
                    userMgmt.ShowDialog();
                    this.Show();
                }
            });

            // Roles & Permissions (Active)
            AddSidebarMenuItem(menuPanel, "🔐", "Roles & Permissions", true, null);

            // Hospitals
            AddSidebarMenuItem(menuPanel, "🏥", "Hospitals", false, () =>
            {
                using (HospitalManagement hospitalForm = new HospitalManagement())
                {
                    this.Hide();
                    hospitalForm.ShowDialog();
                    this.Show();
                }
            });

            // Blood Components
            AddSidebarMenuItem(menuPanel, "🩸", "Blood Components", false, () =>
            {
                using (BloodComponents bloodForm = new BloodComponents())
                {
                    this.Hide();
                    bloodForm.ShowDialog();
                    this.Show();
                }
            });

            // Reports
            AddSidebarMenuItem(menuPanel, "📊", "Reports", false, () =>
            {
                using (Reports reportsForm = new Reports())
                {
                    this.Hide();
                    reportsForm.ShowDialog();
                    this.Show();
                }
            });

            // Settings
            AddSidebarMenuItem(menuPanel, "⚙", "Settings", false, () =>
            {
                using (Settings settingsForm = new Settings())
                {
                    this.Hide();
                    settingsForm.ShowDialog();
                    this.Show();
                }
            });


            sidebarPanel.Controls.Add(menuPanel);
            sidebarPanel.Controls.Add(logoPanel);
            this.Controls.Add(sidebarPanel);
        }

        private void AddSidebarMenuItem(FlowLayoutPanel panel, string icon, string text, bool active, Action clickAction)
        {
            Panel item = new Panel();
            item.Size = new Size(200, 42);
            item.Margin = new Padding(0, 2, 0, 2);
            item.Cursor = Cursors.Hand;
            item.BackColor = active ? Color.FromArgb(230, 57, 70) : Color.Transparent;

            Label iconLbl = new Label();
            iconLbl.Text = icon;
            iconLbl.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            iconLbl.ForeColor = Color.White;
            iconLbl.BackColor = Color.Transparent;
            iconLbl.AutoSize = false;
            iconLbl.Size = new Size(25, 25);
            iconLbl.Location = new Point(12, 9);
            iconLbl.TextAlign = ContentAlignment.MiddleCenter;

            Label textLbl = new Label();
            textLbl.Text = text;
            textLbl.Font = new Font("Segoe UI", 10);
            textLbl.ForeColor = Color.White;
            textLbl.BackColor = Color.Transparent;
            textLbl.AutoSize = false;
            textLbl.Size = new Size(150, 25);
            textLbl.Location = new Point(42, 9);
            textLbl.TextAlign = ContentAlignment.MiddleLeft;

            EventHandler enter = (s, e) => { if (!active) item.BackColor = Color.FromArgb(200, 57, 70); };
            EventHandler leave = (s, e) => { if (!active) item.BackColor = Color.Transparent; };

            if (clickAction != null)
            {
                EventHandler click = (s, e) => clickAction();
                item.Click += click;
                iconLbl.Click += click;
                textLbl.Click += click;
            }

            iconLbl.MouseEnter += enter;
            iconLbl.MouseLeave += leave;
            textLbl.MouseEnter += enter;
            textLbl.MouseLeave += leave;
            item.MouseEnter += enter;
            item.MouseLeave += leave;

            item.Controls.Add(iconLbl);
            item.Controls.Add(textLbl);
            panel.Controls.Add(item);
        }

        // =====================================================
        // MAIN CONTENT
        // =====================================================
        private void BuildMainContent()
        {
            mainPanel = new Panel();
            mainPanel.Dock = DockStyle.Fill;
            mainPanel.BackColor = Color.FromArgb(244, 246, 249);
            mainPanel.Padding = new Padding(20, 15, 20, 15);

            // Title
            Label titleLabel = new Label();
            titleLabel.Text = "Roles & Permissions";
            titleLabel.Font = new Font("Segoe UI", 22, FontStyle.Bold);
            titleLabel.ForeColor = Color.FromArgb(17, 24, 39);
            titleLabel.BackColor = Color.Transparent;
            titleLabel.Dock = DockStyle.Top;
            titleLabel.Height = 50;
            titleLabel.TextAlign = ContentAlignment.MiddleLeft;

            // Card Panel
            Panel cardPanel = new Panel();
            cardPanel.Dock = DockStyle.Fill;
            cardPanel.BackColor = Color.White;
            cardPanel.Padding = new Padding(20);
            cardPanel.Paint += (s, e) =>
            {
                Panel p = (Panel)s;
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (GraphicsPath gp = RoundRect(p.ClientRectangle, 12))
                {
                    p.Region = new Region(gp);
                    using (Pen border = new Pen(Color.FromArgb(220, 220, 225), 1f))
                        e.Graphics.DrawPath(border, gp);
                }
            };

            // Card Title
            Label cardTitle = new Label();
            cardTitle.Text = "Admin Permissions";
            cardTitle.Font = new Font("Segoe UI", 16, FontStyle.Bold);
            cardTitle.ForeColor = Color.FromArgb(17, 24, 39);
            cardTitle.BackColor = Color.Transparent;
            cardTitle.Dock = DockStyle.Top;
            cardTitle.Height = 40;
            cardTitle.TextAlign = ContentAlignment.MiddleLeft;

            // Permissions Table
            TableLayoutPanel permTable = BuildPermissionsTable();
            permTable.Dock = DockStyle.Top;
            permTable.Height = 350;

            // Save Button
            Button saveButton = new Button();
            saveButton.Text = "Save Permissions";
            saveButton.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            saveButton.ForeColor = Color.White;
            saveButton.BackColor = Color.FromArgb(230, 57, 70);
            saveButton.FlatStyle = FlatStyle.Flat;
            saveButton.FlatAppearance.BorderSize = 0;
            saveButton.Size = new Size(180, 42);
            saveButton.Cursor = Cursors.Hand;
            saveButton.Dock = DockStyle.Top;
            saveButton.Margin = new Padding(0, 15, 0, 0);
            saveButton.Paint += (s, e) =>
            {
                Button btn = (Button)s;
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (GraphicsPath gp = RoundRect(btn.ClientRectangle, 8))
                {
                    btn.Region = new Region(gp);
                }
            };
            saveButton.Click += (s, e) =>
            {
                SavePermissions();
            };
            saveButton.MouseEnter += (s, e) => saveButton.BackColor = Color.FromArgb(200, 40, 50);
            saveButton.MouseLeave += (s, e) => saveButton.BackColor = Color.FromArgb(230, 57, 70);

            cardPanel.Controls.Add(saveButton);
            cardPanel.Controls.Add(permTable);
            cardPanel.Controls.Add(cardTitle);

            mainPanel.Controls.Add(cardPanel);
            mainPanel.Controls.Add(titleLabel);
            this.Controls.Add(mainPanel);
        }

        private TableLayoutPanel BuildPermissionsTable()
        {
            TableLayoutPanel table = new TableLayoutPanel();
            table.ColumnCount = 5;
            table.RowCount = modules.Length + 1; // Header + modules
            table.BackColor = Color.White;
            table.CellBorderStyle = TableLayoutPanelCellBorderStyle.Single;

            // Column widths
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30f)); // Module name
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 17.5f)); // View
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 17.5f)); // Add
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 17.5f)); // Edit
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 17.5f)); // Delete

            // Row styles
            table.RowStyles.Add(new RowStyle(SizeType.Absolute, 45)); // Header
            for (int i = 0; i < modules.Length; i++)
                table.RowStyles.Add(new RowStyle(SizeType.Absolute, 45));

            // Headers
            string[] headers = { "Module", "View", "Add", "Edit", "Delete" };
            for (int col = 0; col < headers.Length; col++)
            {
                Label headerLabel = new Label();
                headerLabel.Text = headers[col];
                headerLabel.Font = new Font("Segoe UI", 10, FontStyle.Bold);
                headerLabel.ForeColor = Color.FromArgb(50, 50, 60);
                headerLabel.BackColor = Color.FromArgb(243, 244, 246);
                headerLabel.Dock = DockStyle.Fill;
                headerLabel.TextAlign = ContentAlignment.MiddleCenter;
                table.Controls.Add(headerLabel, col, 0);
            }

            // Permission checkboxes
            permissionCheckboxes = new CheckBox[modules.Length, 4];

            // Default permissions (Admin ke liye)
            bool[,] defaultPerms = {
                { true,  false, false, false }, // Dashboard
                { true,  true,  true,  true  }, // User Management
                { true,  false, false, false }, // Hospitals
                { true,  false, false, false }, // Reports
                { true,  false, false, false }, // Settings
                { true,  false, false, false }  // Blood Components
            };

            for (int row = 0; row < modules.Length; row++)
            {
                // Module name
                Label moduleLabel = new Label();
                moduleLabel.Text = modules[row];
                moduleLabel.Font = new Font("Segoe UI", 10);
                moduleLabel.ForeColor = Color.FromArgb(30, 30, 40);
                moduleLabel.BackColor = row % 2 == 0 ? Color.White : Color.FromArgb(250, 251, 253);
                moduleLabel.Dock = DockStyle.Fill;
                moduleLabel.TextAlign = ContentAlignment.MiddleLeft;
                moduleLabel.Padding = new Padding(15, 0, 0, 0);
                table.Controls.Add(moduleLabel, 0, row + 1);

                // Checkboxes
                for (int col = 0; col < 4; col++)
                {
                    CheckBox chk = new CheckBox();
                    chk.Checked = defaultPerms[row, col];
                    chk.BackColor = row % 2 == 0 ? Color.White : Color.FromArgb(250, 251, 253);
                    chk.Dock = DockStyle.Fill;
                    chk.TextAlign = ContentAlignment.MiddleCenter;
                    chk.CheckAlign = ContentAlignment.MiddleCenter;
                    chk.FlatStyle = FlatStyle.Flat;
                    chk.Cursor = Cursors.Hand;

                    // Center the checkbox
                    chk.Padding = new Padding(0);
                    chk.Margin = new Padding(0);

                    permissionCheckboxes[row, col] = chk;

                    // Panel to center checkbox
                    Panel chkPanel = new Panel();
                    chkPanel.BackColor = row % 2 == 0 ? Color.White : Color.FromArgb(250, 251, 253);
                    chkPanel.Dock = DockStyle.Fill;

                    chk.Location = new Point(
                        (chkPanel.Width - 20) / 2,
                        (chkPanel.Height - 20) / 2
                    );
                    chk.Size = new Size(20, 20);

                    chkPanel.Resize += (s, e) =>
                    {
                        chk.Location = new Point(
                            (chkPanel.Width - 20) / 2,
                            (chkPanel.Height - 20) / 2
                        );
                    };

                    chkPanel.Controls.Add(chk);
                    table.Controls.Add(chkPanel, col + 1, row + 1);
                }
            }

            return table;
        }

        private void SavePermissions()
        {
            string message = "Permissions saved successfully!\n\n";

            for (int row = 0; row < modules.Length; row++)
            {
                message += $"\n{modules[row]}: ";
                string[] actions = { "View", "Add", "Edit", "Delete" };
                for (int col = 0; col < 4; col++)
                {
                    if (permissionCheckboxes[row, col].Checked)
                        message += $"{actions[col]} ✓ ";
                }
            }

            MessageBox.Show(message, "Permissions Saved",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
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