using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace BsuirScheduleLib.BsuirApi.LastUpdate
{
    public static class Loader
    {
        public static async Task<LastUpdate> Load(string group)
        {
            string url = string.Format(Constants.lastUpdateFormat, group);
            return JsonConvert.DeserializeObject<LastUpdate>(await Utils.LoadString(url));
        }
    }
}
