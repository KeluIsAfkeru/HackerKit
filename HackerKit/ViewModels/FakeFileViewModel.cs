using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Windows.Input;
using HackerKit.Models;
using HackerKit.Services;
using HackerKit.Services.Interfaces;

namespace HackerKit.ViewModels;

public class FakeFileViewModel : INotifyPropertyChanged
{
	private readonly IClipboardService _clipboardService;
	private readonly IToastService _toastService;

	private string _fileName;
	private string _fileSize;
	private string _resultText;
	private bool _hasResult;

	public FakeFileViewModel(IClipboardService clipboardService, IToastService toastService)
	{
		_clipboardService = clipboardService;
		_toastService = toastService;

		// 初始化命令
		GenerateCommand = new Command(Generate);
		ClearCommand = new Command(Clear);
		CopyResultCommand = new Command(async () => await CopyResult());

		// 初始化默认值
		_hasResult = false;
	}

	#region 属性

	public string FileName
	{
		get => _fileName;
		set
		{
			if (_fileName != value)
			{
				_fileName = value;
				OnPropertyChanged();
			}
		}
	}

	public string FileSize
	{
		get => _fileSize;
		set
		{
			if (_fileSize != value)
			{
				_fileSize = value;
				OnPropertyChanged();
			}
		}
	}

	public string ResultText
	{
		get => _resultText;
		set
		{
			if (_resultText != value)
			{
				_resultText = value;
				OnPropertyChanged();
			}
		}
	}

	public bool HasResult
	{
		get => _hasResult;
		set
		{
			if (_hasResult != value)
			{
				_hasResult = value;
				OnPropertyChanged();
			}
		}
	}

	#endregion

	#region 命令

	public ICommand GenerateCommand { get; }
	public ICommand ClearCommand { get; }
	public ICommand CopyResultCommand { get; }

	#endregion

	#region 方法

	private void Generate()
	{
		try
		{
			var parameters = new Dictionary<string, object>();

			if (!string.IsNullOrEmpty(FileName?.Trim()))
				parameters["f4"] = FileName.Trim();

			if (ulong.TryParse(FileSize?.Trim(), out ulong fileSizeValue) && fileSizeValue > 0)
				parameters["f3"] = fileSizeValue.ToString();

			var result = FakeFileService.MakeFakeFileJson(parameters);
			ResultText = JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
			HasResult = true;

			_toastService?.ShowToastAsync("生成FakeFile成功辣~", ToastType.Success, 1500);
		}
		catch (Exception ex)
		{
			_toastService?.ShowToastAsync($"生成FakeFile失败惹~：{ex.Message}", ToastType.Error, 2000);
		}
	}

	private void Clear()
	{
		FileName = string.Empty;
		FileSize = string.Empty;
		ResultText = string.Empty;
		HasResult = false;
	}

	private async Task CopyResult()
	{
		var text = ResultText ?? "";
		if (!string.IsNullOrWhiteSpace(text))
		{
			await _clipboardService.SetTextAsync(text);
			_toastService?.ShowToastAsync("结果已复制", ToastType.Success, 1500);
		}
		else
		{
			_toastService?.ShowToastAsync("没有内容可复制", ToastType.Warning, 1500);
		}
	}

	#endregion

	#region INotifyPropertyChanged

	public event PropertyChangedEventHandler PropertyChanged;

	protected void OnPropertyChanged([CallerMemberName] string name = null)
		=> PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

	#endregion
}