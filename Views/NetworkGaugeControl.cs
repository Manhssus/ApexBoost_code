using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Optimizer.Localization;

namespace Optimizer.Views
{
    public class NetworkGaugeControl : Control
    {
        public bool IsConnected { get; set; } = false;
        public int IssueCount { get; set; } = 0;

        public NetworkGaugeControl()
        {
            this.DoubleBuffered = true;
            this.BackColor = Color.FromArgb(20, 20, 20); // Deep dark
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            // Border
            using (var borderPen = new Pen(Color.FromArgb(45, 45, 45), 1))
            {
                g.DrawRectangle(borderPen, 0, 0, this.Width - 1, this.Height - 1);
            }

            int ringThickness = 12;
            int diameter = 96;
            int centerX = this.Width / 2;
            
            int ringTop = 16;
            int centerY = ringTop + (diameter / 2);

            Rectangle rect = new Rectangle(centerX - (diameter / 2), ringTop, diameter, diameter);

            // Background track
            using (var trackPen = new Pen(Color.FromArgb(35, 35, 35), ringThickness))
            {
                g.DrawEllipse(trackPen, rect);
            }

            // Active arc and center text
            Color accentColor = IsConnected && IssueCount == 0 ? Color.FromArgb(48, 209, 88) : Color.IndianRed;
            string centerText = IssueCount.ToString();
            string mainLabel = IsConnected && IssueCount == 0 ? L.Get("common.status.connected") : L.Get("network.issuesFound");
            string subLabel = L.Get("network.internetHealth");

            if (IsConnected && IssueCount == 0)
            {
                using (var arcPen = new Pen(accentColor, ringThickness) { StartCap = LineCap.Round, EndCap = LineCap.Round })
                {
                    g.DrawArc(arcPen, rect, -90, 360);
                }
            }
            else
            {
                using (var arcPen = new Pen(accentColor, ringThickness) { StartCap = LineCap.Round, EndCap = LineCap.Round })
                {
                    g.DrawArc(arcPen, rect, -90, 180);
                }
            }

            // Draw center number
            using (var centerFont = new Font("Segoe UI", 24, FontStyle.Bold))
            using (var centerBrush = new SolidBrush(accentColor))
            using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
            {
                g.DrawString(centerText, centerFont, centerBrush, rect, sf);
            }

            // Draw labels below ring
            int labelY = ringTop + diameter + 10;
            
            using (var mainFont = new Font("Segoe UI", 10, FontStyle.Regular))
            using (var mainBrush = new SolidBrush(Color.White))
            using (var sf = new StringFormat { Alignment = StringAlignment.Center })
            {
                g.DrawString(mainLabel, mainFont, mainBrush, new Point(centerX, labelY), sf);
            }

            if (!string.IsNullOrEmpty(subLabel))
            {
                using (var subFont = new Font("Segoe UI", 8, FontStyle.Bold))
                using (var subBrush = new SolidBrush(accentColor))
                using (var sf = new StringFormat { Alignment = StringAlignment.Center })
                {
                    g.DrawString(subLabel, subFont, subBrush, new Point(centerX, labelY + 20), sf);
                }
            }
        }
    }
}
