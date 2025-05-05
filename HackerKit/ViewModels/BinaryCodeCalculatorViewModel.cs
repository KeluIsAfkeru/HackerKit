using System.ComponentModel;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Input;
using HackerKit.Models;
using HackerKit.Services.Interfaces;

namespace HackerKit.ViewModels;

public class BinaryCodeCalculatorViewModel : INotifyPropertyChanged
{
	private readonly IClipboardService _clipboardService;
	private readonly IToastService _toastService;

	public BinaryCodeCalculatorViewModel(IClipboardService clipboardService, IToastService toastService)
	{
		_clipboardService = clipboardService;
		_toastService = toastService;

		// 默认选择8位
		_selectedBitSize = 8;

		CalculateCommand = new Command(Calculate);
		ClearCommand = new Command(Clear);
		PasteCommand = new Command(async () => await Paste());
		CopyOriginalCommand = new Command(async () => await CopyOriginal());
		CopyInverseCommand = new Command(async () => await CopyInverse());
		CopyComplementCommand = new Command(async () => await CopyComplement());
	}

	private string _inputNumber;
	public string InputNumber
	{
		get => _inputNumber;
		set { _inputNumber = value; OnPropertyChanged(); }
	}

	private int _selectedBitSize;
	public int SelectedBitSize
	{
		get => _selectedBitSize;
		set { _selectedBitSize = value; OnPropertyChanged(); }
	}

	private string _originalCode;
	public string OriginalCode
	{
		get => _originalCode;
		set { _originalCode = value; OnPropertyChanged(); }
	}

	private string _inverseCode;
	public string InverseCode
	{
		get => _inverseCode;
		set { _inverseCode = value; OnPropertyChanged(); }
	}

	private string _complementCode;
	public string ComplementCode
	{
		get => _complementCode;
		set { _complementCode = value; OnPropertyChanged(); }
	}

	public ICommand CalculateCommand { get; }
	public ICommand ClearCommand { get; }
	public ICommand PasteCommand { get; }
	public ICommand CopyOriginalCommand { get; }
	public ICommand CopyInverseCommand { get; }
	public ICommand CopyComplementCommand { get; }

	private void Calculate()
	{
		if (string.IsNullOrWhiteSpace(InputNumber))
		{
			_toastService?.ShowToastAsync("请输入一个整数", ToastType.Warning, 2000);
			return;
		}

		try
		{
			//尝试解析输入整数，支持大整数
			if (!BigInteger.TryParse(InputNumber, out BigInteger number))
			{
				_toastService?.ShowToastAsync("输入的不是有效整数", ToastType.Warning, 2000);
				return;
			}

			//判断输入数是否能用选定位数表示
			BigInteger minValue = -BigInteger.Pow(2, SelectedBitSize - 1);
			BigInteger maxValue = BigInteger.Pow(2, SelectedBitSize - 1) - 1;

			if (number < minValue || number > maxValue)
			{
				_toastService?.ShowToastAsync($"输入数超出{SelectedBitSize}位有符号整数范围：[{minValue} ~ {maxValue}]", ToastType.Warning, 2000);
				return;
			}

			//计算原码、反码、补码
			OriginalCode = FormatBinaryString(GetOriginalCode(number, SelectedBitSize));
			InverseCode = FormatBinaryString(GetInverseCode(number, SelectedBitSize));
			ComplementCode = FormatBinaryString(GetComplementCode(number, SelectedBitSize));

			_toastService?.ShowToastAsync("计算完成", ToastType.Success, 1500);
		}
		catch (Exception ex)
		{
			_toastService?.ShowToastAsync($"计算出错: {ex.Message}", ToastType.Error, 2000);
		}
	}

	private void Clear()
	{
		InputNumber = string.Empty;
		OriginalCode = string.Empty;
		InverseCode = string.Empty;
		ComplementCode = string.Empty;
	}

	private async Task Paste()
	{
		var text = await _clipboardService.GetTextAsync();
		if (!string.IsNullOrEmpty(text))
		{
			InputNumber = text;
			_toastService?.ShowToastAsync("已粘贴", ToastType.Success, 1500);
		}
		else
		{
			_toastService?.ShowToastAsync("剪贴板为空", ToastType.Info, 1500);
		}
	}

	private async Task CopyOriginal()
	{
		if (!string.IsNullOrEmpty(OriginalCode))
		{
			await _clipboardService.SetTextAsync(OriginalCode);
			_toastService?.ShowToastAsync("原码已复制到剪贴板", ToastType.Success, 1500);
		}
		else
		{
			_toastService?.ShowToastAsync("没有可复制的原码", ToastType.Warning, 1500);
		}
	}

	private async Task CopyInverse()
	{
		if (!string.IsNullOrEmpty(InverseCode))
		{
			await _clipboardService.SetTextAsync(InverseCode);
			_toastService?.ShowToastAsync("反码已复制到剪贴板", ToastType.Success, 1500);
		}
		else
		{
			_toastService?.ShowToastAsync("没有可复制的反码", ToastType.Warning, 1500);
		}
	}

	private async Task CopyComplement()
	{
		if (!string.IsNullOrEmpty(ComplementCode))
		{
			await _clipboardService.SetTextAsync(ComplementCode);
			_toastService?.ShowToastAsync("补码已复制到剪贴板", ToastType.Success, 1500);
		}
		else
		{
			_toastService?.ShowToastAsync("没有可复制的补码", ToastType.Warning, 1500);
		}
	}

	/// <summary>
	///获取原码
	/// </summary>
	private string GetOriginalCode(BigInteger number, int bitCount)
	{
		bool isNegative = number < 0;
		BigInteger absValue = BigInteger.Abs(number);

		string magnitudeBinary = ConvertBigIntegerToBinary(absValue, bitCount - 1);

		return (isNegative ? "1" : "0") + magnitudeBinary;
	}

	/// <summary>
	///获取反码
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
	/// </summary>
	private string GetComplementCode(BigInteger number, int bitCount)
	{
		if (number >= 0)
		{
			return GetOriginalCode(number, bitCount);
		}
		else if (number == -BigInteger.Pow(2, bitCount - 1))  //特殊情况：最小值
		{
			StringBuilder sb = new StringBuilder("1");
			for (int i = 0; i < bitCount - 1; i++)
				sb.Append('0');
			return sb.ToString();
		}
		else
		{
			string inverse = GetInverseCode(number, bitCount);

			//转换为二进制数进行加1操作
			bool carry = true;
			char[] complementArray = inverse.ToCharArray();

			for (int i = complementArray.Length - 1; i >= 0; i--)
			{
				if (complementArray[i] == '0' && carry)
				{
					complementArray[i] = '1';
					carry = false;
				}
				else if (complementArray[i] == '1' && carry)
				{
					complementArray[i] = '0';
				}
			}

			return new string(complementArray);
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

		if (value == 0)
		{
			sb.Append('0');
		}
		else
		{
			BigInteger v = value;
			while (v > 0)
			{
				sb.Insert(0, (v & 1) == 1 ? '1' : '0');
				v >>= 1;
			}
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

	public event PropertyChangedEventHandler PropertyChanged;
	protected void OnPropertyChanged([CallerMemberName] string name = null)
		=> PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
