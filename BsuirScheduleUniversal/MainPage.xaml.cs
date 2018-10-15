using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using Windows.ApplicationModel.Core;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Popups;

namespace BsuirScheduleUniversal
{
    public sealed partial class MainPage : Page, INotifyPropertyChanged
    {
        private static ApplicationDataContainer LocalSettings => ApplicationData.Current.LocalSettings;

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private bool _selectionLocked = false;
        private bool _isBusy = false;
        private DateTime? _beginDate;
        private DateTime? _endDate;
        public ObservableCollection<DayScheduleVM> Schedule;
        public Visibility LoadMoreVisibility => (Schedule != null && Schedule.Count > 0 && !IsFullSchedule) ? Visibility.Visible : Visibility.Collapsed;

        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                _isBusy = value;
                NotifyPropertyChanged();
            }
        }

        public bool IsFullSchedule
        {
            get => (LocalSettings.Values["IsFullSchedule"] as bool?) ?? false;
            set
            {
                LocalSettings.Values["IsFullSchedule"] = (bool?)value;
                Reload();
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
        private int _currentDayIndex;

        private string _selectedGroup = null;
        public string SelectedGroup
        {
            get => _selectedGroup ?? (LocalSettings.Values["selectedGroup"] as string);
            set
            {
                SetSelectedGroup(value);
            }
        }

        private async void SetSelectedGroup(string value)
        {
            _selectedGroup = value;
            NotifyPropertyChanged();
            await Reload();
            LocalSettings.Values["selectedGroup"] = _selectedGroup;
        }

        public MainPage()
        {
            InitializeComponent();
            Reload();
            FillGroupCombobox();
        }

        private async Task<ObservableCollection<DayScheduleVM>> LoadSchedule()
        {
            var schedule = new ObservableCollection<DayScheduleVM>();
            DateTime day = DateTime.Today;
            for (int i = 0; i < 7; i++)
            {
                day = day.AddDays(-1);
                _currentDayIndex++;
                if (day.DayOfWeek == DayOfWeek.Monday)
                    break;
            }

            _beginDate = day;

            for (; day < _beginDate.Value.AddDays(30) || day.DayOfWeek != DayOfWeek.Monday; day = day.AddDays(1))
            {
                schedule.Add(await DayScheduleVM.Create(SelectedGroup, day, CheckedSubgroup));
            }

            _endDate = day.AddDays(-1);
            return schedule;
        }

        private async Task<ObservableCollection<DayScheduleVM>> LoadFullSchedule()
        {
            var schedule = new ObservableCollection<DayScheduleVM>();
            
            for (var day = DayOfWeek.Monday; day != DayOfWeek.Sunday; day = (DayOfWeek)((int)(day + 1) % 7))
            {
                schedule.Add(await DayScheduleVM.CreateFull(SelectedGroup, day, CheckedSubgroup));
            }
            return schedule;
        }

        private async void OnScheduleUpdated(ScheduleResponse schedule)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                async () =>
                {
                    Reload();
                    var dialog = new MessageDialog("Schedule updated");
                    await dialog.ShowAsync();
                }
            );
        }

        private async Task Reload()
        {
            if (ScheduleGridView == null) return;
            if (SelectedGroup == null) return;
            IsBusy = true;

            try
            {
                Schedule = null;

                if(IsFullSchedule)
                    Schedule = await LoadFullSchedule();
                else
                    Schedule = await LoadSchedule();
                Loader.AddScheduleUpdateListener(OnScheduleUpdated, SelectedGroup);
            }
            catch (ScheduleLoadingException e)
            {
                if(LocalSettings.Values.ContainsKey("selectedGroup"))
                    SelectedGroup = LocalSettings.Values["selectedGroup"] as string; // Revert selected group
                return;
            }
            catch (Exception e)
            {
                // ignored
            }

            FillGroupCombobox();
            NotifyPropertyChanged("Schedule");
            NotifyPropertyChanged("LoadMoreVisibility");
            IsBusy = false;
        }

        private void SubgroupChecked(object sender, RoutedEventArgs e)
        {
            CheckedSubgroup = int.Parse((string)((RadioButton) sender).Tag);
        }

        private void FillGroupCombobox()
        {
            _selectionLocked = true;
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
            _selectionLocked = false;
        }

        private async void GroupSelected(object sender, SelectionChangedEventArgs e)
        {
            if (_selectionLocked)
                return;

            if(string.IsNullOrEmpty(GroupComboBox.SelectedItem?.ToString())) return;

            if (GroupComboBox.SelectedItem?.ToString() == "Load group...")
            {
                AddGroupDialog dlg = new AddGroupDialog();
                await dlg.ShowAsync();
                if (dlg.Value == null) return;

                SelectedGroup = dlg.Value;
            }
            else
            {
                SelectedGroup = GroupComboBox.SelectedItem?.ToString();
            }
        }

        private void ChartButton_OnClick(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(Chart));
        }

        private void DayScheduleControl_OnPairSelected(PairVM obj)
        {
            this.Frame.Navigate(typeof(PairPage), obj);
        }

        private async void LoadUpClick(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < 7; i++)
            {
                _beginDate = _beginDate.Value.AddDays(-1);
                Schedule.Insert(0, await DayScheduleVM.Create(SelectedGroup, _beginDate.Value, CheckedSubgroup));
            }
            NotifyPropertyChanged("Schedule");
        }

        private async void LoadDownClick(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < 7; i++)
            {
                _endDate = _endDate.Value.AddDays(1);
                Schedule.Add(await DayScheduleVM.Create(SelectedGroup, _endDate.Value, CheckedSubgroup));
            }
            NotifyPropertyChanged("Schedule");
        }

        private void DayScheduleControl_PairDeleted()
        {
            Reload();
        }
    }
}
