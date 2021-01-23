using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace WnekoTankControlApp
{
    class CommandsList
    {
        private Dictionary<string, string> dict = new Dictionary<string, string>();

        public CommandsList()
        {
            Type type = typeof(WnekoTankMeadow.CommandList);
            FieldInfo[] info = type.GetFields();
            foreach(FieldInfo fieldInfo in info)
            {
                string name = fieldInfo.Name;
                string value = (string)fieldInfo.GetValue(null);
                dict.Add(name, value);
            }
        }

        public string GetCode(string method)
        {
            return dict[method];
        }
    }
}
