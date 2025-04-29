using System;
using System.Numerics;
using System.Text;
using Xamarin.Forms;
using HackerKit.Services;
using Xamarin.Essentials;

namespace HackerKit.Views
{
	public partial class BinaryCodeCalculator : ContentPage
	{
		public BinaryCodeCalculator()
		{
			InitializeComponent();

			int[] allowedBits = new int[] { 8, 16, 32, 64, 128, 256, 512, 1024, 2048, 4096 };
			foreach (var bits in allowedBits)
			{
				BitPicker.Items.Add(bits.ToString());
			}
			BitPicker.SelectedIndex = 0;
		}


		private async void OnCalculateClicked(object sender, EventArgs e)
		{
			var inputText = InputEntry.Text?.Trim();
			if (string.IsNullOrEmpty(inputText))
			{
				await ToastService.ShowToast("请输入一个整数");
				return;
			}

			if (BitPicker.SelectedIndex < 0)
			{
				await ToastService.ShowToast("请选择位数");
				return;
			}

			int bitCount = int.Parse(BitPicker.Items[BitPicker.SelectedIndex]);

			//尝试解析输入整数，支持大整数
			if (!BigInteger.TryParse(inputText, out BigInteger number))
			{
				await ToastService.ShowToast("输入的不是有效整数");
				return;
			}

			//判断输入数是否能用选定位数表示
			BigInteger minValue = -BigInteger.Pow(2, bitCount - 1);
			BigInteger maxValue = BigInteger.Pow(2, bitCount - 1) - 1;

			if (number < minValue || number > maxValue)
			{
				await ToastService.ShowToast($"输入数超出{bitCount}位有符号整数范围：[{minValue} ~ {maxValue}]");
				return;
			}

			try
			{
				//计算原码、反码、补码
				string originalCode = GetOriginalCode(number, bitCount);
				string inverseCode = GetInverseCode(number, bitCount);
				string complementCode = GetComplementCode(number, bitCount);

				OriginalCodeLabel.Text = FormatBinaryString(originalCode);
				InverseCodeLabel.Text = FormatBinaryString(inverseCode);
				ComplementCodeLabel.Text = FormatBinaryString(complementCode);
			}
			catch (Exception)
			{
				await ToastService.ShowToast("计算出错，请重试");
			}
		}

		/// <summary>
		/// 获取原码
		/// </summary>
		private string GetOriginalCode(BigInteger number, int bitCount)
		{
			bool isNegative = number < 0;
			BigInteger absValue = BigInteger.Abs(number);

			string magnitudeBinary = ConvertBigIntegerToBinary(absValue, bitCount - 1);

			return (isNegative ? "1" : "0") + magnitudeBinary;
		}

		/// <summary>
		/// 获取反码
		/// </summary>
		private string GetInverseCode(BigInteger number, int bitCount)
		{
			if (number >= 0)
			{
				return GetOriginalCode(number, bitCount);
			}
			else
			{
				string original = GetOriginalCode(number, bitCount);
				char signBit = original[0];
				string magnitude = original.Substring(1);

				var invertedMagnitude = new StringBuilder();
				foreach (var c in magnitude)
				{
					invertedMagnitude.Append(c == '0' ? '1' : '0');
				}

				return signBit + invertedMagnitude.ToString();
			}
		}

		/// <summary>
		/// 获取补码
		/// 补码：正数同原码，负数反码+1
		/// </summary>
		private string GetComplementCode(BigInteger number, int bitCount)
		{
			if (number >= 0)
			{
				return GetOriginalCode(number, bitCount);
			}
			else
			{
				string inverse = GetInverseCode(number, bitCount);

				BigInteger inverseValue = BigInteger.Parse(inverse, System.Globalization.NumberStyles.AllowLeadingWhite | System.Globalization.NumberStyles.AllowTrailingWhite, null);

				BigInteger inverseBigInt = BinaryStringToBigInteger(inverse);

				BigInteger complementBigInt = inverseBigInt + 1;

				string complementBinary = ConvertBigIntegerToBinary(complementBigInt, bitCount);

				return complementBinary;
			}
		}

		/// <summary>
		/// 将BigInteger转换为指定长度的二进制字符串，左侧补0
		/// </summary>
		private string ConvertBigIntegerToBinary(BigInteger value, int length)
		{
			if (value < 0)
				throw new ArgumentException("value必须为非负数");

			var sb = new StringBuilder();

			BigInteger v = value;
			while (v > 0)
			{
				sb.Insert(0, (v & 1) == 1 ? '1' : '0');
				v >>= 1;
			}

			while (sb.Length < length)
			{
				sb.Insert(0, '0');
			}

			if (sb.Length > length)
			{
				sb.Remove(0, sb.Length - length);
			}

			return sb.ToString();
		}

		/// <summary>
		/// 将二进制字符串转换为BigInteger
		/// </summary>
		private BigInteger BinaryStringToBigInteger(string binary)
		{
			BigInteger result = BigInteger.Zero;
			foreach (char c in binary)
			{
				result <<= 1;
				if (c == '1')
					result += 1;
				else if (c != '0')
					throw new ArgumentException("二进制字符串包含非法字符");
			}
			return result;
		}

		/// <summary>
		/// 格式化二进制字符串，每4位空格分隔，方便阅读
		/// </summary>
		private string FormatBinaryString(string binary)
		{
			var sb = new StringBuilder();
			for (int i = 0; i < binary.Length; i++)
			{
				sb.Append(binary[i]);
				if ((i + 1) % 4 == 0 && i != binary.Length - 1)
					sb.Append(' ');
			}
			return sb.ToString();
		}

		private async void OnCopyOriginalClicked(object sender, EventArgs e)
		{
			var text = OriginalCodeLabel.Text ?? "";
			if (!string.IsNullOrWhiteSpace(text))
			{
				await Clipboard.SetTextAsync(text);
				await ToastService.ShowToast("原码已复制");
			}
			else
			{
				await ToastService.ShowToast("没有原码可复制");
			}
		}

		private async void OnCopyInverseClicked(object sender, EventArgs e)
		{
			var text = InverseCodeLabel.Text ?? "";
			if (!string.IsNullOrWhiteSpace(text))
			{
				await Clipboard.SetTextAsync(text);
				await ToastService.ShowToast("反码已复制");
			}
			else
			{
				await ToastService.ShowToast("没有反码可复制");
			}
		}

		private async void OnCopyComplementClicked(object sender, EventArgs e)
		{
			var text = ComplementCodeLabel.Text ?? "";
			if (!string.IsNullOrWhiteSpace(text))
			{
				await Clipboard.SetTextAsync(text);
				await ToastService.ShowToast("补码已复制");
			}
			else
			{
				await ToastService.ShowToast("没有补码可复制");
			}
		}

		private async void OnPasteClicked(object sender, EventArgs e)
		{
			try
			{
				var text = await Clipboard.GetTextAsync();
				if (!string.IsNullOrEmpty(text))
				{
					InputEntry.Text = text;
					await ToastService.ShowToast("内容已粘贴");
				}
				else
				{
					await ToastService.ShowToast("剪贴板为空");
				}
			}
			catch (Exception)
			{
				await ToastService.ShowToast("访问剪贴板失败");
			}
		}

		private void OnClearClicked(object sender, EventArgs e)
		{
			InputEntry.Text = string.Empty;
			OriginalCodeLabel.Text = string.Empty;
			InverseCodeLabel.Text = string.Empty;
			ComplementCodeLabel.Text = string.Empty;
			BitPicker.SelectedIndex = 0;
		}
	}
}
