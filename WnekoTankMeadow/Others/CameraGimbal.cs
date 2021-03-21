using Meadow.Foundation.Servos;
using Meadow.Hardware;
using System;
using System.Collections.Generic;
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
        int stabilizeDeltaTime = 100;

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
            if (args[0] == '1') StartStabilizing();
            else StopStabilizing();
        }

        public void StartStabilizing()
        {
#if DEBUG
            Console.WriteLine("Starting stabilization of camera");
#endif
            float[] reading = sensor.Read();
            float pitch = reading[2];
            float roll = reading[1];
            CancellationToken token = source.Token;
            float tmpAngle = Math.Abs(horizontalAngle);
            float meanAngle = roll * (tmpAngle / 90) + pitch * (1 - tmpAngle / 90);
            vertical.RotateTo(verticalAngle + (int)Math.Round(meanAngle) + 100);
#if DEBUG
            Console.WriteLine($"Angle: {pitch}, {roll}, stabilizing to: {verticalAngle + (int)Math.Round(meanAngle) + 100}");
#endif
            Thread worker = new Thread(() =>
            {
                while (true)
                {
                    if (token.IsCancellationRequested) break;
                    reading = sensor.Read();
                    pitch = reading[2];
                    roll = reading[1];
                    tmpAngle = Math.Abs(horizontalAngle);
                    meanAngle = roll * (tmpAngle / 90) + pitch * (1 - (tmpAngle / 90));
#if DEBUG
                    Console.WriteLine($"Angle: {pitch}, {roll}, {meanAngle}, {horizontalAngle}, stabilizing to: {verticalAngle + (int)Math.Round(meanAngle) + 100}");
#endif
                    vertical.RotateTo(verticalAngle + (int)Math.Round(meanAngle) + 100);
                    Thread.Sleep(stabilizeDeltaTime);
                }
            });
            worker.Start();
        }

        public void StopStabilizing()
        {
            source.Cancel();
            source.Dispose();
            source = new CancellationTokenSource();
        }
    }
}
