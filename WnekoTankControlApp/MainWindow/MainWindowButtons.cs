using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using CommonsLibrary;
using WnekoTankControlApp.CommandControl.ComDevices;

namespace WnekoTankControlApp
{
    /// <summary>
    /// Partial class for readability containing all button click handlers
    /// </summary>
    public partial class MainWindow
    {
        List<Button> connectButtons;
        bool shouldQueue = false;
        private void Send(string msg)
        {
            if (shouldQueue)
            {
                commandList.Add(msg);
                return;
            }
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
            stabilizeOff.IsChecked = true;
            SendEmergency(msg);
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
            foreach (Button btn in connectButtons) btn.IsEnabled = true;
            DisconnectButton.IsEnabled = false;
            communication?.ClosePort();
            communication = null;
            queue = null;
        }

        private async void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            string port = PortBox.Text;
            try
            {
                if ((bool)usbComRadio.IsChecked) communication = new ComPortCommunication(port);
                else communication = new HC12Communication(port);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            await Connect();
        }

        private async void MockConnectButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                communication = new MockCommunication();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            await Connect();
        }

        private async Task Connect()
        {
            foreach (Button btn in connectButtons) btn.IsEnabled = false;
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
            msg += (bool)moveForwardSoftBox.IsChecked ? ";1" : ";0";
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

        private void startAddingButton_Click(object sender, RoutedEventArgs e)
        {
            shouldQueue = true;
            stopAddingButton.IsEnabled = true;
            startAddingButton.IsEnabled = false;
        }

        private void stopAddingButton_Click(object sender, RoutedEventArgs e)
        {
            shouldQueue = false;
            stopAddingButton.IsEnabled = false;
            startAddingButton.IsEnabled = true;
        }

        private void sendQueuedButton_Click(object sender, RoutedEventArgs e)
        {
            stopAddingButton_Click(null, null);
            foreach (string msg in commandList) Send(msg);
        }

        private void clearQueuedButton_Click(object sender, RoutedEventArgs e)
        {
            commandList = new ObservableCollection<string>();
        }

        private void queueListHideButton_Click(object sender, RoutedEventArgs e)
        {
            if (QueueColumn.Width.Value == 0)
            {
                QueueColumn.Width = new GridLength(1, GridUnitType.Star);
                queueListHideButton.Content = ">>";
            }
            else
            {
                QueueColumn.Width = new GridLength(0, GridUnitType.Pixel);
                queueListHideButton.Content = "<<";
            }
        }

        private void stabilizeOff_Checked(object sender, RoutedEventArgs e)
        {
            string msg = comList.GetCode("stabilize") + "0";
            Send(msg);
        }

        private void stabilizeOn_Checked(object sender, RoutedEventArgs e)
        {
            string msg = comList.GetCode("stabilize") + "1";
            Send(msg);
        }

        private void proximity_Checked(object sender, RoutedEventArgs e)
        {
            string msg = comList.GetCode("setProxSensors");
            if ((bool)proximityNone.IsChecked) msg += "0";
            else if ((bool)proximityStop.IsChecked) msg += "1";
            else if ((bool)proximitySoftStop.IsChecked) msg += "2";
            else if ((bool)proximityStopAndReturn.IsChecked) msg += "3";
            Send(msg);
        }

        private void proxReset_Click(object sender, RoutedEventArgs e)
        {
            queue.ClearQueue();
            string msg = comList.GetCode("emergencyPrefix") + comList.GetCode("clearQueue");
            SendEmergency(msg);
            msg = comList.GetCode("emergencyPrefix") + comList.GetCode("startInvoking");
            SendEmergency(msg);
        }

        private void sendQueueClearBtn_Click(object sender, RoutedEventArgs e)
        {
            queue.ClearQueue();
        }

        private void gimbalStabilizeStartBtn_Click(object sender, RoutedEventArgs e)
        {
            string msg = comList.GetCode("emergencyPrefix") + comList.GetCode("stabilizeGimbal") + "1";
            Send(msg);
        }

        private void gimbalStabilizeStopBtn_Click(object sender, RoutedEventArgs e)
        {
            string msg = comList.GetCode("emergencyPrefix") + comList.GetCode("stabilizeGimbal") + "0";
            Send(msg);
        }

        private void diagnozeBtn_Click(object sender, RoutedEventArgs e)
        {
            string msg = comList.GetCode("emergencyPrefix") + comList.GetCode("diagnoze");
            Send(msg);
        }

        private void electricButton_Click(object sender, RoutedEventArgs e)
        {
            string msg = comList.GetCode("emergencyPrefix") + comList.GetCode("getElectricData");
            Send(msg);
        }

        private void motorsFanCheckBox_Click(object sender, RoutedEventArgs e)
        {
            string msg = comList.GetCode("emergencyPrefix") + comList.GetCode("fanMotorsState");
            msg += (bool)motorsFanCheckBox.IsChecked ? "1" : "0";
            Send(msg);
        }

        private void ledsFanCheckBox_Click(object sender, RoutedEventArgs e)
        {
            string msg = comList.GetCode("emergencyPrefix") + comList.GetCode("fanLedsState");
            msg += (bool)ledsFanCheckBox.IsChecked ? "1" : "0";
            Send(msg);
        }

        private void inasFanCheckBox_Click(object sender, RoutedEventArgs e)
        {
            string msg = comList.GetCode("emergencyPrefix") + comList.GetCode("fanInasState");
            msg += (bool)inasFanCheckBox.IsChecked ? "1" : "0";
            Send(msg);
        }
    }
}
