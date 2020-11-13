using Meadow.Foundation;
using Meadow.Foundation.Leds;
using Meadow.Hardware;
using System;
using System.Linq;
using System.Messaging;
using System.Text;
using System.Threading;

namespace WnekoTankMeadow
{
    internal class ComController
    {
        ISerialMessagePort port;
        MotorController motor;

        public ComController(ISerialMessagePort p, MotorController m)
        {
            port = p;
            port.Open();
            port.MessageReceived += Port_MessageReceived;
            motor = m;
        }

        private void Port_MessageReceived(object sender, SerialMessageData e)
        {
            string msg = Encoding.UTF8.GetString(e.Message);
            port.Write(Encoding.UTF8.GetBytes($"ACK:{msg}"));  //There is small issue that messages contain LF at the end and it's sent back too
                                                               //but nah, in target use it won't matter
            ExecuteCommand(msg);
        }

        private void ExecuteCommand(string msg)
        {
            string command = msg.Substring(0, 3);
            string arguments = msg.Substring(3);
            switch (command)
            {
                case "GEA":
                    motor.SetGear(int.Parse(arguments));
                    break;
                case "SPD":
                    motor.SetLinearSpeed(int.Parse(arguments));
                    break;
                case "TRN":
                    motor.SetTurn(int.Parse(arguments));
                    break;
                case "STP":
                    motor.Break();
                    break;
                default: 
                    port.Write(Encoding.UTF8.GetBytes($"Unknown command {msg} - {command}, {arguments}"));
                break;
            }
        }
    }
}