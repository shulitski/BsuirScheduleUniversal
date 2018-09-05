using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BsuirScheduleLib.BsuirApi.Schedule
{
    public class Pair
    {
        public List<int> weekNumber { get; set; }
        public List<string> studentGroup { get; set; }
        public List<string> studentGroupInformation { get; set; }
        public int numSubgroup { get; set; }
        public List<string> auditory { get; set; }
        public string lessonTime { get; set; }
        public string startLessonTime { get; set; }
        public string endLessonTime { get; set; }
        public string subject { get; set; }
        public string note { get; set; }
        public string lessonType { get; set; }
        public List<Employee> employee { get; set; }
        public bool zaoch { get; set; }

        public string Auditory => auditory.Aggregate("", (s, s1) => s + s1);
        public string EmployeeName => employee.Count > 0 ? employee.First().fio : "";

    }
}
