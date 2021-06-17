using Meadow.Hardware;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WnekoTankMeadow.Others
{
    /// <summary>
    /// Basic, monotone buzzer to generate sound signals for basic communication with user
    /// Uses MCP23008 expander and IC NOT gate to reverse it's signal, as when turning on before expander is instantiated it's default would be emiting buzz
    /// NOT gate changes default state to false
    /// </summary>
    class Buzzer
    {
        IDigitalOutputPort port;
        int defaultLength = 1000;

        /// <summary>
        /// Basic constructor 
        /// </summary>
        /// <param name="p"></param>
        public Buzzer(IDigitalOutputPort p)
        {
            port = p;
        }

        /// <summary>
        /// Generate sound for default time
        /// Can be invoked without awaiting, it's buzzing while other actions can be performed then
        /// </summary>
        /// <returns>Task so it can be awaited if needed</returns>
        public async Task Buzz()
        {
            port.State = true;
            await Task.Delay(defaultLength);
            port.State = false;
        }

        /// <summary>
        /// Generate sound for selected time
        /// Can be invoked without awaiting, it's buzzing while other actions can be performed then
        /// </summary>
        /// <param name="length">Time of buz in miliseconds</param>
        /// <returns>Task so it can be awaited if needed</returns>
        public async Task Buzz(int length)
        {
            port.State = true;
            await Task.Delay(length);
            port.State = false;
        }

        /// <summary>
        /// Generate series of sounds
        /// Can be invoked without awaiting, it's buzzing while other actions can be performed then
        /// </summary>
        /// <param name="on">Time of sound signal in miliseconds</param>
        /// <param name="off">Time between signals in miliseconds</param>
        /// <param name="count">Number of signals</param>
        /// <returns></returns>
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
