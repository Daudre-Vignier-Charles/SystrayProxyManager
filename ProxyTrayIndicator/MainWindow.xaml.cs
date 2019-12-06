using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;

namespace ProxyTrayIndicator
{
    public partial class MainWindow : Window
    {
        // Mutex is used to allow only one instance of ProxyTrayIndicator
        private bool close = false;
        private string savePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\proxysSave";
        private static System.Threading.Mutex mutex = new System.Threading.Mutex(false, "{100008-B9A1-45fd-AC6F-73F04E6BDE8F}");
        private static string key = @"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Internet Settings\";
        private static string regProxyEnable = "ProxyEnable";
        private static string regProxyServer = "ProxyServer";
        private bool proxySet = true;
        private System.Windows.Forms.NotifyIcon notifyIcon = new System.Windows.Forms.NotifyIcon()
        {
            Text = (string)Microsoft.Win32.Registry.GetValue(key, "ProxyServer", "Aucune"),
            Visible = true
        };
        private System.Windows.Forms.ContextMenu menu = new System.Windows.Forms.ContextMenu();
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
            LoadProxys();
            BuildMenu();
            notifyIcon.ContextMenu = menu;
            notifyIcon.Click += Click;
            SetTimer();
            dgProxy.ItemsSource = proxys;
        }

        /// <summary>
        /// Switch proxy state on tray icon click
        /// </summary>
        private void Click(Object sender, EventArgs e)
        {
            if (((System.Windows.Forms.MouseEventArgs)e).Button == System.Windows.Forms.MouseButtons.Left)
            {
                if (proxySet)
                    Microsoft.Win32.Registry.SetValue(key, regProxyEnable, 0, Microsoft.Win32.RegistryValueKind.DWord);
                else
                    Microsoft.Win32.Registry.SetValue(key, regProxyEnable, 1, Microsoft.Win32.RegistryValueKind.DWord);
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
            notifyIcon.Text = (string)Microsoft.Win32.Registry.GetValue(key, "ProxyServer", "no proxy server");
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
            menu.MenuItems.Add("Copyright & sources", new EventHandler(ShowC));
            menu.MenuItems.Add("Show IE settings", new EventHandler(LaunchIEParamClick));
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

        private void SetProxy(Object sender, EventArgs e)
        {
            System.Windows.Forms.MenuItem item = sender as System.Windows.Forms.MenuItem;
            if (item != null)
            {
                Proxy proxy = item.Tag as Proxy;
                if (proxy != null)
                {
                    Microsoft.Win32.Registry.SetValue(key, regProxyServer, String.Format("{0}:{1}", proxy.Address, proxy.Port), Microsoft.Win32.RegistryValueKind.String);
                }
            }
        }

        private void EditProxy(Object sender, EventArgs e)
        {
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
    }
}
