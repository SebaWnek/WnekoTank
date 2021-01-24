﻿using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace WnekoTankControlApp
{
    /// <summary>
    /// Responsible for communication using serial port
    /// </summary>
    class ComPortCommunication : ICommunication
    {
        private SerialPort port;
        EventHandler<MessageEventArgs> messageEvent;

        /// <summary>
        /// Main constructor
        /// </summary>
        /// <param name="portNum">Name of COM port to be used</param>
        public ComPortCommunication(string portNum)
        {
            try
            {
                port = new SerialPort(portNum, 921600, Parity.None, 8, StopBits.One);
                port.DataReceived += Port_DataReceived;
                port.Open();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Passes received message to registered methods
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Message from serial port</param>
        private void Port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            string message = port.ReadLine();
            messageEvent?.Invoke(this, new MessageEventArgs(message));
        }

        /// <summary>
        /// Sends message
        /// </summary>
        /// <param name="msg">Message to be sent</param>
        public void SendMessage(string msg)
        {
            port.WriteLine(msg);
        }

        /// <summary>
        /// Registers method to receive messages received by port
        /// </summary>
        /// <param name="handler"></param>
        public void SubscribeToMessages(EventHandler<MessageEventArgs> handler)
        {
            messageEvent += handler;
        }

        /// <summary>
        /// Disposes of port
        /// </summary>
        public void ClosePort()
        {
            port.Close();
            port.Dispose();
        }
    }
}