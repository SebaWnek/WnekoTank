using Meadow.Hardware;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WnekoTankMeadow.Others
{
    class Buzzer
    {
        IDigitalOutputPort port;
        int defaultDelay = 1000;

        public Buzzer(IDigitalOutputPort p)
        {
            port = p;
        }

        //public void StopBuzz()
        //{
        //    port.State = false;
        //}

        public async Task Buzz()
        {
            port.State = true;
            await Task.Delay(defaultDelay);
            port.State = false;
        }

        public async Task Buzz(int delay)
        {
            port.State = true;
            await Task.Delay(delay);
            port.State = false;
        }

        public async Task BuzzPulse(int on, int off, int count)
        {
            for(int i = 0; i < count; i++)
            {
                port.State = true;
                await Task.Delay(on);
                port.State = false;
                await Task.Delay(off);
            }
        }
    }
}
