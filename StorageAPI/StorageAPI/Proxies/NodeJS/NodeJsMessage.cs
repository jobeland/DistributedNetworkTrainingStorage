using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StorageAPI.Proxies.NodeJS
{
    public class NodeJsMessage
    {
        public string NetworkGenes { get; set; }
        public double Eval { get; set; }
        public int Version { get; set; }
    }
}
