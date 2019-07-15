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
                if (e.PropertyName == "selectedGroup") Reload();
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
            if (Loader.CachedGroupsArray != null)
            {
                foreach (var group in Loader.CachedGroupsArray)
                {
                    var panel = new StackPanel {Orientation = Orientation.Horizontal};

                    var button = new Button
                    {
                        Content = "X",
                        Margin = new Thickness(0, 0, 10, 0)
                    };

                    button.Click += (s, e) => DeleteGroup(group);
                    panel.Children.Add(button);

                    var name = new TextBlock {Text = group};
                    panel.Children.Add(name);


                    GroupComboBox.Items.Add(panel);
                    if (group == VM.SelectedGroup)
                        GroupComboBox.SelectedItem = panel;
                }
            }
            GroupComboBox.Items.Add("Load group...");
            _selectionLocked = false;
        }

        private async void GroupSelected(object sender, SelectionChangedEventArgs e)
        {
            if (_selectionLocked)
                return;

            if(GroupComboBox.SelectedItem == null) return;

            string value;
            if (GroupComboBox.SelectedItem is StackPanel panel)
                value = ((panel.Children[1] ?? panel.Children[0]) as TextBlock)?.Text;
            else
                value = GroupComboBox.SelectedItem as string;

            await VM.GroupSelected(value);
        }

        private void DeleteGroup(string group)
        {
            VM.DeleteGroup(group);
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
