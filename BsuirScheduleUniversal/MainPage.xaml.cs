using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
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
using BsuirScheduleUniversal.ViewModels;
using System.Runtime.CompilerServices;
using Windows.UI;

namespace BsuirScheduleUniversal
{
    public class DaySchedule
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

        private DaySchedule(DateTime date)
        {
            _date = date;
        }

        public static async Task<DaySchedule> Create(string group, DateTime date, int subGroup)
        {
            DaySchedule result = new DaySchedule(date);
            var pairs = await Loader.LoadPairs(group, date, subGroup);
            if (pairs == null) return result;

            foreach (var pair in pairs)
            {
                result.Pairs.Add(new PairVM(pair));
            }

            return result;
        }
    }

    public sealed partial class MainPage : Page, INotifyPropertyChanged
    {
        private static ApplicationDataContainer LocalSettings => ApplicationData.Current.LocalSettings;

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        private bool _isBusy = false;
        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                _isBusy = value;
                NotifyPropertyChanged();
            }
        }

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
                NotifyPropertyChanged();
                Reload();
            }
        }

        public MainPage()
        {
            InitializeComponent();
            Reload();
            FillGroupCombobox();
        }

        private async void Reload()
        {
            if (ScheduleGridView == null) return;
            if (SelectedGroup == null) return;
            IsBusy = true;

            try
            {
                ScheduleGridView.ItemsSource = null;

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
                    schedule.Add(await DaySchedule.Create(SelectedGroup, day.AddDays(i), CheckedSubgroup));
                }

                ScheduleGridView.ItemsSource = schedule;
                ScheduleGridView.SelectedIndex = currentDayIndex;
            }
            catch (Exception)
            {
                // ignored
            }

            IsBusy = false;
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
