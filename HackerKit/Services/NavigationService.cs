using HackerKit.Models;
using HackerKit.Services.Interfaces;
using HackerKit.Views;
using HackerKit.Views.Pages;
using System.Reflection;

namespace HackerKit.Services
{
	[Singleton]
	public class NavigationService : INavigationService
	{
		private readonly IServiceProvider _serviceProvider;
		private readonly Dictionary<string, Func<ContentView>> _viewResolvers = new()
		{
			//编码工具
			["TextEncodingConverter"] = () => new TextEncodingConverterView(),
			["BinaryCodeCalculator"] = () => new BinaryCodeCalculatorView(),
			["Default"] = () => new DefaultView(),
			["HomeIntro"] = () => new HomeIntroView(),
			//["UnicodeConverter"] = () => new UnicodeConverterView(),
			//["UrlConverter"] = () => new UrlConverterView(),
			["BaseConverter"] = () => new BaseConverterView(),

			//数字工具
			["JWTTool"] = () => new JWTToolView(),

			//加密解密
			//["AesEncryption"] = () => new AesEncryptionView(),
			//["Md5Hash"] = () => new Md5HashView(),

			//Protobuf
			["ProtobufConverter"] = () => new ProtobufConverterView(),
			//["ProtobufDecoder"] = () => new ProtobufDecoderView(),

			//QQ FakeFile
			["FakeFile"] = () => new FakeFileView(),
		};
		private readonly Dictionary<string, ContentView> _cachedViews = []; //缓存字典

		public NavigationService(IServiceProvider serviceProvider)
		{
			_serviceProvider = serviceProvider;
		}

		public Task NavigateToModuleAsync(ModuleItem moduleItem)
		{
			//模块导航逻辑
			return Task.CompletedTask;
		}

		//在应用启动时调用此方法预加载所有页面
		public void PreloadModuleViews(IEnumerable<string> viewTypes)
		{
			foreach (var viewType in viewTypes)
			{
				//预加载并缓存
				if (!_cachedViews.ContainsKey(viewType) && _viewResolvers.ContainsKey(viewType))
				{
					_cachedViews[viewType] = _viewResolvers[viewType]();
				}
			}
		}

		public ContentView ResolveModuleView(string viewType)
		{
			try
			{
				//本来想着通过动态反射拿页面服务的
				//但是windows会自动裁剪掉没有使用过的程序集
				//还是老老实实手动暴力枚举罢
				if (_cachedViews.TryGetValue(viewType, out var cachedView))
					return cachedView;

				if (_viewResolvers.TryGetValue(viewType, out var factory))
				{
					var newView = factory();
					_cachedViews[viewType] = newView;
					return newView;
				}

				return _cachedViews["Default"];
				#region xxx
				//var viewTypeName = $"HackerKit.Views.{viewType}View";
				//var type = Assembly.GetExecutingAssembly().GetType(viewTypeName);

				//if (type != null && typeof(ContentView).IsAssignableFrom(type))
				//{
				//	// 用依赖注入解析
				//	return (ContentView)_serviceProvider.GetService(type)
				//		   ?? (ContentView)Activator.CreateInstance(type);
				//}

				//return new DefaultView(); 
				#endregion
			}
			catch (Exception ex)
			{
				Console.WriteLine($"解析模块视图错误: {ex.Message}");
				return new DefaultView();
			}
		}
	}
}