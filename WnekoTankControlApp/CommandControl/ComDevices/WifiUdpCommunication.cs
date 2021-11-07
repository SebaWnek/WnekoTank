using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WnekoTankControlApp.CommandControl.ComDevices
{
    class WifiCommunication : ICommunication
    {
        public void ClosePort()
        {
            throw new NotImplementedException();
        }

        public void SendMessage(string msg)
        {
            throw new NotImplementedException();
        }

        public void SubscribeToMessages(EventHandler<MessageEventArgs> handler)
        {
            throw new NotImplementedException();
        }
    }
}
