using Rg.Plugins.Popup.Services;
using System.Threading.Tasks;

namespace HackerKit.Services
{
	public static class ToastService
	{
		//显示气泡框
		public static async Task ShowToast(string message) => await PopupNavigation.Instance.PushAsync(new Views.ToastPopupPage(message));
	}
}
