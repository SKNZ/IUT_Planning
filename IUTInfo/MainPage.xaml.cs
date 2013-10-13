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
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Microsoft.Phone.Controls;
using Microsoft.Xna.Framework.GamerServices;
using GestureEventArgs = System.Windows.Input.GestureEventArgs;

namespace IUTInfo
{
    public partial class MainPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private async void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (App.Planning == null)
            {
                App.Planning = ADE_AMU_IUT_Info_Planning.Load("Planning.xml") ?? new ADE_AMU_IUT_Info_Planning();

                App.Planning.Initialize();

                // Progress overlay when download stuff
                App.Planning.NetworkOperationStartedCallback += OnPlanningNetworkOperationStartedCallback;
                App.Planning.NetworkOperationFinishedCallback += OnPlanningNetworkOperationFinishedCallback;
            }
            
            await Refresh();
        }
        
        private void OnPlanningNetworkOperationStartedCallback()
        {
            ProgressOverlay.Show();
            ContentPanel.Opacity = 0.1f;
        }

        private void OnPlanningNetworkOperationFinishedCallback()
        {
            ProgressOverlay.Hide();
            ContentPanel.Opacity = 1.0f;
        }

        public async Task Refresh()
        {
            try
            {
                ContentPanel.Background = new ImageBrush
                {
                    ImageSource = await App.Planning.SelectedWeekPlanningImage
                };
            }
            catch (NetworkNotAvailableException)
            {
                MessageBox.Show("Une connexion réseau est requise pour le premier lancement de cette application.");
                Application.Current.Terminate();
            }
        }

        private void ContentPannel_DoubleTap(object sender, GestureEventArgs e)
        {
            NavigationService.Navigate(new Uri("/Settings.xaml", UriKind.Relative));
        }

        private async void OnFlick(object sender, FlickGestureEventArgs e)
        {
            try
            {
                if (ProgressOverlay.Visibility == Visibility.Visible)
                    return;

                var result = e.HorizontalVelocity < 0 ? App.Planning.MoveNextWeek() : App.Planning.MovePreviousWeek();

                if (result)
                    await Refresh();
            }
            catch (NetworkNotAvailableException)
            {
                MessageBox.Show("La semaine suivante/précedante n'ayant jamais été vue n'a pas été sauvegardée pour consultation hors ligne.");
            }
        }
    }
}