using BsuirScheduleLib.BsuirApi.Group;
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
        private List<Group> _groups;

        public AddGroupDialog()
        {
            InitializeComponent();
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            Value = GroupTextBox.Text;
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
                    GroupTextBox.ItemsSource = _groups.FindAll(g => g.name.Contains(sender.Text)).Select(g => g.name);
            }
        }

        private async void GroupTextBox_Loaded(object sender, RoutedEventArgs e)
        {
            _groups = await Loader.Load();
            GroupTextBox.IsEnabled = true;
        }
    }
}
