using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace BsuirScheduleLib.BsuirApi.Employee
{
    public class Loader
    {
        public static async Task<List<Employee>> Load()
        {
            return JsonConvert.DeserializeObject<List<Employee>>(await Utils.LoadString(Constants.employeesUrl));
        }
    }
}
