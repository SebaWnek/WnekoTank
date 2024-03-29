﻿using System;
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
    public partial class MeadowApp : App<F7Micro, MeadowApp>
    {
        ITankCommunication com;
        ITankCommunication serialCom;
        ITankCommunication ipCom;
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
        Rtc clock;
        public MeadowApp()
        {
            try
            {
                Initialize();
                onboardLed.SetColor(Color.Green);
                //TestThings();
            }
            //Catching all exceptions, so we can send them to control application and display on screen
            catch (Exception e)
            {
                com.SendMessage(ReturnCommandList.exception + e.Message + ReturnCommandList.exceptionTrace + e.StackTrace);
#if DEBUG
                Console.WriteLine(e.Message + "/n/n" + e.StackTrace);
#endif
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
            serialCom = new HC12Communication(Device.CreateSerialMessagePort(Device.SerialPortNames.Com4,
                                                                       suffixDelimiter: new byte[] { 10 },
                                                                       preserveDelimiter: true, 115200, 8, Parity.None,
                                                                       StopBits.One));
            com = new CommunicationWrapper(serialCom);

#if DEBUG
            Console.WriteLine("Initializing WiFi");
#endif
            displaySmall.Write("Initializing WiFi");
            ipCom = new WifiUdpCommunication(Device.WiFiAdapter, new Action<string>[] { com.SendMessage, displaySmall.Write });
            //Trying to connect at startup to speed process up, but router might be still booting so in that case will try again when requested
            Task.Run(()=> 
            {
                try
                {
                    ConnectToWiFi("");
                }
                catch (Exception e)
                {
                    com.SendMessage(ReturnCommandList.exception + e.Message + ReturnCommandList.exceptionTrace + e.StackTrace);
#if DEBUG
                    Console.WriteLine(e.Message + "/n/n" + e.StackTrace);
#endif
                }
            }); 

#if DEBUG
            Console.WriteLine("Initializing RTC");
#endif
            displaySmall.Write("Initializing RTC");
            clock = new Rtc(bus, com.SendMessage, SetClock);
            clock.SetClockFromRtc();

#if DEBUG
            Console.WriteLine("Initializing watchdog");
#endif
            displaySmall.Write("Initializing watchdog");
            watchdog = new Watchdog(Watchdog.Type.SerialPort);
            //serialCom.RegisterWatchdog(watchdog.MessageReceived);
            //ipCom.RegisterWatchdog(watchdog.MessageReceived);
            watchdog.RegisterSender(com.SendMessage);
            watchdog.RegisterBlockAction(EmergencyDisable);
            watchdog.RegisterSwitchToSerial(SwitchToSerial);

            dict = new MethodsDictionary();
            queue = new MethodsQueue(com, dict);
            ipCom.SubscribeToMessages(queue.MessageReceived);
            ipCom.SubscribeToMessages(watchdog.MessageReceived);
            serialCom.SubscribeToMessages(queue.MessageReceived);
            serialCom.SubscribeToMessages(watchdog.MessageReceived);

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
            watchdog.RegisterBuzzer(buzzer.Buzz);
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
                                        positionSensor, gimbal, queue.ClearQueue);

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
            //watchdog.StartCheckingMessages();
#if DEBUG
            Console.WriteLine("All hardware initialized!");
#endif
            displaySmall.Clear();
            displaySmall.Write("Ready!");
            com.SendMessage("Ready!");
#pragma warning disable CS4014 // To wywołanie nie jest oczekiwane, dlatego wykonywanie bieżącej metody będzie kontynuowane do czasu ukończenia wywołania
            buzzer.BuzzPulse(100, 100, 3);
#pragma warning restore CS4014 // To wywołanie nie jest oczekiwane, dlatego wykonywanie bieżącej metody będzie kontynuowane do czasu ukończenia wywołania
        }

        /// <summary>
        /// Register possible methods and their corresponding protocol codes in methods dictionary
        /// </summary>
        private void RegisterMethods()
        {
            dict.RegisterMethod(TankCommandList.setGear, motor.SetGear);
            dict.RegisterMethod(TankCommandList.setLinearSpeed, motor.SetLinearSpeed);
            dict.RegisterMethod(TankCommandList.setTurn, motor.SetTurn);
            dict.RegisterMethod(TankCommandList.setSpeedWithTurn, motor.SetSpeedAndTurn);
            dict.RegisterMethod(TankCommandList.stop, motor.Break);
            dict.RegisterMethod(TankCommandList.wait, Wait);
            dict.RegisterMethod(TankCommandList.startInvoking, queue.StartInvoking);
            dict.RegisterMethod(TankCommandList.stopInvoking, queue.StopInvoking);
            dict.RegisterMethod(TankCommandList.enumerateQueue, queue.EnumerateQueue);
            dict.RegisterMethod(TankCommandList.clearQueue, queue.ClearQueue);
            dict.RegisterMethod(TankCommandList.handshake, HandShake);
            dict.RegisterMethod(TankCommandList.moveForwardBy, motor.MoveForwardBy);
            dict.RegisterMethod(TankCommandList.softStop, motor.SoftBreak);
            dict.RegisterMethod(TankCommandList.tempPres, tempPresSensor.Read);
            dict.RegisterMethod(TankCommandList.position, positionSensor.Read);
            dict.RegisterMethod(TankCommandList.calibrate, positionSensor.Calibrate);
            dict.RegisterMethod(TankCommandList.checkCalibration, positionSensor.CheckCalibration);
            dict.RegisterMethod(TankCommandList.turnBy, motor.TurnBy);
            dict.RegisterMethod(TankCommandList.moveByCamera, motor.MoveToByAngles);
            dict.RegisterMethod(TankCommandList.turnToByCamera, motor.TurnToByCamera);
            dict.RegisterMethod(TankCommandList.stabilizeDirection, motor.StabilizeDirection);
            dict.RegisterMethod(TankCommandList.setProxSensors, proxSensors.SetBehavior);
            dict.RegisterMethod(TankCommandList.setGimbalAngle, gimbal.SetAngle);
            dict.RegisterMethod(TankCommandList.changeGimbalAngleBy, gimbal.ChangeAngleBy);
            dict.RegisterMethod(TankCommandList.stabilizeGimbal, gimbal.SetStabilization);
            dict.RegisterMethod(TankCommandList.diagnoze, Diangoze);
            dict.RegisterMethod(TankCommandList.getElectricData, ina219s.ReturnData);
            dict.RegisterMethod(TankCommandList.sendingElectricData, ina219s.ChangeSending);
            dict.RegisterMethod(TankCommandList.setElectricDataDelay, ina219s.ChangeTimeDelta);
            dict.RegisterMethod(TankCommandList.fanInasState, inasFan.SetState);
            dict.RegisterMethod(TankCommandList.fanLedsState, LEDsFans.SetState);
            dict.RegisterMethod(TankCommandList.fanMotorsState, motorsFans.SetState);
            dict.RegisterMethod(TankCommandList.ledNarrowPower, narrowLed.SetBrightnes);
            dict.RegisterMethod(TankCommandList.ledWidePower, wideLed.SetBrightnes);
            dict.RegisterMethod(TankCommandList.hello, Hello);
            dict.RegisterMethod(TankCommandList.connectUdp, SwitchToUdp);
            dict.RegisterMethod(TankCommandList.setClock, clock.SetClockFromPc);
            dict.RegisterMethod(TankCommandList.checkClock, clock.CheckClock);
            dict.RegisterMethod(TankCommandList.resetDevice, ResetDevice);
            dict.RegisterMethod(TankCommandList.checkWiFiStatus, CheckWiFiStatus);
            dict.RegisterMethod(TankCommandList.connectToWiFi, ConnectToWiFi);
        }
    }
}