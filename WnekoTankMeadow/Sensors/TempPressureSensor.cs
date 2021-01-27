using Meadow.Foundation.Sensors.Atmospheric;
using Meadow.Hardware;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WnekoTankMeadow.Sensors
{
    class TempPressureSensor
    {
        Bme280 sensor;
        ITankCommunication communication;

        public TempPressureSensor(ITankCommunication com, II2cBus bus, Bme280.I2cAddress address = Bme280.I2cAddress.Adddress0x76)
        {
            sensor = new Bme280(bus, address);
            communication = com;
        }

        public string GetTemperaturePreasure()
        {
            var readings = sensor.Read();
            string msg = readings.Result.Temperature + ";" + readings.Result.Pressure;
#if DEBUG
            Console.WriteLine(msg);
#endif
            return msg;
        }

        internal void Read(string oemptybj)
        {
            string msg = GetTemperaturePreasure();
            communication.SendMessage(msg);
        }
    }
}
