using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using System.Web;
using System.Windows.Input;
using HackerKit.Models;
using HackerKit.Services;
using HackerKit.Services.Interfaces;

namespace HackerKit.ViewModels;

public class TextEncodingConverterViewModel : INotifyPropertyChanged
{
	private readonly IClipboardService _clipboardService;
	private readonly IToastService _toastService;

	public TextEncodingConverterViewModel(IClipboardService clipboardService, IToastService toastService)
	{
		_clipboardService = clipboardService;
		_toastService = toastService;

		// 默认选择Base64
		_selectedEncodingType = "Base64";

		EncodeCommand = new Command(OnEncode);
		DecodeCommand = new Command(OnDecode);
		ClearCommand = new Command(OnClear);
		PasteCommand = new Command(async () => await OnPaste());
		CopyCommand = new Command(async () => await OnCopy());
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

	private string _selectedEncodingType;
	public string SelectedEncodingType
	{
		get => _selectedEncodingType;
		set { _selectedEncodingType = value; OnPropertyChanged(); }
	}

	public static string EncodeIcon { get; } = "M380.8,708.2L183.8,512l196.8,-196.2-60,-59.8L64,512l256.8,256,60,-59.8z m262.4,0L840,512l-196.8,-196.2,60,-59.8L960,512,703.2,768l-60,-59.8z";

	public static string DecodeIcon { get; } = "M999.04,512l-211.2,-211.2,-60.352,60.352L878.336,512l-106.688,106.688,60.352,60.352L999.04,512z m-85.376,341.376L170.624,110.336l-60.288,60.352,128,128L24.96,512l211.2,211.2,60.288,-60.352L145.664,512l152.96,-152.96,554.688,554.624,60.352,-60.288z";

	public ICommand EncodeCommand { get; }
	public ICommand DecodeCommand { get; }
	public ICommand ClearCommand { get; }
	public ICommand PasteCommand { get; }
	public ICommand CopyCommand { get; }

	private void OnEncode()
	{
		if (string.IsNullOrEmpty(InputText))
		{
			OutputText = "";
			_toastService?.ShowToastAsync("请在编辑框中输入内容", ToastType.Warning, 2000);
			return;
		}

		try
		{
			switch (SelectedEncodingType)
			{
				case "Base64":
					var bytesBase64 = Encoding.UTF8.GetBytes(InputText);
					OutputText = Convert.ToBase64String(bytesBase64);
					break;

				case "Unicode":
					StringBuilder unicodeBuilder = new StringBuilder();
					foreach (char c in InputText)
					{
						unicodeBuilder.Append("\\u");
						unicodeBuilder.Append(((int)c).ToString("X4"));
					}
					OutputText = unicodeBuilder.ToString();
					break;
				case "URL":
					OutputText = HttpUtility.UrlEncode(InputText);
					break;
				case "Hex":
					byte[] bytesHex = Encoding.UTF8.GetBytes(InputText);
					OutputText = BitConverter.ToString(bytesHex).Replace("-", "");
					break;
				case "UTF-8":
					OutputText = BitConverter.ToString(Encoding.UTF8.GetBytes(InputText)).Replace("-", "");
					break;
				case "UTF-32":
					OutputText = BitConverter.ToString(Encoding.UTF32.GetBytes(InputText)).Replace("-", "");
					break;
				case "ASCII":
					OutputText = BitConverter.ToString(Encoding.ASCII.GetBytes(InputText)).Replace("-", "");
					break;
				case "Punycode":
					var idn = new IdnMapping();
					OutputText = idn.GetAscii(InputText);
					break;
				case "HTML实体":
					OutputText = HttpUtility.HtmlEncode(InputText);
					break;
			}
		}
		catch (Exception e)
		{
			_toastService?.ShowToastAsync($"编码失败: {e.Message}", ToastType.Error, 2000);
		}
	}

	private void OnDecode()
	{
		if (string.IsNullOrEmpty(InputText))
		{
			OutputText = "";
			_toastService?.ShowToastAsync("你还没有输入内容捏x", ToastType.Warning, 1500);
			return;
		}

		try
		{
			switch (SelectedEncodingType)
			{
				case "Base64":
					OutputText = InputText.StringToBase64();
					break;
				case "Unicode":
					string text = InputText;
					if (!text.Contains("\\u"))
					{
						_toastService?.ShowToastAsync("Unicode格式错误", ToastType.Error, 2000);
						return;
					}

					StringBuilder unicodeBuilder = new StringBuilder();
					for (int i = 0; i < text.Length; i++)
					{
						if (text.Substring(i).Length >= 6 && text.Substring(i, 2) == "\\u")
						{
							string hexValue = text.Substring(i + 2, 4);
							unicodeBuilder.Append((char)int.Parse(hexValue, System.Globalization.NumberStyles.HexNumber));
							i += 5;
						}
						else
						{
							unicodeBuilder.Append(text[i]);
						}
					}
					OutputText = unicodeBuilder.ToString();
					break;
				case "URL":
					OutputText = HttpUtility.UrlDecode(InputText);
					break;
				case "Hex":
					string hexText = InputText.Replace(" ", "");
					if (hexText.Length % 2 != 0)
					{
						_toastService?.ShowToastAsync("Hex格式错误", ToastType.Error, 2000);
						return;
					}

					byte[] bytes = new byte[hexText.Length / 2];
					for (int i = 0; i < bytes.Length; i++)
					{
						bytes[i] = Convert.ToByte(hexText.Substring(i * 2, 2), 16);
					}
					OutputText = Encoding.UTF8.GetString(bytes);
					break;
				case "UTF-8":
					OutputText = HexDecode(InputText, Encoding.UTF8);
					break;
				case "UTF-32":
					OutputText = HexDecode(InputText, Encoding.UTF32);
					break;
				case "ASCII":
					OutputText = HexDecode(InputText, Encoding.ASCII);
					break;
				case "Punycode":
					var idn = new IdnMapping();
					OutputText = idn.GetUnicode(InputText);
					break;
				case "HTML实体":
					OutputText = HttpUtility.HtmlDecode(InputText);
					break;
			}
		}
		catch (Exception ex)
		{
			_toastService?.ShowToastAsync($"{SelectedEncodingType}格式错误: {ex.Message}", ToastType.Error, 2000);
		}
	}

	private void OnClear()
	{
		InputText = "";
		OutputText = "";
	}

	private async Task OnPaste()
	{
		var text = await _clipboardService.GetTextAsync();
		if (!string.IsNullOrEmpty(text))
		{
			InputText = text;
		}
		else
		{
			_toastService?.ShowToastAsync("剪贴板为空", ToastType.Info, 1500);
		}
	}

	private async Task OnCopy()
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

	private string HexDecode(string hex, Encoding encoding)
	{
		string hexText = hex.Replace(" ", "");
		if (hexText.Length % 2 != 0)
		{
			_toastService?.ShowToastAsync("Hex格式错误", ToastType.Error, 2000);
			return "";
		}
		byte[] bytes = new byte[hexText.Length / 2];
		for (int i = 0; i < bytes.Length; i++)
		{
			bytes[i] = Convert.ToByte(hexText.Substring(i * 2, 2), 16);
		}
		return encoding.GetString(bytes);
	}

	public event PropertyChangedEventHandler PropertyChanged;
	protected void OnPropertyChanged([CallerMemberName] string name = null)
		=> PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
