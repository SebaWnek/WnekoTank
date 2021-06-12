using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using CommonsLibrary;

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
            TextBox[] box = new TextBox[3];
            int counter = 0;
            string[] data = obj.Split(new char[] {';'}, StringSplitOptions.RemoveEmptyEntries);
            foreach (string str in data)
            {
                switch (str)
                {
                    case "C":
                        box[0] = centVBatBox;
                        box[1] = centABatBox;
                        box[2] = centWBatBox;
                        counter = 0;
                        break;
                    case "L":
                        box[0] = leftVBatBox;
                        box[1] = leftABatBox;
                        box[2] = leftWBatBox;
                        counter = 0;
                        break;
                    case "R":
                        box[0] = rightVBatBox;
                        box[1] = rightABatBox;
                        box[2] = rightWBatBox;
                        counter = 0;
                        break;
                    default:
                        Dispatcher.Invoke(() => box[counter].Text = str);
                        counter++;
                        break;
                }
                if (counter > 2) break;
            }
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
