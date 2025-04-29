using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;
using Xamarin.Essentials;
using HackerKit.Services;
using static Xamarin.Essentials.Permissions;

namespace HackerKit.Views
{
	public partial class PbConverter : ContentPage
	{
		public PbConverter()
		{
			InitializeComponent();
		}

		//Hex转byte array
		private static byte[] HexStringToBytes(string hex)
		{
			hex = hex.Replace(" ", "").Replace("\r", "").Replace("\n", "");
			if (hex.Length % 2 != 0)
				throw new FormatException("Hex字符串长度必须为偶数");
			var bytes = new byte[hex.Length / 2];
			for (int i = 0; i < bytes.Length; i++)
				bytes[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
			return bytes;
		}

		private async void OnEncodeClicked(object sender, EventArgs e)
		{
			var input = InputEditor.Text?.Trim() ?? "";
			if (string.IsNullOrEmpty(input))
			{
				await ToastService.ShowToast("请输入Protobuf字符串（JSON格式或Hex）");
				return;
			}

			try
			{
				//var proto = ProtobufService.StrToProto(input); //将输入protobuf字符串反序列化成Proto
				//var hexResult = proto.ToHex(); //转为Hex
				var proto = ProtobufService.FromJson(input);
				var hex = proto.ToHex();
				ResultEditor.Text = hex;
				await ToastService.ShowToast("编码成功，结果为Hex字符串");
			}
			catch (Exception ex)
			{
				await ToastService.ShowToast($"编码失败：{ex.Message}");
			}
		}

		private async void OnDecodeClicked(object sender, EventArgs e)
		{
			var input = InputEditor.Text?.Trim() ?? "";
			if (string.IsNullOrEmpty(input))
			{
				await ToastService.ShowToast("请输入Hex格式的Protobuf二进制字符串");
				return;
			}

			try
			{
				var hex = input.ParseHexWithSpaces();
				var proto = ProtobufService.Decode(hex);
				var json = proto.ToJson();

				ResultEditor.Text = json;
				ProtobufService.PrintProto(proto);
				await ToastService.ShowToast("解码成功，结果为JSON字符串");
			}
			catch (FormatException ex)
			{
				await ToastService.ShowToast($"格式错误：{ex.Message}");
			}
			catch (Exception ex)
			{
				await ToastService.ShowToast($"解码失败：{ex.Message}");
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
