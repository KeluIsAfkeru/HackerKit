using System;
using System.Net;
using Xamarin.Forms;
using Xamarin.Essentials;
using HackerKit.Services; // 引入ToastService命名空间

namespace HackerKit.Views
{
	public partial class UrlConverter : ContentPage
	{
		public UrlConverter()
		{
			InitializeComponent();
		}

		private async void OnEncodeClicked(object sender, EventArgs e)
		{
			var input = InputEditor.Text?.Trim() ?? "";
			if (string.IsNullOrEmpty(input))
			{
				await ToastService.ShowToast("请输入点内容，才能帮你编码哦~");
				return;
			}

			try
			{
				var encoded = WebUtility.UrlEncode(input);
				ResultEditor.Text = encoded;
				await ToastService.ShowToast("编码成功");
			}
			catch (Exception)
			{
				await ToastService.ShowToast("编码出错啦，请检查输入内容");
			}
		}

		private async void OnDecodeClicked(object sender, EventArgs e)
		{
			var input = InputEditor.Text?.Trim() ?? "";
			if (string.IsNullOrEmpty(input))
			{
				await ToastService.ShowToast("请先输入URL编码内容哦~");
				return;
			}

			try
			{
				var decoded = WebUtility.UrlDecode(input);
				ResultEditor.Text = decoded;
				await ToastService.ShowToast("解码成功，结果已更新！");
			}
			catch (Exception)
			{
				await ToastService.ShowToast("解码失败，请确认内容格式。");
			}
		}

		private void OnClearClicked(object sender, EventArgs e)
		{
			InputEditor.Text = string.Empty;
			ResultEditor.Text = string.Empty;
		}

		private async void OnPasteClicked(object sender, EventArgs e)
		{
			try
			{
				var text = await Clipboard.GetTextAsync();
				if (!string.IsNullOrEmpty(text))
				{
					InputEditor.Text = text;
					await ToastService.ShowToast("内容已粘贴");
				}
				else
				{
					await ToastService.ShowToast("剪贴板里空空如也呢~");
				}
			}
			catch (Exception)
			{
				await ToastService.ShowToast("访问剪贴板失败，请稍后再试。");
			}
		}

		private async void OnCopyClicked(object sender, EventArgs e)
		{
			var text = ResultEditor.Text ?? "";
			if (!string.IsNullOrWhiteSpace(text))
			{
				await Clipboard.SetTextAsync(text);
				await ToastService.ShowToast("结果已复制，快去粘贴吧！");
			}
			else
			{
				await ToastService.ShowToast("没有内容可以复制哦~");
			}
		}
	}
}
