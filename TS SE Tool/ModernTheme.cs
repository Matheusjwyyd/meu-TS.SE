using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Windows.Forms;

namespace TS_SE_Tool
{
    /// <summary>
    /// Applies one consistent visual system to the main window and every modal form.
    /// The save editing controls and event handlers remain the existing controls.
    /// </summary>
    internal static class ModernTheme
    {
        internal static readonly Color Canvas = Color.FromArgb(240, 243, 246);
        internal static readonly Color Surface = Color.White;
        internal static readonly Color Ink = Color.FromArgb(45, 51, 58);
        internal static readonly Color Muted = Color.FromArgb(111, 120, 130);
        internal static readonly Color Navy = Color.FromArgb(45, 50, 56);
        internal static readonly Color NavyLight = Color.FromArgb(61, 67, 74);
        internal static readonly Color PrimaryGray = Color.FromArgb(103, 112, 122);
        internal static readonly Color PrimaryGrayHover = Color.FromArgb(82, 91, 101);
        internal static readonly Color Border = Color.FromArgb(211, 217, 223);
        internal static readonly Color Danger = Color.FromArgb(184, 59, 70);

        private static readonly HashSet<Form> StyledForms = new HashSet<Form>();
        private static readonly Dictionary<string, Font> FontCache = new Dictionary<string, Font>();
        private static readonly Font RegularFont = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
        private static readonly Font StrongFont = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
        private static readonly Font MenuFont = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
        private static readonly ToolStripProfessionalRenderer Renderer = new ToolStripProfessionalRenderer(new ModernColorTable());

        internal static void Install()
        {
            Application.Idle -= Application_Idle;
            Application.Idle += Application_Idle;
        }

        internal static void ConfigureMainWindow(Form form, TableLayoutPanel rootLayout)
        {
            form.ClientSize = new Size(1320, 810);
            form.MinimumSize = new Size(1024, 700);
            form.MaximumSize = Size.Empty;
            form.MaximizeBox = true;
            form.FormBorderStyle = FormBorderStyle.Sizable;
            form.StartPosition = FormStartPosition.CenterScreen;

            if (rootLayout != null && rootLayout.ColumnStyles.Count > 1)
            {
                rootLayout.ColumnStyles[0].SizeType = SizeType.Percent;
                rootLayout.ColumnStyles[0].Width = 100F;
                rootLayout.ColumnStyles[1].SizeType = SizeType.Absolute;
                rootLayout.ColumnStyles[1].Width = 280F;
            }

            MenuStrip menuStrip = FindControl<MenuStrip>(form, "menuStripMain");
            StatusStrip statusStrip = FindControl<StatusStrip>(form, "statusStripMain");
            TabControl mainTabs = FindControl<TabControl>(form, "tabControlMain");
            TableLayoutPanel sidebar = FindControl<TableLayoutPanel>(form, "tableLayoutPanel16");
            GroupBox profileCard = FindControl<GroupBox>(form, "groupBoxMainProfilesAndSaves");

            if (mainTabs != null)
            {
                mainTabs.Dock = DockStyle.Fill;
                mainTabs.Margin = new Padding(4);
                mainTabs.DrawMode = TabDrawMode.OwnerDrawFixed;
                mainTabs.SizeMode = TabSizeMode.Fixed;
                mainTabs.Multiline = false;
                mainTabs.Padding = new Point(12, 5);
            }

            if (menuStrip != null && statusStrip != null && rootLayout != null)
            {
                form.SuspendLayout();
                form.Controls.Remove(menuStrip);
                form.Controls.Remove(rootLayout);
                form.Controls.Remove(statusStrip);

                menuStrip.Dock = DockStyle.Top;
                menuStrip.AutoSize = false;
                menuStrip.LayoutStyle = ToolStripLayoutStyle.HorizontalStackWithOverflow;
                menuStrip.Height = 32;
                menuStrip.ImageScalingSize = new Size(18, 18);
                statusStrip.Dock = DockStyle.Bottom;
                statusStrip.AutoSize = false;
                statusStrip.Height = 24;

                TableLayoutPanel contentHost = new TableLayoutPanel();
                contentHost.Name = "modernContentHost";
                contentHost.Dock = DockStyle.None;
                contentHost.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
                contentHost.Margin = Padding.Empty;
                contentHost.Padding = Padding.Empty;
                contentHost.ColumnCount = 1;
                contentHost.RowCount = 1;
                contentHost.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
                contentHost.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
                rootLayout.Dock = DockStyle.Fill;
                contentHost.Controls.Add(rootLayout, 0, 0);
                menuStrip.Dock = DockStyle.Top;
                statusStrip.Dock = DockStyle.Bottom;
                form.Controls.Add(contentHost);
                form.Controls.Add(menuStrip);
                form.Controls.Add(statusStrip);
                form.Controls.SetChildIndex(menuStrip, 0);
                form.Controls.SetChildIndex(statusStrip, 1);
                form.Controls.SetChildIndex(contentHost, 2);
                form.MainMenuStrip = menuStrip;
                form.ResumeLayout(true);
                form.Resize -= MainWindow_Resize;
                form.Resize += MainWindow_Resize;
                LayoutMainContent(form);
            }

            if (sidebar != null)
            {
                sidebar.Padding = new Padding(8, 8, 10, 8);
                if (sidebar.RowStyles.Count >= 6)
                {
                    sidebar.RowStyles[0].Height = 88F;
                    sidebar.RowStyles[1].Height = 250F;
                    sidebar.RowStyles[2].Height = 68F;
                    sidebar.RowStyles[3].Height = 34F;
                    sidebar.RowStyles[5].Height = 74F;
                }

                Control gamePicker = FindControl<Control>(sidebar, "tableLayoutPanel10");
                Control loadCard = FindControl<Control>(sidebar, "tableLayoutPanel11");
                Control loadButton = FindControl<Control>(sidebar, "buttonMainLoadSave");
                Control unloadButton = FindControl<Control>(sidebar, "buttonMainCloseSave");
                Control saveButton = FindControl<Control>(sidebar, "buttonMainWriteSave");
                if (gamePicker != null) gamePicker.Margin = new Padding(4, 2, 4, 4);
                if (profileCard != null) profileCard.Margin = new Padding(4, 4, 4, 6);
                if (loadCard != null) loadCard.Margin = new Padding(4, 4, 4, 4);
                if (loadButton != null) loadButton.Margin = new Padding(0, 2, 0, 2);
                if (unloadButton != null) unloadButton.Margin = new Padding(4, 0, 0, 2);
                if (saveButton != null) saveButton.Margin = new Padding(4, 4, 0, 2);
            }

            if (profileCard != null)
            {
                profileCard.Resize -= ProfileCard_Resize;
                profileCard.Resize += ProfileCard_Resize;
                LayoutProfileCard(profileCard);
            }
        }

        internal static void RefreshMainNavigation(Form form)
        {
            TabControl tabs = FindControl<TabControl>(form, "tabControlMain");
            if (tabs == null)
                return;

            ApplyTabControl(tabs);
            tabs.Invalidate();
        }

        private static T FindControl<T>(Control parent, string name) where T : Control
        {
            if (parent == null)
                return null;

            Control[] matches = parent.Controls.Find(name, true);
            return matches.Length > 0 ? matches[0] as T : null;
        }

        private static void MainWindow_Resize(object sender, EventArgs e)
        {
            LayoutMainContent(sender as Form);
        }

        private static void LayoutMainContent(Form form)
        {
            if (form == null || form.IsDisposed)
                return;

            MenuStrip menuStrip = FindControl<MenuStrip>(form, "menuStripMain");
            StatusStrip statusStrip = FindControl<StatusStrip>(form, "statusStripMain");
            Control contentHost = FindControl<Control>(form, "modernContentHost");
            if (menuStrip == null || statusStrip == null || contentHost == null)
                return;

            int top = menuStrip.Bottom;
            int bottom = statusStrip.Top;
            Rectangle bounds = new Rectangle(0, top, menuStrip.Width, Math.Max(0, bottom - top));
            if (contentHost.Bounds != bounds)
                contentHost.Bounds = bounds;

            TableLayoutPanel mainLayout = FindControl<TableLayoutPanel>(form, "tableLayoutPanel15");
            if (mainLayout != null && mainLayout.ColumnStyles.Count > 1 && contentHost.ClientSize.Width > 0)
            {
                const float sidebarWidth = 280F;
                float centerPercent = Math.Max(1F, (contentHost.ClientSize.Width - sidebarWidth) * 100F / contentHost.ClientSize.Width);
                mainLayout.ColumnStyles[0].SizeType = SizeType.Percent;
                mainLayout.ColumnStyles[0].Width = centerPercent;
                mainLayout.ColumnStyles[1].SizeType = SizeType.Absolute;
                mainLayout.ColumnStyles[1].Width = sidebarWidth;
                mainLayout.PerformLayout();
            }
        }

        private static void ProfileCard_Resize(object sender, EventArgs e)
        {
            LayoutProfileCard(sender as GroupBox);
        }

        private static void LayoutProfileCard(GroupBox card)
        {
            if (card == null || card.IsDisposed)
                return;

            int width = card.ClientSize.Width;
            int height = card.ClientSize.Height;
            const int left = 10;
            const int right = 10;
            const int actionWidth = 26;
            int actionX = Math.Max(left + 135, width - right - actionWidth);
            int comboWidth = Math.Max(120, actionX - left - 8);

            string[] comboNames = { "comboBoxRootFolders", "comboBoxProfiles", "comboBoxSaves" };
            string[] buttonNames = { "buttonProfilesAndSavesRefreshAll", "buttonProfilesAndSavesEditProfile", "buttonProfilesAndSavesRestoreBackup" };
            for (int i = 0; i < comboNames.Length; i++)
            {
                ComboBox combo = FindControl<ComboBox>(card, comboNames[i]);
                Button button = FindControl<Button>(card, buttonNames[i]);
                if (combo != null)
                    combo.Width = comboWidth;
                if (button != null)
                {
                    button.Location = new Point(actionX, combo == null ? 106 + i * 27 : combo.Top - 2);
                    button.Size = new Size(actionWidth, 25);
                }
            }

            PictureBox avatar = FindControl<PictureBox>(card, "pictureBoxProfileAvatar");
            if (avatar != null)
            {
                avatar.Location = new Point(left, 23);
                avatar.Size = new Size(82, 82);
            }

            CheckBox backups = FindControl<CheckBox>(card, "checkBoxProfilesAndSavesProfileBackups");
            if (backups != null)
                backups.Location = new Point(Math.Max(left + 90, width - backups.Width - right), 82);

            int buttonTop = Math.Max(184, height - 33);
            int actionButtonWidth = Math.Max(92, (width - left - right - 8) / 2);
            Button decrypt = FindControl<Button>(card, "buttonMainDecryptSave");
            Button openFolder = FindControl<Button>(card, "buttonProfilesAndSavesOpenSaveFolder");
            if (decrypt != null)
            {
                decrypt.Location = new Point(left, buttonTop);
                decrypt.Size = new Size(actionButtonWidth, 25);
                decrypt.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            }
            if (openFolder != null)
            {
                openFolder.Location = new Point(left + actionButtonWidth + 8, buttonTop);
                openFolder.Size = new Size(actionButtonWidth, 25);
                openFolder.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            }
        }

        internal static void ConfigureSplash(Form splash, TableLayoutPanel contentTable)
        {
            splash.ClientSize = new Size(480, 570);
            splash.MinimumSize = new Size(480, 570);
            splash.MaximumSize = new Size(480, 570);
            splash.FormBorderStyle = FormBorderStyle.FixedDialog;
            splash.StartPosition = FormStartPosition.CenterParent;
            splash.ShowInTaskbar = false;

            if (contentTable == null || contentTable.RowStyles.Count == 0 || contentTable.Controls.ContainsKey("modernBrandBanner"))
                return;

            contentTable.RowStyles[0].SizeType = SizeType.Absolute;
            contentTable.RowStyles[0].Height = 126F;

            PictureBox banner = new PictureBox();
            banner.Name = "modernBrandBanner";
            banner.Dock = DockStyle.Fill;
            banner.Margin = new Padding(8, 8, 8, 4);
            banner.SizeMode = PictureBoxSizeMode.StretchImage;
            banner.Image = ModernBrandArtwork.CreateBanner(960, 240);
            contentTable.Controls.Add(banner, 0, 0);
            splash.FormClosed += delegate
            {
                if (banner.Image != null)
                {
                    banner.Image.Dispose();
                    banner.Image = null;
                }
            };
        }

        internal static void ApplyNow(Form form)
        {
            if (form == null || form.IsDisposed)
                return;

            if (!StyledForms.Contains(form))
            {
                StyledForms.Add(form);
                form.FormClosed += Form_FormClosed;
            }

            ApplyForm(form);
        }

        private static void Application_Idle(object sender, EventArgs e)
        {
            for (int i = 0; i < Application.OpenForms.Count; i++)
            {
                Form form = Application.OpenForms[i];
                if (form != null && !form.IsDisposed && !StyledForms.Contains(form))
                    ApplyNow(form);
            }
        }

        private static void Form_FormClosed(object sender, FormClosedEventArgs e)
        {
            Form form = sender as Form;
            if (form != null)
                StyledForms.Remove(form);
        }

        private static void ApplyForm(Form form)
        {
            form.BackColor = Canvas;
            form.ForeColor = Ink;
            form.Font = RegularFont;
            form.Icon = Properties.Resources.MainIco;

            ApplyControls(form.Controls, new HashSet<Control>());
            form.Invalidate(true);
        }

        private static void ApplyControls(Control.ControlCollection controls, HashSet<Control> visited)
        {
            foreach (Control control in controls)
            {
                if (!visited.Add(control))
                    continue;

                ToolStrip toolStrip = control as ToolStrip;
                if (toolStrip != null)
                {
                    ApplyToolStrip(toolStrip);
                    continue;
                }

                ApplyControl(control);
                ApplyControls(control.Controls, visited);
            }
        }

        private static void ApplyControl(Control control)
        {
            if (control is TabControl)
            {
                ApplyTabControl((TabControl)control);
                return;
            }

            if (control is GroupBox)
            {
                control.BackColor = Surface;
                control.ForeColor = Ink;
                control.Font = StrongFont;
                ((GroupBox)control).FlatStyle = FlatStyle.Flat;
                return;
            }

            if (control is Button)
            {
                ApplyButton((Button)control);
                return;
            }

            if (control is RadioButton)
            {
                ApplyRadioButton((RadioButton)control);
                return;
            }

            if (control is CheckBox)
            {
                control.ForeColor = Ink;
                control.BackColor = Color.Transparent;
                ((CheckBox)control).FlatStyle = FlatStyle.Flat;
                return;
            }

            if (control is TextBoxBase)
            {
                control.BackColor = Surface;
                control.ForeColor = Ink;
                ((TextBoxBase)control).BorderStyle = BorderStyle.FixedSingle;
                control.Font = RegularFont;
                return;
            }

            if (control is ComboBox)
            {
                control.BackColor = Surface;
                control.ForeColor = Ink;
                ((ComboBox)control).FlatStyle = FlatStyle.Flat;
                control.Font = RegularFont;
                return;
            }

            if (control is ListBox || control is CheckedListBox || control is ListView)
            {
                control.BackColor = Surface;
                control.ForeColor = Ink;
                control.Font = RegularFont;
                if (control is ListView)
                    ((ListView)control).BorderStyle = BorderStyle.FixedSingle;
                return;
            }

            if (control is DataGridView)
            {
                ApplyGrid((DataGridView)control);
                return;
            }

            if (control is NumericUpDown || control is DateTimePicker)
            {
                control.BackColor = Surface;
                control.ForeColor = Ink;
                control.Font = RegularFont;
                return;
            }

            if (control is LinkLabel)
            {
                LinkLabel link = (LinkLabel)control;
                link.LinkColor = PrimaryGray;
                link.ActiveLinkColor = PrimaryGrayHover;
                link.VisitedLinkColor = NavyLight;
                link.Font = RegularFont;
                return;
            }

            if (control is Label)
            {
                if (control.ForeColor == SystemColors.ControlText || control.ForeColor == Color.Black)
                    control.ForeColor = Ink;
                control.Font = FontFor(control.Font);
                return;
            }

            if (control is TabPage)
            {
                control.BackColor = Surface;
                control.ForeColor = Ink;
                control.Font = RegularFont;
                return;
            }

            if (control is TableLayoutPanel || control is FlowLayoutPanel)
            {
                control.BackColor = control.Name == "tableLayoutPanel16" ? Color.FromArgb(234, 238, 242) : Canvas;
                control.ForeColor = Ink;
                control.Font = RegularFont;
                return;
            }

            if (control is Panel)
            {
                control.BackColor = Canvas;
                control.ForeColor = Ink;
                return;
            }

            if (control is PictureBox)
            {
                if (((PictureBox)control).Image == null)
                    control.BackColor = Color.Transparent;
                return;
            }

            control.ForeColor = Ink;
            control.Font = FontFor(control.Font);
        }

        private static Font FontFor(Font current)
        {
            if (current == null)
                return RegularFont;

            FontStyle style = current.Style;
            float size = Math.Max(8.25F, current.Size);
            if (style == FontStyle.Regular && Math.Abs(size - 9F) < 0.25F)
                return RegularFont;
            if ((style & FontStyle.Bold) != 0 && Math.Abs(size - 9F) < 0.25F)
                return StrongFont;

            string key = size.ToString("F1", CultureInfo.InvariantCulture) + ":" + ((int)style).ToString(CultureInfo.InvariantCulture);
            Font cached;
            if (!FontCache.TryGetValue(key, out cached))
            {
                cached = new Font("Segoe UI", size, style, current.Unit, current.GdiCharSet, current.GdiVerticalFont);
                FontCache.Add(key, cached);
            }
            return cached;
        }

        private static void ApplyButton(Button button)
        {
            bool primary = button.Name == "buttonMainLoadSave" || button.Name == "buttonMainWriteSave" ||
                           button.Name == "buttonOK" || button.Name.IndexOf("Save", StringComparison.OrdinalIgnoreCase) >= 0 ||
                           button.Name.IndexOf("Load", StringComparison.OrdinalIgnoreCase) >= 0;
            bool destructive = button.Name.IndexOf("Delete", StringComparison.OrdinalIgnoreCase) >= 0 ||
                               button.Name.IndexOf("Fire", StringComparison.OrdinalIgnoreCase) >= 0;

            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = primary ? 0 : 1;
            button.FlatAppearance.BorderColor = Border;
            button.FlatAppearance.MouseOverBackColor = primary ? PrimaryGrayHover : Color.FromArgb(226, 231, 236);
            button.FlatAppearance.MouseDownBackColor = Color.FromArgb(211, 217, 223);
            button.Cursor = Cursors.Hand;
            button.Font = FontFor(button.Font);

            if (destructive)
            {
                button.BackColor = Color.FromArgb(255, 239, 240);
                button.ForeColor = Danger;
            }
            else if (primary)
            {
                button.BackColor = PrimaryGray;
                button.ForeColor = Color.White;
                button.Font = StrongFont;
            }
            else
            {
                button.BackColor = Surface;
                button.ForeColor = Ink;
            }
        }

        private static void ApplyRadioButton(RadioButton button)
        {
            button.ForeColor = Ink;
            button.Font = FontFor(button.Font);
            if (button.Appearance == Appearance.Button)
            {
                button.FlatStyle = FlatStyle.Flat;
                button.FlatAppearance.BorderSize = 1;
                button.FlatAppearance.BorderColor = Border;
                button.FlatAppearance.MouseOverBackColor = Color.FromArgb(226, 231, 236);
                button.BackColor = button.Checked ? PrimaryGray : Surface;
                if (button.Checked)
                    button.ForeColor = Color.White;
                button.CheckedChanged += delegate
                {
                    button.BackColor = button.Checked ? PrimaryGray : Surface;
                    button.ForeColor = button.Checked ? Color.White : Ink;
                };
            }
            else
                button.BackColor = Color.Transparent;
        }

        private static void ApplyToolStrip(ToolStrip strip)
        {
            bool status = strip is StatusStrip;
            strip.Renderer = Renderer;
            strip.BackColor = status ? NavyLight : Navy;
            strip.ForeColor = Color.White;
            strip.Font = MenuFont;
            strip.GripStyle = ToolStripGripStyle.Hidden;
            strip.Padding = status ? new Padding(8, 2, 8, 2) : new Padding(8, 3, 8, 3);
            if (strip is MenuStrip)
                strip.AutoSize = false;

            foreach (ToolStripItem item in strip.Items)
                ApplyToolStripItem(item, status ? Color.White : Color.FromArgb(231, 240, 246));
        }

        private static void ApplyToolStripItem(ToolStripItem item, Color foreground)
        {
            item.ForeColor = foreground;
            item.Font = MenuFont;
            if (!(item is ToolStripSeparator))
                item.Margin = new Padding(4, 1, 4, 1);

            ToolStripDropDownItem dropdown = item as ToolStripDropDownItem;
            if (dropdown != null)
            {
                dropdown.DropDown.BackColor = NavyLight;
                dropdown.DropDown.ForeColor = Color.White;
                dropdown.DropDown.Renderer = Renderer;
                foreach (ToolStripItem child in dropdown.DropDownItems)
                    ApplyToolStripItem(child, Color.FromArgb(231, 240, 246));
            }
        }

        private static void ApplyTabControl(TabControl tabs)
        {
            tabs.DrawMode = TabDrawMode.OwnerDrawFixed;
            tabs.SizeMode = TabSizeMode.Fixed;
            tabs.Padding = new Point(12, 5);
            tabs.BackColor = Surface;
            tabs.ForeColor = Ink;
            tabs.DrawItem -= TabControl_DrawItem;
            tabs.DrawItem += TabControl_DrawItem;
            UpdateTabItemSize(tabs);
        }

        private static void UpdateTabItemSize(TabControl tabs)
        {
            int tabWidth = Math.Max(82, Math.Min(145, (Math.Max(tabs.ClientSize.Width, 260) - 28) / Math.Max(tabs.TabPages.Count, 1)));
            tabs.ItemSize = new Size(tabWidth, tabs.Name == "tabControlMain" ? 38 : 32);
        }

        private static void TabControl_DrawItem(object sender, DrawItemEventArgs e)
        {
            TabControl tabs = sender as TabControl;
            if (tabs == null || e.Index < 0 || e.Index >= tabs.TabPages.Count)
                return;

            TabPage page = tabs.TabPages[e.Index];
            bool selected = e.Index == tabs.SelectedIndex;
            Rectangle bounds = e.Bounds;
            Graphics graphics = e.Graphics;
            graphics.SmoothingMode = SmoothingMode.AntiAlias;

            using (SolidBrush background = new SolidBrush(selected ? Surface : Canvas))
                graphics.FillRectangle(background, bounds);

            if (selected)
            {
                using (SolidBrush underline = new SolidBrush(PrimaryGray))
                    graphics.FillRectangle(underline, bounds.Left + 8, bounds.Bottom - 3, bounds.Width - 16, 3);
            }

            int textLeft = bounds.Left + 8;
            if (tabs.ImageList != null && page.ImageIndex >= 0 && page.ImageIndex < tabs.ImageList.Images.Count)
            {
                Image image = tabs.ImageList.Images[page.ImageIndex];
                int imageSize = Math.Min(22, bounds.Height - 8);
                int imageY = bounds.Top + (bounds.Height - imageSize) / 2 - 1;
                graphics.DrawImage(image, new Rectangle(textLeft, imageY, imageSize, imageSize));
                textLeft += imageSize + 6;
            }

            Rectangle textBounds = new Rectangle(textLeft, bounds.Top + 2, bounds.Right - textLeft - 5, bounds.Height - 6);
            TextRenderer.DrawText(graphics, page.Text, selected ? StrongFont : RegularFont, textBounds,
                selected ? Ink : Muted,
                TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
        }

        private static void ApplyGrid(DataGridView grid)
        {
            grid.BackgroundColor = Surface;
            grid.BorderStyle = BorderStyle.None;
            grid.GridColor = Border;
            grid.EnableHeadersVisualStyles = false;
            grid.ColumnHeadersDefaultCellStyle.BackColor = NavyLight;
            grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            grid.ColumnHeadersDefaultCellStyle.Font = StrongFont;
            grid.DefaultCellStyle.BackColor = Surface;
            grid.DefaultCellStyle.ForeColor = Ink;
            grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(222, 227, 232);
            grid.DefaultCellStyle.SelectionForeColor = Ink;
            grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(247, 250, 252);
            grid.RowHeadersDefaultCellStyle.BackColor = Canvas;
            grid.RowHeadersDefaultCellStyle.ForeColor = Muted;
            grid.Font = RegularFont;
        }

        private sealed class ModernColorTable : ProfessionalColorTable
        {
            public override Color MenuStripGradientBegin { get { return Navy; } }
            public override Color MenuStripGradientEnd { get { return Navy; } }
            public override Color ToolStripGradientBegin { get { return Navy; } }
            public override Color ToolStripGradientMiddle { get { return Navy; } }
            public override Color ToolStripGradientEnd { get { return Navy; } }
            public override Color MenuItemSelected { get { return NavyLight; } }
            public override Color MenuItemSelectedGradientBegin { get { return NavyLight; } }
            public override Color MenuItemSelectedGradientEnd { get { return NavyLight; } }
            public override Color MenuItemPressedGradientBegin { get { return PrimaryGrayHover; } }
            public override Color MenuItemPressedGradientEnd { get { return PrimaryGrayHover; } }
            public override Color MenuBorder { get { return Navy; } }
            public override Color ToolStripBorder { get { return Navy; } }
            public override Color SeparatorDark { get { return Color.FromArgb(55, 83, 105); } }
            public override Color SeparatorLight { get { return Color.FromArgb(55, 83, 105); } }
        }
    }
}
