using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bundle.Client.Helpers
{
    internal class ConvertHelper
    {
        public static int StringToInt(string str)
        {
            try
            {
                return Convert.ToInt32(str.Trim());
            }
            catch
            {
                return 0;
            }
        }
    }
}
