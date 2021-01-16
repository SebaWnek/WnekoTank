﻿using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WnekoTankControlApp
{
    class ComPortCommunication : ICommunication
    {
        private SerialPort port;
        EventHandler<MessageEventArgs> messageEvent;


        public ComPortCommunication()
        {
            port = new SerialPort("COM4", 921600, Parity.None, 8, StopBits.One);
            port.DataReceived += Port_DataReceived;
            port.Open();
            port.WriteLine("ACK");
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



    }
}
