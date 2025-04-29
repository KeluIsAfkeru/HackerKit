using System;
using Xamarin.Forms;
using Xamarin.Essentials;
using HackerKit.Services;

namespace HackerKit.Views
{
	public partial class FakeFile : ContentPage
	{
		public FakeFile()
		{
			InitializeComponent();
		}

		private async void OnGenerateClicked(object sender, EventArgs e)
		{
			var fileName = FileNameEntry.Text?.Trim() ?? "";
			var fileSizeStr = FileSizeEntry.Text?.Trim() ?? "";

			if (string.IsNullOrEmpty(fileName))
			{
				await ToastService.ShowToast("请输入文件名");
				return;
			}

			if (!ulong.TryParse(fileSizeStr, out ulong fileSize) || fileSize == 0)
			{
				await ToastService.ShowToast("请输入有效的文件大小（正整数）");
				return;
			}

			try
			{
				var parameters = new System.Collections.Generic.Dictionary<string, object>
				{
					["f4"] = fileName,
					["f3"] = fileSize
				};

				var result = FakeFileService.MakeFakeFileJson(parameters);

				string jsonResult = Newtonsoft.Json.JsonConvert.SerializeObject(result, Newtonsoft.Json.Formatting.Indented);
				ResultEditor.Text = jsonResult;

				await ToastService.ShowToast("生成成功");
			}
			catch (Exception ex)
			{
				await ToastService.ShowToast($"生成失败：{ex.Message}");
			}
		}

		private void OnClearClicked(object sender, EventArgs e)
		{
			FileNameEntry.Text = string.Empty;
			FileSizeEntry.Text = string.Empty;
			ResultEditor.Text = string.Empty;
		}

		private async void OnPasteClicked(object sender, EventArgs e)
		{
			try
			{
				var text = await Clipboard.GetTextAsync();
				if (!string.IsNullOrEmpty(text))
				{
					FileNameEntry.Text = text;
					await ToastService.ShowToast("内容已粘贴到文件名");
				}
				else
				{
					await ToastService.ShowToast("剪贴板为空");
				}
			}
			catch
			{
				await ToastService.ShowToast("访问剪贴板失败");
			}
		}

		private async void OnCopyClicked(object sender, EventArgs e)
		{
			var text = ResultEditor.Text ?? "";
			if (!string.IsNullOrWhiteSpace(text))
			{
				await Clipboard.SetTextAsync(text);
				await ToastService.ShowToast("结果已复制");
			}
			else
			{
				await ToastService.ShowToast("没有内容可复制");
			}
		}
	}
}