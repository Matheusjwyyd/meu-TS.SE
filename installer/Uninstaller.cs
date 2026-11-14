using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Windows.Forms;
using Microsoft.Win32;

namespace MeuSaveEditorUninstaller
{
    internal static class Program
    {
        private const string UninstallKey = @"Software\Microsoft\Windows\CurrentVersion\Uninstall\MeuSaveEditor";

        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            string installPath = Path.GetDirectoryName(Application.ExecutablePath);
            string appPath = Path.Combine(installPath, "MeuSaveEditor.exe");

            if (IsApplicationRunning(appPath))
            {
                MessageBox.Show("Feche o Meu Save Editor antes de desinstalar.", "Meu Save Editor",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            DialogResult answer = MessageBox.Show(
                "Deseja remover o Meu Save Editor e os atalhos deste computador?",
                "Desinstalar Meu Save Editor", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (answer != DialogResult.Yes)
                return;

            string startMenu = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.StartMenu),
                "Programs", "Meu Save Editor");
            string desktopShortcut = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory),
                "Meu Save Editor.lnk");
            try { if (Directory.Exists(startMenu)) Directory.Delete(startMenu, true); }
            catch { }
            try { if (File.Exists(desktopShortcut)) File.Delete(desktopShortcut); }
            catch { }
            try { Registry.CurrentUser.DeleteSubKeyTree(UninstallKey, false); }
            catch { }

            ProcessStartInfo cleanup = new ProcessStartInfo
            {
                FileName = Environment.GetEnvironmentVariable("ComSpec"),
                Arguments = "/c ping 127.0.0.1 -n 3 > nul & rmdir /s /q \"" + installPath + "\"",
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                UseShellExecute = false
            };
            Process.Start(cleanup);
        }

        private static bool IsApplicationRunning(string executablePath)
        {
            string fullPath = Path.GetFullPath(executablePath);
            foreach (Process process in Process.GetProcessesByName("MeuSaveEditor"))
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
    }
}
