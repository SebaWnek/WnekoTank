using Meadow.Foundation.Displays.Lcd;
using Meadow.Hardware;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WnekoTankMeadow.Others
{
    class Display16x2
    {
        I2cCharacterDisplay display;
        byte rows = 2;
        byte columns = 16;

        public Display16x2(II2cBus bus, byte address)
        {
            display = new I2cCharacterDisplay(bus, address, rows, columns);
        }

        public void Write(object sender, string msg)
        {
            Write(msg);
        }

        public void Write(string msg)
        {
            display.ClearLines();
            if (msg.Length <= 16) display.Write(msg);
            else if (msg.Length > 16 && msg.Length <= 32)
            {
                display.WriteLine(msg.Substring(0, 16), 0);
                display.WriteLine(msg.Substring(16, msg.Length - 16), 1);
            }
            else 
            {
                display.WriteLine(msg.Substring(0, 16), 0);
                display.WriteLine(msg.Substring(16, 16), 1);
            }
        }

        public void WriteTwoLines(string first, string second)
        {
            display.WriteLine(first, 0);
            display.WriteLine(second, 1);
        }
    }
}
