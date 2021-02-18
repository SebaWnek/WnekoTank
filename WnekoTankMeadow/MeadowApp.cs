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
using WnekoTankMeadow.CommandControl.ComDevices;

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
        ProximitySensorsArray proxSensors;

        public MeadowApp()
        {
            //try
            //{
            Initialize();
            onboardLed.SetColor(Color.Green);
            TestThings();
            //}
            //catch (Exception e)
            //{
            //    ReportToControl(e.Message);
            //    ReportToControl(e.Message);
            //    queue.StopInvoking();
            //    queue.ClearQueue();
            //    motor.Break();
            //}
        }

        private void TestThings()
        {
            Console.WriteLine("testin ina219");
            INA219.INA219Configuration configuration = new INA219.INA219Configuration(INA219.BusVoltageRangeSettings.range32v,
                                                                                      INA219.PGASettings.gain320mV,
                                                                                      INA219.ADCsettings.Samples128,
                                                                                      INA219.ADCsettings.Samples128,
                                                                                      INA219.ModeSettings.ShuntVoltageContinuous);
            INA219 ina = new INA219(bus, 0x41, configuration);
            Thread.Sleep(100);
            ina.ResetToFactory();
            Thread.Sleep(100);
            ina.Calibrate(3.2f, 0.1f);
            Thread.Sleep(100);
            ina.EnumerateRegisters();
            while (true)
            {
                Console.WriteLine($"Shunt voltage: {ina.ReadShuntVltage()}");
                Console.WriteLine($"Bus voltage: {ina.ReadBusVoltage()}");
                Thread.Sleep(1000);
            }
        }

        private void Test1_Changed(object sender, DigitalInputPortEventArgs e)
        {
            if (e.Value == true)
            {
                onboardLed.SetColor(Color.Blue);
            }
            else
            {
                onboardLed.SetColor(Color.Red);
            }
            Console.WriteLine($"Interrupt: {e.Value}");
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

            expander1 = new Mcp23x08(bus, 32, Device.CreateDigitalInputPort(Device.Pins.D02, InterruptMode.EdgeBoth));

            display = new I2cCharacterDisplay(bus, 39, 2, 16);
            display.Write("I'm alive!");

            //com = new ComCommunication(Device.CreateSerialMessagePort(Device.SerialPortNames.Com4, suffixDelimiter: new byte[] { 10 }, preserveDelimiter: true, 921600, 8, Parity.None, StopBits.One));

            com = new HC12Communication(Device.CreateSerialMessagePort(Device.SerialPortNames.Com1, suffixDelimiter: new byte[] { 10 }, preserveDelimiter: true, 115200, 8, Parity.None, StopBits.One));

            dict = new MethodsDictionary();
            queue = new MethodsQueue(com, dict);

            tempPresSensor = new TempPressureSensor(com, bus);
            positionSensor = new PositionSensor(com, bus);

            motor = new MotorController(
                                        pwm1600.CreatePwmPort(14, 0),
                                        pwm1600.CreatePwmPort(15, 0),
                                        pwm1600.CreatePwmPort(12, 0),
                                        pwm1600.CreatePwmPort(13, 0),
                                        pwm50.CreatePwmPort(15),
                                        Device.CreateDigitalInputPort(Device.Pins.D03, InterruptMode.EdgeRising, ResistorMode.InternalPullDown, 20, 20),
                                        Device.CreateDigitalInputPort(Device.Pins.D04, InterruptMode.EdgeRising, ResistorMode.InternalPullDown, 20, 20),
                                        positionSensor);

            proxSensors = new ProximitySensorsArray(new ProximitySensor[]
            {
                new ProximitySensor(expander1.CreateDigitalInputPort(expander1.Pins.GP7, InterruptMode.EdgeRising), Direction.Forward, StopBehavior.Stop, "Front, right", queue, motor),
                new ProximitySensor(expander1.CreateDigitalInputPort(expander1.Pins.GP6, InterruptMode.EdgeRising), Direction.Forward, StopBehavior.Stop, "Front, left", queue, motor),
                new ProximitySensor(expander1.CreateDigitalInputPort(expander1.Pins.GP5, InterruptMode.EdgeRising), Direction.Forward, StopBehavior.Stop, "Front, center", queue, motor),
                new ProximitySensor(expander1.CreateDigitalInputPort(expander1.Pins.GP4, InterruptMode.EdgeRising), Direction.Backward, StopBehavior.Stop, "Back, center", queue, motor)
            });
            proxSensors.Register(ReportToControl);
            proxSensors.Register(ShowOnDisplay);

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
            dict.RegisterMethod(CommandList.stabilize, new Action<string>(motor.StabilizeDirection));
            dict.RegisterMethod(CommandList.setProxSensors, new Action<string>(proxSensors.SetBehavior));
        }

        private void ReportToControl(object o, string s)
        {
            ReportToControl(s);
        }

        private void ReportToControl(string s)
        {
            com.SendMessage(s);
        }

        private void ShowOnDisplay(object o, string s)
        {
            ShowOnDisplay(s);
        }

        private void ShowOnDisplay(string s)
        {
            display.Write(s);
        }
    }
}