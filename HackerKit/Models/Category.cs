using System.Collections.ObjectModel;

namespace HackerKit.Models
{
	public class Category
	{
		public string Name { get; set; }
		public object Icon { get; set; } = IconPacks.IconKind.Material.BuildCircle;
		public ObservableCollection<Module> Modules { get; set; } = [];
	}
}