using Meadow.Hardware;
using System;
using System.Threading;
using WnekoTankMeadow.Drive;
using Meadow.Foundation.Controllers.Pid;
using WnekoTankMeadow.Others;

namespace WnekoTankMeadow
{
    /// <summary>
    /// Class responsible for all movement operations, connecting all driving subsystems
    /// </summary>
    class MotorController
    {
        private const int slowdownTurns = 2;
        Motor leftMotor;
        Motor rightMotor;
        GearBox gearbox;
        HallEffectCounter rightCounter;
        HallEffectCounter leftCounter;
        BNO055 positionSensor;
        CameraGimbal gimbal;
        int teethCount = 14;
        int magnetsCount = 1;
        float chainPitch = 12.7f; //08b chain, half inch pitch, in mm
        float circumference;
        float minSoftForwardDist = 2;
        int slowDownTicks = 2;
        AutoResetEvent moveForwardResetEventLeft;
        AutoResetEvent moveForwardResetEventRight;
        AutoResetEvent turnResetEvent;
        CancellationTokenSource turnTokenSource;
        CancellationTokenSource stabilizeTokenSource;
        IdealPidController turnPid;
        IdealPidController stabilizePid;
        private int slowSpeed = 10;
        private int turnTimeDeltaMax = 51;
        private int stabilizeTimeDelta = 200;
        private bool isStabilizing = false;
        private float stabilizeTargetDirection;
        private int stabilizeTurnRate = 20;
        private int defaultTurnRate = 50;
        private int defaultSpeed = 40;
        private float cameraHeigth = 0.40f; //m, from ground level
        private float cameraPosition = -0.25f; //m, from center 
        private int fullCircle = 360;
        private int quarterCircle, halfCircle;
        private byte defaultGear = 1;
        private Action clearQueue;

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
                               BNO055 posSens,
                               CameraGimbal gimb, 
                               Action clQueue)
        {
            quarterCircle = fullCircle / 4;
            halfCircle = fullCircle / 2;
            positionSensor = posSens;
            gimbal = gimb;
            clearQueue = clQueue;
            circumference = teethCount * chainPitch / magnetsCount;
            leftMotor = new Motor(leftForwardPwm, leftBackPwm);
            rightMotor = new Motor(rightForwardPwm, rightBackPwm);
            gearbox = new GearBox(gearPwm);
            rightCounter = new HallEffectCounter(righCounterPort);
            leftCounter = new HallEffectCounter(leftCounterPort);
            rightCounter.Name = Side.Right;
            leftCounter.Name = Side.Left;
#if DEBUG
            rightCounter.RegisterForCount(CountChanged);
            leftCounter.RegisterForCount(CountChanged);
#endif

            rightCounter.RegisterForLimitReached(MoveForwardStop);
            leftCounter.RegisterForLimitReached(MoveForwardStop);

            positionSensor.RegisterForHeadingChanged(HeadingChanged);

            moveForwardResetEventLeft = new AutoResetEvent(false);
            moveForwardResetEventRight = new AutoResetEvent(false);
            turnResetEvent = new AutoResetEvent(false);
            turnTokenSource = new CancellationTokenSource();

            turnPid = new IdealPidController();
            turnPid.ProportionalComponent = 1.8f;
            turnPid.IntegralComponent = 0.05f;
            turnPid.DerivativeComponent = 0.01f;
            turnPid.OutputMax = 50;
            turnPid.OutputMin = -50;

            stabilizePid = new IdealPidController();
            stabilizePid.ProportionalComponent = 1.8f;
            stabilizePid.IntegralComponent = 0.05f;
            stabilizePid.DerivativeComponent = 0.01f;
            stabilizePid.OutputMax = stabilizeTurnRate;
            stabilizePid.OutputMin = -stabilizeTurnRate;
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
        /// Changes speed slowly
        /// </summary>
        /// <param name="speed">Speed value</param>
        public void SetLinearSpeedSoft(int speed)
        {
#if DEBUG
            Console.WriteLine($"setting speed: {speed}");
#endif
            leftMotor.SetSpeedSoft(speed);
            rightMotor.SetSpeedSoft(speed);
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
            moveForwardResetEventLeft.Set();
            moveForwardResetEventRight.Set();
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
        /// <param name="args">String with arguments. Separated by ";"</param>
        public void MoveForwardBy(string args)
        {
            string[] arguments = args.Split(';');
#if DEBUG
            foreach (string arg in arguments)
            {
                Console.WriteLine(arg);
            }
#endif
            int speed = int.Parse(arguments[0]);
            float distance = float.Parse(arguments[1]);
            bool shouldBreak = arguments[2].StartsWith("1");
            byte gear = byte.Parse(arguments[3]);
            bool softBreak = arguments[4].StartsWith("1");
            if (arguments[4] == "1" && distance > minSoftForwardDist)
            {
                MoveForwardBySoft(speed, distance, shouldBreak, gear);
            }
            else
            {
                MoveForwardBy(speed, distance, shouldBreak, gear);
            }
        }

        /// <summary>
        /// Move in straight line selected distance. Doesn't count turns, uses first sensor to reach limit
        /// </summary>
        /// <param name="speed">Speed to travel at</param>
        /// <param name="distance">Distance to travel</param>
        public void MoveForwardBy(int speed, float distance, bool shouldBreak, byte gear)
        {
            if (gear > 0) SetGear(gear);
            byte direction = (byte)Math.Sign(distance);
            float absDist = Math.Abs(distance);
            int turns = (int)Math.Round(absDist * 1000 / circumference);
            rightCounter.SetTarget(turns);
            leftCounter.SetTarget(turns);
            SetLinearSpeed(direction > 0 ? speed : -1 * speed);
            moveForwardResetEventLeft.WaitOne();
            moveForwardResetEventRight.WaitOne();
            if (shouldBreak) Break();
            rightCounter.DisableTarget();
            leftCounter.DisableTarget();
        }

        /// <summary>
        /// Move in straight line selected distance. Doesn't count turns, uses first sensor to reach limit
        /// Uses slow speed change
        /// </summary>
        /// <param name="speed">Speed to travel at</param>
        /// <param name="distance">Distance to travel</param>
        public void MoveForwardBySoft(int speed, float distance, bool shouldBreak, byte gear)
        {
            if (gear > 0) SetGear(gear);
            byte direction = (byte)Math.Sign(distance);
            float absDist = Math.Abs(distance);
            int turns = (int)Math.Round(absDist * 1000 / circumference);
            rightCounter.SetTarget(turns - slowdownTurns);
            leftCounter.SetTarget(turns - slowdownTurns);
            SetLinearSpeed(direction > 0 ? speed : -1 * speed);
            moveForwardResetEventLeft.WaitOne();
            moveForwardResetEventRight.WaitOne();
            SetGear(1);
            rightCounter.SetTarget(slowDownTicks);
            leftCounter.SetTarget(slowDownTicks);
            SetLinearSpeedSoft(slowSpeed);
            moveForwardResetEventLeft.WaitOne();
            moveForwardResetEventRight.WaitOne();
            if (shouldBreak) Break();
            rightCounter.DisableTarget();
            leftCounter.DisableTarget();
        }

        /// <summary>
        /// Event handler for stopping
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MoveForwardStop(object sender, EventArgs e)
        {
            HallEffectCounter counter = sender as HallEffectCounter;
            if (counter.Name == Side.Left) moveForwardResetEventLeft.Set();
            else moveForwardResetEventRight.Set();
            counter.DisableTarget();
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

        internal void SetSpeedAndTurn(string obj)
        {
            string[] arguments = obj.Split(';');

            SetLinearSpeed(int.Parse(arguments[0]));
            SetTurn(int.Parse(arguments[1]));
        }

        /// <summary>
        /// Depreciated, replaced by TurnByPid
        /// </summary>
        /// <param name="d"></param>
        /// <param name="tr"></param>
        /// <param name="gear"></param>
        public void TurnBy(int d, int tr, byte gear)
        {
            if (gear > 0) SetGear(gear);
            int degrees = d;
            int turnRate = tr == 0 ? defaultTurnRate : tr;
            int direction = Math.Sign(degrees);
            SetTurn(direction * turnRate);
            positionSensor.StartCheckingAngle(degrees, turnTokenSource.Token);
            turnResetEvent.WaitOne();
            SetTurn(0);
            turnTokenSource = new CancellationTokenSource();
        }

        public void StabilizeDirection(string input)
        {
            if(input == "1" && isStabilizing == false)
            {
                StabilizeDirection();
            }
            else if(input == "0" && isStabilizing == true)
            {
                StopStabilizingDirection();
            }
        }

        public void StopStabilizingDirection()
        {
            stabilizeTokenSource.Cancel();
            stabilizeTokenSource = new CancellationTokenSource();
            isStabilizing = false;
        }

        public void StabilizeDirection()
        {
            isStabilizing = true;
            CancellationToken token = stabilizeTokenSource.Token;
            stabilizeTargetDirection = positionSensor.ReadHeading();
            float previousHeading = stabilizeTargetDirection;
            float currentHeading = stabilizeTargetDirection;
            float deltaAngle = 0;
            int direction;
            float turnRate;
            stabilizePid.CalculateControlOutput(); //to reset last update time
            stabilizePid.ResetIntegrator();
            Thread stabilizeThread = new Thread(() =>
            {
                while (true)
                {
                    if (token.IsCancellationRequested) break;
                    currentHeading = positionSensor.ReadHeading();
                    deltaAngle = currentHeading - previousHeading;
                    if (Math.Abs(deltaAngle) > 180) //If angle is ok we only check once, if it's abnormal then we can spend more time as this will happen rarely 
                    {
                        if (deltaAngle > 180) deltaAngle -= 360;
                        if (deltaAngle < 180) deltaAngle += 360;
                    }
                    direction = Math.Sign(deltaAngle);
                    turnPid.ActualInput = currentHeading;
                    turnRate = turnPid.CalculateControlOutput();
                    SetTurn((int)Math.Round(turnRate));
                    previousHeading = currentHeading;
                    Thread.Sleep(stabilizeTimeDelta);
                }
            });
            stabilizeThread.Start();
        }
        
        public void TurnByPid(int d, int tr, byte gear)
        {
            if (!isStabilizing)
            {
                CancellationToken token = turnTokenSource.Token;
                int turnTimeDelta = turnTimeDeltaMax - tr; //So when it turn at max turn rate of 50 it has max frequency and with slower turn rates it's not that needed
                int angle = d;
                int direction = Math.Sign(angle);
                float previousHeading = positionSensor.ReadHeading();
                float currentHeading = positionSensor.ReadHeading();
                float turned = 0;
                float deltaAngle = 0;
                bool done = false;
                float turnRate = tr == 0 ? defaultTurnRate : tr;
                turnPid.OutputMax = tr;
                turnPid.OutputMin = -tr;
                turnPid.TargetInput = d;
#if DEBUG
                turnPid.OutputTuningInformation = true;
                Console.WriteLine($"D: {deltaAngle}, t: {turned}, dir: {currentHeading}, tr: {turnRate}\r\n");
#endif
                SetTurn((int)(direction * turnRate));
                turnPid.CalculateControlOutput(); //to reset last update time
                turnPid.ResetIntegrator();
                while (!done)
                {
                    if (token.IsCancellationRequested) break;
                    //direction = Math.Sign(turnRate);      ??????
                    currentHeading = positionSensor.ReadHeading();
                    deltaAngle = currentHeading - previousHeading;
                    if (Math.Abs(deltaAngle) > 180) //If angle is ok we only check once, if it's abnormal then we can spend more time as this will happen rarely 
                    {
                        if (deltaAngle > 180) deltaAngle -= 360;
                        if (deltaAngle < 180) deltaAngle += 360;
                    }
                    turned += deltaAngle;
                    turnPid.ActualInput = turned;
                    turnRate = turnPid.CalculateControlOutput();
#if DEBUG
                    Console.WriteLine($"D: {deltaAngle}, t: {turned}, dir: {currentHeading}, tr: {turnRate}\r\n");
#endif
                    if (Math.Abs(turnRate) < 0.2) break;
                    if (Math.Abs(turnRate) < 10) turnRate = Math.Sign(turnRate) * 10;
                    SetTurn((int)Math.Round(turnRate));
                    previousHeading = currentHeading;
                    Thread.Sleep(turnTimeDelta);
                }
                Break(); 
            }
            else
            {
                stabilizePid.TargetInput += d;
            }
        }

        public void TurnToByCamera(string args)
        {
            clearQueue.Invoke();
            Break();
            int angle = int.Parse(args);
            TurnToByCamera(angle);
        }

        public void TurnToByCamera(int angle)
        {
            float heading = positionSensor.ReadHeading();
            int camAngle = gimbal.GetCurrentPosition()[0];
            float camHeading = heading + camAngle;
            float targetHeading = camHeading + angle;
            targetHeading = targetHeading < 0 ? targetHeading + fullCircle : targetHeading > fullCircle ? targetHeading - fullCircle : targetHeading;
            TurnTo(targetHeading);
        }

        private void TurnTo(float targetHeading)
        {
            float heading = positionSensor.ReadHeading();
            int headingDelta = (int)Math.Round(targetHeading - heading);
            headingDelta = headingDelta < -halfCircle ? headingDelta + fullCircle : headingDelta > halfCircle ? headingDelta - fullCircle : headingDelta;
#if DEBUG
            Console.WriteLine($"Turning to {targetHeading}, by {headingDelta}");
#endif
            TurnByPid(headingDelta, defaultTurnRate, defaultGear);
        }

        public void MoveToByAngles(string args)
        {
            clearQueue.Invoke();
            Break();
            string[] values = args.Split(';');
            int x = int.Parse(values[0]);
            int y = int.Parse(values[1]);
            MoveToByAngles(x, y);
        }

        private void MoveToByAngles(int horAngle, int verAngle)
        {
            float[] position = positionSensor.Read();
            int[] gimbalAngles = gimbal.GetCurrentPosition();
            TurnToByCamera(horAngle);

            int tmpAngle = Math.Abs(gimbalAngles[0]);
            int sign = Math.Sign(gimbalAngles[0]);
            float deviceAngle = sign * position[1] * (tmpAngle / quarterCircle) + position[2] * (1 - (tmpAngle / quarterCircle));

            float groundAngle = -1* verAngle + deviceAngle + gimbalAngles[1];
            float distance = cameraHeigth / (float)Math.Tan(groundAngle);
            //MoveForwardBy(defaultSpeed, distance, true, defaultGear);
#if DEBUG
            Console.WriteLine($"Moving by {distance}");
#endif
            MoveForwardBy(defaultSpeed, distance, true, defaultGear);
        }

        private void HeadingChanged(object sender, EventArgs e)
        {
            turnResetEvent.Set();
        }
    }
}