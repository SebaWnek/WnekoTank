using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace WnekoTankControlApp
{
    class ComPortCommunication : ICommunication
    {
        private SerialPort port;
        EventHandler<MessageEventArgs> messageEvent;


        public ComPortCommunication(string portNum)
        {
            try
            {
                port = new SerialPort(portNum, 921600, Parity.None, 8, StopBits.One);
                port.DataReceived += Port_DataReceived;
                port.Open();
                port.WriteLine("ACK");
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            string message = port.ReadLine();
            messageEvent?.Invoke(this, new MessageEventArgs(message));
        }

        public SerialPort Port { get => port; }

        public void SendMessage(string msg)
        {
            port.WriteLine(msg);
        }

        public void SubscribeToMessages(EventHandler<MessageEventArgs> handler)
        {
            messageEvent += handler;
        }

        public void ClosePort()
        {
            port.Close();
            port.Dispose();
        }
    }
}
