using Cairo;
using Meadow.Hardware;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WnekoTankMeadow
{
    /// <summary>
    /// Represents communication using serial port
    /// </summary>
    class ComCommunication : ITankCommunication
    {
        ISerialMessagePort port;
        EventHandler<MessageEventArgs> messageEvent;
        /// <summary>
        /// Main constructor
        /// </summary>
        /// <param name="p">Serial port to use for communication</param>
        public ComCommunication(ISerialMessagePort p)
        {
            port = p;
            port.Open();
            port.MessageReceived += Port_MessageReceived;
        }

        /// <summary>
        /// Handles incomming messages and sends acknowledges back
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Port_MessageReceived(object sender, SerialMessageData e)
        {
            string msg = Encoding.ASCII.GetString(e.Message);
#if DEBUG
            foreach (byte b in e.Message)
            {
                Console.WriteLine(b);
            }
            Console.WriteLine($"Received {msg}");
            foreach(byte b in Encoding.ASCII.GetBytes($"ACK:{msg}"))
            {
                Console.WriteLine(b);
            }
            Console.WriteLine($"Trying to send ACK:{msg}");
#endif
            port.Write(Encoding.ASCII.GetBytes($"ACK:{msg}"));
            messageEvent.Invoke(this, new MessageEventArgs(msg));
        }

        /// <summary>
        /// Sends message to PC
        /// </summary>
        /// <param name="msg">Message to be sent</param>
        public void SendMessage(string msg)
        {
            port.Write(Encoding.ASCII.GetBytes(msg));
        }

        /// <summary>
        /// Register method for receiving messages
        /// </summary>
        /// <param name="handler">Delegate of method to be registered</param>
        public void SubscribeToMessages(EventHandler<MessageEventArgs> handler)
        {
            messageEvent += handler;
        }
    }
}
