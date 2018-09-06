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

        public string startLessonTime => _obj.startLessonTime;
        public string endLessonTime => _obj.endLessonTime;
        public string subject => _obj.subject;

        public string Auditory => _obj.auditory.Aggregate("", (s, s1) => s + s1);


        public string NumSubgroup => (_obj.numSubgroup == 0) ? "" : $"подгр. {_obj.numSubgroup}";
        public string EmployeeName
        {
            get
            {
                if (_obj.employee.Count == 0) return "";
                var teacher = _obj.employee.First();
                return $"{teacher.lastName} {teacher.firstName.First()}. {teacher.middleName.First()}.";

            }
        }
        public string LessonType => $" ({_obj.lessonType})";
        public Brush LessonTypeBrush
        {
            get
            {
                switch (_obj.lessonType)
                {
                    case "ЛК":
                        return new SolidColorBrush(Colors.LightGray);
                    case "ПЗ":
                        return new SolidColorBrush(Colors.Gold);
                    case "ЛР":
                        return new SolidColorBrush(Colors.Brown);
                    default:
                        return new SolidColorBrush(Colors.White);
                }
            }
        }

        private Brush GetWeekBrush(int week)
        {
            return _obj.weekNumber.Contains(week)
                ? new SolidColorBrush(Colors.Aqua)
                : new SolidColorBrush(Colors.Transparent);
        }

        public Brush WeekBrush1 => GetWeekBrush(1);
        public Brush WeekBrush2 => GetWeekBrush(2);
        public Brush WeekBrush3 => GetWeekBrush(3);
        public Brush WeekBrush4 => GetWeekBrush(4);
    }
}
