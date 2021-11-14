using CommonsLibrary;
using Meadow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WnekoTankMeadow.Others
{
    class Watchdog
    {
        public enum Type
        {
            SerialPort,
            IP
        }

        int waitTime = 10000;
        int repeatTime = 3000;
        int repeatCount = 3;
        AutoResetEvent ipResetEvent;
        AutoResetEvent serialResetEvent;
        Action blockAction;
        Action switchToSerial;
        Func<int, Task> buzzer;
        Type watchdogType;
        CancellationTokenSource udpSource;
        CancellationTokenSource serialSource;

        private Action<string> sendMessage;
        private TimeSpan meadowWatchdogTime = TimeSpan.FromMilliseconds(32767);
        private bool meadowWatchdogStarted = false;

        public bool IsStarted { get; set; }
        public Watchdog(Type type)
        {
            watchdogType = type;
            serialResetEvent = new AutoResetEvent(false);
            ipResetEvent = new AutoResetEvent(false);
        }

        public void ChangeType(Type type)
        {
            Stop();
            watchdogType = type;
            StartCheckingMessages();
        }

        public void RegisterSwitchToSerial(Action switchAction)
        {
            switchToSerial += switchAction;
        }

        public void RegisterSender(Action<string> sendAction)
        {
            sendMessage += sendAction;
        }

        public void RemoveSender(Action<string> sendAction)
        {
            sendMessage -= sendAction; 
        }

        public void ResetSender()
        {
            sendMessage = null;
        }

        public void RegisterBlockAction(Action stopAction)
        {
            blockAction = stopAction;
        }

        internal void MessageReceived(object sender, MessageEventArgs msg)
        {
            MeadowOS.CurrentDevice.WatchdogReset();
#if DEBUG
            Console.WriteLine($"Message received by watchdog from {(sender as ITankCommunication).CommunicationType}, current type: {watchdogType}");
#endif
            if (watchdogType == Type.IP) ipResetEvent?.Set();
            else serialResetEvent?.Set();
        }

        public void StartCheckingMessages()
        {
            if (!IsStarted)
            {
                if (!meadowWatchdogStarted)
                {
                    MeadowOS.CurrentDevice.WatchdogEnable(meadowWatchdogTime);
                    meadowWatchdogStarted = true;
                }
                IsStarted = true;
                switch (watchdogType)
                {
                    case Type.SerialPort:
                        serialSource = new CancellationTokenSource();
                        StartCheckingMessagesSerial();
                        break;
                    case Type.IP:
                        udpSource = new CancellationTokenSource();
                        StartCheckingMessagesIP();
                        break;
                } 
            }
        }

        private void StartCheckingMessagesIP()
        {
#if DEBUG
            Console.WriteLine("Starting watchdog - IP comunication mode!");
#endif
            Thread watchdogThread = new Thread(() =>
            {
                try
                {
                    CancellationToken token = udpSource.Token;
                    bool signaled = true;
                    while (true)
                    {
                        if (token.IsCancellationRequested)
                        {
#if DEBUG
                            Console.WriteLine($"IP Watchdog cancelled!");
#endif
                            return;
                        }
                        signaled = ipResetEvent.WaitOne(waitTime);
#if DEBUG
                        Console.WriteLine($"IP Watchdog signaled: {signaled}");
#endif
                        if (signaled) continue;
                        for (int i = 0; i < repeatCount; i++)
                        {

#if DEBUG
                            Console.WriteLine($"IP Watchdog not signaled, repeat: {i}");
#endif
                            sendMessage.Invoke(ReturnCommandList.handShake);
                            buzzer(100);
                            signaled = ipResetEvent.WaitOne(repeatTime);
                            if (signaled) break;
                        }
                        if (!signaled)
                        {
                            if (token.IsCancellationRequested)
                            {
#if DEBUG
                                Console.WriteLine($"IP Watchdog cancelled!");
#endif
                                return;
                            }
#if DEBUG
                            Console.WriteLine($"IP Watchdog not signaled, switching to serial!");
#endif
                            switchToSerial();
                            ChangeType(Type.SerialPort);
                            return;
                        }
                    }
                }
                catch (Exception e)
                {
                    sendMessage.Invoke(ReturnCommandList.exception + e.Message + ReturnCommandList.exceptionTrace + e.StackTrace);
#if DEBUG
                    Console.WriteLine(e.Message + "/n/n" + e.StackTrace);
#endif
                }
            });
            watchdogThread.Start();
        }

        private void StartCheckingMessagesSerial()
        {
#if DEBUG
            Console.WriteLine("Starting watchdog - serial comunication mode!");
#endif
            Thread watchdogThread = new Thread(() =>
            {
                try
                {
                    CancellationToken token = serialSource.Token;
                    bool signaled = true;
                    while (true)
                    {
                        if (token.IsCancellationRequested)
                        {
#if DEBUG
                            Console.WriteLine($"Serial Watchdog cancelled!");
#endif
                            return;
                        }
                        signaled = serialResetEvent.WaitOne(waitTime);
#if DEBUG
                        Console.WriteLine($"Serial Watchdog signaled: {signaled}");
#endif
                        if (signaled) continue;
                        for (int i = 0; i < repeatCount; i++)
                        {

#if DEBUG
                            Console.WriteLine($"Serial Watchdog not signaled, repeat: {i}");
#endif
                            sendMessage.Invoke(ReturnCommandList.handShake);
                            buzzer(100);
                            signaled = serialResetEvent.WaitOne(repeatTime);
                            if (signaled) break;
                        }
                        if (!signaled)
                        {
                            if (token.IsCancellationRequested)
                            {
#if DEBUG
                                Console.WriteLine($"Serial Watchdog cancelled!");
#endif
                                return;
                            }
#if DEBUG
                            Console.WriteLine($"Serial Watchdog not signaled, blocking!");
#endif
                            blockAction();
                            return;
                        }
                    }
                }
                catch (Exception e)
                {
                    sendMessage.Invoke(ReturnCommandList.exception + e.Message + ReturnCommandList.exceptionTrace + e.StackTrace);
#if DEBUG
                    Console.WriteLine(e.Message + "/n/n" + e.StackTrace);
#endif
                }
            });
            watchdogThread.Start();
        }

        public void Stop()
        {
            if (IsStarted)
            {
                udpSource?.Cancel();
                serialSource?.Cancel();
                ipResetEvent.Set();
                serialResetEvent.Set();
                IsStarted = false;
            }
        }

        internal void RegisterBuzzer(Func<int, Task> buzz)
        {
            buzzer += buzz;
        }
    }
}
