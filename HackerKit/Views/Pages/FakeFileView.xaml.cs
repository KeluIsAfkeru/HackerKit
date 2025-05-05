using HackerKit.ViewModels;

namespace HackerKit.Views.Pages;

public partial class FakeFileView : ContentView
{
	public FakeFileView()
	{
		InitializeComponent();
		var services = IPlatformApplication.Current.Services;
		BindingContext = services.GetService<FakeFileViewModel>();
	}
}