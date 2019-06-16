using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows.Forms;
using drawing.Messaging;
using drawing.Threading;
using drawing.Networking;

namespace drawing_server
{
    static class Program
    {
        

        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Drawing_Paint_Server());


         // listener на сервере, который принимает класс Lines и отправляет всем подключенным клиентам
            
          //
        }

        
        

        /*
        private static string GetConnectionString()
        {
            return builder.ConnectionString;
        }
        */

    }
}
