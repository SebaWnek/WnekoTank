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
        int stabilizeDeltaTime = 25;
        bool isStabilizing = false;
        bool horizontalStabilization = false;
#if DEBUG
        Stopwatch stopwatch;
#endif

        public CameraGimbal(IPwmPort ver, IPwmPort hor, IPositionSensor s)
        {
            horizontalPort = hor;
            verticalPort = ver;
            horizontalCoinfig = new ServoConfig(0, 200, 370, 2700, 50);
            verticalConfig = new ServoConfig(0, 200, 370, 2700, 50);
            horizontal = new Servo(horizontalPort, horizontalCoinfig);
            vertical = new Servo(verticalPort, verticalConfig);
            sensor = s;
            source = new CancellationTokenSource();
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
            SetAngle(-100, -100);
            Thread.Sleep(2000);
            SetAngle(100, 100);
            Thread.Sleep(2000);
            SetAngle(0, 0);
            Thread.Sleep(2000);
        }

        public void SetAngle(int verticalAngle, int horizontalAngle)
        {
            vertical.RotateTo(verticalAngle + 100);
            horizontal.RotateTo(100 - horizontalAngle);
            this.verticalAngle = verticalAngle;
            this.horizontalAngle = horizontalAngle;
        }

        public void SetAngle(string args)
        {
            string[] arguments = args.Split(';');
            int vertival = int.Parse(arguments[0]);
            int horizontal = int.Parse(arguments[1]);
            SetAngle(vertival, horizontal);
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
            float meanAngle = roll * (tmpAngle / 90) + pitch * (1 - tmpAngle / 90);
            int sign;
            float cameraHeading = heading + horizontalAngle, previousHeading = heading;
            //cameraHeading = cameraHeading < 0 ? cameraHeading + 360 : cameraHeading > 360 ? cameraHeading - 360 : cameraHeading;
            int verticalTarget, horizontalTarget = 0, currentHorizontalAngle, previousAngle = horizontalAngle;
            vertical.RotateTo(verticalAngle + (int)Math.Round(meanAngle) + 100);
#if DEBUG
            Console.WriteLine($"Angle: {pitch}, {roll}, stabilizing to: {verticalAngle + (int)Math.Round(meanAngle) + 100}");
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
                        if (heading - previousHeading < -180) cameraHeading -= 360;
                        if (heading - previousHeading > 180) cameraHeading += 360;
                        if (previousAngle != horizontalAngle)
                        {
                            cameraHeading = heading + horizontalAngle;
                            //cameraHeading = cameraHeading < 0 ? cameraHeading + 360 : cameraHeading > 360 ? cameraHeading - 360 : cameraHeading;
                        }

                        currentHorizontalAngle = (int)(heading - cameraHeading);
                        currentHorizontalAngle = currentHorizontalAngle < -100 ? -100 : currentHorizontalAngle > 100 ? 100 : currentHorizontalAngle;
                        horizontalTarget = currentHorizontalAngle + 100;
                        horizontal.RotateTo(horizontalTarget);

                        previousAngle = horizontalAngle;
                        previousHeading = heading;
                    }
                    else currentHorizontalAngle = horizontalAngle;

                    tmpAngle = Math.Abs(currentHorizontalAngle);
                    sign = Math.Sign(currentHorizontalAngle);
                    meanAngle = sign * roll * (tmpAngle / 90) + pitch * (1 - (tmpAngle / 90));
                    verticalTarget = verticalAngle + (int)Math.Round(meanAngle) + 100;
                    verticalTarget = verticalTarget < 0 ? 0 : verticalTarget > 200 ? 200 : verticalTarget;
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
