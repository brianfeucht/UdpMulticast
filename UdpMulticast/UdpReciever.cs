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
    public class UdpReciever<T>
    {
        private readonly IPAddress multiCastAddress;
        private readonly Action<T> onRecieveAction;
        private UdpClient udpClient;
        private bool run;

        public UdpReciever(Action<T> onRecieve)
        {
            udpClient = new UdpClient();

            udpClient.ExclusiveAddressUse = false;
            udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

            udpClient.Client.Bind(new IPEndPoint(IPAddress.Any, MulticastConstants.Port));

            multiCastAddress = IPAddress.Parse(MulticastConstants.IpAddress);

            onRecieveAction = onRecieve;
        }

        public void Start()
        {
            run = true;
            udpClient.JoinMulticastGroup(multiCastAddress);

            Task.Run(() => ProcessMessages());
        }

        private async Task ProcessMessages()
        {
            while(true)
            {
                var result = await udpClient.ReceiveAsync();                
                if (result == null || result.Buffer == null)
                    continue;

                try
                {
                    var json = Encoding.UTF8.GetString(result.Buffer);
                    var value = JsonConvert.DeserializeObject<T>(json);
                    Task.Run(() => onRecieveAction(value));
                }
                catch
                {

                }
            }
        }        

        public void Stop()
        {
            run = false;

            udpClient.DropMulticastGroup(multiCastAddress);
        }
    }
}
