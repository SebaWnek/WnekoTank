using System;

namespace WnekoTankMeadow
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