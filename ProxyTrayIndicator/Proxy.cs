using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Data;

namespace ProxyTrayIndicator
{
    public class Proxies
    {
        public List<Proxy> proxies = new List<Proxy>();
        private string path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\proxysSave";
        private static string key = @"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Internet Settings\";

        internal void SaveProxies() =>
            Serializer.Save(path, proxies);

        internal void LoadProxies()
        {
            if (File.Exists(path))
                proxies = Serializer.Load<List<Proxy>>(path);
        }

        internal bool GetProxyState()
        {
            if ((int)Microsoft.Win32.Registry.GetValue(key, "ProxyEnable", 2) == 1)
                return true;
            else
                return false;
        }

        internal void SetProxyState(bool state)
        {
            if (state)
                Microsoft.Win32.Registry.SetValue(key, "ProxyEnable", 1, Microsoft.Win32.RegistryValueKind.DWord);
            else
                Microsoft.Win32.Registry.SetValue(key, "ProxyEnable", 0, Microsoft.Win32.RegistryValueKind.DWord);
        }

        internal void ClearProxyServer(Object sender, EventArgs e) =>
            Microsoft.Win32.Registry.SetValue(key, "ProxyServer", "", Microsoft.Win32.RegistryValueKind.String);

        internal Proxy GetProxyServer()
        {
            string address = (string)Microsoft.Win32.Registry.GetValue(key, "ProxyServer", "");
            return new Proxy()
            {
                Name = String.IsNullOrWhiteSpace(address) ? "" : address.Substring(0, address.IndexOf(":")),
                Address = String.IsNullOrWhiteSpace(address) ? "" : address.Substring(0, address.IndexOf(":")),
                Port = String.IsNullOrWhiteSpace(address) ? "" : address.Substring(address.IndexOf(":") + 1)
            };
        }

        internal void SetProxyServer(Proxy proxy) =>
            Microsoft.Win32.Registry.SetValue(key, "ProxyServer", proxy.ToString(), Microsoft.Win32.RegistryValueKind.String);

        internal List<Proxy> GetValidatedProxies()
        {
            ProxyValidationRule rule = new ProxyValidationRule();
            List<Proxy> lst = new List<Proxy>();
            foreach (Proxy proxy in proxies.ToArray())
            {
                if (rule.Validate(proxy, Thread.CurrentThread.CurrentCulture) == ValidationResult.ValidResult)
                {
                    lst.Add(proxy);
                }
            }
            return lst;
        }
    }

    [Serializable]
    public class Proxy
    {
        public string Name { get; set; }
        public string Address { get; set; }
        public string Port { get; set; }

        public override string ToString() =>
            String.IsNullOrWhiteSpace(Address) ? "" : String.Format("{0}:{1}", Address, Port);
    }

    public class ProxyValidationRule : ValidationRule
    {
        int i;
        public override ValidationResult Validate(object value,
            System.Globalization.CultureInfo cultureInfo)
        {
            Proxy proxy = value as Proxy;
            if (proxy == null)
            {
                proxy = (value as BindingGroup).Items[0] as Proxy;
            }
            if (String.IsNullOrWhiteSpace(proxy.Name))
            {
                return new ValidationResult(false,
                    "Name cannot be empty");
            }
            else if (string.IsNullOrWhiteSpace(proxy.Address))
            {
                return new ValidationResult(false,
                    "Address cannot be empty");
            }
            else if (!int.TryParse(proxy.Port, out i))
            {
                return new ValidationResult(false,
                    "Port must be a numeric value and cannot be empty");
            }
            else
            {
                return ValidationResult.ValidResult;
            }
        }
    }
}
