using System;
using System.Net;
using drawing.Messaging;
using System.Net.Sockets;

namespace drawing.Networking
{
    class UdpReader<E> : INetworkReader<E>
    {
        private IPEndPoint _sender;

        private readonly UdpClient _socket;
        public IPEndPoint Sender => _sender;

        public UdpReader(UdpClient client)
        {
            _socket = client;
        }

        public void Dispose() => _socket.Close();

        public E Read()
        {
            byte[] data = _socket.Receive(ref _sender);
            return MessageFactory.Deserialize<E>(data);
        }
    }
}
