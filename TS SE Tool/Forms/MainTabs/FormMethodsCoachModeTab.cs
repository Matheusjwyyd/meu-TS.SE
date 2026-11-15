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
        private NumericUpDown[] coachActiveJobStatistics;
        private Label labelCoachActiveJobStatus;
        private Button buttonCoachEditAccessories;
        private Panel[] coachPartProgressPanels;
        private Label[] coachPartNameLabels;
        private Button[] coachPartRepairButtons;
        private Button buttonCoachRepairAll;
        private Panel coachFuelProgressPanel;
        private Button buttonCoachRefuel;
        private bool refreshingCoachMode;
        private bool coachTruckLabelsCaptured;
        private string defaultTruckGroupText;
        private string defaultTruckDetailsText;
        private string defaultTruckShareText;
        private string defaultSelectCurrentText;
        private string defaultSwitchCurrentText;

        private static readonly string[] CoachPartNames =
        {
            "Motor", "Transmissão", "Chassi", "Cabine", "Rodas"
        };

        private void CreateCoachModeControlsRefined()
        {
            BuildCoachStatisticsPage();
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

            GroupBox activeJobGroup = CreateCoachGroup("Trabalho de ônibus atual", 145);
            TableLayoutPanel activeJobGrid = CreateCoachGrid(2, 2);
            activeJobGrid.RowStyles.Clear();
            activeJobGrid.RowStyles.Add(new RowStyle(SizeType.Percent, 72F));
            activeJobGrid.RowStyles.Add(new RowStyle(SizeType.Percent, 28F));
            activeJobGroup.Controls.Add(activeJobGrid);

            coachActiveJobStatistics = new NumericUpDown[2];
            coachActiveJobStatistics[0] = CreateCoachNumberInput();
            coachActiveJobStatistics[0].Minimum = 0;
            coachActiveJobStatistics[0].Maximum = 100;
            coachActiveJobStatistics[0].DecimalPlaces = 1;
            coachActiveJobStatistics[0].Increment = 0.1M;
            coachActiveJobStatistics[0].ValueChanged += CoachActiveJobStatisticChanged;
            AddCoachGridField(activeJobGrid, 0, 0, "Satisfação dos passageiros (%)", coachActiveJobStatistics[0]);

            coachActiveJobStatistics[1] = CreateCoachNumberInput();
            coachActiveJobStatistics[1].Minimum = -10080;
            coachActiveJobStatistics[1].Maximum = 10080;
            coachActiveJobStatistics[1].DecimalPlaces = 0;
            coachActiveJobStatistics[1].Increment = 1;
            coachActiveJobStatistics[1].ValueChanged += CoachActiveJobStatisticChanged;
            AddCoachGridField(activeJobGrid, 1, 0, "Atraso atual (min)", coachActiveJobStatistics[1]);

            labelCoachActiveJobStatus = new Label
            {
                Name = "labelCoachActiveJobStatus",
                AutoSize = false,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = ModernTheme.PrimaryGray,
                Text = "Carregue um salvamento com uma viagem de ônibus ativa."
            };
            activeJobGrid.Controls.Add(labelCoachActiveJobStatus, 0, 1);
            activeJobGrid.SetColumnSpan(labelCoachActiveJobStatus, 2);
            flow.Controls.Add(activeJobGroup);
            MakeCoachFlowResponsive(flow);
        }

        private void ApplyCoachGarageMode(bool busMode)
        {
            if (groupBoxUserTruckTruck == null || tableLayoutPanel9 == null)
                return;

            if (!coachTruckLabelsCaptured)
            {
                defaultTruckGroupText = groupBoxUserTruckTruck.Text;
                defaultTruckDetailsText = groupBoxUserTruckTruckDetails.Text;
                defaultTruckShareText = groupBoxUserTruckShareTruckSettings.Text;
                defaultSelectCurrentText = buttonUserTruckSelectCurrent.Text;
                defaultSwitchCurrentText = buttonUserTruckSwitchCurrent.Text;
                coachTruckLabelsCaptured = true;
            }

            groupBoxUserTruckTruck.Text = busMode ? "Ônibus" : defaultTruckGroupText;
            groupBoxUserTruckTruckDetails.Text = busMode ? "Detalhes do ônibus" : defaultTruckDetailsText;
            groupBoxUserTruckShareTruckSettings.Text = busMode
                ? "Compartilhar configurações do ônibus"
                : defaultTruckShareText;
            buttonUserTruckSelectCurrent.Text = busMode ? "Selecionar ônibus atual" : defaultSelectCurrentText;
            buttonUserTruckSwitchCurrent.Text = busMode
                ? "Ônibus selecionado para edição"
                : defaultSwitchCurrentText;
            buttonUserTruckSwitchCurrent.Enabled = !busMode && comboBoxUserTruckCompanyTrucks.SelectedValue != null &&
                comboBoxUserTruckCompanyTrucks.SelectedValue.ToString() != "null";

            if (tableLayoutPanel9.Parent != tabPageTruck)
                tabPageTruck.Controls.Add(tableLayoutPanel9);
            tableLayoutPanel9.Dock = DockStyle.Fill;
            tableLayoutPanel9.Visible = true;
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
            Panel rootPanel = new Panel
            {
                Name = "panelCoachGarageLayout",
                Dock = DockStyle.Fill,
                AutoScroll = true,
                Padding = new Padding(8),
                BackColor = ModernTheme.Canvas
            };
            page.Controls.Add(rootPanel);

            TableLayoutPanel garageLayout = new TableLayoutPanel
            {
                Name = "tableLayoutCoachGarage",
                Dock = DockStyle.Top,
                ColumnCount = 1,
                RowCount = 3,
                Height = 476,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };
            garageLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            garageLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 78F));
            garageLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 58F));
            garageLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 340F));
            rootPanel.Controls.Add(garageLayout);

            GroupBox selectionGroup = CreateCoachGroup("Ônibus selecionado", 74);
            selectionGroup.Dock = DockStyle.Fill;
            selectionGroup.Margin = Padding.Empty;
            TableLayoutPanel selectionLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 1,
                Padding = new Padding(8, 4, 8, 4)
            };
            selectionLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 56F));
            selectionLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 46F));
            selectionLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 54F));
            selectionGroup.Controls.Add(selectionLayout);

            PictureBox coachIcon = new PictureBox
            {
                Name = "pictureCoachVehicleIcon",
                Dock = DockStyle.Fill,
                Image = CreateCoachVehicleIcon(),
                SizeMode = PictureBoxSizeMode.Zoom,
                Margin = new Padding(2)
            };
            selectionLayout.Controls.Add(coachIcon, 0, 0);

            comboBoxCoachVehicle = new ComboBox
            {
                Name = "comboBoxCoachVehicle",
                Dock = DockStyle.Fill,
                DropDownStyle = ComboBoxStyle.DropDownList,
                FormattingEnabled = true
            };
            comboBoxCoachVehicle.SelectedIndexChanged += CoachVehicleSelectionChangedRefined;
            selectionLayout.Controls.Add(comboBoxCoachVehicle, 1, 0);
            labelCoachVehicleModel = new Label
            {
                Name = "labelCoachVehicleModel",
                Text = "Nenhum ônibus próprio encontrado neste save.",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                AutoEllipsis = true
            };
            selectionLayout.Controls.Add(labelCoachVehicleModel, 2, 0);
            garageLayout.Controls.Add(selectionGroup, 0, 0);

            FlowLayoutPanel actions = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                Margin = Padding.Empty,
                Padding = new Padding(0, 3, 0, 3)
            };
            buttonCoachRepairAll = CreateCoachImageButton("buttonCoachRepairWear", RepairImg, "Reparar o ônibus inteiro", 54);
            buttonCoachRepairAll.Click += CoachRepairAllClick;
            buttonCoachRepairAll.EnabledChanged += CoachActionButtonEnabledChanged;
            actions.Controls.Add(buttonCoachRepairAll);

            buttonCoachRefuel = CreateCoachImageButton("buttonCoachRefuel", RefuelImg, "Abastecer o ônibus", 54);
            buttonCoachRefuel.Click += CoachRefuelClick;
            buttonCoachRefuel.EnabledChanged += CoachActionButtonEnabledChanged;
            actions.Controls.Add(buttonCoachRefuel);

            buttonCoachEditAccessories = CreateCoachImageButton("buttonCoachEditAccessories", CustomizeImg, "Editar componentes do ônibus", 54);
            buttonCoachEditAccessories.Click += CoachEditAccessoriesClickRefined;
            actions.Controls.Add(buttonCoachEditAccessories);
            garageLayout.Controls.Add(actions, 0, 1);

            GroupBox detailsGroup = CreateCoachGroup("Detalhes e condição", 328);
            detailsGroup.Dock = DockStyle.Fill;
            detailsGroup.Margin = Padding.Empty;
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

                Button repairPart = CreateCoachImageButton("buttonCoachPartRepair" + i, RepairImg, "Reparar " + CoachPartNames[i], 40);
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
            garageLayout.Controls.Add(detailsGroup, 0, 2);
        }

        private Bitmap CreateCoachVehicleIcon()
        {
            Bitmap bitmap = new Bitmap(56, 48);
            using (Graphics g = Graphics.FromImage(bitmap))
            using (Pen outline = new Pen(ModernTheme.PrimaryGray, 2.2F))
            using (SolidBrush body = new SolidBrush(Color.FromArgb(222, 227, 232)))
            using (SolidBrush window = new SolidBrush(Color.FromArgb(119, 143, 163)))
            using (SolidBrush wheel = new SolidBrush(ModernTheme.NavyLight))
            using (GraphicsPath shape = new GraphicsPath())
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                shape.AddArc(5, 10, 12, 12, 180, 90);
                shape.AddLine(11, 10, 43, 10);
                shape.AddArc(39, 10, 12, 12, 270, 90);
                shape.AddLine(51, 16, 51, 32);
                shape.AddArc(43, 26, 12, 12, 0, 90);
                shape.AddLine(49, 38, 11, 38);
                shape.AddArc(5, 26, 12, 12, 90, 90);
                shape.CloseFigure();
                g.FillPath(body, shape);
                g.DrawPath(outline, shape);
                g.FillRectangle(window, 10, 14, 8, 9);
                g.FillRectangle(window, 21, 14, 8, 9);
                g.FillRectangle(window, 32, 14, 8, 9);
                g.FillRectangle(window, 43, 14, 5, 9);
                g.FillEllipse(wheel, 12, 32, 9, 9);
                g.FillEllipse(wheel, 38, 32, 9, 9);
                g.DrawLine(outline, 8, 27, 48, 27);
            }
            return bitmap;
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

        private Button CreateCoachImageButton(string name, Image image, string tooltip, int size = 46)
        {
            Button button = new Button
            {
                Name = name,
                Size = new Size(size, size),
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
            UpdateCoachActiveJobStatisticsDisplay();
            refreshingCoachMode = false;
            UpdateCoachModeGameAvailability();
        }

        private void ClearCoachPanelControlsRefined()
        {
            refreshingCoachMode = true;
            if (coachCareerStatistics != null)
                foreach (NumericUpDown input in coachCareerStatistics)
                    input.Value = 0;
            if (coachActiveJobStatistics != null)
            {
                foreach (NumericUpDown input in coachActiveJobStatistics)
                {
                    input.Value = 0;
                    input.Enabled = false;
                }
            }
            if (labelCoachActiveJobStatus != null)
                labelCoachActiveJobStatus.Text = "Carregue um salvamento com uma viagem de ônibus ativa.";
            refreshingCoachMode = false;
            UpdateCoachModeGameAvailability();
        }

        private static void SetCoachNumber(NumericUpDown input, int value)
        {
            decimal number = value;
            if (number < input.Minimum) number = input.Minimum;
            if (number > input.Maximum) number = input.Maximum;
            input.Value = number;
        }

        private static void SetCoachDecimal(NumericUpDown input, decimal value)
        {
            if (value < input.Minimum) value = input.Minimum;
            if (value > input.Maximum) value = input.Maximum;
            input.Value = value;
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

        private void UpdateCoachActiveJobStatisticsDisplay()
        {
            if (coachActiveJobStatistics == null || labelCoachActiveJobStatus == null)
                return;

            Unidentified job;
            Unidentified stage;
            List<Unidentified> passengers;
            int stageIndex;
            if (!TryGetActiveCoachJob(out job, out stage, out passengers, out stageIndex))
            {
                bool previousRefreshState = refreshingCoachMode;
                refreshingCoachMode = true;
                coachActiveJobStatistics[0].Value = 0;
                coachActiveJobStatistics[1].Value = 0;
                coachActiveJobStatistics[0].Enabled = false;
                coachActiveJobStatistics[1].Enabled = false;
                refreshingCoachMode = previousRefreshState;
                labelCoachActiveJobStatus.Text = "Nenhuma viagem de ônibus ativa foi encontrada neste save.";
                return;
            }

            bool previousDisplayRefreshState = refreshingCoachMode;
            refreshingCoachMode = true;
            float satisfaction;
            bool hasSatisfaction = TryReadSCSFloat(stage, "satisfaction", out satisfaction) && satisfaction >= 0F;
            if (!hasSatisfaction)
            {
                float total = 0F;
                int count = 0;
                foreach (Unidentified passenger in passengers)
                {
                    float passengerSatisfaction;
                    if (TryReadSCSFloat(passenger, "satisfaction", out passengerSatisfaction) &&
                        passengerSatisfaction >= 0F && passengerSatisfaction <= 1F)
                    {
                        total += passengerSatisfaction;
                        count++;
                    }
                }
                if (count > 0)
                {
                    satisfaction = total / count;
                    hasSatisfaction = true;
                }
            }

            coachActiveJobStatistics[0].Enabled = hasSatisfaction || HasRawField(stage, "satisfaction");
            if (hasSatisfaction)
                SetCoachDecimal(coachActiveJobStatistics[0], (decimal)Math.Round(satisfaction * 100F, 1));
            else
                SetCoachDecimal(coachActiveJobStatistics[0], 0M);

            int plannedArrival = 0;
            int estimatedArrival = 0;
            bool hasArrivalEstimate = TryGetActiveCoachDelay(stage, out plannedArrival, out estimatedArrival);
            coachActiveJobStatistics[1].Enabled = hasArrivalEstimate;
            if (!hasArrivalEstimate)
                SetCoachDecimal(coachActiveJobStatistics[1], 0M);
            if (hasArrivalEstimate)
            {
                decimal delay = (decimal)estimatedArrival - plannedArrival;
                SetCoachDecimal(coachActiveJobStatistics[1], delay);
                toolTipMain.SetToolTip(coachActiveJobStatistics[1],
                    "Atraso estimado = chegada estimada menos chegada programada. Edite o atraso em minutos.");
                labelCoachActiveJobStatus.Text = "Estágio " + (stageIndex + 1) + " da viagem ativa; passageiros: " + passengers.Count + ".";
                labelCoachActiveJobStatus.ForeColor = ModernTheme.PrimaryGray;
            }
            else
            {
                labelCoachActiveJobStatus.Text = "Viagem ativa detectada; o estágio atual ainda não tem horários de chegada para calcular o atraso.";
                labelCoachActiveJobStatus.ForeColor = ModernTheme.PrimaryGray;
            }

            if (!coachActiveJobStatistics[0].Enabled)
            {
                labelCoachActiveJobStatus.Text = "Viagem ativa detectada; a satisfação ainda não está disponível neste estágio.";
                labelCoachActiveJobStatus.ForeColor = ModernTheme.PrimaryGray;
            }
            refreshingCoachMode = previousDisplayRefreshState;
        }

        private bool TryGetActiveCoachJob(out Unidentified job, out Unidentified stage,
            out List<Unidentified> passengers, out int stageIndex)
        {
            job = null;
            stage = null;
            passengers = new List<Unidentified>();
            stageIndex = 0;

            if (SiiNunitData == null || SiiNunitData.Player == null)
                return false;

            string jobId = SiiNunitData.Player.current_bus_job;
            if (String.IsNullOrWhiteSpace(jobId) || jobId == "null" ||
                !TryGetUnidentifiedBlock(jobId, out job))
                return false;

            if (!TryReadRawInt(job, "stage_idx", out stageIndex))
                return false;

            List<string> stageIds = ReadRawArray(job, "stages");
            if (stageIndex < 0 || stageIndex >= stageIds.Count ||
                !TryGetUnidentifiedBlock(stageIds[stageIndex], out stage))
                return false;

            foreach (string passengerId in ReadRawArray(job, "passengers"))
            {
                Unidentified passenger;
                if (TryGetUnidentifiedBlock(passengerId, out passenger))
                    passengers.Add(passenger);
            }

            return true;
        }

        private bool TryGetActiveCoachDelay(Unidentified stage, out int plannedArrival, out int estimatedArrival)
        {
            plannedArrival = 0;
            estimatedArrival = 0;
            if (stage == null || SiiNunitData == null || SiiNunitData.Economy == null)
                return false;

            int plannedDeparture = 0;
            int plannedStageDuration = 0;
            int plannedFerryDuration = 0;
            int actualDeparture = 0;
            bool hasPlannedArrival = TryReadRawInt(stage, "planned_arrival_time", out plannedArrival);
            bool hasActualArrival = TryReadRawInt(stage, "arrival_time", out estimatedArrival);
            bool hasPlannedDeparture = TryReadRawInt(stage, "planned_departure_time", out plannedDeparture);
            bool hasStageDuration = TryReadRawInt(stage, "planned_stage_time", out plannedStageDuration);
            bool hasFerryDuration = TryReadRawInt(stage, "planned_ferry_time", out plannedFerryDuration);
            bool hasActualDeparture = TryReadRawInt(stage, "departure_time", out actualDeparture);

            if (!hasPlannedArrival)
            {
                if (!hasPlannedDeparture || !hasStageDuration)
                    return false;

                long scheduledArrival = (long)plannedDeparture + plannedStageDuration +
                    (hasFerryDuration ? plannedFerryDuration : 0);
                if (scheduledArrival < Int32.MinValue || scheduledArrival > Int32.MaxValue)
                    return false;
                plannedArrival = (int)scheduledArrival;
            }

            if (hasActualArrival)
                return true;

            int remainingStageMinutes;
            int remainingFerryMinutes = hasFerryDuration ? plannedFerryDuration : 0;
            float plannedDistance;
            float travelledDistance;
            if (hasStageDuration && TryReadRawNumber(stage, "planned_distance", out plannedDistance) &&
                TryReadRawNumber(stage, "travelled_distance", out travelledDistance) && plannedDistance > 0F)
            {
                float progress = Math.Max(0F, Math.Min(1F, travelledDistance / plannedDistance));
                remainingStageMinutes = (int)Math.Ceiling(plannedStageDuration * (1F - progress));

                if (hasActualDeparture && hasFerryDuration)
                {
                    int elapsed = Math.Max(0, (int)SiiNunitData.Economy.game_time - actualDeparture);
                    int ferryElapsed = Math.Max(0, elapsed - plannedStageDuration);
                    remainingFerryMinutes = Math.Max(0, plannedFerryDuration - ferryElapsed);
                }
            }
            else if (hasStageDuration && hasActualDeparture)
            {
                int elapsed = Math.Max(0, (int)SiiNunitData.Economy.game_time - actualDeparture);
                remainingStageMinutes = Math.Max(0, plannedStageDuration - elapsed);
                if (hasFerryDuration)
                    remainingFerryMinutes = Math.Max(0, plannedFerryDuration - Math.Max(0, elapsed - plannedStageDuration));
            }
            else
            {
                return false;
            }

            long estimate = (long)SiiNunitData.Economy.game_time + remainingStageMinutes + remainingFerryMinutes;
            if (estimate < Int32.MinValue || estimate > Int32.MaxValue)
                return false;

            estimatedArrival = (int)estimate;
            return true;
        }

        private static bool TryReadRawNumber(Unidentified block, string fieldName, out float value)
        {
            value = 0F;
            string raw;
            if (!TryReadRawField(block, fieldName, out raw) || raw == "nil")
                return false;

            try
            {
                Save.DataFormat.SCS_Float parsed = raw;
                value = parsed.Value;
                return !Single.IsNaN(value) && !Single.IsInfinity(value);
            }
            catch
            {
                return false;
            }
        }

        private bool TryGetUnidentifiedBlock(string blockId, out Unidentified block)
        {
            block = null;
            if (String.IsNullOrWhiteSpace(blockId) || blockId == "null" ||
                !SiiNunitData.SiiNitems.ContainsKey(blockId))
                return false;

            block = SiiNunitData.SiiNitems[blockId] as Unidentified;
            return block != null;
        }

        private static List<string> ReadRawArray(Unidentified block, string fieldName)
        {
            List<KeyValuePair<int, string>> values = new List<KeyValuePair<int, string>>();
            if (block == null || block.lines == null)
                return new List<string>();

            string prefix = fieldName + "[";
            foreach (string line in block.lines)
            {
                int colon = line.IndexOf(':');
                if (colon <= 0)
                    continue;

                string tag = line.Substring(0, colon).Trim();
                if (!tag.StartsWith(prefix, StringComparison.Ordinal) || !tag.EndsWith("]", StringComparison.Ordinal))
                    continue;

                int index;
                string indexText = tag.Substring(prefix.Length, tag.Length - prefix.Length - 1);
                if (Int32.TryParse(indexText, out index))
                    values.Add(new KeyValuePair<int, string>(index, line.Substring(colon + 1).Trim()));
            }

            return values.OrderBy(item => item.Key).Select(item => item.Value).ToList();
        }

        private static bool HasRawField(Unidentified block, string fieldName)
        {
            string value;
            return TryReadRawField(block, fieldName, out value);
        }

        private static bool TryReadRawField(Unidentified block, string fieldName, out string value)
        {
            value = null;
            if (block == null || block.lines == null)
                return false;

            foreach (string line in block.lines)
            {
                int colon = line.IndexOf(':');
                if (colon <= 0 || !String.Equals(line.Substring(0, colon).Trim(), fieldName, StringComparison.Ordinal))
                    continue;

                value = line.Substring(colon + 1).Trim();
                return true;
            }
            return false;
        }

        private static bool TryReadRawInt(Unidentified block, string fieldName, out int value)
        {
            string raw;
            value = 0;
            return TryReadRawField(block, fieldName, out raw) &&
                Int32.TryParse(raw, System.Globalization.NumberStyles.Integer,
                    System.Globalization.CultureInfo.InvariantCulture, out value);
        }

        private static bool TryReadSCSFloat(Unidentified block, string fieldName, out float value)
        {
            value = 0F;
            string raw;
            if (!TryReadRawField(block, fieldName, out raw) || raw == "nil")
                return false;

            try
            {
                Save.DataFormat.SCS_Float parsed = raw;
                value = parsed.Value;
                return !Single.IsNaN(value) && !Single.IsInfinity(value);
            }
            catch
            {
                return false;
            }
        }

        private static bool SetRawField(Unidentified block, string fieldName, string value)
        {
            if (block == null || block.lines == null)
                return false;

            for (int i = 0; i < block.lines.Count; i++)
            {
                string line = block.lines[i];
                int colon = line.IndexOf(':');
                if (colon <= 0 || !String.Equals(line.Substring(0, colon).Trim(), fieldName, StringComparison.Ordinal))
                    continue;

                block.lines[i] = line.Substring(0, colon + 1) + " " + value;
                return true;
            }
            return false;
        }

        private void CoachActiveJobStatisticChanged(object sender, EventArgs e)
        {
            if (refreshingCoachMode || SiiNunitData == null || !IsBusMode || coachActiveJobStatistics == null)
                return;

            Unidentified job;
            Unidentified stage;
            List<Unidentified> passengers;
            int stageIndex;
            if (!TryGetActiveCoachJob(out job, out stage, out passengers, out stageIndex))
            {
                UpdateCoachActiveJobStatisticsDisplay();
                return;
            }

            if (sender == coachActiveJobStatistics[0])
            {
                float target = (float)(coachActiveJobStatistics[0].Value / 100M);
                List<KeyValuePair<Unidentified, float>> currentPassengerSatisfaction = new List<KeyValuePair<Unidentified, float>>();
                foreach (Unidentified passenger in passengers)
                {
                    float value;
                    if (TryReadSCSFloat(passenger, "satisfaction", out value) && value >= 0F && value <= 1F)
                        currentPassengerSatisfaction.Add(new KeyValuePair<Unidentified, float>(passenger, value));
                }

                if (currentPassengerSatisfaction.Count > 0)
                {
                    float average = currentPassengerSatisfaction.Average(item => item.Value);
                    float shift = target - average;
                    foreach (KeyValuePair<Unidentified, float> passenger in currentPassengerSatisfaction)
                    {
                        float adjusted = Math.Max(0F, Math.Min(1F, passenger.Value + shift));
                        Save.DataFormat.SCS_Float serialized = adjusted;
                        SetRawField(passenger.Key, "satisfaction", serialized.ToString());
                    }
                }

                Save.DataFormat.SCS_Float stageSatisfaction = target;
                SetRawField(stage, "satisfaction", stageSatisfaction.ToString());
            }
            else if (sender == coachActiveJobStatistics[1])
            {
                int plannedArrival;
                int estimatedArrival;
                bool hasSchedule = TryGetActiveCoachDelay(stage, out plannedArrival, out estimatedArrival);

                if (hasSchedule)
                {
                    decimal scheduledArrival = estimatedArrival - coachActiveJobStatistics[1].Value;
                    if (scheduledArrival >= Int32.MinValue && scheduledArrival <= Int32.MaxValue)
                        SetRawField(stage, "planned_arrival_time", Decimal.ToInt32(scheduledArrival).ToString(
                            System.Globalization.CultureInfo.InvariantCulture));
                }
            }

            UpdateCoachActiveJobStatisticsDisplay();
        }

        private void CoachVehicleSelectionChangedRefined(object sender, EventArgs e)
        {
            if (!refreshingCoachMode)
                UpdateCoachVehicleDisplay();
        }

        private void UpdateCoachVehicleDisplay()
        {
            if (labelCoachVehicleModel == null || comboBoxCoachVehicle == null)
                return;

            Vehicle coach = GetSelectedCoach();
            bool available = coach != null && SiiNunitData != null && GameType == "ETS2";
            if (available)
            {
                labelCoachVehicleModel.Text = GetCoachModelName(comboBoxCoachVehicle.SelectedValue.ToString(), coach) +
                    Environment.NewLine + "Odômetro: " + coach.odometer.ToString("N0") + " km";
            }
            else
            {
                labelCoachVehicleModel.Text = UserBusDictionary == null || UserBusDictionary.Count == 0
                    ? "Nenhum ônibus próprio encontrado neste save."
                    : "Selecione um ônibus para editar.";
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
