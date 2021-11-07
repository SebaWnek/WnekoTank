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

namespace WnekoTankMeadow.CommandControl.ComDevices
{
    class WifiUdpCommunication : MeadowApp, ITankCommunication
    {
        private UdpClient client;
        IPEndPoint localEP;
        IPEndPoint clientEP;
        Action<string> showMessage;
        public object Locker => new object();

        public bool Connected { get; internal set; }

        EventHandler<MessageEventArgs> messageEvent;
        Action<string> signalWatchdog;

        public WifiUdpCommunication(Action<string>[] showMsg)
        {
            Console.WriteLine(3);
            foreach (Action<string> a in showMsg) showMessage += a;
            Console.WriteLine(4);
            ConnectToWiFi();
            localEP = new IPEndPoint(IPAddress.Any, 22222);
            client = new UdpClient(localEP);
            
            showMessage.Invoke($"Connected WiFi! {Dns.GetHostEntry(Dns.GetHostName()).AddressList[0]}");
#if DEBUG
            Console.WriteLine("Connected to WiFi!");
#endif
        }

        private void ConnectToWiFi()
        {
            showMessage.Invoke("Trying to connect to Wifi...");
#if DEBUG
            Console.WriteLine(5);
            Console.WriteLine("Trying to connect to Wifi...");
#endif
            Console.WriteLine(6);
            Device.InitWiFiAdapter().Wait();
            ConnectionResult result = Device.WiFiAdapter.Connect("Meadow", "testtest").Result;

            if (result.ConnectionStatus != ConnectionStatus.Success)
            {
                showMessage.Invoke("Cannot connect to WiFi!");
#if DEBUG
                Console.WriteLine("Cannot connect to WiFi!");
#endif
                Connected = false;
                return;
            }

            Connected = true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="showMsg"></param>
        /// <param name="msg">
        /// Incoming data: IP address;port
        /// </param>
        public WifiUdpCommunication(string msg, Action<string>[] showMsg) : this(showMsg)
        {
            string[] data = msg.Split(';');
            Connect(data[0], data[1]); //assuming data validation on controll app for now
        }

        private void Connect(string ip, string port)
        {
            clientEP = new IPEndPoint(IPAddress.Parse(ip), int.Parse(port));
            client.Connect(clientEP);
            showMessage.Invoke($"Connected: {clientEP.Address}:{clientEP.Port}");
#if DEBUG
            Console.WriteLine($"Connected: {clientEP.Address}:{clientEP.Port}");
#endif
            byte[] response = Encoding.ASCII.GetBytes($"Connected: {clientEP.Address}:{clientEP.Port}");
            client.Send(response, response.Length);
            ListenUdp();
        }

        private async void AcceptConnection()
        {
            await Task.Run(() =>
            {
                byte[] buffer = client.Receive(ref clientEP);
                client.Receive(ref clientEP);
                client.Connect(clientEP);
                byte[] response = Encoding.ASCII.GetBytes($"Connected: {clientEP.Address}:{clientEP.Port}");
#if DEBUG
                Console.WriteLine($"Connected: {clientEP.Address}:{clientEP.Port}");
#endif
                client.Send(response, response.Length);
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
            Thread listener = new Thread(() =>
            {
                byte[] buffer;
                string msg, response;
                while (true)
                {
                    buffer = client.Receive(ref clientEP);
                    lock (Locker)
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
                    signalWatchdog?.Invoke(msg);
                }
            });
            listener.Start();
        }


        public void SendMessage(string msg)
        {
            lock (Locker)
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

        public void RegisterWatchdog(Action<string> action)
        {
            signalWatchdog += action;
        }

        internal void Disconnect()
        {
            client.Close();
            //Removing references just in case, probably not needed as socket closed, so no events will fire anyway
            messageEvent = null;
            signalWatchdog = null;
            showMessage = null;
        }
    }
}
