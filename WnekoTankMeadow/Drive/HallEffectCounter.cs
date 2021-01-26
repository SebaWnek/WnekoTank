using Meadow.Hardware;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WnekoTankMeadow.Drive
{
    class HallEffectCounter
    {
        IDigitalInputPort port;
        private int counter = 0;
        private int targetCount = -1;
        public int Counter { get => counter; }
        public string Name { get; set; }

        EventHandler LimitReached;
        EventHandler<int> CountChanged;

        public HallEffectCounter(IDigitalInputPort p)
        {
            port = p;
            port.Changed += Port_Changed;
        }


        private void Port_Changed(object sender, DigitalInputPortEventArgs e)
        {
            counter++;
            OnCountChanged();
            if (counter == targetCount) OnLimitReached();
        }

        private void ResetCount()
        {
            counter = 0;
        }

        private void OnCountChanged()
        {
            CountChanged?.Invoke(this, counter);
        }

        private void OnLimitReached()
        {
            LimitReached?.Invoke(this, null);
        }

        public void DisableTarget()
        {
            targetCount = -1;
        }

        public void SetCounter(int target)
        {
            ResetCount();
            targetCount = target;
        }

        public void RegisterForCount(EventHandler<int> handler)
        {
            CountChanged += handler;
        }

        public void RegisterForLimitReached(EventHandler handler)
        {
            LimitReached += handler;
        }
        public void RegisterForLimitReached(EventHandler handler, int count)
        {
            SetCounter(count);
            LimitReached += handler;
        }
    }
}
