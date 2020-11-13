using Meadow.Hardware;
using System;

namespace WnekoTankMeadow
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

        public void SetLinearSpeed(int speed)
        {
            Console.WriteLine($"setting speed: {speed}");
            leftMotor.SetSpeed(speed);
            rightMotor.SetSpeed(speed);
        }

        public void SetTurn(int turn)
        {
            leftMotor.SetTurn(turn);
            rightMotor.SetTurn(-turn);
        }

        public void Break()
        {
            leftMotor.SetTurn(0);
            rightMotor.SetTurn(0);
        }
    }
}