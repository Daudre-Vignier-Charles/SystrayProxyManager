using Hardcodet.Wpf.TaskbarNotification;
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Threading;

namespace SystrayProxyManager
{
    enum ExitOrigin { User, Mutex, None};
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ExitOrigin exitOrigin = ExitOrigin.None;
        private Proxies proxies = new Proxies();
        private DispatcherTimer timer;
        private static System.Threading.Mutex mutex = new System.Threading.Mutex(false, "{5D1B5C13-C70C-43CA-8039-7A353F026452}");

        public MainWindow()
        {
            InitializeComponent();
            if (!mutex.WaitOne(TimeSpan.Zero, true))
            {
                MessageBox.Show("only one instance at a time");
                exitOrigin = ExitOrigin.Mutex;
                this.Close();
                return;
            }
            SetTimer();
            taskBarIcon.Visibility = Visibility.Visible;
            proxies.userDefinedProxyState = proxies.CurrentProxyState;
            proxies.userDefinedProxyServer = proxies.CurrentProxyServer;
            taskBarIcon.LeftClickCommand = new TaskBarIcon_LeftClick(proxies, taskBarIcon);
            taskBarIcon.ToolTipText = proxies.CurrentProxyServer.ToString();
            UpdateSetProxyMenu();
            dgProxy.ItemsSource = proxies.proxies;
        }

        #region UI update
        private void UpdateSetProxyMenu()
        {
            menu_proxy.Items.Clear();
            foreach (Proxy proxy in proxies.proxies)
            {
                MenuItem item = new MenuItem() { Header = proxy.Name, Tag = proxy};
                item.Click += SetProxy_Click;
                menu_proxy.Items.Add(item);
            }
        }
        #endregion UI update

        #region Timer
        private void SetTimer()
        {
            timer = new DispatcherTimer();
            timer.Tick += new EventHandler(Timer_Elapsed);
            timer.Interval = new TimeSpan(0, 0, 1);
            timer.Start();
        }

        /// <summary>
        /// Check proxy state and update tray icon
        /// </summary>
        private void Timer_Elapsed(object sender, EventArgs e)
        {
            taskBarIcon.ToolTipText = proxies.CurrentProxyServer.ToString();
            if (menu_Force.IsChecked)
            {
                if (taskBarIcon.ToolTipText != proxies.userDefinedProxyServer.ToString())
                    proxies.CurrentProxyServer = proxies.userDefinedProxyServer;
                if (proxies.CurrentProxyState != proxies.userDefinedProxyState)
                    proxies.CurrentProxyState = proxies.userDefinedProxyState;
            }
            if (proxies.CurrentProxyState)
                taskBarIcon.Icon = Resource.on;
            else
                taskBarIcon.Icon = Resource.off;
        }
        #endregion Timer

        #region MainMenu click events
        private void Exit_Click(Object sender, RoutedEventArgs e)
        {
            exitOrigin = ExitOrigin.User;
            this.Close();
        }

        private void Copyright_Click(Object sender, RoutedEventArgs e) =>
            MessageBox.Show(Resource.License, "SystrayProxyManager - Copyrights and license",
                MessageBoxButton.OK,
                MessageBoxImage.Information,
                MessageBoxResult.OK,
                MessageBoxOptions.DefaultDesktopOnly);

        private void IESettings_Click(Object sender, RoutedEventArgs e) =>
            IESettings.Run();

        private void Clear_Click(Object sender, RoutedEventArgs e) =>
            proxies.CurrentProxyServer = null;

        private void Force_Click(Object sender, RoutedEventArgs e) =>
            menu_Force.IsChecked = menu_Force.IsChecked ? true : false;

        private void Edit_Click(Object sender, RoutedEventArgs e)
        {
            CenterWindowOnScreen();
            this.Show();
            this.ShowInTaskbar = true;
        }

        private void SetProxy_Click(Object sender, EventArgs e)
        {
            MenuItem item = sender as MenuItem;
            if (item != null)
            {
                Proxy proxy = item.Tag as Proxy;
                if (proxy != null)
                {
                    proxies.CurrentProxyServer = proxy;
                    proxies.userDefinedProxyServer = proxy;
                }
            }
        }
        #endregion MainMenu click events

        #region MainWindow events
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (exitOrigin == ExitOrigin.None)
            {
                UpdateSetProxyMenu();
                this.Hide();
                this.ShowInTaskbar = false;
                e.Cancel = true;
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if (exitOrigin == ExitOrigin.User)
            {
                timer.Stop();
                proxies.SaveProxies();
                mutex.ReleaseMutex();
            }
        }
        #endregion MainWindow events

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
    public class TaskBarIcon_LeftClick : ICommand
    {
        Proxies proxies;
        TaskbarIcon taskBarIcon;
        public event EventHandler CanExecuteChanged;
        public TaskBarIcon_LeftClick(Proxies proxies, TaskbarIcon taskBarIcon)
        {
            this.proxies = proxies;
            this.taskBarIcon = taskBarIcon;
        }
        public void Execute(object o)
        {
            if (proxies.CurrentProxyState)
            {
                proxies.CurrentProxyState = false;
                proxies.userDefinedProxyState = false;
                taskBarIcon.Icon = Resource.on;
            }
            else
            {
                proxies.CurrentProxyState = true;
                proxies.userDefinedProxyState = true;
                taskBarIcon.Icon = Resource.off;
            }
            
        }
        public bool CanExecute(object parameter) =>
            true;
    }

    internal static class IESettings
    {
        private static System.Diagnostics.Process InternetSettings = new System.Diagnostics.Process()
        {
            //  Microsoft Windows Internet Settings window
            StartInfo = new System.Diagnostics.ProcessStartInfo("cmd", "/c " + "inetcpl.cpl ,4")
            {
                CreateNoWindow = true,
                UseShellExecute = false,
            }
        };

        internal static void Run() =>
            InternetSettings.Start();
    }
}
