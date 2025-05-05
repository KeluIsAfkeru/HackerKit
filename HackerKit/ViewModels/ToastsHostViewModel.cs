using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using HackerKit.Models;

namespace HackerKit.ViewModels
{
	public partial class ToastsHostViewModel : ObservableObject
	{
		public ObservableCollection<ToastViewModel> Toasts { get; } = [];

		public void AddToast(ToastModel toast)
		{
			var toastVm = new ToastViewModel(toast);
			Toasts.Insert(0, toastVm); //新气泡显示在最上面
			DismissAfterDelayAsync(toastVm, toast.Duration);
		}

		private async void DismissAfterDelayAsync(ToastViewModel toastVm, int duration)
		{
			await Task.Delay(duration);
			//淡出动画
			for (double opacity = 1; opacity >= 0; opacity -= 0.08)
			{
				toastVm.Opacity = opacity;
				await Task.Delay(20);
			}
			MainThread.BeginInvokeOnMainThread(() => Toasts.Remove(toastVm));
		}
	}
}