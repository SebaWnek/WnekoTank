using Meadow.Hardware;
using System;

namespace MotorControl
{
    public class MotorController
    {
        Motor leftMotor;
        Motor rightMotor;
        GearBox gearbox;

        public MotorController(IPwmPort leftForwardPwm, IPwmPort leftBackPwm, IPwmPort rightForwardPwm, IPwmPort rightBackPwm, IPwmPort gearPwm)
        {
            leftMotor = new Motor(leftForwardPwm, leftBackPwm);
            rightMotor = new Motor(rightForwardPwm, rightBackPwm);
            gearbox = new GearBox(gearPwm);
        }

        public void SetGear(int gear)
        {
            gearbox.SetGear(gear);
        }
    }
}