using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Forms;

namespace ProxyTrayIndicator
{
    public partial class MainWindow : Window
    {
        Proxies proxies = new Proxies();
        private bool close = false;
        private Proxy userDefinedProxy;
        private bool internalProxySet = false;
        private static System.Threading.Mutex mutex = new System.Threading.Mutex(false, "{10000O-B9A1-45fd-AC6F-73F04E6BDE8F}");
        private System.Windows.Forms.NotifyIcon notifyIcon;
        private System.Windows.Forms.ContextMenu mainMenu = new System.Windows.Forms.ContextMenu();
        private System.Windows.Forms.MenuItem force;
        private System.Windows.Forms.MenuItem proxyMenu = new System.Windows.Forms.MenuItem() { Text = "Set proxy" };
        private static System.Timers.Timer timer;
        private System.Diagnostics.Process InternetSettings = new System.Diagnostics.Process()
        {
            //  Microsoft Windows Internet Settings
            StartInfo = new System.Diagnostics.ProcessStartInfo("cmd", "/c " + "inetcpl.cpl ,4")
            {
                CreateNoWindow = true,
                UseShellExecute = false,
            }
        };

        public MainWindow()
        {
            InitializeComponent();
            this.Closing += EventClosing;
            this.Closed += EventClosed;
            if (!mutex.WaitOne(TimeSpan.Zero, true))
            {
                System.Windows.Forms.MessageBox.Show("only one instance at a time");
                this.Close();
                return;
            }
            notifyIcon = new System.Windows.Forms.NotifyIcon()
            {
                Text = proxies.GetProxyServer().ToString(),
                Visible = true
            };
            proxies.LoadProxies();
            BuildMenu();
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            notifyIcon.Click += IconClick;
            SetTimer();
            userDefinedProxy = proxies.GetProxyServer();
            dgProxy.ItemsSource = proxies.proxies;
        }

        private void IconClick(Object sender, EventArgs e)
        {
            if (((System.Windows.Forms.MouseEventArgs)e).Button == System.Windows.Forms.MouseButtons.Left)
            {
                if (proxies.GetProxyState())
                {
                    proxies.SetProxyState(false);
                    internalProxySet = false;
                }
                else
                {
                    proxies.SetProxyState(true);
                    internalProxySet = true;
                }
            }
        }

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
            notifyIcon.Text = proxies.GetProxyServer().ToString();
            if (force.Checked)
            {
                if (notifyIcon.Text != userDefinedProxy.ToString())
                    proxies.SetProxyServer(userDefinedProxy);
                if (proxies.GetProxyState() != internalProxySet)
                {
                    if (internalProxySet)
                        proxies.SetProxyState(true);
                    else
                        proxies.SetProxyState(false);
                }
                
            }
            if (proxies.GetProxyState())
                notifyIcon.Icon = Resource.on;
            else
                notifyIcon.Icon = Resource.off;
        }

        private void ForceSwitch(object sender, EventArgs e) =>
            force.Checked = force.Checked ? false : true;

        private void ExitClick(object sender, EventArgs e)
        {
            this.close = true;
            this.Close();
        }

        private void ShowCopyright(object sender, EventArgs e) =>
            System.Windows.Forms.MessageBox.Show(Resource.License, "Copyrights and licenses");

        private void LaunchIEParamClick(object sender, EventArgs e) =>
            InternetSettings.Start();

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
            proxies.SaveProxies();
            mutex.ReleaseMutex();
            notifyIcon.Dispose();
            return;
        }

        private void BuildMenu()
        {
            mainMenu.MenuItems.Add("Exit", new EventHandler(ExitClick));
            mainMenu.MenuItems.Add("Copyright and sources", new EventHandler(ShowCopyright));
            mainMenu.MenuItems.Add("Show IE settings", new EventHandler(LaunchIEParamClick));
            mainMenu.MenuItems.Add("Clear proxy", new EventHandler(proxies.ClearProxyServer));
            force = new System.Windows.Forms.MenuItem("Force mode", ForceSwitch) { Checked = false };
            mainMenu.MenuItems.Add(3, force);
            mainMenu.MenuItems.Add("Edit proxys", new EventHandler(EditProxy));
            UpdateProxyMenu();
            notifyIcon.ContextMenu = mainMenu;
        }

        private void UpdateProxyMenu()
        {
            proxyMenu.MenuItems.Clear();
            proxyMenu.MenuItems.AddRange(GetProxyList());
            mainMenu.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] { proxyMenu });
        }

        private System.Windows.Forms.MenuItem[] GetProxyList()
        {
            List<System.Windows.Forms.MenuItem> menu = new List<System.Windows.Forms.MenuItem>();
            foreach (Proxy proxy in proxies.proxies)
                menu.Add(new System.Windows.Forms.MenuItem(proxy.Name, SetProxy) { Tag = proxy });
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
                    proxies.SetProxyServer(proxy);
                    userDefinedProxy = proxy;
                }
            }
        }

        private void EditProxy(Object sender, EventArgs e)
        {
            CenterWindowOnScreen();
            this.Show();
            this.ShowInTaskbar = true;
        }

        private void CenterWindowOnScreen()
        {
            double screenWidth = System.Windows.SystemParameters.PrimaryScreenWidth;
            double screenHeight = System.Windows.SystemParameters.PrimaryScreenHeight;
            double windowWidth = this.Width;
            double windowHeight = this.Height;
            this.Left = (screenWidth - windowWidth) / 2;
            this.Top = (screenHeight - windowHeight) / 2;
        }
    }
}
