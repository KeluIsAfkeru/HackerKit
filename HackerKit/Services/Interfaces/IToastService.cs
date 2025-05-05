using HackerKit.Models;

namespace HackerKit.Services.Interfaces
{
	public interface IToastService
	{
		Task ShowToastAsync(string message, ToastType type = ToastType.Info, int durationMs = 3000);
	}
}