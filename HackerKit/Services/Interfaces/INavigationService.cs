using HackerKit.Models;

namespace HackerKit.Services.Interfaces
{
	public interface INavigationService
	{
		Task NavigateToModuleAsync(ModuleItem moduleItem);
		ContentView ResolveModuleView(string viewType);
		void PreloadModuleViews(IEnumerable<string> viewTypes);
	}
}