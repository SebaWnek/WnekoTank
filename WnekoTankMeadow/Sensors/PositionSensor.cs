using Meadow.Foundation.Sensors.Motion;
using Meadow.Hardware;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WnekoTankMeadow.Drive
{
    class PositionSensor
    {
        Bno055 sensor;
        ITankCommunication communication;
        I2cPeripheral bno;

        public PositionSensor(ITankCommunication com, II2cBus bus, byte address = 40)
        {
            sensor = new Bno055(bus, address);
            sensor.OperatingMode = Bno055.OperatingModes.NineDegreesOfFreedom;
            communication = com;
            bno = new I2cPeripheral(bus, address);
        }

        public void Calibrate(string empty)
        {
            Calibrate();
        }

        public void Calibrate()
        {
            for (int i = 0; i < 60; i++) //Timeout after one minute
            {
                byte[] cal = CheckCalibration();
                communication.SendMessage($"S: {cal[0]}, G: {cal[1]}, A: {cal[2]}, M: {cal[3]}");
#if DEBUG
                Console.WriteLine($"S: {cal[0]}, G: {cal[1]}, A: {cal[2]}, M: {cal[3]}");
#endif
                if (cal[0] == 3 && cal[1] == 3 && cal[2] == 3 && cal[3] == 3)
                {
                    Thread.Sleep(100);
                    communication.SendMessage($"Done! S: {cal[0]}, G: {cal[1]}, A: {cal[2]}, M: {cal[3]}");
#if DEBUG
                    Console.WriteLine($"Done! S: {cal[0]}, G: {cal[1]}, A: {cal[2]}, M: {cal[3]}");
#endif
                    return;
                }
                Thread.Sleep(1000);
            }
            communication.SendMessage("Not calibrated!");
#if DEBUG
            Console.WriteLine("Not calibrated!");
#endif
        }

        public void CheckCalibration(string empty)
        {
            byte[] cal = CheckCalibration();
            communication.SendMessage($"S: {cal[0]}, G: {cal[1]}, A: {cal[2]}, M: {cal[3]}");
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

        internal void Read(string empty)
        {
            throw new NotImplementedException();
        }
    }
}
