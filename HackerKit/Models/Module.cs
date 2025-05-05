using System.Collections.ObjectModel;

namespace HackerKit.Models
{
	public class Module
	{
		public string Title { get; set; }
		public object Icon { get; set; } = IconPacks.IconKind.Material.BuildCircle;
		public ObservableCollection<ModuleItem> Items { get; set; } = [];
	}
}
