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

		_toastService.ShowToastAsync("���ܻ��ڿ�����OVO", ToastType.Error, 3000);

	}

	private void Button_Clicked(object sender, TouchEventArgs e)
	{
		_toastService.ShowToastAsync("���������x", ToastType.Error, 3000);
	}
}