using System;
using System.Net;
using System.Threading;
using System.Windows.Forms;
using drawing.Networking;

namespace drawing.Threading
{
    public sealed class IncommingMessageEventArgs<E> : EventArgs
    {
        public E Message { get; internal set; }

        public IPEndPoint Sender { get; internal set; }
    }
    public class UdpMessageListener<E> : IDisposable
    {
        private bool exit;

        private INetworkReader<E> reader;

        public event EventHandler<IncommingMessageEventArgs<E>> IncomingMessage;

        public UdpMessageListener(int port)
        {
            reader = NetworkingFactory.UdpReader<E>(port);
        }
        private void ThreadProc()
        {
            while (!exit)
            {
                try
                {
                    //E massage = reader.Read();
                    var args = new IncommingMessageEventArgs<E>
                    {
                        Message = reader.Read(),
                        Sender = reader.Sender
                    };
                    IncomingMessage?.Invoke(this, args);
                }
                catch (Exception e)
                {
                    string message = e.Message;
                    string caption = "Error Detected in Input";
                    MessageBoxButtons buttons = MessageBoxButtons.OK;

                    // Displays the MessageBox.
                    var result = MessageBox.Show(message, caption, buttons);
                }
            }
        }

        public void Start()
        {
            Thread thread = new Thread(ThreadProc) {IsBackground = true};
            thread.Start();
        }

        public void Dispose()
        {
            exit = true;
            reader.Dispose();

        }
    }
}
