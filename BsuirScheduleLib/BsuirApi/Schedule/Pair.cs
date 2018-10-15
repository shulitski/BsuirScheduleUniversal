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

        public override bool Equals(object obj)
        {
            var pair = obj as Pair;
            return pair != null &&
                   weekNumber.SequenceEqual(pair.weekNumber) &&
                   studentGroup.SequenceEqual(pair.studentGroup) &&
                   studentGroupInformation.SequenceEqual(pair.studentGroupInformation) &&
                   numSubgroup == pair.numSubgroup &&
                   auditory.SequenceEqual(pair.auditory) &&
                   lessonTime == pair.lessonTime &&
                   startLessonTime == pair.startLessonTime &&
                   endLessonTime == pair.endLessonTime &&
                   subject == pair.subject &&
                   note == pair.note &&
                   lessonType == pair.lessonType &&
                   employee.SequenceEqual(pair.employee) &&
                   zaoch == pair.zaoch;
        }
    }
}
