using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;

namespace FastSocket
{
    public static class NetworkInterfaceUtil
    {
        private readonly static Dictionary<int,string> LocalIPv4Dic =new Dictionary<int, string>();
        public static IReadOnlyDictionary<int, string> GetLocalIPv4s()
        {
            if (LocalIPv4Dic!=null && LocalIPv4Dic.Count>0) return LocalIPv4Dic;
            
            NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
            foreach (var item in nics)
            {
                IPInterfaceProperties ipitem=null;
                IPv4InterfaceProperties p=null;
                try
                {
                    
                    if (item.OperationalStatus!=OperationalStatus.Up) continue;
                    if (!item.SupportsMulticast) continue;
                    ipitem = item.GetIPProperties();
                    if (ipitem==null) continue;
                    if (ipitem!.UnicastAddresses.Count==0) continue;
                    p=ipitem.GetIPv4Properties();
                    
                }
                catch (Exception)
                {

                }
                if (ipitem==null) continue;
                if (p==null) continue;
                foreach (var unicastIP in ipitem!.UnicastAddresses)
                {
                    
                    if (unicastIP.Address.AddressFamily==AddressFamily.InterNetwork)
                    {
                        var ip = unicastIP.Address.ToString();
                        LocalIPv4Dic![p!.Index]=ip;
                    }
                }
            }
            return LocalIPv4Dic!;
        }
        
    }
}
