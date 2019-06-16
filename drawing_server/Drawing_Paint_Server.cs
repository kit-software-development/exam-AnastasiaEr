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
using System.Threading;
using System.Data.SqlClient;
using System.IO;

namespace drawing_server
{
    public partial class Drawing_Paint_Server : Form
    {
        static INetworkWriter<Lines> _client_line;
        static INetworkWriter<ServiceMessage> _client;
        static INetworkWriter<Pictures> _client_pictures;
        static List<IPAddress> _clientList = new List<IPAddress>();
        static Bitmap pic;
        static Graphics g; //Создаем графический элемент для отображения действий клиентов на сервере
        UdpMessageListener<Lines> linesListener; 
        UdpMessageListener<ServiceMessage> SMlistener;
        

        public Drawing_Paint_Server()
        {
            InitializeComponent();
            g = panel1.CreateGraphics(); //Создаем область для работы с графикой на элементе panel
            pic = new Bitmap(panel1.Width, panel1.Height); // картинка
            timer1.Start();
            timer2.Start();
        }

        private List<Lines> linesList = new List<Lines>();

        // обработчик события "появилась линия"
        private void OnLinesMessage(object sender, IncommingMessageEventArgs<Lines> e)
        {
            // если такого клиента, который прислал линию, ещё нет в списке, добавить его в список
            if (!_clientList.Contains(e.Sender.Address))
            {
                _clientList.Add(e.Sender.Address);
            }
            // для каждого клиента в списке подключенных (потом вместо списка можно использовать БД)
            foreach (var client in _clientList)
            {
                _client_line = NetworkingFactory.UdpWriter<Lines>(client, Ports.LineServerPort);
                var msg = new Lines(e.Message.PrPoint(), e.Message.CurtPoint(), e.Message.pen());
                
                // передать информацию о линии всем клиентам 
                _client_line.Write(msg);
                _client_line.Dispose();
            }

            try
            {
                Monitor.Enter(panel1);
                Graphics G = Graphics.FromImage(pic);
                foreach (var line in linesList)
                {
                    G.DrawLine(line.pen(), line.CurtPoint(), line.PrPoint());
                }
                linesList = new List<Lines>();

                G.DrawLine(e.Message.pen(), e.Message.CurtPoint(), e.Message.PrPoint());
                G.Dispose();
            }
            catch
            {
                linesList.Add(e.Message);
            }
            finally
            {
                Monitor.Exit(panel1);
            }
            
        }

        // обработчик события "очистка экрана"
        private void OnServiceMessage(object sender, IncommingMessageEventArgs<ServiceMessage> e)
        {
            // если такого клиента, который прислал линию, ещё нет в списке, добавить его в список
            if (!_clientList.Contains(e.Sender.Address))
            {
                _clientList.Add(e.Sender.Address);
            }

            if (e.Message.Command == Command.CleanScreen)
            {
                //для каждого клиента в списке подключенных 
                foreach (var client in _clientList)
                {
                    _client = NetworkingFactory.UdpWriter<ServiceMessage>(client, Ports.SMServerPort);
                    var msg = new ServiceMessage(Command.CleanScreen);
                    linesList = new List<Lines>();
                    clear();

                    _client.Write(msg);
                    _client.Dispose();
                }
            }
            else if (e.Message.Command == Command.GetPic)
            {
                try
                {
                    Monitor.Enter(panel1);
                    Monitor.Enter(pic);
                    _client_pictures = NetworkingFactory.UdpWriter<Pictures>(e.Sender.Address, Ports.PictPort);
                    var msg = new Pictures(pic);
                    _client_pictures.Write(msg);
                }
                finally
                {
                    Monitor.Exit(panel1);
                    Monitor.Exit(pic);
                }
            }
        }

        // функция, очищающая все элементы экрана
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
                Graphics G = Graphics.FromImage(pic);
                G.Clear(Color.LightGray);
                G.Dispose();
            }
        }

        // функция, обновляющая рисунок
        private void updateImg()
        {
            if (this.panel1.InvokeRequired)
            {
                Invoke(new MethodInvoker(delegate () {
                    updateImg();
                }));
            }
            else
            {
                panel1.Refresh();
                g.DrawImage(pic, new Point(0, 0));   
            }
        }

        // при загрузке формочки запускаем слушателей, которые ловят события
        private void Form1_Load(object sender, EventArgs e)
        {
            linesListener = new UdpMessageListener<Lines>(Ports.LinePort);
            linesListener.IncomingMessage += OnLinesMessage;
            linesListener.Start();
            
            SMlistener = new UdpMessageListener<ServiceMessage>(Ports.SMPort);
            SMlistener.IncomingMessage += OnServiceMessage;
            SMlistener.Start();
        }

        // при закрытии формочки слушатели от обязанностей ловить освобождаются
        private void Form1_Close(object sender, FormClosingEventArgs e)
        {
            linesListener.Dispose();
            SMlistener.Dispose();
        }

        // по срабатыванию этого таймера рисунок на сервере обновляется
        private void timer1_Tick(object sender, EventArgs e)
        {
            try
            {
                Monitor.Enter(panel1);
                updateImg();
            }
            finally
            {
                Monitor.Exit(panel1);
            }
        }

        // по срабатыванию этого таймера сервер сохраняет рисунок
        private void timer2_Tick(object sender, EventArgs e)
        {
            // назнание файла - дата
            string date = DateTime.Now.ToString("dd.MM HH_mm_ss") + ".bmp";
            // строчка ниже - сохранение в файл. сейчас не нужен, сейчас сохраняется в базу
            //pic.Save(date);
            // добавить файл в список доступных для отображения сохранений
            comboBox1.Items.Add(date);
            // в базу нельзя сохранять Bitmap, перевести в доступный формат
            ImageConverter converter = new ImageConverter();
            byte[] test = (byte[])converter.ConvertTo(pic, typeof(byte[]));

            // подключаемся к базе
            string connectionString = GetConnectionString();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                // вставляем в таблицу сохранений новую строку с названием и файлом
                SqlCommand command = new SqlCommand();
                command.Connection = connection;
                command.CommandText = @"INSERT INTO BACKUP_TABLE VALUES (@SaveFile, @screen)";

                command.Parameters.AddWithValue("@SaveFile", SqlDbType.NVarChar);
                command.Parameters.AddWithValue("@screen", SqlDbType.Image);

                command.Parameters["@SaveFile"].Value = date;
                command.Parameters["@screen"].Value = test;

                command.ExecuteNonQuery();
            }
        }

        // получить данные для подключения к базе 
        private static string GetConnectionString()
        {
            var builder = new SqlConnectionStringBuilder
            {
                DataSource = @"(LocalDB)\MSSQLLocalDB",
                AttachDBFilename = Path.GetFullPath("Database.mdf"),
                IntegratedSecurity = true
            };
            return builder.ConnectionString;
        }

        // при нажатии этой кнопки из базы выбирается нужный бэкап и загружается на экран
        private void button1_Click(object sender, EventArgs e)
        {
            Bitmap res_pic = null;
            string name_of_file = comboBox1.SelectedItem.ToString();

            string connectionString = GetConnectionString();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = @"SELECT screen FROM [BACKUP_TABLE] WHERE SaveFile = @SaveFile";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@SaveFile", SqlDbType.NVarChar);
                    command.Parameters["@SaveFile"].Value = name_of_file;
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        reader.Read();
                        byte[] iTrimByte = (byte[])reader["screen"];
                        TypeConverter tc = TypeDescriptor.GetConverter(typeof(Bitmap));
                        res_pic = (Bitmap)tc.ConvertFrom(iTrimByte);
                    }
                }
            }
            clear();
            draw_Picture(res_pic);
            Send_picture();
        }

        // функция получает картинку в виде Bitmap и отрисовывает её на сервере
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

        // функция пересылает картинку с сервера всем клиентам
        private void Send_picture()
        {
            foreach (var client in _clientList)
            {
                try
                {
                    Monitor.Enter(panel1);
                    Monitor.Enter(pic);
                    _client_pictures = NetworkingFactory.UdpWriter<Pictures>(client, Ports.PictPort);
                    var msg = new Pictures(pic);
                    _client_pictures.Write(msg);
                }
                finally
                {
                    Monitor.Exit(panel1);
                    Monitor.Exit(pic);
                }
            }
        }

    }
}
