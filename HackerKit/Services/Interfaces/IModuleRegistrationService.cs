using HackerKit.Models;
using System.Collections.ObjectModel;

namespace HackerKit.Services.Interfaces
{
	public interface IModuleRegistrationService
	{
		ObservableCollection<Category> GetCategories();
		void RegisterModules();
	}
}