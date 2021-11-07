using CommonsLibrary;
using Meadow.Foundation.Sensors.Motion;
using Meadow.Hardware;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WnekoTankMeadow.Drive
{
    class BNO055 : IPositionSensor
    {
        Bno055 sensor;
        Action<string> sendMessage;
        Action<string> displayMessage;
        I2cPeripheral bno;
        float representationInLSB = 16;
        Stopwatch stopwatch = new Stopwatch();
        EventHandler headingChanged;
        private int turnTimeDelta = 100;
        //private int headingCorrection = 180;

        /// <summary>
        /// Basic constructor
        /// </summary>
        /// <param name="bus">I2C bus</param>
        /// <param name="address">I2C device address</param>
        public BNO055(II2cBus bus, byte address = 40)
        {
            sensor = new Bno055(bus, address);
            sensor.OperatingMode = Bno055.OperatingModes.NINE_DEGREES_OF_FREEDOM;
            bno = new I2cPeripheral(bus, address);
        }

        /// <summary>
        /// Constructor with sender action added
        /// </summary>
        /// <param name="sender">Method to send data to control app</param>
        /// <param name="bus">I2C bus</param>
        /// <param name="address">I2C device address</param>
        public BNO055(Action<string> sender, II2cBus bus, byte address = 40) : this(bus, address)
        {
            sendMessage += sender;
        }
        /// <summary>
        /// https://www.bosch-sensortec.com/media/boschsensortec/downloads/datasheets/bst-bno055-ds000.pdf
        /// Page 26
        /// </summary>
        private void RemapAxis()
        {
            byte axisPositionAddress = 0x41;
            byte axisSignAddress = 0x42;
            byte newAxisPosition = 0b00_10_00_01;
            byte newAxisSigns = 0b00000_1_0_0;
            sensor.OperatingMode = Bno055.OperatingModes.CONFIGURATION_MODE;
            Thread.Sleep(100);
#if DEBUG
            Console.WriteLine($"Writing: {Convert.ToString(newAxisPosition, 2)} to {axisPositionAddress:X}");
            Console.WriteLine($"Writing: {Convert.ToString(newAxisSigns,2)} to {axisSignAddress:X}");
#endif 
            bno.WriteRegister(axisPositionAddress, newAxisPosition);
            bno.WriteRegister(axisSignAddress, newAxisSigns);
            Thread.Sleep(100);
            sensor.OperatingMode = Bno055.OperatingModes.NINE_DEGREES_OF_FREEDOM;
        }
        
        /// <summary>
        /// Register method to send data to control app
        /// </summary>
        /// <param name="sender">Sender method</param>
        public void RegisterSender(Action<string> sender)
        {
            sendMessage += sender;
        }


        /// <summary>
        /// Action<string> to be invoked by queue
        /// </summary>
        /// <param name="empty">No arguments needed</param>
        public void Calibrate(string empty)
        {
            Calibrate();
        }

        /// <summary>
        /// Calibrate sensor.
        /// 1) Resets BNO;
        /// 2) Checks calibration every second (or other sleepTime)
        /// 3) Prints current calibration to control app and small display
        /// 4) If all 3 - gyroscope, accelerometer and magnetometer are calibrates finishes calibration
        /// 5) Ignores system, as there is bug in firmware so can show it at less than 3 even tough all others are 3
        /// BNO055 is calibrated constantly so there is no strict calibrate method - here it's just making sure it's reset first and then that all 3 sensors got calibrated correctly
        /// </summary>
        public void Calibrate()
        {
            byte resetAddress = 0x3F;
            byte resetByte = 0b00100000;
            int sleepTime = 1000;
            byte[] cal;
            sendMessage("Reseting BNO055");
            bno.WriteRegister(resetAddress, resetByte);
            Thread.Sleep(sleepTime);
            RemapAxis();
            sensor.OperatingMode = Bno055.OperatingModes.NINE_DEGREES_OF_FREEDOM;
            for (int i = 0; i < 60; i++) //Timeout after one minute
            {
                cal = CheckCalibration();
                sendMessage(ReturnCommandList.calibrationData + $"{cal[0]};{cal[1]};{cal[2]};{cal[3]}");
                displayMessage($"S:{cal[0]} G:{cal[1]} A: {cal[2]} M:{cal[3]}");
#if DEBUG
                Console.WriteLine($"S: {cal[0]}, G: {cal[1]}, A: {cal[2]}, M: {cal[3]}");
#endif
                if (cal[1] == 3 && cal[2] == 3 && cal[3] == 3)
                {
                    Thread.Sleep(100);
                    sendMessage(ReturnCommandList.calibrationData + $"{cal[0]};{cal[1]};{cal[2]};{cal[3]}");
                    displayMessage($"OK! S:{cal[0]}, G:{cal[1]}, A:{cal[2]}, M:{cal[3]}");
                    Task task = new Task(async () =>
                    {
                        await Task.Delay(5000);
                        displayMessage("Ready!");
                    });
                    task.Start();
#if DEBUG
                    Console.WriteLine($"Done! S: {cal[0]}, G: {cal[1]}, A: {cal[2]}, M: {cal[3]}");
#endif
                    return;
                }
                Thread.Sleep(1000);
            }
            sendMessage(ReturnCommandList.displayMessage + "Not calibrated!");
#if DEBUG
            Console.WriteLine("Not calibrated!");
#endif
        }

        /// <summary>
        /// Checks current calibration and sends back to control app
        /// </summary>
        /// <param name="empty">No parameters needed</param>
        public void CheckCalibration(string empty)
        {
            byte[] cal = CheckCalibration();
            sendMessage(ReturnCommandList.calibrationData + $"{cal[0]};{cal[1]};{cal[2]};{cal[3]}");
#if DEBUG
            Console.WriteLine($"S: {cal[0]}, G: {cal[1]}, A: {cal[2]}, M: {cal[3]}");
#endif
        }
        
        /// <summary>
        /// Reads calibration registers and calculates current calibration from them
        /// </summary>
        /// <returns>Calibration values</returns>
        public byte[] CheckCalibration()
        {
            byte callibration = bno.ReadRegister(53);
            byte system = (byte)((callibration >> 6) & 3);
            byte gyro = (byte)((callibration >> 4) & 3);
            byte acc = (byte)((callibration >> 2) & 3);
            byte mag = (byte)((callibration >> 0) & 3);
            return new byte[] { system, gyro, acc, mag };
        }

        /// <summary>
        /// Gets current position info and sends back to control app.
        /// </summary>
        /// <param name="empty">No params needed</param>
        public void Read(string empty)
        {
            float[] position = Read();
            string msg = ReturnCommandList.positionData + $"{position[0]};{position[1]};{position[2]}";
            //string msg = $"Heading: {position[0]}deg,\r\nRoll: {position[1]}deg,\r\nPitch: {position[2]}deg";
            sendMessage(msg);
        }
        
        /// <summary>
        /// Reads position registers and calculates euler angles from them
        /// </summary>
        /// <returns>Euler angles representing current position - yaw, roll, pitch</returns>
        public float[] Read()
        {
            byte[] data = bno.ReadRegisters(0x1a, 6);
            float[] result = new float[]
            {
                (short)((data[1] << 8) | data[0])/representationInLSB,  //heading
                (short)((data[3] << 8) | data[2])/representationInLSB,  //roll
                (short)((data[5] << 8) | data[4])/representationInLSB   //pitch
            };
            return result;
        }

        /// <summary>
        /// Reads only heading from respective register
        /// </summary>
        /// <returns>Current heading</returns>
        internal float ReadHeading()
        {
            byte[] data = bno.ReadRegisters(0x1a, 2);
            float result = (short)((data[1] << 8) | data[0]) / representationInLSB;
            return result;
        }

        /// <summary>
        /// Heading changed event
        /// </summary>
        private void OnHeadingChanged()
        {
            headingChanged?.Invoke(this, null);
        }

        /// <summary>
        /// Registers for heading changed event
        /// </summary>
        /// <param name="handler"></param>
        public void RegisterForHeadingChanged(EventHandler handler)
        {
            headingChanged += handler;
        }

        /// <summary>
        /// Checks if heading changed to selected value
        /// Depreciated - this will overshoot, as it's not using PID, should use PID version
        /// </summary>
        /// <param name="a">Desired angle</param>
        /// <param name="token">Cancellation token to stop before heading reached</param>
        public void StartCheckingAngle(int a, CancellationToken token)
        {
            int angle = a;
            int direction = Math.Sign(angle);
            float previousHeading = ReadHeading();
            float currentHeading = 0;
            float turned = 0;
            float deltaAngle;
            bool done = false;
            while (!done)
            {
                if (token.IsCancellationRequested) break;
                currentHeading = ReadHeading();
                deltaAngle = currentHeading - previousHeading;
                if (direction == 1 && deltaAngle < 0) deltaAngle += 360;
                if (direction == -1 && deltaAngle > 0) deltaAngle -= 360;
                turned += deltaAngle;
#if DEBUG
                Console.WriteLine($"D: {deltaAngle}, t: {turned}, dir: {currentHeading}");
#endif
                if (Math.Abs(turned) >= Math.Abs(angle))
                {
                    done = true;
                    OnHeadingChanged();
                }
                previousHeading = currentHeading;
                Thread.Sleep(turnTimeDelta);
            }
        }

        /// <summary>
        /// Registers method for displaying text on small screen
        /// </summary>
        /// <param name="write"></param>
        internal void RegisterScreen(Action<string> write)
        {
            displayMessage += write;
        }
    }
}
