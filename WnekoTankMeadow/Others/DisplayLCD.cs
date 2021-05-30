using Meadow.Foundation.Displays.Lcd;
using Meadow.Hardware;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WnekoTankMeadow.Others
{
    class DisplayLCD
    {
        I2cCharacterDisplay display;
        byte rows;
        byte columns;

        public DisplayLCD(II2cBus bus, byte address, byte r, byte c)
        {
            display = new I2cCharacterDisplay(bus, address, r, c);
            rows = r;
            columns = c;
        }

        public void Write(object sender, string msg)
        {
            Write(msg);
        }

        public void Write(string msg)
        {
            for(byte i = 0; i < rows; i++)
            {
                if (msg.Length > columns * (i + 1)) display.WriteLine(msg.Substring(columns * i, columns), i);
                else if (msg.Length > columns * i && msg.Length <= columns * (i + 1)) display.WriteLine(msg.Substring(columns * i), i);
                else return;
            }
        }

        public void WriteMultipleLines(string[] lines)
        {
            byte count = rows < lines.Length ? rows : (byte)lines.Length;
            for (byte i = 0; i < count; i++)
            {
                display.WriteLine(lines[i], i);
            }
        }

        internal void Clear()
        {
            display.ClearLines();
        }

        //public void Write(string msg)
        //{
        //    display.ClearLines();
        //    if (msg.Length <= 16) display.Write(msg);
        //    else if (msg.Length > 16 && msg.Length <= 32)
        //    {
        //        display.WriteLine(msg.Substring(0, 16), 0);
        //        display.WriteLine(msg.Substring(16, msg.Length - 16), 1);
        //    }
        //    else 
        //    {
        //        display.WriteLine(msg.Substring(0, 16), 0);
        //        display.WriteLine(msg.Substring(16, 16), 1);
        //    }
        //}

        //public void WriteTwoLines(string first, string second)
        //{
        //    display.WriteLine(first, 0);
        //    display.WriteLine(second, 1);
        //}
    }
}
