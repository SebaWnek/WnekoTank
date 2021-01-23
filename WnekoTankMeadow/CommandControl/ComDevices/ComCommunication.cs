using Cairo;
using Meadow.Hardware;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WnekoTankMeadow
{
    class ComCommunication : ITankCommunication
    {
        ISerialMessagePort port;
        EventHandler<MessageEventArgs> messageEvent;
        public ComCommunication(ISerialMessagePort p)
        {
            port = p;
            port.Open();
            port.MessageReceived += Port_MessageReceived;
        }

        private void Port_MessageReceived(object sender, SerialMessageData e)
        {
            string msg = Encoding.ASCII.GetString(e.Message);
            port.Write(Encoding.ASCII.GetBytes($"ACK:{msg}"));
            messageEvent.Invoke(this, new MessageEventArgs(msg));
        }

        public void SendMessage(string msg)
        {
            port.Write(Encoding.ASCII.GetBytes(msg));
        }

        public void SubscribeToMessages(EventHandler<MessageEventArgs> handler)
        {
            messageEvent += handler;
        }
    }
}
