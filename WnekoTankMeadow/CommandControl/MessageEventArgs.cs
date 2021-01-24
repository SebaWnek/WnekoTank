using System;

namespace WnekoTankMeadow
{
    /// <summary>
    /// Messages to be passed from communication device to handling class
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