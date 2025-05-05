using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using HackerKit.Models;
using HackerKit.Services.Interfaces;
using HackerKit.Views.Pages;

namespace HackerKit.ViewModels;

public class MainPageViewModel : INotifyPropertyChanged
{
	private readonly INavigationService _navigationService;
	private readonly IModuleRegistrationService _moduleRegistrationService;
	private readonly IToastService _toastService;

	public MainPageViewModel(
		INavigationService navigationService,
		IModuleRegistrationService moduleRegistrationService,
		IToastService toastService)
	{
		_navigationService = navigationService;
		_moduleRegistrationService = moduleRegistrationService;
		_toastService = toastService;

		//初始化命令
		MenuButtonCommand = new Command(OnMenuButtonClicked);
		OverlayTappedCommand = new Command(OnOverlayTapped);
		ModuleItemCommand = new Command<ModuleItem>(OnModuleItemClicked);
		SettingsItemCommand = new Command(OnSettingsItemClicked);

		//注册所有模块
		_moduleRegistrationService.RegisterModules();

		//加载分类数据
		Categories = _moduleRegistrationService.GetCategories();
	}

	//标题
	private string _title = "HackerKit";
	public string Title
	{
		get => _title;
		set { _title = value; OnPropertyChanged(); }
	}

	//功能模块分类
	private ObservableCollection<Category> _categories;
	public ObservableCollection<Category> Categories
	{
		get => _categories;
		set { _categories = value; OnPropertyChanged(); }
	}

	// 当前显示的内容
	private View _currentContent;
	public View CurrentContent
	{
		get => _currentContent;
		set { _currentContent = value; OnPropertyChanged(); }
	}

	// 是否显示抽屉
	private bool _isDrawerOpen;
	public bool IsDrawerOpen
	{
		get => _isDrawerOpen;
		set { _isDrawerOpen = value; OnPropertyChanged(); }
	}

	// 命令
	public ICommand MenuButtonCommand { get; }
	public ICommand OverlayTappedCommand { get; }
	public ICommand ModuleItemCommand { get; }
	public ICommand SettingsItemCommand { get; }

	// 抽屉状态变化事件
	public event EventHandler<bool> DrawerStateChanged;

	// 菜单按钮点击处理
	private void OnMenuButtonClicked()
	{
		IsDrawerOpen = true;
		DrawerStateChanged?.Invoke(this, true);
	}

	// 遮罩点击处理
	private void OnOverlayTapped()
	{
		CloseDrawer();
	}

	// 模块项点击处理
	private void OnModuleItemClicked(ModuleItem item)
	{
		LoadModuleView(item);
		CloseDrawer();
	}

	// 设置项点击处理
	private void OnSettingsItemClicked()
	{
		LoadSettingsView();
		CloseDrawer();
	}

	// 加载模块视图
	public void LoadModuleView(ModuleItem item)
	{
		try
		{
			var view = _navigationService.ResolveModuleView(item.ViewType);
			if (view != null)
			{
				CurrentContent = view;
			}
		}
		catch (Exception ex)
		{
			Console.WriteLine($"加载模块视图错误: {ex.Message}");
			_toastService?.ShowToastAsync($"加载模块失败: {ex.Message}", ToastType.Error, 2000);
		}
	}

	// 加载设置视图
	private void LoadSettingsView()
	{
		CurrentContent = new DefaultView();
	}

	// 关闭抽屉
	private void CloseDrawer()
	{
		IsDrawerOpen = false;
		DrawerStateChanged?.Invoke(this, false);
	}

	// 选择第一个可用模块（应用启动时调用）
	public void SelectFirstModule()
	{
		if (Categories != null && Categories.Count > 0)
		{
			foreach (var category in Categories)
			{
				foreach (var module in category.Modules)
				{
					if (module.Items.Count > 0)
					{
						LoadModuleView(module.Items[0]);
						return;
					}
				}
			}
		}
	}

	// INotifyPropertyChanged 实现
	public event PropertyChangedEventHandler PropertyChanged;
	protected void OnPropertyChanged([CallerMemberName] string name = null)
		=> PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
