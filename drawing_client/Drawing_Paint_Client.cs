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
    public partial class Drawing_Paint_CLient : Form
    {
        Color color = Color.Black; //Создаем переменную типа Color присваиваем ей черный цвет.
        bool isPressed = false; //логическая переменная понадобиться для опеределения когда можно рисовать на panel
        Point CurrentPoint; //Текущая точка рисунка.
        Point PrevPoint; //Это начальная точка рисунка.
        Graphics g; //Создаем графический элемент.
        ColorDialog colorDialog = new ColorDialog(); //диалоговое окно для выбора цвета.
        //string serverIP = "192.168.13.102";
        //string serverIP = "127.0.0.1";
        string serverIP = "25.85.139.86";
        UdpMessageListener<Lines> listener;
        UdpMessageListener<ServiceMessage> SMlistener;
        UdpMessageListener<Pictures> Pictlistener;
        static Bitmap pic;

        public Drawing_Paint_CLient()
        {
            InitializeComponent();
            label2.BackColor = Color.Black; //По умолчанию для пера задан черный цвет, поэтому мы зададим такой же фон для label2
            g = panel1.CreateGraphics(); //Создаем область для работы с графикой на элементе panel
            pic = new Bitmap(panel1.Width, panel1.Height); // картинка

            listener = new UdpMessageListener<Lines>(Ports.LineServerPort);
            listener.IncomingMessage += OnLinesMessage;
            listener.Start();

            SMlistener = new UdpMessageListener<ServiceMessage>(Ports.SMServerPort);
            SMlistener.IncomingMessage += OnServiceMessage;
            SMlistener.Start();

            Pictlistener = new UdpMessageListener<Pictures>(Ports.PictPort);
            Pictlistener.IncomingMessage += OnPicturesMessage;
            Pictlistener.Start();

            using (var writer = NetworkingFactory.UdpWriter<ServiceMessage>(IPAddress.Parse(serverIP), Ports.SMPort))
            {
                var info = new ServiceMessage(Command.GetPic);
                writer.Write(info);
            }

        }

        // обработка сообщений от сервера о линиях
        private void OnLinesMessage(object sender, IncommingMessageEventArgs<Lines> e)
        {
            draw_Line(e.Message.pen(), e.Message.CurtPoint(), e.Message.PrPoint());
        }

        // обработка сообщений от сервера об очистке экрана
        private void OnServiceMessage(object sender, IncommingMessageEventArgs<ServiceMessage> e)
        {
            if (e.Message.Command == Command.CleanScreen)
            {
                clear();
            }
        }

        // обработка сообщений от сервера об установке картинки 
        private void OnPicturesMessage(object sender, IncommingMessageEventArgs<Pictures> e)
        {
            draw_Picture(e.Message.pic);
        }

        // кнопка меняющая цвет линии
        private void label2_Click(object sender, EventArgs e)
        {
            if (colorDialog.ShowDialog() == DialogResult.OK) //Если окно закрылось с OK, то меняем цвет для пера и фона label2
            {
                color = colorDialog.Color; //меняем цвет для пера
                label2.BackColor = colorDialog.Color; //меняем цвет для Фона label2
            }
        }

        // кнопка, отвечающая за очистку экрана
        private void button1_Click(object sender, EventArgs e)
        {
            my_clear();
            panel1.Refresh(); //Очищает элемент Panel
        }

        // если нажали на клавишу мыши - начать линию
        private void panel1_MouseDown(object sender, MouseEventArgs e)
        {
            isPressed = true;
            CurrentPoint = e.Location;
        }

        // если отпустили клавишу - линия закончена
        private void panel1_MouseUp(object sender, MouseEventArgs e)
        {
            isPressed = false;
        }

        // при нажатой клавише и перемещении - рисовать
        private void panel1_MouseMove(object sender, MouseEventArgs e)
        {       
            if (isPressed)
            {
                PrevPoint = CurrentPoint;
                CurrentPoint = e.Location;
                my_Pen();
            }
        }

        // функция передаёт на сервер информацию о проведённой линии
        private void my_Pen()
        {
            Pen pen = new Pen(color, (float)numericUpDown1.Value); //Создаем перо, задаем ему цвет и толщину.
            // если закомментирована строчка ниже, то клиент не рисует на своей доске сам, 
            // а передаёт линию серверу, который рисует на клиентах
            // g.DrawLine(pen, CurrentPoint, PrevPoint); //Соединияем точки линиями

            using (var writer = NetworkingFactory.UdpWriter<Lines>(IPAddress.Parse(serverIP), Ports.LinePort))
            {
                var info = new Lines(CurrentPoint, PrevPoint, pen);
                // передать информацию о линии на сервер
                writer.Write(info);
            }
        }

        // функция передаёт на сервер информацию о том, что экран был очищен
        private void my_clear()
        {
            using (var writer = NetworkingFactory.UdpWriter<ServiceMessage>(IPAddress.Parse(serverIP), Ports.SMPort))
            {
                var info = new ServiceMessage(Command.CleanScreen);
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
                Graphics G = Graphics.FromImage(pic);
                G.DrawLine(pen, FirstPoint.X, FirstPoint.Y, SecondPoint.X, SecondPoint.Y);
                G.Dispose();
                g.DrawLine(pen, FirstPoint, SecondPoint); //Соединияем точки линиями
            }
        }

        private void draw_Picture(Bitmap picture)
        {
            if (this.panel1.InvokeRequired)
            {
                Invoke(new MethodInvoker(delegate () {
                    draw_Picture(picture);
                }));
            }
            else
            {
                Graphics G = Graphics.FromImage(pic);
                G.DrawImage(picture, new Rectangle(0, 0, picture.Width, picture.Height));
                G.Dispose();
                g.DrawImage(picture, new Rectangle(0, 0, picture.Width, picture.Height)); //Соединияем точки линиями
            }
        }

        private void clear()
        {
            if (this.panel1.InvokeRequired)
            {
                Invoke(new MethodInvoker(delegate () {
                    clear();
                }));
            }
            else
            {
                panel1.Refresh();
                pic = new Bitmap(panel1.Width, panel1.Height);
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            listener.Dispose();
            SMlistener.Dispose();
            Pictlistener.Dispose();
        }
    }
}
