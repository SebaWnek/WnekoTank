using System;

namespace WnekoTankControlApp
{
    /// <summary>
    /// Messages to be passed from communication device to other classes
    /// </summary>
    internal class MessageEventArgs : EventArgs
    { 
        public string Message { get; }
        public MessageEventArgs(string msg)
        {
            Message = msg;
        }

    }
}