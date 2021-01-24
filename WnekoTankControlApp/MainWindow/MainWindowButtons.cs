using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using WnekoTankMeadow;

namespace WnekoTankControlApp
{
    /// <summary>
    /// Partial class for readability containing all button click handlers
    /// </summary>
    public partial class MainWindow 
    {
        private void gear1btn_Click(object sender, RoutedEventArgs e)
        {
            string msg = comList.GetCode("setGear") + "1";
            queue.SendMessage(msg);
        }

        private void gear2btn_Click(object sender, RoutedEventArgs e)
        {
            string msg = comList.GetCode("setGear") + "2";
            queue.SendMessage(msg);
        }

        private void stopBtn_Click(object sender, RoutedEventArgs e)
        {
            string msg = comList.GetCode("emergencyPrefix") + comList.GetCode("stop") + "0";
            queue.SendEmergencyMessage(msg);
            clearQueue_Click(this, null);
        }

        private void queueStart_Click(object sender, RoutedEventArgs e)
        {
            string msg = comList.GetCode("emergencyPrefix") + comList.GetCode("startInvoking") + "0";
            queue.SendMessage(msg);
        }

        private void queueStop_Click(object sender, RoutedEventArgs e)
        {
            string msg = comList.GetCode("emergencyPrefix") + comList.GetCode("stopInvoking") + "0";
            string msg2 = comList.GetCode("handshake");
            queue.SendMessage(msg);
            queue.SendMessage(msg2);
        }

        private void listQueue_Click(object sender, RoutedEventArgs e)
        {
            string msg = comList.GetCode("emergencyPrefix") + comList.GetCode("enumerateQueue") + "0";
            queue.SendMessage(msg);
        }

        private void waitButton_Click(object sender, RoutedEventArgs e)
        {
            string msg = comList.GetCode("wait") + waitBox.Text;
            queue.SendMessage(msg);
        }

        private void setSpeedButton_Click(object sender, RoutedEventArgs e)
        {
            string msg = comList.GetCode("setLinearSpeed") + setSpeedBox.Text;
            queue.SendMessage(msg);
        }

        private void setTurnButton_Click(object sender, RoutedEventArgs e)
        {
            string msg = comList.GetCode("setTurn") + setTurnBox.Text;
            queue.SendMessage(msg);
        }

        private void handshakeButton_Click(object sender, RoutedEventArgs e)
        {
            string msg = comList.GetCode("emergencyPrefix") + comList.GetCode("handshake");
            queue.SendEmergencyMessage(msg);
        }

        private void DisconnectButton_Click(object sender, RoutedEventArgs e)
        {
            communication?.ClosePort();
            communication = null;
            queue = null;
        }

        private void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            communication = new ComPortCommunication(PortBox.Text);
            queue = new MessageQueue(communication, DisplayMessage);
            queue.SendMessage(CommandList.handshake);
            queue.SendMessage(CommandList.handshake);
        }

        private void clearQueue_Click(object sender, RoutedEventArgs e)
        {
            string msg = comList.GetCode("emergencyPrefix") + comList.GetCode("clearQueue") + "0";
        }

    }
}
