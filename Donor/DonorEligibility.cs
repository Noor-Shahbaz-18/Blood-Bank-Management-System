using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using BloodBankManagementSystem.Classes.Database;
using BloodBankManagementSystem.Classes.Helpers;

namespace BloodBankManagementSystem.Forms.Donor
{
    public partial class DonorEligibility : Form
    {
        private readonly Color darkRed = Color.FromArgb(120, 22, 27);
        private readonly Color lightBg = Color.FromArgb(250, 245, 245);

        private NumericUpDown numAge, numWeight;
        private DateTimePicker dtpLastDonation;
        private CheckBox chkAnemia, chkHighBP, chkDiabetes,
                               chkHeartDisease, chkRecentSurgery, chkTattoo;
        private Button btnCheck;
        private Panel resultPanel;
        private Label lblResultIcon, lblResultTitle, lblResultMessage;
        private Label lblProfileInfo;

        public DonorEligibility()
        {
            InitializeComponent();
            BuildUI();
            LoadDonorInfoFromDatabase();
        }

        // =====================================================
        // BUILD UI
        // =====================================================
        private void BuildUI()
        {
            this.Text = "Check Donor Eligibility";
            this.Size = new Size(620, 700);
            this.MinimumSize = new Size(620, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(245, 245, 245);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            // ── Outer scroll container ──
            Panel outerPanel = new Panel();
            outerPanel.Dock = DockStyle.Fill;
            outerPanel.AutoScroll = true;
            outerPanel.Padding = new Padding(0);
            this.Controls.Add(outerPanel);

            // ── White card ──
            Panel card = new Panel();
            card.BackColor = Color.White;
            card.Size = new Size(560, 640);
            card.Location = new Point(30, 20);
            card.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (Pen pen = new Pen(Color.FromArgb(220, 200, 200), 1))
                using (System.Drawing.Drawing2D.GraphicsPath path = RoundRect(
                    new Rectangle(0, 0, card.Width - 1, card.Height - 1), 14))
                {
                    e.Graphics.DrawPath(pen, path);
                    card.Region = new Region(path);
                }
            };
            outerPanel.Controls.Add(card);

            int y = 30;
            int lx = 30;        // left X
            int lw = 160;       // label width
            int fx = 200;       // field X
            int fw = 300;       // field width

            // ── Header ──
            Panel header = new Panel();
            header.Size = new Size(560, 75);
            header.Location = new Point(0, 0);
            header.BackColor = darkRed;
            header.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                // Rounded top only
                using (System.Drawing.Drawing2D.GraphicsPath path = new System.Drawing.Drawing2D.GraphicsPath())
                {
                    int r = 14;
                    path.AddArc(0, 0, r * 2, r * 2, 180, 90);
                    path.AddArc(header.Width - r * 2, 0, r * 2, r * 2, 270, 90);
                    path.AddLine(header.Width, header.Height, 0, header.Height);
                    path.CloseFigure();
                    header.Region = new Region(path);
                }
            };
            card.Controls.Add(header);

            Label lblIcon = new Label();
            lblIcon.Text = "🩸";
            lblIcon.Font = new Font("Segoe UI Emoji", 26);
            lblIcon.ForeColor = Color.White;
            lblIcon.Location = new Point(30, 12);
            lblIcon.AutoSize = true;
            header.Controls.Add(lblIcon);

            Label lblTitle = new Label();
            lblTitle.Text = "Donor Eligibility Checker";
            lblTitle.Font = new Font("Segoe UI", 18, FontStyle.Bold);
            lblTitle.ForeColor = Color.White;
            lblTitle.Location = new Point(78, 20);
            lblTitle.AutoSize = true;
            header.Controls.Add(lblTitle);

            y = 95; // below header

            // ── Section: Basic Info ──
            AddSectionLabel(card, "Basic Information", lx, y); y += 32;

            AddFieldRow(card, "Age (years):", lx, lw, fx, y);
            numAge = new NumericUpDown
            {
                Location = new Point(fx, y - 2),
                Width = 110,
                Minimum = 16,
                Maximum = 70,
                Value = 25,
                Font = new Font("Segoe UI", 11)
            };
            card.Controls.Add(numAge);
            y += 40;

            AddFieldRow(card, "Weight (kg):", lx, lw, fx, y);
            numWeight = new NumericUpDown
            {
                Location = new Point(fx, y - 2),
                Width = 110,
                Minimum = 30,
                Maximum = 200,
                Value = 65,
                Font = new Font("Segoe UI", 11)
            };
            card.Controls.Add(numWeight);
            y += 40;

            AddFieldRow(card, "Last Donation:", lx, lw, fx, y);
            dtpLastDonation = new DateTimePicker
            {
                Location = new Point(fx, y - 2),
                Width = 170,
                Format = DateTimePickerFormat.Short,
                Font = new Font("Segoe UI", 11),
                Value = DateTime.Now.AddMonths(-4)
            };
            card.Controls.Add(dtpLastDonation);
            y += 40;

            // Profile info label (shown after DB load)
            lblProfileInfo = new Label();
            lblProfileInfo.Text = "";
            lblProfileInfo.Font = new Font("Segoe UI", 8, FontStyle.Italic);
            lblProfileInfo.ForeColor = Color.FromArgb(0, 100, 180);
            lblProfileInfo.Location = new Point(lx, y);
            lblProfileInfo.Size = new Size(500, 20);
            card.Controls.Add(lblProfileInfo);
            y += 26;

            // ── Divider ──
            Panel div = new Panel
            {
                Location = new Point(lx, y),
                Size = new Size(500, 1),
                BackColor = Color.FromArgb(220, 210, 210)
            };
            card.Controls.Add(div);
            y += 14;

            // ── Section: Health Conditions ──
            AddSectionLabel(card, "Health Conditions", lx, y); y += 32;

            chkAnemia = AddCheck(card, "Anemia / Low Hemoglobin", lx, y); y += 30;
            chkHighBP = AddCheck(card, "High Blood Pressure", lx, y); y += 30;
            chkDiabetes = AddCheck(card, "Diabetes", lx, y); y += 30;
            chkHeartDisease = AddCheck(card, "Heart Disease", lx, y); y += 30;
            chkRecentSurgery = AddCheck(card, "Recent Surgery (within 6 months)", lx, y); y += 30;
            chkTattoo = AddCheck(card, "Recent Tattoo / Piercing (within 12 months)", lx, y); y += 40;

            // ── Check Button ──
            btnCheck = new Button();
            btnCheck.Text = "🔍  Check Eligibility";
            btnCheck.Size = new Size(220, 48);
            btnCheck.Location = new Point((card.Width - 220) / 2, y);
            btnCheck.BackColor = darkRed;
            btnCheck.ForeColor = Color.White;
            btnCheck.FlatStyle = FlatStyle.Flat;
            btnCheck.FlatAppearance.BorderSize = 0;
            btnCheck.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            btnCheck.Cursor = Cursors.Hand;
            btnCheck.Click += BtnCheck_Click;
            btnCheck.MouseEnter += (s, e) => btnCheck.BackColor = Color.FromArgb(160, 30, 35);
            btnCheck.MouseLeave += (s, e) => btnCheck.BackColor = darkRed;
            card.Controls.Add(btnCheck);
            y += 62;

            // ── Result Panel ──
            resultPanel = new Panel();
            resultPanel.Location = new Point(lx, y);
            resultPanel.Size = new Size(500, 115);
            resultPanel.BackColor = lightBg;
            resultPanel.Visible = false;
            resultPanel.Paint += (s, e) =>
            {
                using (Pen pen = new Pen(Color.FromArgb(210, 190, 190), 1))
                    e.Graphics.DrawRectangle(pen, 0, 0, resultPanel.Width - 1, resultPanel.Height - 1);
            };
            card.Controls.Add(resultPanel);

            lblResultIcon = new Label
            {
                Location = new Point(14, 22),
                Size = new Size(44, 44),
                Font = new Font("Segoe UI Emoji", 24),
                TextAlign = ContentAlignment.MiddleCenter
            };

            lblResultTitle = new Label
            {
                Location = new Point(68, 16),
                Size = new Size(420, 30),
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft
            };

            lblResultMessage = new Label
            {
                Location = new Point(68, 52),
                Size = new Size(420, 55),
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.FromArgb(80, 80, 80)
            };

            resultPanel.Controls.Add(lblResultIcon);
            resultPanel.Controls.Add(lblResultTitle);
            resultPanel.Controls.Add(lblResultMessage);

            // Grow card height to fit result panel
            card.Height = y + resultPanel.Height + 25;
        }

        // =====================================================
        // HELPERS
        // =====================================================
        private void AddSectionLabel(Panel parent, string text, int x, int y)
        {
            Label lbl = new Label
            {
                Text = text,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = darkRed,
                Location = new Point(x, y),
                AutoSize = true
            };
            parent.Controls.Add(lbl);
        }

        private void AddFieldRow(Panel parent, string labelText, int lx, int lw, int fx, int y)
        {
            Label lbl = new Label
            {
                Text = labelText,
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.FromArgb(60, 60, 60),
                Location = new Point(lx, y + 3),
                Size = new Size(lw, 26),
                TextAlign = ContentAlignment.MiddleLeft
            };
            parent.Controls.Add(lbl);
        }

        private CheckBox AddCheck(Panel parent, string text, int x, int y)
        {
            CheckBox chk = new CheckBox
            {
                Text = text,
                Location = new Point(x, y),
                AutoSize = true,
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.FromArgb(50, 50, 50)
            };
            parent.Controls.Add(chk);
            return chk;
        }

        private System.Drawing.Drawing2D.GraphicsPath RoundRect(Rectangle rect, int radius)
        {
            var path = new System.Drawing.Drawing2D.GraphicsPath();
            path.AddArc(rect.X, rect.Y, radius * 2, radius * 2, 180, 90);
            path.AddArc(rect.X + rect.Width - radius * 2, rect.Y, radius * 2, radius * 2, 270, 90);
            path.AddArc(rect.X + rect.Width - radius * 2, rect.Y + rect.Height - radius * 2, radius * 2, radius * 2, 0, 90);
            path.AddArc(rect.X, rect.Y + rect.Height - radius * 2, radius * 2, radius * 2, 90, 90);
            path.CloseFigure();
            return path;
        }

        // =====================================================
        // LOAD FROM DATABASE
        // =====================================================
        private void LoadDonorInfoFromDatabase()
        {
            try
            {
                int userID = SessionManager.CurrentUserID;
                var donor = DonorDAL.GetDonorByUserID(userID);
                if (donor == null) return;

                bool anyLoaded = false;

                if (donor.DateOfBirth != DateTime.MinValue && donor.DateOfBirth.Year > 1900)
                {
                    int age = DateTimeHelper.CalculateAge(donor.DateOfBirth);
                    if (age >= 16 && age <= 70)
                    {
                        numAge.Value = age;
                        numAge.ReadOnly = true;
                        anyLoaded = true;
                    }
                }

                if (donor.Weight >= 30 && donor.Weight <= 200)
                {
                    numWeight.Value = donor.Weight;
                    numWeight.ReadOnly = true;
                    anyLoaded = true;
                }

                if (donor.LastDonationDate.HasValue)
                {
                    dtpLastDonation.Value = donor.LastDonationDate.Value;
                    dtpLastDonation.Enabled = false;
                    anyLoaded = true;
                }

                if (anyLoaded)
                    lblProfileInfo.Text = "ℹ️  Age, weight and last donation date loaded from your profile.";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading donor info: {ex.Message}");
            }
        }

        // =====================================================
        // CHECK ELIGIBILITY
        // =====================================================
        private void BtnCheck_Click(object sender, EventArgs e)
        {
            int age = (int)numAge.Value;
            int weight = (int)numWeight.Value;
            DateTime lastDonation = dtpLastDonation.Value;

            bool hasHealthIssue = chkAnemia.Checked || chkHighBP.Checked ||
                                     chkDiabetes.Checked || chkHeartDisease.Checked;
            bool hasRecentProcedure = chkRecentSurgery.Checked || chkTattoo.Checked;

            bool isEligible = true;
            string message = "";

            if (age < 18)
            {
                isEligible = false;
                message += "• Age must be at least 18 years.\n";
            }
            else if (age > 65)
            {
                isEligible = false;
                message += "• Age must be less than 65 years.\n";
            }

            if (weight < 50)
            {
                isEligible = false;
                message += "• Weight must be at least 50 kg.\n";
            }

            if (lastDonation > DateTime.Now.AddMonths(-3))
            {
                isEligible = false;
                message += "• You must wait 3 months between donations.\n";
            }

            if (hasHealthIssue)
            {
                isEligible = false;
                message += "• Please consult a doctor before donating.\n";
            }

            if (hasRecentProcedure)
            {
                isEligible = false;
                message += "• You have a temporary deferral period.\n";
            }

            resultPanel.Visible = true;

            if (isEligible)
            {
                resultPanel.BackColor = Color.FromArgb(240, 253, 244);
                lblResultIcon.Text = "✅";
                lblResultIcon.ForeColor = Color.FromArgb(21, 128, 61);
                lblResultTitle.Text = "YOU ARE ELIGIBLE TO DONATE BLOOD!";
                lblResultTitle.ForeColor = Color.FromArgb(21, 128, 61);
                lblResultMessage.Text = "Thank you for your willingness to donate. Please schedule an appointment at your nearest blood bank.";
                lblResultMessage.ForeColor = Color.FromArgb(40, 100, 60);
            }
            else
            {
                resultPanel.BackColor = Color.FromArgb(255, 243, 243);
                lblResultIcon.Text = "❌";
                lblResultIcon.ForeColor = Color.FromArgb(185, 28, 28);
                lblResultTitle.Text = "NOT ELIGIBLE AT THIS TIME";
                lblResultTitle.ForeColor = Color.FromArgb(185, 28, 28);
                lblResultMessage.Text = message.TrimEnd('\n');
                lblResultMessage.ForeColor = Color.FromArgb(100, 40, 40);
            }

            // Scroll result into view
            (resultPanel.Parent as ScrollableControl)?.ScrollControlIntoView(resultPanel);
        }
    }
}