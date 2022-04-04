using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecApp.CustomEventArgs
{
    public class ScreenAreaEventArgs : EventArgs
    {
        public int ScreenNum;
        public double Left;
        public double Top;
        public double Width;
        public double Height;
    }
}
