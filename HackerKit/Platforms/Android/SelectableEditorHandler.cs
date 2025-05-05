#if ANDROID
using Android.Text;
using AndroidX.AppCompat.Widget;
using Microsoft.Maui.Handlers;
using Android.Content;

namespace HackerKit.Platforms.Android
{
	public partial class SelectableEditorHandler : EditorHandler
	{
		protected override AppCompatEditText CreatePlatformView()
		{
			var context = MauiApplication.Current.ApplicationContext;
			return new NonEditableSelectableEditText(context);
		}

		protected override void ConnectHandler(AppCompatEditText nativeView)
		{
			base.ConnectHandler(nativeView);

			nativeView.SetTextIsSelectable(true);
			nativeView.LongClickable = true;
			nativeView.Clickable = true;
			nativeView.Focusable = true;
			nativeView.FocusableInTouchMode = true;
			nativeView.SetCursorVisible(false);
			nativeView.InputType = InputTypes.Null;
		}
	}
}
#endif