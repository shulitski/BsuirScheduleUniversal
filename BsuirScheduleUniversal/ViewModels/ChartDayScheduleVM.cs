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
    class ChartDayScheduleVM
    {
        private readonly DateTime _date;
        public List<ChartPairVM> Pairs { get; set; } = new List<ChartPairVM>();

        private ChartDayScheduleVM(DateTime date)
        {
            _date = date;
        }

        public ChartDayScheduleVM()
        {
            _date = DateTime.Now;
            for (int i = 0; i < 4; i++)
                Pairs.Add(new ChartPairVM());
        }

        public static async Task<ChartDayScheduleVM> Create(string group, DateTime date, int subGroup)
        {
            ChartDayScheduleVM result = new ChartDayScheduleVM(date);
            var pairs = await Loader.LoadPairs(group, date, subGroup);
            if (pairs == null) return result;

            foreach (var pair in pairs)
            {
                result.Pairs.Add(new ChartPairVM(pair));
            }

            return result;
        }

        private ChartPairVM GetPair(int index)
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

        

        public ChartPairVM Pair1 => GetPair(1);
        public ChartPairVM Pair2 => GetPair(2);
        public ChartPairVM Pair3 => GetPair(3);
        public ChartPairVM Pair4 => GetPair(4);
        public ChartPairVM Pair5 => GetPair(5);
        public ChartPairVM Pair6 => GetPair(6);
        public ChartPairVM Pair7 => GetPair(7);
        public ChartPairVM Pair8 => GetPair(8);

        public string WeekDayName => _date.DayOfWeek.ToString();
        public string Date => $"{_date:dd.MM.yyyy}";
        public double Height => Utils.GetSize().Height - Constants.TopBarHeight;
        public double DateHeight => Constants.ChartDateHeight;

        private bool IsToday => _date.Date == DateTime.Today.Date;
        public Brush Background => IsToday
            ? (Brush)new SolidColorBrush(Color.FromArgb(32, 0, 255, 255))
            : (Brush)new SolidColorBrush(Colors.Transparent);

        public Brush Border => IsToday
            ? (Brush)new SolidColorBrush(Color.FromArgb(255, 0, 255, 255))
            : (Brush)new SolidColorBrush(Colors.Transparent);
    }
}
