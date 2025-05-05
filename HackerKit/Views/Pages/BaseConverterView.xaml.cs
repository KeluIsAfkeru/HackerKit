using HackerKit.ViewModels;

namespace HackerKit.Views.Pages;

public partial class BaseConverterView : ContentView
{
	public BaseConverterView()
	{
		InitializeComponent();
		var services = IPlatformApplication.Current.Services;
		BindingContext = services.GetService<BaseConverterViewModel>();

		// 设置默认选中项
		if (SourceFormatComboBox.Items.Count > 0)
			SourceFormatComboBox.SelectedIndex = 0;

		if (TargetFormatComboBox.Items.Count > 0)
			TargetFormatComboBox.SelectedIndex = 1;

		if (SourceSeparatorComboBox.Items.Count > 0)
			SourceSeparatorComboBox.SelectedIndex = 0;

		if (TargetSeparatorComboBox.Items.Count > 0)
			TargetSeparatorComboBox.SelectedIndex = 0;
	}

	private void OnSourceFormatChanged(object sender, SelectedItemChangedEventArgs e)
	{
		if (e.SelectedItem is Material.Components.Maui.MenuItem menuItem && BindingContext is BaseConverterViewModel viewModel)
		{
			viewModel.SelectedSourceFormat = menuItem.Text;
		}
	}

	private void OnTargetFormatChanged(object sender, SelectedItemChangedEventArgs e)
	{
		if (e.SelectedItem is Material.Components.Maui.MenuItem menuItem && BindingContext is BaseConverterViewModel viewModel)
		{
			viewModel.SelectedTargetFormat = menuItem.Text;
		}
	}

	private void OnSourceSeparatorChanged(object sender, SelectedItemChangedEventArgs e)
	{
		if (e.SelectedItem is Material.Components.Maui.MenuItem menuItem && BindingContext is BaseConverterViewModel viewModel)
		{
			viewModel.SelectedSourceSeparator = menuItem.Text;
			viewModel.IsSourceSeparatorCustom = menuItem.Text == "自定义";
		}
	}

	private void OnTargetSeparatorChanged(object sender, SelectedItemChangedEventArgs e)
	{
		if (e.SelectedItem is Material.Components.Maui.MenuItem menuItem && BindingContext is BaseConverterViewModel viewModel)
		{
			viewModel.SelectedTargetSeparator = menuItem.Text;
			viewModel.IsTargetSeparatorCustom = menuItem.Text == "自定义";
		}
	}
}
