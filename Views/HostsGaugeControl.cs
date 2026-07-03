using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Optimizer.Localization;

namespace Optimizer.Views
{
    public class HostsGaugeControl : Control
    {
        public int TotalEntries { get; set; } = 0;
        public int EnabledEntries { get; set; } = 0;

        public HostsGaugeControl()
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
            Color accentColor = Color.FromArgb(48, 209, 88); // Green
            string centerText = "-";
            string mainLabel = L.Get("common.status.notScanned");
            string subLabel = "";

            if (TotalEntries > 0)
            {
                centerText = TotalEntries.ToString();
                
                float ratio = (float)EnabledEntries / TotalEntries;
                if (ratio == 0 && TotalEntries > 0) ratio = 0.01f; // Just to show some arc if there are only disabled entries
                float sweepAngle = 360f * ratio;
                
                accentColor = Color.FromArgb(48, 209, 88); // Green for healthy
                mainLabel = L.Get("common.status.healthy");
                subLabel = L.Get("hosts.hostsFile");
                
                using (var arcPen = new Pen(accentColor, ringThickness) { StartCap = LineCap.Round, EndCap = LineCap.Round })
                {
                    g.DrawArc(arcPen, rect, -90, sweepAngle);
                }
            }
            else if (TotalEntries == 0)
            {
                centerText = "0";
                accentColor = Color.DarkGray;
                mainLabel = L.Get("hosts.empty");
                subLabel = L.Get("hosts.noCustomEntries");
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
