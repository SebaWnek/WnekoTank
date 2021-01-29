using System;
using System.Text;
using System.Threading;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation;
using Meadow.Foundation.Displays.Lcd;
using Meadow.Foundation.ICs.IOExpanders;
using Meadow.Foundation.Leds;
using Meadow.Foundation.Servos;
using Meadow.Hardware;
using WnekoTankMeadow.Drive;
using WnekoTankMeadow.Sensors;
using CommonsLibrary;

namespace WnekoTankMeadow
{
    /// <summary>
    /// Main class
    /// </summary>
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        ITankCommunication com;
        RgbPwmLed onboardLed;
        II2cBus bus;
        MotorController motor;
        Pca9685 pwm1600;
        Pca9685 pwm50;
        MethodsDictionary dict;
        MethodsQueue queue;
        Mcp23x08 expander1;
        I2cCharacterDisplay display;
        TempPressureSensor tempPresSensor;
        PositionSensor positionSensor;

        public MeadowApp()
        {
            Initialize();
            onboardLed.SetColor(Color.Green);
            TestThings();
        }

        private void TestThings()
        {
        }

        /// <summary>
        /// Initializes all hardware and software classes
        /// </summary>
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


            pwm1600 = new Pca9685(bus, 65, 1600);
            pwm1600.Initialize();
            pwm50 = new Pca9685(bus, 64, 50);
            pwm50.Initialize();

            expander1 = new Mcp23x08(bus, 32, Device.CreateDigitalInputPort(Device.Pins.D02, InterruptMode.EdgeRising, ResistorMode.PullDown, 20, 20));

            display = new I2cCharacterDisplay(bus, 39, 2, 16);
            display.Write("I'm alive!");

            com = new ComCommunication(Device.CreateSerialMessagePort(Device.SerialPortNames.Com4, suffixDelimiter: new byte[] { 10 }, preserveDelimiter: true, 921600, 8, Parity.None, StopBits.One));

            dict = new MethodsDictionary();
            queue = new MethodsQueue(com, dict);

            tempPresSensor = new TempPressureSensor(com, bus);
            positionSensor = new PositionSensor(com, bus);

            motor = new MotorController(pwm1600.CreatePwmPort(12, 0),
                                        pwm1600.CreatePwmPort(13, 0),
                                        pwm1600.CreatePwmPort(14, 0),
                                        pwm1600.CreatePwmPort(15, 0),
                                        pwm50.CreatePwmPort(15),
                                        Device.CreateDigitalInputPort(Device.Pins.D03, InterruptMode.EdgeRising, ResistorMode.PullDown, 20, 20),
                                        Device.CreateDigitalInputPort(Device.Pins.D04, InterruptMode.EdgeRising, ResistorMode.PullDown, 20, 20),
                                        positionSensor);


            RegisterMethods();
        }

        /// <summary>
        /// Register possible methods and their corresponding protocol codes in methods dictionary
        /// </summary>
        private void RegisterMethods()
        {
            dict.RegisterMethod(CommandList.setGear, new Action<string>(motor.SetGear));
            dict.RegisterMethod(CommandList.setLinearSpeed, new Action<string>(motor.SetLinearSpeed));
            dict.RegisterMethod(CommandList.setTurn, new Action<string>(motor.SetTurn));
            dict.RegisterMethod(CommandList.stop, new Action<string>(motor.Break));
            dict.RegisterMethod(CommandList.wait, new Action<string>(HelperMethods.Wait));
            dict.RegisterMethod(CommandList.startInvoking, new Action<string>(queue.StartInvoking));
            dict.RegisterMethod(CommandList.stopInvoking, new Action<string>(queue.StopInvoking));
            dict.RegisterMethod(CommandList.enumerateQueue, new Action<string>(queue.EnumerateQueue));
            dict.RegisterMethod(CommandList.clearQueue, new Action<string>(queue.ClearQueue));
            dict.RegisterMethod(CommandList.handshake, new Action<string>(HelperMethods.HandShake));
            dict.RegisterMethod(CommandList.moveForwardBy, new Action<string>(motor.MoveForwardBy));
            dict.RegisterMethod(CommandList.softStop, new Action<string>(motor.SoftBreak));
            dict.RegisterMethod(CommandList.tempPres, new Action<string>(tempPresSensor.Read));
            dict.RegisterMethod(CommandList.position, new Action<string>(positionSensor.Read));
            dict.RegisterMethod(CommandList.calibrate, new Action<string>(positionSensor.Calibrate));
            dict.RegisterMethod(CommandList.checkCalibration, new Action<string>(positionSensor.CheckCalibration));
            dict.RegisterMethod(CommandList.turnBy, new Action<string>(motor.TurnBy));
        }
    }
}