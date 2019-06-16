using System;

namespace drawing.Networking
{
    public interface INetworkWriter<E> : IDisposable
    {
        void Write(E message);

    }
}
