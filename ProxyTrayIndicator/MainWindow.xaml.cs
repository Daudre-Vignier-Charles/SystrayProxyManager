using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;

namespace ProxyTrayIndicator
{
    public partial class MainWindow : Window
    {
        // Mutex is used to allow only one instance of ProxyTrayIndicator
        private bool close = false;
        private Proxy currentProxy;
        private string savePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\proxysSave";
        private static System.Threading.Mutex mutex = new System.Threading.Mutex(false, "{10000C-B9A1-45fd-AC6F-73F04E6BDE8F}");
        private static string key = @"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Internet Settings\";
        private static string regProxyEnable = "ProxyEnable";
        private static string regProxyServer = "ProxyServer";
        private bool proxySet = true;
        private bool internalProxySet = false;
        private System.Windows.Forms.NotifyIcon notifyIcon = new System.Windows.Forms.NotifyIcon()
        {
            Text = (string)Microsoft.Win32.Registry.GetValue(key, "ProxyServer", "Aucune"),
            Visible = true
        };
        private System.Windows.Forms.ContextMenu menu = new System.Windows.Forms.ContextMenu();
        private System.Windows.Forms.MenuItem force;
        private System.Windows.Forms.MenuItem proxyMenu = new System.Windows.Forms.MenuItem() { Text = "Set proxy" };
        private static System.Timers.Timer timer;
        private System.Diagnostics.Process proc = new System.Diagnostics.Process()
        {
            //  Microsoft Windows Internet Settings
            StartInfo = new System.Diagnostics.ProcessStartInfo("cmd", "/c " + "inetcpl.cpl ,4")
            {
                CreateNoWindow = true,
                UseShellExecute = false,
            }
        };
        private List<Proxy> proxys = new List<Proxy>();

        public MainWindow()
        {
            this.Closing += EventClosing;
            this.Closed += EventClosed;
            if (!mutex.WaitOne(TimeSpan.Zero, true))
            {
                System.Windows.Forms.MessageBox.Show("only one instance at a time");
                this.Close();
                return;
            }
            InitializeComponent();
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            force = new System.Windows.Forms.MenuItem("Force mode", ForceSwitch) { Checked = false };
            LoadProxys();
            BuildMenu();
            notifyIcon.ContextMenu = menu;
            notifyIcon.Click += Click;
            SetTimer();
            dgProxy.ItemsSource = proxys;
            string tmp = (string)Microsoft.Win32.Registry.GetValue(key, "ProxyServer", "");
            if (String.IsNullOrWhiteSpace(tmp))
            {
                currentProxy = new Proxy
                {
                    Address = "",
                    Name = "",
                    Port = ""
                };
            }
            else
            {
                currentProxy = new Proxy
                {
                    Address = tmp.Substring(0, tmp.IndexOf(":")),
                    Name = "",
                    Port = tmp.Substring(tmp.IndexOf(":")+1)
                };
            }
        }

        /// <summary>
        /// Switch proxy state on tray icon click
        /// </summary>
        private void Click(Object sender, EventArgs e)
        {
            if (((System.Windows.Forms.MouseEventArgs)e).Button == System.Windows.Forms.MouseButtons.Left)
            {
                if (proxySet)
                {
                    Microsoft.Win32.Registry.SetValue(key, regProxyEnable, 0, Microsoft.Win32.RegistryValueKind.DWord);
                    proxySet = false;
                    internalProxySet = false;
                }
                else
                {
                    Microsoft.Win32.Registry.SetValue(key, regProxyEnable, 1, Microsoft.Win32.RegistryValueKind.DWord);
                    proxySet = true;
                    internalProxySet = true;
                }
            }
        }

        /// <summary>
        /// Every one second, Timer_Elapsed is called
        /// </summary>
        private void SetTimer()
        {
            timer = new System.Timers.Timer(1000);
            timer.Elapsed += Timer_Elapsed;
            timer.AutoReset = true;
            timer.Enabled = true;
        }

        /// <summary>
        /// Check proxy state and update tray icon
        /// </summary>
        private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            notifyIcon.Text = (string)Microsoft.Win32.Registry.GetValue(key, "ProxyServer", "no proxy server");
            if (force.Checked)
            {
                if (notifyIcon.Text != currentProxy.ToString())
                {
                    Microsoft.Win32.Registry.SetValue(key, regProxyServer, currentProxy.ToString(), Microsoft.Win32.RegistryValueKind.String);
                }
                if (internalProxySet)
                {
                    Microsoft.Win32.Registry.SetValue(key, regProxyEnable, 1, Microsoft.Win32.RegistryValueKind.DWord);
                    proxySet = true;
                }
                else
                {
                    Microsoft.Win32.Registry.SetValue(key, regProxyEnable, 0, Microsoft.Win32.RegistryValueKind.DWord);
                    proxySet = false;
                }
            }
            if ((int)Microsoft.Win32.Registry.GetValue(key, regProxyEnable, 8) == 1)
            {
                proxySet = true;
                notifyIcon.Icon = Resource.on;
            }
            else
            {
                proxySet = false;
                notifyIcon.Icon = Resource.off;
            }
        }

        private void ForceSwitch(object sender, EventArgs e)
        {
            if (force.Checked)
            {
                force.Checked = false;
            }
            else
            {
                force.Checked = true;
            }
        }

        private void ExitClick(object sender, EventArgs e)
        {
            this.close = true;
            this.Close();
        }

        private void ShowC(object sender, EventArgs e) =>
            System.Windows.Forms.MessageBox.Show(Resource.License, "Copyrights and licenses");

        private void LaunchIEParamClick(object sender, EventArgs e) =>
            proc.Start();

        private void EventClosing(Object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (! this.close)
            {
                UpdateProxyMenu();
                this.Hide();
                this.ShowInTaskbar = false;
                e.Cancel = true;
            }
        }
        private void EventClosed(Object sender, EventArgs e)
        {
            SaveProxys();
            mutex.ReleaseMutex();
            notifyIcon.Dispose();
            return;
        }

        private void BuildMenu()
        {
            menu.MenuItems.Add("Exit", new EventHandler(ExitClick));
            menu.MenuItems.Add("Copyright and sources", new EventHandler(ShowC));
            menu.MenuItems.Add("Show IE settings", new EventHandler(LaunchIEParamClick));
            menu.MenuItems.Add("Clear proxy", new EventHandler(ClearProxy));
            menu.MenuItems.Add(3, force);
            menu.MenuItems.Add("Edit proxys", new EventHandler(EditProxy));
            UpdateProxyMenu();
        }

        private void UpdateProxyMenu()
        {
            proxyMenu.MenuItems.Clear();
            proxyMenu.MenuItems.AddRange(GetProxyList());
            menu.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] { proxyMenu });
        }

        private System.Windows.Forms.MenuItem[] GetProxyList()
        {
            List<System.Windows.Forms.MenuItem> menu = new List<System.Windows.Forms.MenuItem>();
            foreach (Proxy proxy in proxys)
            {
                menu.Add(new System.Windows.Forms.MenuItem(proxy.Name, SetProxy) { Tag = proxy });
            }
            return menu.ToArray();
        }

        private void ClearProxy(Object sender, EventArgs e)
        {
            Microsoft.Win32.Registry.SetValue(key, regProxyServer, "", Microsoft.Win32.RegistryValueKind.String);
        }

        private void SetProxy(Object sender, EventArgs e)
        {
            System.Windows.Forms.MenuItem item = sender as System.Windows.Forms.MenuItem;
            if (item != null)
            {
                Proxy proxy = item.Tag as Proxy;
                if (proxy != null)
                {
                    Microsoft.Win32.Registry.SetValue(key, regProxyServer, proxy.ToString(), Microsoft.Win32.RegistryValueKind.String);
                    currentProxy = proxy;
                }
            }
        }

        private void EditProxy(Object sender, EventArgs e)
        {
            CenterWindowOnScreen();
            this.Show();
            this.ShowInTaskbar = true;
        }

        private void SaveProxys()
        {
            Serializer.Save(savePath, proxys);
        }

        private void LoadProxys()
        {
            if (File.Exists(savePath))
                proxys = Serializer.Load<List<Proxy>>(savePath);
        }

        private void CenterWindowOnScreen()
        {
            double screenWidth = System.Windows.SystemParameters.PrimaryScreenWidth;
            double screenHeight = System.Windows.SystemParameters.PrimaryScreenHeight;
            double windowWidth = this.Width;
            double windowHeight = this.Height;
            this.Left = (screenWidth / 2) - (windowWidth / 2);
            this.Top = (screenHeight / 2) - (windowHeight / 2);
        }
    }
}
