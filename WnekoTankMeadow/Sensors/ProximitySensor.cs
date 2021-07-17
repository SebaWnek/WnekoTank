using Meadow.Hardware;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WnekoTankMeadow.Sensors
{
    enum Direction
    {
        Forward,
        Backward
    }

    enum StopBehavior
    {
        None,
        Stop,
        SoftStop,
        StopAndReturn
    }

    class ProximitySensor
    {
        Direction direction;
        StopBehavior behavior;
        string position;
        IDigitalInputPort port;
        MethodsQueue queue;
        MotorController motor;
        Action stopAction;
        float returnDistance = 0.4f;
        int returnSpeed = 20;

        EventHandler<string> stopEvent;

        internal StopBehavior Behavior
        {
            get => behavior;
            set
            {
                behavior = value;
                SetBehavior();
#if DEBUG
                Console.WriteLine($"{behavior} set on {position}");
#endif
            }
        }

        public ProximitySensor(IDigitalInputPort p, Direction dir, StopBehavior beh, string pos, MethodsQueue q, MotorController m)
        {
            port = p;
            direction = dir;
            behavior = beh;
            position = pos;
            queue = q;
            motor = m;
            SetBehavior();
            port.Changed += Port_Changed;
        }

        private void Port_Changed(object sender, DigitalInputPortEventArgs e)
        {
            Task handler = new Task(() =>
            {
                stopAction?.Invoke();
                stopEvent?.Invoke(this, $"{position} detected obstacle");
            });
            handler.Start();
#if DEBUG
            Console.WriteLine($"{position} detected obstacle");
#endif
        }

        private void SetBehavior()
        {
            switch (behavior)
            {
                case StopBehavior.None:
                    stopAction = null;
                    break;
                case StopBehavior.Stop:
                    stopAction = Stop;
                    break;
                case StopBehavior.SoftStop:
                    stopAction = SoftStop;
                    break;
                case StopBehavior.StopAndReturn:
                    stopAction = StopAndReturn;
                    break;
                default:
                    throw new Exception("Unknown behavior");
            }
        }

        public void Register(EventHandler<string> eventHandler)
        {
            stopEvent += eventHandler;
        }

        public void SetBehavior(string args)
        {
            Behavior = (StopBehavior)int.Parse(args);
        }

        private void Stop()
        {
            queue.StopInvoking();
            queue.ClearQueue();
            motor.Break();
        }

        private void SoftStop()
        {
            queue.StopInvoking();
            queue.ClearQueue();
            motor.SoftBreak();
        }

        private void StopAndReturn()
        {
            queue.StopInvoking();
            queue.ClearQueue();
            motor.Break();
            if (direction == Direction.Forward) motor.MoveForwardBy(-1 * returnSpeed, returnDistance, true, 1);
            else motor.MoveForwardBy(returnSpeed, returnDistance, true, 1);
        }
    }
}
