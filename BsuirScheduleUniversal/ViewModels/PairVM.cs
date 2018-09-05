using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml.Media;
using BsuirScheduleLib.BsuirApi.Schedule;

namespace BsuirScheduleUniversal.ViewModels
{
    public class PairVM
    {
        private readonly Pair _obj;
        public PairVM(Pair obj)
        {
            _obj = obj;
        }

        public int numSubgroup => _obj.numSubgroup;
        public string startLessonTime => _obj.startLessonTime;
        public string endLessonTime => _obj.endLessonTime;
        public string subject => _obj.subject;
        public string lessonType => _obj.lessonType;

        public string Auditory => _obj.auditory.Aggregate("", (s, s1) => s + s1);
        public string EmployeeName => _obj.employee.Count > 0 ? _obj.employee.First().fio : "";

        public Brush LessonTypeBrush
        {
            get
            {
                switch (lessonType)
                {
                    case "ЛК":
                        return new SolidColorBrush(Colors.White);
                    case "ПЗ":
                        return new SolidColorBrush(Colors.Yellow);
                    case "ЛР":
                        return new SolidColorBrush(Colors.Red);
                    default:
                        return new SolidColorBrush(Colors.White);
                }
            }
        }
    }
}
