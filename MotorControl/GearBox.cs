using Meadow.Foundation.Servos;
using Meadow.Hardware;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MotorControl
{
    class GearBox
    {
        private IPwmPort pwm;
        private Servo servo;

        public GearBox(IPwmPort gearPwm)
        {
            pwm = gearPwm;
            ServoConfig config = new ServoConfig(-90, 90, 500, 2500, 50);
            servo = new Servo(pwm, config);
        }

        public void SetGear(int angle)
        {
            servo.RotateTo(angle);
        }
    }
}
