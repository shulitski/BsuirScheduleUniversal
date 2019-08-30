using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BsuirScheduleLib.BsuirApi.Group
{
    public class Group
    {
        public string name;
        public int facultyId;
        public string facultyName;
        public int specialityDepartmentEducationFormId;
        public string specialityName;
        public int? course;
        public int id;
        public string calendarId;

        public override string ToString()
        {
            return name;
        }
    }
}
