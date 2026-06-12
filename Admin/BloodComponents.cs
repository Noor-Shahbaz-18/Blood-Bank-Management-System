using BloodBankManagementSystem.Classes.Database;
using BloodBankManagementSystem.Classes.Helpers;
using System;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace BloodBankManagementSystem.Forms.Admin
{
    public class BloodComponents : Form
    {
        private Panel sidebarPanel;
        private Panel mainPanel;
        private DataGridView bloodTable;
        private TextBox searchInput;

        public BloodComponents()
        {
            this.Text = "Blood Components - Blood Bank";
            this.WindowState = FormWindowState.Maximized;
            this.BackColor = Color.FromArgb(244, 247, 252);
            this.MinimumSize = new Size(1100, 650);
            this.StartPosition = FormStartPosition.CenterScreen;

            BuildMainContent();
            BuildSidebar();
            LoadComponents();
        }

        // =====================================================
        // LOAD COMPONENTS FROM DATABASE
        // =====================================================
        private void LoadComponents()
        {
            try
            {
                DataTable dt = BloodComponentDAL.GetAllComponents();
                bloodTable.Rows.Clear();

                foreach (DataRow row in dt.Rows)
                {
                    string status = row["Status"].ToString();
                    DateTime expiryDate = Convert.ToDateTime(row["ExpiryDate"]);

                    if (expiryDate < DateTime.Today)
                        status = "Expired";
                    else if (expiryDate <= DateTime.Today.AddDays(7) && status == "Available")
                        status = "Expiring Soon";

                    bloodTable.Rows.Add(
                        row["ComponentID"],
                        row["ComponentName"],
                        row["BloodGroup"],
                        row["Quantity"],
                        Convert.ToDateTime(row["ExpiryDate"]).ToString("dd-MMM-yyyy"),
                        row["StorageLocation"],
                        status,
                        "Edit",
                        "Delete"
                    );
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading components: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // =====================================================
        // FILTER TABLE (SEARCH)
        // =====================================================
        private void FilterTable(string query)
        {
            if (bloodTable == null) return;
            string q = query.ToLower().Trim();
            bool isEmpty = string.IsNullOrEmpty(q) || q == "search components...";

            foreach (DataGridViewRow row in bloodTable.Rows)
            {
                if (isEmpty) { row.Visible = true; continue; }
                bool match = false;
                foreach (DataGridViewCell cell in row.Cells)
                {
                    if (cell.Value != null && cell.Value.ToString().ToLower().Contains(q))
                    {
                        match = true;
                        break;
                    }
                }
                row.Visible = match;
            }
        }

        // =====================================================
        // MAIN CONTENT
        // =====================================================
        private void BuildMainContent()
        {
            mainPanel = new Panel();
            mainPanel.Dock = DockStyle.Fill;
            mainPanel.BackColor = Color.FromArgb(244, 247, 252);
            mainPanel.Padding = new Padding(18, 12, 18, 12);

            // ----- TOPBAR -----
            Panel topbarPanel = new Panel();
            topbarPanel.Dock = DockStyle.Top;
            topbarPanel.Height = 65;
            topbarPanel.BackColor = Color.Transparent;

            Label titleLabel = new Label();
            titleLabel.Text = "Blood Components";
            titleLabel.Font = new Font("Segoe UI", 20, FontStyle.Bold);
            titleLabel.ForeColor = Color.FromArgb(20, 20, 30);
            titleLabel.BackColor = Color.Transparent;
            titleLabel.AutoSize = true;
            titleLabel.Location = new Point(0, 14);

            // Search box
            Panel searchBox = new Panel();
            searchBox.Size = new Size(260, 38);
            searchBox.BackColor = Color.White;
            searchBox.Paint += (s, e) => {
                var p = (Panel)s;
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (GraphicsPath gp = RoundRect(p.ClientRectangle, 8))
                {
                    p.Region = new Region(gp);
                    using (Pen pen = new Pen(Color.FromArgb(220, 220, 228), 1f))
                        e.Graphics.DrawPath(pen, gp);
                }
            };

            Label searchIcon = new Label();
            searchIcon.Text = "🔍";
            searchIcon.Font = new Font("Segoe UI Emoji", 11, FontStyle.Regular);
            searchIcon.BackColor = Color.Transparent;
            searchIcon.AutoSize = true;
            searchIcon.Location = new Point(10, 9);

            searchInput = new TextBox();
            searchInput.BorderStyle = BorderStyle.None;
            searchInput.Font = new Font("Segoe UI", 10);
            searchInput.Size = new Size(200, 22);
            searchInput.Location = new Point(38, 10);
            searchInput.Text = "Search components...";
            searchInput.ForeColor = Color.Gray;
            searchInput.Enter += (s, e) => { if (searchInput.Text == "Search components...") { searchInput.Text = ""; searchInput.ForeColor = Color.Black; } };
            searchInput.Leave += (s, e) => { if (string.IsNullOrWhiteSpace(searchInput.Text)) { searchInput.Text = "Search components..."; searchInput.ForeColor = Color.Gray; } };
            searchInput.TextChanged += (s, e) => FilterTable(searchInput.Text);

            searchBox.Controls.Add(searchIcon);
            searchBox.Controls.Add(searchInput);

            // Add Component Button
            Panel addButton = new Panel();
            addButton.Size = new Size(160, 38);
            addButton.BackColor = Color.FromArgb(37, 99, 235);
            addButton.Cursor = Cursors.Hand;
            addButton.Paint += (s, e) => {
                var p = (Panel)s;
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (GraphicsPath gp = RoundRect(p.ClientRectangle, 8))
                using (SolidBrush br = new SolidBrush(p.BackColor))
                {
                    p.Region = new Region(gp);
                    e.Graphics.FillPath(br, gp);
                }
            };

            Label addLabel = new Label();
            addLabel.Text = "+ Add Component";
            addLabel.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            addLabel.ForeColor = Color.White;
            addLabel.BackColor = Color.Transparent;
            addLabel.AutoSize = false;
            addLabel.Size = new Size(160, 38);
            addLabel.Location = new Point(0, 0);
            addLabel.TextAlign = ContentAlignment.MiddleCenter;
            addLabel.Cursor = Cursors.Hand;

            // Add Component Click Event
            EventHandler addClick = (s, e) =>
            {
                using (AddEditBloodComponent addForm = new AddEditBloodComponent())
                {
                    if (addForm.ShowDialog() == DialogResult.OK)
                    {
                        LoadComponents();
                    }
                }
            };
            addButton.Click += addClick;
            addLabel.Click += addClick;

            addButton.Controls.Add(addLabel);

            topbarPanel.Controls.Add(titleLabel);
            topbarPanel.Controls.Add(searchBox);
            topbarPanel.Controls.Add(addButton);

            EventHandler reposition = (s, e) => {
                int r = topbarPanel.ClientSize.Width - 12;
                addButton.Location = new Point(r - addButton.Width, 13);
                searchBox.Location = new Point(addButton.Left - searchBox.Width - 12, 13);
            };
            topbarPanel.Resize += reposition;
            topbarPanel.HandleCreated += reposition;

            // ----- TABLE PANEL -----
            Panel tablePanel = new Panel();
            tablePanel.Dock = DockStyle.Fill;
            tablePanel.BackColor = Color.White;
            tablePanel.Padding = new Padding(12, 8, 12, 12);
            tablePanel.Paint += (s, e) => {
                var p = (Panel)s;
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (GraphicsPath gp = RoundRect(p.ClientRectangle, 12))
                {
                    p.Region = new Region(gp);
                    using (SolidBrush br = new SolidBrush(Color.White))
                        e.Graphics.FillPath(br, gp);
                    using (Pen pen = new Pen(Color.FromArgb(220, 220, 225), 1f))
                        e.Graphics.DrawPath(pen, gp);
                }
            };

            Label headerLabel = new Label();
            headerLabel.Text = "All Blood Components";
            headerLabel.Font = new Font("Segoe UI", 13, FontStyle.Bold);
            headerLabel.ForeColor = Color.FromArgb(20, 20, 30);
            headerLabel.BackColor = Color.Transparent;
            headerLabel.Dock = DockStyle.Top;
            headerLabel.Height = 36;
            headerLabel.TextAlign = ContentAlignment.MiddleLeft;
            headerLabel.Padding = new Padding(4, 0, 0, 0);

            // ----- DATAGRIDVIEW -----
            bloodTable = new DataGridView();
            bloodTable.Dock = DockStyle.Fill;
            bloodTable.BackgroundColor = Color.White;
            bloodTable.BorderStyle = BorderStyle.None;
            bloodTable.RowHeadersVisible = false;
            bloodTable.AllowUserToAddRows = false;
            bloodTable.AllowUserToDeleteRows = false;
            bloodTable.AllowUserToResizeRows = false;
            bloodTable.AllowUserToResizeColumns = false;
            bloodTable.ReadOnly = true;
            bloodTable.EditMode = DataGridViewEditMode.EditProgrammatically;
            bloodTable.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            bloodTable.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            bloodTable.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            bloodTable.ColumnHeadersHeight = 42;
            bloodTable.RowTemplate.Height = 42;
            bloodTable.GridColor = Color.FromArgb(235, 235, 240);
            bloodTable.EnableHeadersVisualStyles = false;
            bloodTable.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;

            bloodTable.DefaultCellStyle = new DataGridViewCellStyle();
            bloodTable.DefaultCellStyle.Font = new Font("Segoe UI", 10);
            bloodTable.DefaultCellStyle.BackColor = Color.White;
            bloodTable.DefaultCellStyle.ForeColor = Color.FromArgb(30, 30, 40);
            bloodTable.DefaultCellStyle.SelectionBackColor = Color.FromArgb(240, 244, 255);
            bloodTable.DefaultCellStyle.SelectionForeColor = Color.FromArgb(30, 30, 40);
            bloodTable.DefaultCellStyle.Padding = new Padding(6, 0, 6, 0);

            bloodTable.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle();
            bloodTable.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            bloodTable.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(248, 249, 251);
            bloodTable.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(60, 60, 70);
            bloodTable.ColumnHeadersDefaultCellStyle.Padding = new Padding(6, 0, 6, 0);

            // ============================================
            // COLUMNS - Exactly like UserManagement
            // ============================================

            // 1. ComponentID Column (HIDDEN)
            var colId = new DataGridViewTextBoxColumn();
            colId.Name = "ComponentID";
            colId.HeaderText = "ID";
            colId.Visible = false;
            colId.ReadOnly = true;
            bloodTable.Columns.Add(colId);

            // 2. Component Name Column
            var colComponent = new DataGridViewTextBoxColumn();
            colComponent.Name = "Component";
            colComponent.HeaderText = "Component";
            colComponent.FillWeight = 20;
            colComponent.ReadOnly = true;
            bloodTable.Columns.Add(colComponent);

            // 3. Blood Group Column
            var colBloodGroup = new DataGridViewTextBoxColumn();
            colBloodGroup.Name = "BloodGroup";
            colBloodGroup.HeaderText = "Blood Group";
            colBloodGroup.FillWeight = 12;
            colBloodGroup.ReadOnly = true;
            bloodTable.Columns.Add(colBloodGroup);

            // 4. Quantity Column
            var colQuantity = new DataGridViewTextBoxColumn();
            colQuantity.Name = "Quantity";
            colQuantity.HeaderText = "Quantity";
            colQuantity.FillWeight = 10;
            colQuantity.ReadOnly = true;
            bloodTable.Columns.Add(colQuantity);

            // 5. Expiry Date Column
            var colExpiry = new DataGridViewTextBoxColumn();
            colExpiry.Name = "ExpiryDate";
            colExpiry.HeaderText = "Expiry Date";
            colExpiry.FillWeight = 15;
            colExpiry.ReadOnly = true;
            bloodTable.Columns.Add(colExpiry);

            // 6. Storage Location Column
            var colLocation = new DataGridViewTextBoxColumn();
            colLocation.Name = "Location";
            colLocation.HeaderText = "Storage Location";
            colLocation.FillWeight = 15;
            colLocation.ReadOnly = true;
            bloodTable.Columns.Add(colLocation);

            // 7. Status Column
            var colStatus = new DataGridViewTextBoxColumn();
            colStatus.Name = "Status";
            colStatus.HeaderText = "Status";
            colStatus.FillWeight = 12;
            colStatus.ReadOnly = true;
            bloodTable.Columns.Add(colStatus);

            // 8. Edit Button Column (Exactly like UserManagement)
            var btnEdit = new DataGridViewButtonColumn();
            btnEdit.Name = "Edit";
            btnEdit.HeaderText = "Actions";
            btnEdit.Text = "Edit";
            btnEdit.UseColumnTextForButtonValue = true;
            btnEdit.FlatStyle = FlatStyle.Flat;
            btnEdit.FillWeight = 8;
            bloodTable.Columns.Add(btnEdit);

            // 9. Delete Button Column (Exactly like UserManagement)
            var btnDelete = new DataGridViewButtonColumn();
            btnDelete.Name = "Delete";
            btnDelete.HeaderText = "";
            btnDelete.Text = "Delete";
            btnDelete.UseColumnTextForButtonValue = true;
            btnDelete.FlatStyle = FlatStyle.Flat;
            btnDelete.FillWeight = 8;
            bloodTable.Columns.Add(btnDelete);

            // ============================================
            // CELL FORMATTING - Exactly like UserManagement
            // ============================================
            bloodTable.CellFormatting += (s, e) =>
            {
                if (e.RowIndex < 0) return;

                // Status column formatting
                if (e.ColumnIndex == bloodTable.Columns["Status"].Index && e.Value != null)
                {
                    string status = e.Value.ToString();
                    Color foreColor;
                    Color backColor;

                    if (status == "Available")
                    {
                        foreColor = Color.FromArgb(6, 95, 70);
                        backColor = Color.FromArgb(209, 250, 229);
                    }
                    else if (status == "Low Stock")
                    {
                        foreColor = Color.FromArgb(146, 64, 14);
                        backColor = Color.FromArgb(254, 243, 199);
                    }
                    else if (status == "Expiring Soon")
                    {
                        foreColor = Color.FromArgb(194, 120, 0);
                        backColor = Color.FromArgb(254, 252, 232);
                    }
                    else
                    {
                        foreColor = Color.FromArgb(153, 27, 27);
                        backColor = Color.FromArgb(254, 226, 226);
                    }

                    e.CellStyle.ForeColor = foreColor;
                    e.CellStyle.BackColor = backColor;
                    e.CellStyle.SelectionForeColor = foreColor;
                    e.CellStyle.SelectionBackColor = backColor;
                    e.CellStyle.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
                    e.CellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                }

                // Edit button formatting (like UserManagement)
                if (e.ColumnIndex == bloodTable.Columns["Edit"].Index)
                {
                    e.CellStyle.ForeColor = Color.White;
                    e.CellStyle.BackColor = Color.FromArgb(37, 99, 235);
                    e.CellStyle.SelectionBackColor = Color.FromArgb(30, 80, 200);
                    e.CellStyle.SelectionForeColor = Color.White;
                    e.CellStyle.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
                    e.CellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                }

                // Delete button formatting (like UserManagement)
                if (e.ColumnIndex == bloodTable.Columns["Delete"].Index)
                {
                    e.CellStyle.ForeColor = Color.White;
                    e.CellStyle.BackColor = Color.FromArgb(220, 38, 38);
                    e.CellStyle.SelectionBackColor = Color.FromArgb(190, 28, 28);
                    e.CellStyle.SelectionForeColor = Color.White;
                    e.CellStyle.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
                    e.CellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                }
            };

            // ============================================
            // CELL CLICK EVENT - Exactly like UserManagement
            // ============================================
            bloodTable.CellClick += (s, e) =>
            {
                if (e.RowIndex < 0 || e.RowIndex >= bloodTable.Rows.Count) return;

                int componentId = Convert.ToInt32(bloodTable.Rows[e.RowIndex].Cells["ComponentID"].Value);
                string componentName = bloodTable.Rows[e.RowIndex].Cells["Component"].Value?.ToString() ?? "Component";

                // Edit button click
                if (e.ColumnIndex == bloodTable.Columns["Edit"].Index)
                {
                    using (AddEditBloodComponent editForm = new AddEditBloodComponent(componentId))
                    {
                        if (editForm.ShowDialog() == DialogResult.OK)
                        {
                            LoadComponents();
                        }
                    }
                }
                // Delete button click
                else if (e.ColumnIndex == bloodTable.Columns["Delete"].Index)
                {
                    DialogResult result = MessageBox.Show($"Are you sure you want to delete '{componentName}'?",
                        "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                    if (result == DialogResult.Yes)
                    {
                        bool success = BloodComponentDAL.DeleteComponent(componentId);
                        if (success)
                        {
                            MessageBox.Show("Component deleted successfully!", "Success",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                            LoadComponents();
                        }
                        else
                        {
                            MessageBox.Show("Failed to delete component. Please try again.", "Error",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            };

            tablePanel.Controls.Add(bloodTable);
            tablePanel.Controls.Add(headerLabel);

            mainPanel.Controls.Add(tablePanel);
            mainPanel.Controls.Add(topbarPanel);
            this.Controls.Add(mainPanel);
        }

        // =====================================================
        // SIDEBAR
        // =====================================================
        private void BuildSidebar()
        {
            sidebarPanel = new Panel();
            sidebarPanel.Dock = DockStyle.Left;
            sidebarPanel.Width = 230;
            sidebarPanel.BackColor = Color.FromArgb(5, 31, 64);

            Panel logoPanel = new Panel();
            logoPanel.Dock = DockStyle.Top;
            logoPanel.Height = 72;
            logoPanel.BackColor = Color.Transparent;

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

            FlowLayoutPanel menuPanel = new FlowLayoutPanel();
            menuPanel.Dock = DockStyle.Fill;
            menuPanel.FlowDirection = FlowDirection.TopDown;
            menuPanel.WrapContents = false;
            menuPanel.AutoScroll = true;
            menuPanel.Padding = new Padding(8, 8, 8, 8);
            menuPanel.BackColor = Color.Transparent;

            // Dashboard
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

            // Roles & Permissions
            AddSidebarMenuItem(menuPanel, "🔐", "Roles & Permissions", false, () =>
            {
                using (RolesPermissions rolesForm = new RolesPermissions())
                {
                    this.Hide();
                    rolesForm.ShowDialog();
                    this.Show();
                }
            });

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

            // Blood Components (Active)
            AddSidebarMenuItem(menuPanel, "🩸", "Blood Components", true, null);

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

            // Logout
            AddSidebarMenuItem(menuPanel, "🚪", "Logout", false, () =>
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

        private void AddSidebarMenuItem(FlowLayoutPanel panel, string icon, string text, bool active, Action clickAction)
        {
            Panel item = new Panel();
            item.Size = new Size(210, 46);
            item.Margin = new Padding(0, 2, 0, 2);
            item.Cursor = Cursors.Hand;
            item.BackColor = active ? Color.FromArgb(230, 57, 70) : Color.Transparent;

            Label iconLbl = new Label();
            iconLbl.Text = icon;
            iconLbl.Font = new Font("Segoe UI Emoji", 14, FontStyle.Regular);
            iconLbl.ForeColor = Color.White;
            iconLbl.BackColor = Color.Transparent;
            iconLbl.AutoSize = false;
            iconLbl.Size = new Size(35, 30);
            iconLbl.Location = new Point(10, 8);
            iconLbl.TextAlign = ContentAlignment.MiddleCenter;

            Label textLbl = new Label();
            textLbl.Text = text;
            textLbl.Font = new Font("Segoe UI", 10, FontStyle.Regular);
            textLbl.ForeColor = Color.White;
            textLbl.BackColor = Color.Transparent;
            textLbl.AutoSize = false;
            textLbl.Size = new Size(155, 30);
            textLbl.Location = new Point(48, 8);
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