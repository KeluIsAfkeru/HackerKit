using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using HackerKit.Models;
using HackerKit.Services;
using HackerKit.Services.Interfaces;

namespace HackerKit.ViewModels;

public class ProtobufConverterViewModel : INotifyPropertyChanged
{
	private readonly IClipboardService _clipboardService;
	private readonly IToastService _toastService;

	public ProtobufConverterViewModel(IClipboardService clipboardService, IToastService toastService)
	{
		_clipboardService = clipboardService;
		_toastService = toastService;

		// 初始化命令
		ConvertCommand = new Command(Convert);
		ClearCommand = new Command(Clear);
		PasteCommand = new Command(async () => await Paste());
		CopyCommand = new Command(async () => await Copy());

		// 默认设置为编码模式
		_isDecodeMode = false;
		_selectedDecodeMode = "普通解码";
	}

	#region 属性

	private bool _isDecodeMode;
	public bool IsDecodeMode
	{
		get => _isDecodeMode;
		set
		{
			_isDecodeMode = value;
			OnPropertyChanged();
			OnPropertyChanged(nameof(ActionButtonText));
			OnPropertyChanged(nameof(InputPlaceholder));
		}
	}

	private string _selectedDecodeMode;
	public string SelectedDecodeMode
	{
		get => _selectedDecodeMode;
		set { _selectedDecodeMode = value; OnPropertyChanged(); }
	}

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

	public string ActionButtonText => IsDecodeMode ? "解码" : "编码";

	public string InputPlaceholder => IsDecodeMode
		? "输入Hex格式的Protobuf二进制字符串"
		: "输入Protobuf字符串(JSON格式)";

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
			_toastService?.ShowToastAsync(
				IsDecodeMode
					? "请输入Hex格式的Protobuf二进制字符串"
					: "请输入Protobuf字符串(JSON格式)",
				ToastType.Warning,
				2000);
			return;
		}

		try
		{
			if (IsDecodeMode)
			{
				// 解码操作
				string json;
				var input = InputText.Trim();

				switch (SelectedDecodeMode)
				{
					case "普通解码":
						{
							var hex = input.ParseHexWithSpaces();
							var proto = ProtobufService.TryParseWithHead(hex);
							json = proto.ToJson();
							break;
						}
					case "无head全部展开":
						{
							var hex = input.ParseHexWithSpaces();
							var parseRootHead = ProtobufService.TryParseWithHead(hex);
							var proto = ProtobufService.DeepParseHexProtos(parseRootHead);
							json = proto.ToJson(false);
							break;
						}
					case "带head全部展开":
						{
							var hex = input.ParseHexWithSpaces();
							var parseRootHead = ProtobufService.TryParseWithHead(hex);
							var proto = ProtobufService.DeepParseHexProtos(parseRootHead);
							json = proto.ToJson(true);
							break;
						}
					default:
						{
							_toastService?.ShowToastAsync("未知解码模式", ToastType.Error, 2000);
							return;
						}
				}

				OutputText = json;
				_toastService?.ShowToastAsync("解码成功，结果为JSON字符串", ToastType.Success, 1500);
			}
			else
			{
				// 编码操作
				var proto = ProtobufService.FromJson(InputText);
				var hex = proto.ToHex();
				OutputText = hex;
				_toastService?.ShowToastAsync("编码成功，结果为Hex字符串", ToastType.Success, 1500);
			}
		}
		catch (FormatException ex)
		{
			_toastService?.ShowToastAsync($"格式错误：{ex.Message}", ToastType.Error, 2000);
		}
		catch (Exception ex)
		{
			_toastService?.ShowToastAsync(
				IsDecodeMode
					? $"解码失败：{ex.Message}"
					: $"编码失败：{ex.Message}",
				ToastType.Error,
				2000);
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
			_toastService?.ShowToastAsync("内容已粘贴", ToastType.Success, 1500);
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

	#endregion

	public event PropertyChangedEventHandler PropertyChanged;
	protected void OnPropertyChanged([CallerMemberName] string name = null)
		=> PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}