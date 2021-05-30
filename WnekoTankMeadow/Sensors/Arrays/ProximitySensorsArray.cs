using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WnekoTankMeadow.Sensors
{
    class ProximitySensorsArray
    {
        ProximitySensor[] sensors;
        public ProximitySensorsArray(ProximitySensor[] s)
        {
            sensors = s;
        }

        public void SetBehavior(string args)
        {
            foreach (ProximitySensor sensor in sensors) sensor.SetBehavior(args);
        }

        public void Register(EventHandler<string> eventHandler)
        {
            foreach (ProximitySensor sensor in sensors) sensor.Register(eventHandler);
        }
    }
}
