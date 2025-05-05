using HackerKit.Models;
using HackerKit.Services.Interfaces;
using HackerKit.ViewModels;
using HackerKit.Views.Pages;
using Material.Components.Maui;
using System.Windows.Input;

namespace HackerKit.Views
{
	public partial class MainPage : ContentPage
	{
		private bool _drawerIsOpen = false;
		private double _drawerWidth = 0;
		private readonly INavigationService _navigationService;
		private readonly IModuleRegistrationService _moduleRegistrationService;
		private readonly IToastService _toastService;

		private double _startDragPosition = 0;
		private bool _isDragging = false;
		private const double DragThreshold = 0.3; //��������رյ���ֵ���������ֵ�ɿ����۵�

		public MainPage()
		{
			InitializeComponent();

			//�����ʼ��
			var services = IPlatformApplication.Current.Services;
			_navigationService = services.GetService<INavigationService>();
			_moduleRegistrationService = services.GetService<IModuleRegistrationService>();
			_toastService = services.GetService<IToastService>();


			//��ViewModel�󶨵���ҳ��Context��
			//_toastViewModel = new();
			//ToastContainer.BindingContext = _toastViewModel;
			ToastsHost.BindingContext = services.GetService<ToastsHostViewModel>();

			//���ز˵�
			LoadMenu();

			//Ĭ����ʾ��ҳ����
			var homeView = _navigationService.ResolveModuleView("HomeIntro");
			MainContentView.Content = homeView;

			//if (NavDrawer.Items.Count > 0)
			//{
			//	//��ȡ��һ����ѡ���ģ����
			//	var firstModuleItem = NavDrawer.Items
			//		.OfType<NavigationDrawerItem>()
			//		.FirstOrDefault();

			//	if (firstModuleItem != null)
			//	{
			//		//ģ������һ��ģ����
			//		ModuleItem_Clicked(firstModuleItem, EventArgs.Empty);
			//	}
			//}
		}

		private void LoadMenu()
		{
			try
			{
				var categories = _moduleRegistrationService.GetCategories();
				LoadCategoriesIntoDrawer(categories);
			}
			catch (Exception ex)
			{
				Console.WriteLine($"���ز˵�����: {ex.Message}");
			}
		}

		private void LoadCategoriesIntoDrawer(System.Collections.ObjectModel.ObservableCollection<Category> categories)
		{
			//������в˵���
			NavDrawer.Items.Clear();

			foreach (var category in categories)
			{
				//��ӷ������
				NavDrawer.Items.Add(new Label
				{
					Text = category.Name,
					FontAttributes = FontAttributes.Bold,
					FontSize = 18,
					Padding = new Thickness(10, 8, 20, 10),
					TextColor = Application.Current.Resources["OnSurfaceVariantColor"] as Color
				});

				//��Ӹ÷����µ�����ģ��
				foreach (var module in category.Modules)
				{
					foreach (var item in module.Items)
					{
						var navItem = new NavigationDrawerItem
						{
							Text = item.Name,
							IconData = (string)item.Icon,
							FontFamily = "AlimamaThin",
							CommandParameter = item
						};
						navItem.Clicked += ModuleItem_Clicked;
						NavDrawer.Items.Add(navItem);
					}
				}
			}
		}

		protected override async void OnAppearing()
		{
			base.OnAppearing();

			_drawerWidth = Width * 0.8;
			NavDrawer.TranslationX = -_drawerWidth;
			NavDrawer.IsVisible = false;
			DrawerOverlay.IsVisible = false;
			_drawerIsOpen = false;

		}

		//�򿪲��������
		private async void MenuButton_Clicked(object sender, EventArgs e)
		{
			if (_drawerIsOpen) return;

			try
			{
				_drawerWidth = this.Width * 0.8;
				NavDrawer.WidthRequest = _drawerWidth;
				NavDrawer.TranslationX = -_drawerWidth;

				//�����ÿɼ������ݲ�ִ�ж���
				NavDrawer.IsVisible = true;
				DrawerOverlay.Opacity = 0;
				DrawerOverlay.IsVisible = true;

				//��UI�߳�һ��ʱ��׼��
				//await Task.Delay(16); //��Լһ֡ʱ��

				//ִ�ж���
				_drawerIsOpen = true;
				await Task.WhenAll(
					NavDrawer.TranslateTo(0, 0, 180, Easing.CubicOut),
					DrawerOverlay.FadeTo(0.3, 180)
				);
			}
			catch (Exception ex)
			{
				Console.WriteLine($"�򿪳������: {ex.Message}");
			}
		}

		//���ֵ���رղ����
		private async void Overlay_Tapped(object sender, EventArgs e)
		{
			await HideDrawerAsync();
		}

		private async Task HideDrawerAsync()
		{
			if (!_drawerIsOpen && !_isDragging) return;

			try
			{
				MenuButton.IsEnabled = false;
				MainContentView.IsEnabled = false;

				var translateAnimation = NavDrawer.TranslateTo(-_drawerWidth, 0, 200, Easing.SinIn);
				var fadeAnimation = DrawerOverlay.FadeTo(0, 200);

				await Task.WhenAll(translateAnimation, fadeAnimation);

				DrawerOverlay.IsVisible = false;
				NavDrawer.IsVisible = false;
				_drawerIsOpen = false;
			}
			catch (Exception ex)
			{
				Console.WriteLine($"���س������: {ex.Message}");
			}
			finally
			{
				MenuButton.IsEnabled = true;
				MainContentView.IsEnabled = true;
			}
		}


		//���Drawer���ر�Drawer
		private async void NavigationItem_Clicked(object sender, EventArgs e)
		{
			try
			{
				if (sender is NavigationDrawerItem item && item == SettingsItem)
				{
					//����ҳ��
					var settingsView = new DefaultView();
					MainContentView.Content = settingsView;
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"NavigationItem_Clicked����: {ex.Message}");
			}
			await HideDrawerAsync();
		}

		private async void ModuleItem_Clicked(object sender, EventArgs e)
		{
			await HideDrawerAsync();
			try
			{
				if (sender is NavigationDrawerItem item && item.CommandParameter is ModuleItem moduleItem)
				{
					ContentView newModuleView = _navigationService.ResolveModuleView(moduleItem.ViewType);
					var currentView = MainContentView.Content;

					if (currentView != null)
						await currentView.FadeTo(0, 150, Easing.CubicIn);

					newModuleView.Opacity = 0;
					newModuleView.Scale = 0.92;
					newModuleView.TranslationY = 30;


					MainContentView.Content = newModuleView;

					await Task.WhenAll(
						newModuleView.FadeTo(1, 280, Easing.CubicOut),
						newModuleView.ScaleTo(1, 350, Easing.SpringOut),
						newModuleView.TranslateTo(0, 0, 300, Easing.CubicOut)
					);
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"ModuleItem_Clicked����: {ex.Message}");
			}
		}

		//���������϶��¼�
		private void OnDrawerPanUpdated(object sender, PanUpdatedEventArgs e)
		{
			switch (e.StatusType)
			{
				case GestureStatus.Started:
					//��ʼ�϶�����¼��ʼλ��
					_startDragPosition = NavDrawer.TranslationX;
					_isDragging = true;
					break;

				case GestureStatus.Running:
					if (!NavDrawer.IsVisible) return;

					//������λ�ã��������ں���Χ��
					double newX = _startDragPosition + e.TotalX;
					//���Ʋ����ϳ��Ҳ�߽磬�Ҳ����Ϲ�ȫ��
					newX = Math.Min(0, Math.Max(-_drawerWidth, newX));

					//���ó���λ��
					NavDrawer.TranslationX = newX;

					//���ݳ���λ�õ�������͸����
					double progress = 1 + (newX / _drawerWidth); //0(�ر�)��1(��ȫ��)
					DrawerOverlay.Opacity = 0.3 * progress;
					break;

				case GestureStatus.Completed:
				case GestureStatus.Canceled:
					if (!NavDrawer.IsVisible) return;

					_isDragging = false;

					double closedThreshold = -_drawerWidth * DragThreshold;

					//�Ƚϵ�ǰλ�ú���ֵ�������Ƿ�ر�
					if (NavDrawer.TranslationX < closedThreshold)
					{
						//����϶�������ֵ���رճ���
						HideDrawerAsync();
					}
					else
					{
						//����ص���ȫ��״̬
						CompleteOpenDrawerAnimation();
					}
					break;
			}
		}

		// ��ɴ򿪳���Ķ���
		private async Task CompleteOpenDrawerAnimation()
		{
			await NavDrawer.TranslateTo(0, 0, 150, Easing.CubicOut);
			DrawerOverlay.Opacity = 0.3;
			_drawerIsOpen = true;
		}

		private void OnOverlayPanUpdated(object sender, PanUpdatedEventArgs e)
		{
			//ֻ���ڳ�����ʾʱ�Ŵ������ֵ�����
			if (!DrawerOverlay.IsVisible) return;

			OnDrawerPanUpdated(NavDrawer, e);
		}

		private void SettingsButton_Clicked(object sender, EventArgs e)
		{
			var homeView = new HomeIntroView();
			MainContentView.Content = homeView;
		}
	}
}
