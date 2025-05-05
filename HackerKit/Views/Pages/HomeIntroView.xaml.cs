namespace HackerKit.Views.Pages;

public partial class HomeIntroView : ContentView
{
	public HomeIntroView()
	{
		InitializeComponent();
		AnimateCards();
	}

	private async void AnimateCards()
	{
		// 入场动画（依次淡入+缩放）
		await Task.Delay(200); // 稍等页面布局
		await Card1.FadeTo(1, 400, Easing.CubicOut);
		await Card1.ScaleTo(1, 400, Easing.SpringOut);

		await Task.Delay(80);
		await Card2.FadeTo(1, 350, Easing.CubicOut);
		await Card2.ScaleTo(1, 350, Easing.SpringOut);

		await Task.Delay(80);
		await Card3.FadeTo(1, 350, Easing.CubicOut);
		await Card3.ScaleTo(1, 350, Easing.SpringOut);

		await Task.Delay(80);
		await Card4.FadeTo(1, 350, Easing.CubicOut);
		await Card4.ScaleTo(1, 350, Easing.SpringOut);
	}
}