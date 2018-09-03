using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BsuirScheduleLib.BsuirApi.Schedule
{
    public class Schedule
    {
        public string weekday { get; set; }
        public List<Pair> schedule { get; set; }
    }
}
