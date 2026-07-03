using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Collections.Generic;
using Optimizer.Localization;

namespace Optimizer.Views
{
    public class AdvancedHostView : UserControl
    {
        private Panel _contentPanel;
        private Panel _hubPanel;
        private Panel _headerPanel;
        private Panel _clippingContainer;
        private Panel _scrollArea;
        private FlowLayoutPanel _flpCards;
        private Panel _pnlWarning;
        private Label _lblWarning;
        private ModernScrollBar _customScrollBar;

        public AdvancedHostView()
        {
            this.BackColor = Color.FromArgb(15, 15, 15);
            this.Dock = DockStyle.Fill;
            this.DoubleBuffered = true;

            _contentPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent
            };
            this.Controls.Add(_contentPanel);

            InitializeHub();

            this.Resize += AdvancedHostView_Resize;
        }

        private void InitializeHub()
        {
            _hubPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent
            };

            // Header Panel containing Title, Subtitle, Warning Banner
            _headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 175,
                BackColor = Color.Transparent
            };
            _hubPanel.Controls.Add(_headerPanel);

            Label title = new Label
            {
                Text = L.Get("advanced.hub.title"),
                Font = new Font("Segoe UI", 24, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(24, 20)
            };
            _headerPanel.Controls.Add(title);

            Label subtitle = new Label
            {
                Text = L.Get("advanced.hub.subtitle"),
                Font = new Font("Segoe UI", 11),
                ForeColor = Color.FromArgb(176, 176, 176),
                AutoSize = true,
                Location = new Point(28, 68)
            };
            _headerPanel.Controls.Add(subtitle);

            _pnlWarning = new Panel
            {
                Location = new Point(24, 112),
                Height = 40,
                BackColor = Color.FromArgb(40, 35, 10)
            };
            _pnlWarning.Paint += (s, e) =>
            {
                Control p = (Control)s;
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var pen = new Pen(Color.FromArgb(120, 100, 30), 1))
                {
                    e.Graphics.DrawRectangle(pen, 0, 0, p.Width - 1, p.Height - 1);
                }
            };

            _lblWarning = new Label
            {
                Text = L.Get("advanced.hub.warning"),
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.FromArgb(241, 196, 15),
                AutoSize = true
            };
            _pnlWarning.Controls.Add(_lblWarning);
            _headerPanel.Controls.Add(_pnlWarning);

            // Clipping Container to hide default WinForms Scrollbar
            _clippingContainer = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent
            };
            _hubPanel.Controls.Add(_clippingContainer);

            _scrollArea = new Panel
            {
                AutoScroll = true,
                BackColor = Color.Transparent
            };
            _clippingContainer.Controls.Add(_scrollArea);

            _flpCards = new FlowLayoutPanel
            {
                Location = new Point(24, 5),
                AutoScroll = false,
                BackColor = Color.Transparent,
                Margin = new Padding(0),
                Padding = new Padding(0)
            };

            AddCategoryCards(_flpCards);
            _scrollArea.Controls.Add(_flpCards);

            // Reusable Custom Scrollbar
            _customScrollBar = new ModernScrollBar(_scrollArea)
            {
                Width = 8,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right,
                Location = new Point(_clippingContainer.Width - 10, 0),
                Height = _clippingContainer.Height
            };
            _clippingContainer.Controls.Add(_customScrollBar);
            _customScrollBar.BringToFront();

            _clippingContainer.BringToFront(); // Fix docking order so it fills remaining space below header

            _contentPanel.Controls.Add(_hubPanel);
        }

        private void AdvancedHostView_Resize(object sender, EventArgs e)
        {
            if (_clippingContainer == null || _scrollArea == null || _flpCards == null || _pnlWarning == null) return;

            int paddingX = 24;
            int availableWidth = _clippingContainer.Width - paddingX * 2 - 12;

            if (availableWidth < 320) availableWidth = 320;

            int columns = 2;
            if (availableWidth >= 1080)
                columns = 3;
            else if (availableWidth < 664)
                columns = 1;

            int gap = 24;
            int cardWidth = (availableWidth - (gap * (columns - 1))) / columns;
            
            // clamping
            if (cardWidth < 320) cardWidth = 320;
            if (cardWidth > 520) cardWidth = 520;

            int gridActualWidth = columns * cardWidth + (columns - 1) * gap;

            // Align warning banner in HeaderPanel
            _pnlWarning.Width = gridActualWidth;
            if (_lblWarning != null)
            {
                _lblWarning.Location = new Point(12, (_pnlWarning.Height - _lblWarning.Height) / 2);
            }

            // Adjust scrollArea to push native scrollbar outside clipping container
            _scrollArea.Location = new Point(0, 0);
            _scrollArea.Size = new Size(_clippingContainer.Width + 25, _clippingContainer.Height);

            // Update width and horizontal position without disrupting AutoScroll Y-offset
            _flpCards.Left = paddingX;
            _flpCards.Width = gridActualWidth;

            // Recalculate margins for perfect grid alignment with no right-edge extra gap
            for (int i = 0; i < _flpCards.Controls.Count; i++)
            {
                Control c = _flpCards.Controls[i];
                if (c is AdvancedCategoryCard card)
                {
                    card.Width = cardWidth;
                    card.Height = 165; // Compact premium height
                    
                    int colIndex = i % columns;
                    int marginRight = (colIndex == columns - 1) ? 0 : gap;
                    card.Margin = new Padding(0, 0, marginRight, gap);
                }
            }

            // Estimate total height needed for flow layout
            int rows = (int)Math.Ceiling((double)_flpCards.Controls.Count / columns);
            int requiredHeight = rows * (165 + gap) + 20;
            _flpCards.Height = requiredHeight;

            // Position and update Custom Scrollbar
            _customScrollBar.Location = new Point(_clippingContainer.Width - 10, 0);
            _customScrollBar.Height = _clippingContainer.Height;
            _customScrollBar.UpdateScrollParams();
        }

        private void AddCategoryCards(FlowLayoutPanel panel)
        {
            var categories = new List<CategoryDef>
            {
                new CategoryDef("sys_opt", "\u2699", L.Get("advanced.system.title"), L.Get("advanced.system.subtitle"), L.Get("common.risk.medium"), L.Get("advanced.hub.status.ready"), new[] { "Enable Performance Tweaks", "Disable Superfetch", "Disable Game Bar", "Disable Xbox Live", "Disable Windows Ink" }),
                new CategoryDef("privacy", "\U0001F6E1", L.Get("advanced.privacy.title"), L.Get("advanced.privacy.subtitle"), L.Get("common.risk.review"), L.Get("advanced.hub.status.ready"), new[] { "Disable Telemetry Tasks", "Disable Telemetry Services", "Disable Cortana", "Disable Start Menu Ads", "Disable Quick Access History" }),
                new CategoryDef("network", "\U0001F310", L.Get("advanced.network.title"), L.Get("advanced.network.subtitle"), L.Get("common.risk.review"), L.Get("advanced.hub.status.ready"), new[] { "Disable Network Throttling", "Disable HomeGroup", "Disable Media Player Sharing" }),
                new CategoryDef("services", "\U0001F9E9", L.Get("advanced.services.title"), L.Get("advanced.services.subtitle"), L.Get("common.risk.medium"), L.Get("advanced.hub.status.review"), new[] { "Disable Print Service", "Disable Fax Service", "Disable Insider Service" }),
                new CategoryDef("hosts", "\U0001F4C4", L.Get("advanced.hosts.title"), L.Get("advanced.hosts.subtitle"), L.Get("common.risk.review"), L.Get("advanced.hub.status.recommended"), new[] { "Locate Hosts File", "Add Custom Entry", "Remove Custom Entry", "Restore Default Hosts" }),
                new CategoryDef("apps", "\U0001F4E6", L.Get("advanced.apps.title"), L.Get("advanced.apps.subtitle"), L.Get("common.risk.medium"), L.Get("advanced.hub.status.review"), new[] { "Uninstall OneDrive", "List UWP Apps", "Remove UWP App" }),
                new CategoryDef("utils", "\U0001F6E0", L.Get("advanced.utilities.title"), L.Get("advanced.utilities.subtitle"), L.Get("common.risk.review"), L.Get("advanced.hub.status.planned"), new[] { "File Unlocker", "Hardware Assessment" })
            };

            foreach (var cat in categories)
            {
                var card = new AdvancedCategoryCard
                {
                    IconText = cat.Icon,
                    Title = cat.Title,
                    Description = cat.Description,
                    RiskLevel = cat.Risk,
                    Status = cat.Status
                };

                card.CardClicked += (s, e) => 
                {
                    if (cat.Id == "hosts")
                    {
                        ShowHostsManagerView();
                    }
                    else if (cat.Id == "sys_opt")
                    {
                        ShowSystemOptimizationView();
                    }
                    else if (cat.Id == "network")
                    {
                        ShowNetworkToolsView();
                    }
                    else if (cat.Id == "privacy")
                    {
                        ShowPrivacyProtectionView();
                    }
                    else if (cat.Id == "services")
                    {
                        ShowWindowsServicesReviewView();
                    }
                    else if (cat.Id == "apps")
                    {
                        ShowAppRemovalReviewView();
                    }
                    else if (cat.Id == "utils")
                    {
                        ShowAdvancedUtilitiesReviewView();
                    }
                    else
                    {
                        ShowCategoryView(cat.Icon, cat.Title, cat.Description, cat.Features);
                    }
                };

                panel.Controls.Add(card);
            }
        }

        private void ClearContentPanel()
        {
            for (int i = _contentPanel.Controls.Count - 1; i >= 0; i--)
            {
                var c = _contentPanel.Controls[i];
                _contentPanel.Controls.RemoveAt(i);
                if (c != _hubPanel)
                {
                    c.Dispose();
                }
            }
        }

        private void ShowHostsManagerView()
        {
            ClearContentPanel();
            var hostsView = new HostsManagerView();
            hostsView.BackRequested += (s, e) => 
            {
                ClearContentPanel();
                _contentPanel.Controls.Add(_hubPanel);
                _hubPanel.BringToFront();
                _customScrollBar?.UpdateScrollParams();
            };
            hostsView.LoadData();
            _contentPanel.Controls.Add(hostsView);
            hostsView.BringToFront();
            
            // For custom view, we might not need the custom scrollbar on the hub wrapper, but keeping consistency.
            _customScrollBar?.UpdateScrollParams();
        }

        private void ShowSystemOptimizationView()
        {
            ClearContentPanel();
            var sysOptView = new SystemOptimizationView();
            sysOptView.BackRequested += (s, e) => 
            {
                ClearContentPanel();
                _contentPanel.Controls.Add(_hubPanel);
                _hubPanel.BringToFront();
                _customScrollBar?.UpdateScrollParams();
            };
            var _ = sysOptView.LoadDataAsync();
            _contentPanel.Controls.Add(sysOptView);
            sysOptView.BringToFront();
            
            _customScrollBar?.UpdateScrollParams();
        }

        private void ShowNetworkToolsView()
        {
            ClearContentPanel();
            var networkView = new NetworkToolsView();
            networkView.BackRequested += (s, e) => 
            {
                ClearContentPanel();
                _contentPanel.Controls.Add(_hubPanel);
                _hubPanel.BringToFront();
                _customScrollBar?.UpdateScrollParams();
            };
            _ = networkView.LoadDataAsync();
            _contentPanel.Controls.Add(networkView);
            networkView.BringToFront();
            
            _customScrollBar?.UpdateScrollParams();
        }

        private void ShowPrivacyProtectionView()
        {
            ClearContentPanel();
            var privacyView = new PrivacyProtectionView();
            privacyView.BackRequested += (s, e) => 
            {
                ClearContentPanel();
                _contentPanel.Controls.Add(_hubPanel);
                _hubPanel.BringToFront();
                _customScrollBar?.UpdateScrollParams();
            };
            var _ = privacyView.LoadDataAsync();
            _contentPanel.Controls.Add(privacyView);
            privacyView.BringToFront();
            
            _customScrollBar?.UpdateScrollParams();
        }

        private void ShowWindowsServicesReviewView()
        {
            ClearContentPanel();
            var servicesView = new WindowsServicesReviewView();
            servicesView.BackRequested += (s, e) => 
            {
                ClearContentPanel();
                _contentPanel.Controls.Add(_hubPanel);
                _hubPanel.BringToFront();
                _customScrollBar?.UpdateScrollParams();
            };
            var _ = servicesView.LoadDataAsync();
            _contentPanel.Controls.Add(servicesView);
            servicesView.BringToFront();
            
            _customScrollBar?.UpdateScrollParams();
        }

        private void ShowAppRemovalReviewView()
        {
            ClearContentPanel();
            var appView = new AppRemovalReviewView();
            appView.BackRequested += (s, e) => 
            {
                ClearContentPanel();
                _contentPanel.Controls.Add(_hubPanel);
                _hubPanel.BringToFront();
                _customScrollBar?.UpdateScrollParams();
            };
            var _ = appView.LoadDataAsync();
            _contentPanel.Controls.Add(appView);
            appView.BringToFront();
            
            _customScrollBar?.UpdateScrollParams();
        }

        private void ShowAdvancedUtilitiesReviewView()
        {
            ClearContentPanel();
            var utilitiesView = new AdvancedUtilitiesReviewView();
            utilitiesView.BackRequested += (s, e) => 
            {
                ClearContentPanel();
                _contentPanel.Controls.Add(_hubPanel);
                _hubPanel.BringToFront();
                _customScrollBar?.UpdateScrollParams();
            };
            var _ = utilitiesView.LoadDataAsync();
            _contentPanel.Controls.Add(utilitiesView);
            utilitiesView.BringToFront();
            
            _customScrollBar?.UpdateScrollParams();
        }

        private void ShowCategoryView(string icon, string title, string description, string[] features)
        {
            ClearContentPanel();
            var categoryView = new AdvancedCategoryView(icon, title, description, features);
            categoryView.BackRequested += (s, e) => 
            {
                ClearContentPanel();
                _contentPanel.Controls.Add(_hubPanel);
            };
            _contentPanel.Controls.Add(categoryView);
        }

        private class CategoryDef
        {
            public string Id;
            public string Icon;
            public string Title;
            public string Description;
            public string Risk;
            public string Status;
            public string[] Features;

            public CategoryDef(string id, string icon, string t, string d, string r, string s, string[] f)
            {
                Id = id; Icon = icon; Title = t; Description = d; Risk = r; Status = s; Features = f;
            }
        }
    }

    public class ModernScrollBar : Control
    {
        private int _thumbHeight = 50;
        private int _thumbY = 0;
        private bool _isDragging = false;
        private int _dragStartMouseY = 0;
        private int _dragStartThumbY = 0;
        private bool _isHovered = false;

        private Panel _targetPanel;

        public ModernScrollBar(Panel targetPanel)
        {
            _targetPanel = targetPanel;
            this.Width = 8;
            this.DoubleBuffered = true;
            this.SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            this.BackColor = Color.Transparent;
            
            _targetPanel.Scroll += TargetPanel_Scroll;
            _targetPanel.MouseWheel += TargetPanel_MouseWheel;
            _targetPanel.Resize += (s, e) => UpdateScrollParams();

            this.MouseDown += ModernScrollBar_MouseDown;
            this.MouseMove += ModernScrollBar_MouseMove;
            this.MouseUp += ModernScrollBar_MouseUp;
            this.MouseEnter += (s, e) => { _isHovered = true; this.Invalidate(); };
            this.MouseLeave += (s, e) => { _isHovered = false; this.Invalidate(); };
        }

        private void TargetPanel_Scroll(object sender, ScrollEventArgs e)
        {
            UpdateThumbPositionFromTarget();
        }

        private void TargetPanel_MouseWheel(object sender, MouseEventArgs e)
        {
            UpdateThumbPositionFromTarget();
        }

        public void UpdateScrollParams()
        {
            int viewHeight = _targetPanel.ClientSize.Height;
            int totalHeight = 0;
            foreach (Control c in _targetPanel.Controls)
            {
                int bottom = c.Top + c.Height;
                if (bottom > totalHeight) totalHeight = bottom;
            }

            if (totalHeight <= viewHeight || viewHeight <= 0)
            {
                this.Visible = false;
                return;
            }

            this.Visible = true;
            float ratio = (float)viewHeight / totalHeight;
            _thumbHeight = (int)(this.Height * ratio);
            if (_thumbHeight < 30) _thumbHeight = 30;

            UpdateThumbPositionFromTarget();
        }

        private void UpdateThumbPositionFromTarget()
        {
            int viewHeight = _targetPanel.ClientSize.Height;
            int totalHeight = 0;
            foreach (Control c in _targetPanel.Controls)
            {
                int bottom = c.Top + c.Height;
                if (bottom > totalHeight) totalHeight = bottom;
            }

            int scrollY = -_targetPanel.AutoScrollPosition.Y;
            int maxScroll = totalHeight - viewHeight;
            if (maxScroll <= 0) return;

            float scrollPercent = (float)scrollY / maxScroll;
            int maxThumbY = this.Height - _thumbHeight;
            _thumbY = (int)(maxThumbY * scrollPercent);

            this.Invalidate();
        }

        private void ModernScrollBar_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (e.Y >= _thumbY && e.Y <= _thumbY + _thumbHeight)
                {
                    _isDragging = true;
                    _dragStartMouseY = Cursor.Position.Y;
                    _dragStartThumbY = _thumbY;
                }
                else
                {
                    int targetY = e.Y - _thumbHeight / 2;
                    ScrollToThumbY(targetY);
                }
            }
        }

        private void ModernScrollBar_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isDragging)
            {
                int deltaY = Cursor.Position.Y - _dragStartMouseY;
                int newThumbY = _dragStartThumbY + deltaY;
                ScrollToThumbY(newThumbY);
            }
        }

        private void ModernScrollBar_MouseUp(object sender, MouseEventArgs e)
        {
            _isDragging = false;
        }

        private void ScrollToThumbY(int thumbY)
        {
            int maxThumbY = this.Height - _thumbHeight;
            if (maxThumbY <= 0) return;

            if (thumbY < 0) thumbY = 0;
            if (thumbY > maxThumbY) thumbY = maxThumbY;

            _thumbY = thumbY;
            float scrollPercent = (float)_thumbY / maxThumbY;

            int viewHeight = _targetPanel.ClientSize.Height;
            int totalHeight = 0;
            foreach (Control c in _targetPanel.Controls)
            {
                int bottom = c.Top + c.Height;
                if (bottom > totalHeight) totalHeight = bottom;
            }

            int maxScroll = totalHeight - viewHeight;
            int targetScrollY = (int)(maxScroll * scrollPercent);

            // Force scroll
            _targetPanel.AutoScrollPosition = new Point(0, targetScrollY);
            this.Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            // Draw modern rounded thumb
            Color thumbColor = _isHovered ? Color.FromArgb(90, 90, 90) : Color.FromArgb(74, 74, 74);
            using (SolidBrush brush = new SolidBrush(thumbColor))
            {
                int radius = 3;
                Rectangle rect = new Rectangle(0, _thumbY, this.Width, _thumbHeight);
                using (GraphicsPath path = new GraphicsPath())
                {
                    path.AddArc(rect.X, rect.Y, radius * 2, radius * 2, 180, 90);
                    path.AddArc(rect.Right - radius * 2, rect.Y, radius * 2, radius * 2, 270, 90);
                    path.AddArc(rect.Right - radius * 2, rect.Bottom - radius * 2, radius * 2, radius * 2, 0, 90);
                    path.AddArc(rect.X, rect.Bottom - radius * 2, radius * 2, radius * 2, 90, 90);
                    path.CloseFigure();
                    e.Graphics.FillPath(brush, path);
                }
            }
        }
    }
}

