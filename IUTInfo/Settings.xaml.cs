/*	
	This file is part of IUTInfo.

	IUTInfo is free software: you can redistribute it and/or modify
	it under the terms of the GNU Lesser General Public License as published by
	the Free Software Foundation, either version 3 of the License, or
	(at your option) any later version.

	IUTInfo is distributed in the hope that it will be useful,
	but WITHOUT ANY WARRANTY; without even the implied warranty of
	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
	GNU Lesser General Public License for more details.

	You should have received a copy of the GNU Lesser General Public License
	along with IUTInfo.  If not, see <http://www.gnu.org/licenses/>.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace IUTInfo
{
    public partial class Settings
    {
        public Settings()
        {
            InitializeComponent();
            
            EntityListPicker.ItemsSource = App.Planning.Entities.Values;
            EntityListPicker.SelectedItem = App.Planning.SelectedEntity;
            EntityListPicker.SelectionChanged += (sendr, args) =>
            {
                if (args.AddedItems.Count <= 0)
                    return;

                App.Planning.SelectedEntityID = ((ADE_AMU_IUT_Info_Planning.EntityData)args.AddedItems[0]).Id;

                App.Planning.Save("Planning.xml");
                App.Planning.ResetCache();
            };

            WeekListPicker.ItemsSource = App.Planning.WeekIdPerMondayDate;
            WeekListPicker.SelectedItem = App.Planning.WeekIdPerMondayDate.Single(o => o.Key == App.Planning.SelectedWeekId);
            WeekListPicker.SelectionChanged += (o, args) =>
            {
                if (args.AddedItems.Count <= 0)
                    return;

                App.Planning.SelectedWeekId = ((KeyValuePair<uint, string>)args.AddedItems[0]).Key;
                App.Planning.Save("Planning.xml");

                NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.Relative));
            };
        }

        private void ResetCache_Click(object sender, RoutedEventArgs e)
        {
            App.Planning.ResetCache();
        }

        private void ThisWeek_Click(object sender, RoutedEventArgs e)
        {
            WeekListPicker.SelectedItem = App.Planning.WeekIdPerMondayDate.Single(o => o.Key == App.Planning.CurrentWeekId);
        }
    }
}