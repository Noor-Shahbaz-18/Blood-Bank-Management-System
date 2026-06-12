using System;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using BloodBankManagementSystem.Classes.Database;
using BloodBankManagementSystem.Classes.Helpers;

namespace BloodBankManagementSystem.Forms.Donor
{
    public partial class NearbyCamps : Form
    {
        private Panel mainScrollPanel;
        private Panel topFixedPanel;
        private FlowLayoutPanel hospitalsContainer;
        private TextBox txtSearch;
        private ComboBox cmbCityFilter;
        private Label lblStatus;
        private Label lblSubtitle;
        private Color brickRed = Color.FromArgb(120, 16, 22);
        private Color brickRedHover = Color.FromArgb(90, 12, 16);
        private Color bgLight = Color.FromArgb(245, 247, 250);
        private string donorCity = "";
        private DataTable allHospitalsData;

        public NearbyCamps()
        {
            this.Text = "Nearby Blood Banks & Hospitals";
            this.WindowState = FormWindowState.Maximized;
            this.BackColor = bgLight;
            this.MinimumSize = new Size(1100, 650);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Font = new Font("Segoe UI", 9F);

            LoadDonorCity();
            BuildUI();
            LoadHospitals();
        }

        private void LoadDonorCity()
        {
            try
            {
                int userID = SessionManager.CurrentUserID;
                var donor = DonorDAL.GetDonorByUserID(userID);
                if (donor != null && !string.IsNullOrEmpty(donor.Address))
                {
                    string[] parts = donor.Address.Split(',');
                    donorCity = parts[parts.Length - 1].Trim();
                    if (string.IsNullOrEmpty(donorCity))
                        donorCity = donor.Address.Trim();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error loading donor city: " + ex.Message);
            }
        }

        private void BuildUI()
        {
            // ── 1. SCROLL PANEL (Fill) — add FIRST ──────────────────────
            mainScrollPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = bgLight,
                Padding = new Padding(20, 8, 20, 20),
                AutoScroll = true   // ✅ Scroll yahan hoga
            };
            this.Controls.Add(mainScrollPanel);

            // ✅ FIX: Dock.Fill nahi, AutoSize use karo
            hospitalsContainer = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = false,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Padding = new Padding(0),
                BackColor = Color.Transparent,
                Location = new Point(0, 0)
            };
            mainScrollPanel.Controls.Add(hospitalsContainer);

            // ✅ Width resize ke saath update ho
            mainScrollPanel.Resize += (s, e) =>
            {
                hospitalsContainer.Width = mainScrollPanel.ClientSize.Width - 20;
            };

            // ── 2. TOP FIXED PANEL (Top) — add SECOND ───────────────────
            topFixedPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 205,
                BackColor = bgLight,
                Padding = new Padding(0)
            };
            this.Controls.Add(topFixedPanel);

            // Header card
            Panel headerCard = new Panel
            {
                Left = 20,
                Top = 10,
                Height = 76,
                BackColor = Color.White,
                Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top
            };
            headerCard.Width = topFixedPanel.Width - 40;
            AddCardBorder(headerCard);
            topFixedPanel.Controls.Add(headerCard);
            topFixedPanel.Resize += (s, e) => headerCard.Width = topFixedPanel.Width - 40;

            Label lblTitle = new Label
            {
                Text = "📍  Nearby Blood Banks & Hospitals",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = brickRed,
                Location = new Point(18, 12),
                AutoSize = true
            };
            headerCard.Controls.Add(lblTitle);

            lblSubtitle = new Label
            {
                Text = string.IsNullOrEmpty(donorCity)
                    ? "Find blood banks and hospitals near your location"
                    : "Showing blood banks near " + donorCity,
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.FromArgb(130, 130, 140),
                Location = new Point(20, 46),
                AutoSize = true
            };
            headerCard.Controls.Add(lblSubtitle);

            // Filter card
            Panel filterCard = new Panel
            {
                Left = 20,
                Top = 98,
                Height = 56,
                BackColor = Color.White,
                Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top
            };
            filterCard.Width = topFixedPanel.Width - 40;
            AddCardBorder(filterCard);
            topFixedPanel.Controls.Add(filterCard);
            topFixedPanel.Resize += (s, e) => filterCard.Width = topFixedPanel.Width - 40;

            Label lblCity = new Label
            {
                Text = "Filter by City:",
                Location = new Point(16, 20),
                AutoSize = true,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(80, 80, 95)
            };
            filterCard.Controls.Add(lblCity);

            cmbCityFilter = new ComboBox
            {
                Location = new Point(120, 15),
                Width = 170,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.White
            };
            cmbCityFilter.Items.Add("All Cities");
            cmbCityFilter.SelectedIndex = 0;
            cmbCityFilter.SelectedIndexChanged += (s, e) => FilterByCity();
            filterCard.Controls.Add(cmbCityFilter);

            Label lblSearch = new Label
            {
                Text = "Search:",
                Location = new Point(315, 20),
                AutoSize = true,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(80, 80, 95)
            };
            filterCard.Controls.Add(lblSearch);

            txtSearch = new TextBox
            {
                Location = new Point(375, 15),
                Width = 210,
                Font = new Font("Segoe UI", 10),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White
            };
            txtSearch.TextChanged += (s, e) => FilterBySearch();
            filterCard.Controls.Add(txtSearch);

            Button btnNearMe = new Button
            {
                Text = "📍  Near Me",
                Location = new Point(608, 11),
                Size = new Size(118, 34),
                BackColor = brickRed,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnNearMe.FlatAppearance.BorderSize = 0;
            MakeRounded(btnNearMe, 8);
            btnNearMe.MouseEnter += (s, e) => btnNearMe.BackColor = brickRedHover;
            btnNearMe.MouseLeave += (s, e) => btnNearMe.BackColor = brickRed;
            btnNearMe.Click += (s, e) => FilterByNearby();
            filterCard.Controls.Add(btnNearMe);

            // Status label
            lblStatus = new Label
            {
                Left = 24,
                Top = 168,
                Height = 24,
                Text = "Loading hospitals...",
                ForeColor = Color.FromArgb(130, 130, 140),
                Font = new Font("Segoe UI", 10),
                AutoSize = true
            };
            topFixedPanel.Controls.Add(lblStatus);
        }

        private void LoadHospitals()
        {
            try
            {
                DataTable dt = HospitalDAL.GetAllHospitals();
                allHospitalsData = dt;

                if (dt == null || dt.Rows.Count == 0)
                {
                    lblStatus.Text = "⚠️  No hospitals found. Please contact admin.";
                    lblStatus.ForeColor = Color.Orange;
                    ShowEmptyMessage("No hospitals available.\n\nAdmin will add hospitals soon.");
                    return;
                }

                cmbCityFilter.Items.Clear();
                cmbCityFilter.Items.Add("All Cities");
                foreach (DataRow row in dt.Rows)
                {
                    string city = row["City"].ToString();
                    if (!string.IsNullOrEmpty(city) && !cmbCityFilter.Items.Contains(city))
                        cmbCityFilter.Items.Add(city);
                }

                if (!string.IsNullOrEmpty(donorCity) && cmbCityFilter.Items.Contains(donorCity))
                    cmbCityFilter.SelectedItem = donorCity;
                else
                    cmbCityFilter.SelectedIndex = 0;

                DisplayHospitals(dt);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading hospitals: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                lblStatus.Text = "❌  Error loading hospitals";
                lblStatus.ForeColor = Color.Red;
            }
        }

        private void DisplayHospitals(DataTable dt)
        {
            hospitalsContainer.Controls.Clear();

            if (dt == null || dt.Rows.Count == 0)
            {
                ShowEmptyMessage("No blood banks/hospitals found in selected city.\n\nTry changing the city filter!");
                lblStatus.Text = "📭  No hospitals found";
                lblStatus.ForeColor = Color.FromArgb(130, 130, 140);
                return;
            }

            lblStatus.Text = "📋  " + dt.Rows.Count + " blood bank(s)/hospital(s) found";
            lblStatus.ForeColor = Color.FromArgb(30, 140, 80);

            foreach (DataRow row in dt.Rows)
                hospitalsContainer.Controls.Add(CreateHospitalCard(row));

            // ✅ Force width update after loading
            hospitalsContainer.Width = mainScrollPanel.ClientSize.Width - 20;
        }

        private void ShowEmptyMessage(string text)
        {
            hospitalsContainer.Controls.Clear();
            hospitalsContainer.Controls.Add(new Label
            {
                Text = text,
                Font = new Font("Segoe UI", 12),
                ForeColor = Color.FromArgb(160, 160, 170),
                TextAlign = ContentAlignment.MiddleCenter,
                Size = new Size(600, 100),
                Margin = new Padding(0, 40, 0, 0)
            });
        }

        private Panel CreateHospitalCard(DataRow row)
        {
            int cardWidth = Math.Max(hospitalsContainer.Width - 10, 600);

            Panel card = new Panel
            {
                Width = cardWidth,
                Height = 115,
                BackColor = Color.White,
                Margin = new Padding(0, 0, 0, 10)
            };
            AddCardBorder(card);

            Panel accent = new Panel
            {
                Width = 4,
                Height = 115,
                BackColor = brickRed,
                Dock = DockStyle.Left
            };
            card.Controls.Add(accent);

            Label lblName = new Label
            {
                Text = row["Name"].ToString(),
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = brickRed,
                Location = new Point(18, 13),
                AutoSize = true
            };
            card.Controls.Add(lblName);

            string city = row["City"].ToString();
            bool isYourCity = !string.IsNullOrEmpty(donorCity) &&
                              city.ToLower() == donorCity.ToLower();

            Label lblCity = new Label
            {
                Text = "📍  " + city + (isYourCity ? "   🏠 Your City" : ""),
                Font = new Font("Segoe UI", 10),
                ForeColor = isYourCity ? Color.FromArgb(30, 140, 80) : Color.FromArgb(100, 100, 110),
                Location = new Point(20, 44),
                AutoSize = true
            };
            card.Controls.Add(lblCity);

            Label lblPhone = new Label
            {
                Text = "📞  " + row["Phone"].ToString(),
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(140, 140, 150),
                Location = new Point(20, 68),
                AutoSize = true
            };
            card.Controls.Add(lblPhone);

            Button btnBook = new Button
            {
                Text = "📅  Book Appointment",
                Size = new Size(158, 36),
                BackColor = brickRed,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Tag = row["HospitalID"]
            };
            btnBook.FlatAppearance.BorderSize = 0;
            MakeRounded(btnBook, 8);
            btnBook.MouseEnter += (s, e) => btnBook.BackColor = brickRedHover;
            btnBook.MouseLeave += (s, e) => btnBook.BackColor = brickRed;
            btnBook.Click += (s, e) => BookAppointmentForHospital(row);
            card.Controls.Add(btnBook);

            void PositionBtn()
            {
                btnBook.Location = new Point(card.Width - 175, 38);
            }
            PositionBtn();

            card.Resize += (s, e) => { card.Width = Math.Max(hospitalsContainer.Width - 10, 600); PositionBtn(); };
            hospitalsContainer.Resize += (s, e) => { card.Width = Math.Max(hospitalsContainer.Width - 10, 600); PositionBtn(); };

            return card;
        }

        private void FilterByCity()
        {
            if (allHospitalsData == null) return;
            string sel = cmbCityFilter.SelectedItem?.ToString() ?? "All Cities";
            DataTable filtered = allHospitalsData.Clone();
            foreach (DataRow row in allHospitalsData.Rows)
                if (sel == "All Cities" || row["City"].ToString() == sel)
                    filtered.ImportRow(row);
            DisplayHospitals(filtered);
        }

        private void FilterBySearch()
        {
            if (allHospitalsData == null) return;
            if (string.IsNullOrWhiteSpace(txtSearch.Text)) { FilterByCity(); return; }
            string q = txtSearch.Text.ToLower();
            string sel = cmbCityFilter.SelectedItem?.ToString() ?? "All Cities";
            DataTable filtered = allHospitalsData.Clone();
            foreach (DataRow row in allHospitalsData.Rows)
            {
                if (sel != "All Cities" && row["City"].ToString() != sel) continue;
                if (row["Name"].ToString().ToLower().Contains(q) ||
                    row["City"].ToString().ToLower().Contains(q))
                    filtered.ImportRow(row);
            }
            DisplayHospitals(filtered);
        }

        private void FilterByNearby()
        {
            if (string.IsNullOrEmpty(donorCity))
            {
                MessageBox.Show("Please update your profile with your address first.\n\nGo to Profile Settings to add your address.",
                    "Location Not Set", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (cmbCityFilter.Items.Contains(donorCity))
                cmbCityFilter.SelectedItem = donorCity;
            else
            {
                MessageBox.Show("No hospitals found near '" + donorCity + "'.\nShowing all hospitals instead.",
                    "No Nearby Hospitals", MessageBoxButtons.OK, MessageBoxIcon.Information);
                cmbCityFilter.SelectedIndex = 0;
            }
        }

        private void BookAppointmentForHospital(DataRow row)
        {
            string msg = "🏥  HOSPITAL DETAILS\n\n" +
                         "Name:   " + row["Name"] + "\n" +
                         "City:   " + row["City"] + "\n" +
                         "Phone:  " + row["Phone"] + "\n\n" +
                         "Would you like to book a donation appointment at this hospital?";

            if (MessageBox.Show(msg, "Hospital Details",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                BookAppointment appt = new BookAppointment();
                appt.ShowDialog();
            }
        }

        private void AddCardBorder(Control c)
        {
            c.Paint += (sender, e) =>
            {
                using (Pen pen = new Pen(Color.FromArgb(218, 218, 224), 1))
                {
                    e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    using (GraphicsPath path = RoundedRect(new Rectangle(0, 0, c.Width - 1, c.Height - 1), 12))
                    {
                        e.Graphics.DrawPath(pen, path);
                        ((Control)sender).Region = new Region(path);
                    }
                }
            };
        }

        private void MakeRounded(Control c, int radius)
        {
            c.Paint += (sender, e) =>
            {
                ((Control)sender).Region = new Region(RoundedRect(
                    new Rectangle(0, 0, c.Width - 1, c.Height - 1), radius));
            };
        }

        private GraphicsPath RoundedRect(Rectangle rect, int radius)
        {
            int d = radius * 2;
            GraphicsPath path = new GraphicsPath();
            path.AddArc(rect.X, rect.Y, d, d, 180, 90);
            path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
            path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
            path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }
    }
}