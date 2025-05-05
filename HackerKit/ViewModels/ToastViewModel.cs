using CommunityToolkit.Mvvm.ComponentModel;
using HackerKit.Models;
using Microsoft.Maui.Graphics.Text;

namespace HackerKit.ViewModels
{
	public partial class ToastViewModel : ObservableObject
	{
		[ObservableProperty]
		private string message;

		[ObservableProperty]
		private object iconData;

		[ObservableProperty]
		private Color iconColor;

		[ObservableProperty]
		private Color textColor;

		[ObservableProperty]
		private Color backgroundColor;

		[ObservableProperty]
		private double verticalOffset;

		[ObservableProperty]
		private double opacity = 1;

		public ToastViewModel(ToastModel toast, double verticalOffset = 0)
		{
			Message = toast.Message;
			VerticalOffset = verticalOffset;

			switch (toast.Type)
			{
				case ToastType.Success:
					IconData = IconPacks.IconKind.Material.CheckCircle;
					IconColor = Colors.White;
					TextColor = Colors.White;
					BackgroundColor = Color.FromArgb("#B3D8F8");
					break;
				case ToastType.Warning:
					IconData = IconPacks.IconKind.Material.Warning;
					IconColor = Colors.White;
					TextColor = Colors.White;
					BackgroundColor = Color.FromArgb("#FF9800");
					break;
				case ToastType.Error:
					IconData = IconPacks.IconKind.Material.Error;
					IconColor = Colors.White;
					TextColor = Colors.White;
					BackgroundColor = Color.FromArgb("#F44336");
					break;
				case ToastType.Info:
				default:
					IconData = IconPacks.IconKind.Material.Info;
					IconColor = Colors.White;
					TextColor = Colors.White;
					BackgroundColor = Color.FromArgb("#B3D8F8");
					break;
			}
		}
	}
}