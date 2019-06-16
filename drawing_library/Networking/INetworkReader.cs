using System;
using System.Net;

namespace drawing.Networking
{
    public interface INetworkReader<E> : IDisposable
    {
        IPEndPoint Sender { get; }

        E Read();
    }
}
