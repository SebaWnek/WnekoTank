using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        }

        private void AtmosphericDataReceived(string obj)
        {
            throw new NotImplementedException();
        }

        private void ElectricDataReceived(string obj)
        {
            throw new NotImplementedException();
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
