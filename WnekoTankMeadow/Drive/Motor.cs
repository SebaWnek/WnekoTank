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
    /// Represents one DC motor controlled by PWM
    /// </summary>
    class Motor
    {
        int softSleepTime = 50;
        int deltaV = 10;

        int baseSpeed = 0;
        int turnModifier = 0;
        int currentSpeed = 0;

        private IPwmPort forwardPwm;
        private IPwmPort backPwm;
        private IPwmPort _currentPwmPort;
        /// <summary>
        /// Using property asures that all the time there is only one PWM signal, 
        /// as all methods use this property and there could be only one port assigned,
        /// and also that when changing direction there is short period without signal on both ports, 
        /// as motor controllers H-bridges could be damaged when receiving signal on both PWM ports 
        /// </summary>
        private IPwmPort CurrentPort
        {
            get { return _currentPwmPort; }
            set
            {
                _currentPwmPort.DutyCycle = 0;
                Thread.Sleep(50);
                _currentPwmPort = value;
            }
        }

        /// <summary>
        /// Main constructor
        /// </summary>
        /// <param name="forward">PWM port for forward signal</param>
        /// <param name="back">PWM port for backward signal</param>
        public Motor(IPwmPort forward, IPwmPort back)
        {
            forwardPwm = forward;
            backPwm = back;
            _currentPwmPort = forward;
        }

        /// <summary>
        /// Sets correct motor speed.
        /// Checks if linear speed has the same sign as turnmodifier,
        /// To determine if turnmodifier should be added or substracted 
        /// </summary>
        /// <param name="speed">Sets motor speed</param>
        public void SetSpeed(int speed)
        {
            turnModifier = baseSpeed * speed >= 0 ? turnModifier : -turnModifier;
            baseSpeed = speed;
            ChangeSpeed();
        }

        /// <summary>
        /// Sets correct motor speed slowly
        /// Checks if linear speed has the same sign as turnmodifier,
        /// To determine if turnmodifier should be added or substracted 
        /// </summary>
        /// <param name="speed">Sets motor speed</param>
        public void SetSpeedSoft(int speed)
        {
            int tmpSpeed = baseSpeed;
            int deltaSpeed = speed - tmpSpeed;
            int sign = Math.Sign(deltaSpeed);
            int abs = Math.Abs(deltaSpeed);
            for(int i = 0; i < abs; i += deltaV)
            {
                tmpSpeed += deltaV * sign;
                if (i > abs) currentSpeed -= abs - currentSpeed;
                SetSpeed(tmpSpeed);
                Thread.Sleep(softSleepTime);
            }
        }

        /// <summary>
        /// Calculates new speed, determines if it needs to use forward or backward PWM port,
        /// Selectr correct one and sets correct duty cycle 
        /// </summary>
        private void ChangeSpeed()
        {
            int newSpeed = baseSpeed + turnModifier;

            if (newSpeed >= 0 && CurrentPort != forwardPwm) CurrentPort = forwardPwm;
            else if (newSpeed < 0 && CurrentPort != backPwm) CurrentPort = backPwm;
            currentSpeed = newSpeed;
            newSpeed = Math.Abs(newSpeed);
            newSpeed = newSpeed > 100 ? 100 : newSpeed;
            CurrentPort.DutyCycle = newSpeed / 100.01f;
        }

        /// <summary>
        /// Stop motor ASAP
        /// </summary>
        internal void Stop()
        {
            baseSpeed = 0;
            turnModifier = 0;
            currentSpeed = 0;
            CurrentPort.DutyCycle = 0;
        }

        /// <summary>
        /// Changes turnmodifier, checks if it's same sign as base speed and determines if it should be added or substracted
        /// </summary>
        /// <param name="turnRate"></param>
        public void SetTurn(int turnRate)
        {
            turnModifier = turnRate;
            ChangeSpeed();
        }

        /// <summary>
        /// Stops motor gently 
        /// </summary>
        internal void SoftStop()
        {
            int sign = Math.Sign(currentSpeed);
            int absSpeed = Math.Abs(currentSpeed);
            for (int i = absSpeed; i > 0; i -= 10)
            {
                currentSpeed = sign * i;
                Thread.Sleep(softSleepTime);
            }
            Stop();
        }
    }
}
