using System.Collections.Generic;
using System.Linq;

using System.Windows;
using System.Windows.Controls;

using System.Net.NetworkInformation;

namespace NetworkInformations
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();

        public MainWindow()
        {
            InitializeComponent();
            LoadNetworkInterfaces();
        }

        private string formatMacAddress(string macAddress)
        {
            return string.Join(":", Enumerable.Range(0, 6).Select(i => macAddress.Substring(i * 2, 2)));
        }

        private void LoadNetworkInterfaces()
        {
            foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                if ((nic.NetworkInterfaceType == NetworkInterfaceType.Ethernet) || (nic.NetworkInterfaceType == NetworkInterfaceType.Wireless80211) && (nic.OperationalStatus == OperationalStatus.Up))
                {
                    foreach (UnicastIPAddressInformation ip in nic.GetIPProperties().UnicastAddresses)
                    {
                        if (ip.Address.ToString().Substring(0,7) != "169.254")
                        {
                            InterfaceSelector.Items.Add(new ComboBoxItem()
                            {
                                Content = nic.Description.ToString()
                            });
                        }
                    }
                }
            }
        }

        private void InterfaceSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string selectedInterface =  ((sender as ComboBox).SelectedItem as ComboBoxItem).Content.ToString();
            foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                foreach (UnicastIPAddressInformation ip in nic.GetIPProperties().UnicastAddresses)
                {
                    if (nic.Description == selectedInterface && ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    {
                        IPInterfaceProperties IpProperties = nic.GetIPProperties();
                        BoxIP.Text = ip.Address.ToString();
                        BoxMAC.Text = formatMacAddress(nic.GetPhysicalAddress().ToString());
                        Dictionary<string, string> defaultIpProperties = GetDefaultIpProperties(IpProperties);
                        BoxGateway.Text = defaultIpProperties["gateway"] ?? "";
                        BoxDHCP.Text = defaultIpProperties["dhcp"] ?? "";
                        BoxDNS.Text = defaultIpProperties["dns"] ?? "";
                    }
                }
            }
        }

        // gateway, dhcp, dns
        private Dictionary<string, string> GetDefaultIpProperties(IPInterfaceProperties IpInterfaceProperties)
        {
            if (IpInterfaceProperties == null)
            {
                return null;
            }
            string defaultGateway;
            string defaultDhcp;
            string defaultDns;
            defaultGateway = IpInterfaceProperties.GatewayAddresses?.FirstOrDefault()?.Address.ToString();
            defaultDhcp = System.Net.IPAddress.Parse(IpInterfaceProperties.DhcpServerAddresses?.FirstOrDefault()?.Address.ToString()).ToString();
            defaultDns = System.Net.IPAddress.Parse(IpInterfaceProperties.DnsAddresses?.FirstOrDefault()?.Address.ToString()).ToString();
            
            return new Dictionary<string, string>
            {
                {"gateway", defaultGateway },
                {"dhcp", defaultDhcp },
                {"dns", defaultDns }
            };
        }
    }
}
