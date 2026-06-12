using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using BloodBankManagementSystem.Classes.Database;

namespace BloodBankManagementSystem.Forms.Patient
{
    public partial class CheckBloodAvailabilityUC : UserControl
    {
        private FlowLayoutPanel mainFlow;
        private FlowLayoutPanel cardsFlow;
        private ComboBox cmbBloodGroup;

        // Data from database
        private DataTable bloodStockData;
        private List<BloodBankStock> bloodBanks = new List<BloodBankStock>();

        private const int CARD_HEIGHT = 180;

        public CheckBloodAvailabilityUC()
        {
            InitializeComponent();
            this.HandleCreated += (s, e) =>
            {
                BuildUI();
                LoadBloodStockFromDatabase();
                LoadBloodBanks();
                RefreshCards();
                UpdateSizes();
            };
        }

        // =====================================================
        // LOAD BLOOD STOCK FROM DATABASE
        // =====================================================
        private void LoadBloodStockFromDatabase()
        {
            try
            {
                // Get blood stock from BloodBags table
                string query = @"
                    SELECT 
                        BloodGroup,
                        COUNT(*) as AvailableUnits,
                        SUM(CASE WHEN ExpiryDate < GETDATE() THEN 1 ELSE 0 END) as ExpiredUnits,
                        MIN(ExpiryDate) as EarliestExpiry
                    FROM BloodBags 
                    WHERE Status = 'Available' AND ExpiryDate >= GETDATE()
                    GROUP BY BloodGroup
                    ORDER BY BloodGroup";

                bloodStockData = CommonDAL.ExecuteReader(query);

                if (bloodStockData == null || bloodStockData.Rows.Count == 0)
                {
                    // If no data, create sample for testing
                    CreateSampleData();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LoadBloodStock Error: {ex.Message}");
                CreateSampleData();
            }
        }

        private void CreateSampleData()
        {
            bloodStockData = new DataTable();
            bloodStockData.Columns.Add("BloodGroup");
            bloodStockData.Columns.Add("AvailableUnits");
            bloodStockData.Columns.Add("ExpiredUnits");
            bloodStockData.Columns.Add("EarliestExpiry");

            bloodStockData.Rows.Add("A+", 45, 2, DateTime.Now.AddDays(15));
            bloodStockData.Rows.Add("A-", 12, 0, DateTime.Now.AddDays(20));
            bloodStockData.Rows.Add("B+", 38, 1, DateTime.Now.AddDays(10));
            bloodStockData.Rows.Add("B-", 8, 0, DateTime.Now.AddDays(25));
            bloodStockData.Rows.Add("AB+", 15, 0, DateTime.Now.AddDays(18));
            bloodStockData.Rows.Add("AB-", 5, 0, DateTime.Now.AddDays(30));
            bloodStockData.Rows.Add("O+", 67, 3, DateTime.Now.AddDays(12));
            bloodStockData.Rows.Add("O-", 23, 1, DateTime.Now.AddDays(22));
        }

        // =====================================================
        // LOAD BLOOD BANKS FROM DATABASE
        // =====================================================
        private void LoadBloodBanks()
        {
            try
            {
                DataTable dt = HospitalDAL.GetAllHospitals();

                bloodBanks.Clear();

                if (dt != null && dt.Rows.Count > 0)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        string name = row["Name"]?.ToString() ?? "Unknown Hospital";
                        string city = row["City"]?.ToString() ?? "Unknown City";
                        string phone = row["Phone"]?.ToString() ?? "N/A";

                        // Get available blood groups for this hospital
                        List<string> availableGroups = GetAvailableBloodGroups();

                        bloodBanks.Add(new BloodBankStock
                        {
                            Name = name,
                            City = city,
                            Phone = phone,
                            AvailableBloodGroups = availableGroups,
                            Distance = "Varies by location"
                        });
                    }
                }

                if (bloodBanks.Count == 0)
                {
                    // Add sample blood banks if no data
                    bloodBanks.Add(new BloodBankStock
                    {
                        Name = "City Blood Bank",
                        City = "Lahore",
                        Phone = "0300-1234567",
                        AvailableBloodGroups = GetAvailableBloodGroups(),
                        Distance = "Main Branch"
                    });
                    bloodBanks.Add(new BloodBankStock
                    {
                        Name = "Red Crescent Blood Bank",
                        City = "Lahore",
                        Phone = "0301-2345678",
                        AvailableBloodGroups = GetAvailableBloodGroups(),
                        Distance = "Gulberg Branch"
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LoadBloodBanks Error: {ex.Message}");
                // Add fallback data
                bloodBanks.Add(new BloodBankStock
                {
                    Name = "City Blood Bank",
                    City = "Lahore",
                    Phone = "0300-1234567",
                    AvailableBloodGroups = GetAvailableBloodGroups(),
                    Distance = "Main Branch"
                });
            }
        }

        private List<string> GetAvailableBloodGroups()
        {
            List<string> groups = new List<string>();
            if (bloodStockData != null)
            {
                foreach (DataRow row in bloodStockData.Rows)
                {
                    int units = Convert.ToInt32(row["AvailableUnits"]);
                    if (units > 0)
                    {
                        groups.Add(row["BloodGroup"].ToString());
                    }
                }
            }
            return groups;
        }

        // =====================================================
        // GET UNITS FOR A BLOOD GROUP
        // =====================================================
        private int GetAvailableUnits(string bloodGroup)
        {
            if (bloodStockData != null)
            {
                foreach (DataRow row in bloodStockData.Rows)
                {
                    if (row["BloodGroup"].ToString() == bloodGroup)
                    {
                        return Convert.ToInt32(row["AvailableUnits"]);
                    }
                }
            }
            return 0;
        }

        // =====================================================
        // GET EXPIRY STATUS
        // =====================================================
        private string GetExpiryStatus(string bloodGroup)
        {
            if (bloodStockData != null)
            {
                foreach (DataRow row in bloodStockData.Rows)
                {
                    if (row["BloodGroup"].ToString() == bloodGroup && row["EarliestExpiry"] != DBNull.Value)
                    {
                        DateTime expiry = Convert.ToDateTime(row["EarliestExpiry"]);
                        int daysLeft = (expiry - DateTime.Today).Days;

                        if (daysLeft <= 3)
                            return "⚠️ Expiring Soon";
                        else if (daysLeft <= 7)
                            return "🟡 Limited Time";
                        else
                            return "✅ Fresh Stock";
                    }
                }
            }
            return "✅ Available";
        }

        // =====================================================
        // UI BUILD
        // =====================================================
        private void BuildUI()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.FromArgb(245, 247, 250);

            mainFlow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true,
                Padding = new Padding(15, 10, 15, 10)
            };
            this.Controls.Add(mainFlow);

            // ================= FILTER CARD =================
            Panel filterCard = new Panel
            {
                Height = 90,
                BackColor = Color.White,
                Width = this.Width - 30
            };
            filterCard.Paint += RoundedPaint;
            mainFlow.Controls.Add(filterCard);

            // Filter label
            filterCard.Controls.Add(new Label
            {
                Text = "🔍 Filter by Blood Group",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.FromArgb(120, 22, 27),
                Location = new Point(20, 12),
                AutoSize = true
            });

            // Blood Group Combo
            filterCard.Controls.Add(new Label
            {
                Text = "Select Blood Group:",
                Location = new Point(20, 45),
                AutoSize = true,
                Font = new Font("Segoe UI", 9)
            });

            cmbBloodGroup = new ComboBox
            {
                Location = new Point(150, 42),
                Size = new Size(180, 32),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10),
                BackColor = Color.White
            };

            // Add blood groups from database
            cmbBloodGroup.Items.Add("All Blood Groups");
            if (bloodStockData != null)
            {
                foreach (DataRow row in bloodStockData.Rows)
                {
                    string bg = row["BloodGroup"].ToString();
                    if (!cmbBloodGroup.Items.Contains(bg))
                        cmbBloodGroup.Items.Add(bg);
                }
            }
            else
            {
                cmbBloodGroup.Items.AddRange(new string[] { "A+", "A-", "B+", "B-", "AB+", "AB-", "O+", "O-" });
            }
            cmbBloodGroup.SelectedIndex = 0;
            cmbBloodGroup.SelectedIndexChanged += (s, e) => RefreshCards();
            filterCard.Controls.Add(cmbBloodGroup);

            // Stock summary label
            Label lblStockSummary = new Label
            {
                Text = GetStockSummary(),
                Location = new Point(360, 45),
                AutoSize = true,
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.Gray
            };
            filterCard.Controls.Add(lblStockSummary);

            // ================= TITLE =================
            mainFlow.Controls.Add(new Label
            {
                Text = "🏥 Available Blood Banks & Hospitals",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.FromArgb(31, 41, 55),
                AutoSize = true,
                Margin = new Padding(5, 15, 5, 10)
            });

            // ================= STOCK SUMMARY CARDS =================
            Panel stockSummaryPanel = new Panel
            {
                Height = 120,
                Width = mainFlow.Width - 30,
                BackColor = Color.Transparent
            };
            mainFlow.Controls.Add(stockSummaryPanel);
            BuildStockSummaryCards(stockSummaryPanel);

            // ================= BLOOD BANKS CARDS =================
            cardsFlow = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                AutoSize = true,
                BackColor = Color.Transparent
            };
            mainFlow.Controls.Add(cardsFlow);

            this.Resize += (s, e) =>
            {
                if (filterCard != null)
                    filterCard.Width = mainFlow.ClientSize.Width - 30;
                if (stockSummaryPanel != null)
                    stockSummaryPanel.Width = mainFlow.ClientSize.Width - 30;
                if (cardsFlow != null)
                    cardsFlow.Width = mainFlow.ClientSize.Width - 30;
                UpdateSizes();
            };
        }

        private string GetStockSummary()
        {
            int totalUnits = 0;
            if (bloodStockData != null)
            {
                foreach (DataRow row in bloodStockData.Rows)
                {
                    totalUnits += Convert.ToInt32(row["AvailableUnits"]);
                }
            }
            return $"📊 Total Available Units: {totalUnits}";
        }

        private void BuildStockSummaryCards(Panel parent)
        {
            parent.Controls.Clear();

            int cardWidth = (parent.Width - 60) / 4;
            if (cardWidth < 150) cardWidth = 150;

            string[] bloodGroups = { "A+", "A-", "B+", "B-", "AB+", "AB-", "O+", "O-" };
            int cardIndex = 0;

            foreach (string bg in bloodGroups)
            {
                int units = GetAvailableUnits(bg);
                Color cardColor = units > 20 ? Color.FromArgb(34, 197, 94) :
                                 units > 5 ? Color.FromArgb(245, 158, 11) :
                                 Color.FromArgb(220, 38, 38);

                Panel card = new Panel
                {
                    Width = cardWidth,
                    Height = 95,
                    BackColor = Color.White,
                    Margin = new Padding(5, 5, 5, 5)
                };
                card.Paint += (s, e) => DrawRoundedCard(s, e);

                Label lblGroup = new Label
                {
                    Text = bg,
                    Font = new Font("Segoe UI", 16, FontStyle.Bold),
                    ForeColor = cardColor,
                    Location = new Point(10, 12),
                    AutoSize = true
                };
                card.Controls.Add(lblGroup);

                Label lblUnits = new Label
                {
                    Text = $"{units} units",
                    Font = new Font("Segoe UI", 12),
                    ForeColor = Color.FromArgb(80, 80, 90),
                    Location = new Point(10, 45),
                    AutoSize = true
                };
                card.Controls.Add(lblUnits);

                Label lblStatus = new Label
                {
                    Text = units > 0 ? GetExpiryStatus(bg) : "⚠️ Out of Stock",
                    Font = new Font("Segoe UI", 8),
                    ForeColor = units > 0 ? Color.Gray : Color.Red,
                    Location = new Point(10, 70),
                    AutoSize = true
                };
                card.Controls.Add(lblStatus);

                parent.Controls.Add(card);
                cardIndex++;
            }
        }

        private void DrawRoundedCard(object sender, PaintEventArgs e)
        {
            Panel p = sender as Panel;
            GraphicsPath path = new GraphicsPath();
            int radius = 12;
            path.AddArc(0, 0, radius, radius, 180, 90);
            path.AddArc(p.Width - radius, 0, radius, radius, 270, 90);
            path.AddArc(p.Width - radius, p.Height - radius, radius, radius, 0, 90);
            path.AddArc(0, p.Height - radius, radius, radius, 90, 90);
            path.CloseFigure();
            p.Region = new Region(path);

            using (Pen pen = new Pen(Color.FromArgb(220, 220, 220), 1))
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                e.Graphics.DrawPath(pen, path);
            }
        }

        // =====================================================
        // REFRESH CARDS (Filter by selected blood group)
        // =====================================================
        private void RefreshCards()
        {
            if (cardsFlow == null) return;

            cardsFlow.SuspendLayout();
            cardsFlow.Controls.Clear();
            cardsFlow.Width = mainFlow.ClientSize.Width - 30;

            string selectedGroup = cmbBloodGroup.SelectedItem?.ToString() ?? "All Blood Groups";

            foreach (var bank in bloodBanks)
            {
                // Check if bank has the selected blood group
                if (selectedGroup != "All Blood Groups" && !bank.AvailableBloodGroups.Contains(selectedGroup))
                    continue;

                Panel card = CreateHospitalCard(bank, selectedGroup);
                cardsFlow.Controls.Add(card);
            }

            if (cardsFlow.Controls.Count == 0)
            {
                Label lblNoData = new Label
                {
                    Text = $"⚠️ No hospitals found with {selectedGroup} blood available.\n\nPlease try another blood group or contact nearby hospitals.",
                    Font = new Font("Segoe UI", 12),
                    ForeColor = Color.Gray,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Width = 500,
                    Height = 100,
                    Margin = new Padding(50, 50, 0, 0)
                };
                cardsFlow.Controls.Add(lblNoData);
            }

            cardsFlow.ResumeLayout(true);
            this.BeginInvoke(new Action(() => UpdateSizes()));
        }

        private Panel CreateHospitalCard(BloodBankStock bank, string selectedGroup)
        {
            int cardWidth = GetCardWidth();

            Panel card = new Panel
            {
                Width = cardWidth,
                Height = CARD_HEIGHT,
                Margin = new Padding(10),
                BackColor = Color.White
            };
            card.Paint += RoundedPaint;

            // Hospital Name
            card.Controls.Add(new Label
            {
                Text = bank.Name,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.FromArgb(120, 22, 27),
                Location = new Point(15, 12),
                AutoSize = true
            });

            // City/Location
            card.Controls.Add(new Label
            {
                Text = $"📍 {bank.City}",
                Location = new Point(15, 40),
                AutoSize = true,
                ForeColor = Color.Gray,
                Font = new Font("Segoe UI", 9)
            });

            // Phone
            card.Controls.Add(new Label
            {
                Text = $"📞 {bank.Phone}",
                Location = new Point(15, 62),
                AutoSize = true,
                ForeColor = Color.Gray,
                Font = new Font("Segoe UI", 9)
            });

            // Available Blood Groups
            string availableText = "🩸 " + string.Join(", ", bank.AvailableBloodGroups.Take(4));
            if (bank.AvailableBloodGroups.Count > 4)
                availableText += $" +{bank.AvailableBloodGroups.Count - 4} more";

            card.Controls.Add(new Label
            {
                Text = availableText,
                Location = new Point(15, 84),
                AutoSize = true,
                ForeColor = Color.FromArgb(60, 60, 70),
                Font = new Font("Segoe UI", 8)
            });

            // Units available for selected group
            if (selectedGroup != "All Blood Groups")
            {
                int units = GetAvailableUnits(selectedGroup);
                string unitsText = units > 0 ? $"📦 {units} units available" : "❌ Out of stock";
                Color unitsColor = units > 0 ? Color.FromArgb(34, 197, 94) : Color.FromArgb(220, 38, 38);

                card.Controls.Add(new Label
                {
                    Text = unitsText,
                    Location = new Point(15, 108),
                    AutoSize = true,
                    ForeColor = unitsColor,
                    Font = new Font("Segoe UI", 9, FontStyle.Bold)
                });
            }

            // Request Button
            Button btnRequest = new Button
            {
                Text = "Request Blood",
                Size = new Size(100, 32),
                BackColor = Color.FromArgb(120, 22, 27),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Tag = bank
            };
            btnRequest.FlatAppearance.BorderSize = 0;
            btnRequest.Click += (s, e) =>
            {
                BloodBankStock selected = (BloodBankStock)((Button)s).Tag;
                string bloodGroup = selectedGroup != "All Blood Groups" ? selectedGroup : "specific";

                DialogResult result = MessageBox.Show(
                    $"Do you want to request blood from:\n\n" +
                    $"🏥 {selected.Name}\n" +
                    $"📍 {selected.City}\n" +
                    $"📞 {selected.Phone}\n\n" +
                    $"Blood Group: {bloodGroup}\n\n" +
                    $"Click Yes to proceed with request.",
                    "Request Blood",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    // Navigate to Request Blood page
                    var parentForm = this.ParentForm as PatientDashboard;
                    if (parentForm != null)
                    {
                        parentForm.NavigateToMenu("Request Blood");
                    }
                }
            };

            card.Controls.Add(btnRequest);

            card.Layout += (s, e) =>
            {
                btnRequest.Location = new Point(card.Width - 115, card.Height - 45);
            };

            return card;
        }

        private int GetCardWidth()
        {
            int w = 0;
            if (cardsFlow != null && cardsFlow.ClientSize.Width > 0)
                w = cardsFlow.ClientSize.Width;
            else if (mainFlow != null && mainFlow.ClientSize.Width > 0)
                w = mainFlow.ClientSize.Width - 30;
            else if (this.ClientSize.Width > 0)
                w = this.ClientSize.Width - 30;

            if (w <= 60) return 320;

            int cardW = (w / 3) - 20;
            return cardW < 280 ? 280 : cardW;
        }

        private void UpdateSizes()
        {
            if (cardsFlow == null) return;
            int w = GetCardWidth();
            foreach (Control c in cardsFlow.Controls)
            {
                if (c is Panel)
                {
                    c.Width = w;
                }
            }
        }

        private void RoundedPaint(object sender, PaintEventArgs e)
        {
            Control c = sender as Control;
            GraphicsPath p = new GraphicsPath();
            int r = 15;
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

        // =====================================================
        // MODEL CLASS
        // =====================================================
        private class BloodBankStock
        {
            public string Name { get; set; }
            public string City { get; set; }
            public string Phone { get; set; }
            public List<string> AvailableBloodGroups { get; set; }
            public string Distance { get; set; }
        }
    }
}