using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WnekoTankMeadow
{
    class MethodsDictionary
    {
        private Dictionary<string, Action<string>> methods = new Dictionary<string, Action<string>>();

        public void RegisterMetod(string code, Action<string> action)
        {
            methods.Add(code, action);
        }

        public void InvokeMethod(string input)
        {
            string code = input.Substring(0, 3);
            string args = input.Substring(3);
            if (methods.ContainsKey(code)) methods[code].Invoke(args);
            else throw new Exception("Method not in dictionary!");
        }

        public (Action<string>, string args) ReturnMethod(string input)
        {
            string code = input.Substring(0, 3);
            string args = input.Substring(3);
            if (methods.ContainsKey(code)) return (methods[code], args);
            else throw new Exception("Method not in dictionary!");
        }
    }
}
