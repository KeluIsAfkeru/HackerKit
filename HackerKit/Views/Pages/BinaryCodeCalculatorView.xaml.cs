using HackerKit.ViewModels;

namespace HackerKit.Views.Pages;

public partial class BinaryCodeCalculatorView : ContentView
{
	public BinaryCodeCalculatorView()
	{
		InitializeComponent();
		var services = IPlatformApplication.Current.Services;
		BindingContext = services.GetService<BinaryCodeCalculatorViewModel>();

		//����Ĭ��ѡ����Ϊ8λ
		if (BitSizeComboBox.Items.Count > 0)
		{
			BitSizeComboBox.SelectedIndex = 0;
		}
	}

	private void OnBitSizeChanged(object sender, SelectedItemChangedEventArgs e)
	{
		if (e.SelectedItem is Material.Components.Maui.MenuItem menuItem && BindingContext is BinaryCodeCalculatorViewModel viewModel)
		{
			string bitText = menuItem.Text;
			int bitSize = int.Parse(bitText.Replace("λ", ""));
			viewModel.SelectedBitSize = bitSize;
		}
	}
}
