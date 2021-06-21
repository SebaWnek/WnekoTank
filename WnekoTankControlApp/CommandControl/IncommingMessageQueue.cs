using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WnekoTankControlApp.CommandControl
{
    class IncommingMessageQueue
    {
        Dictionary<string, Action<string>> commands;
        BlockingCollection<(Action<string>, string)> queue;
        bool isWorking = false;
        CancellationTokenSource source;

        public IncommingMessageQueue()
        {
            source = new CancellationTokenSource();
            commands = new Dictionary<string, Action<string>>();
            queue = new BlockingCollection<(Action<string>, string)>();
            StartInvoking();
        }
        public void IncommingMessageHandler(object sender, MessageEventArgs message)
        {
            string msg = message.Message; 
            if (msg.Length >= 3)
            {

                string command = msg.Substring(0, 3);
                string args = msg.Substring(3);
                if (commands.ContainsKey(command))
                {
                    queue.Add((commands[command], args));
                } 
            }
        }

        public void StartInvoking()
        {
            if (isWorking) return;
            CancellationToken token = source.Token;
            Action<string> command;
            string args;
            Thread worker = new Thread(() =>
            {
                while (true)
                {
                    if (token.IsCancellationRequested) break;
                    (command, args) = queue.Take();
                    command.Invoke(args);
                }
            });
            worker.Start();
        }

        public void RegisterMethod(string command, Action<string> method)
        {
            commands.Add(command, method);
        }

        public void StopInvoking()
        {
            isWorking = false;
            source.Cancel();
            source.Dispose();
            source = new CancellationTokenSource();
        }

        public void ClearQueue()
        {
            bool wasWorking = isWorking;
            if (wasWorking) StopInvoking();
            queue = new BlockingCollection<(Action<string>, string)>();
            if (wasWorking) StartInvoking();
        }
    }
}
