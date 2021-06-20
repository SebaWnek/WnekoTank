using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;
using CommonsLibrary;
using WnekoTankControlApp.CommandControl;
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
                outQueue.SendMessage(msg);
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
                outQueue.SendEmergencyMessage(msg);
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
            string msg = TankCommandList.setGear + "1";
            //string msg = CommandList.setGear") + "1";
            Send(msg);
        }

        private void gear2btn_Click(object sender, RoutedEventArgs e)
        {
            string msg = TankCommandList.setGear + "2";
            Send(msg);
        }

        private void stopEmergencyBtn_Click(object sender, RoutedEventArgs e)
        {
            string msg = TankCommandList.emergencyPrefix + TankCommandList.stop;
            SendEmergency(msg);
            clearQueue_Click(this, null);
            stabilizeOff.IsChecked = true;
            SendEmergency(msg);
        }

        private void softStopButton_Click(object sender, RoutedEventArgs e)
        {
            string msg = TankCommandList.softStop + "0";
            Send(msg);
        }
        private void stopNormalButton_Click(object sender, RoutedEventArgs e)
        {
            string msg = TankCommandList.stop + "0";
            Send(msg);
        }

        private void queueStart_Click(object sender, RoutedEventArgs e)
        {
            string msg = TankCommandList.emergencyPrefix + TankCommandList.startInvoking;
            Send(msg);
        }

        private void queueStop_Click(object sender, RoutedEventArgs e)
        {
            string msg = TankCommandList.emergencyPrefix + TankCommandList.stopInvoking;
            string msg2 = TankCommandList.handshake;
            Send(msg);
            Send(msg2);
        }

        private void listQueue_Click(object sender, RoutedEventArgs e)
        {
            string msg = TankCommandList.emergencyPrefix + TankCommandList.enumerateQueue;
            Send(msg);
        }

        private void waitButton_Click(object sender, RoutedEventArgs e)
        {
            string msg = TankCommandList.wait + waitBox.Text;
            Send(msg);
        }

        private void setSpeedButton_Click(object sender, RoutedEventArgs e)
        {
            string msg = TankCommandList.setLinearSpeed + setSpeedBox.Text;
            Send(msg);
        }

        private void setTurnButton_Click(object sender, RoutedEventArgs e)
        {
            string msg = TankCommandList.setTurn + setTurnBox.Text;
            Send(msg);
        }

        private void handshakeButton_Click(object sender, RoutedEventArgs e)
        {
            string msg = TankCommandList.emergencyPrefix + TankCommandList.handshake;
            SendEmergency(msg);
        }

        private void tempPresBtn_Click(object sender, RoutedEventArgs e)
        {
            string msg = TankCommandList.tempPres;
            Send(msg);
        }

        private void positionButton_Click(object sender, RoutedEventArgs e)
        {
            string msg = TankCommandList.position;
            Send(msg);
        }

        private void calibrateBtn_Click(object sender, RoutedEventArgs e)
        {
            string msg = TankCommandList.calibrate;
            Send(msg);
        }

        private void checkCalibrationBtn_Click(object sender, RoutedEventArgs e)
        {
            string msg = TankCommandList.checkCalibration;
            Send(msg);
        }

        private void DisconnectButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (Button btn in connectButtons) btn.IsEnabled = true;
            DisconnectButton.IsEnabled = false;
            communication?.ClosePort();
            communication = null;
            outQueue = null;
            inQueue = null;
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
                leftCameraImage = 
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
            outQueue = new OutgoingMessageQueue(communication, DisplayMessage);
            inQueue = new IncommingMessageQueue();
            communication.SubscribeToMessages(inQueue.IncommingMessageHandler);
            RegisterMethods();

            await Task.Delay(1000);
            Send(TankCommandList.handshake);
            await Task.Delay(1000);
            Send(TankCommandList.handshake);
        }

        private void clearQueue_Click(object sender, RoutedEventArgs e)
        {
            string msg = TankCommandList.emergencyPrefix + TankCommandList.clearQueue;
            SendEmergency(msg);
        }

        private void moveForwardByButton_Click(object sender, RoutedEventArgs e)
        {
            string msg = TankCommandList.moveForwardBy;
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
            string msg = TankCommandList.turnBy;
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
            string msg = TankCommandList.stabilize + "0";
            Send(msg);
        }

        private void stabilizeOn_Checked(object sender, RoutedEventArgs e)
        {
            string msg = TankCommandList.stabilize + "1";
            Send(msg);
        }

        private void proximity_Checked(object sender, RoutedEventArgs e)
        {
            string msg = TankCommandList.setProxSensors;
            if ((bool)proximityNone.IsChecked) msg += "0";
            else if ((bool)proximityStop.IsChecked) msg += "1";
            else if ((bool)proximitySoftStop.IsChecked) msg += "2";
            else if ((bool)proximityStopAndReturn.IsChecked) msg += "3";
            Send(msg);
        }

        private void proxReset_Click(object sender, RoutedEventArgs e)
        {
            outQueue.ClearQueue();
            string msg = TankCommandList.emergencyPrefix + TankCommandList.clearQueue;
            SendEmergency(msg);
            msg = TankCommandList.emergencyPrefix + TankCommandList.startInvoking;
            SendEmergency(msg);
        }

        private void sendQueueClearBtn_Click(object sender, RoutedEventArgs e)
        {
            outQueue.ClearQueue();
        }

        private void gimbalStabilizeStartBtn_Click(object sender, RoutedEventArgs e)
        {
            string msg = TankCommandList.emergencyPrefix + TankCommandList.stabilizeGimbal + "1;0";
            Send(msg);
        }

        private void gimbalStabilizeWithHorizontalStartBtn_Click(object sender, RoutedEventArgs e)
        {

            string msg = TankCommandList.emergencyPrefix + TankCommandList.stabilizeGimbal + "1;1";
            Send(msg);
        }

        private void gimbalStabilizeStopBtn_Click(object sender, RoutedEventArgs e)
        {
            string msg = TankCommandList.emergencyPrefix + TankCommandList.stabilizeGimbal + "0;0";
            Send(msg);
        }

        private void diagnozeBtn_Click(object sender, RoutedEventArgs e)
        {
            string msg = TankCommandList.emergencyPrefix + TankCommandList.diagnoze;
            Send(msg);
        }

        private void electricButton_Click(object sender, RoutedEventArgs e)
        {
            string msg = TankCommandList.emergencyPrefix + TankCommandList.getElectricData;
            Send(msg);
        }

        private void motorsFanCheckBox_Click(object sender, RoutedEventArgs e)
        {
            string msg = TankCommandList.emergencyPrefix + TankCommandList.fanMotorsState;
            msg += (bool)motorsFanCheckBox.IsChecked ? "1" : "0";
            Send(msg);
        }

        private void ledsFanCheckBox_Click(object sender, RoutedEventArgs e)
        {
            string msg = TankCommandList.emergencyPrefix + TankCommandList.fanLedsState;
            msg += (bool)ledsFanCheckBox.IsChecked ? "1" : "0";
            Send(msg);
        }

        private void inasFanCheckBox_Click(object sender, RoutedEventArgs e)
        {
            string msg = TankCommandList.emergencyPrefix + TankCommandList.fanInasState;
            msg += (bool)inasFanCheckBox.IsChecked ? "1" : "0";
            Send(msg);
        }

        private void delayButton_Click(object sender, RoutedEventArgs e)
        {
            string msg = TankCommandList.emergencyPrefix + TankCommandList.setElectricDataDelay;
            int dt = int.Parse(delayBox.Text);
            msg += dt;
            Send(msg);

        }
    }
}
