using System;

namespace WnekoTankControlApp
{
    /// <summary>
    /// Interface for communication device. Compared to vehicle one contains also method for correctly closing port, ommited on vehicle for simplicity
    /// </summary>
    interface ICommunication
    {
        void SendMessage(string msg);
        void SubscribeToMessages(EventHandler<MessageEventArgs> handler);
        void ClosePort();
    }
}