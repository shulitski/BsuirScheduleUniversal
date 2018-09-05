using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;
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
        private int _checkedSubgroup = 0;
        private string _group = "551005";

        public MainPage()
        {
            this.InitializeComponent();
            Reload();
        }

        private void Reload()
        {
            if (ScheduleGridView == null)
                return;

            List<DaySchedule> schedule = new List<DaySchedule>();
            DateTime day = DateTime.Today;
            int currentDayIndex = 0;
            for (int i = 0; i < 7; i++)
            {
                day = day.AddDays(-1);
                currentDayIndex++;
                if (day.DayOfWeek == DayOfWeek.Monday)
                    break;
            }
            for (int i = 0; i < 30; i++)
            {
                schedule.Add(new DaySchedule(_group, day.AddDays(i), _checkedSubgroup));
            };

            ScheduleGridView.ItemsSource = schedule;
            ScheduleGridView.SelectedIndex = currentDayIndex;
            //ScheduleGridView.ScrollIntoView(ScheduleGridView.Items[30]);
        }

        private void GroupTextbox_OnKeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Enter)
            {
                _group = GroupTextBox.Text;
                Reload();
            }
        }

        private void SubgroupChecked(object sender, RoutedEventArgs e)
        {
            _checkedSubgroup = int.Parse((string)((RadioButton) sender).Tag);
            Reload();
        }
    }
}
