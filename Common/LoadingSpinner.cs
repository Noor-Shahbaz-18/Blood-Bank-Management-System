using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Threading.Tasks;

namespace BloodBankManagementSystem.Forms.Common
{
    public partial class LoadingSpinner : Form
    {
        private int _angle = 0;
        private Timer _timer;
        private string _loadingText = "Loading...";

        public LoadingSpinner(string text = "Loading...")
        {
            _loadingText = text;
            InitializeComponent();
            BuildUI();
            StartSpinner();
        }

        private void BuildUI()
        {
            this.Text = "";
            this.Size = new Size(200, 200);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = Color.White;
            this.TopMost = true;
            this.ShowInTaskbar = false;

            this.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

                // Draw semi-transparent background
                using (SolidBrush brush = new SolidBrush(Color.FromArgb(240, 255, 255, 255)))
                {
                    e.Graphics.FillRectangle(brush, this.ClientRectangle);
                }

                // Draw spinner
                using (Pen pen = new Pen(Color.FromArgb(120, 22, 27), 6))
                {
                    pen.StartCap = LineCap.Round;
                    pen.EndCap = LineCap.Round;
                    int centerX = this.Width / 2;
                    int centerY = this.Height / 2 - 20;
                    int radius = 40;

                    for (int i = 0; i < 12; i++)
                    {
                        int angle = i * 30 + _angle;
                        double radians = angle * Math.PI / 180;
                        int x = (int)(centerX + radius * Math.Cos(radians));
                        int y = (int)(centerY + radius * Math.Sin(radians));

                        int alpha = 255 - (i * 20);
                        pen.Color = Color.FromArgb(Math.Max(50, alpha), 120, 22, 27);
                        e.Graphics.DrawLine(pen, centerX, centerY, x, y);
                    }
                }

                // Draw text
                using (Font font = new Font("Segoe UI", 12, FontStyle.Bold))
                using (SolidBrush brush = new SolidBrush(Color.FromArgb(80, 80, 90)))
                {
                    SizeF textSize = e.Graphics.MeasureString(_loadingText, font);
                    float textX = (this.Width - textSize.Width) / 2;
                    float textY = this.Height / 2 + 30;
                    e.Graphics.DrawString(_loadingText, font, brush, textX, textY);
                }
            };
        }

        private void StartSpinner()
        {
            _timer = new Timer();
            _timer.Interval = 50;
            _timer.Tick += (s, e) =>
            {
                _angle = (_angle + 15) % 360;
                this.Invalidate();
            };
            _timer.Start();
        }

        public static async Task ShowAsync(string text = "Loading...", int delayMs = 0)
        {
            using (var spinner = new LoadingSpinner(text))
            {
                var task = Task.Run(() => spinner.ShowDialog());
                if (delayMs > 0)
                    await Task.Delay(delayMs);
                spinner.Close();
                await task;
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _timer?.Stop();
            _timer?.Dispose();
            base.OnFormClosing(e);
        }
    }
}