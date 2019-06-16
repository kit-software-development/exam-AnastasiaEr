using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Net;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using drawing.Messaging;
using drawing.Networking;
using drawing.Threading;

namespace drawing_client
{
    public partial class Drawing_Paint : Form
    {
        Color color = Color.Black; //Создаем переменную типа Color присваиваем ей черный цвет.
        bool isPressed = false; //логическая переменная понадобиться для опеределения когда можно рисовать на panel
        Point CurrentPoint; //Текущая точка рисунка.
        Point PrevPoint; //Это начальная точка рисунка.
        Graphics g; //Создаем графический элемент.
        ColorDialog colorDialog = new ColorDialog(); //диалоговое окно для выбора цвета.
        string serverIP = "192.168.13.102";
        UdpMessageListener<Lines> listener;

        public Drawing_Paint()
        {
            InitializeComponent();
            label2.BackColor = Color.Black; //По умолчанию для пера задан черный цвет, поэтому мы зададим такой же фон для label2
            g = panel1.CreateGraphics(); //Создаем область для работы с графикой на элементе panel
            listener = new UdpMessageListener<Lines>(9090);
            listener.IncomingMessage += OnServiceMessage;
            listener.Start();
        }

        private void OnServiceMessage(object sender, IncommingMessageEventArgs<Lines> e)
        {
            draw_Line(e.Message.pen(), e.Message.CurtPoint(), e.Message.PrPoint());
        }

        private void label2_Click(object sender, EventArgs e)
        {
            if (colorDialog.ShowDialog() == DialogResult.OK) //Если окно закрылось с OK, то меняем цвет для пера и фона label2
            {
                color = colorDialog.Color; //меняем цвет для пера
                label2.BackColor = colorDialog.Color; //меняем цвет для Фона label2
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            panel1.Refresh(); //Очищает элемент Panel
        }

        private void panel1_MouseDown(object sender, MouseEventArgs e)
        {
            isPressed = true;
            CurrentPoint = e.Location;
        }

        private void panel1_MouseUp(object sender, MouseEventArgs e)
        {
            isPressed = false;
        }

        private void panel1_MouseMove(object sender, MouseEventArgs e)
        {
            if (isPressed)
            {
                PrevPoint = CurrentPoint;
                CurrentPoint = e.Location;
                my_Pen();
            }
        }

        private void my_Pen()
        {
            Pen pen = new Pen(color, (float)numericUpDown1.Value); //Создаем перо, задаем ему цвет и толщину.
            // если закомментирована строчка ниже, то клиент не рисует на своей доске сам, 
            // а передаёт линию серверу, который рисует на клиентах
            // g.DrawLine(pen, CurrentPoint, PrevPoint); //Соединияем точки линиями

            using (var writer = NetworkingFactory.UdpWriter<Lines>(IPAddress.Parse(serverIP), 8080))
            {
                var info = new Lines(CurrentPoint, PrevPoint, pen);
                writer.Write(info);
            }
        }

        private void draw_Line(Pen pen, Point FirstPoint, Point SecondPoint)
        {
            if (this.panel1.InvokeRequired)
            {
                Invoke(new MethodInvoker(delegate () {
                    draw_Line(pen, FirstPoint, SecondPoint);
                }));
            }
            else
            {
                g.DrawLine(pen, FirstPoint, SecondPoint); //Соединияем точки линиями
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            listener.Dispose();
        }
    }
}
