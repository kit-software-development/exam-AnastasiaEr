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


namespace drawing.Messaging
{
    [Serializable]
    public class Lines
    {
        public int StartPoint_x {get;}
        public int StartPoint_y {get;}
        public int EndPoint_x   {get;}
        public int EndPoint_y   {get;}
        public float penSize      {get;}
        public int colorR       {get;}
        public int colorG       {get;}
        public int colorB { get; }

        public Lines(Point prPoint, Point curtPoint, Pen pen)
        {
            StartPoint_x = prPoint.X;
            StartPoint_y = prPoint.Y;
            EndPoint_x = curtPoint.X;
            EndPoint_y = curtPoint.Y;
            colorR = pen.Color.R;
            colorG = pen.Color.G;
            colorB = pen.Color.B;
            penSize = pen.Width;
        }
        
        public Pen pen()
        {
            return new Pen(Color.FromArgb(colorR, colorG, colorB), penSize);
        }
        
        public Point PrPoint()
        {
            return new Point(StartPoint_x, StartPoint_y);
        }

        public Point CurtPoint()
        {
            return new Point(EndPoint_x, EndPoint_y);
        }
    }
}
