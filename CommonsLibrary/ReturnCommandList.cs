using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonsLibrary
{
    public static class ReturnCommandList
    {
        /// <summary>
        /// float yaw ; float roll ; float pitch
        /// </summary>
        public static string positionData = "PDT";

        /// <summary>
        /// for each battery:
        /// string name ; float voltage ; float current ; float power ; 
        /// semicolon at the end so can sum more lines into one 
        /// </summary>
        public static string electricData = "EDT";

        /// <summary>
        /// byte system ; byte gyroscope ; byte magnetic ; byte accelerometer
        /// </summary>
        public static string calibrationData = "CDT";

        /// <summary>
        /// float temperature ; float humidity 
        /// </summary>
        public static string tempHumidData = "TDT";

        /// <summary>
        /// TBD
        /// </summary>
        public static string diagnosticData = "DDT";

        /// <summary>
        /// Estring exception message + exceptionTrace + string exception trace
        /// </summary>
        public static string exception = "EXC";

        /// <summary>
        /// string exception trace
        /// to be used with exception
        /// </summary>
        public static string exceptionTrace = "EXT";

        /// <summary>
        /// string acknowledged command
        /// </summary>
        public static string acknowledge = "ACK";

        /// <summary>
        /// string battery name ; float voltage
        /// </summary>
        public static string lowBattery = "BAL";

        /// <summary>
        /// string battery name ; float voltage
        /// </summary>
        public static string dischargedBattery = "BAD";

        /// <summary>
        /// empty
        /// </summary>
        public static string handShake = "HSK";
    }
}
