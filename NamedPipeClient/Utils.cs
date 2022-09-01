using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Net.Sockets;
using System.Net;
using System.Security.Principal;
using System.Text;
using System.Threading;

namespace TestPipe
{
    class Utils
    {
        public static List<string> LocalIpAddressList()
        {
            List<string> ips = new List<string>();
            IPHostEntry ipEntry = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in ipEntry.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    ips.Add(ip.ToString());
                }
            }
            return ips;
        }
    }
}