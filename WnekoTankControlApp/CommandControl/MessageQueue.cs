using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WnekoTankControlApp
{
    /// <summary>
    /// Class responsible for communication with vehicle 
    /// </summary>
    class MessageQueue
    {
        AutoResetEvent canTransmit = new AutoResetEvent(true);
        private ICommunication comPort;
        Action<string> DisplayMessage;
        BlockingCollection<string> queue = new BlockingCollection<string>();

        /// <summary>
        /// Main constructor
        /// </summary>
        /// <param name="com">Communication device compatible with ICommunication interface</param>
        /// <param name="display">Delegate for printing messages in app window</param>
        public MessageQueue(ICommunication com, Action<string> display)
        {
            comPort = com;
            comPort.SubscribeToMessages(DataReceived);
            DisplayMessage = display;
            Thread sender = new Thread(SendMessagesFromQueue);
            sender.Start();
        }

        /// <summary>
        /// Handles incomming data. If message is acknowledgement of previous message sent it allows next message in queue to be sent
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Message from communication device</param>
        private void DataReceived(object sender, MessageEventArgs e)
        {
            string msg = e.Message;
            if (msg.Contains("ACK"))
            {
                canTransmit.Set();
            }
            DisplayMessage.Invoke(e.Message);
        }

        /// <summary>
        /// Adds message to queue to be sent
        /// </summary>
        /// <param name="msg">Message</param>
        public void SendMessage(string msg)
        {
            DisplayMessage.Invoke("Queueing: " + msg);
            queue.Add(msg);
        }

        /// <summary>
        /// Sends message bypassing queue, used for messages that must be sent ASAP, e.g. stoping vehicle
        /// </summary>
        /// <param name="msg">Message</param>
        public void SendEmergencyMessage(string msg)
        {
            DisplayMessage.Invoke("Sending EMERGNECY: " + msg);
            comPort.SendMessage(msg);
        }

        /// <summary>
        /// Sends messages from queue,
        /// If messages are present, sends first and waits for acknowledgement,
        /// If no messages are present waits for new one
        /// </summary>
        private void SendMessagesFromQueue()
        {
            while (true)
            {
                canTransmit.WaitOne();
                string msg = queue.Take();
                DisplayMessage.Invoke("Sending: " + msg);
                comPort.SendMessage(msg);
            }
        }
    }
}
