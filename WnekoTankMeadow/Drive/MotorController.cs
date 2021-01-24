using Meadow.Hardware;
using System;

namespace WnekoTankMeadow
{
    /// <summary>
    /// Class responsible for all movement operations, connecting all driving subsystems
    /// </summary>
    public class MotorController
    {
        Motor leftMotor;
        Motor rightMotor;
        GearBox gearbox;

        /// <summary>
        /// Main constructor
        /// </summary>
        /// <param name="leftForwardPwm">PWM port responsible for left motor moving forward</param>
        /// <param name="leftBackPwm">PWM port responsible for left motor moving backward</param>
        /// <param name="rightForwardPwm">PWM port responsible for right motor moving forward</param>
        /// <param name="rightBackPwm">PWM port responsible for right motor moving backward</param>
        /// <param name="gearPwm">PWM port responsible for controling gear changing servo</param>
        public MotorController(IPwmPort leftForwardPwm, IPwmPort leftBackPwm, IPwmPort rightForwardPwm, IPwmPort rightBackPwm, IPwmPort gearPwm)
        {
            leftMotor = new Motor(leftForwardPwm, leftBackPwm);
            rightMotor = new Motor(rightForwardPwm, rightBackPwm);
            gearbox = new GearBox(gearPwm);
        }

        /// <summary>
        /// Changes gear
        /// </summary>
        /// <param name="gear">String contaiinng int parsable gear</param>
        public void SetGear(string gear)
        {
            gearbox.SetGear(int.Parse(gear));
        }

        /// <summary>
        /// Changes gear
        /// </summary>
        /// <param name="gear">Gear number</param>
        public void SetGear(int gear)
        {
            gearbox.SetGear(gear);
        }

        /// <summary>
        /// Changes speed
        /// </summary>
        /// <param name="speed">String contaiinng int parsable speed</param>
        public void SetLinearSpeed(string speed)
        {
            SetLinearSpeed(int.Parse(speed));
        }

        /// <summary>
        /// Changes speed
        /// </summary>
        /// <param name="speed">Speed value</param>
        public void SetLinearSpeed(int speed)
        {
#if DEBUG
            Console.WriteLine($"setting speed: {speed}"); 
#endif
            leftMotor.SetSpeed(speed);
            rightMotor.SetSpeed(speed);
        }


        /// <summary>
        /// Changes turn
        /// </summary>
        /// <param name="turn">String contaiinng int parsable turn rate</param>
        public void SetTurn(string turn)
        {
            SetTurn(int.Parse(turn));
        }

        /// <summary>
        /// Changes turn
        /// </summary>
        /// <param name="turn">Turn rate</param>
        public void SetTurn(int turn)
        {
            leftMotor.SetTurn(turn);
            rightMotor.SetTurn(-turn);
        }

        /// <summary>
        /// Stop motor ASAP
        /// </summary>
        /// <param name="empty">Empty, just for being compatible with MethodsQueue required signature</param>
        public void Break(string empty)
        {
            Break();
        }

        /// <summary>
        /// Stops motors ASAP
        /// </summary>
        public void Break()
        {
            leftMotor.Stop();
            rightMotor.Stop();
        }
    }
}