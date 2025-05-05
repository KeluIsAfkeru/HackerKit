using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Input;
using HackerKit.Models;
using HackerKit.Services.Interfaces;

namespace HackerKit.ViewModels;

public class BaseConverterViewModel : INotifyPropertyChanged
{
	private readonly IClipboardService _clipboardService;
	private readonly IToastService _toastService;

	public BaseConverterViewModel(IClipboardService clipboardService, IToastService toastService)
	{
		_clipboardService = clipboardService;
		_toastService = toastService;

		// 默认选择
		_selectedSourceFormat = "十进制";
		_selectedTargetFormat = "十六进制";
		_selectedSourceSeparator = "无";
		_selectedTargetSeparator = "无";
		_isSourceSeparatorCustom = false;
		_isTargetSeparatorCustom = false;
		_customSourceSeparator = "";
		_customTargetSeparator = "";

		ConvertCommand = new Command(Convert);
		ClearCommand = new Command(Clear);
		PasteCommand = new Command(async () => await Paste());
		CopyCommand = new Command(async () => await Copy());
	}

	#region 属性

	private string _inputText;
	public string InputText
	{
		get => _inputText;
		set { _inputText = value; OnPropertyChanged(); }
	}

	private string _outputText;
	public string OutputText
	{
		get => _outputText;
		set { _outputText = value; OnPropertyChanged(); }
	}

	private string _selectedSourceFormat;
	public string SelectedSourceFormat
	{
		get => _selectedSourceFormat;
		set { _selectedSourceFormat = value; OnPropertyChanged(); }
	}

	private string _selectedTargetFormat;
	public string SelectedTargetFormat
	{
		get => _selectedTargetFormat;
		set { _selectedTargetFormat = value; OnPropertyChanged(); }
	}

	private string _selectedSourceSeparator;
	public string SelectedSourceSeparator
	{
		get => _selectedSourceSeparator;
		set { _selectedSourceSeparator = value; OnPropertyChanged(); }
	}

	private string _selectedTargetSeparator;
	public string SelectedTargetSeparator
	{
		get => _selectedTargetSeparator;
		set { _selectedTargetSeparator = value; OnPropertyChanged(); }
	}

	private bool _isSourceSeparatorCustom;
	public bool IsSourceSeparatorCustom
	{
		get => _isSourceSeparatorCustom;
		set { _isSourceSeparatorCustom = value; OnPropertyChanged(); }
	}

	private bool _isTargetSeparatorCustom;
	public bool IsTargetSeparatorCustom
	{
		get => _isTargetSeparatorCustom;
		set { _isTargetSeparatorCustom = value; OnPropertyChanged(); }
	}

	private string _customSourceSeparator;
	public string CustomSourceSeparator
	{
		get => _customSourceSeparator;
		set { _customSourceSeparator = value; OnPropertyChanged(); }
	}

	private string _customTargetSeparator;
	public string CustomTargetSeparator
	{
		get => _customTargetSeparator;
		set { _customTargetSeparator = value; OnPropertyChanged(); }
	}

	#endregion

	#region 命令

	public ICommand ConvertCommand { get; }
	public ICommand ClearCommand { get; }
	public ICommand PasteCommand { get; }
	public ICommand CopyCommand { get; }

	#endregion

	#region 方法

	private void Convert()
	{
		if (string.IsNullOrWhiteSpace(InputText))
		{
			_toastService?.ShowToastAsync("请输入要转换的内容", ToastType.Warning, 2000);
			return;
		}

		try
		{
			//获取实际分隔符
			string sourceSeparator = GetSeparator(SelectedSourceSeparator, CustomSourceSeparator);
			string targetSeparator = GetSeparator(SelectedTargetSeparator, CustomTargetSeparator);

			//拆分输入内容
			List<string> parts;
			if (string.IsNullOrEmpty(sourceSeparator))
			{
				parts = [InputText];
			}
			else
			{
				parts = InputText.Split(sourceSeparator).ToList();
			}

			// 转换每个部分
			List<byte[]> byteArrays = new List<byte[]>();

			foreach (var part in parts.Where(p => !string.IsNullOrWhiteSpace(p)))
			{
				byte[] bytes;

				switch (SelectedSourceFormat)
				{
					case "十进制":
						if (int.TryParse(part, out int decimalValue))
						{
							//处理负数
							if (decimalValue < 0)
							{
								bytes = BitConverter.GetBytes(decimalValue);
								//取最短的表示形式
								int lastNonZeroIndex = bytes.Length - 1;
								while (lastNonZeroIndex > 0 && bytes[lastNonZeroIndex] == (decimalValue < 0 ? (byte)0xFF : (byte)0))
									lastNonZeroIndex--;

								bytes = bytes.Take(lastNonZeroIndex + 1).ToArray();
							}
							else
								bytes = [(byte)decimalValue];
						}
						else
						{
							throw new FormatException($"无效的十进制数值: {part}");
						}
						break;
					case "十六进制":
						string hexValue = part;
						if (hexValue.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
							hexValue = part[2..];
						//确保长度为偶数
						if (hexValue.Length % 2 != 0)
						{
							hexValue = "0" + hexValue;
						}

						try
						{
							bytes = [.. Enumerable.Range(0, hexValue.Length / 2).Select(i => System.Convert.ToByte(hexValue.Substring(i * 2, 2), 16))];
						}
						catch
						{
							throw new FormatException($"无效的十六进制数值: {hexValue}");
						}
						break;
					case "二进制":
						try
						{
							//每8位一组转换为byte
							StringBuilder binaryStr = new StringBuilder(part.Replace(" ", ""));

							//确保位数是8的倍数
							while (binaryStr.Length % 8 != 0)
								binaryStr.Insert(0, '0');

							bytes = new byte[binaryStr.Length / 8];
							for (int i = 0; i < bytes.Length; i++)
								bytes[i] = System.Convert.ToByte(binaryStr.ToString().Substring(i * 8, 8), 2);
						}
						catch
						{
							throw new FormatException($"无效的二进制数值: {part}");
						}
						break;
					case "Base64":
						try
						{
							bytes = System.Convert.FromBase64String(part);
						}
						catch
						{
							throw new FormatException($"无效的Base64编码: {part}");
						}
						break;

					default:
						throw new NotSupportedException($"不支持的源格式: {SelectedSourceFormat}");
				}

				byteArrays.Add(bytes);
			}

			// 将字节数组转换为目标格式
			List<string> results = new List<string>();

			foreach (var bytes in byteArrays)
			{
				string result;

				switch (SelectedTargetFormat)
				{
					case "十进制":
						if (bytes.Length <= 4)
						{
							// 处理整数范围内的值
							int value = 0;
							for (int i = 0; i < bytes.Length; i++)
							{
								value |= bytes[i] << (i * 8);
							}
							result = value.ToString();
						}
						else
						{
							// 超出整数范围
							result = string.Join(targetSeparator, bytes.Select(b => ((int)b).ToString()));
						}
						break;

					case "十六进制":
						result = BitConverter.ToString(bytes).Replace("-", "").ToLower();
						break;

					case "二进制":
						result = string.Join("", bytes.Select(b => System.Convert.ToString(b, 2).PadLeft(8, '0')));
						break;

					case "Base64":
						result = System.Convert.ToBase64String(bytes);
						break;

					default:
						throw new NotSupportedException($"不支持的目标格式: {SelectedTargetFormat}");
				}

				results.Add(result);
			}

			// 根据选择的分隔符组合结果
			if (SelectedTargetFormat == "十六进制" && targetSeparator != "")
			{
				// 十六进制特殊处理，按每两个字符分隔
				List<string> hexResults = new List<string>();
				foreach (var result in results)
				{
					List<string> hexParts = new List<string>();
					for (int i = 0; i < result.Length; i += 2)
					{
						if (i + 2 <= result.Length)
						{
							hexParts.Add(result.Substring(i, 2));
						}
						else
						{
							hexParts.Add(result.Substring(i));
						}
					}
					hexResults.Add(string.Join(targetSeparator, hexParts));
				}
				OutputText = string.Join(targetSeparator, hexResults);
			}
			else if (SelectedTargetFormat == "二进制" && targetSeparator != "")
			{
				// 二进制特殊处理，每8位一组
				List<string> binaryResults = new List<string>();
				foreach (var result in results)
				{
					List<string> binaryParts = new List<string>();
					for (int i = 0; i < result.Length; i += 8)
					{
						if (i + 8 <= result.Length)
						{
							binaryParts.Add(result.Substring(i, 8));
						}
						else
						{
							binaryParts.Add(result.Substring(i));
						}
					}
					binaryResults.Add(string.Join(targetSeparator, binaryParts));
				}
				OutputText = string.Join(targetSeparator, binaryResults);
			}
			else
			{
				OutputText = string.Join(targetSeparator, results);
			}

			_toastService?.ShowToastAsync("转换完成", ToastType.Success, 1500);
		}
		catch (Exception ex)
		{
			_toastService?.ShowToastAsync($"转换失败: {ex.Message}", ToastType.Error, 2000);
		}
	}

	private void Clear()
	{
		InputText = string.Empty;
		OutputText = string.Empty;
	}

	private async Task Paste()
	{
		var text = await _clipboardService.GetTextAsync();
		if (!string.IsNullOrEmpty(text))
		{
			InputText = text;
			_toastService?.ShowToastAsync("已粘贴", ToastType.Success, 1500);
		}
		else
		{
			_toastService?.ShowToastAsync("剪贴板为空", ToastType.Info, 1500);
		}
	}

	private async Task Copy()
	{
		if (!string.IsNullOrEmpty(OutputText))
		{
			await _clipboardService.SetTextAsync(OutputText);
			_toastService?.ShowToastAsync("已复制到剪贴板", ToastType.Success, 1500);
		}
		else
		{
			_toastService?.ShowToastAsync("没有可复制的内容", ToastType.Warning, 1500);
		}
	}

	private string GetSeparator(string separator, string customSeparator)
	{
		switch (separator)
		{
			case "无":
				return "";
			case "逗号 (,)":
				return ",";
			case "空格 ( )":
				return " ";
			case "分号 (;)":
				return ";";
			case "自定义":
				return customSeparator;
			default:
				return "";
		}
	}

	#endregion

	public event PropertyChangedEventHandler PropertyChanged;
	protected void OnPropertyChanged([CallerMemberName] string name = null)
		=> PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}