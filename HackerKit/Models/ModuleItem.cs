namespace HackerKit.Models
{
	public class ModuleItem
	{
		public string Name { get; set; }
		public object Icon { get; set; } = IconPacks.IconKind.Material.BuildCircle;
		public string ViewType { get; set; }
	}
}