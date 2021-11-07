using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
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
        private void Gear1btn_Click(object sender, RoutedEventArgs e)
        {
            string msg = TankCommandList.setGear + "1";
            //string msg = CommandList.setGear") + "1";
            Send(msg);
        }

        private void Gear2btn_Click(object sender, RoutedEventArgs e)
        {
            string msg = TankCommandList.setGear + "2";
            Send(msg);
        }

        private void StopEmergencyBtn_Click(object sender, RoutedEventArgs e)
        {
            string msg = TankCommandList.emergencyPrefix + TankCommandList.stop;
            SendEmergency(msg);
            ClearQueue_Click(this, null);
            stabilizeOff.IsChecked = true;
            SendEmergency(msg);
        }

        private void SoftStopButton_Click(object sender, RoutedEventArgs e)
        {
            string msg = TankCommandList.softStop + "0";
            Send(msg);
        }
        private void StopNormalButton_Click(object sender, RoutedEventArgs e)
        {
            string msg = TankCommandList.stop + "0";
            Send(msg);
        }

        private void QueueStart_Click(object sender, RoutedEventArgs e)
        {
            string msg = TankCommandList.emergencyPrefix + TankCommandList.startInvoking;
            Send(msg);
        }

        private void QueueStop_Click(object sender, RoutedEventArgs e)
        {
            string msg = TankCommandList.emergencyPrefix + TankCommandList.stopInvoking;
            string msg2 = TankCommandList.handshake;
            Send(msg);
            Send(msg2);
        }

        private void ListQueue_Click(object sender, RoutedEventArgs e)
        {
            string msg = TankCommandList.emergencyPrefix + TankCommandList.enumerateQueue;
            Send(msg);
        }

        private void WaitButton_Click(object sender, RoutedEventArgs e)
        {
            string msg = TankCommandList.wait + waitBox.Text;
            Send(msg);
        }

        private void SetSpeedButton_Click(object sender, RoutedEventArgs e)
        {
            string msg = TankCommandList.setLinearSpeed + setSpeedBox.Text;
            Send(msg);
        }

        private void SetTurnButton_Click(object sender, RoutedEventArgs e)
        {
            string msg = TankCommandList.setTurn + setTurnBox.Text;
            Send(msg);
        }

        private void HandshakeButton_Click(object sender, RoutedEventArgs e)
        {
            string msg = TankCommandList.emergencyPrefix + TankCommandList.handshake;
            SendEmergency(msg);
        }

        private void TempPresBtn_Click(object sender, RoutedEventArgs e)
        {
            string msg = TankCommandList.tempPres;
            Send(msg);
        }

        private void PositionButton_Click(object sender, RoutedEventArgs e)
        {
            string msg = TankCommandList.position;
            Send(msg);
        }

        private void CalibrateBtn_Click(object sender, RoutedEventArgs e)
        {
            string msg = TankCommandList.calibrate;
            Send(msg);
        }

        private void CheckCalibrationBtn_Click(object sender, RoutedEventArgs e)
        {
            string msg = TankCommandList.checkCalibration;
            Send(msg);
        }

        private void ClearQueue_Click(object sender, RoutedEventArgs e)
        {
            string msg = TankCommandList.emergencyPrefix + TankCommandList.clearQueue;
            SendEmergency(msg);
        }

        private void MoveForwardByButton_Click(object sender, RoutedEventArgs e)
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

        private void TurnByButton_Click(object sender, RoutedEventArgs e)
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

        private void StartAddingButton_Click(object sender, RoutedEventArgs e)
        {
            shouldQueue = true;
            stopAddingButton.IsEnabled = true;
            startAddingButton.IsEnabled = false;
        }

        private void StopAddingButton_Click(object sender, RoutedEventArgs e)
        {
            shouldQueue = false;
            stopAddingButton.IsEnabled = false;
            startAddingButton.IsEnabled = true;
        }

        private void SendQueuedButton_Click(object sender, RoutedEventArgs e)
        {
            StopAddingButton_Click(null, null);
            foreach (string msg in commandList) Send(msg);
        }

        private void ClearQueuedButton_Click(object sender, RoutedEventArgs e)
        {
            commandList = new ObservableCollection<string>();
        }

        private void QueueListHideButton_Click(object sender, RoutedEventArgs e)
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

        private void StabilizeOff_Checked(object sender, RoutedEventArgs e)
        {
            string msg = TankCommandList.stabilizeDirection + "0";
            Send(msg);
        }

        private void StabilizeOn_Checked(object sender, RoutedEventArgs e)
        {
            string msg = TankCommandList.stabilizeDirection + "1";
            Send(msg);
        }

        private void Proximity_Checked(object sender, RoutedEventArgs e)
        {
            string msg = TankCommandList.setProxSensors;
            if ((bool)proximityNone.IsChecked) msg += "0";
            else if ((bool)proximityStop.IsChecked) msg += "1";
            else if ((bool)proximitySoftStop.IsChecked) msg += "2";
            else if ((bool)proximityStopAndReturn.IsChecked) msg += "3";
            Send(msg);
        }

        private void ProxReset_Click(object sender, RoutedEventArgs e)
        {
            outQueue.ClearQueue();
            string msg = TankCommandList.emergencyPrefix + TankCommandList.clearQueue;
            SendEmergency(msg);
            msg = TankCommandList.emergencyPrefix + TankCommandList.startInvoking;
            SendEmergency(msg);
        }

        private void SendQueueClearBtn_Click(object sender, RoutedEventArgs e)
        {
            outQueue.ClearQueue();
        }

        private void GimbalStabilizeStartBtn_Click(object sender, RoutedEventArgs e)
        {
            string msg = TankCommandList.emergencyPrefix + TankCommandList.stabilizeGimbal + "1;0";
            Send(msg);
        }

        private void GimbalStabilizeWithHorizontalStartBtn_Click(object sender, RoutedEventArgs e)
        {

            string msg = TankCommandList.emergencyPrefix + TankCommandList.stabilizeGimbal + "1;1";
            Send(msg);
        }

        private void GimbalStabilizeStopBtn_Click(object sender, RoutedEventArgs e)
        {
            string msg = TankCommandList.emergencyPrefix + TankCommandList.stabilizeGimbal + "0;0";
            Send(msg);
        }

        private void DiagnozeBtn_Click(object sender, RoutedEventArgs e)
        {
            string msg = TankCommandList.emergencyPrefix + TankCommandList.diagnoze;
            Send(msg);
        }

        private void ElectricButton_Click(object sender, RoutedEventArgs e)
        {
            string msg = TankCommandList.emergencyPrefix + TankCommandList.getElectricData;
            Send(msg);
        }

        private void MotorsFanCheckBox_Click(object sender, RoutedEventArgs e)
        {
            string msg = TankCommandList.emergencyPrefix + TankCommandList.fanMotorsState;
            msg += (bool)motorsFanCheckBox.IsChecked ? "1" : "0";
            Send(msg);
        }

        private void LedsFanCheckBox_Click(object sender, RoutedEventArgs e)
        {
            string msg = TankCommandList.emergencyPrefix + TankCommandList.fanLedsState;
            msg += (bool)ledsFanCheckBox.IsChecked ? "1" : "0";
            Send(msg);
        }

        private void InasFanCheckBox_Click(object sender, RoutedEventArgs e)
        {
            string msg = TankCommandList.emergencyPrefix + TankCommandList.fanInasState;
            msg += (bool)inasFanCheckBox.IsChecked ? "1" : "0";
            Send(msg);
        }

        private void DelayButton_Click(object sender, RoutedEventArgs e)
        {
            string msg = TankCommandList.emergencyPrefix + TankCommandList.setElectricDataDelay;
            int dt = int.Parse(delayBox.Text);
            msg += dt;
            Send(msg);

        }

        private void CheckClockBtn_Click(object sender, RoutedEventArgs e)
        {
            string msg = TankCommandList.checkClock;
            Send(msg);
        }

        private void SetClockBtn_Click(object sender, RoutedEventArgs e)
        {
            string msg = TankCommandList.setClock + DateTime.Now.ToString();
            Send(msg);
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            string msg = TankCommandList.emergencyPrefix + TankCommandList.resetDevice;
            SendEmergency(msg);
        }
    }
}
