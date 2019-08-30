using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml.Media;
using BsuirScheduleLib.BsuirApi.Schedule;
using EmployeeApi = BsuirScheduleLib.BsuirApi.Employee;

namespace BsuirScheduleUniversal.ViewModels
{
    class ChartPairVM
    {
        private readonly Pair _obj;
        public ChartPairVM(Pair obj)
        {
            _obj = obj;
        }

        public ChartPairVM()
        {
            Pair pair = new Pair
            {
                auditory = new List<string> { "210-4" },
                startLessonTime = "08:00",
                endLessonTime = "09:35",
                subject = "ОСиСП",
                numSubgroup = 2,
                employee = new List<EmployeeApi.Employee> { new EmployeeApi.Employee { lastName = "Конь", firstName = "Игорь", middleName = "Генадиевич" } },
                lessonType = "ЛР",
                weekNumber = new List<int> { 1, 3 }
            };

            _obj = pair;
        }

        public string startLessonTime => _obj.startLessonTime;

        private const int LessonTypeTransparency = 0xA0;
        public Brush LessonTypeBrush
        {
            get
            {
                switch (_obj.lessonType)
                {
                    default:
                        return new SolidColorBrush(Color.FromArgb(LessonTypeTransparency, 179, 179, 179));
                    case "ПЗ":
                        return new SolidColorBrush(Color.FromArgb(LessonTypeTransparency, 179, 151, 0));
                    case "ЛР":
                        return new SolidColorBrush(Color.FromArgb(LessonTypeTransparency, 126, 32, 32));
                }
            }
        }
        public string Subject => _obj.subject;
    }
}
