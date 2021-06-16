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
using System.Threading.Tasks;

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
        Mcp23x08 expander2;
        DisplayLCD displaySmall;
        DisplayLCD displayBig;
        TempPressureSensor tempPresSensor;
        BNO055 positionSensor;
        ProximitySensorsArray proxSensors;
        CameraGimbal gimbal;
        Buzzer buzzer;
        INA219Array ina219s;
        Fan motorsFans;
        Fan inasFan;
        Fan LEDsFans;
        LedLamp wideLed;
        LedLamp narrowLed;
        Camera camera;
        Watchdog watchdog;
        public MeadowApp()
        {
            try
            {
                Initialize();
                onboardLed.SetColor(Color.Green);
                //TestThings();
            }
            catch (Exception e)
            {
                com.SendMessage(ReturnCommandList.exception + e.Message + ReturnCommandList.exceptionTrace + e.StackTrace);
                displayBig.Write(e.Message);
                displaySmall.Write(e.Message);
                queue.StopInvoking();
                queue.ClearQueue();
                motor.Break();
            }
        }

        private void TestThings()
        {
            Thread.Sleep(10000);
            throw new Exception("Test exception");
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

#if DEBUG
            Console.WriteLine("Initializing bus");
#endif
            bus = Device.CreateI2cBus(400000);

#if DEBUG
            Console.WriteLine("Initializing display 1");
#endif
            displaySmall = new DisplayLCD(bus, 0x27, 2, 16);
            displaySmall.Write("I'm alive!");

#if DEBUG
            Console.WriteLine("Initializing display 2");
#endif
            displayBig = new DisplayLCD(bus, 0x23, 4, 20);
            displayBig.Write("I'm alive!");

#if DEBUG
            Console.WriteLine("Initializing hc12");
#endif
            displaySmall.Write("Initializing radio");
            com = new HC12Communication(Device.CreateSerialMessagePort(Device.SerialPortNames.Com4,
                                                                       suffixDelimiter: new byte[] { 10 },
                                                                       preserveDelimiter: true, 115200, 8, Parity.None,
                                                                       StopBits.One));

            watchdog = new Watchdog();
            com.RegisterWatchdog(watchdog.MessageReceived);
            watchdog.RegisterSender(com.SendMessage);
            watchdog.RegisterBlockAction(EmergencyDisable);
            watchdog.StartCheckingMessages();

            dict = new MethodsDictionary();
            queue = new MethodsQueue(com, dict);

#if DEBUG
            Console.WriteLine("Initializing pca @1600Hz");
#endif
            displaySmall.Write("Initializing PWM @1600Hz");
            pwm1600 = new Pca9685(bus, 0x61, 1600);
            pwm1600.Initialize();
#if DEBUG
            Console.WriteLine("Initializing pca @50Hz");
#endif
            displaySmall.Write("Initializing PWM @50Hz");
            pwm50 = new Pca9685(bus, 0x60, 50);
            pwm50.Initialize();

#if DEBUG
            Console.WriteLine("Initializing expander 1");
#endif
            displaySmall.Write("Initializing expander 1");
            expander1 = new Mcp23x08(bus, 0x20, Device.CreateDigitalInputPort(Device.Pins.D02, InterruptMode.EdgeBoth));

#if DEBUG
            Console.WriteLine("Initializing expander 2");
#endif
            displaySmall.Write("Initializing expander 2");
            expander2 = new Mcp23x08(bus, 0x21, Device.CreateDigitalInputPort(Device.Pins.D06, InterruptMode.EdgeBoth));

#if DEBUG
            Console.WriteLine("Initializing fans");
#endif
            displaySmall.Write("Initializing fans");
            LEDsFans = new Fan(expander2.CreateDigitalOutputPort(expander2.Pins.GP4), "LEDs' fans");
            motorsFans = new Fan(expander2.CreateDigitalOutputPort(expander2.Pins.GP5), "Motors' fans");
            inasFan = new Fan(expander2.CreateDigitalOutputPort(expander2.Pins.GP6), "INAs fan");

#if DEBUG
            Console.WriteLine("Initializing buzzer");
#endif
            displaySmall.Write("Initializing buzzer");
            buzzer = new Buzzer(expander2.CreateDigitalOutputPort(expander2.Pins.GP3, false, OutputType.OpenDrain));
#pragma warning disable CS4014 // To wywołanie nie jest oczekiwane, dlatego wykonywanie bieżącej metody będzie kontynuowane do czasu ukończenia wywołania
            buzzer.Buzz();
#pragma warning restore CS4014 // To wywołanie nie jest oczekiwane, dlatego wykonywanie bieżącej metody będzie kontynuowane do czasu ukończenia wywołania

#if DEBUG
            Console.WriteLine("Initializing power sensors");
#endif
            displaySmall.Write("Initializing power sensors");
            INA219.INA219Configuration config = new INA219.INA219Configuration(INA219.BusVoltageRangeSettings.range32v,
                                                                               INA219.PGASettings.Gain320mV,
                                                                               INA219.ADCsettings.Samples128,
                                                                               INA219.ADCsettings.Samples128,
                                                                               INA219.ModeSettings.ShuntBusContinuous);
            ina219s = new INA219Array(new INA219[]
            {
                new INA219(3.2f, 0.1f, 1, bus, 0x41, config, "C"),
                new INA219(10f, 0.01f, 1, bus, 0x44, config, "L"),
                new INA219(10f, 0.01f, 1, bus, 0x45, config, "R")
            }, buzzer, displayBig, EmergencyDisable);
            ina219s.RegisterSender(com.SendMessage);
            ina219s.RegisterFan(inasFan.SetState);
            ina219s.RegisterFan(motorsFans.SetState);
            ina219s.StartMonitoringVoltage();

            //com = new ComCommunication(Device.CreateSerialMessagePort(Device.SerialPortNames.Com4, suffixDelimiter: new byte[] { 10 }, preserveDelimiter: true, 921600, 8, Parity.None, StopBits.One));

#if DEBUG
            Console.WriteLine("Initializing temperature sensor");
#endif
            displaySmall.Write("Initializing temperature sensor");
            tempPresSensor = new TempPressureSensor(bus);
            tempPresSensor.RegisterSender(com.SendMessage);
#if DEBUG
            Console.WriteLine("Initializing position sensor");
#endif
            displaySmall.Write("Initializing position sensor");
            positionSensor = new BNO055(bus, 0x28);
            positionSensor.RegisterSender(com.SendMessage);
            positionSensor.RegisterScreen(displaySmall.Write);

#if DEBUG
            Console.WriteLine("Initializing motor controller");
#endif
            displaySmall.Write("Initializing motor controller");
            motor = new MotorController(pwm1600.CreatePwmPort(3, 0),
                                        pwm1600.CreatePwmPort(2, 0),
                                        pwm1600.CreatePwmPort(1, 0),
                                        pwm1600.CreatePwmPort(0, 0),
                                        pwm50.CreatePwmPort(0),
                                        Device.CreateDigitalInputPort(Device.Pins.D03, InterruptMode.EdgeRising, ResistorMode.InternalPullDown, 20, 20),
                                        Device.CreateDigitalInputPort(Device.Pins.D04, InterruptMode.EdgeRising, ResistorMode.InternalPullDown, 20, 20),
                                        positionSensor);

#if DEBUG
            Console.WriteLine("Initializing gimbal");
#endif
            displaySmall.Write("Initializing camera gimbal");
            gimbal = new CameraGimbal(pwm50.CreatePwmPort(2), pwm50.CreatePwmPort(1), positionSensor);

#if DEBUG
            Console.WriteLine("Initializing proximity sensors");
#endif
            displaySmall.Write("Initializing proximity sensors");
            proxSensors = new ProximitySensorsArray(new ProximitySensor[]
            {
                new ProximitySensor(expander1.CreateDigitalInputPort(expander1.Pins.GP0, InterruptMode.EdgeRising, ResistorMode.InternalPullUp, 500, 500), Direction.Forward, StopBehavior.Stop, "Front, right", queue, motor),
                new ProximitySensor(expander1.CreateDigitalInputPort(expander1.Pins.GP1, InterruptMode.EdgeRising, ResistorMode.InternalPullUp, 500, 500), Direction.Forward, StopBehavior.Stop, "Front, left", queue, motor),
                new ProximitySensor(expander1.CreateDigitalInputPort(expander1.Pins.GP2, InterruptMode.EdgeRising, ResistorMode.InternalPullUp, 500, 500), Direction.Forward, StopBehavior.Stop, "Front, center", queue, motor),
                new ProximitySensor(expander1.CreateDigitalInputPort(expander1.Pins.GP3, InterruptMode.EdgeRising, ResistorMode.InternalPullUp, 500, 500), Direction.Backward, StopBehavior.Stop, "Back, center", queue, motor)
            });
            proxSensors.Register(com.SendMessage);
            //proxSensors.Register(displaySmall.Write);

#if DEBUG
            Console.WriteLine("Initializing lamps");
#endif
            displaySmall.Write("Initializing lamps");
            narrowLed = new LedLamp(pwm1600.CreatePwmPort(4, 0), LEDsFans, "Front, narrow");
            wideLed = new LedLamp(pwm1600.CreatePwmPort(5, 0), LEDsFans, "Front, wide");

#if DEBUG
            Console.WriteLine("Initializing cameras");
#endif
            displaySmall.Write("Initializing cameras");
            camera = new Camera(expander2.CreateDigitalOutputPort(expander2.Pins.GP7));
            camera.SetCamera(true);

#if DEBUG
            Console.WriteLine("Finishing initialization");
#endif
            displaySmall.Write("Finishing initialization");
            RegisterMethods();
#if DEBUG
            Console.WriteLine("All hardware initialized!");
#endif
            displaySmall.Clear();
            displaySmall.Write("Ready!");
            com.SendMessage("Ready!");
            buzzer.BuzzPulse(100, 100, 3);
        }

        /// <summary>
        /// Register possible methods and their corresponding protocol codes in methods dictionary
        /// </summary>
        private void RegisterMethods()
        {
            dict.RegisterMethod(TankCommandList.setGear, new Action<string>(motor.SetGear));
            dict.RegisterMethod(TankCommandList.setLinearSpeed, new Action<string>(motor.SetLinearSpeed));
            dict.RegisterMethod(TankCommandList.setTurn, new Action<string>(motor.SetTurn));
            dict.RegisterMethod(TankCommandList.stop, new Action<string>(motor.Break));
            dict.RegisterMethod(TankCommandList.wait, new Action<string>(HelperMethods.Wait));
            dict.RegisterMethod(TankCommandList.startInvoking, new Action<string>(queue.StartInvoking));
            dict.RegisterMethod(TankCommandList.stopInvoking, new Action<string>(queue.StopInvoking));
            dict.RegisterMethod(TankCommandList.enumerateQueue, new Action<string>(queue.EnumerateQueue));
            dict.RegisterMethod(TankCommandList.clearQueue, new Action<string>(queue.ClearQueue));
            dict.RegisterMethod(TankCommandList.handshake, new Action<string>(HandShake));
            dict.RegisterMethod(TankCommandList.moveForwardBy, new Action<string>(motor.MoveForwardBy));
            dict.RegisterMethod(TankCommandList.softStop, new Action<string>(motor.SoftBreak));
            dict.RegisterMethod(TankCommandList.tempPres, new Action<string>(tempPresSensor.Read));
            dict.RegisterMethod(TankCommandList.position, new Action<string>(positionSensor.Read));
            dict.RegisterMethod(TankCommandList.calibrate, new Action<string>(positionSensor.Calibrate));
            dict.RegisterMethod(TankCommandList.checkCalibration, new Action<string>(positionSensor.CheckCalibration));
            dict.RegisterMethod(TankCommandList.turnBy, new Action<string>(motor.TurnBy));
            dict.RegisterMethod(TankCommandList.stabilize, new Action<string>(motor.StabilizeDirection));
            dict.RegisterMethod(TankCommandList.setProxSensors, new Action<string>(proxSensors.SetBehavior));
            dict.RegisterMethod(TankCommandList.setGimbalAngle, new Action<string>(gimbal.SetAngle));
            dict.RegisterMethod(TankCommandList.changeGimbalAngleBy, new Action<string>(gimbal.ChangeAngleBy));
            dict.RegisterMethod(TankCommandList.stabilizeGimbal, new Action<string>(gimbal.StartStabilizing));
            dict.RegisterMethod(TankCommandList.diagnoze, new Action<string>(Diangoze));
            dict.RegisterMethod(TankCommandList.getElectricData, new Action<string>(ina219s.ReturnData));
            dict.RegisterMethod(TankCommandList.sendingElectricData, new Action<string>(ina219s.ChangeSending));
            dict.RegisterMethod(TankCommandList.fanInasState, new Action<string>(inasFan.SetState));
            dict.RegisterMethod(TankCommandList.fanLedsState, new Action<string>(LEDsFans.SetState));
            dict.RegisterMethod(TankCommandList.fanMotorsState, new Action<string>(motorsFans.SetState));
            dict.RegisterMethod(TankCommandList.ledNarrowPower, new Action<string>(narrowLed.SetBrightnes));
            dict.RegisterMethod(TankCommandList.ledWidePower, new Action<string>(wideLed.SetBrightnes));

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

        private void EmergencyDisable()
        {
            motor.Break();
            queue.LockQueue();
            onboardLed.SetColor(Color.Red);
            displaySmall.Write("BATTERY TOO LOW!!!CHARGE  ASAP!!");
        }
        public void HandShake(string empty)
        {
#pragma warning disable CS4014 // To wywołanie nie jest oczekiwane, dlatego wykonywanie bieżącej metody będzie kontynuowane do czasu ukończenia wywołania
            buzzer.Buzz();
#pragma warning restore CS4014 // To wywołanie nie jest oczekiwane, dlatego wykonywanie bieżącej metody będzie kontynuowane do czasu ukończenia wywołania
        }
    }
}