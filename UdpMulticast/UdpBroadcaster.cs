using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace UdpMulticast
{
    public class UdpBroadcaster
    {
        private IPEndPoint multiCastEndpoint;
        private UdpClient udpClient;
        public UdpBroadcaster()
        {
            udpClient = new UdpClient();
            var multiCastAddress = IPAddress.Parse(MulticastConstants.IpAddress);
            multiCastEndpoint = new IPEndPoint(multiCastAddress, MulticastConstants.Port);
        }

        public async Task Send<T>(T message)
        {
            var jsonString = JsonConvert.SerializeObject(message);
            var bytes = Encoding.UTF8.GetBytes(jsonString);

            await udpClient.SendAsync(bytes, bytes.Length, multiCastEndpoint);
        }
    }
}
