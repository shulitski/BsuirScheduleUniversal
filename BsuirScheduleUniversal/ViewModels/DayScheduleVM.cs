using BsuirScheduleLib.BsuirApi.Schedule;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace BsuirScheduleUniversal.ViewModels
{
    public class DayScheduleVM
    {
        private readonly DateTime? _date;
        private readonly DayOfWeek? _dayOfWeek;
        private bool IsToday => (_date == null) ? (_dayOfWeek.Value == DateTime.Today.DayOfWeek) : (_date.Value.Date == DateTime.Today.Date);
        public List<PairVM> Pairs { get; set; } = new List<PairVM>();
        public string WeekDayName => (_date == null) ? _dayOfWeek.ToString() : $"{_date.Value:dd.MM.yyyy} {_date.Value.DayOfWeek.ToString()}";
        public Brush Background => IsToday
            ? (Brush)new SolidColorBrush(Color.FromArgb(32, 0, 255, 255))
            : (Brush)new SolidColorBrush(Colors.Transparent);
        
        public Brush Border => IsToday
            ? (Brush)new SolidColorBrush(Color.FromArgb(255, 0, 255, 255))
            : (Brush)new SolidColorBrush(Colors.Transparent);

        public double ControlHeight => (_date == null) ? 700 : 300;

        private DayScheduleVM(DateTime date)
        {
            _date = date;
        }

        private DayScheduleVM(DayOfWeek day)
        {
            _dayOfWeek = day;
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
      
        public static async Task<DayScheduleVM> CreateFull(string group, DayOfWeek day, int subGroup)
        {
            DayScheduleVM result = new DayScheduleVM(day);
            var pairs = await Loader.LoadPairsFull(group, day, subGroup);
            if (pairs == null) return result;

            foreach (var pair in pairs)
            {
                result.Pairs.Add(new PairVM(pair));
            }

            return result;
        }
    }
}
