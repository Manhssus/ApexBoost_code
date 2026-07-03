using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Optimizer.Domain.Models;

namespace Optimizer.Views
{
    public class HostsEntryRowControl : UserControl
    {
        public HostsFileEntry Entry { get; private set; }

        private Label lblIP;
        private Label lblDomain;
        private Label lblComment;
        private Button btnToggle;
        private Button btnDelete;

        public event EventHandler ToggleRequested;
        public event EventHandler DeleteRequested;

        private bool _isHovered;

        public HostsEntryRowControl(HostsFileEntry entry)
        {
            this.Entry = entry;
            this.Size = new Size(700, 50);
            this.Margin = new Padding(0, 0, 0, 5);
            this.BackColor = Color.FromArgb(30, 30, 30);
            this.DoubleBuffered = true;

            this.MouseEnter += (s, e) => { _isHovered = true; this.Invalidate(); };
            this.MouseLeave += (s, e) => { _isHovered = false; this.Invalidate(); };

            InitializeComponents();
        }

        private void InitializeComponents()
        {
            lblIP = new Label
            {
                Text = Entry.IPAddress,
                Font = new Font("Segoe UI Semibold", 10),
                ForeColor = Entry.IsEnabled ? Color.White : Color.Gray,
                Location = new Point(15, 15),
                AutoSize = true
            };

            lblDomain = new Label
            {
                Text = Entry.Domain,
                Font = new Font("Segoe UI", 10),
                ForeColor = Entry.IsEnabled ? Color.LightSkyBlue : Color.Gray,
                Location = new Point(180, 15),
                AutoSize = true
            };

            lblComment = new Label
            {
                Text = Entry.Comment ?? "",
                Font = new Font("Segoe UI", 9, FontStyle.Italic),
                ForeColor = Color.FromArgb(120, 120, 120),
                Location = new Point(400, 16),
                AutoSize = false,
                Size = new Size(130, 20),
                AutoEllipsis = true
            };

            btnToggle = CreateActionButton(Entry.IsEnabled ? "Disable" : "Enable", 540);
            btnToggle.Click += (s, e) => ToggleRequested?.Invoke(this, EventArgs.Empty);

            btnDelete = CreateActionButton("Delete", 620);
            btnDelete.ForeColor = Color.IndianRed;
            btnDelete.Click += (s, e) => DeleteRequested?.Invoke(this, EventArgs.Empty);

            this.Controls.Add(lblIP);
            this.Controls.Add(lblDomain);
            this.Controls.Add(lblComment);
            this.Controls.Add(btnToggle);
            this.Controls.Add(btnDelete);
        }

        private Button CreateActionButton(string text, int x)
        {
            Button btn = new Button
            {
                Text = text,
                Location = new Point(x, 10),
                Size = new Size(70, 30),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(30, 30, 30),
                ForeColor = Color.LightGray,
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", 9)
            };
            btn.FlatAppearance.BorderSize = 1;
            btn.FlatAppearance.BorderColor = Color.FromArgb(60, 60, 60);
            btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(50, 50, 50);
            return btn;
        }

        public void SetWritable(bool isWritable)
        {
            btnToggle.Enabled = isWritable;
            btnDelete.Enabled = isWritable;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (_isHovered)
            {
                using (var pen = new Pen(Color.FromArgb(148, 146, 230), 1))
                {
                    e.Graphics.DrawRectangle(pen, 0, 0, this.Width - 1, this.Height - 1);
                }
            }
        }
    }
}
