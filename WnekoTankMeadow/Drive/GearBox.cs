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
    /// <summary>
    /// Controls changing gears
    /// </summary>
    class GearBox
    {
        private IPwmPort pwm;
        private Servo servo;
        //Values determined empirically
        int gear1 = 53;
        int gear2 = -18;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="gearPwm">PWM port for controlling servo</param>
        public GearBox(IPwmPort gearPwm)
        {
            pwm = gearPwm;
            ServoConfig config = new ServoConfig(0, 180, 500, 2500, 50);
            servo = new Servo(pwm, config);
        }

        /// <summary>
        /// Moves servo to set requested gear
        /// </summary>
        /// <param name="gear">Requested gear</param>
        public void SetGear(int gear)
        {
            if (gear == 1) SetAngle(gear1);
            else SetAngle(gear2);
        }

        /// <summary>
        /// Allows fine tuning of servo or sets it to requested angle
        /// </summary>
        /// <param name="angle">Requested angle</param>
        public async void SetAngle(int angle)
        {
            servo.RotateTo(angle + 90);
            await Task.Delay(500);
            servo.Stop();
        }
    }
}
