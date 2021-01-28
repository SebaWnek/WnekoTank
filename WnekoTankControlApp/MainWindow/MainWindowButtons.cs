using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using CommonsLibrary;

namespace WnekoTankControlApp
{
    /// <summary>
    /// Partial class for readability containing all button click handlers
    /// </summary>
    public partial class MainWindow
    {
        private void Send(string msg)
        {
            try
            {
                queue.SendMessage(msg);
            }
            catch (Exception ex)
            {
                if(ex is NullReferenceException) MessageBox.Show("Connect first!", "Not connected!", MessageBoxButton.OK, MessageBoxImage.Error);
                else MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
        }

        private void SendEmergency(string msg)
        {
            try
            {
                queue.SendEmergencyMessage(msg);
            }
            catch (Exception ex)
            {
                if (ex is NullReferenceException) MessageBox.Show("Connect first!", "Not connected!", MessageBoxButton.OK, MessageBoxImage.Error);
                else MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
        }





        private void gear1btn_Click(object sender, RoutedEventArgs e)
        {
            string msg = comList.GetCode("setGear") + "1";
            Send(msg);
        }

        private void gear2btn_Click(object sender, RoutedEventArgs e)
        {
            string msg = comList.GetCode("setGear") + "2";
            Send(msg);
        }

        private void stopEmergencyBtn_Click(object sender, RoutedEventArgs e)
        {
            string msg = comList.GetCode("emergencyPrefix") + comList.GetCode("stop");
            SendEmergency(msg);
            clearQueue_Click(this, null);
        }

        private void softStopButton_Click(object sender, RoutedEventArgs e)
        {
            string msg = comList.GetCode("softStop") + "0";
            Send(msg);
        }
        private void stopNormalButton_Click(object sender, RoutedEventArgs e)
        {
            string msg = comList.GetCode("stop") + "0";
            Send(msg);
        }

        private void queueStart_Click(object sender, RoutedEventArgs e)
        {
            string msg = comList.GetCode("emergencyPrefix") + comList.GetCode("startInvoking");
            Send(msg);
        }

        private void queueStop_Click(object sender, RoutedEventArgs e)
        {
            string msg = comList.GetCode("emergencyPrefix") + comList.GetCode("stopInvoking");
            string msg2 = comList.GetCode("handshake");
            Send(msg);
            Send(msg2);
        }

        private void listQueue_Click(object sender, RoutedEventArgs e)
        {
            string msg = comList.GetCode("emergencyPrefix") + comList.GetCode("enumerateQueue");
            Send(msg);
        }

        private void waitButton_Click(object sender, RoutedEventArgs e)
        {
            string msg = comList.GetCode("wait") + waitBox.Text;
            Send(msg);
        }

        private void setSpeedButton_Click(object sender, RoutedEventArgs e)
        {
            string msg = comList.GetCode("setLinearSpeed") + setSpeedBox.Text;
            Send(msg);
        }

        private void setTurnButton_Click(object sender, RoutedEventArgs e)
        {
            string msg = comList.GetCode("setTurn") + setTurnBox.Text;
            Send(msg);
        }

        private void handshakeButton_Click(object sender, RoutedEventArgs e)
        {
            string msg = comList.GetCode("emergencyPrefix") + comList.GetCode("handshake");
            SendEmergency(msg);
        }

        private void tempPresBtn_Click(object sender, RoutedEventArgs e)
        {
            string msg = comList.GetCode("tempPres");
            Send(msg);
        }

        private void positionButton_Click(object sender, RoutedEventArgs e)
        {
            string msg = comList.GetCode("position");
            Send(msg);
        }

        private void calibrateBtn_Click(object sender, RoutedEventArgs e)
        {
            string msg = comList.GetCode("calibrate");
            Send(msg);
        }

        private void checkCalibrationBtn_Click(object sender, RoutedEventArgs e)
        {
            string msg = comList.GetCode("checkCalibration");
            Send(msg);
        }

        private void DisconnectButton_Click(object sender, RoutedEventArgs e)
        {
            ConnectButton.IsEnabled = true;
            DisconnectButton.IsEnabled = false;
            communication?.ClosePort();
            communication = null;
            queue = null;
        }

        private async void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                communication = new ComPortCommunication(PortBox.Text);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            ConnectButton.IsEnabled = false;
            DisconnectButton.IsEnabled = true;
            outputBox.Text += "Connecting... \r\n";
            queue = new MessageQueue(communication, DisplayMessage);

            await Task.Delay(1000);
            Send(CommandList.handshake);
            await Task.Delay(1000);
            Send(CommandList.handshake);
        }

        private void clearQueue_Click(object sender, RoutedEventArgs e)
        {
            string msg = comList.GetCode("emergencyPrefix") + comList.GetCode("clearQueue");
            SendEmergency(msg);
        }

        private void moveForwardByButton_Click(object sender, RoutedEventArgs e)
        {
            string msg = comList.GetCode("moveForwardBy");
            msg += moveForwardBySpeedBox.Text;
            msg += ';' + moveForwardByDistBox.Text;
            msg += (bool)shouldStopAfterCheckBox.IsChecked ? ";1" : ";0";
            if ((bool)moveForwardBySendGearCheckbox.IsChecked)
            {
                msg += (bool)firstGearRadio.IsChecked ? ";1" : ";2";
            }
            else msg += ";0";
            Send(msg);
        }

        private void turnByButton_Click(object sender, RoutedEventArgs e)
        {
            string msg = comList.GetCode("turnBy");
            msg += turnByAngleBox.Text;
            msg += ';' + turnBySpeedBox.Text;
            if ((bool)turnBySendGearCheckbox.IsChecked)
            {
                msg += (bool)firstGearRadio.IsChecked ? ";1" : ";2";
            }
            else msg += ";0";
            Send(msg);
        }
    }
}
