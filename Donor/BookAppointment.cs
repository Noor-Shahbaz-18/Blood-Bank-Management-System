using BloodBankManagementSystem.Classes.Database;
using BloodBankManagementSystem.Classes.Helpers;
using System;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace BloodBankManagementSystem.Forms.Donor
{
    public class BookAppointment : Form
    {
        private Panel sidebarPanel;
        private Panel mainPanel;
        private Color brickRed = Color.FromArgb(120, 22, 27);
        private ComboBox bloodBankCombo;
        private DateTimePicker datePicker;
        private Button selectedTimeBtn;
        private FlowLayoutPanel timeSlotsPanel;
        private Label lblStatus;

        // Time slots with their hour values for comparison
        private class TimeSlotInfo
        {
            public string DisplayText { get; set; }
            public int Hour { get; set; }
            public int Minute { get; set; }

            public TimeSlotInfo(string display, int hour, int minute)
            {
                DisplayText = display;
                Hour = hour;
                Minute = minute;
            }
        }

        private TimeSlotInfo[] timeSlots = new TimeSlotInfo[]
        {
            new TimeSlotInfo("09:00 AM", 9, 0),
            new TimeSlotInfo("11:00 AM", 11, 0),
            new TimeSlotInfo("01:00 PM", 13, 0),
            new TimeSlotInfo("03:00 PM", 15, 0),
            new TimeSlotInfo("05:00 PM", 17, 0)
        };

        public BookAppointment()
        {
            this.Text = "Book Appointment - Blood Donor";
            this.WindowState = FormWindowState.Maximized;
            this.BackColor = Color.FromArgb(244, 244, 244);
            this.MinimumSize = new Size(1100, 650);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.AutoScaleMode = AutoScaleMode.Dpi;

            BuildMainContent();
            BuildSidebar();

            // Load available time slots when date changes
            datePicker.ValueChanged += DatePicker_ValueChanged;
            LoadAvailableTimeSlots();
        }

        // =====================================================
        // CHECK IF TIME SLOT HAS PASSED FOR TODAY
        // =====================================================
        private bool IsTimeSlotPassed(TimeSlotInfo slot)
        {
            DateTime selectedDate = datePicker.Value.Date;
            DateTime now = DateTime.Now;

            // If selected date is in the future, no time slot is passed
            if (selectedDate > now.Date)
                return false;

            // If selected date is today, check if slot time has passed
            if (selectedDate == now.Date)
            {
                DateTime slotTime = new DateTime(now.Year, now.Month, now.Day, slot.Hour, slot.Minute, 0);
                return slotTime < now;
            }

            // If selected date is in the past, all slots are passed
            return true;
        }

        // =====================================================
        // LOAD AVAILABLE TIME SLOTS BASED ON SELECTED DATE
        // =====================================================
        private void LoadAvailableTimeSlots()
        {
            DateTime selectedDate = datePicker.Value.Date;

            // Check if selected date is in the past
            if (selectedDate < DateTime.Today)
            {
                MessageBox.Show("Cannot select past date. Please select today or future date.",
                    "Invalid Date", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                datePicker.Value = DateTime.Today;
                return;
            }

            // Clear existing time slot buttons
            timeSlotsPanel.Controls.Clear();

            // Get current donor ID
            int donorID = GetCurrentDonorID();

            foreach (TimeSlotInfo slot in timeSlots)
            {
                bool isAvailable = AppointmentDAL.IsTimeSlotAvailable(selectedDate, slot.DisplayText);
                bool alreadyBooked = IsAlreadyBookedByDonor(donorID, selectedDate, slot.DisplayText);
                bool isTimePassed = IsTimeSlotPassed(slot);

                Button timeBtn = new Button();
                timeBtn.Font = new Font("Segoe UI", 10);
                timeBtn.Size = new Size(115, 40);
                timeBtn.Margin = new Padding(0, 0, 10, 10);
                timeBtn.FlatStyle = FlatStyle.Flat;
                timeBtn.FlatAppearance.BorderSize = 1;
                timeBtn.Cursor = Cursors.Hand;

                // Check if time slot has passed for today
                if (isTimePassed)
                {
                    timeBtn.BackColor = Color.LightGray;
                    timeBtn.ForeColor = Color.DarkGray;
                    timeBtn.Enabled = false;
                    timeBtn.FlatAppearance.BorderColor = Color.LightGray;
                    timeBtn.Text = slot.DisplayText + " (Passed)";
                }
                // Check if slot is fully booked
                else if (!isAvailable)
                {
                    timeBtn.BackColor = Color.LightGray;
                    timeBtn.ForeColor = Color.DarkGray;
                    timeBtn.Enabled = false;
                    timeBtn.FlatAppearance.BorderColor = Color.LightGray;
                    timeBtn.Text = slot.DisplayText + " (Full)";
                }
                // Check if donor already booked this slot
                else if (alreadyBooked)
                {
                    timeBtn.BackColor = Color.LightGreen;
                    timeBtn.ForeColor = Color.DarkGreen;
                    timeBtn.Enabled = false;
                    timeBtn.FlatAppearance.BorderColor = Color.LightGreen;
                    timeBtn.Text = slot.DisplayText + " (Booked)";
                }
                else
                {
                    // Slot is available
                    timeBtn.BackColor = Color.White;
                    timeBtn.ForeColor = Color.FromArgb(51, 51, 51);
                    timeBtn.FlatAppearance.BorderColor = Color.FromArgb(200, 200, 205);
                    timeBtn.Enabled = true;
                    timeBtn.Text = slot.DisplayText;

                    timeBtn.Click += (s, e) =>
                    {
                        // Reset all buttons
                        foreach (Control c in timeSlotsPanel.Controls)
                        {
                            if (c is Button b && b.Enabled && !b.Text.Contains("(Passed)") && !b.Text.Contains("(Full)") && !b.Text.Contains("(Booked)"))
                            {
                                b.BackColor = Color.White;
                                b.ForeColor = Color.FromArgb(51, 51, 51);
                                b.FlatAppearance.BorderColor = Color.FromArgb(200, 200, 205);
                            }
                        }

                        Button clicked = (Button)s;
                        clicked.BackColor = brickRed;
                        clicked.ForeColor = Color.White;
                        clicked.FlatAppearance.BorderColor = brickRed;
                        selectedTimeBtn = clicked;
                    };

                    timeBtn.MouseEnter += (s, e) =>
                    {
                        Button btn = (Button)s;
                        if (btn != selectedTimeBtn && btn.Enabled)
                            btn.BackColor = Color.FromArgb(255, 240, 240);
                    };
                    timeBtn.MouseLeave += (s, e) =>
                    {
                        Button btn = (Button)s;
                        if (btn != selectedTimeBtn && btn.Enabled)
                            btn.BackColor = Color.White;
                    };
                }

                timeBtn.Paint += (s, e) =>
                {
                    Button btn = (Button)s;
                    e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    using (GraphicsPath path = RoundRect(btn.ClientRectangle, 8))
                        btn.Region = new Region(path);
                };

                timeSlotsPanel.Controls.Add(timeBtn);
            }

            // Update status label
            if (selectedDate == DateTime.Today)
            {
                lblStatus.Text = "⚠️ Time slots that have already passed today are disabled.";
                lblStatus.ForeColor = Color.Orange;
            }
            else
            {
                lblStatus.Text = "Select date and time slot to book appointment";
                lblStatus.ForeColor = Color.FromArgb(80, 80, 90);
            }
        }

        // Check if donor already booked this time slot
        private bool IsAlreadyBookedByDonor(int donorID, DateTime date, string timeSlot)
        {
            try
            {
                DataTable dt = AppointmentDAL.GetAppointmentsByDonor(donorID);
                if (dt != null && dt.Rows.Count > 0)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        DateTime appDate = Convert.ToDateTime(row["AppointmentDate"]);
                        string appTime = row["TimeSlot"].ToString();

                        if (appDate.Date == date.Date && appTime == timeSlot)
                        {
                            return true; // Already booked
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error checking booking: {ex.Message}");
            }
            return false;
        }

        private void DatePicker_ValueChanged(object sender, EventArgs e)
        {
            LoadAvailableTimeSlots();
            selectedTimeBtn = null;
        }

        // =====================================================
        // HELPER: Get DonorID from current session
        // =====================================================
        private int GetCurrentDonorID()
        {
            int userID = SessionManager.CurrentUserID;
            var donor = DonorDAL.GetDonorByUserID(userID);

            if (donor == null)
            {
                throw new Exception("Donor profile not found. Please contact administrator.");
            }

            return donor.DonorID;
        }

        // =====================================================
        // SIDEBAR (Left side fixed)
        // =====================================================
        private void BuildSidebar()
        {
            sidebarPanel = new Panel();
            sidebarPanel.Dock = DockStyle.Left;
            sidebarPanel.Width = 250;
            sidebarPanel.BackColor = brickRed;
            sidebarPanel.Padding = new Padding(18, 20, 18, 25);

            // LOGO PANEL
            Panel logoContainer = new Panel();
            logoContainer.Dock = DockStyle.Top;
            logoContainer.Height = 130;
            logoContainer.BackColor = Color.White;
            logoContainer.Paint += (s, e) =>
            {
                Panel p = (Panel)s;
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (GraphicsPath path = RoundRect(p.ClientRectangle, 12))
                    p.Region = new Region(path);
            };

            TableLayoutPanel logoTable = new TableLayoutPanel();
            logoTable.Dock = DockStyle.Fill;
            logoTable.RowCount = 3;
            logoTable.ColumnCount = 1;
            logoTable.RowStyles.Add(new RowStyle(SizeType.Percent, 45f));
            logoTable.RowStyles.Add(new RowStyle(SizeType.Percent, 32f));
            logoTable.RowStyles.Add(new RowStyle(SizeType.Percent, 23f));
            logoTable.Padding = new Padding(5);
            logoTable.BackColor = Color.Transparent;
            logoContainer.Controls.Add(logoTable);

            // Blood Drop Icon
            PictureBox bloodLogo = new PictureBox();
            bloodLogo.SizeMode = PictureBoxSizeMode.Zoom;
            bloodLogo.Dock = DockStyle.Fill;
            bloodLogo.Margin = new Padding(0, 5, 0, 5);
            bloodLogo.BackColor = Color.White;

            Bitmap bmp = new Bitmap(100, 100);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.Clear(Color.White);
                using (SolidBrush brush = new SolidBrush(brickRed))
                {
                    GraphicsPath path = new GraphicsPath();
                    path.AddEllipse(25, 45, 50, 50);
                    Point[] points = { new Point(50, 15), new Point(32, 55), new Point(68, 55) };
                    path.AddPolygon(points);
                    path.FillMode = FillMode.Winding;
                    g.FillPath(brush, path);
                }
            }
            bloodLogo.Image = bmp;
            logoTable.Controls.Add(bloodLogo, 0, 0);

            Label systemName = new Label();
            systemName.Text = "Blood Bank";
            systemName.Font = new Font("Segoe UI", 16, FontStyle.Bold);
            systemName.ForeColor = brickRed;
            systemName.Dock = DockStyle.Fill;
            systemName.TextAlign = ContentAlignment.MiddleCenter;
            logoTable.Controls.Add(systemName, 0, 1);

            Label systemSubName = new Label();
            systemSubName.Text = "Management System";
            systemSubName.Font = new Font("Segoe UI", 9);
            systemSubName.ForeColor = Color.FromArgb(107, 114, 128);
            systemSubName.Dock = DockStyle.Fill;
            systemSubName.TextAlign = ContentAlignment.TopCenter;
            logoTable.Controls.Add(systemSubName, 0, 2);

            // Menu Panel
            FlowLayoutPanel menuPanel = new FlowLayoutPanel();
            menuPanel.Dock = DockStyle.Fill;
            menuPanel.FlowDirection = FlowDirection.TopDown;
            menuPanel.WrapContents = false;
            menuPanel.AutoScroll = true;
            menuPanel.Padding = new Padding(0, 15, 0, 0);
            menuPanel.BackColor = Color.Transparent;

            // Menu Items
            AddDonorMenuItem(menuPanel, "🏠", "Dashboard", false, () =>
            {
                this.Hide();
                DonorDashboard dashboard = new DonorDashboard();
                dashboard.ShowDialog();
                this.Close();
            });

            AddDonorMenuItem(menuPanel, "📅", "Appointments", true, null);

            AddDonorMenuItem(menuPanel, "📄", "Donation History", false, () =>
            {
                this.Hide();
                DonationHistory historyForm = new DonationHistory();
                historyForm.ShowDialog();
                this.Show();
            });

            AddDonorMenuItem(menuPanel, "📍", "Nearby Camps", false, () =>
            {
                NearbyCamps campsForm = new NearbyCamps();
                campsForm.Owner = this;
                this.Hide();
                campsForm.FormClosed += (s, args) => this.Show();
                campsForm.Show();
            });

            AddDonorMenuItem(menuPanel, "🔔", "Notifications", false, () =>
            {
                DonorNotifications notifForm = new DonorNotifications();
                notifForm.Owner = this;
                this.Hide();
                notifForm.FormClosed += (s, args) => this.Show();
                notifForm.Show();
            });

            AddDonorMenuItem(menuPanel, "👤", "Profile Settings", false, () =>
            {
                DonorProfileSetting profileForm = new DonorProfileSetting();
                profileForm.Owner = this;
                this.Hide();
                profileForm.FormClosed += (s, args) => this.Show();
                profileForm.Show();
            });

            // Spacer
            Panel spacer = new Panel();
            spacer.Size = new Size(210, 1);
            spacer.Margin = new Padding(0, 20, 0, 0);
            spacer.BackColor = Color.Transparent;
            menuPanel.Controls.Add(spacer);

            // Logout
            AddDonorMenuItem(menuPanel, "🚪", "Logout", false, () =>
            {
                DialogResult result = MessageBox.Show("Are you sure you want to logout?",
                    "Logout Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                {
                    Application.Exit();
                }
            });

            sidebarPanel.Controls.Add(menuPanel);
            sidebarPanel.Controls.Add(logoContainer);
            this.Controls.Add(sidebarPanel);
        }

        private void AddDonorMenuItem(FlowLayoutPanel panel, string icon, string text, bool active, Action clickAction)
        {
            Panel item = new Panel();
            item.Size = new Size(210, 48);
            item.Margin = new Padding(0, 3, 0, 3);
            item.Cursor = Cursors.Hand;
            item.BackColor = active ? Color.White : Color.Transparent;
            item.Paint += (s, e) =>
            {
                if (active)
                {
                    Panel p = (Panel)s;
                    e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    using (GraphicsPath path = RoundRect(p.ClientRectangle, 12))
                        p.Region = new Region(path);
                }
            };

            Label iconLbl = new Label();
            iconLbl.Text = icon;
            iconLbl.Font = new Font("Segoe UI Emoji", 14);
            iconLbl.ForeColor = active ? brickRed : Color.White;
            iconLbl.BackColor = Color.Transparent;
            iconLbl.AutoSize = false;
            iconLbl.Size = new Size(30, 30);
            iconLbl.Location = new Point(14, 9);
            iconLbl.TextAlign = ContentAlignment.MiddleCenter;

            Label textLbl = new Label();
            textLbl.Text = text;
            textLbl.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            textLbl.ForeColor = active ? brickRed : Color.White;
            textLbl.BackColor = Color.Transparent;
            textLbl.AutoSize = false;
            textLbl.Size = new Size(150, 30);
            textLbl.Location = new Point(50, 9);
            textLbl.TextAlign = ContentAlignment.MiddleLeft;

            EventHandler enter = (s, e) => { if (!active) item.BackColor = Color.FromArgb(200, 50, 50); };
            EventHandler leave = (s, e) => { if (!active) item.BackColor = Color.Transparent; };

            if (clickAction != null)
            {
                EventHandler click = (s, e) => clickAction();
                item.Click += click; iconLbl.Click += click; textLbl.Click += click;
            }

            iconLbl.MouseEnter += enter; iconLbl.MouseLeave += leave;
            textLbl.MouseEnter += enter; textLbl.MouseLeave += leave;
            item.MouseEnter += enter; item.MouseLeave += leave;

            item.Controls.Add(iconLbl);
            item.Controls.Add(textLbl);
            panel.Controls.Add(item);
        }

        // =====================================================
        // MAIN CONTENT (Right side - Appointment Form)
        // =====================================================
        private void BuildMainContent()
        {
            mainPanel = new Panel();
            mainPanel.Dock = DockStyle.Fill;
            mainPanel.BackColor = Color.FromArgb(244, 244, 244);
            mainPanel.Padding = new Padding(25);
            mainPanel.AutoScroll = true;

            // ----- TOP BAR -----
            Panel topBar = new Panel();
            topBar.Dock = DockStyle.Top;
            topBar.Height = 65;
            topBar.BackColor = Color.White;
            topBar.Margin = new Padding(0, 0, 0, 20);
            topBar.Paint += (s, e) =>
            {
                Panel p = (Panel)s;
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (GraphicsPath path = RoundRect(p.ClientRectangle, 15))
                    p.Region = new Region(path);
            };

            Label titleLabel = new Label();
            titleLabel.Text = "📅 Book Appointment";
            titleLabel.Font = new Font("Segoe UI", 20, FontStyle.Bold);
            titleLabel.ForeColor = Color.FromArgb(34, 34, 34);
            titleLabel.BackColor = Color.Transparent;
            titleLabel.AutoSize = true;
            titleLabel.Location = new Point(20, 16);
            topBar.Controls.Add(titleLabel);

            // ----- FORM CARD -----
            Panel formCard = new Panel();
            formCard.Dock = DockStyle.Fill;
            formCard.BackColor = Color.White;
            formCard.Padding = new Padding(30, 25, 30, 25);
            formCard.AutoScroll = true;
            formCard.Paint += (s, e) =>
            {
                Panel p = (Panel)s;
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (GraphicsPath path = RoundRect(p.ClientRectangle, 18))
                    p.Region = new Region(path);
            };

            // Form Title
            Label formTitle = new Label();
            formTitle.Text = "Schedule Your Donation";
            formTitle.Font = new Font("Segoe UI", 16, FontStyle.Bold);
            formTitle.ForeColor = Color.FromArgb(34, 34, 34);
            formTitle.BackColor = Color.Transparent;
            formTitle.Dock = DockStyle.Top;
            formTitle.Height = 35;
            formTitle.Margin = new Padding(0, 0, 0, 20);

            // Status Label
            lblStatus = new Label();
            lblStatus.Text = "Select date and time slot to book appointment";
            lblStatus.Font = new Font("Segoe UI", 10);
            lblStatus.ForeColor = Color.FromArgb(80, 80, 90);
            lblStatus.Dock = DockStyle.Top;
            lblStatus.Height = 30;
            lblStatus.TextAlign = ContentAlignment.MiddleCenter;
            formCard.Controls.Add(lblStatus);

            // Blood Bank Location
            Label locationLabel = new Label();
            locationLabel.Text = "Select Blood Bank Location";
            locationLabel.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            locationLabel.ForeColor = Color.FromArgb(51, 51, 51);
            locationLabel.BackColor = Color.Transparent;
            locationLabel.Dock = DockStyle.Top;
            locationLabel.Height = 22;
            locationLabel.Margin = new Padding(0, 0, 0, 6);

            bloodBankCombo = new ComboBox();
            bloodBankCombo.Font = new Font("Segoe UI", 11);
            bloodBankCombo.DropDownStyle = ComboBoxStyle.DropDownList;
            bloodBankCombo.FlatStyle = FlatStyle.Flat;
            bloodBankCombo.Dock = DockStyle.Top;
            bloodBankCombo.Height = 42;
            bloodBankCombo.Margin = new Padding(0, 0, 0, 18);
            bloodBankCombo.Items.AddRange(new string[] {
                "City Blood Bank, Lahore",
                "Red Crescent Blood Bank",
                "Jinnah Hospital Blood Bank"
            });
            bloodBankCombo.SelectedIndex = 0;

            // Date
            Label dateLabel = new Label();
            dateLabel.Text = "Select Date";
            dateLabel.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            dateLabel.ForeColor = Color.FromArgb(51, 51, 51);
            dateLabel.BackColor = Color.Transparent;
            dateLabel.Dock = DockStyle.Top;
            dateLabel.Height = 22;
            dateLabel.Margin = new Padding(0, 0, 0, 6);

            datePicker = new DateTimePicker();
            datePicker.Font = new Font("Segoe UI", 11);
            datePicker.Format = DateTimePickerFormat.Short;
            datePicker.MinDate = DateTime.Today;
            datePicker.Dock = DockStyle.Top;
            datePicker.Height = 42;
            datePicker.Margin = new Padding(0, 0, 0, 18);

            // Time Slots
            Label timeLabel = new Label();
            timeLabel.Text = "Select Time Slot";
            timeLabel.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            timeLabel.ForeColor = Color.FromArgb(51, 51, 51);
            timeLabel.BackColor = Color.Transparent;
            timeLabel.Dock = DockStyle.Top;
            timeLabel.Height = 22;
            timeLabel.Margin = new Padding(0, 0, 0, 8);

            timeSlotsPanel = new FlowLayoutPanel();
            timeSlotsPanel.Dock = DockStyle.Top;
            timeSlotsPanel.FlowDirection = FlowDirection.LeftToRight;
            timeSlotsPanel.WrapContents = true;
            timeSlotsPanel.AutoSize = true;
            timeSlotsPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            timeSlotsPanel.BackColor = Color.Transparent;
            timeSlotsPanel.Margin = new Padding(0, 0, 0, 20);

            // Instructions
            Panel instructionsPanel = new Panel();
            instructionsPanel.Dock = DockStyle.Top;
            instructionsPanel.BackColor = Color.FromArgb(255, 243, 243);
            instructionsPanel.Padding = new Padding(15);
            instructionsPanel.AutoSize = true;
            instructionsPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            instructionsPanel.Margin = new Padding(0, 0, 0, 20);
            instructionsPanel.Paint += (s, e) =>
            {
                Panel p = (Panel)s;
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (GraphicsPath path = RoundRect(p.ClientRectangle, 12))
                    p.Region = new Region(path);
            };

            Label instructionsTitle = new Label();
            instructionsTitle.Text = "⚠ Important Instructions";
            instructionsTitle.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            instructionsTitle.ForeColor = brickRed;
            instructionsTitle.BackColor = Color.Transparent;
            instructionsTitle.Dock = DockStyle.Top;
            instructionsTitle.Height = 28;

            string[] instructions = {
                "• Please bring your original CNIC.",
                "• Don't donate on an empty stomach.",
                "• Get enough rest and hydration."
            };

            foreach (string inst in instructions)
            {
                Label instLabel = new Label();
                instLabel.Text = inst;
                instLabel.Font = new Font("Segoe UI", 10);
                instLabel.ForeColor = Color.FromArgb(85, 85, 85);
                instLabel.BackColor = Color.Transparent;
                instLabel.Dock = DockStyle.Top;
                instLabel.Height = 22;
                instructionsPanel.Controls.Add(instLabel);
            }

            instructionsPanel.Controls.Add(instructionsTitle);
            instructionsPanel.Controls.SetChildIndex(instructionsTitle, 0);

            // Confirm Button
            Button confirmBtn = new Button();
            confirmBtn.Text = "Confirm Appointment";
            confirmBtn.Font = new Font("Segoe UI", 13, FontStyle.Bold);
            confirmBtn.ForeColor = Color.White;
            confirmBtn.BackColor = brickRed;
            confirmBtn.FlatStyle = FlatStyle.Flat;
            confirmBtn.FlatAppearance.BorderSize = 0;
            confirmBtn.Dock = DockStyle.Top;
            confirmBtn.Height = 48;
            confirmBtn.Cursor = Cursors.Hand;
            confirmBtn.Paint += (s, e) =>
            {
                Button btn = (Button)s;
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (GraphicsPath path = RoundRect(btn.ClientRectangle, 10))
                    btn.Region = new Region(path);
            };
            confirmBtn.Click += (s, e) =>
            {
                if (selectedTimeBtn == null)
                {
                    MessageBox.Show("Please select a time slot!", "Validation",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (datePicker.Value.Date < DateTime.Today)
                {
                    MessageBox.Show("Cannot book appointment for past date!", "Validation",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (bloodBankCombo.SelectedIndex < 0)
                {
                    MessageBox.Show("Please select a blood bank location!", "Validation",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                try
                {
                    int donorID = GetCurrentDonorID();
                    DateTime selectedDate = datePicker.Value.Date;
                    string selectedTime = selectedTimeBtn.Text;

                    // Check if selected time is passed for today
                    if (selectedDate == DateTime.Today)
                    {
                        TimeSpan currentTime = DateTime.Now.TimeOfDay;
                        TimeSpan selectedTimeOfDay = TimeSpan.Zero;

                        if (selectedTime.Contains("09:00")) selectedTimeOfDay = new TimeSpan(9, 0, 0);
                        else if (selectedTime.Contains("11:00")) selectedTimeOfDay = new TimeSpan(11, 0, 0);
                        else if (selectedTime.Contains("01:00")) selectedTimeOfDay = new TimeSpan(13, 0, 0);
                        else if (selectedTime.Contains("03:00")) selectedTimeOfDay = new TimeSpan(15, 0, 0);
                        else if (selectedTime.Contains("05:00")) selectedTimeOfDay = new TimeSpan(17, 0, 0);

                        if (selectedTimeOfDay < currentTime)
                        {
                            MessageBox.Show("Cannot book appointment for a time that has already passed today!",
                                "Time Passed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            LoadAvailableTimeSlots();
                            return;
                        }
                    }

                    // Final availability check before saving
                    if (!AppointmentDAL.IsTimeSlotAvailable(selectedDate, selectedTime))
                    {
                        MessageBox.Show("Sorry, this time slot is no longer available. Please select another time.",
                            "Slot Unavailable", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        LoadAvailableTimeSlots();
                        return;
                    }

                    // Check if donor already booked this slot
                    if (IsAlreadyBookedByDonor(donorID, selectedDate, selectedTime))
                    {
                        MessageBox.Show("You have already booked this time slot. Please select another time.",
                            "Already Booked", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    bool saved = AppointmentDAL.SaveAppointment(
                        donorID,
                        bloodBankCombo.Text,
                        selectedDate,
                        selectedTime
                    );

                    if (saved)
                    {
                        MessageBox.Show(
                            $"✅ Appointment Confirmed!\n\n" +
                            $"📍 Location: {bloodBankCombo.Text}\n" +
                            $"📅 Date: {selectedDate:dd MMM yyyy}\n" +
                            $"🕐 Time: {selectedTime}\n\n" +
                            $"Please arrive 15 minutes before your scheduled time.",
                            "Appointment Confirmed",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);

                        // Refresh available time slots
                        LoadAvailableTimeSlots();
                        selectedTimeBtn = null;
                    }
                    else
                    {
                        MessageBox.Show("Failed to save appointment. Please try again.",
                            "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error: {ex.Message}", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };
            confirmBtn.MouseEnter += (s, e) => confirmBtn.BackColor = Color.FromArgb(196, 7, 17);
            confirmBtn.MouseLeave += (s, e) => confirmBtn.BackColor = brickRed;

            formCard.Controls.Add(confirmBtn);
            formCard.Controls.Add(instructionsPanel);
            formCard.Controls.Add(timeSlotsPanel);
            formCard.Controls.Add(timeLabel);
            formCard.Controls.Add(datePicker);
            formCard.Controls.Add(dateLabel);
            formCard.Controls.Add(bloodBankCombo);
            formCard.Controls.Add(locationLabel);
            formCard.Controls.Add(lblStatus);
            formCard.Controls.Add(formTitle);

            mainPanel.Controls.Add(formCard);
            mainPanel.Controls.Add(topBar);
            this.Controls.Add(mainPanel);
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