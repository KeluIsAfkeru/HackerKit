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
		private const double DragThreshold = 0.3; //侧边栏被关闭的阈值，超过这个值松开就折叠

		public MainPage()
		{
			InitializeComponent();

			//服务初始化
			var services = IPlatformApplication.Current.Services;
			_navigationService = services.GetService<INavigationService>();
			_moduleRegistrationService = services.GetService<IModuleRegistrationService>();
			_toastService = services.GetService<IToastService>();


			//将ViewModel绑定到主页的Context里
			//_toastViewModel = new();
			//ToastContainer.BindingContext = _toastViewModel;
			ToastsHost.BindingContext = services.GetService<ToastsHostViewModel>();

			//加载菜单
			LoadMenu();

			//默认显示主页介绍
			var homeView = _navigationService.ResolveModuleView("HomeIntro");
			MainContentView.Content = homeView;

			//if (NavDrawer.Items.Count > 0)
			//{
			//	//获取第一个可选择的模块项
			//	var firstModuleItem = NavDrawer.Items
			//		.OfType<NavigationDrawerItem>()
			//		.FirstOrDefault();

			//	if (firstModuleItem != null)
			//	{
			//		//模拟点击第一个模块项
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
				Console.WriteLine($"加载菜单错误: {ex.Message}");
			}
		}

		private void LoadCategoriesIntoDrawer(System.Collections.ObjectModel.ObservableCollection<Category> categories)
		{
			//清空现有菜单项
			NavDrawer.Items.Clear();

			foreach (var category in categories)
			{
				//添加分类标题
				NavDrawer.Items.Add(new Label
				{
					Text = category.Name,
					FontAttributes = FontAttributes.Bold,
					FontSize = 18,
					Padding = new Thickness(10, 8, 20, 10),
					TextColor = Application.Current.Resources["OnSurfaceVariantColor"] as Color
				});

				//添加该分类下的所有模块
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

		//打开侧边栏动画
		private async void MenuButton_Clicked(object sender, EventArgs e)
		{
			if (_drawerIsOpen) return;

			try
			{
				_drawerWidth = this.Width * 0.8;
				NavDrawer.WidthRequest = _drawerWidth;
				NavDrawer.TranslationX = -_drawerWidth;

				//先设置可见，但暂不执行动画
				NavDrawer.IsVisible = true;
				DrawerOverlay.Opacity = 0;
				DrawerOverlay.IsVisible = true;

				//给UI线程一点时间准备
				//await Task.Delay(16); //大约一帧时间

				//执行动画
				_drawerIsOpen = true;
				await Task.WhenAll(
					NavDrawer.TranslateTo(0, 0, 180, Easing.CubicOut),
					DrawerOverlay.FadeTo(0.3, 180)
				);
			}
			catch (Exception ex)
			{
				Console.WriteLine($"打开抽屉错误: {ex.Message}");
			}
		}

		//遮罩点击关闭侧边栏
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
				Console.WriteLine($"隐藏抽屉错误: {ex.Message}");
			}
			finally
			{
				MenuButton.IsEnabled = true;
				MainContentView.IsEnabled = true;
			}
		}


		//点击Drawer项后关闭Drawer
		private async void NavigationItem_Clicked(object sender, EventArgs e)
		{
			try
			{
				if (sender is NavigationDrawerItem item && item == SettingsItem)
				{
					//设置页面
					var settingsView = new DefaultView();
					MainContentView.Content = settingsView;
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"NavigationItem_Clicked错误: {ex.Message}");
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
				Console.WriteLine($"ModuleItem_Clicked错误: {ex.Message}");
			}
		}

		//处理抽屉的拖动事件
		private void OnDrawerPanUpdated(object sender, PanUpdatedEventArgs e)
		{
			switch (e.StatusType)
			{
				case GestureStatus.Started:
					//开始拖动，记录初始位置
					_startDragPosition = NavDrawer.TranslationX;
					_isDragging = true;
					break;

				case GestureStatus.Running:
					if (!NavDrawer.IsVisible) return;

					//计算新位置，但限制在合理范围内
					double newX = _startDragPosition + e.TotalX;
					//限制不能拖出右侧边界，且不能拖过全宽
					newX = Math.Min(0, Math.Max(-_drawerWidth, newX));

					//设置抽屉位置
					NavDrawer.TranslationX = newX;

					//根据抽屉位置调整遮罩透明度
					double progress = 1 + (newX / _drawerWidth); //0(关闭)到1(完全打开)
					DrawerOverlay.Opacity = 0.3 * progress;
					break;

				case GestureStatus.Completed:
				case GestureStatus.Canceled:
					if (!NavDrawer.IsVisible) return;

					_isDragging = false;

					double closedThreshold = -_drawerWidth * DragThreshold;

					//比较当前位置和阈值来决定是否关闭
					if (NavDrawer.TranslationX < closedThreshold)
					{
						//如果拖动超过阈值，关闭抽屉
						HideDrawerAsync();
					}
					else
					{
						//否则回到完全打开状态
						CompleteOpenDrawerAnimation();
					}
					break;
			}
		}

		// 完成打开抽屉的动画
		private async Task CompleteOpenDrawerAnimation()
		{
			await NavDrawer.TranslateTo(0, 0, 150, Easing.CubicOut);
			DrawerOverlay.Opacity = 0.3;
			_drawerIsOpen = true;
		}

		private void OnOverlayPanUpdated(object sender, PanUpdatedEventArgs e)
		{
			//只有在抽屉显示时才处理遮罩的手势
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
