using HackerKit.ViewModels;
using Material.Components.Maui;
using Material.Components.Maui.Primitives;

namespace HackerKit.Views.Pages;

public partial class JWTToolView : ContentView
{
	public JWTToolView()
	{
		InitializeComponent();
		var services = IPlatformApplication.Current.Services;
		BindingContext = services.GetService<JWTToolViewModel>();

		//设置默认选中项
		if (ModeSelector.Items.Count > 0)
			ModeSelector.Items[0].IsSelected = true;

		if (AlgorithmComboBox.Items.Count > 0)
			AlgorithmComboBox.SelectedIndex = 0;
	}

	private void OnModeChanged(object sender, SelectedItemsChangedArgs<SegmentedItem> e)
	{
		if (BindingContext is JWTToolViewModel viewModel && e.SelectedItems.Any())
		{
			var selectedItem = e.SelectedItems.FirstOrDefault();
			if (selectedItem != null)
			{
				int index = ModeSelector.Items.IndexOf(selectedItem);
				viewModel.SelectedMode = index;
			}
		}
	}

	private void OnAlgorithmChanged(object sender, SelectedItemChangedEventArgs e)
	{
		if (e.SelectedItem is Material.Components.Maui.MenuItem menuItem && BindingContext is JWTToolViewModel viewModel)
		{
			string algorithmText = menuItem.Text;
			// 提取算法代码（如HS256）
			string algorithm = algorithmText.Split(' ')[0].Trim();
			viewModel.SelectedAlgorithm = algorithm;
		}
	}
}