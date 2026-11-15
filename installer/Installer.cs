using System;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.Win32;

namespace MeuSaveEditorInstaller
{
    internal static class Program
    {
        internal const string ProductName = "Meu Save Editor";
        internal const string ProductVersion = "0.3.13.2";
        internal const string UninstallKey = @"Software\Microsoft\Windows\CurrentVersion\Uninstall\MeuSaveEditor";

        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new InstallerWindow());
        }
    }

    internal sealed class InstallerWindow : Form
    {
        private readonly TextBox installPath;
        private readonly CheckBox desktopShortcut;
        private readonly CheckBox launchAfterInstall;
        private readonly Button installButton;
        private readonly Label statusLabel;

        internal InstallerWindow()
        {
            Text = Program.ProductName + " — Instalação";
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = true;
            ClientSize = new System.Drawing.Size(610, 365);
            Font = new System.Drawing.Font("Segoe UI", 9F);
            BackColor = System.Drawing.Color.FromArgb(243, 245, 247);

            Panel header = new Panel
            {
                Dock = DockStyle.Top,
                Height = 92,
                BackColor = System.Drawing.Color.FromArgb(48, 54, 61)
            };
            Controls.Add(header);

            Label title = new Label
            {
                AutoSize = true,
                Text = Program.ProductName,
                Font = new System.Drawing.Font("Segoe UI Semibold", 19F),
                ForeColor = System.Drawing.Color.White,
                Location = new System.Drawing.Point(25, 15)
            };
            header.Controls.Add(title);

            Label subtitle = new Label
            {
                AutoSize = true,
                Text = "Instalador para Windows · versão " + Program.ProductVersion,
                Font = new System.Drawing.Font("Segoe UI", 9F),
                ForeColor = System.Drawing.Color.FromArgb(210, 216, 222),
                Location = new System.Drawing.Point(28, 56)
            };
            header.Controls.Add(subtitle);

            Label folderLabel = new Label
            {
                AutoSize = true,
                Text = "Pasta de instalação",
                Font = new System.Drawing.Font("Segoe UI Semibold", 10F),
                Location = new System.Drawing.Point(28, 119)
            };
            Controls.Add(folderLabel);

            installPath = new TextBox
            {
                Location = new System.Drawing.Point(31, 149),
                Size = new System.Drawing.Size(440, 27),
                Text = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "Programs", Program.ProductName)
            };
            Controls.Add(installPath);

            Button browseButton = new Button
            {
                Text = "Procurar…",
                Location = new System.Drawing.Point(480, 148),
                Size = new System.Drawing.Size(99, 29)
            };
            browseButton.Click += BrowseForFolder;
            Controls.Add(browseButton);

            desktopShortcut = new CheckBox
            {
                AutoSize = true,
                Text = "Criar atalho na área de trabalho",
                Checked = true,
                Location = new System.Drawing.Point(32, 195)
            };
            Controls.Add(desktopShortcut);

            launchAfterInstall = new CheckBox
            {
                AutoSize = true,
                Text = "Abrir o programa ao terminar",
                Checked = true,
                Location = new System.Drawing.Point(32, 222)
            };
            Controls.Add(launchAfterInstall);

            statusLabel = new Label
            {
                AutoSize = false,
                Text = "A instalação será feita para este usuário e não exige privilégios de administrador.",
                ForeColor = System.Drawing.Color.FromArgb(86, 94, 102),
                Location = new System.Drawing.Point(32, 258),
                Size = new System.Drawing.Size(545, 34)
            };
            Controls.Add(statusLabel);

            ProgressBar progress = new ProgressBar
            {
                Name = "installProgress",
                Style = ProgressBarStyle.Marquee,
                MarqueeAnimationSpeed = 20,
                Visible = false,
                Location = new System.Drawing.Point(32, 296),
                Size = new System.Drawing.Size(545, 5)
            };
            Controls.Add(progress);

            installButton = new Button
            {
                Text = "Instalar",
                Font = new System.Drawing.Font("Segoe UI Semibold", 10F),
                BackColor = System.Drawing.Color.FromArgb(83, 91, 100),
                ForeColor = System.Drawing.Color.White,
                FlatStyle = FlatStyle.Flat,
                Location = new System.Drawing.Point(409, 312),
                Size = new System.Drawing.Size(170, 38)
            };
            installButton.FlatAppearance.BorderSize = 0;
            installButton.Click += Install;
            Controls.Add(installButton);

            AcceptButton = installButton;
        }

        private void BrowseForFolder(object sender, EventArgs e)
        {
            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Escolha onde instalar o Meu Save Editor.";
                dialog.ShowNewFolderButton = true;
                if (Directory.Exists(installPath.Text))
                    dialog.SelectedPath = installPath.Text;
                if (dialog.ShowDialog(this) == DialogResult.OK)
                    installPath.Text = Path.Combine(dialog.SelectedPath, Program.ProductName);
            }
        }

        private void Install(object sender, EventArgs e)
        {
            if (String.IsNullOrWhiteSpace(installPath.Text))
            {
                MessageBox.Show(this, "Informe uma pasta de instalação.", Program.ProductName,
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string destination = Path.GetFullPath(installPath.Text.Trim());
            string executable = Path.Combine(destination, "MeuSaveEditor.exe");

            if (IsApplicationRunning(executable))
            {
                MessageBox.Show(this, "Feche o Meu Save Editor antes de instalar ou atualizar.",
                    Program.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            installButton.Enabled = false;
            statusLabel.Text = "Instalando os arquivos…";
            Control progress = Controls["installProgress"];
            if (progress != null)
                progress.Visible = true;
            Application.DoEvents();

            string staging = Path.Combine(Path.GetTempPath(), "MeuSaveEditor-" + Guid.NewGuid().ToString("N"));
            try
            {
                Directory.CreateDirectory(staging);
                ExtractPayload(staging);
                CopyDirectory(staging, destination);
                Directory.Delete(staging, true);

                string menuFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.StartMenu),
                    "Programs", Program.ProductName);
                Directory.CreateDirectory(menuFolder);
                TryCreateShortcut(Path.Combine(menuFolder, Program.ProductName + ".lnk"), executable,
                    destination, "Abrir o Meu Save Editor");
                TryCreateShortcut(Path.Combine(menuFolder, "Desinstalar Meu Save Editor.lnk"),
                    Path.Combine(destination, "Uninstall.exe"), destination, "Desinstalar o Meu Save Editor");
                TryCreateShortcut(Path.Combine(menuFolder, "Tutorial de instalação.lnk"),
                    Path.Combine(destination, "TUTORIAL_INSTALACAO.md"), destination, "Abrir o tutorial");

                if (desktopShortcut.Checked)
                {
                    string desktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                    TryCreateShortcut(Path.Combine(desktop, Program.ProductName + ".lnk"), executable,
                        destination, "Abrir o Meu Save Editor");
                }

                RegisterUninstaller(destination);
                statusLabel.Text = "Instalação concluída.";
                if (MessageBox.Show(this, "O Meu Save Editor foi instalado. Deseja abri-lo agora?",
                    Program.ProductName, MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
                {
                    try
                    {
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(executable)
                        {
                            WorkingDirectory = destination
                        });
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(this, "A instalação terminou, mas o programa não abriu:\r\n" + ex.Message,
                            Program.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
                Close();
            }
            catch (Exception ex)
            {
                if (Directory.Exists(staging))
                {
                    try { Directory.Delete(staging, true); }
                    catch { }
                }
                statusLabel.Text = "Não foi possível concluir a instalação.";
                installButton.Enabled = true;
                if (progress != null)
                    progress.Visible = false;
                MessageBox.Show(this, "A instalação falhou. Confira a pasta e tente novamente.\r\n\r\n" + ex.Message,
                    Program.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static bool IsApplicationRunning(string executablePath)
        {
            string fullPath = Path.GetFullPath(executablePath);
            foreach (System.Diagnostics.Process process in System.Diagnostics.Process.GetProcessesByName("MeuSaveEditor"))
            {
                try
                {
                    if (String.Equals(Path.GetFullPath(process.MainModule.FileName), fullPath,
                        StringComparison.OrdinalIgnoreCase))
                        return true;
                }
                catch
                {
                    return true;
                }
            }
            return false;
        }

        private static void ExtractPayload(string destination)
        {
            using (Stream payload = Assembly.GetExecutingAssembly().GetManifestResourceStream("Payload.zip"))
            {
                if (payload == null)
                    throw new InvalidOperationException("O pacote de arquivos do programa não foi encontrado.");

                using (ZipArchive archive = new ZipArchive(payload, ZipArchiveMode.Read))
                {
                    string root = Path.GetFullPath(destination).TrimEnd(Path.DirectorySeparatorChar) +
                        Path.DirectorySeparatorChar;
                    foreach (ZipArchiveEntry entry in archive.Entries)
                    {
                        string entryPath = entry.FullName.Replace('/', Path.DirectorySeparatorChar);
                        string target = Path.GetFullPath(Path.Combine(root, entryPath));
                        if (!target.StartsWith(root, StringComparison.OrdinalIgnoreCase))
                            throw new InvalidDataException("O pacote contém um caminho de arquivo inválido.");

                        if (entry.Name.Length == 0)
                        {
                            Directory.CreateDirectory(target);
                            continue;
                        }

                        string parent = Path.GetDirectoryName(target);
                        if (!Directory.Exists(parent))
                            Directory.CreateDirectory(parent);
                        using (Stream source = entry.Open())
                        using (FileStream output = new FileStream(target, FileMode.Create, FileAccess.Write, FileShare.None))
                            source.CopyTo(output);
                    }
                }
            }
        }

        private static void CopyDirectory(string source, string destination)
        {
            Directory.CreateDirectory(destination);
            foreach (string directory in Directory.GetDirectories(source, "*", SearchOption.AllDirectories))
                Directory.CreateDirectory(Path.Combine(destination, directory.Substring(source.Length).TrimStart(Path.DirectorySeparatorChar)));
            foreach (string file in Directory.GetFiles(source, "*", SearchOption.AllDirectories))
            {
                string relative = file.Substring(source.Length).TrimStart(Path.DirectorySeparatorChar);
                string target = Path.Combine(destination, relative);
                string parent = Path.GetDirectoryName(target);
                if (!Directory.Exists(parent))
                    Directory.CreateDirectory(parent);
                File.Copy(file, target, true);
            }
        }

        private static void RegisterUninstaller(string destination)
        {
            using (RegistryKey key = Registry.CurrentUser.CreateSubKey(Program.UninstallKey))
            {
                key.SetValue("DisplayName", Program.ProductName);
                key.SetValue("DisplayVersion", Program.ProductVersion);
                key.SetValue("Publisher", "Meu Save Editor");
                key.SetValue("InstallLocation", destination);
                key.SetValue("DisplayIcon", Path.Combine(destination, "MeuSaveEditor.exe"));
                key.SetValue("UninstallString", "\"" + Path.Combine(destination, "Uninstall.exe") + "\"");
                key.SetValue("NoModify", 1, RegistryValueKind.DWord);
                key.SetValue("NoRepair", 1, RegistryValueKind.DWord);
            }
        }

        private static void TryCreateShortcut(string linkPath, string targetPath, string workingDirectory, string description)
        {
            object shell = null;
            object shortcut = null;
            try
            {
                Type shellType = Type.GetTypeFromProgID("WScript.Shell");
                if (shellType == null)
                    return;
                shell = Activator.CreateInstance(shellType);
                shortcut = shellType.InvokeMember("CreateShortcut", BindingFlags.InvokeMethod,
                    null, shell, new object[] { linkPath });
                Type shortcutType = shortcut.GetType();
                shortcutType.InvokeMember("TargetPath", BindingFlags.SetProperty, null, shortcut, new object[] { targetPath });
                shortcutType.InvokeMember("WorkingDirectory", BindingFlags.SetProperty, null, shortcut, new object[] { workingDirectory });
                shortcutType.InvokeMember("Description", BindingFlags.SetProperty, null, shortcut, new object[] { description });
                shortcutType.InvokeMember("IconLocation", BindingFlags.SetProperty, null, shortcut,
                    new object[] { Path.Combine(workingDirectory, "MeuSaveEditor.exe") + ",0" });
                shortcutType.InvokeMember("Save", BindingFlags.InvokeMethod, null, shortcut, null);
            }
            catch
            {
                // The program can still be opened from its installation folder if shortcut creation is unavailable.
            }
            finally
            {
                if (shortcut != null && Marshal.IsComObject(shortcut))
                    Marshal.FinalReleaseComObject(shortcut);
                if (shell != null && Marshal.IsComObject(shell))
                    Marshal.FinalReleaseComObject(shell);
            }
        }
    }
}
