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
using WnekoTankMeadow.Others;
using System.Diagnostics;

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
        Display16x2 display;
        TempPressureSensor tempPresSensor;
        BNO055 positionSensor;
        ProximitySensorsArray proxSensors;
        CameraGimbal gimbal;

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
            //Console.WriteLine(positionSensor.Read().ToString());
            motor.SetLinearSpeed(100);
        }

        private void TestThings()
        {
            Console.WriteLine("testing ina219");
            INA219.INA219Configuration configuration = new INA219.INA219Configuration(INA219.BusVoltageRangeSettings.range32v,
                                                                                      INA219.PGASettings.Gain320mV,
                                                                                      INA219.ADCsettings.Samples128,
                                                                                      INA219.ADCsettings.Samples128,
                                                                                      INA219.ModeSettings.ShuntBusContinuous);
            Console.WriteLine("Trying to creaate INA");
            INA219 ina = new INA219(bus, 0x45);
            Console.WriteLine("Ina created!");
            Thread.Sleep(100);
            ina.ResetToFactory();
            ina.Calibrate(3.2f, 0.1f);
            Thread.Sleep(100);
            //ina.EnumerateRegisters();
            Thread.Sleep(1000);
            //ina.Configure(configuration);
            Thread.Sleep(1000);
            ina.EnumerateRegisters();
            while (true)
            {
                Console.WriteLine($"Shunt voltage: {ina.ReadShuntVltage()}V");
                Console.WriteLine($"Bus voltage: {ina.ReadBusVoltage()}V");
                Console.WriteLine($"Current: {ina.ReadCurrent()}A");
                Console.WriteLine($"Power: {ina.ReadPower()}W");
                Thread.Sleep(2000);
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


            pwm1600 = new Pca9685(bus, 97, 1600);
            pwm1600.Initialize();
            pwm50 = new Pca9685(bus, 96, 50);
            pwm50.Initialize();

            display = new Display16x2(bus, 39);
            display.Write("I'm alive!");

            expander1 = new Mcp23x08(bus, 32, Device.CreateDigitalInputPort(Device.Pins.D02, InterruptMode.EdgeBoth));

            //com = new ComCommunication(Device.CreateSerialMessagePort(Device.SerialPortNames.Com4, suffixDelimiter: new byte[] { 10 }, preserveDelimiter: true, 921600, 8, Parity.None, StopBits.One));

            com = new HC12Communication(Device.CreateSerialMessagePort(Device.SerialPortNames.Com1, suffixDelimiter: new byte[] { 10 }, preserveDelimiter: true, 115200, 8, Parity.None, StopBits.One));

            dict = new MethodsDictionary();
            queue = new MethodsQueue(com, dict);

            tempPresSensor = new TempPressureSensor(bus);
            tempPresSensor.RegisterSender(com.SendMessage);
            positionSensor = new BNO055(bus, 0x28);
            positionSensor.RegisterSender(com.SendMessage);

            motor = new MotorController(
                                        pwm1600.CreatePwmPort(2, 0),
                                        pwm1600.CreatePwmPort(3, 0),
                                        pwm1600.CreatePwmPort(0, 0),
                                        pwm1600.CreatePwmPort(1, 0),
                                        pwm50.CreatePwmPort(0),
                                        Device.CreateDigitalInputPort(Device.Pins.D03, InterruptMode.EdgeRising, ResistorMode.InternalPullDown, 20, 20),
                                        Device.CreateDigitalInputPort(Device.Pins.D04, InterruptMode.EdgeRising, ResistorMode.InternalPullDown, 20, 20),
                                        positionSensor);

            gimbal = new CameraGimbal(pwm50.CreatePwmPort(14), pwm50.CreatePwmPort(13), positionSensor);

            proxSensors = new ProximitySensorsArray(new ProximitySensor[]
            {
                new ProximitySensor(expander1.CreateDigitalInputPort(expander1.Pins.GP7, InterruptMode.EdgeRising), Direction.Forward, StopBehavior.Stop, "Front, right", queue, motor),
                new ProximitySensor(expander1.CreateDigitalInputPort(expander1.Pins.GP6, InterruptMode.EdgeRising), Direction.Forward, StopBehavior.Stop, "Front, left", queue, motor),
                new ProximitySensor(expander1.CreateDigitalInputPort(expander1.Pins.GP5, InterruptMode.EdgeRising), Direction.Forward, StopBehavior.Stop, "Front, center", queue, motor),
                new ProximitySensor(expander1.CreateDigitalInputPort(expander1.Pins.GP4, InterruptMode.EdgeRising), Direction.Backward, StopBehavior.Stop, "Back, center", queue, motor)
            });
            proxSensors.Register(com.SendMessage);
            proxSensors.Register(display.Write);

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
            dict.RegisterMethod(CommandList.setGimbalAngle, new Action<string>(gimbal.SetAngle));
            dict.RegisterMethod(CommandList.stabilizeGimbal, new Action<string>(gimbal.StartStabilizing));
            dict.RegisterMethod(CommandList.diagnoze, new Action<string>(Diangoze));
        }

        public void Diangoze(string empty)
        {
            Process currentProcess = Process.GetCurrentProcess();
#if DEBUG
            Console.WriteLine(currentProcess.Id);
            //Console.WriteLine(currentProcess.ProcessName);
            Console.WriteLine(currentProcess.Threads.Count);
#endif
            ProcessThreadCollection currentThreads = currentProcess.Threads;

            foreach (ProcessThread thread in currentThreads)
            {
#if DEBUG
                Console.WriteLine(thread.Id.ToString());
#endif
                //com.SendMessage(thread.Id.ToString());
                Thread.Sleep(20);
            }
        }
    }
}