using Meadow.Hardware;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WnekoTankMeadow.Others
{
    class LedLamp
    {
        private IPwmPort port;
        private Fan fan;
        private int waitTime = 10000;
        public string Name { get; set; }

        public LedLamp(IPwmPort p, Fan f, string name)
        {
            port = p;
            p.DutyCycle = 0;
            Name = name;
            fan = f;
        }

        public void SetBrightnes(int brightness)
        {
            brightness = brightness > 100 ? 100 : brightness;
            port.DutyCycle = brightness / 100.01f;
            if (brightness > 0) fan.StartFan();
#pragma warning disable CS4014 // To wywołanie nie jest oczekiwane, dlatego wykonywanie bieżącej metody będzie kontynuowane do czasu ukończenia wywołania
            else fan.StopWithDelay(waitTime);
#pragma warning restore CS4014 // To wywołanie nie jest oczekiwane, dlatego wykonywanie bieżącej metody będzie kontynuowane do czasu ukończenia wywołania
        }
        public void SetBrightnes(string msg)
        {
            int power = int.Parse(msg);
            SetBrightnes(power);
        }
    }
}
