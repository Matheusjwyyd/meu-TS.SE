using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

using TS_SE_Tool.Save.Items;

namespace TS_SE_Tool
{
    public partial class FormMain
    {
        private TabPage tabPageCoachMode;
        private ComboBox comboBoxCoachVehicle;
        private Label labelCoachVehicleModel;

        private static readonly string[] CoachSkillNames =
        {
            "Jornadas mais longas",
            "Cronogramas rigorosos",
            "Serviço premium",
            "Jornada confortável",
            "Queridinho dos passageiros",
            "Direção econômica"
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

        private void ConfigureModeSelector()
        {
            tableLayoutPanel10.SuspendLayout();
            tableLayoutPanel10.ColumnCount = 3;
            tableLayoutPanel10.ColumnStyles.Clear();
            for (int i = 0; i < 3; i++)
                tableLayoutPanel10.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F / 3F));
            tableLayoutPanel10.SetColumnSpan(buttonMainAddCustomFolder, 3);

            radioButtonMainGameSwitchBus = new RadioButton
            {
                Name = "radioButtonMainGameSwitchBus",
                Appearance = Appearance.Button,
                AutoSize = false,
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 8.25F, FontStyle.Bold),
                Text = "ETS Bus",
                TextAlign = ContentAlignment.MiddleCenter,
                Padding = new Padding(1),
                UseVisualStyleBackColor = true,
                TabStop = true
            };
            radioButtonMainGameSwitchBus.CheckedChanged += ToggleGame_Click;
            tableLayoutPanel10.Controls.Add(radioButtonMainGameSwitchBus, 2, 0);
            radioButtonMainGameSwitchETS.Font = new Font("Segoe UI", 8.25F, FontStyle.Bold);
            radioButtonMainGameSwitchATS.Font = new Font("Segoe UI", 8.25F, FontStyle.Bold);
            radioButtonMainGameSwitchETS.Padding = new Padding(1);
            radioButtonMainGameSwitchATS.Padding = new Padding(1);
            toolTipMain.SetToolTip(radioButtonMainGameSwitchBus, "Editar o perfil ETS 2 no modo ônibus");
            tableLayoutPanel10.ResumeLayout(true);
        }

        private void SetModeSelectorChecks()
        {
            if (radioButtonMainGameSwitchBus == null)
                return;

            suppressModeEvents = true;
            radioButtonMainGameSwitchATS.Checked = GameType == "ATS";
            radioButtonMainGameSwitchBus.Checked = IsBusMode;
            radioButtonMainGameSwitchETS.Checked = GameType == "ETS2" && !IsBusMode;
            suppressModeEvents = false;
        }

        private void UpdateNavigationForMode()
        {
            if (tabControlMain == null || tabPageCoachMode == null)
                return;

            TabPage selected = tabControlMain.SelectedTab;
            TabPage[] standardPages =
            {
                tabPageProfile, tabPageCompany, tabPageTruck, tabPageTrailer,
                tabPageFreightMarket, tabPageCargoMarket, tabPageConvoyTools
            };
            TabPage[] busPages = { tabPageProfile, tabPageCompany, tabPageTruck, tabPageCoachMode };
            TabPage[] visiblePages = IsBusMode ? busPages : standardPages;

            foreach (TabPage page in tabControlMain.TabPages.Cast<TabPage>().ToArray())
                tabControlMain.TabPages.Remove(page);

            tabPageTruck.Text = IsBusMode
                ? "Ônibus"
                : (String.IsNullOrEmpty(defaultTruckTabText) ? "Truck" : defaultTruckTabText);
            tabPageCoachMode.Text = "Estatísticas";

            foreach (TabPage page in visiblePages)
            {
                tabControlMain.TabPages.Add(page);
                page.Enabled = SiiNunitData != null;
            }

            if (selected != null && visiblePages.Contains(selected))
                tabControlMain.SelectedTab = selected;
            else
                tabControlMain.SelectedTab = tabPageProfile;

            ApplyCoachGarageMode(IsBusMode);
            ModernTheme.RefreshMainNavigation(this);
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

            // Append the new page to preserve the indexes used for existing tab icons.
            tabControlMain.TabPages.Add(tabPageCoachMode);
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
            panel.Controls.Add(new Label
            {
                AutoSize = true,
                Text = title,
                Font = new Font("Segoe UI", 15F, FontStyle.Bold),
                Location = new Point(2, 0)
            });
            panel.Controls.Add(new Label
            {
                AutoSize = false,
                Text = description,
                Location = new Point(3, 34),
                Size = new Size(470, 32),
                ForeColor = Color.FromArgb(95, 103, 112)
            });
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
                    if (model.Length > 0)
                        return System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(model);
                }
            }

            return String.IsNullOrEmpty(coachId)
                ? "Ônibus"
                : "Ônibus " + coachId.Substring(Math.Max(0, coachId.Length - 6));
        }

        private Vehicle GetSelectedCoach()
        {
            if (comboBoxCoachVehicle == null || comboBoxCoachVehicle.SelectedValue == null || UserBusDictionary == null)
                return null;

            Vehicle coach;
            return UserBusDictionary.TryGetValue(comboBoxCoachVehicle.SelectedValue.ToString(), out coach) ? coach : null;
        }

        private void UpdateCoachModeGameAvailability()
        {
            if (tabPageCoachMode == null)
                return;

            bool busModeAvailable = GameType == "ETS2";
            if (!busModeAvailable && tabControlMain.SelectedTab == tabPageCoachMode)
                tabControlMain.SelectedTab = tabPageProfile;
            tabPageCoachMode.Enabled = busModeAvailable;
        }
    }
}
