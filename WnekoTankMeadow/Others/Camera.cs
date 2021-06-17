using Meadow.Hardware;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WnekoTankMeadow.Others
{
    /// <summary>
    /// To be implemented
    /// </summary>
    class Camera
    {
        IDigitalOutputPort port;

        public Camera(IDigitalOutputPort p)
        {
            port = p;
        }

        public void SetCamera(string msg)
        {
            bool on = msg.StartsWith("1");
        }

        public void SetCamera(bool on)
        {
            port.State = on;
        }
    }
}
