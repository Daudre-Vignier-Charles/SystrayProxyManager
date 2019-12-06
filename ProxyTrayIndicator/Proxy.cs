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
        public override string ToString() =>
            String.Format("{0}:{1}", Address, Port);

        public string Name { get; set; }
        public string Address { get; set; }
        public string Port { get; set; }
    }
}
