using BsuirScheduleLib.BsuirApi.Schedule;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using BsuirScheduleLib.BsuirApi;

namespace BsuirScheduleUniversal.ViewModels
{
    public class DayScheduleVM : INotifyPropertyChanged
    {
        private readonly DateTime? _date;
        private readonly DayOfWeek? _dayOfWeek;

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        DispatcherTimer _dispatcherTimer;

        private bool IsToday => (_date == null) ? (_dayOfWeek.Value == DateTime.Today.DayOfWeek) : (_date.Value.Date == DateTime.Today.Date);
        private bool IsHolyday => _date?.IsHolyday() ?? (_dayOfWeek.Value == DayOfWeek.Sunday);
        public List<PairVM> Pairs { get; set; } = new List<PairVM>();
        public string WeekDayName => (_date == null) 
            ? GetDayName(_dayOfWeek.Value) 
            : $"{_date.Value:dd.MM.yyyy} {GetDayName(_date.Value.DayOfWeek)}";
        public Brush Border => GetBorderAndBackground().Item1;
        public Brush Background => GetBorderAndBackground().Item2;
        
        private string GetDayName(DayOfWeek day)
        {
            return CultureInfo.CurrentCulture.DateTimeFormat.GetDayName(day);
        }

        private (Brush, Brush) GetBorderAndBackground()
        {
            Color borderColor;
            Color backgroundColor;
            if (IsToday && !IsHolyday)
                borderColor = Color.FromArgb(255, 0, 255, 255);
            else if (!IsToday && IsHolyday)
                borderColor = Color.FromArgb(255, 255, 96, 96);
            else if (IsToday && IsHolyday)
                borderColor = Color.FromArgb(255, 192, 64, 255);
            else
                borderColor = Colors.Transparent;
            backgroundColor = borderColor;
            backgroundColor.A = (byte)((backgroundColor.A > 0) ? 32 : 0);
            return (new SolidColorBrush(borderColor), new SolidColorBrush(backgroundColor));
        }

        public double ControlHeight => (_date == null) ? 700 : 300;

        private DayScheduleVM(DateTime date)
        {
            _date = date;
            RunTimer();
        }

        private DayScheduleVM(DayOfWeek day)
        {
            _dayOfWeek = day;
            RunTimer();
        }

        public DayScheduleVM()
        {
            _date = DateTime.Now;
            for (int i = 0; i < 4; i++)
                Pairs.Add(new PairVM());
        }

        private void RunTimer()
        {
            _dispatcherTimer = new DispatcherTimer();
            _dispatcherTimer.Tick += (s, e) =>
            {
                NotifyPropertyChanged("Border");
                NotifyPropertyChanged("Background");
            };
            _dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
            _dispatcherTimer.Start();
        }

        public static async Task<DayScheduleVM> Create(ScheduleQuery query, DateTime date, int subGroup)
        {
            DayScheduleVM result = new DayScheduleVM(date);
            var pairs = await Loader.LoadPairs(query, date, subGroup);
            if (pairs == null) return result;

            foreach (var pair in pairs)
            {
                result.Pairs.Add(new PairVM(pair));
            }

            return result;
        }
      
        public static async Task<DayScheduleVM> CreateFull(ScheduleQuery query, DayOfWeek day, int subGroup)
        {
            DayScheduleVM result = new DayScheduleVM(day);
            var pairs = await Loader.LoadPairsFull(query, day, subGroup);
            if (pairs == null) return result;

            foreach (var pair in pairs)
            {
                result.Pairs.Add(new PairVM(pair));
            }

            return result;
        }
    }
}
