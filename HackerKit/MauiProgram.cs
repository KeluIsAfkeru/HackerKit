using HackerKit.Services;
using HackerKit.Services.Interfaces;
using HackerKit.ViewModels;
using HackerKit.Views;
using HackerKit.Models;
using Material.Components.Maui.Extensions;
using Microsoft.Extensions.Logging;
#if ANDROID
using HackerKit.Platforms.Android;
#endif

namespace HackerKit
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
				.UseMaterialComponents()
				.ConfigureFonts(fonts =>
                {
					fonts.AddFont("AlimamaFangYuanTiVF-Thin.ttf", "AlimamaThin");
					//fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
					//fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
				});
			builder.ConfigureMauiHandlers(handlers =>
			{
				Microsoft.Maui.Handlers.EditorHandler.Mapper.AppendToMapping("EnableVerticalScroll", (handler, view) =>
				{
					#if ANDROID
						handler.PlatformView.VerticalScrollBarEnabled = true;
						handler.PlatformView.SetOnTouchListener(new EditorScrollTouchListener());
					#endif
				});
					#if ANDROID
						handlers.AddHandler(typeof(SelectableEditor), typeof(SelectableEditorHandler));
					#endif
			});//通过添加触摸监听器解决编辑框和页面scrollbar滚动冲突的bug

			RegisterService(builder); //注册所有服务，包括接口页面和ViewModel

			#if DEBUG
				builder.Logging.AddDebug();
			#endif

			return builder.Build(); ;
        }

		private static void RegisterService(MauiAppBuilder builder)
		{
			//使用自定义接口服务标签一键注册所有服务
			builder.Services.RegisterAllServicesWithAttributes();

			//注册ModelView服务
			builder.Services.AddSingleton<ToastViewModel>();
			builder.Services.AddSingleton<ToastsHostViewModel>();
			builder.Services.AddSingleton<TextEncodingConverterViewModel>();
			builder.Services.AddSingleton<BinaryCodeCalculatorViewModel>();
			builder.Services.AddSingleton<BaseConverterViewModel>();
			builder.Services.AddTransient<JWTToolViewModel>();
			builder.Services.AddTransient<ProtobufConverterViewModel>();
			builder.Services.AddTransient<FakeFileViewModel>();
		}

	}
}
