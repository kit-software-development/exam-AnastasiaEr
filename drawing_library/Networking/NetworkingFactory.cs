using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace drawing.Networking
{
    public static class NetworkingFactory
    {
        public static INetworkReader<TE> UdpReader<TE>(int port)
        {
            UdpClient client = new UdpClient(port);
            return new UdpReader<TE>(client);
        }

        public static INetworkWriter<TE> UdpWriter<TE>(IPAddress address, int port)
        {
            UdpClient client = new UdpClient();
            client.Connect(address, port);
            return new UDPWriter<TE>(client);
        }
        
        public static INetworkWriter<TE> Broadcast<TE>(int port)
        {
            UdpClient client = new UdpClient {EnableBroadcast = true};
            var z = IPAddress.Broadcast;
            client.Connect(IPAddress.Parse("192.168.13.255"), port);
            return new UDPWriter<TE>(client);
        }
    }
}
