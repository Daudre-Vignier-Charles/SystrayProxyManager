using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Controls;
using System.Windows.Data;

namespace SystrayProxyManager
{
    public class Proxies
    {
        public Proxy CurrentProxyServer
        {
            get
            {
                string address = (string)Microsoft.Win32.Registry.GetValue(key, "ProxyServer", "");
                return new Proxy()
                {
                    Name = String.IsNullOrWhiteSpace(address) ? "" : address.Substring(0, address.IndexOf(":")),
                    Address = String.IsNullOrWhiteSpace(address) ? "" : address.Substring(0, address.IndexOf(":")),
                    Port = String.IsNullOrWhiteSpace(address) ? "" : address.Substring(address.IndexOf(":") + 1)
                };
            }
            set
            {
                if (value == null)
                    Microsoft.Win32.Registry.SetValue(key, "ProxyServer", "", Microsoft.Win32.RegistryValueKind.String);
                else
                    Microsoft.Win32.Registry.SetValue(key, "ProxyServer", value.ToString(), Microsoft.Win32.RegistryValueKind.String);
            }
                
        }

        public bool CurrentProxyState
        {
            get =>
                (int)Microsoft.Win32.Registry.GetValue(key, "ProxyEnable", 2) == 1;
            set
            {
                if (value)
                    Microsoft.Win32.Registry.SetValue(key, "ProxyEnable", 1, Microsoft.Win32.RegistryValueKind.DWord);
                else
                    Microsoft.Win32.Registry.SetValue(key, "ProxyEnable", 0, Microsoft.Win32.RegistryValueKind.DWord);
            }
        }

        public List<Proxy> proxies { get; set; } = new List<Proxy>();
        //public List<Proxy> proxies = new List<Proxy>();
        private static ProxyValidationRule rule = new ProxyValidationRule();
        private static string path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\SystrayProxyManager\proxy_list.dat";
        private static string key = @"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Internet Settings\";

        public Proxies()
        {
            if (File.Exists(path))
                proxies = Serializer.Load<List<Proxy>>(path);
        }

        internal void SaveProxies() =>
            Serializer.Save(path, proxies);
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
        static uint i;
        public override ValidationResult Validate(object value,
            System.Globalization.CultureInfo cultureInfo)
        {
            Proxy proxy = value as Proxy; // using if from direct call
            if (proxy == null)
                proxy = (value as BindingGroup).Items[0] as Proxy; // using if from RowValidationRules
            if (String.IsNullOrWhiteSpace(proxy.Name))
                return new ValidationResult(false, "Name cannot be empty");
            else if (string.IsNullOrWhiteSpace(proxy.Address))
                return new ValidationResult(false, "Address cannot be empty");
            else if (!uint.TryParse(proxy.Port, out i))
                return new ValidationResult(false, "Port must be a numeric value and cannot be empty");
            else
                return ValidationResult.ValidResult;
        }
    }
}
