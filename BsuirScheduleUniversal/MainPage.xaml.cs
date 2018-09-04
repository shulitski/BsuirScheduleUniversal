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
        public List<Pair> Pairs { get; set; }

        public DaySchedule(List<Pair> pairs)
        {
            Pairs = pairs;
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
            List<DaySchedule> schedule = new List<DaySchedule>
            {
                new DaySchedule(Loader.LoadPairs("551005", DateTime.Today, 2)),
                new DaySchedule(Loader.LoadPairs("551005", DateTime.Today.AddDays(1), 2)),
                new DaySchedule(Loader.LoadPairs("551005", DateTime.Today.AddDays(2), 2)),
                new DaySchedule(Loader.LoadPairs("551005", DateTime.Today.AddDays(3), 2)),
                new DaySchedule(Loader.LoadPairs("551005", DateTime.Today.AddDays(4), 2)),
                new DaySchedule(Loader.LoadPairs("551005", DateTime.Today.AddDays(5), 2))
            };
            ScheduleGridView.ItemsSource = schedule;
            //Panel.Children.Add();

            //PairsListView.ItemsSource = Loader.LoadPairs("551005", DateTime.Now, 2);
        }
    }
}
