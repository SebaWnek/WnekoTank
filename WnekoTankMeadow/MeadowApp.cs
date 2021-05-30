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
        public MeadowApp()
        {
            //try
            //{
            Initialize();
            onboardLed.SetColor(Color.Green);
            //TestThings();
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
        }

        private void TestThings()
        {
            //LEDsFans.StartFan();
            //Thread.Sleep(5000);
            //LEDsFans.StopFan();
            //motorsFans.StartFan();
            //Thread.Sleep(5000);
            //motorsFans.StopFan();
            //inasFan.StartFan();
            //Thread.Sleep(5000);
            //inasFan.StopFan();

            //LEDsFans.StartFan();

            //for (int i = 0; i <= 100; i++)
            //{
            //    wideLed.SetBrightnes(i);
            //    Console.WriteLine(i);
            //    Thread.Sleep(50);
            //}
            //Thread.Sleep(1000);
            //wideLed.SetBrightnes(0);
            //for (int i = 0; i <= 100; i++)
            //{
            //    narrowLed.SetBrightnes(i);
            //    Console.WriteLine(i);
            //    Thread.Sleep(50);
            //}
            //Thread.Sleep(1000);
            //narrowLed.SetBrightnes(0);

            //buzzer.BuzzPulse(500, 1000, 3);

            //LEDsFans.StopFan();

            //LEDsFans.StartFan();
            //motorsFans.StartFan();
            //inasFan.StartFan();
            ////string test = "123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 ";
            ////displayBig.Write(test);
            ////displaySmall.Write(test);
            ////Console.WriteLine("testing ina219");
            ////INA219.INA219Configuration configuration = new INA219.INA219Configuration(INA219.BusVoltageRangeSettings.range32v,
            ////                                                                          INA219.PGASettings.Gain320mV,
            ////                                                                          INA219.ADCsettings.Samples128,
            ////                                                                          INA219.ADCsettings.Samples128,
            ////                                                                          INA219.ModeSettings.ShuntBusContinuous);
            ////Console.WriteLine("Trying to creaate INA");
            ////INA219 ina = new INA219(bus, 0x41);
            ////Console.WriteLine("Ina created!");
            ////Thread.Sleep(100);
            ////ina.ResetToFactory();
            ////ina.Calibrate(3.2f, 0.1f);
            ////Thread.Sleep(100);
            //////ina.EnumerateRegisters();
            ////Thread.Sleep(1000);
            //////ina.Configure(configuration);
            ////Thread.Sleep(1000);
            ////ina.EnumerateRegisters();
            ////while (true)
            ////{
            ////    Console.WriteLine($"Shunt voltage: {ina.ReadShuntVltage()}V");
            ////    Console.WriteLine($"Bus voltage: {ina.ReadBusVoltage()}V");
            ////    Console.WriteLine($"Current: {ina.ReadCurrent()}A");
            ////    Console.WriteLine($"Power: {ina.ReadPower()}W");
            ////    Thread.Sleep(2000);
            ////}
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

            dict = new MethodsDictionary();
            queue = new MethodsQueue(com, dict);

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
            ina219s.StartMonitoringVoltage();

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
            Console.WriteLine("Initializing buzzer");
#endif
            displaySmall.Write("Initializing buzzer");
            buzzer = new Buzzer(expander2.CreateDigitalOutputPort(expander2.Pins.GP3, false, OutputType.OpenDrain));
            buzzer.Buzz();

#if DEBUG
            Console.WriteLine("Initializing fans");
#endif
            displaySmall.Write("Initializing fans");
            LEDsFans = new Fan(expander2.CreateDigitalOutputPort(expander2.Pins.GP4), "LEDs' fans");
            motorsFans = new Fan(expander2.CreateDigitalOutputPort(expander2.Pins.GP5), "Motors' fans");
            inasFan = new Fan(expander2.CreateDigitalOutputPort(expander2.Pins.GP6), "INAs fan");

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
            dict.RegisterMethod(CommandList.handshake, new Action<string>(HandShake));
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
            dict.RegisterMethod(CommandList.getElectricData, new Action<string>(ina219s.ReturnData));
            dict.RegisterMethod(CommandList.fanInasState, new Action<string>(inasFan.SetState));
            dict.RegisterMethod(CommandList.fanLedsState, new Action<string>(LEDsFans.SetState));
            dict.RegisterMethod(CommandList.fanMotorsState, new Action<string>(motorsFans.SetState));
            dict.RegisterMethod(CommandList.ledNarrowPower, new Action<string>(narrowLed.SetBrightnes));
            dict.RegisterMethod(CommandList.ledWidePower, new Action<string>(wideLed.SetBrightnes));
            
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
            buzzer.Buzz();
        }
    }
}