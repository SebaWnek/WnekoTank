using CommonsLibrary;
using Meadow.Foundation.Sensors.Atmospheric;
using Meadow.Hardware;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WnekoTankMeadow.Sensors
{
    /// <summary>
    /// Temperature and preasure sensor BME280
    /// </summary>
    class TempPressureSensor
    {
        Bme280 sensor;
        Action<string> sendMessage;
        /// <summary>
        /// Basic constructor creating Bme280 object
        /// </summary>
        /// <param name="bus">I2C bus</param>
        /// <param name="address">I2C device address</param>
        public TempPressureSensor(II2cBus bus, Bme280.I2cAddress address = Bme280.I2cAddress.Address0x76)
        {
            sensor = new Bme280(bus, address);
        }

        /// <summary>
        /// Constructor creating Bme280 object and assigning communication method
        /// </summary>
        /// <param name="bus">I2C bus</param>
        /// <param name="address">I2C device address</param>
        /// <param name="send">Communication method for sending data to controll app</param>
        public TempPressureSensor(II2cBus bus, Action<string> send, Bme280.I2cAddress address = Bme280.I2cAddress.Address0x76) : this(bus, address)
        {
            sendMessage = send;
        }

        /// <summary>
        /// Register method for sending data do controll app
        /// </summary>
        /// <param name="sender">Communication method</param>
        public void RegisterSender(Action<string> sender)
        {
            sendMessage += sender;
        }

        /// <summary>
        /// Reads temperature and humidity from sensor
        /// </summary>
        /// <returns></returns>
        public string GetTemperaturePreasure()
        {
            var readings = sensor.Read();
            string msg = readings.Result.Temperature + ";" + readings.Result.Pressure;
#if DEBUG
            Console.WriteLine(msg);
#endif
            return msg;
        }

        /// <summary>
        /// Reads current values and sends back to control app
        /// </summary>
        /// <param name="empty">No parameters needed</param>
        internal void Read(string empty)
        {
            string msg = ReturnCommandList.tempHumidData + GetTemperaturePreasure();
            sendMessage(msg);
        }
    }
}
