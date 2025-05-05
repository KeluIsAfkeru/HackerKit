using Android.Content;
using Android.Util;
using Android.Views;
using AndroidX.AppCompat.Widget;

namespace HackerKit.Platforms.Android
{
	public class NonEditableSelectableEditText : AppCompatEditText
	{
		public NonEditableSelectableEditText(Context context) : base(context) { }
		public NonEditableSelectableEditText(Context context, IAttributeSet attrs) : base(context, attrs) { }
		public NonEditableSelectableEditText(Context context, IAttributeSet attrs, int defStyleAttr) : base(context, attrs, defStyleAttr) { }

		public override bool OnCheckIsTextEditor()
		{
			//不是文本编辑器，不弹出输入法
			return false;
		}

		public override bool OnTextContextMenuItem(int id)
		{
			if (id == global::Android.Resource.Id.Copy)
				return base.OnTextContextMenuItem(id); //允许复制
			return false; //禁止剪切和粘贴
		}

		public override bool OnTouchEvent(MotionEvent e)
		{
			//允许选择
			base.OnTouchEvent(e);
			//不弹键盘
			return true;
		}
	}
}