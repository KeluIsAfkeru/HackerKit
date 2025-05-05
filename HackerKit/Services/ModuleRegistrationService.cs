using HackerKit.Models;
using HackerKit.Services.Interfaces;
using System.Collections.ObjectModel;
using Icon = IconPacks.IconKind.Material;

namespace HackerKit.Services
{
	[Singleton]
	public class ModuleRegistrationService : IModuleRegistrationService
	{
		private static readonly Category[] CategoriesTemplate =
		{
			new() {
				Name = "嘿壳工具", Icon = Icon.Handyman, Modules =
				{
					new Module
					{
						Title = "Protobuf编码解码", Icon = Icon.Code, Items =
						{
							new ModuleItem { Name = "Protobuf编码解码", Icon = Icon.Transform, ViewType = "ProtobufConverter" },
							//new ModuleItem { Name = "Protobuf编码", Icon = Icon.Transform, ViewType = "ProtobufEncoder" },
							//new ModuleItem { Name = "Protobuf解码", Icon = Icon.LinkOff, ViewType = "ProtobufDecoder" }
						}
					},
					new Module
					{
						Title = "QQ FakeFile生成", Icon = Icon.AttachFile, Items =
						{
							new ModuleItem { Name = "QQFakeFile生成", Icon = Icon.AttachFile, ViewType = "FakeFile" }
						}
					}
				}
			},
			new()
			{
				Name = "数字编码", Icon = Icon.Code, Modules =
				{
					new Module { Title = "文本编码", Icon = Icon.DataObject, Items = { new ModuleItem { Name = "文本编码", Icon = Icon.Translate, ViewType = "TextEncodingConverter" } } },
					//new Module { Title = "Unicode 编码/解码", Icon = Icon.FormatUnderlined, Items = { new ModuleItem { Name = "Unicode 编码/解码", Icon = Icon.Translate, ViewType = "UnicodeConverter" } } },
					//new Module { Title = "URL 编码/解码", Icon = Icon.Link, Items = { new ModuleItem { Name = "URL 编码/解码", Icon = Icon.LinkOff, ViewType = "UrlConverter" } } },
					new Module { Title = "进制转换 (单字节)", Icon = Icon.SwapHoriz, Items = { new ModuleItem { Name = "进制转换 (单字节)", Icon = Icon.SwapCalls, ViewType = "BaseConverter" } } },
					new Module { Title = "原码/反码/补码计算", Icon = Icon.Calculate, Items = { new ModuleItem { Name = "原码/反码/补码计算", Icon = Icon.Calculate, ViewType = "BinaryCodeCalculator" } } },
					new Module { Title = "JWT解析器", Icon = Icon.Key, Items = { new ModuleItem { Name = "JWT解析器", Icon = Icon.VpnKey, ViewType = "JWTTool" } } }
				}
			},
			new()
			{
				Name = "加密解密", Icon = Icon.Lock, Modules =
				{
					new Module
					{
						Title = "AES加密", Icon = Icon.EnhancedEncryption, Items =
						{
							new ModuleItem { Name = "AES加密", Icon = Icon.Security, ViewType = "AesEncryption" },
							new ModuleItem { Name = "MD5哈希", Icon = Icon.Fingerprint, ViewType = "Md5Hash" }
						}
					}
				}
			}
		};

		private readonly ObservableCollection<Category> _categories = [];

		public ObservableCollection<Category> GetCategories() => _categories;

		public void RegisterModules()
		{
			_categories.Clear();
			foreach (var cat in CategoriesTemplate)
				_categories.Add(cat);
		}
	}
}