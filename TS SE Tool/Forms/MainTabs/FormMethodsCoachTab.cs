using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

using TS_SE_Tool.Save.DataFormat;
using TS_SE_Tool.Save.Items;

namespace TS_SE_Tool
{
    public partial class FormMain : Form
    {
        private TabPage tabPageCoachMode;
        private ComboBox comboBoxCoachVehicle;
        private Label labelCoachVehicleModel;
        private NumericUpDown numericCoachExperience;
        private NumericUpDown[] numericCoachSkills;
        private NumericUpDown[] numericCoachStatistics;
        private NumericUpDown numericCoachOdometer;
        private NumericUpDown numericCoachFuel;
        private NumericUpDown[] numericCoachWear;
        private Button buttonCoachEditAccessories;
        private Button buttonCoachRepairWear;
        private bool refreshingCoachControls;

        private static readonly string[] CoachSkillNames =
        {
            "Viagens longas",
            "Horário apertado",
            "Redução de satisfação",
            "Aceleração suave",
            "Aumento de satisfação",
            "Eco-condução"
        };

        private static readonly string[] CoachStatisticNames =
        {
            "Distância total",
            "Viagens concluídas",
            "Viagens canceladas",
            "Passageiros transportados",
            "Paradas realizadas",
            "Tempo de jogo",
            "Tempo jogado"
        };

        private static readonly string[] CoachWearNames =
        {
            "Motor (%)",
            "Transmissão (%)",
            "Cabine (%)",
            "Chassi (%)"
        };

        private sealed class CoachVehicleOption
        {
            public string Id { get; private set; }
            public string Name { get; private set; }

            internal CoachVehicleOption(string id, string name)
            {
                Id = id;
                Name = name;
            }
        }

        private void AddCoachModeTab()
        {
            tabPageCoachMode = new TabPage
            {
                Name = "tabPageCoachMode",
                Text = "Ônibus",
                Padding = new Padding(3),
                UseVisualStyleBackColor = true
            };

            // Keep the existing seven tab indexes (and their icons) unchanged.
            tabControlMain.TabPages.Add(tabPageCoachMode);
        }

        private void CreateCoachModeControls()
        {
            TabControl coachSections = new TabControl
            {
                Name = "tabControlCoachSections",
                Dock = DockStyle.Fill,
                Padding = new Point(12, 4),
                Font = new Font("Segoe UI", 9F)
            };

            TabPage progressPage = new TabPage("Carreira e habilidades")
            {
                Name = "tabPageCoachProgress",
                AutoScroll = true,
                Padding = new Padding(14)
            };
            TabPage vehiclesPage = new TabPage("Meus ônibus")
            {
                Name = "tabPageCoachVehicles",
                AutoScroll = true,
                Padding = new Padding(14)
            };

            coachSections.TabPages.Add(progressPage);
            coachSections.TabPages.Add(vehiclesPage);
            tabPageCoachMode.Controls.Add(coachSections);

            BuildCoachProgressPage(progressPage);
            BuildCoachVehiclesPage(vehiclesPage);
        }

        private void BuildCoachProgressPage(TabPage page)
        {
            FlowLayoutPanel flow = CreateCoachFlowPanel();
            page.Controls.Add(flow);

            flow.Controls.Add(CreateCoachHeading("Modo ônibus do ETS 2",
                "Progressão e habilidades do ônibus são independentes da carreira de caminhão."));

            GroupBox skillsGroup = CreateCoachGroup("Habilidades e experiência", 242);
            TableLayoutPanel skillsGrid = CreateCoachGrid(2, 4);
            skillsGroup.Controls.Add(skillsGrid);

            numericCoachExperience = CreateCoachNumberInput();
            numericCoachExperience.ValueChanged += CoachCareerValueChanged;
            AddCoachGridField(skillsGrid, 0, 0, "Experiência de ônibus (XP)", numericCoachExperience);

            numericCoachSkills = new NumericUpDown[CoachSkillNames.Length];
            for (int i = 0; i < CoachSkillNames.Length; i++)
            {
                numericCoachSkills[i] = CreateCoachNumberInput();
                numericCoachSkills[i].ValueChanged += CoachCareerValueChanged;
                AddCoachGridField(skillsGrid, (i + 1) % 2, (i + 1) / 2, CoachSkillNames[i], numericCoachSkills[i]);
            }
            flow.Controls.Add(skillsGroup);

            GroupBox statisticsGroup = CreateCoachGroup("Estatísticas do modo ônibus", 258);
            TableLayoutPanel statisticsGrid = CreateCoachGrid(2, 4);
            statisticsGroup.Controls.Add(statisticsGrid);

            numericCoachStatistics = new NumericUpDown[CoachStatisticNames.Length];
            for (int i = 0; i < CoachStatisticNames.Length; i++)
            {
                numericCoachStatistics[i] = CreateCoachNumberInput();
                numericCoachStatistics[i].ValueChanged += CoachCareerValueChanged;
                AddCoachGridField(statisticsGrid, i % 2, i / 2, CoachStatisticNames[i], numericCoachStatistics[i]);
            }
            flow.Controls.Add(statisticsGroup);
        }

        private void BuildCoachVehiclesPage(TabPage page)
        {
            FlowLayoutPanel flow = CreateCoachFlowPanel();
            page.Controls.Add(flow);

            flow.Controls.Add(CreateCoachHeading("Garagem de ônibus",
                "Selecione um ônibus salvo no perfil para ajustar seus dados e componentes."));

            GroupBox selectionGroup = CreateCoachGroup("Ônibus selecionado", 94);
            TableLayoutPanel selectionGrid = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                Padding = new Padding(8, 4, 8, 4)
            };
            selectionGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 58F));
            selectionGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 42F));
            selectionGroup.Controls.Add(selectionGrid);

            comboBoxCoachVehicle = new ComboBox
            {
                Name = "comboBoxCoachVehicle",
                Dock = DockStyle.Top,
                DropDownStyle = ComboBoxStyle.DropDownList,
                FormattingEnabled = true,
                Margin = new Padding(4, 8, 4, 4)
            };
            comboBoxCoachVehicle.SelectedIndexChanged += CoachVehicleSelectionChanged;
            selectionGrid.Controls.Add(comboBoxCoachVehicle, 0, 0);

            labelCoachVehicleModel = new Label
            {
                Name = "labelCoachVehicleModel",
                Dock = DockStyle.Fill,
                Text = "Carregue um save com ônibus próprio.",
                TextAlign = ContentAlignment.MiddleLeft,
                AutoEllipsis = true
            };
            selectionGrid.Controls.Add(labelCoachVehicleModel, 1, 0);
            flow.Controls.Add(selectionGroup);

            GroupBox detailsGroup = CreateCoachGroup("Dados do veículo", 232);
            TableLayoutPanel detailsGrid = CreateCoachGrid(2, 3);
            detailsGroup.Controls.Add(detailsGrid);

            numericCoachOdometer = CreateCoachNumberInput();
            numericCoachOdometer.Maximum = UInt32.MaxValue;
            numericCoachOdometer.ValueChanged += CoachVehicleValueChanged;
            AddCoachGridField(detailsGrid, 0, 0, "Odômetro", numericCoachOdometer);

            numericCoachFuel = CreateCoachNumberInput();
            numericCoachFuel.DecimalPlaces = 1;
            numericCoachFuel.Increment = 1;
            numericCoachFuel.Maximum = 100;
            numericCoachFuel.ValueChanged += CoachVehicleValueChanged;
            AddCoachGridField(detailsGrid, 1, 0, "Combustível (%)", numericCoachFuel);

            numericCoachWear = new NumericUpDown[CoachWearNames.Length];
            for (int i = 0; i < CoachWearNames.Length; i++)
            {
                numericCoachWear[i] = CreateCoachNumberInput();
                numericCoachWear[i].DecimalPlaces = 1;
                numericCoachWear[i].Increment = 0.1M;
                numericCoachWear[i].Maximum = 100;
                numericCoachWear[i].ValueChanged += CoachVehicleValueChanged;
                AddCoachGridField(detailsGrid, i % 2, 1 + i / 2, CoachWearNames[i], numericCoachWear[i]);
            }
            flow.Controls.Add(detailsGroup);

            FlowLayoutPanel actions = new FlowLayoutPanel
            {
                AutoSize = true,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                Margin = new Padding(0, 2, 0, 6),
                Padding = new Padding(0)
            };
            buttonCoachEditAccessories = new Button
            {
                Name = "buttonCoachEditAccessories",
                Text = "Editar componentes",
                Width = 170,
                Height = 36,
                Enabled = false
            };
            buttonCoachEditAccessories.Click += CoachEditAccessoriesClick;
            actions.Controls.Add(buttonCoachEditAccessories);

            buttonCoachRepairWear = new Button
            {
                Name = "buttonCoachRepairWear",
                Text = "Reparar desgastes",
                Width = 170,
                Height = 36,
                Enabled = false
            };
            buttonCoachRepairWear.Click += CoachRepairWearClick;
            actions.Controls.Add(buttonCoachRepairWear);
            flow.Controls.Add(actions);
        }

        private FlowLayoutPanel CreateCoachFlowPanel()
        {
            return new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                Padding = new Padding(2),
                BackColor = Color.FromArgb(247, 248, 250)
            };
        }

        private Control CreateCoachHeading(string title, string description)
        {
            Panel panel = new Panel
            {
                Width = 490,
                Height = 68,
                Margin = new Padding(0, 0, 0, 10)
            };
            Label titleLabel = new Label
            {
                AutoSize = true,
                Text = title,
                Font = new Font("Segoe UI", 15F, FontStyle.Bold),
                Location = new Point(2, 0)
            };
            Label descriptionLabel = new Label
            {
                AutoSize = false,
                Text = description,
                Location = new Point(3, 34),
                Size = new Size(470, 32),
                ForeColor = Color.FromArgb(95, 103, 112)
            };
            panel.Controls.Add(titleLabel);
            panel.Controls.Add(descriptionLabel);
            return panel;
        }

        private GroupBox CreateCoachGroup(string title, int height)
        {
            return new GroupBox
            {
                Text = title,
                Width = 490,
                Height = height,
                Margin = new Padding(0, 0, 0, 12),
                Padding = new Padding(10),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
        }

        private TableLayoutPanel CreateCoachGrid(int columns, int rows)
        {
            TableLayoutPanel grid = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = columns,
                RowCount = rows,
                Padding = new Padding(4, 6, 4, 2)
            };
            for (int i = 0; i < columns; i++)
                grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F / columns));
            for (int i = 0; i < rows; i++)
                grid.RowStyles.Add(new RowStyle(SizeType.Percent, 100F / rows));
            return grid;
        }

        private NumericUpDown CreateCoachNumberInput()
        {
            return new NumericUpDown
            {
                Minimum = 0,
                Maximum = Int32.MaxValue,
                Width = 145,
                ThousandsSeparator = true,
                TextAlign = HorizontalAlignment.Right,
                Anchor = AnchorStyles.Left | AnchorStyles.Top
            };
        }

        private void AddCoachGridField(TableLayoutPanel grid, int column, int row, string title, NumericUpDown input)
        {
            Panel field = new Panel { Dock = DockStyle.Fill, Padding = new Padding(4, 2, 4, 2) };
            Label label = new Label
            {
                AutoSize = false,
                Dock = DockStyle.Top,
                Height = 20,
                Text = title,
                AutoEllipsis = true
            };
            input.Dock = DockStyle.Top;
            field.Controls.Add(input);
            field.Controls.Add(label);
            grid.Controls.Add(field, column, row);
        }

        private void FillCoachPanelControls()
        {
            if (numericCoachExperience == null || SiiNunitData == null)
            {
                ClearCoachPanelControls();
                return;
            }

            refreshingCoachControls = true;
            Economy economy = SiiNunitData.Economy;
            SetCoachNumber(numericCoachExperience, economy.bus_experience_points);

            int[] skills =
            {
                economy.long_duty,
                economy.tight_schedule,
                economy.satisfaction_decrease,
                economy.low_acceleration,
                economy.satisfaction_increase,
                economy.ecodriving
            };
            for (int i = 0; i < numericCoachSkills.Length; i++)
                SetCoachNumber(numericCoachSkills[i], skills[i]);

            int[] statistics =
            {
                economy.bus_total_distance,
                economy.bus_finished_job_count,
                economy.bus_cancelled_job_count,
                economy.bus_total_passengers,
                economy.bus_total_stops,
                economy.bus_game_time,
                economy.bus_playing_time
            };
            for (int i = 0; i < numericCoachStatistics.Length; i++)
                SetCoachNumber(numericCoachStatistics[i], statistics[i]);

            List<CoachVehicleOption> coaches = UserBusDictionary
                .OrderBy(item => item.Key, StringComparer.Ordinal)
                .Select((item, index) => new CoachVehicleOption(item.Key,
                    "Ônibus " + (index + 1) + " — " + GetCoachModelName(item.Key, item.Value)))
                .ToList();
            comboBoxCoachVehicle.DataSource = null;
            comboBoxCoachVehicle.DisplayMember = "Name";
            comboBoxCoachVehicle.ValueMember = "Id";
            comboBoxCoachVehicle.DataSource = coaches;
            refreshingCoachControls = false;

            UpdateCoachModeGameAvailability();
            RefreshCoachVehicleControls();
        }

        private void ClearCoachPanelControls()
        {
            if (numericCoachExperience == null)
                return;

            refreshingCoachControls = true;
            numericCoachExperience.Value = 0;
            if (numericCoachSkills != null)
                foreach (NumericUpDown input in numericCoachSkills)
                    input.Value = 0;
            if (numericCoachStatistics != null)
                foreach (NumericUpDown input in numericCoachStatistics)
                    input.Value = 0;
            if (comboBoxCoachVehicle != null)
                comboBoxCoachVehicle.DataSource = null;
            if (numericCoachOdometer != null)
                numericCoachOdometer.Value = 0;
            if (numericCoachFuel != null)
                numericCoachFuel.Value = 0;
            if (numericCoachWear != null)
                foreach (NumericUpDown input in numericCoachWear)
                    input.Value = 0;
            if (labelCoachVehicleModel != null)
                labelCoachVehicleModel.Text = "Carregue um save com ônibus próprio.";
            if (buttonCoachEditAccessories != null)
                buttonCoachEditAccessories.Enabled = false;
            if (buttonCoachRepairWear != null)
                buttonCoachRepairWear.Enabled = false;
            refreshingCoachControls = false;
        }

        private void CoachCareerValueChanged(object sender, EventArgs e)
        {
            if (refreshingCoachControls || SiiNunitData == null || numericCoachSkills == null)
                return;

            Economy economy = SiiNunitData.Economy;
            economy.bus_experience_points = (int)numericCoachExperience.Value;
            economy.long_duty = (int)numericCoachSkills[0].Value;
            economy.tight_schedule = (int)numericCoachSkills[1].Value;
            economy.satisfaction_decrease = (int)numericCoachSkills[2].Value;
            economy.low_acceleration = (int)numericCoachSkills[3].Value;
            economy.satisfaction_increase = (int)numericCoachSkills[4].Value;
            economy.ecodriving = (int)numericCoachSkills[5].Value;

            economy.bus_total_distance = (int)numericCoachStatistics[0].Value;
            economy.bus_finished_job_count = (int)numericCoachStatistics[1].Value;
            economy.bus_cancelled_job_count = (int)numericCoachStatistics[2].Value;
            economy.bus_total_passengers = (int)numericCoachStatistics[3].Value;
            economy.bus_total_stops = (int)numericCoachStatistics[4].Value;
            economy.bus_game_time = (int)numericCoachStatistics[5].Value;
            economy.bus_playing_time = (int)numericCoachStatistics[6].Value;
        }

        private void CoachVehicleSelectionChanged(object sender, EventArgs e)
        {
            if (!refreshingCoachControls)
                RefreshCoachVehicleControls();
        }

        private void RefreshCoachVehicleControls()
        {
            Vehicle coach = GetSelectedCoach();
            bool available = coach != null && SiiNunitData != null && GameType == "ETS2";
            refreshingCoachControls = true;

            if (available)
            {
                SetCoachNumber(numericCoachOdometer, coach.odometer);
                SetCoachNumber(numericCoachFuel, ClampPercent((float)coach.fuel_relative));
                SetCoachNumber(numericCoachWear[0], ClampPercent((float)coach.engine_wear));
                SetCoachNumber(numericCoachWear[1], ClampPercent((float)coach.transmission_wear));
                SetCoachNumber(numericCoachWear[2], ClampPercent((float)coach.cabin_wear));
                SetCoachNumber(numericCoachWear[3], ClampPercent((float)coach.chassis_wear));
                labelCoachVehicleModel.Text = GetCoachModelName((string)comboBoxCoachVehicle.SelectedValue, coach);
            }
            else
            {
                SetCoachNumber(numericCoachOdometer, 0);
                SetCoachNumber(numericCoachFuel, 0);
                foreach (NumericUpDown input in numericCoachWear)
                    SetCoachNumber(input, 0);
                labelCoachVehicleModel.Text = UserBusDictionary.Count == 0
                    ? "Nenhum ônibus próprio encontrado neste save."
                    : "Selecione um ônibus para editar.";
            }

            numericCoachOdometer.Enabled = available;
            numericCoachFuel.Enabled = available;
            foreach (NumericUpDown input in numericCoachWear)
                input.Enabled = available;
            buttonCoachEditAccessories.Enabled = available;
            buttonCoachRepairWear.Enabled = available;
            refreshingCoachControls = false;
        }

        private Vehicle GetSelectedCoach()
        {
            if (comboBoxCoachVehicle == null || comboBoxCoachVehicle.SelectedValue == null || UserBusDictionary == null)
                return null;

            Vehicle coach;
            return UserBusDictionary.TryGetValue(comboBoxCoachVehicle.SelectedValue.ToString(), out coach) ? coach : null;
        }

        private void CoachVehicleValueChanged(object sender, EventArgs e)
        {
            if (refreshingCoachControls)
                return;

            Vehicle coach = GetSelectedCoach();
            if (coach == null)
                return;

            coach.odometer = (uint)numericCoachOdometer.Value;
            coach.fuel_relative = new SCS_Float((float)(numericCoachFuel.Value / 100M));
            coach.engine_wear = new SCS_Float((float)(numericCoachWear[0].Value / 100M));
            coach.transmission_wear = new SCS_Float((float)(numericCoachWear[1].Value / 100M));
            coach.cabin_wear = new SCS_Float((float)(numericCoachWear[2].Value / 100M));
            coach.chassis_wear = new SCS_Float((float)(numericCoachWear[3].Value / 100M));
        }

        private void CoachRepairWearClick(object sender, EventArgs e)
        {
            Vehicle coach = GetSelectedCoach();
            if (coach == null)
                return;

            coach.engine_wear = 0;
            coach.transmission_wear = 0;
            coach.cabin_wear = 0;
            coach.chassis_wear = 0;
            for (int i = 0; i < coach.wheels_wear.Count; i++)
                coach.wheels_wear[i] = 0;
            RefreshCoachVehicleControls();
        }

        private void CoachEditAccessoriesClick(object sender, EventArgs e)
        {
            string coachId = comboBoxCoachVehicle.SelectedValue == null ? null : comboBoxCoachVehicle.SelectedValue.ToString();
            Vehicle coach = GetSelectedCoach();
            if (coach == null || String.IsNullOrEmpty(coachId))
                return;

            Dictionary<string, dynamic> accessories = new Dictionary<string, dynamic>();
            foreach (string accessoryId in coach.accessories)
            {
                dynamic accessory;
                if (SiiNunitData.SiiNitems.TryGetValue(accessoryId, out accessory))
                    accessories[accessoryId] = accessory;
            }

            FormVehicleEditor editor = new FormVehicleEditor(coach, accessories);
            editor.StartPosition = FormStartPosition.CenterParent;
            if (editor.ShowDialog(this) != DialogResult.OK)
                return;

            List<string> newAccessoryIds = editor.Accessories.Keys.ToList();
            List<string> removed = coach.accessories.Except(newAccessoryIds).ToList();
            foreach (string accessoryId in removed)
            {
                coach.accessories.Remove(accessoryId);
                if (!SiiNunitData.NamelessIgnoreList.Contains(accessoryId))
                    SiiNunitData.NamelessIgnoreList.Add(accessoryId);
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

            RefreshCoachVehicleControls();
        }

        private string GetCoachModelName(string coachId, Vehicle coach)
        {
            if (coach != null && SiiNunitData != null)
            {
                foreach (string accessoryId in coach.accessories)
                {
                    Vehicle_Accessory accessory = SiiNunitData.SiiNitems.ContainsKey(accessoryId)
                        ? SiiNunitData.SiiNitems[accessoryId] as Vehicle_Accessory
                        : null;
                    if (accessory == null || accessory.accType != "basepart")
                        continue;

                    string path = accessory.data_path.Trim('"');
                    int busIndex = path.IndexOf("/bus/", StringComparison.OrdinalIgnoreCase);
                    if (busIndex < 0)
                        continue;

                    string model = path.Substring(busIndex + 5);
                    int end = model.IndexOf('/');
                    if (end >= 0)
                        model = model.Substring(0, end);
                    model = model.Replace('_', ' ').Replace('.', ' ').Replace('-', ' ').Trim();
                    if (!String.IsNullOrWhiteSpace(model))
                        return System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(model);
                }
            }

            return String.IsNullOrEmpty(coachId) ? "Ônibus" : "Ônibus " + coachId.Substring(Math.Max(0, coachId.Length - 6));
        }

        private void UpdateCoachModeGameAvailability()
        {
            if (tabPageCoachMode == null)
                return;

            bool ets2 = GameType == "ETS2";
            if (!ets2 && tabControlMain.SelectedTab == tabPageCoachMode)
                tabControlMain.SelectedTab = tabPageProfile;
            tabPageCoachMode.Enabled = ets2;
        }

        private static void SetCoachNumber(NumericUpDown input, decimal value)
        {
            if (input == null)
                return;

            if (value < input.Minimum)
                value = input.Minimum;
            if (value > input.Maximum)
                value = input.Maximum;
            input.Value = value;
        }

        private static void SetCoachNumber(NumericUpDown input, uint value)
        {
            SetCoachNumber(input, (decimal)value);
        }

        private static decimal ClampPercent(float value)
        {
            decimal percent = (decimal)value * 100M;
            if (percent < 0) return 0;
            if (percent > 100) return 100;
            return percent;
        }
    }
}
