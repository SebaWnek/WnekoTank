using Cairo;
using Meadow.Hardware;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonsLibrary;

namespace WnekoTankMeadow
{
    /// <summary>
    /// Represents communication using serial port
    /// </summary>
    class ComCommunication : ITankCommunication
    {
        ISerialMessagePort port;
        EventHandler<MessageEventArgs> messageEvent;
        Action<string> signalWatchdog;

        public object Locker => new object();

        /// <summary>
        /// Main constructor
        /// </summary>
        /// <param name="p">Serial port to use for communication</param>
        public ComCommunication(ISerialMessagePort p)
        {
            port = p;
            port.Open();
            port.MessageReceived += Port_MessageReceived;
            SendMessage("Waiting for connection...");
        }

        /// <summary>
        /// Handles incomming messages and sends acknowledges back
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Port_MessageReceived(object sender, SerialMessageData e)
        {
            string msg;
            lock (Locker)
            {
                msg = Encoding.ASCII.GetString(e.Message);
#if DEBUG
                //foreach (byte b in e.Message)
                //{
                //    Console.WriteLine(b);
                //}
                Console.WriteLine($"Received {msg}");
                //foreach (byte b in Encoding.ASCII.GetBytes($"ACK:{msg}"))
                //{
                //    Console.WriteLine(b);
                //}
                Console.WriteLine($"Trying to send {ReturnCommandList.acknowledge}{msg}");
#endif
                port.Write(Encoding.ASCII.GetBytes($"{ReturnCommandList.acknowledge}{msg}"));


                signalWatchdog?.Invoke(msg);
            }
            messageEvent.Invoke(this, new MessageEventArgs(msg));
        }

        /// <summary>
        /// Sends message to PC
        /// </summary>
        /// <param name="msg">Message to be sent</param>
        public void SendMessage(string msg)
        {
            lock (Locker)
            {
                port.Write(Encoding.ASCII.GetBytes(msg));
            }
        }

        /// <summary>
        /// Register method for receiving messages
        /// </summary>
        /// <param name="handler">Delegate of method to be registered</param>
        public void SubscribeToMessages(EventHandler<MessageEventArgs> handler)
        {
            lock (Locker)
            {
                messageEvent += handler;
            }
        }

        public void SendMessage(object sender, string msg)
        {
            lock (Locker)
            {
#if DEBUG
                Console.WriteLine($"Sending: -{msg}-");
#endif
                SendMessage(msg);
            }
        }

        public void RegisterWatchdog(Action<string> action)
        {
            signalWatchdog += action;
        }
    }
}
