using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WnekoTankControlApp
{
    class MessageQueue
    {
        AutoResetEvent canTransmit = new AutoResetEvent(true);
        private ICommunication comPort;
        Action<string> DisplayMessage;
        BlockingCollection<string> queue = new BlockingCollection<string>();

        public MessageQueue(ICommunication com, Action<string> display)
        {
            comPort = com;
            comPort.SubscribeToMessages(DataReceived);
            DisplayMessage = display;
        }

        private void DataReceived(object sender, MessageEventArgs e)
        {
            string msg = e.Message;
            if (msg.Contains("ACK"))
            {
                canTransmit.Set();
            }
            DisplayMessage.Invoke(e.Message);
        }

        public void SendMessage(string msg)
        {
            queue.Add(msg);
        }

        public void SendEmergencyMessage(string msg)
        {
            comPort.SendMessage(msg);
        }

        private void SendMessagesFromQueue()
        {
            while (true)
            {
                canTransmit.WaitOne();
                string msg = queue.Take();
                comPort.SendMessage(msg);
            }
        }
    }
}
