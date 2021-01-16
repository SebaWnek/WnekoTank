using System;

namespace WnekoTankControlApp
{
    interface ICommunication
    {
        void SendMessage(string msg);
        void SubscribeToMessages(EventHandler<MessageEventArgs> handler);
        void ClosePort();
    }
}