using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace BsuirScheduleLib.BsuirApi.Group
{
    public class Loader
    {
        public static async Task<List<Group>> Load()
        {
            return JsonConvert.DeserializeObject<List<Group>>(await Utils.LoadString(Constants.groupsUrl));
        }
    }
}
