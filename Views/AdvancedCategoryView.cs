using System;
using System.Drawing;
using System.Windows.Forms;

namespace Optimizer.Views
{
    public class AdvancedCategoryView : UserControl
    {
        public event EventHandler BackRequested;

        public AdvancedCategoryView(string icon, string title, string description, string[] plannedFeatures)
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.FromArgb(15, 15, 15);

            Button btnBack = new Button
            {
                Text = "← Back",
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(45, 45, 45),
                ForeColor = Color.White,
                Size = new Size(100, 35),
                Location = new Point(30, 20),
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            btnBack.FlatAppearance.BorderSize = 0;
            btnBack.Click += (s, e) => BackRequested?.Invoke(this, EventArgs.Empty);
            btnBack.MouseEnter += (s, e) => { btnBack.BackColor = Color.FromArgb(55, 55, 55); };
            btnBack.MouseLeave += (s, e) => { btnBack.BackColor = Color.FromArgb(45, 45, 45); };

            Label lblIcon = new Label
            {
                Text = icon,
                Font = new Font("Segoe UI Emoji", 32),
                Location = new Point(25, 75),
                AutoSize = true,
                BackColor = Color.Transparent,
                ForeColor = Color.White
            };

            Label lblTitle = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 24, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(80, 80),
                AutoSize = true
            };

            Label lblDescription = new Label
            {
                Text = description,
                Font = new Font("Segoe UI", 12),
                ForeColor = Color.LightGray,
                Location = new Point(30, 140),
                Size = new Size(700, 40),
                AutoEllipsis = true
            };

            Panel pnlWarning = new Panel
            {
                BackColor = Color.FromArgb(40, 35, 10),
                Size = new Size(700, 40),
                Location = new Point(30, 190)
            };
            pnlWarning.Paint += (s, e) =>
            {
                Control p = (Control)s;
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                using (var pen = new Pen(Color.FromArgb(120, 100, 30), 1))
                {
                    e.Graphics.DrawRectangle(pen, 0, 0, p.Width - 1, p.Height - 1);
                }
            };
            Label lblWarning = new Label
            {
                Text = "⚠️ This module will be migrated in a later step. No tweaks are active yet.",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(241, 196, 15),
                Location = new Point(10, 10),
                AutoSize = true
            };
            pnlWarning.Controls.Add(lblWarning);

            Label lblPlannedTitle = new Label
            {
                Text = "Planned Features for Migration:",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(30, 250),
                AutoSize = true
            };

            Panel pnlFeatures = new Panel
            {
                Location = new Point(30, 290),
                Size = new Size(700, 300),
                AutoScroll = true
            };

            int currentY = 0;
            foreach (var feature in plannedFeatures)
            {
                Label lblFeature = new Label
                {
                    Text = "• " + feature,
                    Font = new Font("Segoe UI", 11),
                    ForeColor = Color.LightGray,
                    Location = new Point(0, currentY),
                    AutoSize = true
                };
                pnlFeatures.Controls.Add(lblFeature);
                currentY += 30;
            }

            this.Controls.Add(btnBack);
            this.Controls.Add(lblIcon);
            this.Controls.Add(lblTitle);
            this.Controls.Add(lblDescription);
            this.Controls.Add(pnlWarning);
            this.Controls.Add(lblPlannedTitle);
            this.Controls.Add(pnlFeatures);
        }
    }
}
