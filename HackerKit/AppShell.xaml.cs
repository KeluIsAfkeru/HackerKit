using Xamarin.Forms;
using Xamarin.CommunityToolkit.Effects;

namespace HackerKit
{
	public partial class AppShell : Shell
	{
		public AppShell()
		{
			InitializeComponent();

			var headerTemplate = new DataTemplate(() =>
			{
				var grid = new Grid
				{
					BackgroundColor = Color.FromHex("#f0f0f0"),
					Padding = new Thickness(0, 0, 0, 0),
					HorizontalOptions = LayoutOptions.Fill
				};

				var label = new Label
				{
					FontAttributes = FontAttributes.Bold,
					FontSize = 16,
					TextColor = Color.FromHex("#666666"),
					VerticalOptions = LayoutOptions.Center,
					HorizontalOptions = LayoutOptions.Center,  
					HorizontalTextAlignment = TextAlignment.Center
				};
				label.SetBinding(Label.TextProperty, "Text");

				grid.Children.Add(label);

				var separator = new BoxView
				{
					HeightRequest = 1,
					BackgroundColor = Color.FromHex("#cccccc"),
					HorizontalOptions = LayoutOptions.FillAndExpand,
					VerticalOptions = LayoutOptions.End
				};
				grid.Children.Add(separator);

				return grid;
			});

			var itemTemplate = new DataTemplate(() =>
			{
				var frame = new Frame
				{
					Style = (Style)this.Resources["MenuItemFrameStyle"],
					HasShadow = true,
					Padding = 0,
					Margin = new Thickness(8, 4),
					CornerRadius = 55,
					HeightRequest = 30,
					IsClippedToBounds = true,
				};

				//添加阴影
				ShadowEffect.SetColor(frame, Color.FromHex("#55000000"));
				ShadowEffect.SetOpacity(frame, 0.3f);
				ShadowEffect.SetRadius(frame, 10);
				ShadowEffect.SetOffsetX(frame, 0);
				ShadowEffect.SetOffsetY(frame, 4);

				var label = new Label
				{
					FontSize = 16,
					VerticalOptions = LayoutOptions.Center,
					HorizontalOptions = LayoutOptions.Center,
					HorizontalTextAlignment = TextAlignment.Center,
					TextColor = Color.FromHex("#333333"),
				};
				label.SetBinding(Label.TextProperty, "Title");

				frame.Content = label;

				VisualStateManager.SetVisualStateGroups(frame, new VisualStateGroupList
	{
		new VisualStateGroup
		{
			Name = "CommonStates",
			States =
			{
				new VisualState
				{
					Name = "Normal",
					Setters =
					{
						new Setter
						{
							Property = VisualElement.BackgroundColorProperty,
							Value = Color.FromHex("#E3F2FD")
                        }
					}
				},
				new VisualState
				{
					Name = "Selected",
					Setters =
					{
						new Setter
						{
							Property = VisualElement.BackgroundColorProperty,
							Value = Color.FromHex("#1976D2")
                        }
					}
				}
			}
		}
	});


				return frame;
			});

			var selector = new FlyoutItemTemplateSelector
			{
				HeaderTemplate = headerTemplate,
				ItemTemplate = itemTemplate
			};

			this.ItemTemplate = selector;
		}

		private VisualStateGroupList GetVisualStateGroups(Label label)
		{
			var commonStates = new VisualStateGroup { Name = "CommonStates" };

			var normalState = new VisualState { Name = "Normal" };
			var selectedState = new VisualState { Name = "Selected" };

			normalState.Setters.Add(new Setter
			{
				Property = VisualElement.BackgroundColorProperty,
				Value = (Color)Application.Current.Resources["InitialBackgroundColor"]
			});
			normalState.Setters.Add(new Setter
			{
				TargetName = label.StyleId,
				Property = Label.TextColorProperty,
				Value = (Color)Application.Current.Resources["InitialTextColor"]
			});

			selectedState.Setters.Add(new Setter
			{
				Property = VisualElement.BackgroundColorProperty,
				Value = (Color)Application.Current.Resources["SelectedBackgroundColor"]
			});
			selectedState.Setters.Add(new Setter
			{
				TargetName = label.StyleId,
				Property = Label.TextColorProperty,
				Value = (Color)Application.Current.Resources["SelectedTextColor"]
			});

			commonStates.States.Add(normalState);
			commonStates.States.Add(selectedState);

			return new VisualStateGroupList { commonStates };
		}
	}
}
