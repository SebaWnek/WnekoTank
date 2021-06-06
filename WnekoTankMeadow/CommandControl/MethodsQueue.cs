using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CommonsLibrary;

namespace WnekoTankMeadow
{
    /// <summary>
    /// Class responsible for handling all the communication with PC, queueing requested methods calls 
    /// </summary>
    class MethodsQueue
    {
        MethodsDictionary dict;
        private BlockingCollection<(Action<string>, string)> queue = new BlockingCollection<(Action<string>, string)>();
        private bool isWorking = false;
        CancellationTokenSource tokenSource = new CancellationTokenSource();
        ITankCommunication communication;
        bool isQueueLocked = false;

        /// <summary>
        /// Main constructor
        /// </summary>
        /// <param name="com">Communication device compatoible with ITankCommunication interface to be used to communicate with PC</param>
        /// <param name="d">Methods dictionary used for resolving messages into delegates</param>
        public MethodsQueue(ITankCommunication com, MethodsDictionary d)
        {
            communication = com;
            communication.SubscribeToMessages(MessageReceived);
            dict = d;
            StartInvoking();
        }

        /// <summary>
        /// Decides what to do with incomming message.
        /// Emergency messages are invoked ASAP, to allow special control outside of queue, 
        /// e.g. starting/pausing queue, stopping vehicle etc.,
        /// Normal messages are queued to be invoked in correct order
        /// In RC mode it will happen ASAP, when used with Wait or other methods taking some time it allows to program series of actions
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Message passed from communicaiton device</param>
        private void MessageReceived(object sender, MessageEventArgs e)
        {
            if (e.Message.StartsWith(TankCommandList.emergencyPrefix)) EmergencyInvoke(e.Message.Substring(3));
            else Enqueue(dict.ReturnMethod(e.Message));
        }

        /// <summary>
        /// Invoke called method ASAP, ignoring queue
        /// </summary>
        /// <param name="msg">Message</param>
        private void EmergencyInvoke(string msg)
        {
            dict.InvokeMethod(msg);
        }

        /// <summary>
        /// Adds method to queue
        /// </summary>
        /// <param name="method">Tuple containing method delegate and string with parameters for it to be invoked with</param>
        private void Enqueue((Action<string>, string) method)
        {
            queue.Add(method);
        }

        /// <summary>
        /// Sends list of enqueued methods back to device
        /// </summary>
        /// <param name="empty">Just to be compatible with required method signature</param>
        public void EnumerateQueue(string empty)
        {
            string queue = EnumerateQueue();
            communication.SendMessage(queue);
        }

        /// <summary>
        /// Generates list of enqueued methods
        /// </summary>
        /// <returns>String containing list of enqueued methods</returns>
        private string EnumerateQueue()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("Queued commands:");
            foreach ((Action<string>, string) tuple in queue)
            {
                sb.Append(tuple.Item1.Method.Name + ": " + tuple.Item2);
            }
            return sb.ToString();
        }

        /// <summary>
        /// Clear queue
        /// </summary>
        /// <param name="empty">Just to be compatible with required method signature</param>
        public void ClearQueue(string empty)
        {
            ClearQueue();
        }

        /// <summary>
        /// Clears queue
        /// </summary>
        public void ClearQueue()
        {
            StopInvoking();
            queue = new BlockingCollection<(Action<string>, string)>();
        }

        /// <summary>
        /// Start invoking queued methods
        /// </summary>
        /// <param name="empty">Just to be compatible with required method signature</param>
        public void StartInvoking(string empty)
        {
            StartInvoking();
        }

        /// <summary>
        /// Start invoking queued methods
        /// </summary>
        public void StartInvoking()
        {
            if (isWorking) return;
            if (isQueueLocked) return;
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

        /// <summary>
        /// Stops invoking methods from queue
        /// </summary>
        /// <param name="empty">Just to be compatible with required method signature</param>
        public void StopInvoking(string empty)
        {
            StopInvoking();
        }

        /// <summary>
        /// Stops invoking methods from queue
        /// </summary>
        public void StopInvoking()
        {
            tokenSource.Cancel();
            tokenSource.Dispose();
            tokenSource = new CancellationTokenSource();
            isWorking = false;
        }

        public void LockQueue()
        {
            isQueueLocked = true;
            StopInvoking();
            ClearQueue();
        }
    }
}
