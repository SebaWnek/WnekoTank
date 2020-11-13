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
        ComController com;
        RgbPwmLed onboardLed;
        II2cBus bus;
        MotorController motor;
        Pca9685 motors;
        Pca9685 servos;


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
            
            com = new ComController(Device.CreateSerialMessagePort(Device.SerialPortNames.Com4, suffixDelimiter: new byte[] { 10 }, preserveDelimiter: true, 921600, 8, Parity.None, StopBits.One), motor);


        }


    }
}
