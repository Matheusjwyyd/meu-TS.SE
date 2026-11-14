using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

using TS_SE_Tool.Save.DataFormat;
using TS_SE_Tool.Save.Items;
using TS_SE_Tool.Save;
using TS_SE_Tool.Global;
using TS_SE_Tool.Utilities;

namespace TS_SE_Tool
{
    public partial class FormMain
    {
        private CheckBox[,] coachSkillButtonArray;
        private Label labelCoachLevelValue;
        private Label labelCoachExperience;
        private Label labelCoachAvailableSkillPoints;
        private ProgressBar progressCoachExperience;
        private NumericUpDown[] coachCareerStatistics;
        private Label labelCoachOdometer;
        private Button buttonCoachEditAccessories;
        private Panel[] coachPartProgressPanels;
        private Label[] coachPartNameLabels;
        private Button[] coachPartRepairButtons;
        private Button buttonCoachRepairAll;
        private Panel coachFuelProgressPanel;
        private Button buttonCoachRefuel;
        private bool refreshingCoachMode;
        private Control panelCoachBusGarage;

        private static readonly string[] CoachPartNames =
        {
            "Motor", "Transmissão", "Chassi", "Cabine", "Rodas"
        };

        private void CreateCoachModeControlsRefined()
        {
            TabPage garageHost = new TabPage
            {
                Name = "coachGarageHost",
                AutoScroll = true,
                Padding = new Padding(14)
            };
            BuildCoachVehiclePage(garageHost);
            panelCoachBusGarage = garageHost.Controls.Count > 0 ? garageHost.Controls[0] : null;
            if (panelCoachBusGarage != null)
            {
                garageHost.Controls.Remove(panelCoachBusGarage);
                panelCoachBusGarage.Name = "panelCoachBusGarage";
                panelCoachBusGarage.Dock = DockStyle.Fill;
            }

            BuildCoachStatisticsPage();
            tabPageCoachMode.Controls.Add(panelCoachBusGarage);
            panelCoachBusGarage.Visible = false;
        }

        private void BuildCoachStatisticsPage()
        {
            tabPageCoachMode.Controls.Clear();
            FlowLayoutPanel flow = CreateCoachFlowPanel();
            tabPageCoachMode.Controls.Add(flow);
            flow.Controls.Add(CreateCoachHeading("Estatísticas do ônibus",
                "Dados da carreira de ônibus deste perfil ETS 2."));

            GroupBox statisticsGroup = CreateCoachGroup("Estatísticas da carreira", 258);
            TableLayoutPanel statisticsGrid = CreateCoachGrid(2, 4);
            statisticsGroup.Controls.Add(statisticsGrid);
            coachCareerStatistics = new NumericUpDown[CoachStatisticNames.Length];
            for (int i = 0; i < CoachStatisticNames.Length; i++)
            {
                coachCareerStatistics[i] = CreateCoachNumberInput();
                coachCareerStatistics[i].ValueChanged += CoachCareerStatisticChanged;
                AddCoachGridField(statisticsGrid, i % 2, i / 2, CoachStatisticNames[i], coachCareerStatistics[i]);
            }
            flow.Controls.Add(statisticsGroup);
        }

        private void ApplyCoachGarageMode(bool busMode)
        {
            if (panelCoachBusGarage == null)
                return;

            if (busMode)
            {
                if (tableLayoutPanel9.Parent == tabPageTruck)
                    tabPageTruck.Controls.Remove(tableLayoutPanel9);
                if (panelCoachBusGarage.Parent != null)
                    panelCoachBusGarage.Parent.Controls.Remove(panelCoachBusGarage);
                if (panelCoachBusGarage.Parent != tabPageTruck)
                    tabPageTruck.Controls.Add(panelCoachBusGarage);
                panelCoachBusGarage.Dock = DockStyle.Fill;
                panelCoachBusGarage.Visible = true;
                UpdateCoachVehicleDisplay();
                ModernTheme.ApplyNow(this);
            }
            else
            {
                if (panelCoachBusGarage.Parent == tabPageTruck)
                    tabPageTruck.Controls.Remove(panelCoachBusGarage);
                if (panelCoachBusGarage.Parent != tabPageCoachMode)
                    tabPageCoachMode.Controls.Add(panelCoachBusGarage);
                panelCoachBusGarage.Visible = false;
                if (tableLayoutPanel9.Parent != tabPageTruck)
                    tabPageTruck.Controls.Add(tableLayoutPanel9);
                tableLayoutPanel9.Dock = DockStyle.Fill;
            }
        }

        private void BuildCoachCareerPage(TabPage page)
        {
            FlowLayoutPanel flow = CreateCoachFlowPanel();
            page.Controls.Add(flow);
            flow.Controls.Add(CreateCoachHeading("Modo ônibus do ETS 2",
                "Nível, XP e habilidades próprios da carreira de ônibus."));

            GroupBox levelGroup = CreateCoachGroup("Nível do jogador", 112);
            TableLayoutPanel levelLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                Padding = new Padding(6, 3, 6, 2)
            };
            levelLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F));
            levelLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            levelGroup.Controls.Add(levelLayout);

            FlowLayoutPanel levelButtons = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                Margin = Padding.Empty
            };
            levelButtons.Controls.Add(CreateCoachLevelButton("<< MIN", 64, () => SetCoachLevel(0)));
            levelButtons.Controls.Add(CreateCoachLevelButton("- 10", 48, () => ChangeCoachLevel(-10)));
            levelButtons.Controls.Add(CreateCoachLevelButton("- 1", 44, () => ChangeCoachLevel(-1)));
            labelCoachLevelValue = new Label
            {
                Name = "labelCoachLevelValue",
                Text = "Nível 0",
                Width = 102,
                Height = 30,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Margin = new Padding(2, 1, 2, 1)
            };
            levelButtons.Controls.Add(labelCoachLevelValue);
            levelButtons.Controls.Add(CreateCoachLevelButton("+ 1", 44, () => ChangeCoachLevel(1)));
            levelButtons.Controls.Add(CreateCoachLevelButton("+ 10", 48, () => ChangeCoachLevel(10)));
            levelButtons.Controls.Add(CreateCoachLevelButton("MAX >>", 64, () => SetCoachLevel(150)));
            levelLayout.Controls.Add(levelButtons, 0, 0);

            TableLayoutPanel xpLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                Margin = new Padding(2, 0, 2, 0)
            };
            xpLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 48F));
            xpLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 52F));
            labelCoachExperience = new Label
            {
                Name = "labelCoachExperience",
                Text = "0 / 0 EXP",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            };
            progressCoachExperience = new ProgressBar
            {
                Name = "progressCoachExperience",
                Dock = DockStyle.Fill,
                Minimum = 0,
                Maximum = 1000,
                Height = 18,
                Style = ProgressBarStyle.Continuous
            };
            xpLayout.Controls.Add(labelCoachExperience, 0, 0);
            xpLayout.Controls.Add(progressCoachExperience, 1, 0);
            levelLayout.Controls.Add(xpLayout, 0, 1);
            flow.Controls.Add(levelGroup);

            GroupBox skillsGroup = CreateCoachGroup("Habilidades", 414);
            TableLayoutPanel skillLayout = new TableLayoutPanel
            {
                Name = "tableLayoutCoachSkills",
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = CoachSkillNames.Length + 1,
                Padding = new Padding(8, 4, 8, 4)
            };
            skillLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 56F));
            skillLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            skillLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 156F));
            skillLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
            for (int i = 0; i < CoachSkillNames.Length; i++)
                skillLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 58F));
            skillsGroup.Controls.Add(skillLayout);

            labelCoachAvailableSkillPoints = new Label
            {
                Name = "labelCoachAvailableSkillPoints",
                Text = "Pontos de habilidade: 0",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleRight,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };
            skillLayout.Controls.Add(labelCoachAvailableSkillPoints, 0, 0);
            skillLayout.SetColumnSpan(labelCoachAvailableSkillPoints, 3);

            coachSkillButtonArray = new CheckBox[CoachSkillNames.Length, 3];
            for (int skill = 0; skill < CoachSkillNames.Length; skill++)
            {
                PictureBox icon = new PictureBox
                {
                    Name = "pictureCoachSkill" + skill,
                    Size = new Size(46, 46),
                    SizeMode = PictureBoxSizeMode.CenterImage,
                    BackgroundImage = SkillImgSBG[0],
                    BackgroundImageLayout = ImageLayout.Stretch,
                    Image = CreateCoachSkillIcon(skill),
                    Margin = new Padding(2, 4, 4, 2)
                };
                skillLayout.Controls.Add(icon, 0, skill + 1);

                Label name = new Label
                {
                    Name = "labelCoachSkill" + skill,
                    Text = CoachSkillNames[skill],
                    Dock = DockStyle.Fill,
                    AutoEllipsis = true,
                    TextAlign = ContentAlignment.MiddleLeft,
                    Font = new Font("Segoe UI", 9F, FontStyle.Bold)
                };
                skillLayout.Controls.Add(name, 1, skill + 1);

                FlowLayoutPanel pips = new FlowLayoutPanel
                {
                    Name = "flowCoachSkillPips" + skill,
                    Dock = DockStyle.Fill,
                    FlowDirection = FlowDirection.LeftToRight,
                    WrapContents = false,
                    Margin = new Padding(0, 7, 0, 0)
                };
                for (int level = 0; level < 3; level++)
                {
                    CheckBox pip = new CheckBox
                    {
                        Appearance = Appearance.Button,
                        FlatStyle = FlatStyle.Flat,
                        Size = new Size(43, 38),
                        Margin = new Padding(2, 0, 3, 0),
                        Name = "buttonCoachSkill" + skill + level,
                        Tag = new Point(skill, level),
                        BackgroundImage = SkillImgSBG[0],
                        BackgroundImageLayout = ImageLayout.Stretch,
                        Text = "",
                        TextAlign = ContentAlignment.MiddleCenter,
                        Padding = new Padding(0)
                    };
                    pip.FlatAppearance.BorderSize = 0;
                    pip.AccessibleName = CoachSkillNames[skill] + " — nível " + (level + 1);
                    toolTipMain.SetToolTip(pip, pip.AccessibleName);
                    pip.CheckedChanged += CoachSkillPipCheckedChanged;
                    pip.MouseEnter += CoachSkillPipMouseEnter;
                    pip.MouseLeave += CoachSkillPipMouseLeave;
                    pip.Click += CoachSkillPipClick;
                    coachSkillButtonArray[skill, level] = pip;
                    pips.Controls.Add(pip);
                }
                skillLayout.Controls.Add(pips, 2, skill + 1);
            }
            flow.Controls.Add(skillsGroup);

            GroupBox statisticsGroup = CreateCoachGroup("Estatísticas da carreira", 258);
            TableLayoutPanel statisticsGrid = CreateCoachGrid(2, 4);
            statisticsGroup.Controls.Add(statisticsGrid);
            coachCareerStatistics = new NumericUpDown[CoachStatisticNames.Length];
            for (int i = 0; i < CoachStatisticNames.Length; i++)
            {
                coachCareerStatistics[i] = CreateCoachNumberInput();
                coachCareerStatistics[i].ValueChanged += CoachCareerStatisticChanged;
                AddCoachGridField(statisticsGrid, i % 2, i / 2, CoachStatisticNames[i], coachCareerStatistics[i]);
            }
            flow.Controls.Add(statisticsGroup);
        }

        private void BuildCoachVehiclePage(TabPage page)
        {
            FlowLayoutPanel flow = CreateCoachFlowPanel();
            page.Controls.Add(flow);
            flow.Controls.Add(CreateCoachHeading("Garagem de ônibus",
                "Selecione um ônibus para editar seus componentes, reparar danos e abastecer."));

            GroupBox selectionGroup = CreateCoachGroup("Ônibus selecionado", 82);
            TableLayoutPanel selectionLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                Padding = new Padding(8, 4, 8, 4)
            };
            selectionLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 57F));
            selectionLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 43F));
            selectionGroup.Controls.Add(selectionLayout);

            comboBoxCoachVehicle = new ComboBox
            {
                Name = "comboBoxCoachVehicle",
                Dock = DockStyle.Fill,
                DropDownStyle = ComboBoxStyle.DropDownList,
                FormattingEnabled = true
            };
            comboBoxCoachVehicle.SelectedIndexChanged += CoachVehicleSelectionChangedRefined;
            selectionLayout.Controls.Add(comboBoxCoachVehicle, 0, 0);
            labelCoachVehicleModel = new Label
            {
                Name = "labelCoachVehicleModel",
                Text = "Nenhum ônibus próprio encontrado neste save.",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                AutoEllipsis = true
            };
            selectionLayout.Controls.Add(labelCoachVehicleModel, 1, 0);
            flow.Controls.Add(selectionGroup);

            FlowLayoutPanel actions = new FlowLayoutPanel
            {
                AutoSize = true,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                Margin = new Padding(0, 0, 0, 8)
            };
            buttonCoachRepairAll = CreateCoachImageButton("buttonCoachRepairWear", RepairImg, "Reparar o ônibus inteiro");
            buttonCoachRepairAll.Click += CoachRepairAllClick;
            buttonCoachRepairAll.EnabledChanged += CoachActionButtonEnabledChanged;
            actions.Controls.Add(buttonCoachRepairAll);

            buttonCoachRefuel = CreateCoachImageButton("buttonCoachRefuel", RefuelImg, "Abastecer o ônibus");
            buttonCoachRefuel.Click += CoachRefuelClick;
            buttonCoachRefuel.EnabledChanged += CoachActionButtonEnabledChanged;
            actions.Controls.Add(buttonCoachRefuel);

            buttonCoachEditAccessories = CreateCoachImageButton("buttonCoachEditAccessories", CustomizeImg, "Editar componentes do ônibus");
            buttonCoachEditAccessories.Click += CoachEditAccessoriesClickRefined;
            actions.Controls.Add(buttonCoachEditAccessories);
            flow.Controls.Add(actions);

            GroupBox detailsGroup = CreateCoachGroup("Detalhes", 384);
            TableLayoutPanel detailsLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                Padding = new Padding(6, 6, 6, 6)
            };
            detailsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            detailsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 96F));
            detailsGroup.Controls.Add(detailsLayout);

            TableLayoutPanel partsLayout = new TableLayoutPanel
            {
                Name = "tableLayoutCoachVehicleParts",
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 5,
                Margin = new Padding(0, 2, 6, 0)
            };
            partsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 42F));
            partsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            partsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 40F));
            for (int i = 0; i < 5; i++)
                partsLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 20F));
            detailsLayout.Controls.Add(partsLayout, 0, 0);

            coachPartProgressPanels = new Panel[CoachPartNames.Length];
            coachPartNameLabels = new Label[CoachPartNames.Length];
            coachPartRepairButtons = new Button[CoachPartNames.Length];
            for (int i = 0; i < CoachPartNames.Length; i++)
            {
                PictureBox partIcon = new PictureBox
                {
                    Name = "pictureCoachPart" + i,
                    Dock = DockStyle.Fill,
                    SizeMode = PictureBoxSizeMode.Zoom,
                    Image = TruckPartsImg[i],
                    Margin = new Padding(2, 5, 2, 5)
                };
                partsLayout.Controls.Add(partIcon, 0, i);

                TableLayoutPanel partInfo = new TableLayoutPanel
                {
                    Name = "tableLayoutCoachPart" + i,
                    Dock = DockStyle.Fill,
                    ColumnCount = 1,
                    RowCount = 2,
                    Margin = new Padding(2, 4, 4, 4)
                };
                partInfo.RowStyles.Add(new RowStyle(SizeType.Absolute, 19F));
                partInfo.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
                partsLayout.Controls.Add(partInfo, 1, i);

                coachPartNameLabels[i] = new Label
                {
                    Name = "labelCoachPartDataName" + i,
                    Text = CoachPartNames[i],
                    Dock = DockStyle.Fill,
                    AutoEllipsis = true
                };
                partInfo.Controls.Add(coachPartNameLabels[i], 0, 0);

                Panel progress = new Panel
                {
                    Name = "progressbarCoachPart" + i,
                    Dock = DockStyle.Fill,
                    BorderStyle = BorderStyle.FixedSingle,
                    Margin = new Padding(0, 1, 0, 0)
                };
                coachPartProgressPanels[i] = progress;
                partInfo.Controls.Add(progress, 0, 1);

                Button repairPart = CreateCoachImageButton("buttonCoachPartRepair" + i, RepairImg, "Reparar " + CoachPartNames[i]);
                repairPart.Tag = i;
                repairPart.Dock = DockStyle.Fill;
                repairPart.Click += CoachRepairPartClick;
                repairPart.EnabledChanged += CoachActionButtonEnabledChanged;
                coachPartRepairButtons[i] = repairPart;
                partsLayout.Controls.Add(repairPart, 2, i);
            }

            TableLayoutPanel fuelLayout = new TableLayoutPanel
            {
                Name = "tableLayoutCoachFuel",
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                Margin = new Padding(4, 0, 0, 0)
            };
            fuelLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 24F));
            fuelLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            detailsLayout.Controls.Add(fuelLayout, 1, 0);
            Label fuelTitle = new Label
            {
                Name = "labelCoachFuel",
                Text = "Combustível",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter
            };
            fuelLayout.Controls.Add(fuelTitle, 0, 0);
            coachFuelProgressPanel = new Panel
            {
                Name = "progressbarCoachFuel",
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.FixedSingle,
                Margin = new Padding(4, 0, 4, 2)
            };
            fuelLayout.Controls.Add(coachFuelProgressPanel, 0, 1);
            flow.Controls.Add(detailsGroup);

            labelCoachOdometer = new Label
            {
                Name = "labelCoachOdometer",
                Text = "Odômetro: —",
                AutoSize = true,
                Margin = new Padding(2, 0, 0, 8)
            };
            flow.Controls.Add(labelCoachOdometer);
        }

        private Button CreateCoachLevelButton(string text, int width, Action action)
        {
            Button button = new Button
            {
                Text = text,
                Width = width,
                Height = 28,
                Margin = new Padding(2, 1, 2, 1),
                FlatStyle = FlatStyle.Flat
            };
            button.Click += (sender, args) => action();
            return button;
        }

        private Button CreateCoachImageButton(string name, Image image, string tooltip)
        {
            Button button = new Button
            {
                Name = name,
                Size = new Size(46, 46),
                FlatStyle = FlatStyle.Flat,
                BackgroundImage = image,
                BackgroundImageLayout = ImageLayout.Zoom,
                Text = "",
                Margin = new Padding(3)
            };
            button.FlatAppearance.BorderSize = 0;
            toolTipMain.SetToolTip(button, tooltip);
            return button;
        }

        private Bitmap CreateCoachSkillIcon(int skill)
        {
            Bitmap icon = new Bitmap(38, 38);
            using (Graphics g = Graphics.FromImage(icon))
            using (Pen pen = new Pen(Color.FromArgb(79, 88, 98), 3F))
            using (SolidBrush brush = new SolidBrush(Color.FromArgb(79, 88, 98)))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.Clear(Color.Transparent);
                switch (skill)
                {
                    case 0: // Route
                        g.DrawBezier(pen, 4, 29, 12, 4, 24, 35, 33, 8);
                        g.FillEllipse(brush, 2, 26, 7, 7);
                        g.FillEllipse(brush, 29, 5, 7, 7);
                        break;
                    case 1: // Schedule
                        g.DrawEllipse(pen, 7, 8, 24, 24);
                        g.DrawLine(pen, 19, 12, 19, 20);
                        g.DrawLine(pen, 19, 20, 25, 23);
                        g.DrawLine(pen, 14, 4, 24, 4);
                        g.DrawLine(pen, 19, 4, 19, 8);
                        break;
                    case 2: // Premium service
                        g.DrawEllipse(pen, 5, 4, 28, 28);
                        g.FillPolygon(brush, CoachStarPoints(19, 18, 11, 5));
                        break;
                    case 3: // Comfort
                        g.DrawBezier(pen, 7, 30, 8, 18, 17, 8, 30, 6);
                        g.DrawBezier(pen, 30, 6, 31, 18, 21, 29, 7, 30);
                        g.DrawLine(pen, 9, 28, 27, 9);
                        break;
                    case 4: // Passengers
                        using (GraphicsPath heart = new GraphicsPath())
                        {
                            heart.AddBezier(19, 32, 14, 26, 4, 19, 5, 12);
                            heart.AddBezier(5, 12, 6, 4, 16, 4, 19, 12);
                            heart.AddBezier(19, 12, 23, 4, 33, 5, 33, 13);
                            heart.AddBezier(33, 13, 33, 20, 24, 27, 19, 32);
                            g.DrawPath(pen, heart);
                        }
                        break;
                    default: // Economical driving
                        g.DrawBezier(pen, 5, 29, 6, 14, 20, 5, 32, 6);
                        g.DrawBezier(pen, 5, 29, 20, 31, 31, 19, 32, 6);
                        g.DrawLine(pen, 8, 27, 27, 9);
                        break;
                }
            }
            return icon;
        }

        private static PointF[] CoachStarPoints(float centerX, float centerY, float outerRadius, float innerRadius)
        {
            PointF[] points = new PointF[10];
            for (int i = 0; i < points.Length; i++)
            {
                double angle = -Math.PI / 2 + i * Math.PI / 5;
                float radius = i % 2 == 0 ? outerRadius : innerRadius;
                points[i] = new PointF(centerX + (float)Math.Cos(angle) * radius,
                    centerY + (float)Math.Sin(angle) * radius);
            }
            return points;
        }

        private void FillCoachPanelControlsRefined()
        {
            if (SiiNunitData == null || coachCareerStatistics == null)
            {
                ClearCoachPanelControlsRefined();
                return;
            }

            refreshingCoachMode = true;
            Economy economy = SiiNunitData.Economy;
            int[] values =
            {
                economy.bus_total_distance,
                economy.bus_finished_job_count,
                economy.bus_cancelled_job_count,
                economy.bus_total_passengers,
                economy.bus_total_stops,
                economy.bus_game_time,
                economy.bus_playing_time
            };
            for (int i = 0; i < coachCareerStatistics.Length; i++)
                SetCoachNumber(coachCareerStatistics[i], values[i]);

            List<CoachVehicleOption> coaches = UserBusDictionary
                .OrderBy(item => item.Key, StringComparer.Ordinal)
                .Select((item, index) => new CoachVehicleOption(item.Key,
                    "Ônibus " + (index + 1) + " — " + GetCoachModelName(item.Key, item.Value)))
                .ToList();
            comboBoxCoachVehicle.DataSource = null;
            comboBoxCoachVehicle.DisplayMember = "Name";
            comboBoxCoachVehicle.ValueMember = "Id";
            comboBoxCoachVehicle.DataSource = coaches;
            refreshingCoachMode = false;

            UpdateCoachVehicleDisplay();
            UpdateCoachModeGameAvailability();
        }

        private void ClearCoachPanelControlsRefined()
        {
            refreshingCoachMode = true;
            if (coachCareerStatistics != null)
                foreach (NumericUpDown input in coachCareerStatistics)
                    input.Value = 0;
            if (comboBoxCoachVehicle != null)
                comboBoxCoachVehicle.DataSource = null;
            refreshingCoachMode = false;
            UpdateCoachVehicleDisplay();
        }

        private static void SetCoachNumber(NumericUpDown input, int value)
        {
            decimal number = value;
            if (number < input.Minimum) number = input.Minimum;
            if (number > input.Maximum) number = input.Maximum;
            input.Value = number;
        }

        private void UpdateCoachLevelDisplay()
        {
            if (labelCoachLevelValue == null)
                return;

            if (SiiNunitData == null)
            {
                labelCoachLevelValue.Text = "Nível 0";
                labelCoachExperience.Text = "0 / 0 EXP";
                progressCoachExperience.Value = 0;
                labelCoachAvailableSkillPoints.Text = "Pontos de habilidade: 0";
                return;
            }

            Economy economy = SiiNunitData.Economy;
            int[] level = economy.getBusPlayerLvl();
            int currentLevel = level[0];
            int nextThreshold = level[1];
            int previousThreshold = 0;
            if (currentLevel > 0)
            {
                for (int i = 0; i < currentLevel; i++)
                    previousThreshold += i < Globals.PlayerLevelUps.Length
                        ? Globals.PlayerLevelUps[i]
                        : Globals.PlayerLevelUps[Globals.PlayerLevelUps.Length - 1];
            }
            int levelProgress = Math.Max(0, economy.bus_experience_points - previousThreshold);
            int levelRequirement = Math.Max(1, nextThreshold - previousThreshold);
            labelCoachLevelValue.Text = "Nível " + currentLevel;
            labelCoachExperience.Text = levelProgress.ToString("N0") + " / " + levelRequirement.ToString("N0") + " EXP";
            toolTipMain.SetToolTip(labelCoachExperience,
                "XP total: " + economy.bus_experience_points.ToString("N0") + " | Próximo nível em " + nextThreshold.ToString("N0") + " XP");
            progressCoachExperience.Maximum = Math.Max(1, levelRequirement);
            progressCoachExperience.Value = Math.Min(progressCoachExperience.Maximum, levelProgress);

            int spentPoints = economy.long_duty + economy.tight_schedule + economy.satisfaction_decrease +
                economy.low_acceleration + economy.satisfaction_increase + economy.ecodriving;
            int availablePoints = currentLevel - spentPoints;
            labelCoachAvailableSkillPoints.Text = "Pontos de habilidade: " + availablePoints;
            labelCoachAvailableSkillPoints.ForeColor = availablePoints < 0
                ? ModernTheme.Danger
                : ModernTheme.PrimaryGray;
        }

        private void ChangeCoachLevel(int delta)
        {
            if (SiiNunitData == null)
                return;
            int current = SiiNunitData.Economy.getBusPlayerLvl()[0];
            SetCoachLevel(current + delta);
        }

        private void SetCoachLevel(int level)
        {
            if (SiiNunitData == null)
                return;
            SiiNunitData.Economy.setBusPlayerExp(level);
            UpdateCoachLevelDisplay();
        }

        private void CoachSkillPipClick(object sender, EventArgs e)
        {
            if (refreshingCoachMode || SiiNunitData == null)
                return;

            CheckBox pip = sender as CheckBox;
            if (pip == null || !(pip.Tag is Point))
                return;

            Point position = (Point)pip.Tag;
            int newLevel = pip.Checked ? position.Y + 1 : position.Y;
            for (int i = 0; i < 3; i++)
                coachSkillButtonArray[position.X, i].Checked = i < newLevel;
            SetCoachSkill(SiiNunitData.Economy, position.X, newLevel);
            UpdateCoachLevelDisplay();
        }

        private void CoachSkillPipCheckedChanged(object sender, EventArgs e)
        {
            CheckBox pip = sender as CheckBox;
            if (pip != null)
                pip.BackgroundImage = SkillImgSBG[pip.Checked ? 3 : 0];
        }

        private void CoachSkillPipMouseEnter(object sender, EventArgs e)
        {
            CheckBox pip = sender as CheckBox;
            if (pip == null || !(pip.Tag is Point))
                return;
            Point position = (Point)pip.Tag;
            for (int i = 0; i <= position.Y; i++)
                if (!coachSkillButtonArray[position.X, i].Checked)
                    coachSkillButtonArray[position.X, i].BackgroundImage = SkillImgSBG[1];
        }

        private void CoachSkillPipMouseLeave(object sender, EventArgs e)
        {
            CheckBox pip = sender as CheckBox;
            if (pip == null || !(pip.Tag is Point))
                return;
            Point position = (Point)pip.Tag;
            for (int i = 0; i < 3; i++)
                coachSkillButtonArray[position.X, i].BackgroundImage = SkillImgSBG[
                    coachSkillButtonArray[position.X, i].Checked ? 3 : 0];
        }

        private static void SetCoachSkill(Economy economy, int skill, int level)
        {
            switch (skill)
            {
                case 0: economy.long_duty = level; break;
                case 1: economy.tight_schedule = level; break;
                case 2: economy.satisfaction_decrease = level; break;
                case 3: economy.low_acceleration = level; break;
                case 4: economy.satisfaction_increase = level; break;
                case 5: economy.ecodriving = level; break;
            }
        }

        private void CoachCareerStatisticChanged(object sender, EventArgs e)
        {
            if (refreshingCoachMode || SiiNunitData == null)
                return;
            Economy economy = SiiNunitData.Economy;
            economy.bus_total_distance = (int)coachCareerStatistics[0].Value;
            economy.bus_finished_job_count = (int)coachCareerStatistics[1].Value;
            economy.bus_cancelled_job_count = (int)coachCareerStatistics[2].Value;
            economy.bus_total_passengers = (int)coachCareerStatistics[3].Value;
            economy.bus_total_stops = (int)coachCareerStatistics[4].Value;
            economy.bus_game_time = (int)coachCareerStatistics[5].Value;
            economy.bus_playing_time = (int)coachCareerStatistics[6].Value;
        }

        private void CoachVehicleSelectionChangedRefined(object sender, EventArgs e)
        {
            if (!refreshingCoachMode)
                UpdateCoachVehicleDisplay();
        }

        private void UpdateCoachVehicleDisplay()
        {
            Vehicle coach = GetSelectedCoach();
            bool available = coach != null && SiiNunitData != null && GameType == "ETS2";
            if (available)
            {
                labelCoachVehicleModel.Text = GetCoachModelName(comboBoxCoachVehicle.SelectedValue.ToString(), coach);
                labelCoachOdometer.Text = "Odômetro: " + coach.odometer.ToString("N0") + " km";
            }
            else
            {
                labelCoachVehicleModel.Text = UserBusDictionary == null || UserBusDictionary.Count == 0
                    ? "Nenhum ônibus próprio encontrado neste save."
                    : "Selecione um ônibus para editar.";
                labelCoachOdometer.Text = "Odômetro: —";
            }

            if (coachPartRepairButtons != null)
                foreach (Button button in coachPartRepairButtons)
                    button.Enabled = available;
            if (buttonCoachRepairAll != null)
                buttonCoachRepairAll.Enabled = false;
            if (buttonCoachRefuel != null)
                buttonCoachRefuel.Enabled = available;
            if (buttonCoachEditAccessories != null)
                buttonCoachEditAccessories.Enabled = available;

            if (!available)
            {
                if (coachPartProgressPanels != null)
                    foreach (Panel panel in coachPartProgressPanels)
                        panel.BackgroundImage = null;
                if (coachFuelProgressPanel != null)
                    coachFuelProgressPanel.BackgroundImage = null;
                return;
            }

            for (byte part = 0; part < CoachPartNames.Length; part++)
                UpdateCoachPartProgress(part, coach);
            buttonCoachRepairAll.Enabled = coachPartRepairButtons.Any(button => button.Enabled);
            UpdateCoachFuelProgress(coach);
        }

        private void UpdateCoachPartProgress(byte part, Vehicle coach)
        {
            float wear = 0F, unfixable = 0F, permanent = 0F;
            string partType = "";
            switch (part)
            {
                case 0:
                    partType = "engine";
                    wear = coach.engine_wear;
                    if (MainSaveFileInfoData.Version > (byte)saveVTV.v148) unfixable = coach.engine_wear_unfixable;
                    break;
                case 1:
                    partType = "transmission";
                    wear = coach.transmission_wear;
                    if (MainSaveFileInfoData.Version > (byte)saveVTV.v148) unfixable = coach.transmission_wear_unfixable;
                    break;
                case 2:
                    partType = "chassis";
                    wear = coach.chassis_wear;
                    if (MainSaveFileInfoData.Version > (byte)saveVTV.v148) unfixable = coach.chassis_wear_unfixable;
                    break;
                case 3:
                    partType = "cabin";
                    wear = coach.cabin_wear;
                    if (MainSaveFileInfoData.Version > (byte)saveVTV.v148) unfixable = coach.cabin_wear_unfixable;
                    break;
                case 4:
                    partType = "tire";
                    if (coach.wheels_wear.Count > 0) wear = coach.wheels_wear.Average(value => (float)value);
                    if (MainSaveFileInfoData.Version > (byte)saveVTV.v148 && coach.wheels_wear_unfixable.Count > 0)
                        unfixable = coach.wheels_wear_unfixable.Average(value => (float)value);
                    break;
            }
            permanent = (float)coach.integrity_odometer / 5000000F;
            wear = Math.Max(0, wear);
            unfixable = Math.Max(0, unfixable);
            permanent = Math.Max(0, permanent);
            float total = Math.Min(1F, wear + unfixable + permanent);

            string partName = "";
            foreach (string accessoryId in coach.accessories)
            {
                if (!SiiNunitData.SiiNitems.ContainsKey(accessoryId))
                    continue;
                Vehicle_Accessory accessory = SiiNunitData.SiiNitems[accessoryId] as Vehicle_Accessory;
                if (partType != "tire" && accessory != null && accessory.accType == partType)
                {
                    partName = GetAccessoryShortName(accessory.data_path);
                    break;
                }
                Vehicle_Wheel_Accessory wheel = SiiNunitData.SiiNitems[accessoryId] as Vehicle_Wheel_Accessory;
                if (partType == "tire" && wheel != null && wheel.accType == "tire")
                {
                    if (partName.Length > 0) partName += " | ";
                    partName += GetAccessoryShortName(wheel.data_path);
                }
            }
            coachPartNameLabels[part].Text = partName.Length == 0 ? CoachPartNames[part] : CoachPartNames[part] + " — " + partName;
            toolTipMain.SetToolTip(coachPartNameLabels[part], coachPartNameLabels[part].Text);

            Button repairButton = coachPartRepairButtons[part];
            repairButton.Enabled = total > 0.0001F;
            Panel bar = coachPartProgressPanels[part];
            if (bar.Width < 2 || bar.Height < 2)
                return;

            Bitmap bitmap = new Bitmap(bar.Width, bar.Height);
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                int healthyWidth = (int)(bar.Width * (1F - total));
                int fixableWidth = Math.Min(bar.Width - healthyWidth, (int)(bar.Width * wear));
                int unfixableWidth = Math.Min(bar.Width - healthyWidth - fixableWidth, (int)(bar.Width * unfixable));
                int permanentWidth = Math.Max(0, bar.Width - healthyWidth - fixableWidth - unfixableWidth);
                using (SolidBrush healthy = new SolidBrush(Graphics_TSSET.GetProgressbarColor(total)))
                    g.FillRectangle(healthy, 0, 0, healthyWidth, bar.Height);
                if (fixableWidth > 0)
                {
                    using (SolidBrush fixableBrush = new SolidBrush(Color.Yellow))
                        g.FillRectangle(fixableBrush, healthyWidth, 0, fixableWidth, bar.Height);
                    using (TextureBrush texture = new TextureBrush(VehicleIntegrityPBImg[0], WrapMode.Tile))
                        g.FillRectangle(texture, healthyWidth, 0, fixableWidth, bar.Height);
                }
                if (unfixableWidth > 0)
                {
                    using (SolidBrush fixedBrush = new SolidBrush(Color.Orange))
                        g.FillRectangle(fixedBrush, healthyWidth + fixableWidth, 0, unfixableWidth, bar.Height);
                    using (TextureBrush texture = new TextureBrush(VehicleIntegrityPBImg[1], WrapMode.Tile))
                        g.FillRectangle(texture, healthyWidth + fixableWidth, 0, unfixableWidth, bar.Height);
                }
                if (permanentWidth > 0)
                {
                    using (SolidBrush permanentBrush = new SolidBrush(Color.Red))
                        g.FillRectangle(permanentBrush, healthyWidth + fixableWidth + unfixableWidth, 0, permanentWidth, bar.Height);
                    using (TextureBrush texture = new TextureBrush(VehicleIntegrityPBImg[2], WrapMode.Tile))
                        g.FillRectangle(texture, healthyWidth + fixableWidth + unfixableWidth, 0, permanentWidth, bar.Height);
                }
                string percent = ((int)((1F - total) * 100F)).ToString() + " %";
                using (Font font = new Font(FontFamily.GenericSansSerif, 10F, FontStyle.Bold))
                {
                    SizeF text = g.MeasureString(percent, font);
                    using (SolidBrush bg = new SolidBrush(Color.FromArgb(190, Color.White)))
                        g.FillRectangle(bg, (bar.Width - text.Width) / 2, (bar.Height - text.Height) / 2, text.Width, text.Height);
                    using (StringFormat format = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                        g.DrawString(percent, font, Brushes.Black, new RectangleF(0, 0, bar.Width, bar.Height), format);
                }
            }
            Image previous = bar.BackgroundImage;
            bar.BackgroundImage = bitmap;
            if (previous != null) previous.Dispose();
        }

        private void UpdateCoachFuelProgress(Vehicle coach)
        {
            if (coachFuelProgressPanel.Width < 2 || coachFuelProgressPanel.Height < 2)
                return;
            float fuel = Math.Max(0F, Math.Min(1F, (float)coach.fuel_relative));
            buttonCoachRefuel.Enabled = fuel < 0.9999F;
            Bitmap bitmap = new Bitmap(coachFuelProgressPanel.Width, coachFuelProgressPanel.Height);
            using (Graphics g = Graphics.FromImage(bitmap))
            using (SolidBrush fill = new SolidBrush(Graphics_TSSET.GetProgressbarColor(1F - fuel)))
            using (Font font = new Font(FontFamily.GenericSansSerif, 10F, FontStyle.Bold))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                int height = (int)(coachFuelProgressPanel.Height * fuel);
                g.FillRectangle(fill, 0, coachFuelProgressPanel.Height - height, coachFuelProgressPanel.Width, height);
                string percent = ((int)(fuel * 100F)).ToString() + " %";
                using (StringFormat format = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                    g.DrawString(percent, font, Brushes.Black,
                        new RectangleF(0, 0, coachFuelProgressPanel.Width, coachFuelProgressPanel.Height), format);
            }
            Image previous = coachFuelProgressPanel.BackgroundImage;
            coachFuelProgressPanel.BackgroundImage = bitmap;
            if (previous != null) previous.Dispose();
        }

        private static string GetAccessoryShortName(string dataPath)
        {
            if (String.IsNullOrEmpty(dataPath)) return "";
            string path = dataPath.Trim('"');
            string name = path.Split('/').LastOrDefault() ?? path;
            int extension = name.LastIndexOf('.');
            if (extension > 0) name = name.Substring(0, extension);
            return name.Replace('_', ' ').Replace('.', ' ');
        }

        private void CoachRefuelClick(object sender, EventArgs e)
        {
            Vehicle coach = GetSelectedCoach();
            if (coach == null) return;
            coach.fuel_relative = 1;
            UpdateCoachFuelProgress(coach);
        }

        private void CoachRepairAllClick(object sender, EventArgs e)
        {
            Vehicle coach = GetSelectedCoach();
            if (coach == null) return;
            coach.engine_wear = 0;
            coach.transmission_wear = 0;
            coach.chassis_wear = 0;
            coach.cabin_wear = 0;
            coach.wheels_wear = new List<SCS_Float>();
            coach.engine_wear_unfixable = 0;
            coach.transmission_wear_unfixable = 0;
            coach.chassis_wear_unfixable = 0;
            coach.cabin_wear_unfixable = 0;
            coach.wheels_wear_unfixable = new List<SCS_Float>();
            coach.integrity_odometer = 0;
            coach.integrity_odometer_float_part = 0;
            UpdateCoachVehicleDisplay();
        }

        private void CoachRepairPartClick(object sender, EventArgs e)
        {
            Button button = sender as Button;
            Vehicle coach = GetSelectedCoach();
            if (button == null || coach == null || !(button.Tag is int)) return;
            int part = (int)button.Tag;
            switch (part)
            {
                case 0:
                    coach.engine_wear = 0;
                    coach.engine_wear_unfixable = 0;
                    break;
                case 1:
                    coach.transmission_wear = 0;
                    coach.transmission_wear_unfixable = 0;
                    break;
                case 2:
                    coach.chassis_wear = 0;
                    coach.chassis_wear_unfixable = 0;
                    break;
                case 3:
                    coach.cabin_wear = 0;
                    coach.cabin_wear_unfixable = 0;
                    break;
                case 4:
                    coach.wheels_wear = new List<SCS_Float>();
                    coach.wheels_wear_unfixable = new List<SCS_Float>();
                    break;
            }
            UpdateCoachVehicleDisplay();
        }

        private void CoachActionButtonEnabledChanged(object sender, EventArgs e)
        {
            Button button = sender as Button;
            if (button != null && (button.Name.StartsWith("buttonCoachPartRepair", StringComparison.Ordinal) || button.Name == "buttonCoachRepairWear"))
                button.BackgroundImage = button.Enabled ? RepairImg : Graphics_TSSET.ConvertBitmapToGrayscale(RepairImg);
            else if (button != null && button.Name == "buttonCoachRefuel")
                button.BackgroundImage = button.Enabled ? RefuelImg : Graphics_TSSET.ConvertBitmapToGrayscale(RefuelImg);
        }

        private void CoachEditAccessoriesClickRefined(object sender, EventArgs e)
        {
            string coachId = comboBoxCoachVehicle.SelectedValue == null ? null : comboBoxCoachVehicle.SelectedValue.ToString();
            Vehicle coach = GetSelectedCoach();
            if (coach == null || String.IsNullOrEmpty(coachId)) return;

            Dictionary<string, dynamic> accessories = new Dictionary<string, dynamic>();
            foreach (string accessoryId in coach.accessories)
                if (SiiNunitData.SiiNitems.ContainsKey(accessoryId))
                    accessories[accessoryId] = SiiNunitData.SiiNitems[accessoryId];

            FormVehicleEditor editor = new FormVehicleEditor(coach, accessories);
            editor.StartPosition = FormStartPosition.CenterParent;
            if (editor.ShowDialog(this) != DialogResult.OK) return;

            List<string> updated = editor.Accessories.Keys.ToList();
            foreach (string removed in coach.accessories.Except(updated).ToList())
            {
                coach.accessories.Remove(removed);
                if (!SiiNunitData.NamelessIgnoreList.Contains(removed))
                    SiiNunitData.NamelessIgnoreList.Add(removed);
            }
            foreach (KeyValuePair<string, dynamic> accessory in editor.Accessories)
            {
                if (!SiiNunitData.SiiNitems.ContainsKey(accessory.Key))
                    SiiNunitData.SiiNitems.Add(accessory.Key, accessory.Value);
                if (!coach.accessories.Contains(accessory.Key))
                    coach.accessories.Add(accessory.Key);
                if (!SiiNunitData.NamelessControlList.Contains(accessory.Key))
                    SiiNunitData.NamelessControlList.Add(accessory.Key);
            }
            UpdateCoachVehicleDisplay();
        }
    }
}
