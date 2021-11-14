using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace WnekoTankControlApp.CommandControl.ComDevices
{
    class WifiUdpCommunication : ICommunication
    {
        private UdpClient client;
        IPEndPoint localEP;
        IPEndPoint clientEP;
        EventHandler<MessageEventArgs> messageEvent;
        CancellationTokenSource source;

        public WifiUdpCommunication(IPEndPoint local)
        {
            localEP = local;
            client = new UdpClient(localEP);
            source = new CancellationTokenSource();
            StartListening(source.Token);
        }

        private void StartListening(CancellationToken token)
        {
            Thread listener = new Thread(() =>
            {
                byte[] buffer;
                buffer = client.Receive(ref clientEP);
                ConnectToClient();
                while (true)
                {
                    if (token.IsCancellationRequested) return;
                    try
                    {
                        buffer = client.Receive(ref clientEP);
                    }
                    catch (SocketException e)
                    {
                        MessageBox.Show(e.Message + "\n\n" + e.StackTrace, e.SocketErrorCode.ToString(), MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    string msg = Encoding.ASCII.GetString(buffer);
                    if (msg.Contains("\n"))
                    {
                        string[] messages = msg.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (string m in messages) messageEvent?.Invoke(this, new MessageEventArgs(m));
                    }
                    else messageEvent?.Invoke(this, new MessageEventArgs(msg));
                }
            })
            {

            };
            listener.Start();
        }

        public void ConnectToClient()
        {
            client.Connect(clientEP);
        }

        public void ClosePort()
        {
            client.Close();
            client.Dispose();
            source.Cancel();
        }

        public void SendMessage(string msg)
        {
            byte[] data = Encoding.ASCII.GetBytes(msg);
            client.Send(data, data.Length);
        }

        public void SubscribeToMessages(EventHandler<MessageEventArgs> handler)
        {
            messageEvent = handler;
        }
    }
}
