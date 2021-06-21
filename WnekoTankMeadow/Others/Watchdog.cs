using CommonsLibrary;
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
        int waitTime = 10000;
        int repeatTime = 2000;
        int repeatCount = 3;
        AutoResetEvent resetEvent;
        Action blockAction;
        Func<int, Task> buzzer;

        private Action<string> sendMessage;

        public Watchdog()
        {
            resetEvent = new AutoResetEvent(false);
        }

        public void RegisterSender(Action<string> sendAction)
        {
            sendMessage += sendAction;
        }

        public void RegisterBlockAction(Action stopAction)
        {
            blockAction = stopAction;
        }

        internal void MessageReceived(string obj)
        {
            resetEvent.Set();
        }

        internal void StartCheckingMessages()
        {
#if DEBUG
            Console.WriteLine("Starting watchdog!");
#endif
            Thread watchdogThread = new Thread(() =>
            {
                bool signaled = true;
                while (true)
                {
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
                        if(signaled) break;
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

        internal void RegisterBuzzer(Func<int, Task> buzz)
        {
            buzzer += buzz;
        }
    }
}
