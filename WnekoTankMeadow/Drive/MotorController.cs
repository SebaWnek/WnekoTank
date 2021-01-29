using Meadow.Hardware;
using System;
using System.Threading;
using WnekoTankMeadow.Drive;
using Meadow.Foundation.Controllers.Pid;

namespace WnekoTankMeadow
{
    /// <summary>
    /// Class responsible for all movement operations, connecting all driving subsystems
    /// </summary>
    class MotorController
    {
        Motor leftMotor;
        Motor rightMotor;
        GearBox gearbox;
        HallEffectCounter rightCounter;
        HallEffectCounter leftCounter;
        PositionSensor positionSensor;
        int teethCount = 14;
        float chainPitch = 12.7f; //08b chain, half inch pitch, in mm
        float circumference;
        AutoResetEvent moveForwardResetEvent;
        AutoResetEvent turnResetEvent;
        CancellationTokenSource turnTokenSource;
        IdealPidController turnPid;

        /// <summary>
        /// Main constructor
        /// </summary>
        /// <param name="leftForwardPwm">PWM port responsible for left motor moving forward</param>
        /// <param name="leftBackPwm">PWM port responsible for left motor moving backward</param>
        /// <param name="rightForwardPwm">PWM port responsible for right motor moving forward</param>
        /// <param name="rightBackPwm">PWM port responsible for right motor moving backward</param>
        /// <param name="gearPwm">PWM port responsible for controling gear changing servo</param>
        public MotorController(IPwmPort rightForwardPwm,
                               IPwmPort rightBackPwm,
                               IPwmPort leftForwardPwm,
                               IPwmPort leftBackPwm,
                               IPwmPort gearPwm,
                               IDigitalInputPort leftCounterPort,
                               IDigitalInputPort righCounterPort,
                               PositionSensor posSens)
        {
            positionSensor = posSens;
            circumference = teethCount * chainPitch;
            leftMotor = new Motor(leftForwardPwm, leftBackPwm);
            rightMotor = new Motor(rightForwardPwm, rightBackPwm);
            gearbox = new GearBox(gearPwm);
            rightCounter = new HallEffectCounter(righCounterPort);
            leftCounter = new HallEffectCounter(leftCounterPort);
            rightCounter.Name = "Right";
            leftCounter.Name = "Left";
#if DEBUG
            rightCounter.RegisterForCount(CountChanged);
            leftCounter.RegisterForCount(CountChanged);
#endif

            rightCounter.RegisterForLimitReached(MoveForwardStop);
            leftCounter.RegisterForLimitReached(MoveForwardStop);

            positionSensor.RegisterForHeadingChanged(HeadingChanged);

            moveForwardResetEvent = new AutoResetEvent(false);
            turnResetEvent = new AutoResetEvent(false);
            turnTokenSource = new CancellationTokenSource();

            turnPid = new IdealPidController();
            turnPid.ProportionalComponent = 1.8f;
            turnPid.IntegralComponent = 0.05f;
            turnPid.DerivativeComponent = 0.01f;
            turnPid.OutputMax = 50;
            turnPid.OutputMin = -50;
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
            turnTokenSource.Cancel();
            turnTokenSource = new CancellationTokenSource();
            turnResetEvent.Set();
            moveForwardResetEvent.Set();
            leftMotor.Stop();
            rightMotor.Stop();
        }

        /// <summary>
        /// Stops motors gently
        /// </summary>
        /// <param name="empty">Empty, just for being compatible with MethodsQueue required signature</param>
        public void SoftBreak(string empty)
        {
            SoftBreak();
        }

        /// <summary>
        /// Stops motors gently
        /// </summary>
        public void SoftBreak()
        {
            leftMotor.SoftStop();
            rightMotor.SoftStop();
        }

        /// <summary>
        /// Move in straight line selected distance (at selected gear if present). Doesn't count turns, uses first sensor to reach limit
        /// </summary>
        /// <param name="args">String with arguments. Separated by ";", 2 args - without gear, 3 args - with gear</param>
        public void MoveForwardBy(string args)
        {
            string[] arguments = args.Split(';');
#if DEBUG
            foreach (string arg in arguments)
            {
                Console.WriteLine(arg);
            }
#endif
            MoveForwardBy(int.Parse(arguments[0]), float.Parse(arguments[1]), byte.Parse(arguments[2]), byte.Parse(arguments[3]));
        }

        /// <summary>
        /// Move in straight line selected distance. Doesn't count turns, uses first sensor to reach limit
        /// </summary>
        /// <param name="speed">Speed to travel at</param>
        /// <param name="distance">Distance to travel</param>
        public void MoveForwardBy(int speed, float distance, byte shouldBreak, byte gear)
        {
            if (gear > 0) SetGear(gear);
            int turns = (int)Math.Round(distance * 1000 / circumference);
            rightCounter.SetTarget(turns);
            leftCounter.SetTarget(turns);
            SetLinearSpeed(speed);
            moveForwardResetEvent.WaitOne();
            if (shouldBreak == 1) Break();
        }


        /// <summary>
        /// Event handler for stopping
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MoveForwardStop(object sender, EventArgs e)
        {
            moveForwardResetEvent.Set();
            (sender as HallEffectCounter).DisableTarget();
        }

        /// <summary>
        /// To see movement
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="count">Rotation count</param>
        private void CountChanged(object sender, int count)
        {
            Console.WriteLine((sender as HallEffectCounter).Name + ": " + count);
        }

        public void TurnBy(string args)
        {
            string[] arguments = args.Split(';');
#if DEBUG
            foreach (string arg in arguments)
            {
                Console.WriteLine(arg);
            }
#endif
            TurnByPid(int.Parse(arguments[0]), int.Parse(arguments[1]), byte.Parse(arguments[2]));
        }

        public void TurnBy(int d, int tr, byte gear)
        {
            if (gear > 0) SetGear(gear);
            int degrees = d;
            int turnRate = tr;
            int direction = Math.Sign(degrees);
            SetTurn(direction * turnRate);
            positionSensor.StartCheckingAngle(degrees, turnTokenSource.Token);
            turnResetEvent.WaitOne();
            SetTurn(0);
            turnTokenSource = new CancellationTokenSource();
        }

        public void TurnByPid(int d, int tr, byte gear)
        {
            CancellationToken token = turnTokenSource.Token;
            int turnTimeDelta = 101 - 2* tr; //So when it turn at max turn rate of 50 it has max frequency and with slower turn rates it's not that needed
            int angle = d;
            int direction = Math.Sign(angle);
            float previousHeading = positionSensor.Read()[0];
            float currentHeading = positionSensor.Read()[0];
            float turned = 0;
            float deltaAngle = 0;
            bool done = false;
            float turnRate = tr;
            turnPid.OutputMax = tr;
            turnPid.OutputMin = -tr;
            turnPid.TargetInput = d;
            turnPid.OutputTuningInformation = true;
            SetTurn((int)(direction * turnRate));
            Console.WriteLine($"D: {deltaAngle}, t: {turned}, dir: {currentHeading}, tr: {turnRate}\r\n");
            turnPid.CalculateControlOutput(); //to reset last update time
            turnPid.ResetIntegrator();
            while (!done)
            {
                if (token.IsCancellationRequested) break;
                direction = Math.Sign(turnRate);
                currentHeading = positionSensor.Read()[0];
                deltaAngle = currentHeading - previousHeading;
                if (Math.Abs(deltaAngle) > 180) //If angle is ok we only check once, if it's abnormal then we can spend more time as this will happen rarely 
                {
                    if (deltaAngle > 180) deltaAngle -= 360;
                    if (deltaAngle < 180) deltaAngle += 360;
                }
                turned += deltaAngle;
#if DEBUG
#endif
                turnPid.ActualInput = turned;
                turnRate = turnPid.CalculateControlOutput();
                Console.WriteLine($"D: {deltaAngle}, t: {turned}, dir: {currentHeading}, tr: {turnRate}\r\n");
                if (Math.Abs(turnRate) < 0.2) break;
                if (Math.Abs(turnRate) < 10) turnRate = Math.Sign(turnRate) * 10;
                SetTurn((int)Math.Round(turnRate));
                previousHeading = currentHeading;
                Thread.Sleep(turnTimeDelta);
            }
            Break();
        }

        private void HeadingChanged(object sender, EventArgs e)
        {
            turnResetEvent.Set();
        }
    }
}