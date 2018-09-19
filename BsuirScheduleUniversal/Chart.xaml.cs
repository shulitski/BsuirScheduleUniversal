using BsuirScheduleUniversal.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace BsuirScheduleUniversal
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Chart : Page
    {
        private static ApplicationDataContainer LocalSettings => ApplicationData.Current.LocalSettings;

        private int CheckedSubgroup => (LocalSettings.Values["checkedSubgroup"] as int?) ?? 0;

        public string SelectedGroup => LocalSettings.Values["selectedGroup"] as string;

        public Chart()
        {
            this.InitializeComponent();
            Reload();
        }

        private async void Reload()
        {
            if (ChartDridView == null) return;
            if (SelectedGroup == null) return;

            try
            {
                ChartDridView.ItemsSource = null;

                var schedule = new List<ChartDayScheduleVM>();
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
                    schedule.Add(await ChartDayScheduleVM.Create(SelectedGroup, day.AddDays(i), CheckedSubgroup));
                }

                ChartDridView.ItemsSource = schedule;
                //ChartDridView.SelectedIndex = currentDayIndex;
            }
            catch (Exception)
            {
                // ignored
            }

        }
    }
}
