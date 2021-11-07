using CommonsLibrary;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WnekoTankMeadow.CommandControl.ComDevices;
using WnekoTankMeadow.Others;

namespace WnekoTankMeadow
{
    /// <summary>
    /// Helper methods not specific to any subsystem
    /// </summary>
    public partial class MeadowApp : App<F7Micro, MeadowApp>
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

        public static void Diangoze(string empty)
        {
            Process currentProcess = Process.GetCurrentProcess();
#if DEBUG
            Console.WriteLine(currentProcess.Id);
            //Console.WriteLine(currentProcess.ProcessName);
            Console.WriteLine(currentProcess.Threads.Count);
#endif
            ProcessThreadCollection currentThreads = currentProcess.Threads;

            foreach (ProcessThread thread in currentThreads)
            {
#if DEBUG
                Console.WriteLine(thread.Id.ToString());
#endif
                //com.SendMessage(thread.Id.ToString());
                Thread.Sleep(20);
            }
        }


        /// <summary>
        /// Lock device from performing any action, either in emergency situation, or when battery is discharged too much
        /// </summary>
        private void EmergencyDisable()
        {
            motor.Break();
            queue.LockQueue();
            onboardLed.SetColor(Color.Red);
#pragma warning disable CS4014 // To wywołanie nie jest oczekiwane, dlatego wykonywanie bieżącej metody będzie kontynuowane do czasu ukończenia wywołania
            buzzer.BuzzPulse(100, 100, int.MaxValue);
#pragma warning restore CS4014 // To wywołanie nie jest oczekiwane, dlatego wykonywanie bieżącej metody będzie kontynuowane do czasu ukończenia wywołania
            displaySmall.Write("!!!EMERGENDY DISABLE  ENGAGED!!!");
        }

        /// <summary>
        /// Reaction to hand shake message from controll app
        /// </summary>
        /// <param name="empty">Incoming parameters, none needed so should me empty</param>
        public void HandShake(string empty)
        {
#pragma warning disable CS4014 // To wywołanie nie jest oczekiwane, dlatego wykonywanie bieżącej metody będzie kontynuowane do czasu ukończenia wywołania
            buzzer.Buzz(200);
#pragma warning restore CS4014 // To wywołanie nie jest oczekiwane, dlatego wykonywanie bieżącej metody będzie kontynuowane do czasu ukończenia wywołania
        }

        /// <summary>
        /// Starts watchdog and sets clock, as RTC not programmed yet, later will be using RTC for that
        /// </summary>
        /// <param name="time">DateTime.Now.ToString() from controll app</param>
        public void Hello(string time)
        {
            if (!watchdog.IsStarted) watchdog.StartCheckingMessages();
            displaySmall.Write("Radio com       received!");
        }

        public void SwitchToSerial()
        {
            motor.Break();
            queue.ClearQueue();
            (ipCom as WifiUdpCommunication).Disconnect();
            ipCom = null;
            (com as CommunicationWrapper).SetCommunication(serialCom);
            watchdog.ChangeType(Watchdog.Type.SerialPort);
            com.SendMessage(ReturnCommandList.displayMessage + "Switched to serial communication!");
        }

        public void SwitchToIP(string data)
        {
            motor.Break();
            queue.ClearQueue();
            ipCom = new WifiUdpCommunication(data, new Action<string>[] { com.SendMessage, displaySmall.Write }); //Waiting for it to return
            if ((ipCom as WifiUdpCommunication).Connected)
            {
                (com as CommunicationWrapper).SetCommunication(ipCom);
                watchdog.ChangeType(Watchdog.Type.IP);
                ipCom.RegisterWatchdog(watchdog.MessageReceived);
                com.SendMessage(ReturnCommandList.displayMessage + "Switched to IP communication!");
            }
            else
            {
                com.SendMessage(ReturnCommandList.exception + "Unable to change communication method!");
            }
        }

        public static void SetClock(DateTime time)
        {
            Device.SetClock(time);
#if DEBUG
            Console.WriteLine("Time set to: " + time.ToString());
#endif
        }

        public static void ResetDevice(string empty)
        {
            MeadowOS.CurrentDevice.Reset();
        }
    }
}
