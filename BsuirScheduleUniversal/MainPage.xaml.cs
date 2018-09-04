using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using BsuirScheduleLib.BsuirApi.Schedule;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace BsuirScheduleUniversal
{
    public class DaySchedule
    {
        private readonly DateTime _date;
        public List<Pair> Pairs { get; set; }
        public string WeekDayName => $"{_date.ToShortDateString()} {_date.DayOfWeek.ToString()}";

        public DaySchedule(string group, DateTime date, int subGroup)
        {
            _date = date;
            Pairs = Loader.LoadPairs(group, date, subGroup);
        }
    }
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            List<DaySchedule> schedule = new List<DaySchedule>();
            for(int i = -30; i < 30; i++)
            {
                schedule.Add(new DaySchedule("551005", DateTime.Today.AddDays(i), 2));
            };
            ScheduleGridView.ItemsSource = schedule;

        }
    }
}
