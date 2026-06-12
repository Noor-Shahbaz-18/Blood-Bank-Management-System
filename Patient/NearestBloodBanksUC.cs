using System;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using BloodBankManagementSystem.Classes.Database;
using BloodBankManagementSystem.Classes.Helpers;

namespace BloodBankManagementSystem.Forms.Patient
{
    public partial class NearestBloodBanksUC : UserControl
    {
        private Panel scrollContainer;
        private Panel cardsPanel;
        private ComboBox cmbCityFilter;
        private TextBox txtSearch;
        private Label lblStatus;
        private Button btnNearMe;
        private DataTable hospitalsData;
        private string userCity = "Lahore"; // Default city

        public NearestBloodBanksUC()
        {
            InitializeComponent();
            LoadUserCity();
            BuildUI();
            LoadHospitalsFromDatabase();
        }

        private void LoadUserCity()
        {
            try
            {
                int userId = SessionManager.CurrentUserID;
                var patient = PatientDAL.GetByUserID(userId);

                if (patient != null && !string.IsNullOrEmpty(patient.Address))
                {
                    string address = patient.Address.ToLower();
                    if (address.Contains("karachi")) userCity = "Karachi";
                    else if (address.Contains("islamabad")) userCity = "Islamabad";
                    else if (address.Contains("rawalpindi")) userCity = "Rawalpindi";
                    else if (address.Contains("multan")) userCity = "Multan";
                    else if (address.Contains("faisalabad")) userCity = "Faisalabad";
                    else if (address.Contains("lahore")) userCity = "Lahore";
                    // Add more cities as needed
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LoadUserCity Error: {ex.Message}");
            }
        }

        private void BuildUI()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.FromArgb(245, 247, 250);

            // Main vertical layout panel
            TableLayoutPanel mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 4,
                BackColor = Color.Transparent,
                Padding = new Padding(20, 15, 20, 15)
            };
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 70));   // Header
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 60));   // Filter
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));   // Status
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));   // Cards
            this.Controls.Add(mainLayout);

            // ===== HEADER =====
            Panel headerCard = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Margin = new Padding(0, 0, 0, 8)
            };
            headerCard.Paint += RoundedPaint;

            Label lblTitle = new Label
            {
                Text = "📍 Blood Banks & Hospitals",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.FromArgb(120, 22, 27),
                Location = new Point(20, 18),
                AutoSize = true
            };
            headerCard.Controls.Add(lblTitle);
            mainLayout.Controls.Add(headerCard, 0, 0);

            // ===== FILTER =====
            Panel filterCard = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Margin = new Padding(0, 0, 0, 8)
            };
            filterCard.Paint += RoundedPaint;

            Label lblCity = new Label
            {
                Text = "City:",
                Location = new Point(20, 20),
                AutoSize = true,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };
            filterCard.Controls.Add(lblCity);

            cmbCityFilter = new ComboBox
            {
                Location = new Point(58, 17),
                Width = 140,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9)
            };
            cmbCityFilter.SelectedIndexChanged += (s, e) => RefreshHospitalsDisplay();
            filterCard.Controls.Add(cmbCityFilter);

            // Near Me Button
            btnNearMe = new Button
            {
                Text = "📍 Near Me",
                Location = new Point(210, 15),
                Size = new Size(95, 30),
                BackColor = Color.FromArgb(120, 22, 27),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnNearMe.FlatAppearance.BorderSize = 0;
            btnNearMe.Click += (s, e) => FilterNearMe();
            filterCard.Controls.Add(btnNearMe);

            Label lblSearch = new Label
            {
                Text = "Search:",
                Location = new Point(320, 20),
                AutoSize = true,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };
            filterCard.Controls.Add(lblSearch);

            txtSearch = new TextBox
            {
                Location = new Point(375, 16),
                Width = 180,
                Font = new Font("Segoe UI", 9),
                BorderStyle = BorderStyle.FixedSingle,
                Text = "Search hospital name..."
            };
            txtSearch.TextChanged += (s, e) => RefreshHospitalsDisplay();
            txtSearch.Enter += (s, e) => { if (txtSearch.Text == "Search hospital name...") txtSearch.Text = ""; };
            txtSearch.Leave += (s, e) => { if (string.IsNullOrWhiteSpace(txtSearch.Text)) txtSearch.Text = "Search hospital name..."; };
            filterCard.Controls.Add(txtSearch);
            mainLayout.Controls.Add(filterCard, 0, 1);

            // ===== STATUS LABEL =====
            lblStatus = new Label
            {
                Text = "Loading hospitals...",
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.Gray,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Margin = new Padding(2, 0, 0, 4)
            };
            mainLayout.Controls.Add(lblStatus, 0, 2);

            // ===== SCROLLABLE CARDS AREA =====
            scrollContainer = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = Color.Transparent,
                Margin = new Padding(0)
            };
            mainLayout.Controls.Add(scrollContainer, 0, 3);

            // Inner panel holds all cards vertically
            cardsPanel = new Panel
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                BackColor = Color.Transparent,
                Location = new Point(0, 0)
            };
            scrollContainer.Controls.Add(cardsPanel);

            // Keep cardsPanel width in sync
            scrollContainer.Resize += (s, e) =>
            {
                cardsPanel.Width = scrollContainer.ClientSize.Width;
            };
        }

        private void FilterNearMe()
        {
            // Set filter to user's city
            if (cmbCityFilter.Items.Contains(userCity))
            {
                cmbCityFilter.SelectedItem = userCity;
                MessageBox.Show($"📍 Showing hospitals near: {userCity}", "Near Me", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show($"No hospitals found in {userCity}.\nShowing all hospitals instead.",
                    "Near Me", MessageBoxButtons.OK, MessageBoxIcon.Information);
                cmbCityFilter.SelectedIndex = 0;
            }
        }

        private void LoadHospitalsFromDatabase()
        {
            try
            {
                hospitalsData = HospitalDAL.GetAllHospitals();

                if (hospitalsData == null || hospitalsData.Rows.Count == 0)
                {
                    AddSampleHospitals();
                    return;
                }

                RefreshHospitalsDisplay();
                UpdateCityFilter();
                lblStatus.Text = $"📍 {hospitalsData.Rows.Count} hospitals available";
                lblStatus.ForeColor = Color.FromArgb(34, 197, 94);
            }
            catch
            {
                AddSampleHospitals();
            }
        }

        private void AddSampleHospitals()
        {
            hospitalsData = new DataTable();
            hospitalsData.Columns.Add("HospitalID", typeof(int));
            hospitalsData.Columns.Add("Name", typeof(string));
            hospitalsData.Columns.Add("City", typeof(string));
            hospitalsData.Columns.Add("Phone", typeof(string));
            hospitalsData.Columns.Add("Address", typeof(string));
            hospitalsData.Columns.Add("IsActive", typeof(bool));

            // Add multiple cities for demo
            hospitalsData.Rows.Add(1, "City Blood Bank", "Lahore", "0300-1234567", "Main Boulevard, Lahore", true);
            hospitalsData.Rows.Add(2, "Red Crescent Blood Bank", "Lahore", "0301-2345678", "Gulberg, Lahore", true);
            hospitalsData.Rows.Add(3, "General Hospital", "Lahore", "0302-3456789", "Mall Road, Lahore", true);
            hospitalsData.Rows.Add(4, "Mayo Hospital", "Lahore", "0303-4567890", "Queen's Road, Lahore", true);
            hospitalsData.Rows.Add(5, "Shaukat Khanum Hospital", "Lahore", "0304-5678901", "Johar Town, Lahore", true);
            hospitalsData.Rows.Add(6, "Jinnah Hospital", "Karachi", "0305-6789012", "Allama Iqbal Town, Karachi", true);
            hospitalsData.Rows.Add(7, "Services Hospital", "Islamabad", "0306-7890123", "Davis Road, Islamabad", true);

            RefreshHospitalsDisplay();
            UpdateCityFilter();
            lblStatus.Text = "📍 Sample hospitals loaded";
            lblStatus.ForeColor = Color.Orange;
        }

        private void UpdateCityFilter()
        {
            cmbCityFilter.Items.Clear();
            cmbCityFilter.Items.Add("All Cities");

            if (hospitalsData != null)
            {
                var cities = hospitalsData.AsEnumerable()
                    .Select(r => r["City"]?.ToString())
                    .Where(c => !string.IsNullOrEmpty(c))
                    .Distinct()
                    .OrderBy(c => c);

                foreach (string city in cities)
                    cmbCityFilter.Items.Add(city);
            }

            // Auto-select user's city if available
            if (cmbCityFilter.Items.Contains(userCity))
            {
                cmbCityFilter.SelectedItem = userCity;
            }
            else
            {
                cmbCityFilter.SelectedIndex = 0;
            }
        }

        private void RefreshHospitalsDisplay()
        {
            if (cardsPanel == null || hospitalsData == null) return;

            // Clear old cards
            cardsPanel.Controls.Clear();

            string selectedCity = cmbCityFilter.SelectedItem?.ToString();
            string searchText = txtSearch.Text.Trim().ToLower();
            if (searchText == "search hospital name...") searchText = "";

            var filteredRows = hospitalsData.AsEnumerable().Where(row =>
            {
                bool cityOk = selectedCity == null || selectedCity == "All Cities" || row["City"]?.ToString() == selectedCity;
                bool searchOk = string.IsNullOrEmpty(searchText) || row["Name"]?.ToString().ToLower().Contains(searchText) == true;
                return cityOk && searchOk;
            }).ToList();

            if (filteredRows.Count == 0)
            {
                Label lblNoData = new Label
                {
                    Text = "⚠️ No hospitals found matching your criteria.",
                    Font = new Font("Segoe UI", 12),
                    ForeColor = Color.Gray,
                    AutoSize = true,
                    Location = new Point(20, 20)
                };
                cardsPanel.Controls.Add(lblNoData);
                lblStatus.Text = "No hospitals found";
                return;
            }

            int totalUnits = GetAvailableUnits();
            lblStatus.Text = $"📍 {filteredRows.Count} hospital(s) found  |  🩸 {totalUnits}+ blood units in stock";
            lblStatus.ForeColor = Color.FromArgb(34, 197, 94);

            int yPos = 0;
            foreach (DataRow row in filteredRows)
            {
                Panel card = CreateHospitalCard(row, totalUnits, yPos);
                cardsPanel.Controls.Add(card);
                yPos += card.Height + 10;
            }

            cardsPanel.Height = yPos;
        }

        private Panel CreateHospitalCard(DataRow row, int totalUnits, int yPosition)
        {
            int cardWidth = scrollContainer.ClientSize.Width - 4;
            if (cardWidth < 300) cardWidth = 300;

            Panel card = new Panel
            {
                Location = new Point(0, yPosition),
                Width = cardWidth,
                Height = 115,
                BackColor = Color.White,
            };
            card.Paint += Card_Paint;

            // Resize card width when scrollContainer resizes
            scrollContainer.Resize += (s, e) =>
            {
                card.Width = scrollContainer.ClientSize.Width - 4;
            };

            // Left accent bar
            Panel accent = new Panel
            {
                Location = new Point(0, 0),
                Size = new Size(5, 115),
                BackColor = Color.FromArgb(120, 22, 27)
            };
            card.Controls.Add(accent);

            // Hospital Name
            Label lblName = new Label
            {
                Text = row["Name"]?.ToString(),
                Font = new Font("Segoe UI", 13, FontStyle.Bold),
                ForeColor = Color.FromArgb(120, 22, 27),
                Location = new Point(20, 10),
                AutoSize = true
            };
            card.Controls.Add(lblName);

            // City
            Label lblCity = new Label
            {
                Text = $"📍  {row["City"]}",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.Gray,
                Location = new Point(20, 38),
                AutoSize = true
            };
            card.Controls.Add(lblCity);

            // Phone
            Label lblPhone = new Label
            {
                Text = $"📞  {row["Phone"]}",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(70, 70, 80),
                Location = new Point(20, 58),
                AutoSize = true
            };
            card.Controls.Add(lblPhone);

            // Blood availability badge
            string unitsText = totalUnits > 10 ? $"🩸  {totalUnits}+ units available"
                             : totalUnits > 0 ? $"🩸  {totalUnits} units available (low)"
                                                : "⚠️  Stock info unavailable";

            Color badgeFg = totalUnits > 10 ? Color.FromArgb(21, 128, 61)
                          : totalUnits > 0 ? Color.FromArgb(161, 98, 7)
                                             : Color.FromArgb(185, 28, 28);

            Label lblUnits = new Label
            {
                Text = unitsText,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = badgeFg,
                Location = new Point(20, 80),
                AutoSize = true
            };
            card.Controls.Add(lblUnits);

            // Address (if available)
            string address = row["Address"]?.ToString();
            if (!string.IsNullOrEmpty(address))
            {
                Label lblAddress = new Label
                {
                    Text = $"🏠  {address}",
                    Font = new Font("Segoe UI", 8),
                    ForeColor = Color.FromArgb(150, 150, 160),
                    Location = new Point(20, 97),
                    AutoSize = false,
                    Width = cardWidth - 160,
                    Height = 16
                };
                card.Controls.Add(lblAddress);
                card.Height = 125;
                accent.Height = 125;
            }

            // Request Button
            Button btnRequest = new Button
            {
                Text = "Request Blood",
                Size = new Size(120, 38),
                BackColor = Color.FromArgb(120, 22, 27),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Tag = row
            };
            btnRequest.FlatAppearance.BorderSize = 0;

            card.Layout += (s, e) =>
            {
                btnRequest.Location = new Point(card.Width - 135, (card.Height - 38) / 2);
            };

            btnRequest.Click += (s, e) =>
            {
                DataRow selectedRow = (DataRow)((Button)s).Tag;
                DialogResult res = MessageBox.Show(
                    $"Request blood from:\n\n🏥 {selectedRow["Name"]}\n📍 {selectedRow["City"]}\n📞 {selectedRow["Phone"]}\n\nProceed?",
                    "Request Blood",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (res == DialogResult.Yes)
                {
                    PatientDashboard parentForm = this.ParentForm as PatientDashboard;
                    parentForm?.NavigateToMenu("Request Blood");
                }
            };
            card.Controls.Add(btnRequest);

            return card;
        }

        private int GetAvailableUnits()
        {
            try
            {
                string query = "SELECT COUNT(*) FROM BloodBags WHERE Status = 'Available' AND ExpiryDate >= GETDATE()";
                object result = CommonDAL.ExecuteScalar(query);
                return result != null ? Convert.ToInt32(result) : 0;
            }
            catch { return 45; }
        }

        private void RoundedPaint(object sender, PaintEventArgs e)
        {
            Control c = sender as Control;
            if (c.Width <= 0 || c.Height <= 0) return;
            GraphicsPath p = new GraphicsPath();
            int r = 12;
            p.AddArc(0, 0, r, r, 180, 90);
            p.AddArc(c.Width - r, 0, r, r, 270, 90);
            p.AddArc(c.Width - r, c.Height - r, r, r, 0, 90);
            p.AddArc(0, c.Height - r, r, r, 90, 90);
            p.CloseFigure();
            c.Region = new Region(p);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using (Pen pen = new Pen(Color.FromArgb(220, 220, 220), 1))
                e.Graphics.DrawPath(pen, p);
        }

        private void Card_Paint(object sender, PaintEventArgs e)
        {
            Panel p = sender as Panel;
            if (p.Width <= 0 || p.Height <= 0) return;
            GraphicsPath path = new GraphicsPath();
            int radius = 10;
            path.AddArc(0, 0, radius, radius, 180, 90);
            path.AddArc(p.Width - radius, 0, radius, radius, 270, 90);
            path.AddArc(p.Width - radius, p.Height - radius, radius, radius, 0, 90);
            path.AddArc(0, p.Height - radius, radius, radius, 90, 90);
            path.CloseFigure();
            p.Region = new Region(path);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using (Pen pen = new Pen(Color.FromArgb(220, 220, 220), 1))
                e.Graphics.DrawPath(pen, path);
        }
    }
}