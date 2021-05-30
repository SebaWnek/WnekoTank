using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WnekoTankMeadow
{
    /// <summary>
    /// Helper methods not specific to any subsystem
    /// </summary>
    public static class HelperMethods
    {
        /// <summary>
        /// Pause execution of next command for specified time.
        /// Useful to run some command for specified time before stopping it. 
        /// </summary>
        /// <param name="input">Wait time in seconds</param>
        public static void Wait(string time)
        {
            int seconds = int.Parse(time);
            Thread.Sleep(seconds * 1000);
        }
    }
}
