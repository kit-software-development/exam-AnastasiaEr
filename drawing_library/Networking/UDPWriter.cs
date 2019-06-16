using System;
using System.Net.Sockets;
using drawing.Messaging;

namespace drawing.Networking
{
    class UDPWriter<E> : INetworkWriter<E>
    {
        private UdpClient socket;
        public UDPWriter(UdpClient client)
        {
            socket = client;
        }


        public void Dispose() => socket.Close();
        

        public void Write(E message)
        {
            byte[] data = message.Serialize();
            socket.Send(data, data.Length);
        }
        
    }
}
