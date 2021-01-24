using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace WnekoTankControlApp
{
    /// <summary>
    /// Imports communicaiton protocol codes from apropriate vehicle class to make sure that both devices have the same codes,
    /// And also that controll app has it's own copy taken during build, not referencing vehicle classes during runtime, as those migh not be accessible
    /// </summary>
    class CommandsList
    {
        private Dictionary<string, string> dict = new Dictionary<string, string>();

        /// <summary>
        /// Uses reflection to get codes and function names
        /// </summary>
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

        /// <summary>
        /// Returns code based of function name
        /// </summary>
        /// <param name="method">Function name</param>
        /// <returns>Protocol code for selected method</returns>
        public string GetCode(string method)
        {
            return dict[method];
        }
    }
}
