using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bundle.Client.Options
{
    public class ServerOptions
    {
        public string LoginMethod { get; set; }
        public string ServerIP { get; set; }
        public ushort ServerPort { get; set; }
        public string ServerVersion { get; set; }
    }
}
