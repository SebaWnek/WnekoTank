using Meadow.Hardware;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MotorControl
{
    class Motor
    {
        private IPwmPort leftForwardPwm;
        private IPwmPort leftBackPwm;

        public Motor(IPwmPort leftForwardPwm, IPwmPort leftBackPwm)
        {
            this.leftForwardPwm = leftForwardPwm;
            this.leftBackPwm = leftBackPwm;
        }
    }
}
