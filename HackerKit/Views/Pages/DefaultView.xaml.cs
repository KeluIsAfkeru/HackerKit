using HackerKit.Models;
using HackerKit.Services.Interfaces;
using HackerKit.ViewModels;

namespace HackerKit.Views.Pages;

public partial class DefaultView : ContentView
{
	private IToastService _toastService;
	public DefaultView()
	{
		InitializeComponent();
		var services = IPlatformApplication.Current.Services;
		_toastService = services.GetService<IToastService>();

		_toastService.ShowToastAsync("功能还在开发中OVO", ToastType.Error, 3000);

	}

	private void Button_Clicked(object sender, TouchEventArgs e)
	{
		_toastService.ShowToastAsync("你点密码呢x", ToastType.Error, 3000);
	}
}