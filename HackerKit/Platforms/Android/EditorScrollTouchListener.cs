using Android.Views;
using AndroidX.AppCompat.Widget;
using JavaObject = Java.Lang.Object;
using AndroidView = Android.Views.View;

namespace HackerKit.Platforms.Android
{
	public class EditorScrollTouchListener : JavaObject, AndroidView.IOnTouchListener
	{
		public bool OnTouch(AndroidView? v, MotionEvent? e)
		{
			if (v != null && v is AppCompatEditText editText)
			{
				if (IsTextScrollable(editText))
				{
					editText.Parent?.RequestDisallowInterceptTouchEvent(true);

					if (e?.Action == MotionEventActions.Up || e?.Action == MotionEventActions.Cancel)
					{
						editText.Parent?.RequestDisallowInterceptTouchEvent(false);
					}
				}
			}
			return false;
		}

		private bool IsTextScrollable(AppCompatEditText editText)
		{
			int textHeight = editText.Layout?.Height ?? 0;
			int visibleHeight = editText.Height - editText.PaddingTop - editText.PaddingBottom;
			return textHeight > visibleHeight;
		}
	}
}
