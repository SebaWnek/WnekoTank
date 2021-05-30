using Meadow.Hardware;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WnekoTankMeadow.Others
{
    class Camera
    {
        IDigitalOutputPort port;

        public Camera(IDigitalOutputPort p)
        {
            port = p;
        }

        public void SetCamera(string msg)
        {
            bool on = bool.Parse(msg);
        }

        public void SetCamera(bool on)
        {
            port.State = on;
        }
    }
}
