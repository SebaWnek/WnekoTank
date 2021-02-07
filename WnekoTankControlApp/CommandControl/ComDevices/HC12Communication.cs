using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WnekoTankControlApp.CommandControl.ComDevices
{
    class HC12Communication : ComPortCommunication
    {
        public HC12Communication(string portNum, int baud = 115200) : base(portNum, baud)
        {
        }
    }
}
