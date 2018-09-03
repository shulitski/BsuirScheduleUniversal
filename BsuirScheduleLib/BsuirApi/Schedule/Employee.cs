using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BsuirScheduleLib.BsuirApi.Schedule
{
    public class Employee
    {
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string middleName { get; set; }
        public string rank { get; set; }
        public string photoLink { get; set; }
        public string calendarId { get; set; }
        public List<string> academicDepartment { get; set; }
        public int id { get; set; }
        public string fio { get; set; }
    }
}
