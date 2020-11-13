using Meadow.Hardware;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WnekoTankMeadow
{
    class Motor
    {
        int baseSpeed = 0;
        int turnModifier = 0;
        int currentSpeed = 0;

        private IPwmPort forwardPwm;
        private IPwmPort backPwm;
        private IPwmPort _currentPwmPort;
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

        public Motor(IPwmPort forward, IPwmPort back)
        {
            forwardPwm = forward;
            backPwm = back;
            _currentPwmPort = forward;
        }

        public void SetSpeed(int speed)
        {
            turnModifier = baseSpeed * speed >= 0 ? turnModifier : -turnModifier;
            baseSpeed = speed;
            ChangeSpeed();
        }

        private void ChangeSpeed()
        {
            int newSpeed = baseSpeed + turnModifier;

            if (newSpeed >= 0) CurrentPort = forwardPwm;
            else CurrentPort = backPwm;
            currentSpeed = newSpeed;
            newSpeed = Math.Abs(newSpeed);
            newSpeed = newSpeed > 100 ? 100 : newSpeed;
            CurrentPort.DutyCycle = newSpeed / 100.01f;
        }

        public void SetTurn(int turnRate)
        {
            turnModifier = baseSpeed >= 0 ? turnRate : -turnRate;
            ChangeSpeed();
        }
    }
}
