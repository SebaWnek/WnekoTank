using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WnekoTankMeadow.Others;

namespace WnekoTankMeadow.Sensors
{
    class INA219Array
    {
        INA219[] inas;
        Buzzer buzzer;
        DisplayLCD display;
        float warningVoltage = 14f;
        float dischargeVoltage = 13.5f;
        float disconnectedTreshold = 2f;
        int delay;
        bool isMeasuring;
        bool isDischarged = false;
        bool[] signaled = new bool[3];
        Action emergencyDisable;
        ITankCommunication communication;

        CancellationTokenSource source;

        public INA219Array(INA219[] iNA219s, Buzzer buz, DisplayLCD dis, Action disable, ITankCommunication com, int del = 5000)
        {
            inas = iNA219s;
            buzzer = buz;
            display = dis;
            delay = del;
            emergencyDisable = disable;
            communication = com;
            source = new CancellationTokenSource();
        }

        public void StartMonitoringVoltage()
        {
            if (isMeasuring) return;
            isMeasuring = true;
            Thread worker = new Thread(() =>
            {
                CancellationToken token = source.Token;
                string[] tmp = new string[inas.Length + 1];
                float[] voltages = new float[3];
                while (true)
                {
                    if (token.IsCancellationRequested) break;
                    tmp[0] = !isDischarged ? $"Last meas: {DateTime.Now.ToLongTimeString()}" : "!BATTERY DISCHARGED!";
#if DEBUG
                    Console.WriteLine(tmp[0]);
#endif
                    for (int i = 0; i < inas.Length; i++)
                    {
                        voltages[i] = inas[i].ReadBusVoltage();
                        tmp[i + 1] = $"{inas[i].Name}:{voltages[i].ToString("n1")}V,{inas[i].ReadCurrent().ToString("n2")}A,{inas[i].ReadPower().ToString("n2")}W";
#if DEBUG
                        Console.WriteLine(tmp[i + 1]);
#endif
                    }
                    display.WriteMultipleLines(tmp);
                    CheckVoltages(voltages);
                    Thread.Sleep(delay);
                }
            });
            worker.Start();
        }

        private void CheckVoltages(float[] voltages)
        {
            for (int i = 0; i < voltages.Length; i++)
            {
                if (voltages[i] < warningVoltage && !signaled[i] && voltages[i] > disconnectedTreshold)
                {
                    signaled[i] = true;
                    SignalWarning();
                }
                if (voltages[i] < dischargeVoltage && !isDischarged && voltages[i] > disconnectedTreshold)
                {
                    SignalDischarge();
                    EmergencyDisable();
                    isDischarged = true;
                }
            }
        }

        private void EmergencyDisable()
        {
            emergencyDisable.Invoke();
        }

        public void StopMonitoringVoltage()
        {
            if (!isMeasuring) return;
            source.Cancel();
            source.Dispose();
            source = new CancellationTokenSource();
        }

        private void SignalWarning()
        {
            buzzer.BuzzPulse(200, 800, 5);
            string msg = "Low Battery!\n\nLow Battery!\n\nLow Battery!";
            communication.SendMessage(msg);
        }

        private void SignalDischarge()
        {
            buzzer.BuzzPulse(500, 500, int.MaxValue);
            string msg = "Battery discharged!\nCharge ASAP!\n\nBattery discharged!\nCharge ASAP!\n\nBattery discharged!\nCharge ASAP!\n\nBattery discharged!\nCharge ASAP!";
            communication.SendMessage(msg);
        }

        public void ReturnData(string empty)
        {
            int count = inas.Length;
            float[] voltages = new float[count];
            float[] currents = new float[count];
            float[] powers = new float[count];
            string msg = "Electric data:\n";
            for (int i = 0; i < count; i++)
            {
                voltages[i] = inas[i].ReadBusVoltage();
                currents[i] = inas[i].ReadCurrent();
                powers[i] = inas[i].ReadPower();
                msg += inas[i].Name + ":" + voltages[i] + "V;" + currents[i] + "A;" + powers[i] + "W\n";
            }
            communication.SendMessage(msg);
        }
    }
}
