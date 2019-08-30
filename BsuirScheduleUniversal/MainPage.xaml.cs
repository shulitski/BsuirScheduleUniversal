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
    public sealed partial class MainPage : Page
    {
        private FullScheduleVM VM => (FullScheduleVM) DataContext;
        private bool _selectionLocked = false;

        
        public MainPage()
        {
            InitializeComponent();
            Reload();
            FillGroupCombobox();
            VM.PropertyChanged += (object sender, PropertyChangedEventArgs e) => {
                if (e.PropertyName == "selectedSchedule")
                    Reload();
                else if (e.PropertyName == "ScheduleList")
                    FillGroupCombobox();
            };
        }

        private void SubgroupChecked(object sender, RoutedEventArgs e)
        {
            var subgroup = int.Parse((string)((RadioButton) sender).Tag);
            VM.SubgroupChecked(subgroup);
        }

        private async Task Reload()
        {
            if (ScheduleGridView == null) return;
            await VM.Reload();
            FillGroupCombobox();
        }

        private void FillGroupCombobox()
        {
            _selectionLocked = true;
            GroupComboBox.Items.Clear();
            object selectedItem = null;
            if (Loader.CachedSchedulesArray != null)
            {
                foreach (var group in Loader.CachedSchedulesArray)
                {
                    var name = new TextBlock {Text = group};
                    var contextMenu = new MenuFlyout();
                    name.ContextFlyout = contextMenu;
                    MenuFlyoutItem deleteMenuItem = new MenuFlyoutItem();
                    deleteMenuItem.Text = "Delete";
                    deleteMenuItem.Click += (s, e) => DeleteGroup(group);
                    contextMenu.Items.Add(deleteMenuItem);
                    var options = new FlyoutShowOptions();
                    options.ShowMode = FlyoutShowMode.Transient;
                    name.RightTapped += (s, e) => contextMenu.ShowAt(name, options);
                    
                    GroupComboBox.Items.Add(name);
                    if (group == VM.SelectedSchedule)
                        selectedItem = name;
                }
            }
            Button loadBtn = new Button();
            loadBtn.Content = "Load schedule...";
            loadBtn.HorizontalAlignment = HorizontalAlignment.Stretch;
            loadBtn.Click += (s, e) => LoadSchedule();
            GroupComboBox.Items.Add(loadBtn);
            GroupComboBox.SelectedValue = GroupComboBox.Items.Where(i => (i as TextBlock)?.Text == VM.SelectedSchedule).FirstOrDefault();
            _selectionLocked = false;
        }

        private async void LoadSchedule()
        {
            AddGroupDialog dlg = new AddGroupDialog();
            await dlg.ShowAsync();
            var name = dlg.Value ?? dlg.SelectedGroup?.name ?? dlg.SelectedEmployee?.FullName;
            if (name == null)
                return;

            try
            {
                await VM.SetSelectedSchedule(dlg.Value, (dlg.SelectedEmployee?.id as int?)?.ToString());
            }
            catch (ScheduleLoadingException)
            {
                MessageDialog errorDlg = new MessageDialog("Invalid group id or teacher name");
                await errorDlg.ShowAsync();
            }
        }

        private void GroupSelected(object sender, SelectionChangedEventArgs e)
        {
            if (GroupComboBox.SelectedItem is TextBlock)
            {
                if (_selectionLocked)
                    return;
                string value = (GroupComboBox.SelectedItem as TextBlock)?.Text;
                if (value != null && value != VM.SelectedSchedule)
                    VM.SelectedSchedule = value;
            }
            else // Select right item
            {
                var item = GroupComboBox.Items.Where(i => (i as TextBlock)?.Text == VM.SelectedSchedule && VM.SelectedSchedule != null).FirstOrDefault();
                GroupComboBox.SelectedValue = item;
                if (item == null)
                    GroupComboBox.PlaceholderText = "Add schedule";
            }
        }

        private void DeleteGroup(string group)
        {
            VM.DeleteSchedule(group);
        }

        private void ChartButton_OnClick(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(Chart));
        }

        private void DayScheduleControl_OnPairSelected(PairVM obj)
        {
            Frame.Navigate(typeof(PairPage), obj);
        }

        private async void LoadUpClick(object sender, RoutedEventArgs e)
        {
            await VM.LoadMore(false);
        }

        private async void LoadDownClick(object sender, RoutedEventArgs e)
        {
            await VM.LoadMore(true);
        }

        private void DayScheduleControl_PairDeleted()
        {
            Reload();
        }
    }
}
