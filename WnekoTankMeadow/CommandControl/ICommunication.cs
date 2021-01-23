using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WnekoTankMeadow
{
    interface ITankCommunication
    {
        void SendMessage(string msg);
        void SubscribeToMessages(EventHandler<MessageEventArgs> handler);
    }
}
