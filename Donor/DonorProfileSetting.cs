using BloodBankManagementSystem.Classes.Database;
using BloodBankManagementSystem.Classes.Helpers;
using System;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using DonorModel = BloodBankManagementSystem.Classes.Models.Donor;

namespace BloodBankManagementSystem.Forms.Donor
{
    public class DonorProfileSetting : Form
    {
        private Panel sidebarPanel;
        private Panel mainPanel;
        private Color brickRed = Color.FromArgb(120, 22, 27);
        private DonorModel currentDonor;
        private int currentUserID;

        private PictureBox profilePicture;
        private Button btnChangePicture;
        private byte[] profilePictureBytes = null;

        private TextBox txtFullName, txtEmail, txtPhone, txtAddress, txtCity, txtCNIC, txtWeight, txtEmergencyContact;
        private ComboBox cmbBloodGroup, cmbGender;
        private DateTimePicker dtpDateOfBirth;
        private Label lblAge;
        private Button btnSave;

        public DonorProfileSetting()
        {
            this.Text = "Blood Donor - Profile Settings";
            this.WindowState = FormWindowState.Maximized;
            this.BackColor = Color.FromArgb(245, 247, 250);
            this.MinimumSize = new Size(1100, 650);
            this.StartPosition = FormStartPosition.CenterScreen;

            LoadCurrentDonor();
            BuildMainContent();  // ✅ pehle
            BuildSidebar();      // ✅ baad mein
            LoadProfileData();
        }

        private void LoadCurrentDonor()
        {
            try
            {
                currentUserID = SessionManager.CurrentUserID;
                currentDonor = DonorDAL.GetDonorByUserID(currentUserID);

                if (currentDonor == null)
                {
                    var user = AdminDAL.GetUserByID(currentUserID);
                    if (user != null)
                    {
                        currentDonor = new DonorModel
                        {
                            UserID = currentUserID,
                            FullName = user.FullName,
                            Email = user.Email,
                            Phone = user.Phone,
                            IsActive = true,
                            RegistrationDate = DateTime.Now
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error loading donor: " + ex.Message);
            }
        }

        private void BuildSidebar()
        {
            sidebarPanel = new Panel();
            sidebarPanel.Dock = DockStyle.Left;
            sidebarPanel.Width = 250;
            sidebarPanel.BackColor = brickRed;
            sidebarPanel.Padding = new Padding(18, 20, 18, 25);

            Panel logoContainer = new Panel();
            logoContainer.Dock = DockStyle.Top;
            logoContainer.Height = 130;
            logoContainer.BackColor = Color.White;
            logoContainer.Paint += (s, e) => MakeRounded(logoContainer, 12);

            TableLayoutPanel logoTable = new TableLayoutPanel();
            logoTable.Dock = DockStyle.Fill;
            logoTable.RowCount = 3;
            logoTable.ColumnCount = 1;
            logoTable.RowStyles.Add(new RowStyle(SizeType.Percent, 45f));
            logoTable.RowStyles.Add(new RowStyle(SizeType.Percent, 32f));
            logoTable.RowStyles.Add(new RowStyle(SizeType.Percent, 23f));
            logoTable.Padding = new Padding(5);
            logoContainer.Controls.Add(logoTable);

            PictureBox bloodLogo = new PictureBox();
            bloodLogo.SizeMode = PictureBoxSizeMode.Zoom;
            bloodLogo.Dock = DockStyle.Fill;
            bloodLogo.BackColor = Color.White;
            bloodLogo.Image = CreateBloodDropImage();
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

            FlowLayoutPanel menuPanel = new FlowLayoutPanel();
            menuPanel.Dock = DockStyle.Fill;
            menuPanel.FlowDirection = FlowDirection.TopDown;
            menuPanel.WrapContents = false;
            menuPanel.AutoScroll = true;
            menuPanel.Padding = new Padding(0, 15, 0, 0);
            menuPanel.BackColor = Color.Transparent;

            AddMenuItem(menuPanel, "🏠", "Dashboard", false, () =>
            {
                DonorDashboard dashboard = new DonorDashboard();
                this.Hide();
                dashboard.FormClosed += (s, args) => this.Close();
                dashboard.Show();
            });

            AddMenuItem(menuPanel, "📅", "Appointments", false, () =>
            {
                BookAppointment apptForm = new BookAppointment();
                this.Hide();
                apptForm.FormClosed += (s, args) => this.Show();
                apptForm.Show();
            });

            AddMenuItem(menuPanel, "📄", "Donation History", false, () =>
            {
                DonationHistory historyForm = new DonationHistory();
                this.Hide();
                historyForm.FormClosed += (s, args) => this.Show();
                historyForm.Show();
            });

            AddMenuItem(menuPanel, "📍", "Nearby Camps", false, () =>
            {
                NearbyCamps campsForm = new NearbyCamps();
                this.Hide();
                campsForm.FormClosed += (s, args) => this.Show();
                campsForm.Show();
            });

            AddMenuItem(menuPanel, "🔔", "Notifications", false, () =>
            {
                DonorNotifications notifForm = new DonorNotifications();
                notifForm.Owner = this;
                this.Hide();
                notifForm.FormClosed += (s, args) => this.Show();
                notifForm.Show();
            });

            AddMenuItem(menuPanel, "👤", "Profile Settings", true, () => { });

            Panel spacer = new Panel();
            spacer.Size = new Size(210, 1);
            spacer.Margin = new Padding(0, 20, 0, 0);
            spacer.BackColor = Color.Transparent;
            menuPanel.Controls.Add(spacer);

            AddMenuItem(menuPanel, "🚪", "Logout", false, () =>
            {
                if (MessageBox.Show("Are you sure you want to logout?", "Logout Confirmation",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    SessionManager.Logout();
                    Application.Exit();
                }
            });

            sidebarPanel.Controls.Add(menuPanel);
            sidebarPanel.Controls.Add(logoContainer);
            this.Controls.Add(sidebarPanel);
        }

        private void AddMenuItem(FlowLayoutPanel panel, string icon, string text, bool active, Action clickAction)
        {
            Panel item = new Panel();
            item.Size = new Size(210, 46);
            item.Margin = new Padding(0, 3, 0, 3);
            item.Cursor = Cursors.Hand;
            item.BackColor = active ? Color.FromArgb(50, Color.White) : Color.Transparent;

            Label iconLbl = new Label();
            iconLbl.Text = icon;
            iconLbl.Font = new Font("Segoe UI Emoji", 14);
            iconLbl.ForeColor = Color.White;
            iconLbl.BackColor = Color.Transparent;
            iconLbl.Size = new Size(30, 46);
            iconLbl.Location = new Point(10, 0);
            iconLbl.TextAlign = ContentAlignment.MiddleCenter;

            Label textLbl = new Label();
            textLbl.Text = text;
            textLbl.Font = new Font("Segoe UI Semibold", 11);
            textLbl.ForeColor = Color.White;
            textLbl.BackColor = Color.Transparent;
            textLbl.Size = new Size(155, 46);
            textLbl.Location = new Point(42, 0);
            textLbl.TextAlign = ContentAlignment.MiddleLeft;

            if (clickAction != null)
            {
                EventHandler click = (s, e) => clickAction();
                item.Click += click;
                iconLbl.Click += click;
                textLbl.Click += click;
            }

            item.Controls.Add(iconLbl);
            item.Controls.Add(textLbl);
            panel.Controls.Add(item);
        }

        private void BuildMainContent()
        {
            mainPanel = new Panel();
            mainPanel.Dock = DockStyle.Fill;
            mainPanel.BackColor = Color.FromArgb(245, 247, 250);
            mainPanel.Padding = new Padding(25);
            mainPanel.AutoScroll = true;
            this.Controls.Add(mainPanel);

            // Top Bar - heading (Dock.Top, added to mainPanel)
            Panel topBar = new Panel();
            topBar.Dock = DockStyle.Top;
            topBar.Height = 80;
            topBar.BackColor = Color.White;
            MakeRounded(topBar, 15);
            mainPanel.Controls.Add(topBar);

            Label lblWelcome = new Label();
            lblWelcome.Text = "👤  Profile Settings";
            lblWelcome.Font = new Font("Segoe UI", 20, FontStyle.Bold);
            lblWelcome.ForeColor = brickRed;
            lblWelcome.Location = new Point(20, 20);
            lblWelcome.AutoSize = true;
            topBar.Controls.Add(lblWelcome);

            // Scroll wrapper for card below heading
            Panel scrollWrapper = new Panel();
            scrollWrapper.Dock = DockStyle.Fill;
            scrollWrapper.AutoScroll = true;
            scrollWrapper.BackColor = Color.Transparent;
            mainPanel.Controls.Add(scrollWrapper);

            // Profile card
            Panel profileCard = new Panel();
            profileCard.BackColor = Color.White;
            profileCard.Location = new Point(15, 15);
            profileCard.Width = scrollWrapper.Width - 40;
            profileCard.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            MakeRounded(profileCard, 15);
            scrollWrapper.Controls.Add(profileCard);

            scrollWrapper.Resize += (s, e) =>
            {
                profileCard.Width = scrollWrapper.Width - 40;
            };

            // ══════════════════════════════════════════════════════════
            // VISUAL ORDER (top to bottom):
            //   1. Profile Picture + Change Photo   ← TOP
            //   2. Divider
            //   3. Form Fields
            //   4. Divider
            //   5. Save Button                      ← BOTTOM
            //
            // WinForms Dock.Top REVERSE RULE:
            // Last Controls.Add = renders at TOP
            // So add in this order:
            //   Add 1st → Save Button   (bottom)
            //   Add 2nd → Divider2
            //   Add 3rd → Fields Panel
            //   Add 4th → Divider1
            //   Add 5th → Picture Panel (top)  ← LAST = TOP
            // ══════════════════════════════════════════════════════════

            // ── ADD 1ST: Save Button Panel (renders at BOTTOM) ────────
            Panel buttonPanel = new Panel();
            buttonPanel.Dock = DockStyle.Top;
            buttonPanel.Height = 75;
            buttonPanel.BackColor = Color.Transparent;

            btnSave = new Button();
            btnSave.Text = "💾  Save Changes";
            btnSave.Size = new Size(190, 46);
            btnSave.BackColor = brickRed;
            btnSave.ForeColor = Color.White;
            btnSave.FlatStyle = FlatStyle.Flat;
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            btnSave.Cursor = Cursors.Hand;
            MakeRounded(btnSave, 10);
            btnSave.Click += BtnSave_Click;
            buttonPanel.Controls.Add(btnSave);

            void CenterSave()
            {
                btnSave.Location = new Point((buttonPanel.Width - 190) / 2, 14);
            }
            CenterSave();
            buttonPanel.Resize += (s, e) => CenterSave();

            profileCard.Controls.Add(buttonPanel);   // 1st added = BOTTOM

            // ── ADD 2ND: Divider below fields ─────────────────────────
            Panel divider2 = new Panel();
            divider2.Dock = DockStyle.Top;
            divider2.Height = 1;
            divider2.BackColor = Color.FromArgb(230, 230, 235);
            profileCard.Controls.Add(divider2);      // 2nd added

            // ── ADD 3RD: Fields Panel ──────────────────────────────────
            Panel fieldsPanel = new Panel();
            fieldsPanel.Dock = DockStyle.Top;
            fieldsPanel.Height = 430;
            fieldsPanel.Padding = new Padding(30, 20, 30, 10);
            fieldsPanel.BackColor = Color.Transparent;

            int labelWidth = 145;
            int fieldWidth = 250;
            int rowHeight = 58;

            // LEFT COLUMN
            int leftX = 20;
            int y = 15;

            CreateTextBoxField(fieldsPanel, "Full Name:", leftX, ref y, labelWidth, fieldWidth, out txtFullName);
            y += rowHeight;
            CreateTextBoxField(fieldsPanel, "Email:", leftX, ref y, labelWidth, fieldWidth, out txtEmail);
            y += rowHeight;
            CreateTextBoxField(fieldsPanel, "Phone:", leftX, ref y, labelWidth, fieldWidth, out txtPhone);
            y += rowHeight;
            CreateTextBoxField(fieldsPanel, "CNIC:", leftX, ref y, labelWidth, fieldWidth, out txtCNIC);
            y += rowHeight;
            CreateComboField(fieldsPanel, "Gender:", leftX, ref y, labelWidth, fieldWidth, out cmbGender,
                new string[] { "Male", "Female", "Other" });
            y += rowHeight;
            CreateDateField(fieldsPanel, "Date of Birth:", leftX, ref y, labelWidth, fieldWidth, out dtpDateOfBirth);
            dtpDateOfBirth.ValueChanged += (s, e) => CalculateAge();
            y += rowHeight;
            CreateLabelField(fieldsPanel, "Age:", leftX, ref y, labelWidth, fieldWidth, out lblAge);

            // RIGHT COLUMN
            int rightX = 450;
            y = 15;

            CreateComboField(fieldsPanel, "Blood Group:", rightX, ref y, labelWidth, fieldWidth, out cmbBloodGroup,
                new string[] { "A+", "A-", "B+", "B-", "AB+", "AB-", "O+", "O-" });
            y += rowHeight;
            CreateTextBoxField(fieldsPanel, "Weight (kg):", rightX, ref y, labelWidth, fieldWidth, out txtWeight);
            y += rowHeight;
            CreateTextBoxField(fieldsPanel, "Address:", rightX, ref y, labelWidth, fieldWidth + 60, out txtAddress);
            y += rowHeight;
            CreateTextBoxField(fieldsPanel, "City:", rightX, ref y, labelWidth, fieldWidth, out txtCity);
            y += rowHeight;
            CreateTextBoxField(fieldsPanel, "Emergency Contact:", rightX, ref y, labelWidth, fieldWidth, out txtEmergencyContact);

            profileCard.Controls.Add(fieldsPanel);   // 3rd added

            // ── ADD 4TH: Divider below picture ────────────────────────
            Panel divider1 = new Panel();
            divider1.Dock = DockStyle.Top;
            divider1.Height = 1;
            divider1.BackColor = Color.FromArgb(230, 230, 235);
            profileCard.Controls.Add(divider1);      // 4th added

            // ── ADD 5TH (LAST): Picture Panel (renders at TOP) ────────
            Panel picturePanel = new Panel();
            picturePanel.Dock = DockStyle.Top;
            picturePanel.Height = 155;
            picturePanel.BackColor = Color.FromArgb(252, 248, 248);

            profilePicture = new PictureBox();
            profilePicture.Size = new Size(100, 100);
            profilePicture.BackColor = Color.FromArgb(230, 230, 230);
            profilePicture.SizeMode = PictureBoxSizeMode.Zoom;
            profilePicture.Cursor = Cursors.Hand;
            MakeRounded(profilePicture, 50);
            profilePicture.Click += (s, e) => ChangeProfilePicture();
            picturePanel.Controls.Add(profilePicture);

            btnChangePicture = new Button();
            btnChangePicture.Text = "📷  Change Photo";
            btnChangePicture.Size = new Size(130, 30);
            btnChangePicture.BackColor = Color.FromArgb(240, 240, 240);
            btnChangePicture.FlatStyle = FlatStyle.Flat;
            btnChangePicture.Font = new Font("Segoe UI", 8, FontStyle.Bold);
            btnChangePicture.Cursor = Cursors.Hand;
            btnChangePicture.Click += (s, e) => ChangeProfilePicture();
            picturePanel.Controls.Add(btnChangePicture);

            void CenterPicture()
            {
                profilePicture.Location = new Point((picturePanel.Width - 100) / 2, 15);
                btnChangePicture.Location = new Point((picturePanel.Width - 130) / 2, 120);
            }
            CenterPicture();
            picturePanel.Resize += (s, e) => CenterPicture();

            profileCard.Controls.Add(picturePanel);  // 5th (LAST) added = TOP

            // Total card height
            profileCard.Height = picturePanel.Height + divider1.Height + fieldsPanel.Height
                                + divider2.Height + buttonPanel.Height + 10;
        }

        private void CreateTextBoxField(Panel parent, string labelText, int x, ref int y,
            int labelWidth, int fieldWidth, out TextBox textBox)
        {
            Label lbl = new Label();
            lbl.Text = labelText;
            lbl.Location = new Point(x, y + 6);
            lbl.Size = new Size(labelWidth, 24);
            lbl.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            lbl.ForeColor = Color.FromArgb(80, 80, 90);
            parent.Controls.Add(lbl);

            textBox = new TextBox();
            textBox.Location = new Point(x + labelWidth, y);
            textBox.Size = new Size(fieldWidth, 32);
            textBox.Font = new Font("Segoe UI", 10);
            textBox.BorderStyle = BorderStyle.FixedSingle;
            parent.Controls.Add(textBox);
        }

        private void CreateLabelField(Panel parent, string labelText, int x, ref int y,
            int labelWidth, int fieldWidth, out Label label)
        {
            Label lbl = new Label();
            lbl.Text = labelText;
            lbl.Location = new Point(x, y + 6);
            lbl.Size = new Size(labelWidth, 24);
            lbl.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            lbl.ForeColor = Color.FromArgb(80, 80, 90);
            parent.Controls.Add(lbl);

            label = new Label();
            label.Location = new Point(x + labelWidth, y + 4);
            label.Size = new Size(fieldWidth, 30);
            label.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            label.ForeColor = brickRed;
            label.Text = "";
            parent.Controls.Add(label);
        }

        private void CreateComboField(Panel parent, string labelText, int x, ref int y,
            int labelWidth, int fieldWidth, out ComboBox comboBox, string[] items)
        {
            Label lbl = new Label();
            lbl.Text = labelText;
            lbl.Location = new Point(x, y + 6);
            lbl.Size = new Size(labelWidth, 24);
            lbl.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            lbl.ForeColor = Color.FromArgb(80, 80, 90);
            parent.Controls.Add(lbl);

            comboBox = new ComboBox();
            comboBox.Location = new Point(x + labelWidth, y);
            comboBox.Size = new Size(fieldWidth, 32);
            comboBox.Font = new Font("Segoe UI", 10);
            comboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBox.Items.AddRange(items);
            if (items.Length > 0) comboBox.SelectedIndex = 0;
            parent.Controls.Add(comboBox);
        }

        private void CreateDateField(Panel parent, string labelText, int x, ref int y,
            int labelWidth, int fieldWidth, out DateTimePicker dtp)
        {
            Label lbl = new Label();
            lbl.Text = labelText;
            lbl.Location = new Point(x, y + 6);
            lbl.Size = new Size(labelWidth, 24);
            lbl.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            lbl.ForeColor = Color.FromArgb(80, 80, 90);
            parent.Controls.Add(lbl);

            dtp = new DateTimePicker();
            dtp.Location = new Point(x + labelWidth, y);
            dtp.Size = new Size(fieldWidth, 32);
            dtp.Font = new Font("Segoe UI", 10);
            dtp.Format = DateTimePickerFormat.Short;
            dtp.MaxDate = DateTime.Today.AddYears(-16);
            parent.Controls.Add(dtp);
        }

        private void LoadProfileData()
        {
            try
            {
                if (currentDonor != null)
                {
                    txtFullName.Text = currentDonor.FullName ?? "";
                    txtEmail.Text = currentDonor.Email ?? "";
                    txtPhone.Text = currentDonor.Phone ?? "";
                    txtCNIC.Text = currentDonor.CNIC ?? "";
                    txtWeight.Text = currentDonor.Weight > 0 ? currentDonor.Weight.ToString() : "";
                    txtAddress.Text = currentDonor.Address ?? "";
                    txtEmergencyContact.Text = currentDonor.EmergencyContact ?? "";

                    if (!string.IsNullOrEmpty(currentDonor.Address))
                    {
                        string[] parts = currentDonor.Address.Split(',');
                        txtCity.Text = parts.Length > 0 ? parts[parts.Length - 1].Trim() : "";
                    }

                    if (!string.IsNullOrEmpty(currentDonor.BloodGroup))
                    {
                        int index = cmbBloodGroup.Items.IndexOf(currentDonor.BloodGroup);
                        if (index >= 0) cmbBloodGroup.SelectedIndex = index;
                    }

                    if (!string.IsNullOrEmpty(currentDonor.Gender))
                    {
                        int index = cmbGender.Items.IndexOf(currentDonor.Gender);
                        if (index >= 0) cmbGender.SelectedIndex = index;
                    }

                    if (currentDonor.DateOfBirth > DateTime.MinValue)
                        dtpDateOfBirth.Value = currentDonor.DateOfBirth;

                    CalculateAge();
                }

                LoadProfilePicture();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading profile: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CalculateAge()
        {
            if (lblAge != null && dtpDateOfBirth != null)
            {
                int age = DateTimeHelper.CalculateAge(dtpDateOfBirth.Value);
                lblAge.Text = age + " years";
            }
        }

        private void LoadProfilePicture()
        {
            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();
                    string sql = "SELECT ProfilePicture FROM Donors WHERE UserID = @UserID";
                    var cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@UserID", currentUserID);
                    byte[] imageData = cmd.ExecuteScalar() as byte[];

                    if (imageData != null && imageData.Length > 0)
                    {
                        using (MemoryStream ms = new MemoryStream(imageData))
                            profilePicture.Image = Image.FromStream(ms);
                    }
                    else
                    {
                        profilePicture.Image = CreateDefaultAvatar();
                    }
                }
            }
            catch
            {
                profilePicture.Image = CreateDefaultAvatar();
            }
        }

        private Image CreateDefaultAvatar()
        {
            Bitmap bmp = new Bitmap(100, 100);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.Clear(Color.FromArgb(230, 230, 230));
                string initial = currentDonor?.FullName?.Length > 0
                    ? currentDonor.FullName[0].ToString().ToUpper() : "D";
                using (Font font = new Font("Segoe UI", 36, FontStyle.Bold))
                using (SolidBrush brush = new SolidBrush(brickRed))
                {
                    SizeF sz = g.MeasureString(initial, font);
                    g.DrawString(initial, font, brush, (100 - sz.Width) / 2, (100 - sz.Height) / 2);
                }
            }
            return bmp;
        }

        private void ChangeProfilePicture()
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif";
                ofd.Title = "Select Profile Picture";

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        Image original = Image.FromFile(ofd.FileName);
                        Image resized = ResizeImage(original, 200, 200);
                        profilePicture.Image = resized;

                        using (MemoryStream ms = new MemoryStream())
                        {
                            resized.Save(ms, ImageFormat.Jpeg);
                            profilePictureBytes = ms.ToArray();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error loading image: " + ex.Message, "Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private Image ResizeImage(Image image, int width, int height)
        {
            Bitmap resized = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(resized))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.DrawImage(image, 0, 0, width, height);
            }
            return resized;
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtFullName.Text))
                {
                    MessageBox.Show("Please enter full name.", "Validation",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtFullName.Focus();
                    return;
                }

                string fullAddress = txtAddress.Text.Trim();
                if (!string.IsNullOrEmpty(txtCity.Text))
                    fullAddress = string.IsNullOrEmpty(fullAddress)
                        ? txtCity.Text
                        : fullAddress + ", " + txtCity.Text;

                currentDonor.FullName = txtFullName.Text.Trim();
                currentDonor.Email = txtEmail.Text.Trim();
                currentDonor.Phone = txtPhone.Text.Trim();
                currentDonor.CNIC = txtCNIC.Text.Trim();
                currentDonor.BloodGroup = cmbBloodGroup.SelectedItem?.ToString();
                currentDonor.Gender = cmbGender.SelectedItem?.ToString();
                currentDonor.Address = fullAddress;
                currentDonor.Weight = string.IsNullOrEmpty(txtWeight.Text) ? 0 : Convert.ToInt32(txtWeight.Text);
                currentDonor.DateOfBirth = dtpDateOfBirth.Value;
                currentDonor.Age = DateTimeHelper.CalculateAge(dtpDateOfBirth.Value);
                currentDonor.EmergencyContact = txtEmergencyContact.Text.Trim();

                bool success = DonorDAL.UpdateDonor(currentDonor);

                if (success)
                {
                    if (profilePictureBytes != null && profilePictureBytes.Length > 0)
                        SaveProfilePictureToDatabase();

                    UpdateUserInfo();

                    MessageBox.Show("✅ Profile updated successfully!", "Success",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);

                    this.DialogResult = DialogResult.OK;
                }
                else
                {
                    MessageBox.Show("Failed to update profile. Please try again.", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error saving profile: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SaveProfilePictureToDatabase()
        {
            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();
                    string sql = "UPDATE Donors SET ProfilePicture = @Picture WHERE UserID = @UserID";
                    var cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@Picture", profilePictureBytes);
                    cmd.Parameters.AddWithValue("@UserID", currentUserID);
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error saving profile picture: " + ex.Message);
            }
        }

        private void UpdateUserInfo()
        {
            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();
                    string sql = "UPDATE Users SET FullName=@Name, Email=@Email, Phone=@Phone WHERE UserID=@UserID";
                    var cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@Name", txtFullName.Text.Trim());
                    cmd.Parameters.AddWithValue("@Email", txtEmail.Text.Trim());
                    cmd.Parameters.AddWithValue("@Phone", txtPhone.Text.Trim());
                    cmd.Parameters.AddWithValue("@UserID", currentUserID);
                    cmd.ExecuteNonQuery();
                    SessionManager.Login(AdminDAL.GetUserByID(currentUserID));
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error updating user info: " + ex.Message);
            }
        }

        private Image CreateBloodDropImage()
        {
            Bitmap bmp = new Bitmap(80, 80);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.Clear(Color.White);
                using (SolidBrush brush = new SolidBrush(brickRed))
                {
                    GraphicsPath path = new GraphicsPath();
                    path.AddEllipse(20, 30, 40, 40);
                    path.AddPolygon(new Point[] { new Point(40, 5), new Point(22, 40), new Point(58, 40) });
                    path.FillMode = FillMode.Winding;
                    g.FillPath(brush, path);
                }
            }
            return bmp;
        }

        private void MakeRounded(Control control, int radius)
        {
            control.Paint += (s, e) =>
            {
                GraphicsPath path = new GraphicsPath();
                int d = radius * 2;
                path.AddArc(0, 0, d, d, 180, 90);
                path.AddArc(control.Width - d, 0, d, d, 270, 90);
                path.AddArc(control.Width - d, control.Height - d, d, d, 0, 90);
                path.AddArc(0, control.Height - d, d, d, 90, 90);
                control.Region = new Region(path);
            };
        }
    }
}