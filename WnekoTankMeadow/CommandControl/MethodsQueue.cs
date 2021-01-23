using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WnekoTankMeadow
{
    class MethodsQueue
    {
        MethodsDictionary dict; 
        private BlockingCollection<(Action<string>, string)> queue = new BlockingCollection<(Action<string>, string)>();
        private bool isWorking = false;
        CancellationTokenSource tokenSource = new CancellationTokenSource();
        ITankCommunication communication;

        public MethodsQueue(ITankCommunication com, MethodsDictionary d)
        {
            communication = com;
            communication.SubscribeToMessages(MessageReceived);
            dict = d;
            StartInvoking();
        }

        private void MessageReceived(object sender, MessageEventArgs e)
        {
            if (e.Message.StartsWith(CommandList.emergencyPrefix)) EmergencyInvoke(e.Message.Substring(3));
            else Enqueue(dict.ReturnMethod(e.Message));
        }

        private void EmergencyInvoke(string msg)
        {
            dict.InvokeMethod(msg);
        }

        private void Enqueue((Action<string>, string) method)
        {
            queue.Add(method);
        }

        public void EnumerateQueue(string empty)
        {
            string queue = EnumerateQueue();
            communication.SendMessage(queue);
        }

        private string EnumerateQueue()
        {
            StringBuilder sb = new StringBuilder();
            foreach ((Action<string>, string) tuple in queue)
            {
                sb.AppendLine(tuple.Item1.Method.Name + ": " + tuple.Item2);
            }
            return sb.ToString();
        }

        public void ClearQueue(string empty)
        {
            ClearQueue();
        }
        public void ClearQueue()
        {
            StopInvoking();
            queue = new BlockingCollection<(Action<string>, string)>();
        }

        public void StartInvoking(string empty)
        {
            StartInvoking();
        }

        public void StartInvoking()
        {
            if (isWorking) return;
            isWorking = true;
            Thread worker = new Thread(() =>
            {
                CancellationToken token = tokenSource.Token;
                while (true)
                {
                    if (token.IsCancellationRequested) break;
                    (Action<string>, string) method = queue.Take();
                    method.Item1.Invoke(method.Item2);
                }
            });
            worker.Start();
        }

        public void StopInvoking(string empty)
        {
            StopInvoking();
        }

        public void StopInvoking()
        {
            tokenSource.Cancel();
            tokenSource.Dispose();
            tokenSource = new CancellationTokenSource();
            isWorking = false;
        }
    }
}
