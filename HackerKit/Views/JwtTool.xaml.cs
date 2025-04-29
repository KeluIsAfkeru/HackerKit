using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;
using Xamarin.Essentials;
using HackerKit.Services;
using JWT;
using JWT.Algorithms;
using JWT.Serializers;

namespace HackerKit.Views
{
	public partial class JwtTool : ContentPage
	{
		private const string SecretKey = "your-256-bit-secret";

		public JwtTool()
		{
			InitializeComponent();
		}

		private async void OnEncryptClicked(object sender, EventArgs e)
		{
			var input = InputEditor.Text?.Trim();
			if (string.IsNullOrEmpty(input))
			{
				await ToastService.ShowToast("请输入JSON格式的载荷内容");
				return;
			}

			try
			{
				var payload = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(input);

				IJwtAlgorithm algorithm = new HMACSHA256Algorithm(); 
				IJsonSerializer serializer = new JsonNetSerializer();
				IBase64UrlEncoder urlEncoder = new JwtBase64UrlEncoder();
				IJwtEncoder encoder = new JwtEncoder(algorithm, serializer, urlEncoder);

				var token = encoder.Encode(payload, SecretKey);
				ResultEditor.Text = token;
				await ToastService.ShowToast("加密成功");
			}
			catch (Exception ex)
			{
				await ToastService.ShowToast($"加密失败, {ex.Message}");
			}
		}

		private async void OnDecryptClicked(object sender, EventArgs e)
		{
			var token = InputEditor.Text?.Trim();
			if (string.IsNullOrEmpty(token))
			{
				await ToastService.ShowToast("请输入JWT字符串");
				return;
			}

			try
			{
				IJsonSerializer serializer = new JsonNetSerializer();
				IDateTimeProvider provider = new UtcDateTimeProvider();
				IJwtValidator validator = new JwtValidator(serializer, provider);
				IBase64UrlEncoder urlEncoder = new JwtBase64UrlEncoder();
				IJwtAlgorithm algorithm = new HMACSHA256Algorithm();
				IJwtDecoder decoder = new JwtDecoder(serializer, validator, urlEncoder, algorithm);

				var jsonPayload = decoder.Decode(token, verify: false);
				ResultEditor.Text = jsonPayload;
				await ToastService.ShowToast("解密成功（未校验签名）");
			}
			catch (Exception)
			{
				await ToastService.ShowToast("解密失败，输入可能不是有效的JWT");
			}
		}

		private async void OnValidateClicked(object sender, EventArgs e)
		{
			var token = InputEditor.Text?.Trim();
			if (string.IsNullOrEmpty(token))
			{
				await ToastService.ShowToast("请输入JWT字符串");
				return;
			}

			try
			{
				IJsonSerializer serializer = new JsonNetSerializer();
				IDateTimeProvider provider = new UtcDateTimeProvider();
				IJwtValidator validator = new JwtValidator(serializer, provider);
				IBase64UrlEncoder urlEncoder = new JwtBase64UrlEncoder();
				IJwtAlgorithm algorithm = new HMACSHA256Algorithm();
				IJwtDecoder decoder = new JwtDecoder(serializer, validator, urlEncoder, algorithm);

				var payload = decoder.DecodeToObject<IDictionary<string, object>>(token, SecretKey, verify: true);
				ResultEditor.Text = Newtonsoft.Json.JsonConvert.SerializeObject(payload, Newtonsoft.Json.Formatting.Indented);
				await ToastService.ShowToast("校验成功，签名有效");
			}
			catch (Exception)
			{
				await ToastService.ShowToast("校验失败，签名无效或token格式错误");
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
					await ToastService.ShowToast("剪贴板为空");
				}
			}
			catch (Exception)
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
