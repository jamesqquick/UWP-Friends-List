using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Security.Credentials;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Friends_List
{
	/// <summary>
	/// An empty page that can be used on its own or navigated to within a Frame.
	/// </summary>
	public sealed partial class FriendsList : Page
	{
		List<string> friends = new List<string>();
		public FriendsList()
		{
			this.InitializeComponent();
			this.Loaded += FriendsList_Loaded;
		}

		protected override void OnNavigatedTo(NavigationEventArgs e)
		{
			//Hide Back Button
			SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
			base.OnNavigatedTo(e);
		}
		private void FriendsList_Loaded(object sender, RoutedEventArgs e)
		{
			LoadFriendsList();
		}

		private void LoadFriendsList()
		{
			friends = JsonConvert.DeserializeObject<List<string>>(App.currentUser.Friends);
			FriendsListView.ItemsSource = friends;
        }

		private async void DeleteButton_Click(object sender, RoutedEventArgs e)
		{
			string response = await AzureUserHelper.DeleteFriendAsync((string)FriendsListView.SelectedItem);
			if (response.Equals("Success"))
			{
				LoadFriendsList();
			}
			else
				DisplayUserMessage(response);
		}

		private void AddButton_Click(object sender, RoutedEventArgs e)
		{
			this.Frame.Navigate(typeof(UserList));
		}

		private async void LogoutButton_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				//Removed stored credentials for user
				PasswordVault vault = new PasswordVault();
				vault.Remove(vault.Retrieve("Custom", App.mobileService.CurrentUser.UserId));
				//Log out of MobileServiceClient
				await App.mobileService.LogoutAsync();
				Debug.WriteLine("Logout Successful");
				//Navigate to Login Page
				this.Frame.Navigate(typeof(LoginPage));

			}
			catch (Exception ex)
			{
				Debug.WriteLine("Logout Exception: " + ex.Message);

			}
		}

		private void FriendsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			//Toggle the Delete and Edit Button when an item is selected
			if (((ListView)sender).SelectedIndex == -1)
			{
				DeleteButton.IsEnabled = false;
			}
			else
			{
				DeleteButton.IsEnabled = true;
			}
		}

		private async void DisplayUserMessage(string message)
		{
			await new Windows.UI.Popups.MessageDialog(message).ShowAsync();
		}


	}
}
