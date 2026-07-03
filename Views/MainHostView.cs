using System;
using System.Drawing;
using System.Windows.Forms;
using Optimizer.Domain.Models;
using Optimizer.Domain.Engines;
using Optimizer.Localization;

namespace Optimizer.Views
{
    public class MainHostView : UserControl
    {
        private Panel sidebarPanel;
        private Panel contentPanel;
        private DashboardView dashboardView;
        private ProfileSelectionView profileView;
        private QuickCleanupView quickCleanupView;
        private UsageAssessmentView usageAssessmentView;
        private AdvancedHostView advancedHostView;
        private SettingsView settingsView;
        private UserControl currentView;

        private Panel titleBarPanel;
        private Button btnClose;
        private Button btnMinimize;

        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        public MainHostView()
        {
            this.BackColor = Color.FromArgb(15, 15, 15);
            this.Padding = new Padding(0);
            this.ForeColor = Color.White;
            this.Dock = DockStyle.Fill;
            this.DoubleBuffered = true;

            dashboardView = new DashboardView() { Dock = DockStyle.Fill };
            profileView = new ProfileSelectionView() { Dock = DockStyle.Fill };
            quickCleanupView = new QuickCleanupView() { Dock = DockStyle.Fill };
            usageAssessmentView = new UsageAssessmentView() { Dock = DockStyle.Fill };
            advancedHostView = new AdvancedHostView() { Dock = DockStyle.Fill };
            settingsView = new SettingsView() { Dock = DockStyle.Fill };

            dashboardView.UsageAssessmentRequested += () => ShowView(usageAssessmentView);
            usageAssessmentView.BackRequested += () => ShowView(dashboardView);

            profileView.ProfileSelected += async (profile) => {
                var rpResult = await System.Threading.Tasks.Task.Run(() => ServiceLocator.SmartProfileBackupService.CreateRestorePoint(profile.Title));
                string rpStatus = "Created";
                
                if (!rpResult.Success || !rpResult.Data)
                {
                    rpStatus = (rpResult.ErrorMessage != null && rpResult.ErrorMessage.Contains("not enabled")) ? "Not available" : "Failed";
                    var result = MessageBox.Show(
                        "A Windows Restore Point could not be created. ApexBoost will still create a Registry Backup before applying changes. Continue?", 
                        "Warning", 
                        MessageBoxButtons.OKCancel, 
                        MessageBoxIcon.Warning, 
                        MessageBoxDefaultButton.Button2);

                    if (result == DialogResult.Cancel)
                    {
                        return;
                    }
                }

                var progressView = new ProgressView();
                contentPanel.Controls.Add(progressView);
                progressView.Dock = DockStyle.Fill;
                progressView.BringToFront();
                
                var progress = new Progress<OptimizationStatus>(status => progressView.UpdateProgress(status));
                bool success = await ServiceLocator.ProfileOptimizationService.ApplyProfileAsync(profile, progress, rpStatus);
                
                if (success)
                {
                    contentPanel.Controls.Remove(progressView);
                    progressView.Dispose();

                    var resultView = new ResultView();
                    resultView.FinishRequested += () => 
                    {
                        contentPanel.Controls.Remove(resultView);
                        resultView.Dispose();
                        ShowView(dashboardView);
                    };
                    contentPanel.Controls.Add(resultView);
                    resultView.Dock = DockStyle.Fill;
                    resultView.BringToFront();
                }
            };

            titleBarPanel = new Panel();
            titleBarPanel.Dock = DockStyle.Top;
            titleBarPanel.Height = 35;
            titleBarPanel.BackColor = Color.FromArgb(15, 15, 15);
            titleBarPanel.MouseDown += TitleBar_MouseDown;

            btnClose = new Button();
            btnClose.Text = "×";
            btnClose.Dock = DockStyle.Right;
            btnClose.Width = 45;
            btnClose.FlatStyle = FlatStyle.Flat;
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.ForeColor = Color.Gray;
            btnClose.Cursor = Cursors.Hand;
            btnClose.Click += (s, e) => Application.Exit();
            btnClose.MouseEnter += (s, e) => { btnClose.BackColor = Color.FromArgb(232, 17, 35); btnClose.ForeColor = Color.White; };
            btnClose.MouseLeave += (s, e) => { btnClose.BackColor = Color.FromArgb(15, 15, 15); btnClose.ForeColor = Color.Gray; };

            btnMinimize = new Button();
            btnMinimize.Text = "—";
            btnMinimize.Dock = DockStyle.Right;
            btnMinimize.Width = 45;
            btnMinimize.FlatStyle = FlatStyle.Flat;
            btnMinimize.FlatAppearance.BorderSize = 0;
            btnMinimize.ForeColor = Color.Gray;
            btnMinimize.Cursor = Cursors.Hand;
            btnMinimize.Click += (s, e) => this.FindForm().WindowState = FormWindowState.Minimized;
            btnMinimize.MouseEnter += (s, e) => btnMinimize.BackColor = Color.FromArgb(40, 40, 40);
            btnMinimize.MouseLeave += (s, e) => btnMinimize.BackColor = Color.FromArgb(15, 15, 15);

            titleBarPanel.Controls.Add(btnMinimize);
            titleBarPanel.Controls.Add(btnClose);

            sidebarPanel = new Panel();
            sidebarPanel.Dock = DockStyle.Left;
            sidebarPanel.Width = 200;
            sidebarPanel.BackColor = Color.FromArgb(15, 15, 15);

            contentPanel = new Panel();
            contentPanel.Dock = DockStyle.Fill;
            contentPanel.BackColor = Color.FromArgb(20, 20, 20);

            Label lblLogo = new Label();
            lblLogo.Text = "ApexBoost";
            lblLogo.Font = new Font("Segoe UI", 16, FontStyle.Bold);
            lblLogo.ForeColor = Color.MediumSlateBlue;
            lblLogo.Dock = DockStyle.Top;
            lblLogo.Height = 60;
            lblLogo.TextAlign = ContentAlignment.MiddleCenter;

            Button btnDashboard = CreateSidebarButton("Dashboard", L.Get("nav.dashboard"), "\U0001F3E0");
            btnDashboard.Click += (s, e) => ShowView(dashboardView);

            Button btnProfiles = CreateSidebarButton("Profiles", L.Get("nav.smartProfiles"), "\u26A1");
            btnProfiles.Click += (s, e) => ShowView(profileView);

            Button btnCleanup = CreateSidebarButton("Cleanup", L.Get("nav.quickCleanup"), "\U0001F9F9");
            btnCleanup.Click += (s, e) => ShowView(quickCleanupView);

            Button btnAdvanced = CreateSidebarButton("Advanced", L.Get("nav.advanced"), "\U0001F6E0");
            btnAdvanced.Click += (s, e) => ShowView(advancedHostView);

            Button btnSettings = CreateSidebarButton("Settings", L.Get("nav.settings"), "\u2699");
            btnSettings.Click += (s, e) => ShowView(settingsView);

            // Adding in reverse order for DockStyle.Top so they render top-to-bottom
            sidebarPanel.Controls.Add(btnSettings);
            sidebarPanel.Controls.Add(btnAdvanced);
            sidebarPanel.Controls.Add(btnCleanup);
            sidebarPanel.Controls.Add(btnProfiles);
            sidebarPanel.Controls.Add(btnDashboard);
            sidebarPanel.Controls.Add(lblLogo);

            this.Controls.Add(contentPanel);
            this.Controls.Add(sidebarPanel);
            this.Controls.Add(titleBarPanel);
            
            titleBarPanel.Visible = true; 
            
            // Fix z-order for docking (highest index claims edge first)
            sidebarPanel.SendToBack();
            titleBarPanel.SendToBack();
            contentPanel.BringToFront();

            // Populate content views
            contentPanel.Controls.Add(dashboardView);
            contentPanel.Controls.Add(profileView);
            contentPanel.Controls.Add(quickCleanupView);
            contentPanel.Controls.Add(usageAssessmentView);
            contentPanel.Controls.Add(advancedHostView);
            contentPanel.Controls.Add(settingsView);

            // Hide them all initially
            foreach (Control c in contentPanel.Controls)
            {
                c.Visible = false;
            }

            ShowView(dashboardView);
        }

        private void TitleBar_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(this.FindForm().Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }

        private Button CreateSidebarButton(string id, string name, string icon)
        {
            Button btn = new Button();
            btn.Text = $" {icon}   {name}";
            btn.Tag = id;
            btn.Dock = DockStyle.Top;
            btn.Height = 50;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.Font = new Font("Segoe UI", 12, FontStyle.Regular);
            btn.ForeColor = Color.FromArgb(160, 160, 160);
            btn.TextAlign = ContentAlignment.MiddleLeft;
            btn.Padding = new Padding(15, 0, 0, 0);
            btn.Cursor = Cursors.Hand;
            btn.AutoEllipsis = true;

            btn.MouseEnter += (s, e) => 
            {
                if (!IsActiveView(id))
                {
                    btn.BackColor = Color.FromArgb(25, 25, 25);
                    btn.ForeColor = Color.White;
                }
            };
            btn.MouseLeave += (s, e) => 
            {
                if (!IsActiveView(id))
                {
                    btn.BackColor = Color.FromArgb(15, 15, 15);
                    btn.ForeColor = Color.FromArgb(160, 160, 160);
                }
            };

            btn.Paint += (s, ev) => 
            {
                var b = s as Button;
                if (IsActiveView(id))
                {
                    using (SolidBrush brush = new SolidBrush(Color.FromArgb(94, 92, 230)))
                    {
                        ev.Graphics.FillRectangle(brush, 0, 0, 4, b.Height);
                    }
                }
            };

            return btn;
        }

        private bool IsActiveView(string id)
        {
            if (currentView == null) return false;
            string expectedName = "";
            switch (id)
            {
                case "Dashboard": expectedName = "DashboardView"; break;
                case "Profiles": expectedName = "ProfileSelectionView"; break;
                case "Cleanup": expectedName = "QuickCleanupView"; break;
                case "Advanced": expectedName = "AdvancedHostView"; break;
                case "Settings": expectedName = "SettingsView"; break;
            }
            return currentView.GetType().Name == expectedName;
        }

        private void ShowView(UserControl view)
        {
            if (currentView == view) return;

            if (currentView != null)
                currentView.Visible = false;

            currentView = view;
            currentView.Visible = true;
            currentView.BringToFront();

            if (view == dashboardView)
            {
                dashboardView.LoadDataAsync();
            }
            else if (view == profileView)
            {
                profileView.LoadProfiles();
            }
            else if (view == usageAssessmentView)
            {
                usageAssessmentView.LoadDataAsync();
            }

            UpdateButtonStates();
        }

        public void NavigateToUsageAssessment()
        {
            ShowView(usageAssessmentView);
        }

        private void UpdateButtonStates()
        {
            foreach (Control c in sidebarPanel.Controls)
            {
                if (c is Button btn)
                {
                    string id = btn.Tag.ToString();
                    string expectedName = "";
                    switch (id)
                    {
                        case "Dashboard": expectedName = "DashboardView"; break;
                        case "Profiles": expectedName = "ProfileSelectionView"; break;
                        case "Cleanup": expectedName = "QuickCleanupView"; break;
                        case "Advanced": expectedName = "AdvancedHostView"; break;
                        case "Settings": expectedName = "SettingsView"; break;
                    }

                    if (currentView != null && currentView.GetType().Name == expectedName)
                    {
                        btn.BackColor = Color.FromArgb(30, 30, 30);
                        btn.ForeColor = Color.White;
                        btn.Font = new Font("Segoe UI", 12, FontStyle.Bold);
                    }
                    else
                    {
                        btn.BackColor = Color.FromArgb(15, 15, 15);
                        btn.ForeColor = Color.FromArgb(160, 160, 160);
                        btn.Font = new Font("Segoe UI", 12, FontStyle.Regular);
                    }
                    btn.Invalidate(); // trigger paint for sidebar accent
                }
            }
        }
    }
}

