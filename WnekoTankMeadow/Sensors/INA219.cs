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
    class INA219
    {
        private enum RegisterAddresses
        {
            Configuration = 0x0,
            ShuntVoltage = 0x1,
            BusVoltage = 0x2,
            Power = 0x3,
            Current = 0x4,
            Calibration = 0x5
        }

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

        public enum BusVoltageRangeSettings
        {
            range16v = 0,
            range32v = 1
        }

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

        float shuntVoltageCorrection = 0f;
        float busVoltageCorrection = 0.9120001f;

        float shuntVoltageLSB = 10f / 1000000f;
        float busVoltageLSB = 4f / 1000f;
        float calibrationScalingFactor;
        float maximumExpectedCurrent;
        float shuntResistance;
        ushort calibration;
        float currentLSB;
        float powerLSB;

        public string Name { get; set; }

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

        public INA219(float maxCurrent, float shuntResistor, float calibraitonScaling, II2cBus bus, byte address = 0x40, INA219Configuration config = null, string name = "")
            : this(bus, address, config, name)
        {
            maximumExpectedCurrent = maxCurrent;
            shuntResistance = shuntResistor;
            calibrationScalingFactor = calibraitonScaling;
            Calibrate();
        }

        private void Calibrate()
        {
            currentLSB = maximumExpectedCurrent / LSBdivider;
            powerLSB = 20 * currentLSB;
            calibration = (ushort)(scaleFactor / (currentLSB * shuntResistance));
#if DEBUG
            Console.WriteLine($"LSB: {currentLSB}, calibration: {calibration}");
#endif
            ina219.WriteUShort((byte)RegisterAddresses.Calibration, calibration, ByteOrder.BigEndian);
        }

        public void Calibrate(float maxCurrent, float shuntResistor, float calibraitonScaling = 1)
        {
            maximumExpectedCurrent = maxCurrent;
            shuntResistance = shuntResistor;
            calibrationScalingFactor = calibraitonScaling;
            Calibrate();
        }

        public void Calibrate(ushort calibration)
        {
            ina219.WriteUShort((byte)RegisterAddresses.Calibration, calibration, ByteOrder.BigEndian);
        }

        public float ReadShuntVltage()
        {
            short register = (short)ina219.ReadUShort((byte)RegisterAddresses.ShuntVoltage, ByteOrder.BigEndian);
//#if DEBUG
//            Console.WriteLine();
//            Console.WriteLine(Convert.ToString(register, 2).PadLeft(16, '0'));
//            Console.WriteLine(ina219.ReadRegister((byte)RegisterAddresses.ShuntVoltage));
//            Console.WriteLine(Convert.ToString(register, 10));
//            Console.WriteLine();
//#endif
            float voltage = register * shuntVoltageLSB;
            return voltage;
        }

        public float ReadBusVoltage()
        {
            ushort register = ina219.ReadUShort((byte)RegisterAddresses.BusVoltage, ByteOrder.BigEndian);
//#if DEBUG
//            Console.WriteLine();
//            Console.WriteLine(Convert.ToString(register, 2).PadLeft(16, '0'));
//            Console.WriteLine(ina219.ReadRegister((byte)RegisterAddresses.BusVoltage));
//#endif
            register = (ushort)(register >> 3);
//#if DEBUG
//            Console.WriteLine(Convert.ToString(register, 2).PadLeft(16, '0'));
//            Console.WriteLine(Convert.ToString(register, 10));
//            Console.WriteLine();
//#endif
            float voltage = register * busVoltageLSB;
            return voltage;
        }

        public float ReadCurrent()
        {
            short register = (short)ina219.ReadUShort((byte)RegisterAddresses.Current, ByteOrder.BigEndian);
//#if DEBUG
//            Console.WriteLine();
//            Console.WriteLine(Convert.ToString(register, 2).PadLeft(16, '0'));
//            Console.WriteLine();
//#endif
            float current = register * currentLSB;
            return current;
        }

        public float ReadPower()
        {
            ushort register = ina219.ReadUShort((byte)RegisterAddresses.Power, ByteOrder.BigEndian);
//#if DEBUG
//            Console.WriteLine();
//            Console.WriteLine(Convert.ToString(register, 2).PadLeft(16, '0'));
//            Console.WriteLine();
//#endif
            float power = register * powerLSB;
            return power;
        }

        public void EnumerateRegisters()
        {
            Console.WriteLine("Enumerating:");
            for (byte i = 0; i <= 5; i++)
            {
                ushort tmp = ina219.ReadUShort(i, ByteOrder.BigEndian);
                Console.WriteLine(Convert.ToString(tmp, 2).PadLeft(16, '0') + " - " + Convert.ToString(tmp, 16) + " - " + tmp);
            }
        }

        public void Configure(INA219Configuration configuration)
        {
            this.configuration = configuration;
            Configure();
        }

        public void Configure()
        {
            int config = 0;
            config = config | (byte)configuration.Mode;
            config = config | ((byte)configuration.ShuntADC << 3);
            config = config | ((byte)configuration.BusADC << 7);
            config = config | ((byte)configuration.Pga << 11);
            config = config | ((byte)configuration.BusVoltageRange << 13);
#if DEBUG
            Console.WriteLine($"Writing configuration:\n{Convert.ToString(config, 16)}\n{Convert.ToString(config, 2).PadLeft(16, '0')}");
#endif
            ina219.WriteUShort((byte)RegisterAddresses.Configuration, (ushort)config, ByteOrder.BigEndian);
        }

        public void Configure(ushort config)
        {
            ina219.WriteUShort((byte)RegisterAddresses.Configuration, (ushort)config, ByteOrder.BigEndian);
        }

        public void ResetToFactory()
        {
            //Configure(new INA219Configuration());
            ina219.WriteUShort((byte)RegisterAddresses.Configuration, 0b1000000000000000, ByteOrder.BigEndian);
        }
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

        //public short CalculateTwosComplementShunt(ushort data)
        //{
        //    if((data & 0b1000000000000000) == 0)
        //    {
        //        return (short)data;
        //    }

        //    ushort mask = 0;
        //    int result;
        //    switch (configuration.Pga)
        //    {
        //        case PGASettings.Gain40mV:
        //            mask = 0b0000111111111111;
        //            break;
        //        case PGASettings.Gain80mV:
        //            mask = 0b0001111111111111;
        //            break;
        //        case PGASettings.Gain160mV:
        //            mask = 0b0011111111111111;
        //            break;
        //        case PGASettings.Gain320mV:
        //            mask = 0b0111111111111111;
        //            break;
        //    }
        //    result = data & mask;
        //    result = (~data + 1) & mask;
        //    return (short)result;
        //}
    }

}
