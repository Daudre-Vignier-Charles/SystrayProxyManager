using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Net;
using System.Drawing;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ProxyTrayIndicator
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static string key = @"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Internet Settings\";
        private static string name = "ProxyEnable";
        private bool proxySet = true;
        System.Windows.Forms.NotifyIcon notifyIcon = new System.Windows.Forms.NotifyIcon()
        {
            Text = String.Format("URL: {0}", (string)Microsoft.Win32.Registry.GetValue(key, "ProxyServer", "Aucune")),
            Visible = true
        };
        System.Windows.Forms.ContextMenu menu = new System.Windows.Forms.ContextMenu();
        private static System.Timers.Timer timer;

        private static System.Diagnostics.ProcessStartInfo procStartInfo = new System.Diagnostics.ProcessStartInfo("cmd", "/c " + "inetcpl.cpl ,4")
        {
            CreateNoWindow = true,
            UseShellExecute = false,
        };
        System.Diagnostics.Process proc = new System.Diagnostics.Process()
        {
            StartInfo = procStartInfo
        };

        public   MainWindow()
        {
            InitializeComponent();
            menu.MenuItems.Add("Show IE settings", new EventHandler(LaunchIEParamClick));
            menu.MenuItems.Add("Exit", new EventHandler(ExitClick));
            menu.MenuItems.Add("Copyright", new EventHandler(ShowC));
            notifyIcon.ContextMenu = menu;
            notifyIcon.Click += Click;
            SetTimer();
        }

        private void Click(Object sender, EventArgs e)
        {
            System.Windows.Forms.MouseEventArgs z = (System.Windows.Forms.MouseEventArgs)e;
            if (z.Button == System.Windows.Forms.MouseButtons.Left)
            {
                if (proxySet)
                    Microsoft.Win32.Registry.SetValue(key, name, 0, Microsoft.Win32.RegistryValueKind.DWord);
                else
                    Microsoft.Win32.Registry.SetValue(key, name, 1, Microsoft.Win32.RegistryValueKind.DWord);
            }
        }

        private void SetTimer()
        {
            timer = new System.Timers.Timer(1000); //Timer goes off every 1 seconds
            timer.Elapsed += Timer_Elapsed;
            timer.AutoReset = true;
            timer.Enabled = true;
        }

        private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            int res = (int)Microsoft.Win32.Registry.GetValue(key, name, 8);
            if (res == 1)
            {
                proxySet = true;
                notifyIcon.Icon = Resource.on;
            }
            else
            {
                proxySet = false;
                notifyIcon.Icon = Resource.off;
            }
            notifyIcon.Text = String.Format("URL: {0}", (string)Microsoft.Win32.Registry.GetValue(key, "ProxyServer", "Aucune"));
        }

        private void ExitClick(object sender, EventArgs e)
        {
            notifyIcon.Dispose();
            this.Close();
        }

        private void ShowC(object sender, EventArgs e)
        {
            MessageBox.Show(Resource.License, "Copyrights and licenses");
        }

        private void LaunchIEParamClick(object sender, EventArgs e)
        {
            proc.Start();
        }
    }
}
