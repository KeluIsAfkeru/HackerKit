using HackerKit.ViewModels;
using Material.Components.Maui;
using Material.Components.Maui.Primitives;

namespace HackerKit.Views.Pages;

public partial class ProtobufConverterView : ContentView
{
	public ProtobufConverterView()
	{
		InitializeComponent();
		var services = IPlatformApplication.Current.Services;
		BindingContext = services.GetService<ProtobufConverterViewModel>();

		// 设置默认选中项
		if (ModeSelector.Items.Count > 0)
			ModeSelector.Items[0].IsSelected = true;

		if (DecodeModePicker.Items.Count > 0)
			DecodeModePicker.SelectedIndex = 0;
	}

	private void OnModeChanged(object sender, SelectedItemsChangedArgs<SegmentedItem> e)
	{
		if (BindingContext is ProtobufConverterViewModel viewModel && e.SelectedItems.Any())
		{
			var selectedItem = e.SelectedItems.FirstOrDefault();
			if (selectedItem != null)
			{
				bool isDecodeMode = selectedItem.Text == "解码";
				viewModel.IsDecodeMode = isDecodeMode;
			}
		}
	}

	private void OnDecodeModeChanged(object sender, SelectedItemChangedEventArgs e)
	{
		if (e.SelectedItem is Material.Components.Maui.MenuItem menuItem && BindingContext is ProtobufConverterViewModel viewModel)
		{
			viewModel.SelectedDecodeMode = menuItem.Text;
		}
	}
}