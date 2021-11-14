using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using CommonsLibrary;
using Meadow;
using Meadow.Foundation.RTCs;
using Meadow.Hardware;

namespace WnekoTankMeadow.Others
{
    class Rtc
    {
        Action<DateTime> setTime;
        Action<string> sendMessage;
        Ds1307 clock;
        int repeatCounter;
        int repeatLimit = 3;
        public Rtc(II2cBus bus)
        {
            clock = new Ds1307(bus);
        }

        public Rtc(II2cBus bus, Action<string> sender, Action<DateTime> timeSetter) : this(bus)
        {
            setTime = timeSetter;
            sendMessage = sender;
        }

        public void SetClockFromRtc()
        {
            DateTime time = clock.GetTime();
            setTime.Invoke(time);
#if DEBUG
            Console.WriteLine("Time set from RTC: " + time.ToString(new CultureInfo("pl-PL")));
#endif
        }

        public void SetClockFromPc(string time)
        {
            DateTime currentTime;
            //bool success = DateTime.TryParse(time, out currentTime);
            bool success = DateTime.TryParse(time, new CultureInfo("pl-PL"), DateTimeStyles.None, out currentTime);
            if (success)
            {
                clock.SetTime(currentTime);
                setTime.Invoke(currentTime);
                repeatCounter = 0;
            }
            else
            {
                repeatCounter++;
                if(repeatCounter >= repeatLimit)
                {
                    sendMessage.Invoke(ReturnCommandList.displayMessage + "Unable to set clock!");
                }
                sendMessage.Invoke(ReturnCommandList.repeatTime);
            }
        }

        public void RegisterSetTime(Action<DateTime> action)
        {
            setTime = action;
        }

        public void RegisterSender(Action<string> sender)
        {
            sendMessage = sender;
        }

        internal void CheckClock(string obj)
        {
            string msg = ReturnCommandList.time + DateTime.Now.ToString(new CultureInfo("pl-PL"));
            Console.WriteLine(msg);
            sendMessage.Invoke(msg);
        }
    }
}
