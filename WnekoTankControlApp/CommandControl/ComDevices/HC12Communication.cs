using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WnekoTankControlApp.CommandControl.ComDevices
{
    /// <summary>
    /// HC12 is basically com communication, just using lower max baud, hence different default value.
    /// Potentially maybe there will be some other differences too.
    /// </summary>
    class HC12Communication : ComPortCommunication
    {
        public HC12Communication(string portNum, int baud = 115200) : base(portNum, baud)
        {
        }
    }
}
