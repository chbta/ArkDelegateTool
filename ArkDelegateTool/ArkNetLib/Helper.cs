using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArkDelegateToolLib
{
    public class Helper
    {
        /// <summary>
        /// Converts an Ark balance string to a double value.
        /// </summary>
        public static double convertBalance(string balance)
        {
            double result = Double.Parse(balance) / 100000000;
            return result;
        }

        /// <summary>
        /// Converts an Ark timestamp to a DateTime object.
        /// </summary>
        public static DateTime convertTime(int time)
        {
            return new DateTime(2017, 3, 21, 13, 0, 0, DateTimeKind.Utc).AddSeconds(time).ToLocalTime();
        }

        /// <summary>
        /// Converts a DateTime object to an integer value for use with the Ark time.
        /// </summary>
        public static int convertTime(DateTime time)
        {
            return (int)Math.Round(time.ToUniversalTime().Subtract(new DateTime(2017, 3, 21, 13, 0, 0, DateTimeKind.Utc)).TotalSeconds);
        }
    }
}
