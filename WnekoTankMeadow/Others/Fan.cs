using Meadow.Hardware;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WnekoTankMeadow.Others
{
    class Fan
    {
        IDigitalOutputPort port;
        string name;
        CancellationTokenSource source;
        bool isCoolingDown = false;

        public Fan(IDigitalOutputPort p, string n)
        {
            port = p;
            name = n;
            source = new CancellationTokenSource();
        }

        public void StartFan()
        {
            port.State = true;
            if (isCoolingDown)
            {
                source.Cancel();
                source.Dispose();
                source = new CancellationTokenSource();
            }
        }

        public void StopFan()
        {
            port.State = false;
        }

        public async Task StopWithDelay(int delay)
        {
            isCoolingDown = true;
            CancellationToken token = source.Token;
            await Task.Delay(delay, token);
            isCoolingDown = false;
            if (token.IsCancellationRequested) return;
            StopFan();
        }

        internal void SetState(string msg)
        {
            if (msg.StartsWith("1")) StartFan();
            else StopFan();
        }
    }
}
