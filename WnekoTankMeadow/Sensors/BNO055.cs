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
        I2cPeripheral bno;
        float representationInLSB = 16;
        Stopwatch stopwatch = new Stopwatch();
        EventHandler headingChanged;
        private int turnTimeDelta = 100;
        private int headingCorrection = 180;

        public BNO055(II2cBus bus, byte address = 40)
        {
            sensor = new Bno055(bus, address);
            sensor.OperatingMode = Bno055.OperatingModes.NineDegreesOfFreedom;
            bno = new I2cPeripheral(bus, address);
        }

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
            sensor.OperatingMode = Bno055.OperatingModes.ConfigurationMode;
            Thread.Sleep(100);
#if DEBUG
            Console.WriteLine($"Writing: {Convert.ToString(newAxisPosition, 2)} to {axisPositionAddress:X}");
            Console.WriteLine($"Writing: {Convert.ToString(newAxisSigns,2)} to {axisSignAddress:X}");
#endif 
            bno.WriteRegister(axisPositionAddress, newAxisPosition);
            bno.WriteRegister(axisSignAddress, newAxisSigns);
            Thread.Sleep(100);
            sensor.OperatingMode = Bno055.OperatingModes.NineDegreesOfFreedom;
        }

        public void RegisterSender(Action<string> sender)
        {
            sendMessage += sender;
        }

        public void Calibrate(string empty)
        {
            Calibrate();
        }

        public void Calibrate()
        {
            byte resetAddress = 0x3F;
            byte resetByte = 0b00100000;
            byte[] cal;
            sendMessage("Reseting BNO055");
            bno.WriteRegister(resetAddress, resetByte);
            Thread.Sleep(1000);
            RemapAxis();
            sensor.OperatingMode = Bno055.OperatingModes.NineDegreesOfFreedom;
            for (int i = 0; i < 60; i++) //Timeout after one minute
            {
                cal = CheckCalibration();
                sendMessage($"S: {cal[0]}, G: {cal[1]}, A: {cal[2]}, M: {cal[3]}");
#if DEBUG
                Console.WriteLine($"S: {cal[0]}, G: {cal[1]}, A: {cal[2]}, M: {cal[3]}");
#endif
                if (cal[1] == 3 && cal[2] == 3 && cal[3] == 3)
                {
                    Thread.Sleep(100);
                    sendMessage($"Done! S: {cal[0]}, G: {cal[1]}, A: {cal[2]}, M: {cal[3]}");
#if DEBUG
                    Console.WriteLine($"Done! S: {cal[0]}, G: {cal[1]}, A: {cal[2]}, M: {cal[3]}");
#endif
                    return;
                }
                Thread.Sleep(1000);
            }
            sendMessage("Not calibrated!");
#if DEBUG
            Console.WriteLine("Not calibrated!");
#endif
        }

        public void CheckCalibration(string empty)
        {
            byte[] cal = CheckCalibration();
            sendMessage($"S: {cal[0]}, G: {cal[1]}, A: {cal[2]}, M: {cal[3]}");
#if DEBUG
            Console.WriteLine($"S: {cal[0]}, G: {cal[1]}, A: {cal[2]}, M: {cal[3]}");
#endif
        }
        
        public byte[] CheckCalibration()
        {
            byte callibration = bno.ReadRegister(53);
            byte system = (byte)((callibration >> 6) & 3);
            byte gyro = (byte)((callibration >> 4) & 3);
            byte acc = (byte)((callibration >> 2) & 3);
            byte mag = (byte)((callibration >> 0) & 3);
            return new byte[] { system, gyro, acc, mag };
        }

        public void Read(string empty)
        {
            float[] position = Read();
            string msg = $"Hading: {position[0]}deg,\r\nRoll: {position[1]}deg,\r\nPitch: {position[2]}deg";
            sendMessage(msg);
        }

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

        internal float ReadHeading()
        {
            byte[] data = bno.ReadRegisters(0x1a, 2);
            float result = (short)((data[1] << 8) | data[0]) / representationInLSB;
            return result;
        }

        private void OnHeadingChanged()
        {
            headingChanged?.Invoke(this, null);
        }

        public void RegisterForHeadingChanged(EventHandler handler)
        {
            headingChanged += handler;
        }

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
    }
}
