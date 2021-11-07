using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WnekoTankMeadow
{
    /// <summary>
    /// Interface for communication devices. They must allow sending messages to PC and accepting delegate that will handle incomming messages
    /// </summary>
    interface ITankCommunication
    {
        void SendMessage(object sender, string msg);
        void SendMessage(string msg);
        void SubscribeToMessages(EventHandler<MessageEventArgs> handler);
        void RegisterWatchdog(Action<string> action);
        object Locker { get; }
    }
}
