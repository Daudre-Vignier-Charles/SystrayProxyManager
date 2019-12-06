using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProxyTrayIndicator
{
    [Serializable]
    public class Proxy
    {
        public string Name { get; set; }
        public string Address { get; set; }
        public string Port { get; set; }
    }
}
