using Meadow.Foundation.Servos;
using Meadow.Hardware;
using Meadow.Units;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WnekoTankMeadow.Drive;

namespace WnekoTankMeadow.Others
{
    /// <summary>
    /// Camera gimbal, using two PWM controlled servos
    /// </summary>
    class CameraGimbal
    {
        Servo horizontal, vertical;
        ServoConfig horizontalCoinfig;
        ServoConfig verticalConfig;
        IPwmPort horizontalPort;
        IPwmPort verticalPort;
        IPositionSensor sensor;
        CancellationTokenSource source;
        Angle verticalAngle = new Angle(0);
        Angle horizontalAngle = new Angle(0);
        int stabilizeDeltaTime = 20;
        bool isStabilizing = false;
        bool horizontalStabilization = false;
        Angle minAngle = new Angle(-100), maxAngle = new Angle(100);
        Angle minServoAngle = new Angle(0), maxServoAngle = new Angle(200);
        Angle angleDelta;
        int fullCircle = 360, quarterCircle;
        Angle lastVerticalChange, lastHorizontalChange;
        private object locker;
#if DEBUG
        Stopwatch stopwatch;
#endif

        /// <summary>
        /// Basic constructor
        /// </summary>
        /// <param name="ver">Vertical movement servo PWM port</param>
        /// <param name="hor">Horizontal movement servo PWM port</param>
        /// <param name="s">Inertial Measurement Unit to check current device position</param>
        public CameraGimbal(IPwmPort ver, IPwmPort hor, IPositionSensor s)
        {
            horizontalPort = hor;
            verticalPort = ver;
            horizontalCoinfig = new ServoConfig(minServoAngle, maxServoAngle, 370, 2700, 50);
            verticalConfig = new ServoConfig(minServoAngle, maxServoAngle, 370, 2700, 50);
            horizontal = new Servo(horizontalPort, horizontalCoinfig);
            vertical = new Servo(verticalPort, verticalConfig);
            sensor = s;
            source = new CancellationTokenSource();
            angleDelta = maxServoAngle - maxAngle;
            quarterCircle = fullCircle / 4;
            locker = new object();
            Test();
#if DEBUG
            stopwatch = new Stopwatch();
#endif
        }

        /// <summary>
        /// Moves gimbal to both min and max axis positions to check if operating properly
        /// </summary>
        private void Test()
        {
#if DEBUG
            Console.WriteLine("Testing gimbal");
#endif
            SetAngle(minAngle, minAngle);
            Thread.Sleep(2000);
            SetAngle(maxAngle, maxAngle);
            Thread.Sleep(2000);
            SetAngle(new Angle((maxAngle.Degrees + minAngle.Degrees) / 2), new Angle( (maxAngle.Degrees + minAngle.Degrees) / 2));
            Thread.Sleep(2000);
        }

        /// <summary>
        /// Turns camera to selected angle
        /// </summary>
        /// <param name="verAngle">Pitch</param>
        /// <param name="horAngle">Yaw</param>
        public void SetAngle(Angle verAngle, Angle horAngle)
        {
            lock (locker)
            {
                verticalAngle = verAngle;
                horizontalAngle = horAngle; 
            }
            vertical.RotateTo(verticalAngle + angleDelta);
            horizontal.RotateTo(angleDelta - horizontalAngle);
        }

        /// <summary>
        /// Turns camera to selected angle from data in string
        /// </summary>
        /// <param name="args">Angles separated by semicolon</param>
        public void SetAngle(string args)
        {
            string[] arguments = args.Split(';');
            Angle vertical = new Angle(int.Parse(arguments[0]));
            Angle horizontal = new Angle(int.Parse(arguments[1]));
            SetAngle(vertical, horizontal);
        }

        /// <summary>
        /// Change camera direction by specified angles, to be used by control app
        /// </summary>
        /// <param name="args">Pitch and yaw change, separated by semicolon</param>
        internal void ChangeAngleBy(string args)
        {
            string[] arguments = args.Split(';');
            int vertical = int.Parse(arguments[0]);
            int horizontal = int.Parse(arguments[1]);
#if DEBUG
            Console.WriteLine("Gimbal: " + vertical + " " + horizontal);
#endif
            ChangeAngleBy(new Angle(vertical), new Angle(horizontal));
        }

        /// <summary>
        /// Change camera direction by specified angles
        /// If camera is stabilizing then changes values used by stabilization and changes common values used by it for easier data delivery to stabilization thread
        /// </summary>
        /// <param name="verticalChange"></param>
        /// <param name="horizontalChange"></param>
        private void ChangeAngleBy(Angle verticalChange, Angle horizontalChange)
        {
            lock (locker)
            {
                lastHorizontalChange = horizontalChange;
                lastVerticalChange = verticalChange;
            }
            verticalAngle += verticalChange;
            horizontalAngle += horizontalChange;
            verticalAngle = verticalAngle < minAngle ? minAngle : verticalAngle > maxAngle ? maxAngle : verticalAngle;
            horizontalAngle = horizontalAngle < minAngle ? minAngle : horizontalAngle > maxAngle ? maxAngle : horizontalAngle;
            vertical.RotateTo(verticalAngle + angleDelta);
            horizontal.RotateTo(angleDelta - horizontalAngle);
#if DEBUG
            Console.WriteLine($"{horizontalAngle}, {verticalAngle}");
#endif
        }

        /// <summary>
        /// Start stabilizing, to be used by controll app
        /// </summary>
        /// <param name="args">Stabilization parameters, should be stabilized also horizontaly and if should start or stop, separated by semicolon</param>
        public void SetStabilization(string args)
        {
#if DEBUG
            Console.WriteLine(args);
#endif
            horizontalStabilization = args[2] == '1' ? true : false;
            if (args[0] == '1' && !isStabilizing) StartStabilizing();
            else if (args[0] == '0' && isStabilizing) StopStabilizing();
        }

        /// <summary>
        /// Stabilize camera
        /// Uses position sensor to determine start and current device euler angles, then from their difference calculates how to turn camera to counterat device movement
        /// Runs on separate thread to allow operation of other parts of device
        /// </summary>
        public void StartStabilizing()
        {
            isStabilizing = true;
#if DEBUG
            Console.WriteLine("Starting stabilization of camera");
#endif
            float[] reading = sensor.Read(); //Get current device position
            float heading = reading[0];
            float pitch = reading[2];
            float roll = reading[1];
            CancellationToken token = source.Token;
            float tmpAngle = (float)Math.Abs(horizontalAngle.Degrees); //Absolute value of camera yaw in relation to device to determine proportions between roll and pitch to use
            float meanAngle = roll * (tmpAngle / quarterCircle) + pitch * (1 - tmpAngle / quarterCircle); //Weighted proportional angle of roll and pitch using yaw as weiht
            int sign;
            float cameraHeading = heading + (float)horizontalAngle.Degrees, previousHeading = heading; //Absolute camera yaw - device heading + yaw in relation to device
            int verticalTarget, horizontalTarget = 0, currentHorizontalAngle, previousAngle = (int)horizontalAngle.Degrees;
            //vertical.RotateTo(verticalAngle + (int)Math.Round(meanAngle) + angleDelta);
            //#if DEBUG
            //            Console.WriteLine($"Angle: {pitch}, {roll}, stabilizing to: {verticalAngle + (int)Math.Round(meanAngle) + angleDelta}");
            //#endif
            Thread worker = new Thread(() =>
            {
                while (true)
                {
#if DEBUG
                    stopwatch.Restart(); //Used to measure performance in Debug mode when connected to PC
#endif
                    if (token.IsCancellationRequested) break;
                    reading = sensor.Read(); //Get current device position
                    pitch = reading[2];
                    roll = reading[1];

                    //Can be stabilizing only vertically, then this part is ommited
                    if (horizontalStabilization)
                    {
                        heading = reading[0];
                        if (heading - previousHeading < minAngle.Degrees) cameraHeading -= fullCircle; //Corrects heading if went over north - as sudden change of value from 360 to 0, or otherwise, would cause troubles with calculations
                        if (heading - previousHeading > maxAngle.Degrees) cameraHeading += fullCircle; //It doesn't matter that value can get >360deg, it's used only internally here
                        if (previousAngle != horizontalAngle.Degrees) //Check if new heading requested by controll app
                        {
                            lock (locker) //So we don't have value updated at the same time, as read 
                            {
                                cameraHeading += (float)lastHorizontalChange.Degrees; //Turn camera to new heading by last angle delta by control app
                            }
                        }

                        currentHorizontalAngle = (int)(heading - cameraHeading); //Camera yaw in relation to device
                        currentHorizontalAngle = currentHorizontalAngle < minAngle.Degrees ? (int)minAngle.Degrees : currentHorizontalAngle > maxAngle.Degrees ? (int)maxAngle.Degrees : currentHorizontalAngle; //Clip values if requested angle is beyond limits
                        horizontalTarget = currentHorizontalAngle + (int)angleDelta.Degrees; //Calculate servo position, as it is shifted from physical position
                        horizontal.RotateTo(new Angle(horizontalTarget)); //Rotate servo

                        previousAngle = (int)horizontalAngle.Degrees;
                        previousHeading = heading;
                    }
                    else currentHorizontalAngle = (int)horizontalAngle.Degrees;

                    tmpAngle = Math.Abs(currentHorizontalAngle); //Absolute value of camera yaw in relation to device to determine proportions between roll and pitch to use
                    sign = Math.Sign(currentHorizontalAngle);
                    meanAngle = sign * roll * (tmpAngle / quarterCircle) + pitch * (1 - (tmpAngle / quarterCircle));  //Inclination in camera direction as weighted proportional angle of roll and pitch using yaw as weiht
                    verticalTarget = (int)verticalAngle.Degrees + (int)Math.Round(meanAngle) + (int)angleDelta.Degrees; //Change target angle by the value of device inclination
                    verticalTarget = verticalTarget < minServoAngle.Degrees ? (int)minServoAngle.Degrees : verticalTarget > maxServoAngle.Degrees ? (int)maxServoAngle.Degrees : verticalTarget; //Clip target to limits
                    vertical.RotateTo(new Angle(verticalTarget)); //Rotate servo
#if DEBUG
                    stopwatch.Stop();
                    Console.WriteLine($"Angle: {heading}, {pitch}, {roll}, {meanAngle}, stabilizing to: {verticalTarget}, {horizontalTarget}\nTime: {stopwatch.ElapsedMilliseconds}ms");
#endif
                    Thread.Sleep(stabilizeDeltaTime); //Wait preselected time for another iteration
                }
            });
            worker.Priority = ThreadPriority.Lowest; //Camera doesn't need to be perectly stable, other things are more important
            worker.Start();
        }

        /// <summary>
        /// Stops camera position stabilization
        /// </summary>
        public void StopStabilizing()
        {
            isStabilizing = false;
            source.Cancel();
            source.Dispose();
            source = new CancellationTokenSource();
        }

        /// <summary>
        /// Current gimbal angles - horizontal, vertical
        /// </summary>
        /// <returns>Angle values</returns>
        public int[] GetCurrentPosition()
        {
            return new int[]
             {
                (int)((Angle)(horizontal.Angle - angleDelta)).Degrees,
                (int)((Angle)(vertical.Angle - angleDelta)).Degrees
             };
        }
    }
}
