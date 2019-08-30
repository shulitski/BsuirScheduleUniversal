using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BsuirScheduleLib.BsuirApi.Schedule
{
    public class ScheduleQuery
    {
        public string Value;
        internal bool IsGroup = true;
        public string Employee { set {Value = value; IsGroup = false; } }

    }
}
