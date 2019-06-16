using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace drawing.Messaging
{
    [Serializable]
    public class Pictures
    {
        public Bitmap pic;
        public Pictures (Bitmap pic) { this.pic = pic; }
    }
}
