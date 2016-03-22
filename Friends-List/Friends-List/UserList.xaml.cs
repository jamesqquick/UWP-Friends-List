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
	public sealed partial class UserList : Page
	{
		List<User> users = new List<User>();
		public UserList()
		{
			this.InitializeComponent();
			this.Loaded += UserList_Loaded;
		}

		protected override void OnNavigatedTo(NavigationEventArgs e)
		{
			//Show back button
			if (Frame.CanGoBack)
			{
				SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
				SystemNavigationManager.GetForCurrentView().BackRequested += UserList_BackRequested;
			}
			else
			{
				SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
			}
			base.OnNavigatedTo(e);	
		}

		protected override void OnNavigatedFrom(NavigationEventArgs e)
		{
			SystemNavigationManager.GetForCurrentView().BackRequested -= UserList_BackRequested;
			base.OnNavigatedFrom(e);
		}

		private void UserList_Loaded(object sender, RoutedEventArgs e)
		{
			

		}

		private void UserList_BackRequested(object sender, BackRequestedEventArgs e)
		{
			if (this.Frame.CanGoBack)
				this.Frame.GoBack();
		}

		public async void LoadUsersAsync(string searchString)
		{
			users.Clear();
			try
			{
				users = await App.mobileService.GetTable<User>()
					.Where(User => User.Username != App.mobileService.CurrentUser.UserId)
					.Where(User => User.Username == searchString)

					.ToListAsync();
				UsersListView.ItemsSource = users;
				//Update ListView to show no selected Item
				UsersListView.SelectedIndex = -1;
			}
			catch (Exception ex)
			{
				Debug.WriteLine("Table Query Exception: " + ex.Message);
				new Windows.UI.Popups.MessageDialog("Trouble Retrieving ToDo Itens");
			}
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

		private void UsersListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			//Toggle the Delete and Edit Button when an item is selected
			if (((ListView)sender).SelectedIndex == -1)
			{
				AddButton.IsEnabled = false;
			}
			else
			{
				AddButton.IsEnabled = true;
			}
		}

		private void SearchBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
		{
			if (!SearchBox.Text.Equals(""))
				LoadUsersAsync(args.QueryText);
			else
				DisplayUserMessage("Please enter a search");
		}

		private void SearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
		{
			//Update the query everytime the user makes a change to the box
		}

		private async void AddButton_Click(object sender, RoutedEventArgs e)
		{
			User tempFriend = (User) UsersListView.SelectedItem;
			string response = await AzureUserHelper.AddFriendAsync(tempFriend.Username);
			if (response.Equals("Success"))
			{
				//Update searchbox and listview
				users = new List<User>();
				UsersListView.ItemsSource = users;
				SearchBox.Text = "";
				UsersListView.SelectedIndex = -1;	
				//Let the user know the friend was successfully added
				DisplayUserMessage("Friend added");
			}
			else
				DisplayUserMessage(response);
		}

		private async void DisplayUserMessage(string message)
		{
			await new Windows.UI.Popups.MessageDialog(message).ShowAsync();
		}
	}
}
