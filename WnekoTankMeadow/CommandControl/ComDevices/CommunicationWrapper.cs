using System;
using System.Collections.Generic;
using System.Text;
using WnekoTankMeadow.CommandControl;

namespace WnekoTankMeadow
{
    class CommunicationWrapper : ITankCommunication
    {
        private ITankCommunication currentCommunication;
        public Type CommunicationType => currentCommunication.CommunicationType;
        //Action<string> watchdogAction;
        //private EventHandler<MessageEventArgs> messageEvent;

        public CommunicationWrapper(ITankCommunication com)
        {
            currentCommunication = com;
        }

        public void SetCommunication(ITankCommunication com)
        {
            //currentCommunication.UnsubscribeMessages();
            currentCommunication = com;
            //currentCommunication.RegisterWatchdog(watchdogAction);
            //currentCommunication.SubscribeToMessages(messageEvent);
        }

        //public void RegisterWatchdog(Action<string> action)
        //{
        //    //watchdogAction = action;
        //    //currentCommunication.RegisterWatchdog(watchdogAction);
        //}

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
            //messageEvent = handler;
            //currentCommunication.SubscribeToMessages(messageEvent);
        }

        public void UnsubscribeMessages()
        {
            //currentCommunication.UnsubscribeMessages();
        }
    }

    public enum Type
    {
        UDP,
        TCP,
        RF,
        Serial
    }
}
