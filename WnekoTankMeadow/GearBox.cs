using Meadow.Foundation.Servos;
using Meadow.Hardware;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WnekoTankMeadow
{
    class GearBox
    {
        private IPwmPort pwm;
        private Servo servo;

        public GearBox(IPwmPort gearPwm)
        {
            pwm = gearPwm;
            ServoConfig config = new ServoConfig(0, 180, 500, 2500, 50);
            servo = new Servo(pwm, config);
        }

        public async void SetGear(int angle)
        {
            servo.RotateTo(angle + 90);
            await Task.Delay(500);
            servo.Stop();
        }
    }
}
