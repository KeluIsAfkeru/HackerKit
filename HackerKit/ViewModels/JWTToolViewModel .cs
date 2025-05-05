using System.ComponentModel;
using System.IdentityModel.Tokens.Jwt;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Windows.Input;
using HackerKit.Models;
using HackerKit.Services.Interfaces;
using Microsoft.IdentityModel.Tokens;

namespace HackerKit.ViewModels;

public class JWTToolViewModel : INotifyPropertyChanged
{
	private readonly IClipboardService _clipboardService;
	private readonly IToastService _toastService;

	public JWTToolViewModel(IClipboardService clipboardService, IToastService toastService)
	{
		_clipboardService = clipboardService;
		_toastService = toastService;

		//初始化命令
		EncryptCommand = new Command(Encrypt);
		DecryptCommand = new Command(Decrypt);
		VerifyCommand = new Command(Verify);
		ClearCommand = new Command(Clear);
		PasteCommand = new Command(async () => await Paste());
		CopyResultCommand = new Command(async () => await CopyResult());
		GenerateKeyPairCommand = new Command(GenerateKeyPair);
		ActionCommand = new Command(ExecuteAction);

		//默认设置
		_selectedMode = 0; //默认"加密"模式
		_selectedAlgorithm = "HS256";
		_payloadJson = GenerateDefaultPayload();
		_secretKey = "this-is-a-very-secure-secret-key-with-sufficient-length-for-jwt-tokens";
		_hasResult = false;
		_hasExpirationClaim = false;
		_hasExpirationClaim = false;
	}

	#region 属性

	private int _selectedMode;
	public int SelectedMode
	{
		get => _selectedMode;
		set
		{
			_selectedMode = value;
			OnPropertyChanged();
			OnPropertyChanged(nameof(IsEncryptMode));
			OnPropertyChanged(nameof(IsDecryptMode));
			OnPropertyChanged(nameof(IsVerifyMode));
			OnPropertyChanged(nameof(IsNotEncryptMode));
			OnPropertyChanged(nameof(ActionButtonText));
			OnPropertyChanged(nameof(ActionButtonIcon));

			//切换模式时清除结果
			HasResult = false;
		}
	}

	public bool IsEncryptMode => SelectedMode == 0;
	public bool IsDecryptMode => SelectedMode == 1;
	public bool IsVerifyMode => SelectedMode == 2;
	public bool IsNotEncryptMode => !IsEncryptMode;

	public string ActionButtonText => IsDecryptMode ? "解析JWT令牌" : "验证JWT令牌";
	public string ActionButtonIcon => IsDecryptMode ? IconPacks.IconKind.Material.LockOpen : IconPacks.IconKind.Material.VerifiedUser;

	private string _selectedAlgorithm;
	public string SelectedAlgorithm
	{
		get => _selectedAlgorithm;
		set
		{
			_selectedAlgorithm = value;
			OnPropertyChanged();
			OnPropertyChanged(nameof(IsSymmetricAlgorithm));
			OnPropertyChanged(nameof(IsAsymmetricAlgorithm));
		}
	}

	public bool IsSymmetricAlgorithm => SelectedAlgorithm.StartsWith("HS");
	public bool IsAsymmetricAlgorithm => !IsSymmetricAlgorithm;

	private string _payloadJson;
	public string PayloadJson
	{
		get => _payloadJson;
		set { _payloadJson = value; OnPropertyChanged(); }
	}

	private string _secretKey;
	public string SecretKey
	{
		get => _secretKey;
		set { _secretKey = value; OnPropertyChanged(); }
	}

	private string _privateKey;
	public string PrivateKey
	{
		get => _privateKey;
		set { _privateKey = value; OnPropertyChanged(); }
	}

	private string _publicKey;
	public string PublicKey
	{
		get => _publicKey;
		set { _publicKey = value; OnPropertyChanged(); }
	}

	private string _jwtToken;
	public string JwtToken
	{
		get => _jwtToken;
		set
		{
			_jwtToken = value;
			OnPropertyChanged();
			ParseJwtToken();
		}
	}

	private bool _hasResult;
	public bool HasResult
	{
		get => _hasResult;
		set { _hasResult = value; OnPropertyChanged(); }
	}

	private bool _hasParsedToken;
	public bool HasParsedToken
	{
		get => _hasParsedToken;
		set { _hasParsedToken = value; OnPropertyChanged(); }
	}

	private bool _hasExpirationClaim;
	public bool HasExpirationClaim
	{
		get => _hasExpirationClaim;
		set { _hasExpirationClaim = value; OnPropertyChanged(); }
	}

	private string _resultTitle;
	public string ResultTitle
	{
		get => _resultTitle;
		set { _resultTitle = value; OnPropertyChanged(); }
	}

	private string _resultText;
	public string ResultText
	{
		get => _resultText;
		set { _resultText = value; OnPropertyChanged(); }
	}

	private string _resultColor;
	public string ResultColor
	{
		get => _resultColor;
		set { _resultColor = value; OnPropertyChanged(); }
	}

	private string _decodedHeader;
	public string DecodedHeader
	{
		get => _decodedHeader;
		set { _decodedHeader = value; OnPropertyChanged(); }
	}

	private string _decodedPayload;
	public string DecodedPayload
	{
		get => _decodedPayload;
		set { _decodedPayload = value; OnPropertyChanged(); }
	}

	private DateTime _expirationTime;
	public DateTime ExpirationTime
	{
		get => _expirationTime;
		set { _expirationTime = value; OnPropertyChanged(); }
	}

	private string _expirationTimeFormatted;
	public string ExpirationTimeFormatted
	{
		get => _expirationTimeFormatted;
		set { _expirationTimeFormatted = value; OnPropertyChanged(); }
	}

	private string _expirationColor;
	public string ExpirationColor
	{
		get => _expirationColor;
		set { _expirationColor = value; OnPropertyChanged(); }
	}

	#endregion

	#region 命令

	public ICommand EncryptCommand { get; }
	public ICommand DecryptCommand { get; }
	public ICommand VerifyCommand { get; }
	public ICommand ClearCommand { get; }
	public ICommand PasteCommand { get; }
	public ICommand CopyResultCommand { get; }
	public ICommand GenerateKeyPairCommand { get; }
	public ICommand ActionCommand { get; }

	#endregion

	#region 方法

	private void ExecuteAction()
	{
		if (IsDecryptMode)
		{
			Decrypt();
		}
		else if (IsVerifyMode)
		{
			Verify();
		}
	}

	private void ParseJwtToken()
	{
		try
		{
			if (string.IsNullOrWhiteSpace(JwtToken))
			{
				HasParsedToken = false;
				return;
			}

			var handler = new JwtSecurityTokenHandler();
			if (!handler.CanReadToken(JwtToken))
			{
				HasParsedToken = false;
				return;
			}

			//解析JWT令牌
			var token = handler.ReadJwtToken(JwtToken);

			//提取算法类型
			if (token.Header.TryGetValue("alg", out var algorithm))
			{
				SelectedAlgorithm = algorithm.ToString();
			}

			//设置Header解析结果
			DecodedHeader = FormatJson(token.Header.SerializeToJson());

			//设置Payload解析结果
			DecodedPayload = FormatJson(token.Payload.SerializeToJson());

			//处理过期时间
			if (token.Payload.TryGetValue("exp", out var expValue))
			{
				HasExpirationClaim = true;
				long expTimestamp = Convert.ToInt64(expValue);
				ExpirationTime = DateTimeOffset.FromUnixTimeSeconds(expTimestamp).DateTime;
				ExpirationTimeFormatted = $"过期时间: {ExpirationTime.ToLocalTime()}";

				//设置颜色
				if (ExpirationTime < DateTime.UtcNow)
				{
					ExpirationColor = "#E53935"; //红色 - 已过期
					ExpirationTimeFormatted += " (已过期)";
				}
				else
				{
					ExpirationColor = "#4CAF50"; //绿色 - 有效
					ExpirationTimeFormatted += " (有效)";
				}
			}
			else
			{
				HasExpirationClaim = false;
			}

			HasParsedToken = true;
		}
		catch
		{
			//解析失败
			HasParsedToken = false;
		}
	}

	private void Encrypt()
	{
		try
		{
			//验证JSON格式
			if (!IsValidJson(PayloadJson))
			{
				_toastService?.ShowToastAsync("Payload不是有效的JSON格式", ToastType.Warning, 2000);
				return;
			}

			//提取算法类型
			string algorithm = SelectedAlgorithm;

			//验证密钥
			if (IsSymmetricAlgorithm)
			{
				if (string.IsNullOrWhiteSpace(SecretKey))
				{
					_toastService?.ShowToastAsync("请输入密钥", ToastType.Warning, 2000);
					return;
				}

				//确保密钥长度足够
				if (Encoding.UTF8.GetByteCount(SecretKey) < 32)
				{
					_toastService?.ShowToastAsync("密钥长度不足，至少需要32字节(256位)", ToastType.Warning, 2000);
					return;
				}
			}
			else
			{
				if (string.IsNullOrWhiteSpace(PrivateKey))
				{
					_toastService?.ShowToastAsync("请输入私钥或生成密钥对", ToastType.Warning, 2000);
					return;
				}
			}

			//创建JWT令牌
			var tokenHandler = new JwtSecurityTokenHandler();
			var key = GetSecurityKey();

			//解析payload
			var payloadObj = JsonSerializer.Deserialize<Dictionary<string, object>>(PayloadJson);
			var claims = new List<Claim>();

			if (payloadObj != null)
			{
				bool hasExp = false;
				foreach (var kvp in payloadObj)
				{
					if (kvp.Key == "exp") hasExp = true;
					claims.Add(new Claim(kvp.Key, kvp.Value.ToString()));
				}

				//如果没有过期时间，添加默认1小时过期
				if (!hasExp)
				{
					claims.Add(new Claim("exp", DateTimeOffset.UtcNow.AddHours(1).ToUnixTimeSeconds().ToString()));
				}
			}

			//生成Token描述
			var tokenDescriptor = new SecurityTokenDescriptor
			{
				Subject = new ClaimsIdentity(claims),
				SigningCredentials = new SigningCredentials(key, GetSigningAlgorithm())
			};

			//生成令牌
			var token = tokenHandler.CreateToken(tokenDescriptor);
			var tokenString = tokenHandler.WriteToken(token);

			//显示结果
			ResultTitle = "JWT生成成功";
			ResultText = tokenString;
			ResultColor = "#4CAF50";
			HasResult = true;

			_toastService?.ShowToastAsync("JWT令牌生成成功", ToastType.Success, 1500);
		}
		catch (Exception ex)
		{
			ResultTitle = "JWT生成失败";
			ResultText = $"错误: {ex.Message}";
			ResultColor = "#F44336";
			HasResult = true;

			_toastService?.ShowToastAsync($"JWT令牌生成失败: {ex.Message}", ToastType.Error, 2000);
		}
	}

	private void Decrypt()
	{
		try
		{
			if (string.IsNullOrWhiteSpace(JwtToken))
			{
				_toastService?.ShowToastAsync("请输入JWT令牌", ToastType.Warning, 2000);
				return;
			}

			var handler = new JwtSecurityTokenHandler();

			if (!handler.CanReadToken(JwtToken))
			{
				_toastService?.ShowToastAsync("无效的JWT令牌格式", ToastType.Warning, 2000);
				return;
			}

			//解析JWT令牌
			var token = handler.ReadJwtToken(JwtToken);

			//格式化JSON以便更好显示
			var payload = token.Payload;
			var formattedPayload = new Dictionary<string, object>();

			foreach (var claim in payload)
			{
				//特殊处理时间戳
				if (claim.Key == "exp" || claim.Key == "iat" || claim.Key == "nbf")
				{
					if (long.TryParse(claim.Value.ToString(), out long timestamp))
					{
						var dateTime = DateTimeOffset.FromUnixTimeSeconds(timestamp).DateTime.ToLocalTime();
						formattedPayload.Add(claim.Key, $"{claim.Value} ({dateTime})");
					}
					else
					{
						formattedPayload.Add(claim.Key, claim.Value);
					}
				}
				else
				{
					formattedPayload.Add(claim.Key, claim.Value);
				}
			}

			//显示结果
			ResultTitle = "JWT解析成功";
			ResultText = FormatJson(JsonSerializer.Serialize(formattedPayload));
			ResultColor = "#4CAF50";
			HasResult = true;

			_toastService?.ShowToastAsync("JWT令牌解析成功", ToastType.Success, 1500);
		}
		catch (Exception ex)
		{
			ResultTitle = "JWT解析失败";
			ResultText = $"错误: {ex.Message}";
			ResultColor = "#F44336";
			HasResult = true;

			_toastService?.ShowToastAsync($"JWT令牌解析失败: {ex.Message}", ToastType.Error, 2000);
		}
	}

	private void Verify()
	{
		try
		{
			if (string.IsNullOrWhiteSpace(JwtToken))
			{
				_toastService?.ShowToastAsync("请输入JWT令牌", ToastType.Warning, 2000);
				return;
			}

			//验证密钥
			if (IsSymmetricAlgorithm)
			{
				if (string.IsNullOrWhiteSpace(SecretKey))
				{
					_toastService?.ShowToastAsync("请输入密钥", ToastType.Warning, 2000);
					return;
				}
			}
			else
			{
				if (string.IsNullOrWhiteSpace(PublicKey))
				{
					_toastService?.ShowToastAsync("请输入公钥", ToastType.Warning, 2000);
					return;
				}
			}

			var handler = new JwtSecurityTokenHandler();

			if (!handler.CanReadToken(JwtToken))
			{
				_toastService?.ShowToastAsync("无效的JWT令牌格式", ToastType.Warning, 2000);
				return;
			}

			//读取令牌
			var token = handler.ReadJwtToken(JwtToken);

			//获取验证参数
			var validationParameters = new TokenValidationParameters
			{
				ValidateIssuerSigningKey = true,
				IssuerSigningKey = GetValidationKey(),
				ValidateIssuer = false,
				ValidateAudience = false,
				ValidateLifetime = true,
				ClockSkew = TimeSpan.Zero
			};

			try
			{
				//验证令牌
				ClaimsPrincipal claimsPrincipal = handler.ValidateToken(JwtToken, validationParameters, out _);

				//检查过期状态
				var expClaim = claimsPrincipal.FindFirst("exp");
				if (expClaim != null)
				{
					var expTime = DateTimeOffset.FromUnixTimeSeconds(long.Parse(expClaim.Value));
					if (expTime < DateTimeOffset.UtcNow)
					{
						ResultTitle = "JWT已过期";
						ResultText = $"令牌已于 {expTime.LocalDateTime} 过期，但签名有效";
						ResultColor = "#FF9800";
						HasResult = true;
						return;
					}
				}

				ResultTitle = "JWT验证成功";
				ResultText = "该JWT令牌签名有效，可以信任";
				ResultColor = "#4CAF50";
				HasResult = true;

				_toastService?.ShowToastAsync("JWT令牌验证成功", ToastType.Success, 1500);
			}
			catch (SecurityTokenExpiredException)
			{
				ResultTitle = "JWT已过期";
				ResultText = "令牌已过期";
				ResultColor = "#FF9800";
				HasResult = true;
			}
			catch (Exception)
			{
				ResultTitle = "JWT验证失败";
				ResultText = "签名无效，请检查密钥是否正确";
				ResultColor = "#F44336";
				HasResult = true;

				_toastService?.ShowToastAsync("JWT令牌验证失败", ToastType.Error, 2000);
			}
		}
		catch (Exception ex)
		{
			ResultTitle = "JWT验证出错";
			ResultText = $"错误: {ex.Message}";
			ResultColor = "#F44336";
			HasResult = true;

			_toastService?.ShowToastAsync($"JWT令牌验证出错: {ex.Message}", ToastType.Error, 2000);
		}
	}

	private void GenerateKeyPair()
	{
		try
		{
			if (SelectedAlgorithm.StartsWith("RS"))
			{
				//生成RSA密钥对
				using (var rsa = RSA.Create(2048))
				{
					//生成私钥
					PrivateKey = ExportPrivateKey(rsa);

					//生成公钥
					PublicKey = ExportPublicKey(rsa);
				}
			}
			else if (SelectedAlgorithm.StartsWith("ES"))
			{
				//确定曲线
				ECCurve curve;
				if (SelectedAlgorithm == "ES256")
					curve = ECCurve.NamedCurves.nistP256;
				else if (SelectedAlgorithm == "ES384")
					curve = ECCurve.NamedCurves.nistP384;
				else //ES512
					curve = ECCurve.NamedCurves.nistP521;

				//生成EC密钥对
				using (var ecdsa = ECDsa.Create(curve))
				{
					//生成私钥
					PrivateKey = ExportECPrivateKey(ecdsa);

					//生成公钥
					PublicKey = ExportECPublicKey(ecdsa);
				}
			}
			else
			{
				throw new NotSupportedException($"不支持的算法类型: {SelectedAlgorithm}");
			}

			_toastService?.ShowToastAsync("密钥对生成成功", ToastType.Success, 1500);
		}
		catch (Exception ex)
		{
			_toastService?.ShowToastAsync($"生成密钥对失败: {ex.Message}", ToastType.Error, 2000);
		}
	}

	private SecurityKey GetSecurityKey()
	{
		//根据算法类型选择不同的密钥处理
		if (IsSymmetricAlgorithm)
		{
			//确保密钥长度足够
			string key = SecretKey;
			while (Encoding.UTF8.GetByteCount(key) < 32) //确保至少32字节(256位)
			{
				key += SecretKey;
			}

			return new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
		}
		else
		{
			//使用私钥
			try
			{
				if (SelectedAlgorithm.StartsWith("RS"))
				{
					var rsa = RSA.Create();
					rsa.ImportFromPem(PrivateKey);
					return new RsaSecurityKey(rsa);
				}
				else if (SelectedAlgorithm.StartsWith("ES"))
				{
					var ecdsa = ECDsa.Create();
					ecdsa.ImportFromPem(PrivateKey);
					return new ECDsaSecurityKey(ecdsa);
				}
				else
				{
					throw new NotSupportedException($"不支持的算法类型: {SelectedAlgorithm}");
				}
			}
			catch
			{
				throw new Exception("提供的私钥格式无效，请提供有效的PEM格式私钥");
			}
		}
	}

	private SecurityKey GetValidationKey()
	{
		//根据算法类型选择不同的密钥处理
		if (IsSymmetricAlgorithm)
		{
			//确保密钥长度足够
			string key = SecretKey;
			while (Encoding.UTF8.GetByteCount(key) < 32) //确保至少32字节(256位)
			{
				key += SecretKey;
			}

			return new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
		}
		else
		{
			//使用公钥
			try
			{
				if (SelectedAlgorithm.StartsWith("RS"))
				{
					var rsa = RSA.Create();
					rsa.ImportFromPem(PublicKey);
					return new RsaSecurityKey(rsa);
				}
				else if (SelectedAlgorithm.StartsWith("ES"))
				{
					var ecdsa = ECDsa.Create();
					ecdsa.ImportFromPem(PublicKey);
					return new ECDsaSecurityKey(ecdsa);
				}
				else
				{
					throw new NotSupportedException($"不支持的算法类型: {SelectedAlgorithm}");
				}
			}
			catch
			{
				throw new Exception("提供的公钥格式无效，请提供有效的PEM格式公钥");
			}
		}
	}

	private string GetSigningAlgorithm()
	{
		return SelectedAlgorithm switch
		{
			"HS256" => SecurityAlgorithms.HmacSha256Signature,
			"HS384" => SecurityAlgorithms.HmacSha384Signature,
			"HS512" => SecurityAlgorithms.HmacSha512Signature,
			"RS256" => SecurityAlgorithms.RsaSha256Signature,
			"RS384" => SecurityAlgorithms.RsaSha384Signature,
			"RS512" => SecurityAlgorithms.RsaSha512Signature,
			"ES256" => SecurityAlgorithms.EcdsaSha256Signature,
			"ES384" => SecurityAlgorithms.EcdsaSha384Signature,
			"ES512" => SecurityAlgorithms.EcdsaSha512Signature,
			_ => throw new NotSupportedException($"不支持的算法: {SelectedAlgorithm}")
		};
	}

	private string ExportPrivateKey(RSA rsa)
	{
		byte[] privateKeyBytes = rsa.ExportPkcs8PrivateKey();
		StringBuilder sb = new StringBuilder();
		sb.AppendLine("-----BEGIN PRIVATE KEY-----");

		string base64 = Convert.ToBase64String(privateKeyBytes);
		//每64个字符一行
		for (int i = 0; i < base64.Length; i += 64)
		{
			sb.AppendLine(base64.Substring(i, Math.Min(64, base64.Length - i)));
		}

		sb.AppendLine("-----END PRIVATE KEY-----");
		return sb.ToString();
	}

	private string ExportPublicKey(RSA rsa)
	{
		byte[] publicKeyBytes = rsa.ExportSubjectPublicKeyInfo();
		StringBuilder sb = new StringBuilder();
		sb.AppendLine("-----BEGIN PUBLIC KEY-----");

		string base64 = Convert.ToBase64String(publicKeyBytes);
		//每64个字符一行
		for (int i = 0; i < base64.Length; i += 64)
		{
			sb.AppendLine(base64.Substring(i, Math.Min(64, base64.Length - i)));
		}

		sb.AppendLine("-----END PUBLIC KEY-----");
		return sb.ToString();
	}


	private string ExportECPrivateKey(ECDsa ecdsa)
	{
		byte[] privateKeyBytes = ecdsa.ExportPkcs8PrivateKey();
		StringBuilder sb = new StringBuilder();
		sb.AppendLine("-----BEGIN PRIVATE KEY-----");

		string base64 = Convert.ToBase64String(privateKeyBytes);
		//每64个字符一行
		for (int i = 0; i < base64.Length; i += 64)
		{
			sb.AppendLine(base64.Substring(i, Math.Min(64, base64.Length - i)));
		}

		sb.AppendLine("-----END PRIVATE KEY-----");
		return sb.ToString();
	}

	private string ExportECPublicKey(ECDsa ecdsa)
	{
		byte[] publicKeyBytes = ecdsa.ExportSubjectPublicKeyInfo();
		StringBuilder sb = new StringBuilder();
		sb.AppendLine("-----BEGIN PUBLIC KEY-----");

		string base64 = Convert.ToBase64String(publicKeyBytes);
		//每64个字符一行
		for (int i = 0; i < base64.Length; i += 64)
		{
			sb.AppendLine(base64.Substring(i, Math.Min(64, base64.Length - i)));
		}

		sb.AppendLine("-----END PUBLIC KEY-----");
		return sb.ToString();
	}


	private void Clear()
	{
		if (IsEncryptMode)
		{
			PayloadJson = GenerateDefaultPayload();

			if (IsSymmetricAlgorithm)
			{
				SecretKey = "this-is-a-very-secure-secret-key-with-sufficient-length-for-jwt-tokens";
			}
			else
			{
				PrivateKey = string.Empty;
				PublicKey = string.Empty;
			}
		}
		else
		{
			JwtToken = string.Empty;

			if (IsSymmetricAlgorithm)
			{
				SecretKey = string.Empty;
			}
			else
			{
				PublicKey = string.Empty;
			}

			HasParsedToken = false;
		}

		HasResult = false;
	}

	private async Task Paste()
	{
		var text = await _clipboardService.GetTextAsync();
		if (!string.IsNullOrEmpty(text))
		{
			if (IsEncryptMode)
			{
				//尝试格式化JSON
				if (IsValidJson(text))
				{
					PayloadJson = FormatJson(text);
				}
				else
				{
					PayloadJson = text;
				}
			}
			else
			{
				JwtToken = text;
			}

			_toastService?.ShowToastAsync("已粘贴", ToastType.Success, 1500);
		}
		else
		{
			_toastService?.ShowToastAsync("剪贴板为空", ToastType.Info, 1500);
		}
	}

	private async Task CopyResult()
	{
		string textToCopy = ResultText;

		if (!string.IsNullOrEmpty(textToCopy))
		{
			await _clipboardService.SetTextAsync(textToCopy);
			_toastService?.ShowToastAsync("已复制到剪贴板", ToastType.Success, 1500);
		}
		else
		{
			_toastService?.ShowToastAsync("没有可复制的内容", ToastType.Warning, 1500);
		}
	}

	private bool IsValidJson(string json)
	{
		try
		{
			using (JsonDocument.Parse(json))
			{
				return true;
			}
		}
		catch
		{
			return false;
		}
	}

	private string FormatJson(string json)
	{
		try
		{
			var jsonObj = JsonSerializer.Deserialize<object>(json);
			return JsonSerializer.Serialize(jsonObj, new JsonSerializerOptions { WriteIndented = true });
		}
		catch
		{
			return json;
		}
	}

	private string GenerateDefaultPayload()
	{
		//生成当前时间戳（秒）
		long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
		long exp = DateTimeOffset.UtcNow.AddHours(1).ToUnixTimeSeconds();

		var payload = new Dictionary<string, object>
		{
			{ "sub", "1234567890" },
			{ "name", "John Doe" },
			{ "iat", now },
			{ "exp", exp }
		};

		return FormatJson(JsonSerializer.Serialize(payload));
	}

	private string ExportPkcs8PrivateKey(RSA rsa)
	{
		byte[] privateKeyBytes = rsa.ExportPkcs8PrivateKey();
		StringBuilder sb = new StringBuilder();
		sb.AppendLine("-----BEGIN PRIVATE KEY-----");

		string base64 = Convert.ToBase64String(privateKeyBytes);
		//每64字符一行
		for (int i = 0; i < base64.Length; i += 64)
		{
			sb.AppendLine(base64.Substring(i, Math.Min(64, base64.Length - i)));
		}

		sb.AppendLine("-----END PRIVATE KEY-----");
		return sb.ToString();
	}

	private string ExportSubjectPublicKeyInfo(RSA rsa)
	{
		byte[] publicKeyBytes = rsa.ExportSubjectPublicKeyInfo();
		StringBuilder sb = new StringBuilder();
		sb.AppendLine("-----BEGIN PUBLIC KEY-----");

		string base64 = Convert.ToBase64String(publicKeyBytes);
		//每64字符一行
		for (int i = 0; i < base64.Length; i += 64)
		{
			sb.AppendLine(base64.Substring(i, Math.Min(64, base64.Length - i)));
		}

		sb.AppendLine("-----END PUBLIC KEY-----");
		return sb.ToString();
	}

	#endregion

	public event PropertyChangedEventHandler PropertyChanged;
	protected void OnPropertyChanged([CallerMemberName] string name = null)
		=> PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
