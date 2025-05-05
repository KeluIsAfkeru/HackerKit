namespace HackerKit.Services.Interfaces
{
	public interface IClipboardService
	{
		Task<string> GetTextAsync();
		Task SetTextAsync(string text);
	}
}
