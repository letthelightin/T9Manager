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
using System.Diagnostics;               // for starting processes in order to open URL in browser window
using System.Runtime.InteropServices;
using Microsoft.Win32;                  // for adding autostart to registry

namespace Franklin_T9_Manager
{
    public class MyCustomApplicationContext : ApplicationContext
    {
        // https://stackoverflow.com/questions/15653921/get-current-folder-path

        static string directory = System.AppDomain.CurrentDomain.BaseDirectory;
        static string plink = directory + "plink.exe";

        string address = "192.168.0.1";

        string clipboard = "";


        // https://stackoverflow.com/questions/995195/how-can-i-make-a-net-windows-forms-application-that-only-runs-in-the-system-tra

        private NotifyIcon trayIcon;

        public MyCustomApplicationContext()
        {
            ContextMenuStrip contextMenuStrip = new()
            {
                ShowCheckMargin = true,
                ShowImageMargin = false,
                Font = new System.Drawing.Font("Segoe UI", 10),
                Items =
                    {

                        new ToolStripMenuItem("Reboot", null, MenuReboot),
                        new ToolStripMenuItem(address, null, MenuWebpanel)
                        {
                            DropDownItems =
                            {   
                                new ToolStripMenuItem("hidden", null, MenuHidden),
                                new ToolStripMenuItem("webpst", null, MenuWebPST),
                                new ToolStripMenuItem("itadmin", null, MenuITAdmin),
                                new ToolStripMenuItem("engineering", null, MenuEngineering),
                            }

                        },
                        new ToolStripMenuItem("Speed Test", null, MenuSpeedTest),
                        new ToolStripMenuItem("About T9", null, MenuAbout),
                        new ToolStripMenuItem("Autostart", null, MenuAutostart),
                        new ToolStripMenuItem("Exit", null, MenuExit),
                    }
            };

            // the following line introduces the windows 10 dark grey on grey menu theme via MyRenderer.cs
            contextMenuStrip.Renderer = new MyRenderer();

            trayIcon = new NotifyIcon()
            {
                Icon = T9Manager.Properties.Resources.t9,

                Visible = true,

                Text = "T9 Manager",

                ContextMenuStrip = contextMenuStrip
            };

            this.trayIcon.MouseClick += new MouseEventHandler(TrayIconClick);

            _ = PeriodicCheckConnectionUpdateIcon(new TimeSpan(0, 0, 1), trayIcon);

        }

        void CopyToClipBoard(object sender, EventArgs e)
        {
            if (clipboard != "")
            {
                Clipboard.SetDataObject(clipboard, true, 2, 2);
            }
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
                //System.Windows.Forms.Clipboard.SetText(command);

                PowerShell ps = PowerShell.Create();
                ps.AddScript(command);
                ps.Invoke();
            }
        }


        private bool IsT9Connected()
        {
            // https://stackoverflow.com/questions/7523741/how-do-you-check-if-a-website-is-online-in-c

            string address = "192.168.0.1";

            var ping = new System.Net.NetworkInformation.Ping();

            var result = ping.Send(address, 150);

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
        
        private void DoNothingSpacer(object? sender, EventArgs e)
        {
        }

        private void MenuReboot(object? sender, EventArgs e)
        {
            if (IsT9Connected())
            {
                ExecuteSSHCommand("reboot");
            }
            else
            {
                ShowNotification("T9 is not connected", "Reboot Command Not Sent", ToolTipIcon.Error, 5000);
            }

        }

        private void MenuWebpanel(object? sender, EventArgs e)
        {
            OpenWebsite("http://" + address);
        }

        private void MenuHidden(object? sender, EventArgs e)
        {
            OpenWebsite("http://" + address + "/hidden/debug-lte_engineering.html");

            clipboard = "frk@r717";

            ShowNotification("password: frk@r717", "Click to copy password", ToolTipIcon.Info, 5);
        }
        
        private void MenuWebPST(object? sender, EventArgs e)
        {
            OpenWebsite("http://" + address + "/webpst/fota_test.html");

            clipboard = "frk@r717";

            ShowNotification("password: frk@r717", "Click to copy password", ToolTipIcon.Info, 5);
        }
        
        private void MenuITAdmin(object? sender, EventArgs e)
        {
            OpenWebsite("http://" + address + "/itadmin/admin_configuration-wifi_basic.html");

            clipboard = "t9_it_@dmin";

            ShowNotification("password: t9_it_@dmin", "Click to copy password", ToolTipIcon.Info, 5);
        }
        
        private void MenuEngineering(object? sender, EventArgs e)
        {
            OpenWebsite("http://" + address + "/engineering/franklin/imei_mac.html");

            clipboard = "frkengr717";

            System.Text.StringBuilder HTMLString = new ();

            ShowNotification("user: r717 password: frkengr717", "Click to copy password", ToolTipIcon.Info, 5);
        }
        
        private void MenuCheatSheet(object? sender, EventArgs e)
        {
            OpenWebsite("https://docs.google.com/document/d/1LgYLB0sJbwAMW2VfcNoGavIJQhiYCJzsWK4onOFdSys/edit");
        }
        
        private void MenuAbout(object? sender, EventArgs e)
        {
            OpenWebsite("https://docs.google.com/document/d/1LgYLB0sJbwAMW2VfcNoGavIJQhiYCJzsWK4onOFdSys/edit");
        }

        private void MenuSpeedTest(object? sender, EventArgs e)
        {
            OpenWebsite("https://librespeed.org/");
        }

        private void MenuAutostart(object? sender, EventArgs e)
        {
            // https://stackoverflow.com/questions/12814584/check-mark-and-image-next-to-mainmenu-item

            RegistryKey key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);


            if ((string)key.GetValue("T9Manager") != Application.ExecutablePath)
            {
                key.SetValue("T9Manager", Application.ExecutablePath);
                SmallNotification("Autostart Enabled");
            }
            else
            {
                key.DeleteValue("T9Manager", false);
                SmallNotification("Autostart Disabled");
            };

        }

        private void MenuExit(object? sender, EventArgs e)
        {
            trayIcon.Visible = false;
            trayIcon.Dispose();

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

        public async Task PeriodicCheckConnectionUpdateIcon(TimeSpan interval, NotifyIcon trayIcon)
        {
            // https://stackoverflow.com/questions/30462079/run-async-method-regularly-with-specified-interval

            bool lastStatus = IsT9Connected();
            bool presentStatus = lastStatus;

            while (true)
            {
                if (presentStatus)
                {
                    trayIcon.Icon = T9Manager.Properties.Resources.t9_online;
                    
                    if (presentStatus != lastStatus) { SmallNotification("T9 is Online"); }
                    lastStatus = true;
                }
                else
                {
                    trayIcon.Icon = T9Manager.Properties.Resources.t9_offline;
                    if (presentStatus != lastStatus) { SmallNotification("T9 is Offline"); }
                    lastStatus = false;
                };

                await Task.Delay(interval);
                
                presentStatus = IsT9Connected();
            }
        }

        private void ShowNotification(string text, string title, ToolTipIcon icon, int seconds)
        {
            trayIcon.BalloonTipText = text;
            trayIcon.BalloonTipTitle = title;
            trayIcon.BalloonTipIcon = icon;
            trayIcon.ShowBalloonTip(seconds * 1000);
            trayIcon.BalloonTipClicked += CopyToClipBoard;
        }

        private void SmallNotification(string text)
        {
            trayIcon.BalloonTipText = text;
            trayIcon.BalloonTipTitle = "";
            trayIcon.BalloonTipIcon = ToolTipIcon.None;
            trayIcon.ShowBalloonTip(2000);
        }

        private void TrayIconClick(object? sender, MouseEventArgs e)
        {
            // https://docs.microsoft.com/en-us/dotnet/api/system.windows.forms.notifyicon.mousedoubleclick?view=windowsdesktop-6.0

            if (e.Button == MouseButtons.Left)
            {
                //SmallNotification("TrayIconClick Left");
            }
            else if (e.Button == MouseButtons.Right)
            {
                //SmallNotification("TrayIconClick Right");
            }
            else if (e.Button == MouseButtons.Middle)
            {
                //SmallNotification("TrayIconClick Middle");
            };

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
            

    }
}
