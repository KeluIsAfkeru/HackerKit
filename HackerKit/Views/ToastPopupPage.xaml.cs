using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace HackerKit.Views
{
	/// <summary>
	/// 交互提示气泡框
	/// </summary>
	public partial class ToastPopupPage : PopupPage
	{
		public ToastPopupPage(string message)
		{
			InitializeComponent();
			MessageLabel.Text = message;
		}

		protected override async void OnAppearing()
		{
			base.OnAppearing();
			//气泡框淡入并上移
			await Task.WhenAll(
				ToastFrame.FadeTo(1, 300),
				ToastFrame.TranslateTo(0, 0, 300, Easing.SinOut)
			);

			//显示2秒后自动消失
			await Task.Delay(2000);

			await Task.WhenAll(
				ToastFrame.FadeTo(0, 300),
				ToastFrame.TranslateTo(0, 50, 300, Easing.SinIn)
			);

			await PopupNavigation.Instance.PopAsync();
		}
	}
}
