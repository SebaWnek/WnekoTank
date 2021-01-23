using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WnekoTankMeadow
{
    public static class HelperMethods
    {
        public static void Wait(string input)
        {
            int seconds = int.Parse(input);
            Thread.Sleep(seconds);
        }
    }
}
