using HackerKit.Models;
using HackerKit.Services.Interfaces;

namespace HackerKit.Services;

[Singleton]
public class ClipboardService : IClipboardService
{
	public async Task<string> GetTextAsync()
	{
		if (Clipboard.HasText)
			return await Clipboard.GetTextAsync();
		return string.Empty;
	}

	public Task SetTextAsync(string text) => Clipboard.SetTextAsync(text);
}
