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
        /// <summary>
        /// Extends inbuilt I2cCharacterDisplay class
        /// </summary>
        I2cCharacterDisplay display;
        byte rows;
        byte columns;

        /// <summary>
        /// Basic constructor 
        /// </summary>
        /// <param name="bus">I2C bus</param>
        /// <param name="address">I2C device address</param>
        /// <param name="r">Number of text rows</param>
        /// <param name="c">Number of text columns</param>
        public DisplayLCD(II2cBus bus, byte address, byte r, byte c)
        {
            display = new I2cCharacterDisplay(bus, address, r, c);
            rows = r;
            columns = c;
        }

        /// <summary>
        /// Write message, to be used with EventHandler of string
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="msg">Message</param>
        public void Write(object sender, string msg)
        {
            Write(msg);
        }

        /// <summary>
        /// Writes selected text using all availible lines, automatically overflowing to next line
        /// </summary>
        /// <param name="msg">Message to be printed</param>
        public void Write(string msg)
        {
            for(byte i = 0; i < rows; i++)
            {
                if (msg.Length > columns * (i + 1)) display.WriteLine(msg.Substring(columns * i, columns), i);
                else if (msg.Length > columns * i && msg.Length <= columns * (i + 1)) display.WriteLine(msg.Substring(columns * i), i);
                else return;
            }
        }

        /// <summary>
        /// Displays selected lines, each string in next line
        /// </summary>
        /// <param name="lines"></param>
        public void WriteMultipleLines(string[] lines)
        {
            byte count = rows < lines.Length ? rows : (byte)lines.Length;
            for (byte i = 0; i < count; i++)
            {
                display.WriteLine(lines[i], i);
            }
        }

        /// <summary>
        /// Clears display 
        /// </summary>
        internal void Clear()
        {
            display.ClearLines();
        }
    }
}
