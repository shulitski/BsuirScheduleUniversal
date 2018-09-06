using BsuirScheduleLib.BsuirApi.Schedule;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml.Media;

namespace BsuirScheduleUniversal.ViewModels
{
    public class DayScheduleVM
    {

        
        private readonly DateTime _date;
        public List<PairVM> Pairs { get; set; } = new List<PairVM>();
        public string WeekDayName => $"{_date.ToShortDateString()} {_date.DayOfWeek.ToString()}";
        public Brush Background => _date.Date == DateTime.Today.Date
            ? (Brush)new SolidColorBrush(Color.FromArgb(32, 0, 255, 255))
            //? (Brush)new SolidColorBrush(Colors.Red) 
            : (Brush)new SolidColorBrush(Colors.Transparent);
        
        public Brush Border => _date.Date == DateTime.Today.Date
            ? (Brush)new SolidColorBrush(Color.FromArgb(255, 0, 255, 255))
            //? (Brush)new SolidColorBrush(Colors.Red) 
            : (Brush)new SolidColorBrush(Colors.Transparent);

        private DayScheduleVM(DateTime date)
        {
            _date = date;
        }
        
        public DayScheduleVM()
        {
            _date = DateTime.Now;
            for (int i = 0; i < 4; i++)
                Pairs.Add(new PairVM());
        }
        public static async Task<DayScheduleVM> Create(string group, DateTime date, int subGroup)
        {
            DayScheduleVM result = new DayScheduleVM(date);
            var pairs = await Loader.LoadPairs(group, date, subGroup);
            if (pairs == null) return result;

            foreach (var pair in pairs)
            {
                result.Pairs.Add(new PairVM(pair));
            }

            return result;
        }
    }
}
