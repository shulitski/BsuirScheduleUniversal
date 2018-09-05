using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using BsuirScheduleLib.BsuirApi.Schedule;

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

    public sealed partial class MainPage : Page
    {
        private static ApplicationDataContainer LocalSettings => ApplicationData.Current.LocalSettings;

        private int CheckedSubgroup
        {
            get => (LocalSettings.Values["checkedSubgroup"] as int?) ?? 0;
            set
            {
                LocalSettings.Values["checkedSubgroup"] = (int?) value;
                Reload();
            }
        }

        private bool IsSubgroup0 => CheckedSubgroup == 0;
        private bool IsSubgroup1 => CheckedSubgroup == 1;
        private bool IsSubgroup2 => CheckedSubgroup == 2;

        public string SelectedGroup
        {
            get => LocalSettings.Values["selectedGroup"] as string;
            set
            {
                LocalSettings.Values["selectedGroup"] = value;
                Reload();
            }
        }

        public MainPage()
        {
            InitializeComponent();
            Reload();
            FillGroupCombobox();
        }

        private void Reload()
        {
            if (ScheduleGridView == null) return;
            if(SelectedGroup == null) return;

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
                schedule.Add(new DaySchedule(SelectedGroup, day.AddDays(i), CheckedSubgroup));
            }

            ScheduleGridView.ItemsSource = schedule;
            ScheduleGridView.SelectedIndex = currentDayIndex;
        }

        private void SubgroupChecked(object sender, RoutedEventArgs e)
        {
            CheckedSubgroup = int.Parse((string)((RadioButton) sender).Tag);
        }

        private void FillGroupCombobox()
        {
            GroupComboBox.Items.Clear();
            if (Loader.CachedGroupsArray != null)
            {
                foreach (var group in Loader.CachedGroupsArray)
                {
                    GroupComboBox.Items.Add(group);
                    if (group == SelectedGroup)
                        GroupComboBox.SelectedItem = group;
                }
            }
            GroupComboBox.Items.Add("Load group...");
        }

        private async void GroupSelected(object sender, SelectionChangedEventArgs e)
        {
            if(string.IsNullOrEmpty(GroupComboBox.SelectedItem?.ToString())) return;

            if (GroupComboBox.SelectedItem?.ToString() == "Load group...")
            {
                AddGroupDialog dlg = new AddGroupDialog();
                await dlg.ShowAsync();
                if (dlg.Value == null) return;

                SelectedGroup = dlg.Value;
                FillGroupCombobox();
            }
            else
            {
                SelectedGroup = GroupComboBox.SelectedItem?.ToString();
            }
        }
    }
}
