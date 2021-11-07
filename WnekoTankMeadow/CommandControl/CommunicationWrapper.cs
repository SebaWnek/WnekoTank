using System;
using System.Collections.Generic;
using System.Text;

namespace WnekoTankMeadow
{
    class CommunicationWrapper : ITankCommunication
    {
        private ITankCommunication currentCommunication;

        public CommunicationWrapper(ITankCommunication com)
        {
            currentCommunication = com;
        }

        public void SetCommunication(ITankCommunication com)
        {
            currentCommunication = com;
        }

        public object Locker => throw new NotImplementedException();

        public void RegisterWatchdog(Action<string> action)
        {
            currentCommunication.RegisterWatchdog(action);
        }

        public void SendMessage(object sender, string msg)
        {
            currentCommunication.SendMessage(sender, msg);
        }

        public void SendMessage(string msg)
        {
            currentCommunication.SendMessage(msg);
        }

        public void SubscribeToMessages(EventHandler<MessageEventArgs> handler)
        {
            currentCommunication.SubscribeToMessages(handler);
        }
    }
}
