using HackerKit.ViewModels;

namespace HackerKit.Views.Pages;

public partial class TextEncodingConverterView : ContentView
{
	public TextEncodingConverterView()
	{
		InitializeComponent();
		var services = IPlatformApplication.Current.Services;
		BindingContext = services.GetService<TextEncodingConverterViewModel>();

		//设置默认选中项为Base64
		if (EncodingTypeComboBox.Items.Count > 0)
		{
			EncodingTypeComboBox.SelectedIndex = 0;
		}
	}

	private void OnEncodingTypeChanged(object sender, SelectedItemChangedEventArgs e)
	{
		if (e.SelectedItem is Material.Components.Maui.MenuItem menuItem && BindingContext is TextEncodingConverterViewModel viewModel)
		{
			viewModel.SelectedEncodingType = menuItem.Text;
		}
	}
}
