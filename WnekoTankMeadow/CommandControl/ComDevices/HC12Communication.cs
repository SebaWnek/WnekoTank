using Meadow.Hardware;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WnekoTankMeadow.CommandControl.ComDevices
{
    class HC12Communication : ComCommunication
    {
        public HC12Communication(ISerialMessagePort p) : base(p)
        {
            communicationType = Type.RF;
        }
    }
}
