using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WnekoTankControlApp.CommandControl.ComDevices
{
    class CommunicationWrapper : ICommunication
    {
        EventHandler<MessageEventArgs> messageEvent;
        ICommunication com;
        public ICommunication Com {
            get => com;
            set 
            {
                com = value;
                com.SubscribeToMessages(messageEvent);
            } 
        }

        public void ClosePort()
        {
            com?.ClosePort();
        }

        public void SendMessage(string msg)
        {
            com.SendMessage(msg);
        }

        public void SubscribeToMessages(EventHandler<MessageEventArgs> handler)
        {
            messageEvent += handler;
            com.SubscribeToMessages(messageEvent);
        }
    }
}
