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
using EmployeeApi = BsuirScheduleLib.BsuirApi.Employee;

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
            FillScheduleCombobox();
            VM.PropertyChanged += (object sender, PropertyChangedEventArgs e) => {
                if (e.PropertyName == "selectedSchedule")
                    Reload();
                else if (e.PropertyName == "ScheduleList")
                    FillScheduleCombobox();
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
            await FillScheduleCombobox();
        }

        private async Task FillScheduleCombobox()
        {
            _selectionLocked = true;
            var employees = await EmployeeApi.Loader.Get();
            ScheduleComboBox.Items.Clear();
            if (Loader.CachedSchedulesArray != null)
            {
                foreach (var scheduleName in Loader.CachedSchedulesArray)
                {
                    var employee = employees.Find(e => e.id.ToString() == scheduleName);
                    var nameTextBlock = new TextBlock {Text = (employee != null) ? employee.FullName : scheduleName};
                    var contextMenu = new MenuFlyout();
                    nameTextBlock.ContextFlyout = contextMenu;
                    MenuFlyoutItem deleteMenuItem = new MenuFlyoutItem();
                    deleteMenuItem.Text = "Delete";
                    deleteMenuItem.Click += (s, e) => DeleteSchedule(scheduleName);
                    contextMenu.Items.Add(deleteMenuItem);
                    var options = new FlyoutShowOptions();
                    options.ShowMode = FlyoutShowMode.Transient;
                    nameTextBlock.RightTapped += (s, e) => contextMenu.ShowAt(nameTextBlock, options);
                    nameTextBlock.Tag = scheduleName;

                    ScheduleComboBox.Items.Add(nameTextBlock);
                }
            }
            Button loadBtn = new Button();
            loadBtn.Content = "Load schedule...";
            loadBtn.HorizontalAlignment = HorizontalAlignment.Stretch;
            loadBtn.Click += (s, e) => LoadSchedule();
            ScheduleComboBox.Items.Add(loadBtn);
            ScheduleComboBox.SelectedValue = ScheduleComboBox.Items.Where(i => (i as TextBlock)?.Text == VM.SelectedSchedule).FirstOrDefault();
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
                MessageDialog errorDlg = new MessageDialog("Invalid group id or employee name");
                await errorDlg.ShowAsync();
            }
            catch (Exception e)
            {
#if DEBUG
                var message = e.Message;
#else
                var message = "Unknown error";
#endif
                MessageDialog errorDlg = new MessageDialog(message);
                await errorDlg.ShowAsync();
            }
        }

        private void ScheduleSelected(object sender, SelectionChangedEventArgs e)
        {
            if (ScheduleComboBox.SelectedItem is TextBlock textBlock)
            {
                if (_selectionLocked)
                    return;
                string value = textBlock?.Tag as string;
                if (value != null && value != VM.SelectedSchedule)
                    VM.SelectedSchedule = value;
            }
            else // Select right item
            {
                var item = ScheduleComboBox.Items.Where(i => (i as TextBlock)?.Tag.ToString() == VM.SelectedSchedule && VM.SelectedSchedule != null).FirstOrDefault();
                ScheduleComboBox.SelectedValue = item;
                if (item == null)
                    ScheduleComboBox.PlaceholderText = "Add schedule";
            }
        }

        private void DeleteSchedule(string name)
        {
            VM.DeleteSchedule(name);
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
