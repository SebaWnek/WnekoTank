using CommonsLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WnekoTankMeadow.Others;

namespace WnekoTankMeadow.Sensors
{
    /// <summary>
    /// Array of all voltage and current sensors INA219, so all batteries can be monitored together
    /// </summary>
    class INA219Array
    {
        INA219[] inas;
        Buzzer buzzer;
        DisplayLCD display;
        float warningVoltage = 13.5f;
        float dischargeVoltage = 13.0f;
        float disconnectedTreshold = 2f;
        int delay;
        int previousBatteriesConnected = 0;
        int batteriesConnected = 0;
        byte[] batteries = { 0, 0, 0 };
        bool isMeasuring;
        bool isDischarged = false;
        bool[] signaled = new bool[3];
        bool shouldSend = true;
        Action emergencyDisable;
        Action<string> sendMessage;
        Action<string> setFanState;
#if DEBUG
        bool printToConsole = false;
#endif

        CancellationTokenSource source;

        /// <summary>
        /// Main constructor
        /// </summary>
        /// <param name="iNA219s">Array of objects representing sensors</param>
        /// <param name="buz">Buzzer to send sound signals</param>
        /// <param name="dis">Display to show current data</param>
        /// <param name="disable">Action to disable device if needed</param>
        /// <param name="del">Time between each electric reads</param>
        public INA219Array(INA219[] iNA219s, Buzzer buz, DisplayLCD dis, Action disable, int del = 5000)
        {
            inas = iNA219s;
            buzzer = buz;
            display = dis;
            delay = del;
            emergencyDisable = disable;
            source = new CancellationTokenSource();
        }

        public void StartMonitoringVoltage()
        {
            if (isMeasuring) return;
            isMeasuring = true;
            string msg = "";
            Thread worker = new Thread(() =>
            {
                CancellationToken token = source.Token;
                string[] tmp = new string[inas.Length + 1];
                float[] voltages = new float[3];
                float[] currents = new float[3];
                float[] powers = new float[3];
                while (true)
                {
                    if (token.IsCancellationRequested) break;
                    tmp[0] = !isDischarged ? $"Last meas: {DateTime.Now.ToLongTimeString()}" : "!BATTERY DISCHARGED!";
#if DEBUG
                    if (printToConsole)
                    {
                        Console.WriteLine(tmp[0]); 
                    }
#endif
                    if (shouldSend) msg = ReturnCommandList.electricData;
                    for (int i = 0; i < inas.Length; i++)
                    {
                        voltages[i] = inas[i].ReadBusVoltage();
                        currents[i] = inas[i].ReadCurrent();
                        powers[i] = inas[i].ReadPower();
                        tmp[i + 1] = $"{inas[i].Name}:{voltages[i]:n1}V,{currents[i]:n2}A,{powers[i]:n2}W";
                        if (shouldSend)
                        {
                            msg += $"{inas[i].Name};{voltages[i]:n2};{currents[i]:n2};{powers[i]:n2};";
                        }
#if DEBUG
                        if (printToConsole)
                        {
                            Console.WriteLine(tmp[i + 1]); 
                        }
#endif
                        if (voltages[i] > disconnectedTreshold) batteries[i] = 1;
                        else batteries[i] = 0;
                    }
                    if (shouldSend) sendMessage.Invoke(msg);
                    batteriesConnected = batteries[0] + batteries[1] + batteries[2];
                    if (batteriesConnected > 1 && previousBatteriesConnected <= 1) setFanState("1");
                    else if (batteriesConnected <= 1 && previousBatteriesConnected > 1) setFanState("0");
                    previousBatteriesConnected = batteriesConnected;
#if DEBUG                    
                    if (printToConsole)
                    {
                        Console.WriteLine($"Batteries: {batteries[0] + batteries[1] + batteries[2]}");
                    }
#endif
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
                    SignalWarning(i, voltages[i]);
                }
                if (voltages[i] < dischargeVoltage && !isDischarged && voltages[i] > disconnectedTreshold)
                {
                    SignalDischarge(i, voltages[i]);
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

        private void SignalWarning(int batNumber, float voltage)
        {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            buzzer.BuzzPulse(200, 800, 5);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            string msg = ReturnCommandList.lowBattery + $"{inas[batNumber].Name};{voltage}";
            sendMessage(msg);
        }

        private async void SignalDischarge(int batNumber, float voltage)
        {
            string msg = ReturnCommandList.dischargedBattery + $"{inas[batNumber].Name};{voltage}";
            sendMessage(msg);
            await buzzer.BuzzPulse(500, 500, 5);
        }

        public void ReturnData(string empty)
        {
            int count = inas.Length;
            float[] voltages = new float[count];
            float[] currents = new float[count];
            float[] powers = new float[count];
            string msg = ReturnCommandList.electricData;
            for (int i = 0; i < count; i++)
            {
                voltages[i] = inas[i].ReadBusVoltage();
                currents[i] = inas[i].ReadCurrent();
                powers[i] = inas[i].ReadPower();
                msg += inas[i].Name + ";" + voltages[i].ToString("n2") + ";" + currents[i].ToString("n2") + ";" + powers[i].ToString("n2") + ";";
            }
            sendMessage(msg);
        }

        public void ChangeSending(string args)
        {
            if (args.StartsWith("1")) shouldSend = true;
            else shouldSend = false;
        }

        public void ChangeTimeDelta(string time)
        {
            int t = int.Parse(time);
            ChangeTimeDelta(t);
        }

        private void ChangeTimeDelta(int t)
        {
            delay = t;
        }

        internal void RegisterSender(Action<string> sender)
        {
            sendMessage += sender;
        }

        internal void RegisterFan(Action<string> fanSetter)
        {
            setFanState += fanSetter;
        }

        internal void SetElectricDataDelay(string args)
        {
            int d = int.Parse(args);
            delay = d;
        }
    }
}
