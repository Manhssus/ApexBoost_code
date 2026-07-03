using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Optimizer.Domain.Models;
using Optimizer.Forms;
using Optimizer.Localization;

namespace Optimizer.Views
{
    public class ProfileSelectionView : UserControl
    {
        private Panel _headerPanel;
        private Label _lblTitle;
        private Label _lblSubtitle;
        private Panel _warningBanner;
        private Label _lblWarningText;

        // Health Dashboard
        private TableLayoutPanel _dashboardPanel;
        private SmartProfileGaugeControl _gaugeControl;

        // Stats Grid
        private TableLayoutPanel _statsGrid;
        private Label _lblAvailableProfiles;
        private Label _lblRecommended;
        private Label _lblLastPreview;
        private Label _lblSafetyMode;

        // List
        private Panel _clippingContainer;
        private Panel _scrollArea;
        private FlowLayoutPanel _flpItems;
        private ModernScrollBar _customScrollBar;

        public event Action<OptimizationProfile> ProfileSelected;

        public ProfileSelectionView()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.FromArgb(20, 20, 20);
            this.DoubleBuffered = true;
            this.Font = new Font("Segoe UI", 10F, FontStyle.Regular);

            InitializeComponents();
        }

        private void InitializeComponents()
        {
            // 1. Header
            _headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 100,
                BackColor = Color.FromArgb(20, 20, 20)
            };

            _lblTitle = new Label
            {
                Text = L.Get("nav.smartProfiles"),
                Location = new Point(40, 16),
                AutoSize = true,
                Font = new Font("Segoe UI Semibold", 22F, FontStyle.Bold),
                ForeColor = Color.White
            };
            _headerPanel.Controls.Add(_lblTitle);

            _lblSubtitle = new Label
            {
                Text = L.Get("profiles.subtitle"),
                Location = new Point(45, 56),
                AutoSize = true,
                Font = new Font("Segoe UI", 11F),
                ForeColor = Color.DarkGray
            };
            _headerPanel.Controls.Add(_lblSubtitle);

            // 2. Warning Banner
            Panel warningContainer = new Panel { Dock = DockStyle.Top, Height = 45, BackColor = Color.Transparent };
            _warningBanner = new Panel
            {
                Location = new Point(40, 0),
                Height = 32,
                BackColor = Color.FromArgb(10, 40, 50)
            };
            _lblWarningText = new Label
            {
                Text = L.Get("profiles.previewWarning"),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(10, 0, 0, 0),
                ForeColor = Color.LightSkyBlue,
                Font = new Font("Segoe UI Semibold", 9F)
            };
            _warningBanner.Controls.Add(_lblWarningText);
            warningContainer.Controls.Add(_warningBanner);

            // 3. Visual Dashboard
            Panel dashboardContainer = new Panel { Dock = DockStyle.Top, Height = 180, BackColor = Color.Transparent };
            _dashboardPanel = new TableLayoutPanel
            {
                Location = new Point(40, 0),
                Height = 160,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = Color.Transparent
            };
            _dashboardPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35F));
            _dashboardPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 65F));

            _gaugeControl = new SmartProfileGaugeControl
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 0, 8, 0)
            };
            _dashboardPanel.Controls.Add(_gaugeControl, 0, 0);

            _statsGrid = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(8, 0, 0, 0),
                RowCount = 2,
                ColumnCount = 2,
                BackColor = Color.Transparent
            };
            _statsGrid.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            _statsGrid.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            _statsGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            _statsGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));

            _lblAvailableProfiles = new Label { Text = "-", Font = new Font("Segoe UI Semibold", 12F), ForeColor = Color.White, AutoSize = true, Location = new Point(15, 25) };
            _lblRecommended = new Label { Text = "Gaming", Font = new Font("Segoe UI Semibold", 11F), ForeColor = Color.LightSkyBlue, AutoSize = true, Location = new Point(15, 25) };
            _lblLastPreview = new Label { Text = L.Get("common.status.never"), Font = new Font("Segoe UI Semibold", 11F), ForeColor = Color.LightGray, AutoSize = true, Location = new Point(15, 25) };
            _lblSafetyMode = new Label { Text = L.Get("profiles.enforced"), Font = new Font("Segoe UI Semibold", 11F), ForeColor = Color.FromArgb(48, 209, 88), AutoSize = true, Location = new Point(15, 25) };

            _statsGrid.Controls.Add(CreateCompactStatCard(L.Get("profiles.available"), _lblAvailableProfiles, false), 0, 0);
            _statsGrid.Controls.Add(CreateCompactStatCard(L.Get("profiles.recommended"), _lblRecommended, false), 1, 0);
            _statsGrid.Controls.Add(CreateCompactStatCard(L.Get("profiles.lastPreview"), _lblLastPreview, true), 0, 1);
            _statsGrid.Controls.Add(CreateCompactStatCard(L.Get("profiles.safetyMode"), _lblSafetyMode, true), 1, 1);

            _dashboardPanel.Controls.Add(_statsGrid, 1, 0);
            dashboardContainer.Controls.Add(_dashboardPanel);

            // 4. Main Content Area (Clipping Container)
            _clippingContainer = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent
            };

            _scrollArea = new Panel
            {
                AutoScroll = true,
                BackColor = Color.Transparent
            };
            _clippingContainer.Controls.Add(_scrollArea);

            _flpItems = new FlowLayoutPanel
            {
                Location = new Point(40, 20),
                AutoScroll = false,
                BackColor = Color.Transparent,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoSize = true,
                Padding = new Padding(0, 0, 0, 30)
            };
            _scrollArea.Controls.Add(_flpItems);

            _customScrollBar = new ModernScrollBar(_scrollArea)
            {
                Width = 8,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right
            };
            _clippingContainer.Controls.Add(_customScrollBar);
            _customScrollBar.BringToFront();

            this.Controls.Add(_clippingContainer);
            this.Controls.Add(dashboardContainer);
            this.Controls.Add(warningContainer);
            this.Controls.Add(_headerPanel);

            this.Resize += ProfileSelectionView_Resize;
        }

        private Panel CreateCompactStatCard(string title, Label valueLabel, bool isBottomRow)
        {
            Panel card = new Panel
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 0, 8, isBottomRow ? 0 : 8),
                BackColor = Color.FromArgb(30, 30, 30)
            };

            Label lblTitle = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.DarkGray,
                Location = new Point(15, 8),
                AutoSize = true
            };
            card.Controls.Add(lblTitle);
            card.Controls.Add(valueLabel);

            return card;
        }

        private void ProfileSelectionView_Resize(object sender, EventArgs e)
        {
            if (_clippingContainer == null) return;
            
            int safeWidth = this.Width - 80;
            if (safeWidth < 500) safeWidth = 500;

            _warningBanner.Width = safeWidth;
            _dashboardPanel.Width = safeWidth;

            _scrollArea.Location = new Point(0, 0);
            _scrollArea.Size = new Size(_clippingContainer.Width + 25, _clippingContainer.Height);
            
            _customScrollBar.Location = new Point(_clippingContainer.Width - 10, 0);
            _customScrollBar.Height = _clippingContainer.Height;

            int panelWidth = safeWidth;
            foreach (Control c in _flpItems.Controls)
            {
                c.Width = panelWidth;
                foreach (Control sub in c.Controls)
                {
                    if (sub.Name == "ActionsPanel")
                    {
                        sub.Location = new Point(panelWidth - sub.Width - 10, 15);
                    }
                }
            }

            _customScrollBar.UpdateScrollParams();
        }

        public void LoadProfiles()
        {
            ClearAndDisposeControls(_flpItems);

            var profiles = ServiceLocator.ProfileOptimizationService?.GetAvailableProfiles();
            if (profiles == null) return;

            _lblAvailableProfiles.Text = profiles.Count.ToString();
            _gaugeControl.TotalProfiles = profiles.Count;
            _gaugeControl.IsReady = true;
            _gaugeControl.Invalidate();

            foreach (var profile in profiles)
            {
                var card = CreateProfileCard(profile);
                _flpItems.Controls.Add(card);
            }

            ProfileSelectionView_Resize(this, EventArgs.Empty);
        }

        private Panel CreateProfileCard(OptimizationProfile profile)
        {
            Panel card = new Panel
            {
                Height = 85,
                BackColor = Color.FromArgb(30, 30, 30),
                Margin = new Padding(0, 0, 0, 10),
                Padding = new Padding(0)
            };

            card.Paint += (s, e) =>
            {
                ControlPaint.DrawBorder(e.Graphics, card.ClientRectangle, Color.FromArgb(50, 50, 50), ButtonBorderStyle.Solid);
            };

            // Accent strip
            Color accentColor = Color.FromArgb(48, 209, 88); // Default Green
            string iconStr = "⚙️";
            Color iconColor = Color.White;

            if (profile.Type == ProfileType.Gaming) 
            {
                accentColor = Color.FromArgb(255, 69, 58); // Red
                iconStr = "🎮";
                iconColor = accentColor;
            }
            else if (profile.Type == ProfileType.Office) 
            {
                accentColor = Color.FromArgb(10, 132, 255); // Blue
                iconStr = "💼";
                iconColor = accentColor;
            }
            else if (profile.Type == ProfileType.Privacy)
            {
                accentColor = Color.FromArgb(191, 90, 242); // Purple
                iconStr = "🛡️";
                iconColor = accentColor;
            }

            Panel leftAccent = new Panel
            {
                Dock = DockStyle.Left,
                Width = 4,
                BackColor = accentColor
            };
            card.Controls.Add(leftAccent);

            Label lblIcon = new Label
            {
                Text = iconStr,
                Font = new Font("Segoe UI", 16F),
                AutoSize = true,
                Location = new Point(15, 25),
                BackColor = Color.Transparent
            };
            card.Controls.Add(lblIcon);

            Label lblName = new Label
            {
                Text = Optimizer.Localization.LanguageManager.GetString(profile.Title, profile.Title),
                Font = new Font("Segoe UI Semibold", 13F),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(55, 15)
            };
            card.Controls.Add(lblName);

            Label lblDesc = new Label
            {
                Text = Optimizer.Localization.LanguageManager.GetString(profile.Description, profile.Description),
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.DarkGray,
                AutoSize = true,
                Location = new Point(55, 45)
            };
            card.Controls.Add(lblDesc);

            // Actions Panel (Right aligned)
            Panel actionsPanel = new Panel
            {
                Name = "ActionsPanel",
                Height = 45,
                Width = 360,
                BackColor = Color.Transparent
            };

            string rawRisk = profile.RiskLevel ?? "";
            string riskKey = "safe";
            if (rawRisk.IndexOf("Low-to-Medium", StringComparison.OrdinalIgnoreCase) >= 0) riskKey = "lowToMedium";
            else if (rawRisk.IndexOf("Zero", StringComparison.OrdinalIgnoreCase) >= 0) riskKey = "zero";
            else if (rawRisk.IndexOf("Medium", StringComparison.OrdinalIgnoreCase) >= 0) riskKey = "medium";
            else if (rawRisk.IndexOf("Safe", StringComparison.OrdinalIgnoreCase) >= 0) riskKey = "safe";
            else if (rawRisk.IndexOf("Review", StringComparison.OrdinalIgnoreCase) >= 0) riskKey = "review";
            else riskKey = rawRisk.ToLower().Replace(" risk", "").Replace("-", "");

            Label lblRisk = new Label
            {
                Text = $"{L.Get("profiles.riskPrefix")} {L.Get($"common.risk.{riskKey}")}",
                Font = new Font("Segoe UI", 8F, FontStyle.Bold),
                ForeColor = Color.Orange,
                BackColor = Color.FromArgb(40, 40, 20),
                AutoSize = true,
                Location = new Point(0, 15),
                Padding = new Padding(4)
            };
            actionsPanel.Controls.Add(lblRisk);

            Button btnPreview = new Button
            {
                Text = L.Get("profiles.previewApply"),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                BackColor = Color.FromArgb(148, 146, 230),
                Size = new Size(130, 32),
                Location = new Point(190, 8),
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", 9F)
            };
            btnPreview.FlatAppearance.BorderSize = 0;
            actionsPanel.Controls.Add(btnPreview);

            Button btnDetails = new Button
            {
                Text = "▼",
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.LightGray,
                BackColor = Color.FromArgb(40, 40, 40),
                Size = new Size(30, 32),
                Location = new Point(325, 8),
                Cursor = Cursors.Hand
            };
            btnDetails.FlatAppearance.BorderSize = 0;
            actionsPanel.Controls.Add(btnDetails);
            
            // Expanded details logic
            Panel detailsPanel = CreateDetailsPanel(profile);
            detailsPanel.Visible = false;
            card.Controls.Add(detailsPanel);

            btnDetails.Click += (s, e) =>
            {
                detailsPanel.Visible = !detailsPanel.Visible;
                btnDetails.Text = detailsPanel.Visible ? "▲" : "▼";
                card.Height = detailsPanel.Visible ? 85 + detailsPanel.Height : 85;
            };

            // Re-bind ProfileSelected event safely through Preview step
            btnPreview.Click += async (s, e) =>
            {
                btnPreview.Enabled = false;
                try
                {
                    _lblLastPreview.Text = profile.Type.ToString();
                    var p = await ServiceLocator.ProfileOptimizationService.GetProfileWithActionsAsync(profile.Type);
                    if (p != null)
                    {
                        using (var dlg = new ProfilePreviewDialog(p))
                        {
                            if (dlg.ShowDialog() == DialogResult.OK)
                            {
                                ProfileSelected?.Invoke(p);
                            }
                        }
                    }
                }
                finally
                {
                    btnPreview.Enabled = true;
                }
            };

            card.Controls.Add(actionsPanel);
            
            return card;
        }

        private Panel CreateDetailsPanel(OptimizationProfile profile)
        {
            Panel details = new Panel
            {
                BackColor = Color.FromArgb(25, 25, 25),
                Location = new Point(4, 85),
                Width = 1000, 
                Height = 120,
                Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right
            };

            Label lblOutcome = new Label
            {
                Text = $"{L.Get("profiles.expectedOutcome")} {profile.ExpectedOutcome}",
                Font = new Font("Segoe UI Semibold", 9F),
                ForeColor = Color.LightSkyBlue,
                Location = new Point(50, 15),
                AutoSize = true
            };
            details.Controls.Add(lblOutcome);

            Label lblNote = new Label
            {
                Text = L.Get("profiles.safetyNote"),
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.IndianRed,
                Location = new Point(50, 45),
                AutoSize = true
            };
            details.Controls.Add(lblNote);

            Label lblActions = new Label
            {
                Text = L.Get("profiles.includedActions") + (profile.Actions?.Count > 0 ? profile.Actions.Count.ToString() + L.Get("profiles.modificationsQueued") : L.Get("profiles.fetchedDuringPreview")),
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.DarkGray,
                Location = new Point(50, 75),
                AutoSize = true
            };
            details.Controls.Add(lblActions);

            return details;
        }

        private void ClearAndDisposeControls(Control container)
        {
            for (int i = container.Controls.Count - 1; i >= 0; i--)
            {
                var c = container.Controls[i];
                container.Controls.RemoveAt(i);
                c.Dispose();
            }
        }
    }
}


