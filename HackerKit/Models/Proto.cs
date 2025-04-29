using HackerKit.Services;
using System;
using System.Collections;
using System.Collections.Generic;
using Google.Protobuf;
using Newtonsoft.Json;
using System.Numerics;
using System.Linq;
using Newtonsoft.Json.Linq;

/*Protobuf模型*/
public class Proto : IReadOnlyDictionary<int, object>
{
	private readonly Dictionary<int, object> _fields;
	private byte[]? head;

	//可以通过索引访问proto[tag]，比如proto[0,1,2]
	public object this[int key] => _fields.TryGetValue(key, out var v) ? v : null;
	public object? this[params int[] keys]
	{
		get
		{
			object? current = this;
			foreach (var key in keys)
			{
				if (current is Proto p)
				{
					if (!p._fields.TryGetValue(key, out current))
						return null;
				}
				else if (current is IList list)
				{
					if (key < 0 || key >= list.Count)
						return null;
					current = list[key];
				}
				else
				{
					return null;
				}
			}
			return current;
		}
	}

	public byte[]? Head
	{
		get => head;
		set => head = value;
	}

	public Proto()
	{
		_fields = new Dictionary<int, object>();
	}

	public Proto(Dictionary<int, object> fields)
	{
		_fields = fields ?? new Dictionary<int, object>();
	}

	public JToken pbJson => JToken.Parse(ToJson()); //返回一个JSON的动态对象（没有递归解析hex）

	public IEnumerable<int> Keys => _fields.Keys;

	public IEnumerable<object> Values => _fields.Values;

	public int Count => _fields.Count;

	public bool ContainsKey(int key) => _fields.ContainsKey(key);

	public bool TryGetValue(int key, out object value) => _fields.TryGetValue(key, out value);

	public IEnumerator<KeyValuePair<int, object>> GetEnumerator() => _fields.GetEnumerator();

	IEnumerator IEnumerable.GetEnumerator() => _fields.GetEnumerator();

	public string ToHex() => BitConverter.ToString(ProtobufService.Encode(this)).Replace("-", "");

	public byte[] ToBuffer() => ProtobufService.Encode(this);

	public string ToJson(bool isShowHead = false)
	=> JsonConvert.SerializeObject(ToJsonObject(this, isShowHead), Formatting.Indented);

	private static object? ToJsonObject(object? obj, bool isShowHead)
	{
		if (obj == null) return null;

		switch (obj)
		{
			case byte[] bytes:
				{
					if (bytes.Length == 0) return "";
					string? str = null;
					try
					{
						str = System.Text.Encoding.UTF8.GetString(bytes);
					}
					catch { }
					if (!string.IsNullOrEmpty(str))
						if (str.IsBase64String() || str.IsPrintableString())
							return str;
					return "hex->" + BitConverter.ToString(bytes).Replace("-", "").ToUpperInvariant();
				}
			case ByteString bs:
				{
					var arr = bs.ToByteArray();
					if (arr.Length == 0) return "";
					string? str = null;
					try
					{
						str = bs.ToStringUtf8();
					}
					catch { }
					if (!string.IsNullOrEmpty(str))
					{
						if (str.IsBase64String() || str.IsPrintableString())
							return str;
					}
					return "hex->" + BitConverter.ToString(arr).Replace("-", "").ToUpperInvariant();
				}
			case Proto proto:
				{
					bool hasHead = isShowHead && proto.Head != null && proto.Head.Length > 0;
					var keys = proto.Keys.ToList();
					keys.Sort();
					var decodedDict = new Dictionary<string, object?>(proto.Count);
					foreach (var key in keys)
					{
						var val = proto[key];
						decodedDict[key.ToString()] = ToJsonObject(val, isShowHead);
					}
					if (hasHead)
					{
						return new Dictionary<string, object?>
						{
							["head"] = BitConverter.ToString(proto.Head!).Replace("-", "").ToLowerInvariant(),
							["decoded"] = decodedDict
						};
					}
					else
					{
						return decodedDict;
					}
				}
			case IList list:
				{
					int count = list.Count;
					if (count > 0 && list[0] is int)
					{
						bool allByte = true;
						for (int i = 0; i < count; i++)
						{
							if (!(list[i] is int vi && vi >= 0 && vi <= 255))
							{
								allByte = false;
								break;
							}
						}
						if (allByte)
						{
							var bytes2 = new byte[count];
							for (int i = 0; i < count; i++)
								bytes2[i] = (byte)(int)list[i];
							string? str = null;
							try
							{
								str = System.Text.Encoding.UTF8.GetString(bytes2);
							}
							catch { }
							if (string.IsNullOrEmpty(str) || str.IsPrintableString())
								return str;
							return "hex->" + BitConverter.ToString(bytes2).Replace("-", "").ToUpperInvariant();
						}
					}
					var arr = new object?[count];
					for (int i = 0; i < count; i++)
						arr[i] = ToJsonObject(list[i], isShowHead);
					return arr;
				}
			case BigInteger bi:
				return bi.ToString();
			default:
				return obj;
		}
	}

	public void SetField(int tag, object value)
	{
		_fields[tag] = value;
	}

	public void AddField(int tag, object value)
	{
		if (_fields.TryGetValue(tag, out var existing))
			if (existing is IList list)
				list.Add(value);
			else
			{
				var newList = new List<object> { existing, value };
				_fields[tag] = newList;
			}
		else
			_fields[tag] = value;
	}
}