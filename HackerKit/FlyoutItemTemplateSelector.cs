using Xamarin.Forms;

namespace HackerKit
{
	public class FlyoutItemTemplateSelector : DataTemplateSelector
	{
		public DataTemplate HeaderTemplate { get; set; }
		public DataTemplate ItemTemplate { get; set; }

		protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
		{
			if (item is MenuItem menuItem && menuItem.Command == null)
			{
				return HeaderTemplate;
			}

			return ItemTemplate;
		}
	}
}
