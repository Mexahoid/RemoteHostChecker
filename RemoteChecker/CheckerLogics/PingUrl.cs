using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RemoteChecker.CheckerLogics
{
    public static class PingUrl
    {
        public static async Task<int> PingUrlAsync(string url)
        {
            var ping = new System.Net.NetworkInformation.Ping();
            var result = await ping.SendPingAsync(url);

            return result.Status switch
            {
                System.Net.NetworkInformation.IPStatus.Success => 200,
                System.Net.NetworkInformation.IPStatus.BadRoute or 
                System.Net.NetworkInformation.IPStatus.TimedOut or 
                System.Net.NetworkInformation.IPStatus.BadDestination or 
                System.Net.NetworkInformation.IPStatus.DestinationUnreachable or 
                System.Net.NetworkInformation.IPStatus.DestinationNetworkUnreachable or 
                System.Net.NetworkInformation.IPStatus.DestinationHostUnreachable => 404,
                _ => 0,
            };
        }
    }
}
