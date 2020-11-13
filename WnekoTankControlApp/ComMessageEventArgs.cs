using System;

namespace WnekoTankControlApp
{
    internal class ComMessageEventArgs : EventArgs
    { 
        public string Message { get; }
        public ComMessageEventArgs(string msg)
        {
            Message = msg;
        }

    }
}