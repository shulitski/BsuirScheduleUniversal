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

        private PairVM GetPair(int index)
        {
            string startTime = "";
            switch (index)
            {
                case 1: startTime = "08:00"; break;
                case 2: startTime = "09:45"; break;
                case 3: startTime = "11:40"; break;
                case 4: startTime = "13:25"; break;
                case 5: startTime = "15:20"; break;
                case 6: startTime = "17:05"; break;
                case 7: startTime = "18:45"; break;
                case 8: startTime = "20:25"; break;
            }
            return Pairs.Find(p => p.startLessonTime == startTime);
        }

        public PairVM Pair1 => GetPair(1);
        public PairVM Pair2 => GetPair(2);
        public PairVM Pair3 => GetPair(3);
        public PairVM Pair4 => GetPair(4);
        public PairVM Pair5 => GetPair(5);
        public PairVM Pair6 => GetPair(6);
        public PairVM Pair7 => GetPair(7);
        public PairVM Pair8 => GetPair(8);
    }
}
