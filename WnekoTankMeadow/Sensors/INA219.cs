using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Meadow;
using Meadow.Foundation;
using Meadow.Hardware;
using Meadow.Peripherals;

namespace WnekoTankMeadow.Sensors
{
    /// <summary>
    /// Voltage, current and power electric sensor
    /// </summary>
    class INA219
    {
        /// <summary>
        /// Register addresses
        /// </summary>
        private enum RegisterAddresses
        {
            Configuration = 0x0,
            ShuntVoltage = 0x1,
            BusVoltage = 0x2,
            Power = 0x3,
            Current = 0x4,
            Calibration = 0x5
        }

        /// <summary>
        /// Analog-digital converter settings 
        /// </summary>
        public enum ADCsettings
        {
            Mode9 = 0b0000,
            Mode10 = 0b0001,
            Mode11 = 0b0010,
            Mode12 = 0b0011,
            Samples1 = 0b1000,
            Samples2 = 0b1001,
            Samples4 = 0b1010,
            Samples8 = 0b1011,
            Samples16 = 0b1100,
            Samples32 = 0b1101,
            Samples64 = 0b1110,
            Samples128 = 0b1111
        }

        /// <summary>
        /// Operating mode settings 
        /// </summary>
        public enum ModeSettings
        {
            powerDown = 0b000,
            ShuntVoltageTriggered = 0b001,
            BusVoltageTriggered = 0b010,
            ShuntBusTriggered = 0b011,
            ADCOff = 0b100,
            ShuntVoltageContinuous = 0b101,
            BusVoltageContinuous = 0b110,
            ShuntBusContinuous = 0b111
        }

        /// <summary>
        /// Bus voltage range setting
        /// </summary>
        public enum BusVoltageRangeSettings
        {
            range16v = 0,
            range32v = 1
        }

        /// <summary>
        /// Gain settings
        /// </summary>
        public enum PGASettings
        {
            Gain40mV = 0b00,
            Gain80mV = 0b01,
            Gain160mV = 0b10,
            Gain320mV = 0b11
        }

        I2cPeripheral ina219;
        INA219Configuration configuration;

        float scaleFactor = 0.04096f; //magic number from datasheet
        ushort LSBdivider = 32768;

        float shuntVoltageLSB = 10f / 1000000f;
        float busVoltageLSB = 4f / 1000f;
        float calibrationScalingFactor;
        float maximumExpectedCurrent;
        float shuntResistance;
        ushort calibration;
        float currentLSB;
        float powerLSB;
        bool writeDataToConsole = false; //Should electric data be written to console?

        public string Name { get; set; }

        /// <summary>
        /// Basic constructor
        /// </summary>
        /// <param name="bus">I2C bus</param>
        /// <param name="address">I2C device address</param>
        /// <param name="config">Device configuration object</param>
        /// <param name="name">Device name to distinct easily between physical devices</param>
        public INA219(II2cBus bus, byte address = 0x40, INA219Configuration config = null, string name = "")
        {
            ina219 = new I2cPeripheral(bus, address);
            Name = name;
            if (config == null) configuration = new INA219Configuration();
            else
            {
                configuration = config;
                Configure();
            }
        }

        /// <summary>
        /// Constructor taking also calibration data
        /// </summary>
        /// <param name="maxCurrent">Maximum expected current</param>
        /// <param name="shuntResistor">Shunt resistor resistance value</param>
        /// <param name="calibraitonScaling">Scaling factor if needed</param>
        /// <param name="bus">I2C bus</param>
        /// <param name="address">I2C device address</param>
        /// <param name="config">Device configuration object</param>
        /// <param name="name">Device name to distinct easily between physical devices</param>
        public INA219(float maxCurrent, float shuntResistor, float calibraitonScaling, II2cBus bus, byte address = 0x40, INA219Configuration config = null, string name = "")
            : this(bus, address, config, name)
        {
            maximumExpectedCurrent = maxCurrent;
            shuntResistance = shuntResistor;
            calibrationScalingFactor = calibraitonScaling;
            Calibrate();
        }

        /// <summary>
        /// Converts current calibration data in the object into binary form and writes to device
        /// </summary>
        private void Calibrate()
        {
            currentLSB = maximumExpectedCurrent / LSBdivider;
            powerLSB = 20 * currentLSB;
            calibration = (ushort)(scaleFactor / (currentLSB * shuntResistance));
#if DEBUG
            Console.WriteLine($"LSB: {currentLSB}, calibration: {calibration}");
#endif
            ina219.WriteUShorts((byte)RegisterAddresses.Calibration, new ushort[] { calibration }, ByteOrder.BigEndian);
        }

        /// <summary>
        /// Writed calibration data into object and invokes writing it to device
        /// </summary>
        /// <param name="maxCurrent">Maximum expected current</param>
        /// <param name="shuntResistor">Shunt resistor resistance value</param>
        /// <param name="calibraitonScaling">Scaling factor if needed</param>
        public void Calibrate(float maxCurrent, float shuntResistor, float calibraitonScaling = 1)
        {
            maximumExpectedCurrent = maxCurrent;
            shuntResistance = shuntResistor;
            calibrationScalingFactor = calibraitonScaling;
            Calibrate();
        }

        /// <summary>
        /// Writes binary calibration data to device
        /// </summary>
        /// <param name="calibration">Calibration binary data</param>
        public void Calibrate(ushort calibration)
        {
            ina219.WriteUShorts((byte)RegisterAddresses.Calibration, new ushort[] { calibration }, ByteOrder.BigEndian);
        }

        /// <summary>
        /// Reads current shunt voltage
        /// </summary>
        /// <returns>Shunt voltage</returns>
        public float ReadShuntVltage()
        {
            short register = (short)ina219.ReadUShorts((byte)RegisterAddresses.ShuntVoltage, 1, ByteOrder.BigEndian)[0];
#if DEBUG
            if (writeDataToConsole)
            {
                Console.WriteLine();
                Console.WriteLine(Convert.ToString(register, 2).PadLeft(16, '0'));
                Console.WriteLine(ina219.ReadRegister((byte)RegisterAddresses.ShuntVoltage));
                Console.WriteLine(Convert.ToString(register, 10));
                Console.WriteLine();
            }
#endif
            float voltage = register * shuntVoltageLSB;
            return voltage;
        }

        /// <summary>
        /// Reads current bus voltage
        /// </summary>
        /// <returns>Bus voltage</returns>
        public float ReadBusVoltage()
        {
            ushort register = ina219.ReadUShorts((byte)RegisterAddresses.BusVoltage, 1, ByteOrder.BigEndian)[0];
#if DEBUG
            if (writeDataToConsole)
            {
                Console.WriteLine();
                Console.WriteLine(Convert.ToString(register, 2).PadLeft(16, '0'));
                Console.WriteLine(ina219.ReadRegister((byte)RegisterAddresses.BusVoltage));
            }
#endif
            register = (ushort)(register >> 3);
#if DEBUG
            if (writeDataToConsole)
            {
                Console.WriteLine(Convert.ToString(register, 2).PadLeft(16, '0'));
                Console.WriteLine(Convert.ToString(register, 10));
                Console.WriteLine(); 
            }
#endif
            float voltage = register * busVoltageLSB;
            return voltage;
        }

        /// <summary>
        /// Reads current
        /// </summary>
        /// <returns>Current current</returns>
        public float ReadCurrent()
        {
            short register = (short)ina219.ReadUShorts((byte)RegisterAddresses.Current, 1, ByteOrder.BigEndian)[0];
#if DEBUG
            if (writeDataToConsole)
            {
                Console.WriteLine();
                Console.WriteLine(Convert.ToString(register, 2).PadLeft(16, '0'));
                Console.WriteLine(); 
            }
#endif
            float current = register * currentLSB;
            return current;
        }

        /// <summary>
        /// Reads power
        /// </summary>
        /// <returns>Current power</returns>
        public float ReadPower()
        {
            ushort register = ina219.ReadUShorts((byte)RegisterAddresses.Power, 1, ByteOrder.BigEndian)[0];
#if DEBUG
            if (writeDataToConsole)
            {
                Console.WriteLine();
                Console.WriteLine(Convert.ToString(register, 2).PadLeft(16, '0'));
                Console.WriteLine(); 
            }
#endif
            float power = register * powerLSB;
            return power;
        }

#if DEBUG
        /// <summary>
        /// Enumerates all registers' contents and sends them to conttoll app and writes them to console. Only in Debug mode with PC connected to Meadow's USB
        /// </summary>
        public void EnumerateRegisters()
        {
            Console.WriteLine("Enumerating:");
            for (byte i = 0; i <= 5; i++)
            {
                ushort tmp = ina219.ReadUShorts(i, 1, ByteOrder.BigEndian)[0];
                Console.WriteLine(Convert.ToString(tmp, 2).PadLeft(16, '0') + " - " + Convert.ToString(tmp, 16) + " - " + tmp);
            }
        }
#endif

        /// <summary>
        /// Saves new configuration data in object and invokes writing it to device
        /// </summary>
        /// <param name="configuration">Configuration settings to be written</param>
        public void Configure(INA219Configuration configuration)
        {
            this.configuration = configuration;
            Configure();
        }


        /// <summary>
        /// Creates configuration binary data from current settings and writes it to device
        /// </summary>
        public void Configure()
        {
            int config = 0;
            config |= (byte)configuration.Mode;
            config |= (byte)configuration.ShuntADC << 3;
            config |= (byte)configuration.BusADC << 7;
            config |= (byte)configuration.Pga << 11;
            config |= (byte)configuration.BusVoltageRange << 13;
#if DEBUG
            Console.WriteLine($"Writing configuration:\n{Convert.ToString(config, 16)}\n{Convert.ToString(config, 2).PadLeft(16, '0')}");
#endif
            ina219.WriteUShorts((byte)RegisterAddresses.Configuration, new ushort[] { (ushort)config }, ByteOrder.BigEndian);
        }

        /// <summary>
        /// Writes configuration data to respective device register
        /// </summary>
        /// <param name="config">Configuration data</param>
        public void Configure(ushort config)
        {
            ina219.WriteUShorts((byte)RegisterAddresses.Configuration, new ushort[] { (ushort)config }, ByteOrder.BigEndian);
        }

        /// <summary>
        /// Writes reset byte that causes device to reset to factory default settings
        /// </summary>
        public void ResetToFactory()
        {
            //Configure(new INA219Configuration());
            ina219.WriteUShorts((byte)RegisterAddresses.Configuration, new ushort[] { 0b1000000000000000 }, ByteOrder.BigEndian);
        }

        /// <summary>
        /// Class containing sensor configuration for easier preparing configuration data to be written to device
        /// </summary>
        internal class INA219Configuration
        {
            BusVoltageRangeSettings busVoltageRange;
            PGASettings pga;
            ADCsettings busADC;
            ADCsettings shuntADC;
            ModeSettings mode;
            public INA219Configuration()
            {
                busVoltageRange = BusVoltageRangeSettings.range32v;
                pga = PGASettings.Gain320mV;
                busADC = ADCsettings.Mode12;
                shuntADC = ADCsettings.Mode12;
                mode = ModeSettings.ShuntBusContinuous;
            }
            public INA219Configuration(BusVoltageRangeSettings busVoltageRange, PGASettings pga, ADCsettings busADC, ADCsettings shuntADC, ModeSettings mode)
            {
                this.busVoltageRange = busVoltageRange;
                this.pga = pga;
                this.busADC = busADC;
                this.shuntADC = shuntADC;
                this.mode = mode;
            }

            internal BusVoltageRangeSettings BusVoltageRange { get => busVoltageRange; set => busVoltageRange = value; }
            internal PGASettings Pga { get => pga; set => pga = value; }
            internal ADCsettings BusADC { get => busADC; set => busADC = value; }
            internal ADCsettings ShuntADC { get => shuntADC; set => shuntADC = value; }
            internal ModeSettings Mode { get => mode; set => mode = value; }
        }
    }
}
