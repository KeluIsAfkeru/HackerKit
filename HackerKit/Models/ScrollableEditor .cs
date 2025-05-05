using Microsoft.Maui.Controls;

#if ANDROID
using Android.Views;
using Microsoft.Maui.Handlers;
#endif

namespace HackerKit.Models
{
	public partial class ScrollableEditor : Editor
	{
#if ANDROID
        protected override void OnHandlerChanged()
        {
            base.OnHandlerChanged();

            if (Handler?.PlatformView is Android.Widget.EditText nativeEdit)
            {
                // 先移除，防止多次注册
                nativeEdit.Touch -= NativeEdit_Touch;
                nativeEdit.Touch += NativeEdit_Touch;
            }
        }

        private void NativeEdit_Touch(object sender, Android.Views.View.TouchEventArgs e)
        {
            var edit = sender as Android.Widget.EditText;
            if (edit == null) return;

            //只要内容超出就允许滚动
            bool canScrollVertically = edit.Layout != null && (edit.Layout.Height > edit.Height + edit.PaddingTop + edit.PaddingBottom);

            if (canScrollVertically)
            {
                switch (e.Event.Action)
                {
                    case MotionEventActions.Down:
                        edit.Parent?.RequestDisallowInterceptTouchEvent(true);
                        break;
                    case MotionEventActions.Up:
                    case MotionEventActions.Cancel:
                        edit.Parent?.RequestDisallowInterceptTouchEvent(false);
                        break;
                }
            }
        }
#endif
	}
}