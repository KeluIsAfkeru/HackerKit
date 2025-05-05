using HackerKit.Models;
using HackerKit.Services.Interfaces;
using HackerKit.ViewModels;

namespace HackerKit.Services
{
	[Singleton]
	public class ToastService : IToastService
	{
		private readonly ToastsHostViewModel _hostViewModel;

		public ToastService(ToastsHostViewModel hostViewModel)
		{
			_hostViewModel = hostViewModel;
		}

		public Task ShowToastAsync(string message, ToastType type = ToastType.Info, int durationMs = 3000)
		{
			var toast = new ToastModel
			{
				Message = message,
				Type = type,
				Duration = durationMs
			};
			MainThread.BeginInvokeOnMainThread(() => _hostViewModel.AddToast(toast));
			return Task.CompletedTask;
		}
	}
}