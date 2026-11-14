/*
   Copyright 2016-2022 LIPtoH <liptoh.codebase@gmail.com>

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/
using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;
using TS_SE_Tool.Utilities;

namespace TS_SE_Tool
{
    public partial class FormMain
    {
        private string[] defaultProfileSkillLabels;
        private string defaultProfileSkillGroupTitle;

        internal void CacheProfileSkillPresentation()
        {
            defaultProfileSkillLabels = new string[6];
            for (int i = 0; i < defaultProfileSkillLabels.Length; i++)
            {
                Label label = groupBoxProfileSkill.Controls.Find("labelProfileSkill" + i, false).FirstOrDefault() as Label;
                defaultProfileSkillLabels[i] = label == null ? "" : label.Text;
            }
            defaultProfileSkillGroupTitle = groupBoxProfileSkill.Text;
        }

        internal void SetProfileEditingMode(bool busMode)
        {
            if (defaultProfileSkillLabels == null)
                CacheProfileSkillPresentation();
            LayoutProfileGroups();

            for (int i = 0; i < 6; i++)
            {
                Label label = groupBoxProfileSkill.Controls.Find("labelProfileSkill" + i, false).FirstOrDefault() as Label;
                Panel icon = groupBoxProfileSkill.Controls.Find("profileSkillsPanel" + i, false).FirstOrDefault() as Panel;
                if (label != null)
                {
                    label.Text = busMode ? CoachSkillNames[i] : defaultProfileSkillLabels[i];
                    if (busMode)
                    {
                        int busRowY = 17 + 62 * i;
                        int busPipsX = GetBusProfilePipsX();
                        label.AutoSize = false;
                        label.Size = new Size(Math.Max(125, busPipsX - 92), 58);
                        label.TextAlign = ContentAlignment.MiddleLeft;
                        label.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
                        label.Location = new Point(80, busRowY);
                    }
                    else
                    {
                        label.AutoSize = true;
                        label.TextAlign = ContentAlignment.TopLeft;
                        label.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
                        label.Location = new Point(88, 17 + 69 * i);
                    }
                }
                if (icon != null)
                {
                    if (icon.BackgroundImage != null)
                        icon.BackgroundImage.Dispose();
                    if (busMode)
                    {
                        icon.BackgroundImage = CreateCoachProfileSkillIcon(i);
                        icon.Size = new Size(58, 58);
                        icon.Location = new Point(12, 17 + 62 * i);
                    }
                    else
                    {
                        icon.BackgroundImage = new Bitmap(SkillImgS[i], 64, 64);
                        icon.Size = new Size(64, 64);
                        icon.Location = new Point(12, 17 + 69 * i);
                    }
                }
            }

            for (int i = 0; i < ADRbuttonArray.Length; i++)
            {
                ADRbuttonArray[i].Visible = !busMode || i < 3;
                ADRbuttonArray[i].Location = busMode
                    ? new Point(GetBusProfilePipsX() + 54 * i, 22)
                    : new Point(88 + 54 * i, 31);
                if (busMode)
                    ADRbuttonArray[i].Image = null;
                else
                    ADRbuttonArray[i].Image = ADRImgSGrey[i];
            }
            for (int i = 0; i < SkillButtonArray.GetLength(0); i++)
                for (int j = 0; j < SkillButtonArray.GetLength(1); j++)
                {
                    SkillButtonArray[i, j].Visible = !busMode || j < 3;
                    SkillButtonArray[i, j].Location = new Point(88 + 54 * j,
                        busMode ? 22 + 62 * (i + 1) : 31 + 69 * (i + 1));
                    if (busMode)
                        SkillButtonArray[i, j].Left = GetBusProfilePipsX() + 54 * j;
                }

            groupBoxProfileSkill.Text = busMode
                ? "Habilidades do ônibus"
                : defaultProfileSkillGroupTitle;
            if (busMode)
                UpdateCoachSkillPointsLabel();
        }

        private Image CreateCoachProfileSkillIcon(int skill)
        {
            int truckSkillIcon = new int[] { 1, 4, 2, 3, -1, 5 }[skill];
            if (truckSkillIcon >= 0)
                return new Bitmap(SkillImgS[truckSkillIcon], 58, 58);

            Image heart = CreateCoachSkillIcon(4);
            Bitmap result = new Bitmap(heart, 58, 58);
            heart.Dispose();
            return result;
        }

        private int GetBusProfilePipsX()
        {
            return Math.Max(236, groupBoxProfileSkill.ClientSize.Width - 168);
        }

        private void LayoutProfileGroups()
        {
            if (tabPageProfile == null || groupBoxProfileSkill == null || groupBoxProfileUserColorsShort == null)
                return;

            int skillWidth = IsBusMode
                ? Math.Max(420, tabPageProfile.ClientSize.Width - groupBoxProfileUserColorsShort.Width - 24)
                : 420;
            groupBoxProfileSkill.Width = skillWidth;
            groupBoxProfileUserColorsShort.Left = groupBoxProfileSkill.Right + 6;
            if (IsBusMode)
                LayoutBusProfileRows();
        }

        private void LayoutBusProfileRows()
        {
            int pipsX = GetBusProfilePipsX();
            for (int skill = 0; skill < 6; skill++)
            {
                int rowY = 17 + 62 * skill;
                Label label = groupBoxProfileSkill.Controls.Find("labelProfileSkill" + skill, false).FirstOrDefault() as Label;
                Panel icon = groupBoxProfileSkill.Controls.Find("profileSkillsPanel" + skill, false).FirstOrDefault() as Panel;
                if (label != null)
                {
                    label.Location = new Point(80, rowY);
                    label.Size = new Size(Math.Max(125, pipsX - 92), 58);
                }
                if (icon != null)
                    icon.Location = new Point(12, rowY);
            }

            for (int i = 0; i < ADRbuttonArray.Length; i++)
                ADRbuttonArray[i].Location = new Point(pipsX + 54 * i, 22);
            for (int row = 0; row < SkillButtonArray.GetLength(0); row++)
                for (int point = 0; point < SkillButtonArray.GetLength(1); point++)
                    SkillButtonArray[row, point].Location = new Point(pipsX + 54 * point, 22 + 62 * (row + 1));
        }

        internal void RefreshProfileForCurrentMode()
        {
            if (SiiNunitData == null)
                return;
            SetProfileEditingMode(IsBusMode);
            FormUpdatePlayerLevel();
            FillProfileSkillsForCurrentMode();
        }

        //Profile tab
        private void CreateProfilePanelControls()
        {
            int pSkillsNameHeight = 64, pSkillsNameWidth = 64, pSkillsNameOffset = 5, pSkillsNamelOffset = 12;
            
            for (int i = 0; i < 6; i++)
            {
                Panel Ppanel = new Panel();
                groupBoxProfileSkill.Controls.Add(Ppanel);

                Ppanel.Parent = groupBoxProfileSkill;
                Ppanel.Location = new Point(pSkillsNamelOffset, 17 + (pSkillsNameHeight + pSkillsNameOffset) * i);
                Ppanel.BorderStyle = BorderStyle.None;
                Ppanel.Size = new Size(pSkillsNameWidth, pSkillsNameHeight);
                Ppanel.Name = "profileSkillsPanel" + i.ToString();

                Bitmap bgimg = new Bitmap(SkillImgS[i], pSkillsNameHeight, pSkillsNameWidth);
                Ppanel.BackgroundImage = bgimg;

                Label slabel = new Label();
                groupBoxProfileSkill.Controls.Add(slabel);
                slabel.Name = "labelProfileSkill" + i.ToString();
                slabel.Location = new Point(pSkillsNamelOffset * 2 + pSkillsNameWidth, 17 + (pSkillsNameHeight + pSkillsNameOffset) * i);
                slabel.AutoSize = true;
            }

            int bADRHeight = 48, bADRWidth = 48, pOffset = 6, lOffset = pSkillsNameWidth + pSkillsNamelOffset * 2;
            
            for (int i = 0; i < 6; i++)
            {
                CheckBox Ppanel = new CheckBox();
                groupBoxProfileSkill.Controls.Add(Ppanel);

                Ppanel.Parent = groupBoxProfileSkill;

                Ppanel.Location = new Point(lOffset + (bADRWidth + pOffset) * i, 17 + 14);
                Ppanel.Appearance = Appearance.Button;
                Ppanel.FlatStyle = FlatStyle.Flat;
                Ppanel.Size = new Size(bADRWidth, bADRHeight);
                Ppanel.Name = "buttonADR" + i.ToString();
                Ppanel.Checked = false;
                Ppanel.Padding = new Padding(0, 0, 1, 2);
                Ppanel.BackgroundImageLayout = ImageLayout.Stretch;

                Ppanel.BackgroundImage = SkillImgSBG[0];
                Ppanel.Image = ADRImgSGrey[i];
                Ppanel.FlatAppearance.BorderSize = 0;

                Ppanel.MouseEnter += new EventHandler(ADRbutton_MouseEnter);
                Ppanel.MouseLeave += new EventHandler(ADRbutton_MouseLeave);
                Ppanel.Click += new EventHandler(ADRbutton_Click);
                Ppanel.CheckedChanged += new EventHandler(ADRbutton_CheckedChanged);
                Ppanel.MouseHover += new EventHandler(ADRbutton_MouseHover);

                ADRbuttonArray[i] = Ppanel;
            }

            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 6; j++)
                {
                    CheckBox Ppanel = new CheckBox();
                    groupBoxProfileSkill.Controls.Add(Ppanel);

                    Ppanel.Parent = groupBoxProfileSkill;

                    Ppanel.Location = new Point(lOffset + (bADRWidth + pOffset) * j, 17 + 14 + (pSkillsNameHeight + pSkillsNameOffset) * (i + 1));
                    Ppanel.Appearance = Appearance.Button;
                    Ppanel.FlatStyle = FlatStyle.Flat;
                    Ppanel.Size = new Size(bADRWidth, bADRHeight);
                    Ppanel.Name = "buttonSkill" + i.ToString() + j.ToString();
                    Ppanel.Checked = false;
                    Ppanel.Padding = new Padding(0, 0, 1, 2);
                    Ppanel.BackgroundImageLayout = ImageLayout.Zoom;

                    Ppanel.BackgroundImage = SkillImgSBG[0];
                    Ppanel.FlatAppearance.BorderSize = 0;
                    
                    Ppanel.MouseEnter += new EventHandler(Skillbutton_MouseEnter);
                    Ppanel.MouseLeave += new EventHandler(Skillbutton_MouseLeave);
                    Ppanel.Click += new EventHandler(Skillbutton_Click);
                    Ppanel.CheckedChanged += new EventHandler(Skillbutton_CheckedChanged);

                    SkillButtonArray[i, j] = Ppanel;
                }
            }

            tabPageProfile.Resize += (sender, args) => LayoutProfileGroups();
            LayoutProfileGroups();
        }

        private void FillFormProfileControls()
        {
            FormUpdatePlayerLevel();
            SetProfileEditingMode(IsBusMode);
            FillProfileSkillsForCurrentMode();

            panelProfileUserColors.VerticalScroll.Value = 0;
            panelProfileUserColors.AutoScrollPosition = new Point(0, 0);

            CreateUserColorsButtons();

            UpdateUserColorsButtons();
        }

        private void FillProfileSkillsForCurrentMode()
        {
            foreach (CheckBox button in ADRbuttonArray)
                button.Checked = false;
            foreach (CheckBox button in SkillButtonArray)
                button.Checked = false;

            if (IsBusMode)
            {
                int[] busSkills = GetCoachSkillValues(SiiNunitData.Economy);
                for (int level = 0; level < 3; level++)
                    ADRbuttonArray[level].Checked = busSkills[0] > level;
                for (int skill = 0; skill < 5; skill++)
                    for (int level = 0; level < 3; level++)
                        SkillButtonArray[skill, level].Checked = busSkills[skill + 1] > level;
                UpdateCoachSkillPointsLabel();
                return;
            }

            char[] ADR = Convert.ToString(SiiNunitData.Economy.adr, 2).PadLeft(6, '0').ToCharArray();
            Array.Reverse(ADR);
            for (int i = 0; i < ADR.Length; i++)
                ADRbuttonArray[i].Checked = ADR[i] == '1';

            for (int i = 0; i < 5; i++)
                for (int j = 0; j < SiiNunitData.Economy._playerSkills[i + 1]; j++)
                    SkillButtonArray[i, j].Checked = true;
        }

        private int[] GetCoachSkillValues(Save.Items.Economy economy)
        {
            return new int[]
            {
                economy.long_duty,
                economy.tight_schedule,
                economy.satisfaction_decrease,
                economy.low_acceleration,
                economy.satisfaction_increase,
                economy.ecodriving
            };
        }

        private void UpdateCoachSkillPointsLabel()
        {
            if (!IsBusMode || SiiNunitData == null)
                return;
            int spent = GetCoachSkillValues(SiiNunitData.Economy).Sum();
            int level = SiiNunitData.Economy.getBusPlayerLvl()[0];
            groupBoxProfileSkill.Text = "Habilidades do ônibus  ·  Pontos: " + (level - spent);
        }

        private void FormUpdatePlayerLevel()
        {
            if (SiiNunitData == null)
                return;

            int[] playerLvlPlus = IsBusMode
                ? SiiNunitData.Economy.getBusPlayerLvl()
                : SiiNunitData.Economy.getPlayerLvl();
            int playerlvl = playerLvlPlus[0];

            for (int i = PlayerLevelNames.Count - 1; i >= 0; i--)
                if (PlayerLevelNames[i].LevelLimit <= playerlvl)
                {
                    labelPlayerLevelName.Text = PlayerLevelNames[i].LevelName;
                    panelPlayerLevel.BackColor = PlayerLevelNames[i].NameColor;
                    break;
                }

            labelPlayerLevelNumber.Text = playerlvl.ToString();

            labelPlayerExperience.Text = IsBusMode
                ? SiiNunitData.Economy.bus_experience_points.ToString()
                : SiiNunitData.Economy.experience_points.ToString();
            labelPlayerExperienceNxtLvlThreshhold.Text = playerLvlPlus[1].ToString();
            UpdateCoachSkillPointsLabel();
        }

        private void CreateUserColorsButtons()
        {
            int padding = 3, width = 24, height = 24, spacing = 4;
            int usableSpace = groupBoxProfileUserColorsShort.Bounds.Width;
                        
            if (MainSaveFileInfoData.Version >= 49)
            {
                int ucc = SiiNunitData.Economy.user_colors.Count / 4;
                int btnNumber = 0;

                for (int i = 0; i < ucc; i++)
                {
                    for (int j = 0; j < 4; j++)
                    {
                        Button rb = new Button();
                        rb.Name = "buttonUC" + btnNumber.ToString();
                        rb.Text = null;

                        int x = padding + j * width + spacing * j;
                        int y = (spacing + height) * (i);

                        rb.Location = new Point(padding + j * width + spacing * j, (spacing + height) * i);
                        rb.Size = new Size(width, height);
                        rb.FlatStyle = FlatStyle.Flat;
                        rb.Enabled = false;

                        rb.Click += new EventHandler(SelectColor);

                        panelProfileUserColors.Controls.Add(rb);
                        btnNumber++;
                    }
                }

                tableLayoutPanelUserColors.RowStyles[1].Height = 50;

                if (ucc >= 40)
                    buttonAddUserColor.Enabled = false;
                else
                    buttonAddUserColor.Enabled = true;

                panelProfileUserColors.VerticalScroll.Maximum = 1500; //Virtual height
                panelProfileUserColors.MouseWheel += new MouseEventHandler(this.panelProfileUserColors_MouseWheel);
            }
            else
                for (int i = 0; i < SiiNunitData.Economy.user_colors.Count; i++)
                {
                    Button rb = new Button();
                    rb.Name = "buttonUC" + i.ToString();
                    rb.Text = null;
                    rb.Location = new Point((usableSpace - width * 3) / 2, 8 + (padding * 2 + height * 2) * i);
                    rb.Size = new Size(width * 3, height * 2);
                    rb.FlatStyle = FlatStyle.Flat;
                    rb.Enabled = false;

                    rb.Click += new EventHandler(SelectColor);

                    panelProfileUserColors.Controls.Add(rb);
                }
        }

        private void panelProfileUserColors_MouseWheel(object sender, MouseEventArgs e)
        {
            if (e.Delta != 0)
            {
                int scroll = 28;
                int location = Math.Abs(panelProfileUserColors.AutoScrollPosition.Y);

                if (e.Delta < 0)
                {
                    if (location + scroll < panelProfileUserColors.VerticalScroll.Maximum)
                    {
                        location += scroll;
                        panelProfileUserColors.VerticalScroll.Value = location;
                    }
                    else
                    {
                        location = panelProfileUserColors.VerticalScroll.Maximum;
                        panelProfileUserColors.AutoScrollPosition = new Point(0, location);
                    }
                }
                else
                {
                    if (location - scroll > 0)
                    {
                        location -= scroll;
                        panelProfileUserColors.VerticalScroll.Value = location;
                    }
                    else
                    {
                        location = 0;
                        panelProfileUserColors.AutoScrollPosition = new Point(0, location);
                    }
                }
            }
        }

        private void buttonProfileShareColors_Click(object sender, EventArgs e)
        {
            FormShareUserColors FormWindow = new FormShareUserColors();
            FormWindow.ShowDialog();
            UpdateUserColorsButtons();
        }

        private void UpdateUserColorsButtons()
        {
            if (MainSaveFileInfoData.Version >= 49)
            {
                int padding = 3, width = 24, height = 24, spacing = 4;

                for (int i = 0; i < SiiNunitData.Economy.user_colors.Count; i++)
                {
                    Button btn = null;
                    string btnname = "buttonUC" + i.ToString();

                    if (panelProfileUserColors.Controls.ContainsKey(btnname))
                    {
                        btn = panelProfileUserColors.Controls[btnname] as Button;

                        applyButtonVisuals(btn, i);
                    }
                    else
                    {
                        for (int j = 0; j < 4; j++)
                        {
                            btn = new Button();
                            btn.Name = "buttonUC" + i.ToString();
                            btn.Text = null;

                            int x = padding + j * width + spacing * j;
                            int y = (spacing + height) * (i / 4);

                            btn.Location = new Point(x, y);
                            btn.Size = new Size(width, height);
                            btn.FlatStyle = FlatStyle.Flat;
                            btn.Enabled = false;

                            btn.Click += new EventHandler(SelectColor);

                            panelProfileUserColors.Controls.Add(btn);

                            applyButtonVisuals(btn, i);

                            i++;
                        }

                        i--;
                    }
                }

                //Scroll panel to the top to properly add buttons
                panelProfileUserColors.AutoScrollPosition = new Point(0, 0);

                panelProfileUserColors.VerticalScroll.Maximum = (height + spacing) * SiiNunitData.Economy.user_colors.Count / 4;

                //Return to top position
                int location = panelProfileUserColors.VerticalScroll.Maximum - panelProfileUserColors.Height;

                if (location > 0)
                {
                    panelProfileUserColors.AutoScrollPosition = new Point(0, location);
                    panelProfileUserColors.VerticalScroll.Value = location;
                }
                //
            }
            else
            {
                int padding = 6, width = 23;

                for (int i = 0; i < SiiNunitData.Economy.user_colors.Count; i++)
                {
                    Button btn = null;
                    string btnname = "buttonUC" + i.ToString();

                    if (panelProfileUserColors.Controls.ContainsKey(btnname))
                    {
                        btn = panelProfileUserColors.Controls[btnname] as Button;
                    }
                    else
                    {
                        btn.Name = "buttonUC" + i.ToString();
                        btn.Text = null;
                        btn.Location = new Point(6 + (padding + width) * (i), 19);
                        btn.Size = new Size(width, 23);
                        btn.FlatStyle = FlatStyle.Flat;
                        btn.Enabled = false;
                        btn.Click += new EventHandler(SelectColor);

                        panelProfileUserColors.Controls.Add(btn);
                    }

                    if (btn != null)
                    {
                        applyButtonVisuals(btn, i);
                    }
                }
            }

            void applyButtonVisuals(Button _btn, int _idx)
            {
                _btn.Enabled = true;

                if (SiiNunitData.Economy.user_colors[_idx].color.A == 0)
                {
                    _btn.Text = "X";
                    _btn.BackColor = Color.FromKnownColor(KnownColor.Control);
                }
                else
                {
                    _btn.Text = "";
                    _btn.BackColor = SiiNunitData.Economy.user_colors[_idx].color;
                }
            }
        }

        private void DeleteUserColorsButtons()
        {
            short counter = 0;
            do
            {
                try
                {
                    Control[] tempArray = panelProfileUserColors.Controls.Find("buttonUC" + counter, false);

                    if (tempArray.Length > 0)
                    {
                        panelProfileUserColors.Controls.Remove(tempArray[0]);
                        counter++;
                    }
                    else
                    {
                        break;
                    }
                }
                catch
                {
                    break;
                }

            } while (true);
        }

        internal void SelectColor(object sender, EventArgs e)
        {
            Button obj = sender as Button;
            Color BC = obj.BackColor;

            if (obj.Text != "")
                BC = Color.White;

            OpenPainter.ColorPicker.FormColorPicker frm = new OpenPainter.ColorPicker.FormColorPicker(BC);
            frm.Font = SystemFonts.DialogFont;

            if (frm.ShowDialog() == DialogResult.OK)
            {
                int index = int.Parse(obj.Name.Substring(8));

                SiiNunitData.Economy.user_colors[index].color = frm.PrimaryColor;

                if (frm.PrimaryColor.A != 0)
                {
                    obj.Text = "";
                    obj.BackColor = frm.PrimaryColor;
                }
                else
                {
                    obj.Text = "X";
                    obj.BackColor = Color.FromKnownColor(KnownColor.Control);
                }
            }

            if (MainSaveFileInfoData.Version >= 49)
            {
                RemoveUserColorUnused4slot();
                RemoveUserColorUnused4slotUI();
                UpdateUserColorsButtons();
            }
        }

        private void buttonAddUserColor_Click(object sender, EventArgs e)
        {
            AddUserColor4slot();
        }

        internal void AddUserColor4slot()
        {
            for (int i = 0; i < 4; i++)
            {
                SiiNunitData.Economy.user_colors.Add(new Save.DataFormat.SCS_Color(0, 0, 0, 0));
            }

            //Scroll panel to the top to properly add buttons
            panelProfileUserColors.AutoScrollPosition = new Point(0, 0);

            UpdateUserColorsButtons();


            //Return to top position
            int location = panelProfileUserColors.VerticalScroll.Maximum - panelProfileUserColors.Height;

            if (location > 0)
            {
                panelProfileUserColors.AutoScrollPosition = new Point(0, location);
                panelProfileUserColors.VerticalScroll.Value = location;
            }
        }

        internal void RemoveUserColorUnused4slot()
        {
            int counter = SiiNunitData.Economy.user_colors.Count - 1;
            int slotCount = SiiNunitData.Economy.user_colors.Count / 4;

            try
            {
                for (int iCol = 0; iCol < slotCount; iCol++)
                {
                    bool delete = false;

                    for (int i = 0; i < 4; i++)
                    {
                        if (SiiNunitData.Economy.user_colors[counter - i].color.A == 0)
                        {
                            delete = true;
                        }
                        else
                        {
                            delete = false;
                            break;
                        }
                    }

                    counter = counter - 4;

                    if (delete)
                    {
                        for (int i = 4; i > 0; i--)
                        {
                            int btnNumber = counter + i;
                            SiiNunitData.Economy.user_colors.RemoveAt(btnNumber);
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            }
            catch
            { }
        }

        internal void RemoveUserColorUnused4slotUI()
        {
            List<string> btnList = new List<string>();

            foreach (Control ctrl in panelProfileUserColors.Controls)
            {
                btnList.Add(ctrl.Name.Substring(8, ctrl.Name.Length - 8));
            }

            foreach (string btnNumber in btnList)
            {
                if (int.Parse(btnNumber) >= SiiNunitData.Economy.user_colors.Count)
                    panelProfileUserColors.Controls.Remove(panelProfileUserColors.Controls.Find("buttonUC" + btnNumber.ToString(), false)[0]);
            }

            if (SiiNunitData.Economy.user_colors.Count / 4 >= 40)
                buttonAddUserColor.Enabled = false;
            else
                buttonAddUserColor.Enabled = true;
        }

        //Profile buttons
        private void buttonPlayerLvlPlus01_Click(object sender, EventArgs e)
        {
            if (IsBusMode)
                SiiNunitData.Economy.setBusPlayerExp(int.Parse(labelPlayerLevelNumber.Text) + 1);
            else
                SiiNunitData.Economy.setPlayerExp(int.Parse(labelPlayerLevelNumber.Text) + 1);

            FormUpdatePlayerLevel();
        }

        private void buttonPlayerLvlPlus10_Click(object sender, EventArgs e)
        {
            if (IsBusMode)
                SiiNunitData.Economy.setBusPlayerExp(int.Parse(labelPlayerLevelNumber.Text) + 10);
            else
                SiiNunitData.Economy.setPlayerExp(int.Parse(labelPlayerLevelNumber.Text) + 10);

            FormUpdatePlayerLevel();
        }

        private void buttonPlayerLvlMax_Click(object sender, EventArgs e)
        {
            if (IsBusMode)
                SiiNunitData.Economy.setBusPlayerExp(150);
            else
                SiiNunitData.Economy.setPlayerExp(150);

            FormUpdatePlayerLevel();
        }

        private void buttonPlayerLvlMinus01_Click(object sender, EventArgs e)
        {
            if (IsBusMode)
                SiiNunitData.Economy.setBusPlayerExp(int.Parse(labelPlayerLevelNumber.Text) - 1);
            else
                SiiNunitData.Economy.setPlayerExp(int.Parse(labelPlayerLevelNumber.Text) - 1);

            FormUpdatePlayerLevel();
        }

        private void buttonPlayerLvlMinus10_Click(object sender, EventArgs e)
        {
            if (IsBusMode)
                SiiNunitData.Economy.setBusPlayerExp(int.Parse(labelPlayerLevelNumber.Text) - 10);
            else
                SiiNunitData.Economy.setPlayerExp(int.Parse(labelPlayerLevelNumber.Text) - 10);

            FormUpdatePlayerLevel();
        }

        private void buttonPlayerLvlMin_Click(object sender, EventArgs e)
        {
            if (IsBusMode)
                SiiNunitData.Economy.setBusPlayerExp(0);
            else
                SiiNunitData.Economy.setPlayerExp(0);

            FormUpdatePlayerLevel();
        }
        // end profile buttons

        //Skill buttons
        private void Skillbutton_Click(object sender, EventArgs e)
        {
            CheckBox thisbutton = sender as CheckBox;

            int skillIndex = int.Parse(thisbutton.Name.Substring(11, 1));
            byte buttonIndex = byte.Parse(thisbutton.Name.Substring(12, 1));

            if (IsBusMode)
            {
                ApplyCoachSkillLevel(skillIndex + 1, buttonIndex, thisbutton.Checked);
                return;
            }

            if (thisbutton.Checked)
            {
                for (int j = 0; j < buttonIndex; j++)                
                    SkillButtonArray[skillIndex, j].Checked = true;

                SiiNunitData.Economy._playerSkills[++skillIndex] = ++buttonIndex;
            }
            else
            {
                for (int j = 5; j >= buttonIndex; j--)                
                    SkillButtonArray[skillIndex, j].Checked = false;

                SiiNunitData.Economy._playerSkills[++skillIndex] = buttonIndex;
            }
        }

        private void Skillbutton_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox thisbutton = sender as CheckBox;

            if (thisbutton.Checked)            
                thisbutton.BackgroundImage = SkillImgSBG[3];            
            else            
                thisbutton.BackgroundImage = SkillImgSBG[0];
        }

        private void Skillbutton_MouseEnter(object sender, EventArgs e)
        {
            CheckBox thisbutton = sender as CheckBox;

            int skillIndex = int.Parse(thisbutton.Name.Substring(11, 1));
            byte buttonIndex = byte.Parse(thisbutton.Name.Substring(12, 1));
            int lastPoint = IsBusMode ? 2 : 5;

            for (int j = 0; j <= buttonIndex; j++)
            {
                if (!SkillButtonArray[skillIndex, j].Checked)                
                    SkillButtonArray[skillIndex, j].BackgroundImage = SkillImgSBG[1];
            }

            for (int j = buttonIndex; j <= lastPoint; j++)
            {
                if (SkillButtonArray[skillIndex, j].Checked)                
                    SkillButtonArray[skillIndex, j].BackgroundImage = SkillImgSBG[0];                
            }
        }

        private void Skillbutton_MouseLeave(object sender, EventArgs e)
        {
            CheckBox thisbutton = sender as CheckBox;


            int i = int.Parse(thisbutton.Name.Substring(11, 1));

            for (int j = 0; j <= int.Parse(thisbutton.Name.Substring(12, 1)); j++)
            {
                if (!SkillButtonArray[i, j].Checked)
                {
                    SkillButtonArray[i, j].BackgroundImage = SkillImgSBG[0];
                }
            }

            for (int j = int.Parse(thisbutton.Name.Substring(12, 1)); j < (IsBusMode ? 3 : 6); j++)
            {
                if (SkillButtonArray[i, j].Checked)
                {
                    SkillButtonArray[i, j].BackgroundImage = SkillImgSBG[3];
                }

            }
        }

        private void ADRbutton_Click(object sender, EventArgs e)
        {
            CheckBox thisbutton = sender as CheckBox;

            if (TextUtilities.ExtractFirstNumber(thisbutton.Name, out int number))
            {
                if (IsBusMode)
                {
                    ApplyCoachSkillLevel(0, (byte)number, thisbutton.Checked);
                    return;
                }

                char[] ADR = Convert.ToString(SiiNunitData.Economy._playerSkills[0], 2).PadLeft(6, '0').ToCharArray();

                Array.Reverse(ADR);

                if (thisbutton.Checked)
                    ADR[number] = '1';
                else
                    ADR[number] = '0';

                Array.Reverse(ADR);
                SiiNunitData.Economy._playerSkills[0] = Convert.ToByte(new string(ADR), 2);

                thisbutton.BackgroundImage = SkillImgSBG[1];
            }            
        }

        private void ADRbutton_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox thisbutton = sender as CheckBox;

            if (IsBusMode)
            {
                thisbutton.Image = null;
                thisbutton.BackgroundImage = SkillImgSBG[thisbutton.Checked ? 3 : 0];
                return;
            }

            byte adrIndex = byte.Parse(thisbutton.Name.Substring(9, 1));

            if (thisbutton.Checked)
            {
                thisbutton.BackgroundImage = SkillImgSBG[3];
                thisbutton.Image = ADRImgS[adrIndex];
            }
            else
            {
                thisbutton.BackgroundImage = SkillImgSBG[0];
                thisbutton.Image = ADRImgSGrey[adrIndex];
            }
        }

        private void ADRbutton_MouseEnter(object sender, EventArgs e)
        {
            CheckBox thisbutton = sender as CheckBox;

            thisbutton.BackgroundImage = SkillImgSBG[1];
        }

        private void ADRbutton_MouseHover(object sender, EventArgs e)
        {
            CheckBox thisbutton = sender as CheckBox;

            if (IsBusMode)
            {
                thisbutton.BackgroundImage = SkillImgSBG[1];
                return;
            }

            byte adrIndex = byte.Parse(thisbutton.Name.Substring(9, 1));

            if (thisbutton.Checked)
            {
                thisbutton.BackgroundImage = SkillImgSBG[1];
                thisbutton.Image = ADRImgS[adrIndex];
            }
            else
            {
                thisbutton.BackgroundImage = SkillImgSBG[1];
                thisbutton.Image = ADRImgSGrey[adrIndex];
            }
        }

        private void ADRbutton_MouseLeave(object sender, EventArgs e)
        {
            CheckBox thisbutton = sender as CheckBox;

            if (thisbutton.Checked)
                thisbutton.BackgroundImage = SkillImgSBG[3];
            else
                thisbutton.BackgroundImage = SkillImgSBG[0];
        }

        private void ApplyCoachSkillLevel(int skill, byte levelIndex, bool selected)
        {
            if (SiiNunitData == null)
                return;

            int targetLevel = selected ? levelIndex + 1 : levelIndex;
            if (skill == 0)
            {
                for (int point = 0; point < 3; point++)
                    ADRbuttonArray[point].Checked = point < targetLevel;
            }
            else
            {
                int row = skill - 1;
                for (int point = 0; point < 3; point++)
                    SkillButtonArray[row, point].Checked = point < targetLevel;
            }

            SetCoachSkill(SiiNunitData.Economy, skill, targetLevel);
            UpdateCoachSkillPointsLabel();
        }

        //end Skill buttons
        //end Profile tab
    }
}
