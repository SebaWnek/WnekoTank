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
using System.Net.Sockets;
using System.Net;
using System.Threading;
using CommonsLibrary;
using Meadow.Gateways;

namespace WnekoTankMeadow.CommandControl.ComDevices
{
    class WifiUdpCommunication : ITankCommunication
    {
        private UdpClient client;
        IPEndPoint localEP;
        IPEndPoint clientEP;
        Action<string> showMessage;
        IWiFiAdapter adapter;
        private object locker;

        public bool ConnectedToWiFi { get; internal set; }
        public bool ConnectedToClient { get; internal set; }

        protected Type communicationType = Type.UDP;
        public Type CommunicationType => communicationType;

        EventHandler<MessageEventArgs> messageEvent;
        //Action<string> signalWatchdog;

        public WifiUdpCommunication(IWiFiAdapter wifi, Action<string>[] showMsg)
        {
            locker = new object();
            adapter = wifi;
            foreach (Action<string> a in showMsg) showMessage += a;
        }

        public void ConnectToWiFi()
        {
            if (ConnectedToWiFi == true) return;
            //Console.WriteLine(adapter.IpAddress);
            //adapter.StartWiFiInterface();
            ConnectionResult connectionResult = null;
            try
            {
#if DEBUG
                ScanForAccessPoints();
#endif
                showMessage.Invoke("Trying to connect to Wifi...");
#if DEBUG
                Console.WriteLine("Trying to connect to Wifi...");
#endif
                connectionResult = adapter.Connect("Meadow", "testtest").Result;

                if (connectionResult.ConnectionStatus != ConnectionStatus.Success)
                {
                    showMessage.Invoke("Cannot connect to WiFi! " + connectionResult.ConnectionStatus.ToString());
#if DEBUG
                    Console.WriteLine("Cannot connect to WiFi! " + connectionResult.ConnectionStatus.ToString());
#endif
                    ConnectedToWiFi = false;
                    return;
                }

                showMessage.Invoke($"Connected to WiFi!");
#if DEBUG
                Console.WriteLine($"Connected to WiFi!");
#endif
                ConnectedToWiFi = true;
            }
            catch (Exception e)
            {
                showMessage.Invoke(ReturnCommandList.exception + e.Message + ReturnCommandList.exceptionTrace + e.StackTrace);
#if DEBUG
                Console.WriteLine(e.Message + "/n/n" + e.StackTrace);
#endif
            }
        }

        public void Connect(IPAddress ip, int port)
        {
            try
            {
                showMessage.Invoke($"Connecting: {ip}:{port}");
#if DEBUG
                Console.WriteLine($"Trying to connect to: {ip}:{port}");
#endif
                localEP = new IPEndPoint(IPAddress.Any, 22222);
                clientEP = new IPEndPoint(ip, port);
                client = new UdpClient(localEP);
                client.Connect(clientEP);
                showMessage.Invoke($"Connected: {clientEP.Address}:{clientEP.Port}");
#if DEBUG
                Console.WriteLine($"Connected: {clientEP.Address}:{clientEP.Port}");
#endif
                byte[] response = Encoding.ASCII.GetBytes($"Connected: {clientEP.Address}:{clientEP.Port}");
                client.Send(response, response.Length);
                ConnectedToClient = true;
                ListenUdp();
            }
            catch (Exception e)
            {
                showMessage.Invoke(ReturnCommandList.exception + e.Message + ReturnCommandList.exceptionTrace + e.StackTrace);
#if DEBUG
                Console.WriteLine(e.Message + "\n\n" + e.StackTrace);
#endif
            }
        }

        public void Connect(string msg)
        {
            string[] data = msg.Split(';');
            Connect(IPAddress.Parse(data[0]), int.Parse(data[1])); //assuming data validation on controll app for now
        }

        private async void AcceptConnection()
        {
            await Task.Run(() =>
            {
                try
                {
                    byte[] buffer = client.Receive(ref clientEP);
                    client.Receive(ref clientEP);
                    client.Connect(clientEP);
                    byte[] response = Encoding.ASCII.GetBytes($"Connected: {clientEP.Address}:{clientEP.Port}");
#if DEBUG
                    Console.WriteLine($"Connected: {clientEP.Address}:{clientEP.Port}");
#endif
                    client.Send(response, response.Length);
                }
                catch (Exception e)
                {
                    showMessage.Invoke(ReturnCommandList.exception + e.Message + ReturnCommandList.exceptionTrace + e.StackTrace);
#if DEBUG
                    Console.WriteLine(e.Message + "/n/n" + e.StackTrace);
#endif
                }
            });
        }

        private void ShowIP()
        {
            string ip = "IP:" + localEP.Address + ":" + localEP.Port;
            showMessage.Invoke(ip);
        }

        private string GetIP()
        {
            string ip = "IP:" + localEP.Address + ":" + localEP.Port;
            return ip;
        }

        private void ListenUdp()
        {
            SendMessage("Starting listener!");
#if DEBUG
            Console.WriteLine("Starting listener!");
#endif
            Thread listener = new Thread(() =>
            {
                try
                {
                    byte[] buffer;
                    string msg, response;
                    while (true)
                    {
                        buffer = client.Receive(ref clientEP);
                        lock (locker)
                        {
                            msg = Encoding.ASCII.GetString(buffer);
#if DEBUG
                            Console.WriteLine("Received: " + msg);
                            Console.WriteLine($"Trying to send {ReturnCommandList.acknowledge}{msg}");
#endif
                            response = ReturnCommandList.acknowledge + msg;
                            buffer = Encoding.ASCII.GetBytes(response);
                            client.Send(buffer, buffer.Length);
                            messageEvent.Invoke(this, new MessageEventArgs(msg));
                        }
                        //signalWatchdog?.Invoke(msg);
                    }
                }
                catch (Exception e)
                {
                    showMessage.Invoke(ReturnCommandList.exception + e.Message + ReturnCommandList.exceptionTrace + e.StackTrace);
#if DEBUG
                    Console.WriteLine(e.Message + "/n/n" + e.StackTrace);
#endif
                }
            });
            listener.Start();
        }


        public void SendMessage(string msg)
        {
            lock (locker)
            {
                byte[] buffer = Encoding.ASCII.GetBytes(msg);
#if DEBUG
                Console.WriteLine("Trying to send: " + msg);
#endif
                client.Send(buffer, buffer.Length);
            }
        }

        public void SubscribeToMessages(EventHandler<MessageEventArgs> handler)
        {
            messageEvent += handler;
        }

        public void SendMessage(object sender, string msg)
        {
            SendMessage(msg);
        }

        //public void RegisterWatchdog(Action<string> action)
        //{
        //    signalWatchdog = action;
        //}

        internal void Disconnect()
        {
            client.Close();
            //Removing references just in case, probably not needed as socket closed, so no events will fire anyway
            messageEvent = null;
            //signalWatchdog = null;
            showMessage = null;
        }

        protected void ScanForAccessPoints()
        {
            Console.WriteLine("Getting list of access points.");
            var networks = adapter.Scan();
            if (networks.Count > 0)
            {
                Console.WriteLine("|-------------------------------------------------------------|---------|");
                Console.WriteLine("|         Network Name             | RSSI |       BSSID       | Channel |");
                Console.WriteLine("|-------------------------------------------------------------|---------|");
                foreach (WifiNetwork accessPoint in networks)
                {
                    Console.WriteLine($"| {accessPoint.Ssid,-32} | {accessPoint.SignalDbStrength,4} | {accessPoint.Bssid,17} |   {accessPoint.ChannelCenterFrequency,3}   |");
                }
            }
            else
            {
                Console.WriteLine($"No access points detected.");
            }
        }

        public void UnsubscribeMessages()
        {
            messageEvent = null;
        }
    }
}
