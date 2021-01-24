using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WnekoTankMeadow
{
    /// <summary>
    /// Translates protocol codes into delegates for apropriate methods
    /// </summary>
    class MethodsDictionary
    {
        private Dictionary<string, Action<string>> methods = new Dictionary<string, Action<string>>();

        /// <summary>
        /// Adds new method to dictionary
        /// </summary>
        /// <param name="code">3 letter code</param>
        /// <param name="action">Delegate for selected method</param>
        public void RegisterMetod(string code, Action<string> action)
        {
            methods.Add(code, action);
        }

        /// <summary>
        /// Used to invoke methods ASAP straight from message received
        /// </summary>
        /// <param name="input">Message containing 3 character code and arguments</param>
        public void InvokeMethod(string input)
        {
            string code = input.Substring(0, 3);
            string args = input.Substring(3);
            if (methods.ContainsKey(code)) methods[code].Invoke(args);
            else throw new Exception("Method not in dictionary!");
        }

        /// <summary>
        /// Takes message containing 3 character code and arguments into touple contaiinng method delegate and arguments to be invoked with
        /// </summary>
        /// <param name="input">Message containing 3 character code and arguments</param>
        /// <returns>Tuple containing method delegate and string with arguments for it</returns>
        public (Action<string>, string args) ReturnMethod(string input)
        {
            string code = input.Substring(0, 3);
            string args = input.Substring(3);
            if (methods.ContainsKey(code)) return (methods[code], args);
            else throw new Exception("Method not in dictionary!");
        }
    }
}
