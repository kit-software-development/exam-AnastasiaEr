using drawing.Messaging;
using drawing.Networking;
using drawing.Threading;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace drawing_server
{
    public partial class Form1 : Form
    {
        static INetworkWriter<Lines> _client;
        static List<IPAddress> _clientList = new List<IPAddress>();
        static Graphics g; //Создаем графический элемент для отображения действий клиентов на сервере (static??)
        UdpMessageListener<Lines> listener;
        

        public Form1()
        {
            InitializeComponent();
            g = panel1.CreateGraphics(); //Создаем область для работы с графикой на элементе panel
            /*
            using (var listener = new UdpMessageListener<Lines>(8080))
            {
                listener.IncomingMessage += OnServiceMessage;
                listener.Start();
                while (true)
                {
                    // do nothing
                }
            }
            */
        }

        private static void OnServiceMessage(object sender, IncommingMessageEventArgs<Lines> e)
        {
            // если такого клиента, который прислал линию, ещё нет в списке, добавить его в список
            if (!_clientList.Contains(e.Sender.Address))
            {
                _clientList.Add(e.Sender.Address);
            }
            // для каждого клиента в списке подключенных (потом вместо списка можно использовать БД)
            foreach (var client in _clientList)
            {
                //_client.Dispose();
                _client = NetworkingFactory.UdpWriter<Lines>(client, 9090);
                var msg = new Lines(e.Message.PrPoint(), e.Message.CurtPoint(), e.Message.pen());
                // для графического объекта вызываем стандартную функцию рисования линии с аргументами, полученными от клиента
                g.DrawLine(e.Message.pen(), e.Message.CurtPoint(), e.Message.PrPoint());

                _client.Write(msg);
                _client.Dispose();
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            listener = new UdpMessageListener<Lines>(8080);
            listener.IncomingMessage += OnServiceMessage;
            listener.Start();
            
        }
        //listener.Dispose();
        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void Form1_Close(object sender, FormClosingEventArgs e)
        {
            listener.Dispose();
        }
    }
}
