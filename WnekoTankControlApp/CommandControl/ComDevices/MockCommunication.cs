using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace WnekoTankControlApp.CommandControl.ComDevices
{
    class MockCommunication : ICommunication
    {
        EventHandler<MessageEventArgs> messageEvent;

        public void ClosePort()
        {
            MessageBox.Show("Disconnected!");
        }

        public void SendMessage(string msg)
        {
            string mockAck = $"ACK:{msg}";
            Thread.Sleep(100);
            messageEvent?.Invoke(this, new MessageEventArgs(mockAck));
        }

        public void SubscribeToMessages(EventHandler<MessageEventArgs> handler)
        {
            messageEvent += handler;
        }
    }
}
