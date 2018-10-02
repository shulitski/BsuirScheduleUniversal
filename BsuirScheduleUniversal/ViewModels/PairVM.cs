using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml;
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

        public PairVM()
        {
            Pair pair = new Pair
            {
                auditory = new List<string> {"210-4"},
                startLessonTime = "08:00",
                endLessonTime = "09:35",
                subject = "ОСиСП",
                numSubgroup = 2,
                employee = new List<Employee>
                {
                    new Employee {lastName = "Конь", firstName = "Игорь", middleName = "Генадиевич", rank = "Профессор"}
                },
                lessonType = "ЛР",
                weekNumber = new List<int> {1, 3},
                note = "Записка"
            };

            _obj = pair;
        }

        public string startLessonTime => _obj.startLessonTime;
        public string endLessonTime => _obj.endLessonTime;
        public string subject => _obj.subject;

        public string Auditory => _obj.auditory.Aggregate("", (s, s1) => s + s1);
        public string PhotoLink => _obj.employee.FirstOrDefault()?.photoLink; 

        public Visibility SubgroupVisibility => (_obj.numSubgroup == 0) ? Visibility.Collapsed : Visibility.Visible;
        public string NumSubgroup => (_obj.numSubgroup == 0) ? "All" : _obj.numSubgroup.ToString();
        public string EmployeeShortName
        {
            get
            {
                if (_obj.employee.Count == 0) return "";
                var teacher = _obj.employee.First();
                return $"{teacher.lastName} {teacher.firstName.First()}. {teacher.middleName.First()}.";

            }
        }

        public string EmployeeRank => _obj.employee.FirstOrDefault()?.rank;
        public string EmployeeFirstName => _obj.employee.FirstOrDefault()?.firstName;
        public string EmployeeMiddleName => _obj.employee.FirstOrDefault()?.middleName;
        public string EmployeeLastName => _obj.employee.FirstOrDefault()?.lastName;

        public string ShortLessonType => $" ({_obj.lessonType})";
        public string LessonType => _obj.lessonType;
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
                        return new SolidColorBrush(Colors.LightGray);
                }
            }
        }

        public Brush AuditoryColor =>
            (Auditory.Last() == '4') ? new SolidColorBrush(Colors.Transparent) : new SolidColorBrush(Colors.Brown);

        public string WeekTooltip
        {
            get
            {
                string result = "";
                foreach( var week in _obj.weekNumber)
                {
                    if (week == 0) continue;
                    if (result != "")
                        result += ", ";
                    result += week;
                }
                if(_obj.weekNumber.Count == 1)
                    result += " week";
                else
                    result += " weeks";
                return result;
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

        public string NoteIndicator => (string.IsNullOrEmpty(_obj.note)) ? "" : "!";
        public string Note => _obj.note;
    }
}
