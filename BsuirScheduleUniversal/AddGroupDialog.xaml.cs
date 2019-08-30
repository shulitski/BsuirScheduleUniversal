using GroupApi = BsuirScheduleLib.BsuirApi.Group;
using EmployeeApi = BsuirScheduleLib.BsuirApi.Employee;
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

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace BsuirScheduleUniversal
{
    public sealed partial class AddGroupDialog : ContentDialog
    {
        public string Value { get; set; }
        public GroupApi.Group SelectedGroup;
        public EmployeeApi.Employee SelectedEmployee;
        private List<GroupApi.Group> _groups;
        private List<EmployeeApi.Employee> _employees;

        public AddGroupDialog()
        {
            InitializeComponent();
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            Value = ScheduleTextBox.Text;
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            Value = null;
        }

        private void GroupTextBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                if (_groups != null)
                {
                    List<object> scheduleList = new List<object>();
                    scheduleList.AddRange(_groups.FindAll(g => g.name.Contains(sender.Text)).ToList());
                    scheduleList.AddRange(_employees.FindAll(e => e.Contains(sender.Text)));
                    ScheduleTextBox.ItemsSource = scheduleList;
                }
            }
        }

        private async void GroupTextBox_Loaded(object sender, RoutedEventArgs e)
        {
            _groups = await GroupApi.Loader.Load();
            _employees = await EmployeeApi.Loader.Load();
            ScheduleTextBox.IsEnabled = true;
        }

        private void ScheduleTextBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (args.ChosenSuggestion != null)
            {
                if (args.ChosenSuggestion is GroupApi.Group g)
                    SelectedGroup = g;
                else if (args.ChosenSuggestion is EmployeeApi.Employee e)
                    SelectedEmployee = e;
            }
            else if (!string.IsNullOrEmpty(args.QueryText))
            {
                Value = args.QueryText;
            }
        }
    }
}
