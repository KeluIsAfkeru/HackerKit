using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Xamarin.Forms;
using Xamarin.Essentials;
using HackerKit.Services;

namespace HackerKit.Views
{
	public partial class BaseConverter : ContentPage
	{
		public BaseConverter()
		{
			InitializeComponent();

			SourceFormatPicker.SelectedIndex = 0;
			TargetFormatPicker.SelectedIndex = 1;
			SourceSeparatorPicker.SelectedIndex = 0; 
			TargetSeparatorPicker.SelectedIndex = 0;
		}

		private void SourceSeparatorPicker_SelectedIndexChanged(object sender, EventArgs e)
		{
			SourceSeparatorEntry.IsVisible = SourceSeparatorPicker.SelectedItem?.ToString() == "自定义";
		}

		private void TargetSeparatorPicker_SelectedIndexChanged(object sender, EventArgs e)
		{
			TargetSeparatorEntry.IsVisible = TargetSeparatorPicker.SelectedItem?.ToString() == "自定义";
		}

		private async void OnConvertClicked(object sender, EventArgs e)
		{
			var input = InputEditor.Text?.Trim() ?? "";
			if (string.IsNullOrEmpty(input))
			{
				await ToastService.ShowToast("请输入要转换的内容");
				return;
			}

			var sourceFormat = SourceFormatPicker.SelectedItem?.ToString();
			var targetFormat = TargetFormatPicker.SelectedItem?.ToString();
			if (string.IsNullOrEmpty(sourceFormat) || string.IsNullOrEmpty(targetFormat))
			{
				await ToastService.ShowToast("请选择源格式和目标格式");
				return;
			}

			string sourceSep = GetSeparator(SourceSeparatorPicker, SourceSeparatorEntry);
			string targetSep = GetSeparator(TargetSeparatorPicker, TargetSeparatorEntry);

			try
			{
				byte[] bytes;

				bytes = ParseInputToBytes(input, sourceFormat, sourceSep);

				string result = ConvertBytesToTarget(bytes, targetFormat, targetSep);

				ResultEditor.Text = result;
				await ToastService.ShowToast("转换成功");
			}
			catch (Exception ex)
			{
				await ToastService.ShowToast($"转换失败：{ex.Message}");
			}
		}

		private string GetSeparator(Picker picker, Entry customEntry)
		{
			var selected = picker.SelectedItem?.ToString();
			if (selected == null) return "";

			return selected switch
			{
				"无" => "",
				"逗号 (,)" => ",",
				"空格 ( )" => " ",
				"分号 (;)" => ";",
				"自定义" => customEntry.Text ?? "",
				_ => ""
			};
		}

		private byte[] ParseInputToBytes(string input, string sourceFormat, string sourceSep)
		{
			if (sourceFormat == "Base64")
			{
				try
				{
					return Convert.FromBase64String(input);
				}
				catch
				{
					throw new Exception("Base64 格式无效，请检查输入");
				}
			}

			// 根据分隔符拆分输入
			string[] parts = string.IsNullOrEmpty(sourceSep)
				? new string[] { input }
				: input.Split(new string[] { sourceSep }, StringSplitOptions.RemoveEmptyEntries);

			List<byte> bytes = new List<byte>();

			foreach (var part in parts)
			{
				string trimmed = part.Trim();

				try
				{
					if (sourceFormat == "十进制")
					{
						if (!int.TryParse(trimmed, NumberStyles.Integer, CultureInfo.InvariantCulture, out int val) || val < 0 || val > 255)
						{
							throw new Exception($"无法解析十进制数字：{trimmed}");
						}
						bytes.Add((byte)val);
					}
					else if (sourceFormat == "十六进制")
					{
						string hex = trimmed.StartsWith("0x", StringComparison.OrdinalIgnoreCase) ? trimmed.Substring(2) : trimmed;
						if (!byte.TryParse(hex, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out byte b))
						{
							throw new Exception($"无法解析十六进制数字：{trimmed}");
						}
						bytes.Add(b);
					}
					else if (sourceFormat == "二进制")
					{
						if (trimmed.Length > 8 || trimmed.Any(c => c != '0' && c != '1'))
						{
							throw new Exception($"无法解析二进制数字：{trimmed}");
						}
						byte b = Convert.ToByte(trimmed, 2);
						bytes.Add(b);
					}
					else
					{
						throw new Exception("不支持的源格式");
					}
				}
				catch (Exception ex)
				{
					throw new Exception($"解析失败：{ex.Message}");
				}
			}

			return bytes.ToArray();
		}

		private string ConvertBytesToTarget(byte[] bytes, string targetFormat, string targetSep)
		{
			if (targetFormat == "Base64")
			{
				return Convert.ToBase64String(bytes);
			}

			IEnumerable<string> parts = targetFormat switch
			{
				"十进制" => bytes.Select(b => b.ToString(CultureInfo.InvariantCulture)),
				"十六进制" => bytes.Select(b => b.ToString("X2")), 
				"二进制" => bytes.Select(b => Convert.ToString(b, 2).PadLeft(8, '0')), 
				_ => throw new Exception("不支持的目标格式"),
			};

			return string.Join(targetSep, parts);
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
