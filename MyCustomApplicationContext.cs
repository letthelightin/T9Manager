/*  2022.04.28 
Franklin T9 Manager - controls a Franklin T9 (r717) Hotspot device over SSH via plink.exe and PowerShell
Made using Visual Studio 2022 (64 Bit) Community Edition, written in C#
Publish settings: target framwork: net6.0-windows, target runtime: portable, deployment mode: framework-dependent,

nuget: 
Install-Package Microsoft.PowerShell.SDK
Install-Package System.Management.Automation
*/

using Franklin_T9_Manager;              // needed for Resources.AppIcon
using System.Net.Http;                  // for downloading plink.exe, an SSH by command line app
using System.Management.Automation;     // for interacting with plink.exe using PowerShell
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace Franklin_T9_Manager
{
    public class MyCustomApplicationContext : ApplicationContext
    {
            // https://stackoverflow.com/questions/15653921/get-current-folder-path

        static string strWorkPath = System.AppDomain.CurrentDomain.BaseDirectory;
        static string plink = strWorkPath + "plink.exe";

        string address = "192.168.0.1";

        // https://stackoverflow.com/questions/995195/how-can-i-make-a-net-windows-forms-application-that-only-runs-in-the-system-tra

        private NotifyIcon trayIcon;

        public MyCustomApplicationContext()
        {
            trayIcon = new NotifyIcon()
            {
                Icon = T9Manager.Properties.Resources.AppIcon,

                Visible = true,

                Text = "T9 Manager",

                ContextMenuStrip = new ContextMenuStrip()
                {
                    Items =
                    {
                        new ToolStripMenuItem("Reboot", null, MenuReboot),
                        new ToolStripMenuItem("Ping", null, MenuPing),
                        new ToolStripMenuItem(address, null, MenuWebpanel)
                        {
                            DropDownItems =
                            {
                                new ToolStripMenuItem("hidden (frk@r717)", null, MenuHidden),
                                new ToolStripMenuItem("webpst (frk@r717)", null, MenuWebPST),
                                new ToolStripMenuItem("itadmin (t9_it_@dmin)", null, MenuITAdmin),
                                new ToolStripMenuItem("engineering (r717 / frkengr717)", null, MenuEngineering)
                            }
                        },
                        new ToolStripMenuItem("Cheat Sheet", null, MenuCheatSheet),
                        new ToolStripMenuItem("Exit", null, MenuExit),
                    }

                }
            };

            CancellationToken cancellationToken = new CancellationToken();

            _ = PeriodicCheckConnection(new TimeSpan(0, 0, 3), trayIcon, cancellationToken);

        }

        private static async void DownloadFile(string fileURL, string fileDestination)
        {
            // https://jonathancrozier.com/blog/how-to-download-files-using-c-sharp

            try
            {

                var httpClient = new HttpClient();

                using (var stream = await httpClient.GetStreamAsync(fileURL))
                {
                    using (var fileStream = new FileStream(fileDestination, FileMode.CreateNew))
                    {
                        await stream.CopyToAsync(fileStream);
                    }
                }
            }
            catch { }
        }

        private void ExecuteSSHCommand(string command)
        {
                // https://stackoverflow.com/questions/56110410/run-a-powershell-script-from-c-sharp
                // https://docs.microsoft.com/en-us/powershell/scripting/developer/prog-guide/runspace01-csharp-code-sample?view=powershell-7.2

            if (IsPlinkAvailable())
            {
                command = "& '" + plink + "' -ssh 192.168.0.1 -l root -batch -pw frk9x07 -a " + command + " 2>&1";

                // // FOR DEBUGGING // //
                System.Windows.Forms.Clipboard.SetText(command);

                PowerShell ps = PowerShell.Create();
                ps.AddScript(command);
                ps.Invoke();
            }
        }

        private bool IsOnline(string address)
        {
            // https://stackoverflow.com/questions/7523741/how-do-you-check-if-a-website-is-online-in-c

            var ping = new System.Net.NetworkInformation.Ping();

            var result = ping.Send(address);

            if (result.Status != System.Net.NetworkInformation.IPStatus.Success)
            { return false; }
            else
            { return true; }
        }

        private bool IsPlinkAvailable()
        {
            if (!File.Exists(plink))
            {
                DialogResult dialogResult = MessageBox.Show(
                    "plink.exe (715kb) is required to communicate via SSH. Download now?", "Download plink.exe?",
                    MessageBoxButtons.YesNo);

                if (dialogResult == DialogResult.Yes)
                {
                    // download plink.exe
                    string url = "http://the.earth.li/~sgtatham/putty/0.76/w64/plink.exe";
                    DownloadFile(url, plink);

                    WaitNSeconds(10);

                    if (!File.Exists(plink))
                    {
                        ShowNotification("Go and get it manually", "plink.exe download failure", ToolTipIcon.Warning, 5000);
                        return false;
                    }
                    else

                        ShowNotification("Placed in the current working directory", "plink.exe download complete", ToolTipIcon.Info, 5000);

                    return true;
                }
                else if (dialogResult == DialogResult.No) { return false; }

                return false;
            }
            else { return true; }
        }

        private void MenuReboot(object? sender, EventArgs e)
        {
            if (IsOnline(address))
            {
                ExecuteSSHCommand("reboot");
            }
            else
            {
                ShowNotification("T9 is not connected", "Reboot Command Not Sent", ToolTipIcon.Error, 5000);
            }
            
        }

        private void MenuPing(object? sender, EventArgs e)
        {
            if (IsOnline(address))
            {
                ShowNotification("T9 is online", "Status", ToolTipIcon.Info, 5000);
                return;
            }

            ShowNotification("T9 is offline", "Status", ToolTipIcon.Warning, 5000);
        }

        private void MenuWebpanel(object? sender, EventArgs e)
        {
            OpenWebsite("http://" + address);
        }

        private void MenuHidden(object? sender, EventArgs e)
        {
            OpenWebsite("http://" + address + "/hidden/debug-lte_engineering.html");
        }
        private void MenuWebPST(object? sender, EventArgs e)
        {
            OpenWebsite("http://" + address + "/webpst/fota_test.html");
        }
        private void MenuITAdmin(object? sender, EventArgs e)
        {
            OpenWebsite("http://" + address + "/itadmin/admin_configuration-wifi_basic.html");
        }
        private void MenuEngineering(object? sender, EventArgs e)
        {
            OpenWebsite("http://" + address + "/engineering/franklin/imei_mac.html");
        }
        private void MenuCheatSheet(object? sender, EventArgs e)
        {
            OpenWebsite("https://docs.google.com/document/d/1LgYLB0sJbwAMW2VfcNoGavIJQhiYCJzsWK4onOFdSys/edit");
        }

        private void MenuExit(object? sender, EventArgs e)
        {
            trayIcon.Visible = false;

            Application.Exit();
        }

        private void OpenWebsite(string url)
        {
                // https://stackoverflow.com/questions/4580263/how-to-open-in-default-browser-in-c-sharp

            try
            {
                Process.Start(url);
            }
            catch
            {
                // hack because of this: https://github.com/dotnet/corefx/issues/10361
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    url = url.Replace("&", "^&");
                    Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    Process.Start("xdg-open", url);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    Process.Start("open", url);
                }
                else
                {
                    throw;
                }
            }
        }

        private void RegisterInStartup(bool isChecked)
        {
                // https://stackoverflow.com/questions/5089601/how-to-run-a-c-sharp-application-at-windows-startup

            RegistryKey registryKey = Registry.CurrentUser.OpenSubKey
                    ("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);

            if (isChecked)
            {
                registryKey.SetValue("T9Manager", Application.ExecutablePath);
            }
            else
            {
                registryKey.DeleteValue("T9Manager");
            }
        }
        private void ShowNotification(string text, string title, ToolTipIcon icon, int seconds)
        {
            trayIcon.BalloonTipText = text;
            trayIcon.BalloonTipTitle = title;
            trayIcon.BalloonTipIcon = icon;
            trayIcon.ShowBalloonTip(seconds * 1000);
        }

        private void WaitNSeconds(int seconds)
        {
                // https://stackoverflow.com/questions/22158278/wait-some-seconds-without-blocking-ui-execution

            if (seconds < 1) return;
            DateTime _desired = DateTime.Now.AddSeconds(seconds);
            while (DateTime.Now < _desired)
            {
                System.Windows.Forms.Application.DoEvents();
            }
        }

        public async Task PeriodicCheckConnection(TimeSpan interval, NotifyIcon trayIcon, CancellationToken cancellationToken)
        {
                // https://stackoverflow.com/questions/30462079/run-async-method-regularly-with-specified-interval

            while (true)
            {
               if ( IsOnline(address) ) 
                {
                    trayIcon.Icon = T9Manager.Properties.Resources.t9_online;
                }
               else
                {
                    trayIcon.Icon = T9Manager.Properties.Resources.t9_offline;
                };

                await Task.Delay(interval, cancellationToken);
            }
        }

    }
}
