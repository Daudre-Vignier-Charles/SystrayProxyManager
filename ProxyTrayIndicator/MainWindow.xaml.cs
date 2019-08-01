using System;
using System.Windows;


namespace ProxyTrayIndicator
{
    public partial class MainWindow : Window
    {
        // Mutex is used to allow only one instance of ProxyTrayIndicator
        private static System.Threading.Mutex mutex = new System.Threading.Mutex(true, "{8F6E0AC4-B9A1-45fd-AC6F-73F04E6BDE8F}");
        private static string key = @"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Internet Settings\";
        private static string name = "ProxyEnable";
        private bool proxySet = true;
        private System.Windows.Forms.NotifyIcon notifyIcon = new System.Windows.Forms.NotifyIcon()
        {
            Text = (string)Microsoft.Win32.Registry.GetValue(key, "ProxyServer", "Aucune"),
            Visible = true
        };
        private System.Windows.Forms.ContextMenu menu = new System.Windows.Forms.ContextMenu();
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

        public MainWindow()
        {
            if (!mutex.WaitOne(TimeSpan.Zero, true))
            {
                MessageBox.Show("only one instance at a time");
                this.Close();
                return;
            }
            Closing += OnClose;
            InitializeComponent();
            menu.MenuItems.Add("Show IE settings", new EventHandler(LaunchIEParamClick));
            menu.MenuItems.Add("Copyright", new EventHandler(ShowC));
            menu.MenuItems.Add("Exit", new EventHandler(ExitClick));
            notifyIcon.ContextMenu = menu;
            notifyIcon.Click += Click;
            SetTimer();
        }

        /// <summary>
        /// Switch proxy state on tray icon click
        /// </summary>
        private void Click(Object sender, EventArgs e)
        {
            if (((System.Windows.Forms.MouseEventArgs)e).Button == System.Windows.Forms.MouseButtons.Left)
            {
                if (proxySet)
                    Microsoft.Win32.Registry.SetValue(key, name, 0, Microsoft.Win32.RegistryValueKind.DWord);
                else
                    Microsoft.Win32.Registry.SetValue(key, name, 1, Microsoft.Win32.RegistryValueKind.DWord);
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
            if ((int)Microsoft.Win32.Registry.GetValue(key, name, 8) == 1)
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

        private void ExitClick(object sender, EventArgs e) =>
            this.Close();

        private void ShowC(object sender, EventArgs e) =>
            MessageBox.Show(Resource.License, "Copyrights and licenses");

        private void LaunchIEParamClick(object sender, EventArgs e) =>
            proc.Start();

        private void OnClose(Object sender, EventArgs e)
        {
            mutex.ReleaseMutex();
            notifyIcon.Dispose();
            return;
        }
    }
}
