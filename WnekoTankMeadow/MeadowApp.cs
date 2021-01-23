using System;
using System.Text;
using System.Threading;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation;
using Meadow.Foundation.ICs.IOExpanders;
using Meadow.Foundation.Leds;
using Meadow.Foundation.Servos;
using Meadow.Hardware;

namespace WnekoTankMeadow
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        ITankCommunication com;
        RgbPwmLed onboardLed;
        II2cBus bus;
        MotorController motor;
        Pca9685 motors;
        Pca9685 servos;
        MethodsDictionary dict;
        MethodsQueue queue;

        public MeadowApp()
        {
            Initialize();
            onboardLed.SetColor(Color.Green);

        }

        void Initialize()
        {
            Console.WriteLine("Initialize hardware...");

            onboardLed = new RgbPwmLed(device: Device,
                redPwmPin: Device.Pins.OnboardLedRed,
                greenPwmPin: Device.Pins.OnboardLedGreen,
                bluePwmPin: Device.Pins.OnboardLedBlue,
                3.3f, 3.3f, 3.3f,
                Meadow.Peripherals.Leds.IRgbLed.CommonType.CommonAnode);

            bus = Device.CreateI2cBus();

            motors = new Pca9685(bus, 65, 1600);
            motors.Initialize();
            servos = new Pca9685(bus, 64, 50);
            servos.Initialize();

            motor = new MotorController(motors.CreatePwmPort(12, 0), motors.CreatePwmPort(13, 0), motors.CreatePwmPort(14, 0), motors.CreatePwmPort(15, 0), servos.CreatePwmPort(15));
            
            com = new ComCommunication(Device.CreateSerialMessagePort(Device.SerialPortNames.Com4, suffixDelimiter: new byte[] { 10 }, preserveDelimiter: true, 921600, 8, Parity.None, StopBits.One));


            dict = new MethodsDictionary();
            queue = new MethodsQueue(com, dict);

            RegisterMethods();
        }

        private void RegisterMethods()
        {
            dict.RegisterMetod(CommandList.setGear, new Action<string>(motor.SetGear));
            dict.RegisterMetod(CommandList.setLinearSpeed, new Action<string>(motor.SetLinearSpeed));
            dict.RegisterMetod(CommandList.setTurn, new Action<string>(motor.SetTurn));
            dict.RegisterMetod(CommandList.stop, new Action<string>(motor.Break));
            dict.RegisterMetod(CommandList.wait, new Action<string>(HelperMethods.Wait));
            dict.RegisterMetod(CommandList.startInvoking, new Action<string>(queue.StartInvoking));
            dict.RegisterMetod(CommandList.stopInvoking, new Action<string>(queue.StopInvoking));
            dict.RegisterMetod(CommandList.enumerateQueue, new Action<string>(queue.EnumerateQueue));
            dict.RegisterMetod(CommandList.clearQueue, new Action<string>(queue.ClearQueue));
        }
    }
}