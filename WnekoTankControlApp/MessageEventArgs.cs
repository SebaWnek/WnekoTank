using System;

namespace WnekoTankControlApp
{
    internal class MessageEventArgs : EventArgs
    { 
        public string Message { get; }
        public MessageEventArgs(string msg)
        {
            Message = msg;
        }

    }
}