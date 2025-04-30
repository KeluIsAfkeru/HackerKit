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
			var fileName = FileNameEntry.Text?.Trim();
			var fileSizeStr = FileSizeEntry.Text?.Trim();

			var parameters = new System.Collections.Generic.Dictionary<string, object>();

			if (!string.IsNullOrEmpty(fileName))
				parameters["f4"] = fileName;

			if (ulong.TryParse(fileSizeStr, out ulong fileSize) && fileSize > 0)
				parameters["f3"] = fileSize.ToString();

			try
			{
				var result = FakeFileService.MakeFakeFileJson(parameters);

				string jsonResult = Newtonsoft.Json.JsonConvert.SerializeObject(result, Newtonsoft.Json.Formatting.Indented);
				ResultEditor.Text = jsonResult;

				await ToastService.ShowToast("生成FakeFile成功辣~");
			}
			catch (Exception ex)
			{
				await ToastService.ShowToast($"生成FakeFile失败惹~：{ex.Message}");
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