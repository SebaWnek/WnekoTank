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
        int waitTime = 5000;
        int repeatTime = 1000;
        int repeatCount = 3;
        AutoResetEvent resetEvent;
        Action blockAction;

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
            Thread watchdogThread = new Thread(() =>
            {
                bool signaled;
                while (true)
                {
                    signaled = false;
                    if (signaled = resetEvent.WaitOne(waitTime)) continue;
                    for (int i = 0; i < repeatCount; i++)
                    {
                        sendMessage(ReturnCommandList.handShake);
                        if (signaled = resetEvent.WaitOne(repeatTime)) break;
                    }
                    if (!signaled)
                    {
                        blockAction();
                        return;
                    }
                }
            });
        }
    }
}
