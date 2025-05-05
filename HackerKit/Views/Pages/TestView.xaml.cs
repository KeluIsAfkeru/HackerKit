namespace HackerKit.Views.Pages;

public partial class TestView : ContentView
{
	public TestView()
	{
		InitializeComponent();
	}

	private void OnShowClicked(object sender, EventArgs e)
	{
		outputLabel.Text = $"ƒ„ ‰»Î¡À: {testEntry.Text}";
	}
}