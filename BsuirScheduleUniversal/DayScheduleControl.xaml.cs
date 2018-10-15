using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
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
using BsuirScheduleUniversal.ViewModels;
using System.ComponentModel;
using Windows.ApplicationModel;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace BsuirScheduleUniversal
{
    public sealed partial class DayScheduleControl : UserControl
    {
        public event Action<PairVM> PairSelected;
        public event Action PairDeleted;

        bool _contextIgnored = false;
        public DayScheduleControl()
        {
            this.InitializeComponent();
        }

        private void UserControl_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            if (!_contextIgnored)
            {
                DataContext = null;
                _contextIgnored = true;
            }
        }

        private void PairsListView_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            PairSelected?.Invoke((PairVM)e.AddedItems[0]);
        }

        private void MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            var daySchedule = (DayScheduleVM)DataContext;
            ((sender as MenuFlyoutItem).DataContext as PairVM).Delete();
            PairDeleted?.Invoke();
        }

        private void PairsListView_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            var pair = ((FrameworkElement)e.OriginalSource).DataContext as PairVM;
            if (pair == null)
                return;
            ListView listView = (ListView)sender;
            pairMenuFlyout.ShowAt(listView, e.GetPosition(listView));
            pairDeleteMenuFlyoutItem.DataContext = pair;
        }
    }
}
