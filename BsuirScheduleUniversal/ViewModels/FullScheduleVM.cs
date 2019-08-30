using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using BsuirScheduleLib.BsuirApi.Schedule;

namespace BsuirScheduleUniversal.ViewModels
{
    public class FullScheduleVM : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region Fields
        private bool _isBusy;
        private int _currentDayIndex;
        private DateTime? _beginDate;
        private DateTime? _endDate;
        private string _selectedGroup;
        #endregion

        #region Properties

        private static ApplicationDataContainer LocalSettings => ApplicationData.Current.LocalSettings;

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

        public ObservableCollection<DayScheduleVM> Schedule { get; set; }
        public Visibility LoadMoreVisibility => (Schedule != null && Schedule.Count > 0 && !IsFullSchedule) ? Visibility.Visible : Visibility.Collapsed;

        public string SelectedGroup
        {
            get => _selectedGroup ?? (LocalSettings.Values["selectedGroup"] as string);
            set => SetSelectedGroup(value);
        }

        private int CheckedSubgroup
        {
            get => (LocalSettings.Values["checkedSubgroup"] as int?) ?? 0;
            set
            {
                LocalSettings.Values["checkedSubgroup"] = (int?)value;
                Reload();
            }
        }

        public bool IsSubgroup0 => CheckedSubgroup == 0;
        public bool IsSubgroup1 => CheckedSubgroup == 1;
        public bool IsSubgroup2 => CheckedSubgroup == 2;

        #endregion

        #region Methods

        public FullScheduleVM()
        {

        }

        public void SubgroupChecked(int subgroup)
        {
            CheckedSubgroup = subgroup;
        }

        private async Task SetSelectedGroup(string value)
        {
            var prevSelectedGroup = _selectedGroup;
            _selectedGroup = value;
            LocalSettings.Values["selectedGroup"] = _selectedGroup;
            try
            {
                await Reload();
            }
            catch (ScheduleLoadingException)
            {
                _selectedGroup = prevSelectedGroup;  // Revert selected group
                LocalSettings.Values["selectedGroup"] = prevSelectedGroup;
                throw;
            }
            NotifyPropertyChanged("selectedGroup");
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

        public async Task Reload()
        {
            //if (ScheduleGridView == null) return;
            IsBusy = true;
            try
            {
                Schedule = null;
                if (SelectedGroup != null)
                {
                    Schedule = IsFullSchedule ? await LoadFullSchedule() : await LoadSchedule();
                    Loader.AddScheduleUpdateListener(OnScheduleUpdated, SelectedGroup);
                }
            }
            catch (ScheduleLoadingException e)
            {
                IsBusy = false;
                throw;
            }
            catch (Exception e)
            {
                // ignored
            }
            NotifyPropertyChanged("Schedule");
            NotifyPropertyChanged("LoadMoreVisibility");
            IsBusy = false;
        }

        public async Task LoadMore(bool down)
        {
            for (int i = 0; i < 7; i++)
            {
                DateTime date;
                date = down 
                    ? (_endDate   =   _endDate.Value.AddDays( 1)).Value 
                    : (_beginDate = _beginDate.Value.AddDays(-1)).Value;

                var daySchedule = await DayScheduleVM.Create(SelectedGroup, date, CheckedSubgroup);

                if (down)
                    Schedule.Add(daySchedule);
                else
                    Schedule.Insert(0, daySchedule);

            }
            NotifyPropertyChanged("Schedule");
        }

        public async Task GroupSelected(string value)
        {
            switch (value)
            {
                case null:
                    return;
                case "Load group...":
                    AddGroupDialog dlg = new AddGroupDialog();
                    await dlg.ShowAsync();
                    if (dlg.Value == null) return;

                    try
                    {
                        await SetSelectedGroup(dlg.Value);
                    }
                    catch (ScheduleLoadingException)
                    {
                        MessageDialog errorDlg = new MessageDialog("Invalid group id or teacher name");
                        await errorDlg.ShowAsync();
                    }
                    
                    break;
                default:
                    SelectedGroup = value;
                    break;
            }
        }

        public async void DeleteGroup(string group)
        {
            await Loader.DeleteGroup(group);
            if (SelectedGroup == group)
                SelectedGroup = null;
            else
                SelectedGroup = SelectedGroup;
        }

        #endregion
    }
}
