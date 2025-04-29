using Android.App;
using Android.Content;
using Android.Views;
using HackerKit.Droid.Assets;
using HackerKit.Models;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(ScrollableEditor), typeof(ScrollableEditorRenderer))]
namespace HackerKit.Droid.Assets
{
	public class ScrollableEditorRenderer : EditorRenderer
	{
		public ScrollableEditorRenderer(Context context) : base(context)
		{
		}

		public override bool DispatchTouchEvent(MotionEvent e)
		{
			Parent?.RequestDisallowInterceptTouchEvent(true);
			return base.DispatchTouchEvent(e);
		}
	}
}