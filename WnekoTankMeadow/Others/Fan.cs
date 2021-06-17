using Meadow.Hardware;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WnekoTankMeadow.Others
{
    /// <summary>
    /// Basic fan with binary state, no speed control needed. Controlled by GPIO from MCP23008
    /// </summary>
    class Fan
    {
        IDigitalOutputPort port;
        string name;
        CancellationTokenSource source;
        bool isCoolingDown = false;

        /// <summary>
        /// Basic constructor
        /// </summary>
        /// <param name="p">GPIO port</param>
        /// <param name="n">Name to easily differenciate from other fans</param>
        public Fan(IDigitalOutputPort p, string n)
        {
            port = p;
            name = n;
            source = new CancellationTokenSource();
        }

        /// <summary>
        /// Start fan.
        /// If fan is already in the process of cooling down cancels it and sets it just to on state
        /// </summary>
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

        /// <summary>
        /// Stops fan immediately
        /// </summary>
        public void StopFan()
        {
            port.State = false;
        }

        /// <summary>
        /// Stops fan after selected time delay.
        /// Useful to cool down device a bit longer even when it stopped, so no logic needed on device state, just signal to stop.
        /// Implements CancellationToken so can be cancelled if needed if, for example device restarted earlier and needs cooling again
        /// </summary>
        /// <param name="delay">Time to cool down after</param>
        /// <returns>Task so can be awaited if needed</returns>
        public async Task StopWithDelay(int delay)
        {
            isCoolingDown = true;
            CancellationToken token = source.Token;
            await Task.Delay(delay, token);
            isCoolingDown = false;
            if (token.IsCancellationRequested) return;
            StopFan();
        }

        /// <summary>
        /// Method changing fan state to be used by control app
        /// </summary>
        /// <param name="msg"></param>
        internal void SetState(string msg)
        {
            if (msg.StartsWith("1")) StartFan();
            else StopFan();
        }
    }
}
