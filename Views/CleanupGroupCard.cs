using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Optimizer.Domain.Models;

namespace Optimizer.Views
{
    public partial class CleanupGroupCard : UserControl
    {
        public CleanupGroup Group { get; private set; }
        
        public event EventHandler SelectionChanged;

        private bool _isExpanded = false;
        private int _headerHeight = 65;
        private int _expandedHeight = 65; // Will be calculated
        private Timer _animationTimer;

        private Color _bgColor = Color.FromArgb(26, 26, 26); // #1A1A1A
        private Color _borderColor = Color.FromArgb(48, 48, 48); // #303030
        private Color _hoverColor = Color.FromArgb(35, 35, 35); // #232323

        private List<CheckBox> _childCheckboxes = new List<CheckBox>();
        private bool _isUpdatingCheckboxes = false;

        public CleanupGroupCard(CleanupGroup group)
        {
            InitializeComponent();
            Group = group;
            
            DoubleBuffered = true;
            BackColor = _bgColor;
            Padding = new Padding(1); // For border effect if needed
            
            _animationTimer = new Timer { Interval = 15 };
            _animationTimer.Tick += AnimationTimer_Tick;

            BuildUI();
        }

        private void BuildUI()
        {
            // Header Panel
            pnlHeader.Height = _headerHeight;
            pnlHeader.BackColor = _bgColor;
            pnlHeader.Cursor = Cursors.Hand;
            pnlHeader.MouseEnter += (s, e) => { pnlHeader.BackColor = _hoverColor; BackColor = _hoverColor; };
            pnlHeader.MouseLeave += (s, e) => { pnlHeader.BackColor = _bgColor; BackColor = _bgColor; };
            pnlHeader.Click += ToggleExpand;

            chkGroup.Text = "";
            chkGroup.Checked = true;
            chkGroup.CheckedChanged += ChkGroup_CheckedChanged;
            
            lblIcon.Text = Group.GroupIcon;
            lblName.Text = Group.GroupName;
            lblDesc.Text = Group.GroupDescription;
            lblSize.Text = Group.FormattedTotalSize;

            foreach (Control c in pnlHeader.Controls)
            {
                if (c != chkGroup)
                {
                    c.Click += ToggleExpand;
                    c.MouseEnter += (s, e) => { pnlHeader.BackColor = _hoverColor; BackColor = _hoverColor; };
                    c.MouseLeave += (s, e) => { pnlHeader.BackColor = _bgColor; BackColor = _bgColor; };
                }
            }

            // Items Panel
            pnlItems.BackColor = _bgColor;
            
            foreach (var item in Group.Items)
            {
                var itemPanel = new Panel { Width = pnlItems.Width - 10, Height = 40, Margin = new Padding(25, 0, 0, 0) };
                
                int textWidth = TextRenderer.MeasureText(item.Name, new Font("Segoe UI", 10)).Width;

                if (item.SizeBytes == 0 || item.Name == "No safe junk found.")
                {
                    var lblName = new Label
                    {
                        Text = item.Name,
                        ForeColor = Color.DarkGray,
                        Font = new Font("Segoe UI", 10),
                        AutoSize = false,
                        Location = new Point(10, 12),
                        Width = textWidth + 25
                    };
                    itemPanel.Controls.Add(lblName);
                    
                    if (item.Name != "No safe junk found.")
                    {
                        var lblSafety = SafetyBadgeHelper.CreateBadge(item.SafetyLevel);
                        lblSafety.Location = new Point(itemPanel.Width - 240, 9);
                        lblSafety.Anchor = AnchorStyles.Top | AnchorStyles.Right;

                        var lblItemSize = new Label
                        {
                            Text = item.FormattedSize,
                            ForeColor = Color.DarkGray,
                            Font = new Font("Segoe UI", 10),
                            AutoSize = true,
                            Location = new Point(itemPanel.Width - 100, 10),
                            Anchor = AnchorStyles.Top | AnchorStyles.Right
                        };

                        itemPanel.Controls.Add(lblSafety);
                        itemPanel.Controls.Add(lblItemSize);
                    }
                }
                else
                {
                    var chk = new CheckBox
                    {
                        Text = item.Name,
                        ForeColor = Color.White,
                        Font = new Font("Segoe UI", 10),
                        AutoSize = false,
                        Location = new Point(10, 10),
                        Checked = item.SafetyLevel != CleanupSafetyLevel.Protected && item.IsSelected,
                        Enabled = item.SafetyLevel != CleanupSafetyLevel.Protected,
                        Tag = item,
                        Cursor = Cursors.Hand,
                        Width = textWidth + 25
                    };
                    chk.CheckedChanged += ChildChk_CheckedChanged;
                    _childCheckboxes.Add(chk);
                    itemPanel.Controls.Add(chk);

                    var lblSafety = SafetyBadgeHelper.CreateBadge(item.SafetyLevel);
                    lblSafety.Location = new Point(itemPanel.Width - 240, 9);
                    lblSafety.Anchor = AnchorStyles.Top | AnchorStyles.Right;

                    var lblItemSize = new Label
                    {
                        Text = item.FormattedSize,
                        ForeColor = Color.DarkGray,
                        Font = new Font("Segoe UI", 10),
                        AutoSize = true,
                        Location = new Point(itemPanel.Width - 100, 10),
                        Anchor = AnchorStyles.Top | AnchorStyles.Right
                    };

                    itemPanel.Controls.Add(lblSafety);
                    itemPanel.Controls.Add(lblItemSize);
                }

                
                pnlItems.Controls.Add(itemPanel);
            }

            _expandedHeight = _headerHeight + (Group.Items.Count * 40) + 10;
            Height = _headerHeight;
        }

        // Removed obsolete GetSafetyText and GetSafetyColor

        private void ToggleExpand(object sender, EventArgs e)
        {
            _isExpanded = !_isExpanded;
            lblArrow.Text = _isExpanded ? "▲" : "▼";
            _animationTimer.Start();
        }

        private void AnimationTimer_Tick(object sender, EventArgs e)
        {
            int step = 20;
            if (_isExpanded)
            {
                if (Height < _expandedHeight)
                {
                    Height = Math.Min(Height + step, _expandedHeight);
                }
                else
                {
                    _animationTimer.Stop();
                }
            }
            else
            {
                if (Height > _headerHeight)
                {
                    Height = Math.Max(Height - step, _headerHeight);
                }
                else
                {
                    _animationTimer.Stop();
                }
            }
        }

        private void ChkGroup_CheckedChanged(object sender, EventArgs e)
        {
            if (_isUpdatingCheckboxes) return;
            
            _isUpdatingCheckboxes = true;
            bool isChecked = chkGroup.Checked;
            foreach (var chk in _childCheckboxes)
            {
                if (chk.Enabled)
                {
                    chk.Checked = isChecked;
                    if (chk.Tag is CleanupItem item)
                    {
                        item.IsSelected = isChecked;
                    }
                }
            }
            _isUpdatingCheckboxes = false;
            
            chkGroup.ForeColor = isChecked ? Color.FromArgb(170, 150, 250) : Color.White;
            SelectionChanged?.Invoke(this, EventArgs.Empty);
        }

        private void ChildChk_CheckedChanged(object sender, EventArgs e)
        {
            if (_isUpdatingCheckboxes) return;

            if (sender is CheckBox c && c.Tag is CleanupItem item)
            {
                if (item.SafetyLevel == CleanupSafetyLevel.Protected)
                {
                    c.Checked = false;
                }
                item.IsSelected = c.Checked;
            }

            _isUpdatingCheckboxes = true;
            
            int checkedCount = 0;
            foreach (var chk in _childCheckboxes)
            {
                if (chk.Checked) checkedCount++;
            }

            if (checkedCount == 0)
            {
                chkGroup.CheckState = CheckState.Unchecked;
                chkGroup.ForeColor = Color.White;
            }
            else if (checkedCount == _childCheckboxes.Count)
            {
                chkGroup.CheckState = CheckState.Checked;
                chkGroup.ForeColor = Color.FromArgb(170, 150, 250);
            }
            else
            {
                chkGroup.CheckState = CheckState.Indeterminate;
                chkGroup.ForeColor = Color.Gold; // Indicator for partial
            }

            _isUpdatingCheckboxes = false;
            
            SelectionChanged?.Invoke(this, EventArgs.Empty);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            // Draw border with rounded appearance using graphics (simple rect for now, padding 1 makes BackColor act as border)
            ControlPaint.DrawBorder(e.Graphics, ClientRectangle, _borderColor, ButtonBorderStyle.Solid);
        }
        
        public void SetAll(bool state)
        {
            chkGroup.Checked = state; // Triggers ChkGroup_CheckedChanged
        }
    }
}
