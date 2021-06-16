using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using CommonsLibrary;
using WnekoTankControlApp.CommandControl;

namespace WnekoTankControlApp
{
    public partial class MainWindow
    {
        private void RegisterMethods()
        {
            inQueue.RegisterMethod(ReturnCommandList.calibrationData, CalibrationDataReceived);
            inQueue.RegisterMethod(ReturnCommandList.diagnosticData, DiagnosticDataReceived);
            inQueue.RegisterMethod(ReturnCommandList.electricData, ElectricDataReceived);
            inQueue.RegisterMethod(ReturnCommandList.positionData, PositionDataReceived);
            inQueue.RegisterMethod(ReturnCommandList.tempHumidData, AtmosphericDataReceived);
            inQueue.RegisterMethod(ReturnCommandList.exception, ExceptionReceived);
            inQueue.RegisterMethod(ReturnCommandList.acknowledge, AckReceived);
            inQueue.RegisterMethod(ReturnCommandList.lowBattery, LowBatteryReceived);
            inQueue.RegisterMethod(ReturnCommandList.dischargedBattery, DischargedBatteryReceived);
            inQueue.RegisterMethod(ReturnCommandList.handShake, HandshakeReceived);
        }

        private void HandshakeReceived(string obj)
        {
            SendEmergency(TankCommandList.handshake);
        }

        private void DischargedBatteryReceived(string obj)
        {
            MessageBox.Show(obj, "Battery discharged!!", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void LowBatteryReceived(string obj)
        {
            MessageBox.Show(obj, "Battery low!", MessageBoxButton.OK, MessageBoxImage.Exclamation);
        }

        private void AckReceived(string obj)
        {

        }

        private void ExceptionReceived(string obj)
        {
            int tracePosition = obj.IndexOf(ReturnCommandList.exceptionTrace);
            string exception = obj.Substring(0, tracePosition);
            string trace = obj.Substring(tracePosition + 3);
            MessageBox.Show(exception + "\n\n" + trace, "Exception!", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void AtmosphericDataReceived(string obj)
        {
            throw new NotImplementedException();
        }

        private void ElectricDataReceived(string obj)
        {
            try
            {
                electricData = DeconstructElectricData(obj);
                UpdateElectricDataBoxes();
                UpdateElectricPlot();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message + "\n\n" + e.StackTrace, "Exception!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateElectricPlot()
        {
            DateTime currentTime = DateTime.Now;
            double dT = (currentTime - startTime).TotalSeconds;
            electricPlotModel.AddPoint("L", dT, new double[] { double.Parse(electricData[1][0], CultureInfo.InvariantCulture), double.Parse(electricData[1][1], CultureInfo.InvariantCulture) });
            electricPlotModel.AddPoint("R", dT, new double[] { double.Parse(electricData[2][0], CultureInfo.InvariantCulture), double.Parse(electricData[2][1], CultureInfo.InvariantCulture) });
            electricPlotModel.AddPoint("C", dT, new double[] { double.Parse(electricData[0][0], CultureInfo.InvariantCulture), double.Parse(electricData[0][1], CultureInfo.InvariantCulture) });
            electricPlot.InvalidatePlot();
        }

        private void UpdateElectricDataBoxes()
        {
            Dispatcher.Invoke(() => centVBatBox.Text = electricData[0][0]);
            Dispatcher.Invoke(() => centABatBox.Text = electricData[0][1]);
            Dispatcher.Invoke(() => centWBatBox.Text = electricData[0][2]);
            Dispatcher.Invoke(() => leftVBatBox.Text = electricData[1][0]);
            Dispatcher.Invoke(() => leftABatBox.Text = electricData[1][1]);
            Dispatcher.Invoke(() => leftWBatBox.Text = electricData[1][2]);
            Dispatcher.Invoke(() => rightVBatBox.Text = electricData[2][0]);
            Dispatcher.Invoke(() => rightABatBox.Text = electricData[2][1]);
            Dispatcher.Invoke(() => rightWBatBox.Text = electricData[2][2]);
        }

        private string[][] DeconstructElectricData(string input)
        {
            string[] data = input.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            int counter = 0;
            string[] cElectric = new string[3];
            string[] lElectric = new string[3];
            string[] rElectric = new string[3];
            string[] currentElectric = null;
            foreach (string str in data)
            {
                switch (str)
                {
                    case "C":
                        currentElectric = cElectric;
                        //box[0] = centVBatBox;
                        //box[1] = centABatBox;
                        //box[2] = centWBatBox;
                        counter = 0;
                        break;
                    case "L":
                        currentElectric = lElectric;
                        //box[0] = leftVBatBox;
                        //box[1] = leftABatBox;
                        //box[2] = leftWBatBox;
                        counter = 0;
                        break;
                    case "R":
                        currentElectric = rElectric;
                        //box[0] = rightVBatBox;
                        //box[1] = rightABatBox;
                        //box[2] = rightWBatBox;
                        counter = 0;
                        break;
                    default:
                        currentElectric[counter] = str;
                        //Dispatcher.Invoke(() => box[counter].Text = str);
                        counter++;
                        break;
                }
                //if (counter > 2) break;
            }
            return new string[][] { cElectric, lElectric, rElectric };
        }

        private void PositionDataReceived(string obj)
        {
            throw new NotImplementedException();
        }

        private void DiagnosticDataReceived(string obj)
        {
            throw new NotImplementedException();
        }

        private void CalibrationDataReceived(string obj)
        {
            throw new NotImplementedException();
        }
    }
}
