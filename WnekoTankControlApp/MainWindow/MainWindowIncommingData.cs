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
        }

        private void CalibrationDataReceived(string obj)
        {
            throw new NotImplementedException();
        }
    }
}
