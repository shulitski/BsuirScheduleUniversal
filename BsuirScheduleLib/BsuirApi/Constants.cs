using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BsuirScheduleLib.BsuirApi
{
    public static class Constants
    {
        private const string domain = "https://journal.bsuir.by";
        public const string studentScheduleFormat = domain + "/api/v1/studentGroup/schedule?studentGroup={0}";
        public const string lastUpdateFormat = domain + "/api/v1/studentGroup/lastUpdateDate?studentGroup={0}";
    }
}
