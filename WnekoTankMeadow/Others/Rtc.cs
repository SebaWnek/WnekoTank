using System;
using System.Collections.Generic;
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
            Console.WriteLine("Time set from RTC: " + time.ToString());
#endif
        }

        public void SetClockFromPc(string time)
        {
            DateTime currentTime = DateTime.Parse(time);
            clock.SetTime(currentTime);
            setTime.Invoke(currentTime);
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
            string msg = ReturnCommandList.time + DateTime.Now.ToString();
            sendMessage.Invoke(msg);
        }
    }
}
