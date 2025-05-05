using HackerKit.Services.Interfaces;
using HackerKit.Views;
using Microsoft.Extensions.DependencyInjection;
using Application = Microsoft.Maui.Controls.Application;

namespace HackerKit
{
	public partial class App : Application
	{
		private readonly INavigationService _navigationService;
		private readonly IModuleRegistrationService _moduleRegistrationService;

		public App(IServiceProvider serviceProvider)
		{
			InitializeComponent();
			_navigationService = serviceProvider.GetRequiredService<INavigationService>();
			_moduleRegistrationService = serviceProvider.GetRequiredService<IModuleRegistrationService>();
			_moduleRegistrationService.RegisterModules(); //注册所有模块分类
			MainPage = new MainPage();
		}

		protected override void OnStart()
		{
			//应用启动时预加载
			_moduleRegistrationService.RegisterModules();

			//收集所有模块类型
			var moduleTypes = new List<string>() { "HomeIntro"};
			foreach (var category in _moduleRegistrationService.GetCategories())
				foreach (var module in category.Modules)
					foreach (var item in module.Items)
						moduleTypes.Add(item.ViewType);

			//预加载所有模块
			_navigationService.PreloadModuleViews(moduleTypes);
		}
	}
}
