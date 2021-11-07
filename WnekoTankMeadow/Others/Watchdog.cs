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
        int repeatTime = 2000;
        int repeatCount = 3;
        AutoResetEvent resetEvent;
        Action blockAction;
        Action switchToSerial;
        Func<int, Task> buzzer;
        Type watchdogType;
        CancellationTokenSource source;

        private Action<string> sendMessage;
        private TimeSpan meadowWatchdogTime = TimeSpan.FromSeconds(30);

        public bool IsStarted { get; set; }
        public Watchdog(Type type)
        {
            watchdogType = type;
            resetEvent = new AutoResetEvent(false);
            source = new CancellationTokenSource();
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

        internal void MessageReceived(string obj)
        {
            resetEvent.Set();
            MeadowOS.CurrentDevice.WatchdogReset();
        }

        public void StartCheckingMessages()
        {
            if (!IsStarted)
            {
                MeadowOS.CurrentDevice.WatchdogEnable(meadowWatchdogTime);
                IsStarted = true;
                source = new CancellationTokenSource();
                switch (watchdogType)
                {
                    case Type.SerialPort:
                        StartCheckingMessagesSerial();
                        break;
                    case Type.IP:
                        StartCheckingMessagesIP();
                        break;
                } 
            }
        }

        private void StartCheckingMessagesIP()
        {
#if DEBUG
            Console.WriteLine("Starting watchdog - serial comunication mode!");
#endif
            Thread watchdogThread = new Thread(() =>
            {
                CancellationToken token = source.Token;
                bool signaled = true;
                while (true)
                {
                    if (token.IsCancellationRequested) return;
                    signaled = resetEvent.WaitOne(waitTime);
#if DEBUG
                    Console.WriteLine($"Watchdog signaled: {signaled}");
#endif
                    if (signaled) continue;
                    for (int i = 0; i < repeatCount; i++)
                    {

#if DEBUG
                        Console.WriteLine($"Watchdog not signaled, repeat: {i}");
#endif
                        buzzer(100);
                        sendMessage.Invoke(ReturnCommandList.handShake);
                        signaled = resetEvent.WaitOne(repeatTime);
                        if (signaled) break;
                    }
                    if (!signaled)
                    {
#if DEBUG
                        Console.WriteLine($"Watchdog not signaled, blocking!");
#endif
                        switchToSerial();
                        ChangeType(Type.SerialPort);
                        return;
                    }
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
                CancellationToken token = source.Token;
                bool signaled = true;
                while (true)
                {
                    if (token.IsCancellationRequested) return;
                    signaled = resetEvent.WaitOne(waitTime);
#if DEBUG
                    Console.WriteLine($"Watchdog signaled: {signaled}");
#endif
                    if (signaled) continue;
                    for (int i = 0; i < repeatCount; i++)
                    {

#if DEBUG
                        Console.WriteLine($"Watchdog not signaled, repeat: {i}");
#endif
                        buzzer(100);
                        sendMessage.Invoke(ReturnCommandList.handShake);
                        signaled = resetEvent.WaitOne(repeatTime);
                        if (signaled) break;
                    }
                    if (!signaled)
                    {
#if DEBUG
                        Console.WriteLine($"Watchdog not signaled, blocking!");
#endif
                        blockAction();
                        return;
                    }
                }
            });
            watchdogThread.Start();
        }

        public void Stop()
        {
            if (IsStarted)
            {
                source.Cancel();
                IsStarted = false;
            }
        }

        internal void RegisterBuzzer(Func<int, Task> buzz)
        {
            buzzer += buzz;
        }
    }
}
