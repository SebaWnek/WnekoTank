using Meadow.Foundation.Servos;
using Meadow.Hardware;
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
    class CameraGimbal
    {
        Servo horizontal, vertical;
        ServoConfig horizontalCoinfig;
        ServoConfig verticalConfig;
        IPwmPort horizontalPort;
        IPwmPort verticalPort;
        IPositionSensor sensor;
        CancellationTokenSource source;
        int verticalAngle = 0;
        int horizontalAngle = 0;
        int stabilizeDeltaTime = 20;
        bool isStabilizing = false;
        bool horizontalStabilization = false;
        int minAngle = -100, maxAngle = 100;
        int minServoAngle = 0, maxServoAngle = 200;
        int angleDelta;
        int fullCircle = 360, quarterCircle;
#if DEBUG
        Stopwatch stopwatch;
#endif

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
            Test();
#if DEBUG
            stopwatch = new Stopwatch();
#endif
        }

        private void Test()
        {
#if DEBUG
            Console.WriteLine("Testing gimbal");
#endif
            SetAngle(minAngle, minAngle);
            Thread.Sleep(2000);
            SetAngle(maxAngle, maxAngle);
            Thread.Sleep(2000);
            SetAngle((maxAngle + minAngle) / 2, (maxAngle + minAngle) / 2);
            Thread.Sleep(2000);
        }

        public void SetAngle(int verAngle, int horAngle)
        {
            verticalAngle = verAngle;
            horizontalAngle = horAngle;
            vertical.RotateTo(verticalAngle + angleDelta);
            horizontal.RotateTo(angleDelta - horizontalAngle);
        }

        public void SetAngle(string args)
        {
            string[] arguments = args.Split(';');
            int vertical = int.Parse(arguments[0]);
            int horizontal = int.Parse(arguments[1]);
            SetAngle(vertical, horizontal);
        }

        internal void ChangeAngleBy(string args)
        {
            string[] arguments = args.Split(';');
            int vertical = int.Parse(arguments[0]);
            int horizontal = int.Parse(arguments[1]);
            ChangeAngleBy(vertical, horizontal);
        }

        private void ChangeAngleBy(int verticalChange, int horizontalChange)
        {
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

        public void StartStabilizing(string args)
        {
#if DEBUG
            Console.WriteLine(args);
#endif
            horizontalStabilization = args[2] == '1' ? true : false;
            if (args[0] == '1' && !isStabilizing) StartStabilizing();
            else if (args[0] == '0' && isStabilizing) StopStabilizing();
        }

        public void StartStabilizing()
        {
            isStabilizing = true;
#if DEBUG
            Console.WriteLine("Starting stabilization of camera");
#endif
            float[] reading = sensor.Read();
            float heading = reading[0];
            float pitch = reading[2];
            float roll = reading[1];
            CancellationToken token = source.Token;
            float tmpAngle = Math.Abs(horizontalAngle);
            float meanAngle = roll * (tmpAngle / quarterCircle) + pitch * (1 - tmpAngle / quarterCircle);
            int sign;
            float cameraHeading = heading + horizontalAngle, previousHeading = heading;
            //cameraHeading = cameraHeading < 0 ? cameraHeading + 360 : cameraHeading > 360 ? cameraHeading - 360 : cameraHeading;
            int verticalTarget, horizontalTarget = 0, currentHorizontalAngle, previousAngle = horizontalAngle;
            vertical.RotateTo(verticalAngle + (int)Math.Round(meanAngle) + angleDelta);
#if DEBUG
            Console.WriteLine($"Angle: {pitch}, {roll}, stabilizing to: {verticalAngle + (int)Math.Round(meanAngle) + angleDelta}");
#endif
            Thread worker = new Thread(() =>
            {
                while (true)
                {
#if DEBUG
                    stopwatch.Restart();
#endif
                    if (token.IsCancellationRequested) break;
                    reading = sensor.Read();
                    pitch = reading[2];
                    roll = reading[1];

                    if (horizontalStabilization)
                    {
                        heading = reading[0];
                        if (heading - previousHeading < minAngle) cameraHeading -= fullCircle;
                        if (heading - previousHeading > maxAngle) cameraHeading += fullCircle;
                        if (previousAngle != horizontalAngle)
                        {
                            cameraHeading = heading + horizontalAngle;
                            //cameraHeading = cameraHeading < 0 ? cameraHeading + 360 : cameraHeading > 360 ? cameraHeading - 360 : cameraHeading;
                        }

                        currentHorizontalAngle = (int)(heading - cameraHeading);
                        currentHorizontalAngle = currentHorizontalAngle < minAngle ? minAngle : currentHorizontalAngle > maxAngle ? maxAngle : currentHorizontalAngle;
                        horizontalTarget = currentHorizontalAngle + angleDelta;
                        horizontal.RotateTo(horizontalTarget);

                        previousAngle = horizontalAngle;
                        previousHeading = heading;
                    }
                    else currentHorizontalAngle = horizontalAngle;

                    tmpAngle = Math.Abs(currentHorizontalAngle);
                    sign = Math.Sign(currentHorizontalAngle);
                    meanAngle = sign * roll * (tmpAngle / quarterCircle) + pitch * (1 - (tmpAngle / quarterCircle));
                    verticalTarget = verticalAngle + (int)Math.Round(meanAngle) + angleDelta;
                    verticalTarget = verticalTarget < minServoAngle ? minServoAngle : verticalTarget > maxServoAngle ? maxServoAngle : verticalTarget;
                    vertical.RotateTo(verticalTarget);
#if DEBUG
                    stopwatch.Stop();
                    Console.WriteLine($"Angle: {heading}, {pitch}, {roll}, {meanAngle}, stabilizing to: {verticalTarget}, {horizontalTarget}\nTime: {stopwatch.ElapsedMilliseconds}ms");
#endif
                    Thread.Sleep(stabilizeDeltaTime);
                }
            });
            worker.Priority = ThreadPriority.Lowest;
            worker.Start();
        }

        public void StopStabilizing()
        {
            isStabilizing = false;
            source.Cancel();
            source.Dispose();
            source = new CancellationTokenSource();
        }
    }
}
