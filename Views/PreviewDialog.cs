using System;
using System.Drawing;
using System.Windows.Forms;
using Optimizer.Domain.Models;
using Optimizer.Localization;

namespace Optimizer.Views
{
    public class PreviewDialog : Form
    {
        public PreviewDialog(CustomAppJunkCategory category)
        {
            this.Text = L.Get("dialog.preview.titleFormat", category.CategoryName);
            this.Size = new Size(800, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(20, 20, 20);
            this.ForeColor = Color.White;
            this.ShowIcon = false;
            
            var lblTitle = new Label
            {
                Text = L.Get("dialog.preview.headerFormat", category.CategoryName),
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.Orange,
                Dock = DockStyle.Top,
                Height = 50,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(10, 0, 0, 0)
            };
            
            var lblSubtitle = new Label
            {
                Text = category.PreviewFiles.Count < category.FileCount 
                    ? L.Get("dialog.preview.partialFormat", category.PreviewFiles.Count, category.FileCount)
                    : L.Get("dialog.preview.allFormat", category.FileCount),
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.Gray,
                Dock = DockStyle.Top,
                Height = 30,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(10, 0, 0, 0)
            };

            var grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.FromArgb(25, 25, 25),
                ForeColor = Color.Black,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };

            grid.Columns.Add("FileName", L.Get("dialog.preview.colFileName"));
            grid.Columns.Add("Path", L.Get("dialog.preview.colFullPath"));
            grid.Columns.Add("Size", L.Get("dialog.preview.colSize"));
            grid.Columns.Add("Modified", L.Get("dialog.preview.colModified"));

            foreach (var f in category.PreviewFiles)
            {
                grid.Rows.Add(f.FileName, f.FullPath, f.SizeBytes.ToString("N0"), f.ModifiedDate.ToString("g"));
            }

            var btnClose = new Button
            {
                Text = L.Get("common.close"),
                Dock = DockStyle.Bottom,
                Height = 40,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(40, 40, 40),
                ForeColor = Color.White,
                Cursor = Cursors.Hand
            };
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.Click += (s, e) => this.Close();

            this.Controls.Add(grid);
            this.Controls.Add(lblSubtitle);
            this.Controls.Add(lblTitle);
            this.Controls.Add(btnClose);
        }
    }
}
