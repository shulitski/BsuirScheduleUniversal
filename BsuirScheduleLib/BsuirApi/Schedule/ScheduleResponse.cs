using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BsuirScheduleLib.BsuirApi.Schedule
{
    public class ScheduleResponse
    {
        public List<Schedule> schedules { get; set; }
        public string todayDate { get; set; }
        public int currentWeekNumber { get; set; }
    }
}
