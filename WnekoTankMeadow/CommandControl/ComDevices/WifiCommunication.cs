using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Meadow;
using Meadow.Devices;
using Meadow.Gateway;
using Meadow.Hardware;
using Meadow.Gateway.WiFi;
using Meadow.Foundation;

namespace WnekoTankMeadow.CommandControl.ComDevices
{
    class WifiCommunication : MeadowApp, ITankCommunication
    {
        public object locker => new object();

        EventHandler<MessageEventArgs> messageEvent;

        public void SendMessage(string msg)
        {
            throw new NotImplementedException();
        }

        public void SubscribeToMessages(EventHandler<MessageEventArgs> handler)
        {
            messageEvent += handler;
        }

        public WifiCommunication()
        {
            Device.InitWiFiAdapter().Wait();
            ConnectionResult result = Device.WiFiAdapter.Connect("Meadow", "testtest");

            if (result.ConnectionStatus != ConnectionStatus.Success)
            {
                //if (Device.WiFiAdapter.Connect(Secrets.WIFI_NAME, Secrets.WIFI_PASSWORD).ConnectionStatus != ConnectionStatus.Success) {
                throw new Exception($"Cannot connect to network: {result.ConnectionStatus}");
            }


        }
    }
}
