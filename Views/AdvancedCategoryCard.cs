using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Optimizer.Localization;

namespace Optimizer.Views
{
    public class AdvancedCategoryCard : UserControl
    {
        private Color _normalColor = Color.FromArgb(30, 30, 30);
        private Color _hoverColor = Color.FromArgb(36, 32, 46); // premium subtle purple tint
        private Color _borderColor = Color.FromArgb(58, 58, 58);
        private Color _hoverBorderColor = Color.FromArgb(123, 92, 255); // ApexBoost purple
        private bool _isHovered = false;

        public event EventHandler CardClicked;

        private Label _lblTitle;
        private Label _lblDescription;
        private Label _lblRiskBadge;
        private Label _lblStatus;
        private Button _btnOpen;
        private Label _lblIcon;
        private Panel _divider;

        private string _riskLevel;

        public string Title 
        { 
            get => _lblTitle.Text; 
            set => _lblTitle.Text = value; 
        }

        public string Description
        {
            get => _lblDescription.Text;
            set => _lblDescription.Text = value;
        }

        public string RiskLevel
        {
            get => _riskLevel;
            set
            {
                _riskLevel = value;
                UpdateRiskBadge(value);
            }
        }

        public string Status
        {
            get => _lblStatus.Text;
            set => _lblStatus.Text = $"{L.Get("common.statusPrefix")} {value}";
        }

        public string IconText
        {
            get => _lblIcon.Text;
            set => _lblIcon.Text = value;
        }

        public AdvancedCategoryCard()
        {
            this.Size = new Size(340, 165); // Refined compact height
            this.BackColor = Color.Transparent;
            this.Cursor = Cursors.Hand;
            this.Margin = new Padding(0);
            this.DoubleBuffered = true;
            
            _lblIcon = new Label
            {
                Size = new Size(32, 32),
                Location = new Point(18, 15),
                BackColor = Color.Transparent,
                ForeColor = Color.FromArgb(176, 176, 176),
                Font = new Font("Segoe UI Emoji", 18),
                TextAlign = ContentAlignment.MiddleCenter
            };

            _lblTitle = new Label
            {
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(56, 14),
                AutoSize = true,
                BackColor = Color.Transparent
            };

            _lblRiskBadge = new Label
            {
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                Location = new Point(56, 36),
                AutoSize = true,
                BackColor = Color.Transparent
            };

            _lblDescription = new Label
            {
                Font = new Font("Segoe UI", 9.5f),
                ForeColor = Color.LightGray,
                Location = new Point(18, 62),
                Size = new Size(304, 40),
                BackColor = Color.Transparent,
                AutoEllipsis = true
            };

            _divider = new Panel
            {
                Height = 1,
                BackColor = Color.FromArgb(45, 45, 45),
                Location = new Point(18, 112),
                Width = 304
            };

            _lblStatus = new Label
            {
                Font = new Font("Segoe UI", 8.5f),
                ForeColor = Color.FromArgb(160, 160, 160),
                Location = new Point(18, 128),
                AutoSize = true,
                BackColor = Color.Transparent
            };

            _btnOpen = new Button
            {
                Text = L.Get("common.open") + " →",
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(94, 92, 230),
                ForeColor = Color.White,
                Size = new Size(100, 34),
                Location = new Point(222, 120),
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };
            _btnOpen.FlatAppearance.BorderSize = 0;
            _btnOpen.MouseEnter += (s, e) => { _btnOpen.BackColor = Color.FromArgb(114, 112, 250); };
            _btnOpen.MouseLeave += (s, e) => { _btnOpen.BackColor = Color.FromArgb(94, 92, 230); };
            _btnOpen.Click += (s, e) => CardClicked?.Invoke(this, EventArgs.Empty);

            this.Controls.Add(_lblIcon);
            this.Controls.Add(_lblTitle);
            this.Controls.Add(_lblRiskBadge);
            this.Controls.Add(_lblDescription);
            this.Controls.Add(_divider);
            this.Controls.Add(_lblStatus);
            this.Controls.Add(_btnOpen);

            this.Resize += AdvancedCategoryCard_Resize;
            AttachHoverEvents(this);
        }

        private void AdvancedCategoryCard_Resize(object sender, EventArgs e)
        {
            if (_lblDescription != null)
                _lblDescription.Width = this.Width - 36;
            if (_divider != null)
            {
                _divider.Location = new Point(18, this.Height - 50);
                _divider.Width = this.Width - 36;
            }
            if (_lblStatus != null)
                _lblStatus.Location = new Point(18, this.Height - 37);
            if (_btnOpen != null)
                _btnOpen.Location = new Point(this.Width - 18 - _btnOpen.Width, this.Height - 44);
        }

        private void UpdateRiskBadge(string risk)
        {
            if (string.IsNullOrEmpty(risk)) return;
            string r = risk.ToLower();
            if (r.Contains("safe") || r.Contains("an toàn"))
            {
                _lblRiskBadge.ForeColor = Color.FromArgb(46, 204, 113);
                _lblRiskBadge.Text = "\u25CF " + L.Get("common.risk.safe");
            }
            else if (r.Contains("review") || r.Contains("cần xem xét"))
            {
                _lblRiskBadge.ForeColor = Color.FromArgb(241, 196, 15);
                _lblRiskBadge.Text = "\u25CF " + L.Get("common.risk.review");
            }
            else if (r.Contains("advanced") || r.Contains("dangerous") || r.Contains("nâng cao"))
            {
                _lblRiskBadge.ForeColor = Color.FromArgb(231, 76, 60);
                _lblRiskBadge.Text = "\u25CF " + L.Get("common.risk.advanced");
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            int radius = 10;
            Rectangle rect = new Rectangle(0, 0, this.Width - 1, this.Height - 1);
            using (GraphicsPath path = new GraphicsPath())
            {
                path.AddArc(rect.X, rect.Y, radius, radius, 180, 90);
                path.AddArc(rect.Right - radius, rect.Y, radius, radius, 270, 90);
                path.AddArc(rect.Right - radius, rect.Bottom - radius, radius, radius, 0, 90);
                path.AddArc(rect.X, rect.Bottom - radius, radius, radius, 90, 90);
                path.CloseFigure();

                Color bgColor = _isHovered ? _hoverColor : _normalColor;
                using (SolidBrush brush = new SolidBrush(bgColor))
                {
                    e.Graphics.FillPath(brush, path);
                }

                using (Pen pen = new Pen(_isHovered ? _hoverBorderColor : _borderColor, 1))
                {
                    e.Graphics.DrawPath(pen, path);
                }
            }
        }

        private void AttachHoverEvents(Control parent)
        {
            parent.MouseEnter += Card_MouseEnter;
            parent.MouseLeave += Card_MouseLeave;
            parent.MouseClick += (s, e) => CardClicked?.Invoke(this, EventArgs.Empty);

            foreach (Control c in parent.Controls)
            {
                if (c == _btnOpen) continue; 
                AttachHoverEvents(c);
            }
        }

        private void Card_MouseEnter(object sender, EventArgs e)
        {
            if (!_isHovered)
            {
                _isHovered = true;
                _lblIcon.ForeColor = Color.White;
                this.Invalidate();
            }
        }

        private void Card_MouseLeave(object sender, EventArgs e)
        {
            Point clientPoint = this.PointToClient(Cursor.Position);
            if (!this.ClientRectangle.Contains(clientPoint))
            {
                _isHovered = false;
                _lblIcon.ForeColor = Color.FromArgb(176, 176, 176);
                this.Invalidate();
            }
        }
    }
}
