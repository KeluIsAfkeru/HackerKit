using HackerKit.Models;
using Google.Protobuf;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace HackerKit.Services
{
	public class ProtobufService
	{
		public static byte[] Encode(Proto proto)
		{
			using var ms = new MemoryStream(1024);
			using var cos = new CodedOutputStream(ms, leaveOpen: true);

			foreach (var kv in proto)
				WriteField(cos, kv.Key, kv.Value);

			cos.Flush();
			return ms.ToArray();
		}

		private static void WriteField(CodedOutputStream cos, int tag, object value)
		{
			if (value == null) return;

			if (value is IList list)
			{
				int count = list.Count;
				for (int i = 0; i < count; i++)
					WriteSingleField(cos, tag, list[i]);
			}
			else
			WriteSingleField(cos, tag, value);
		}

		private static void WriteSingleField(CodedOutputStream cos, int tag, object value)
		{
			switch (value)
			{
				case int i:
					cos.WriteTag(tag, WireFormat.WireType.Varint);
					cos.WriteInt32(i);
					break;
				case long l:
					cos.WriteTag(tag, WireFormat.WireType.Varint);
					cos.WriteInt64(l);
					break;
				case uint ui:
					cos.WriteTag(tag, WireFormat.WireType.Varint);
					cos.WriteUInt32(ui);
					break;
				case ulong ul:
					cos.WriteTag(tag, WireFormat.WireType.Varint);
					cos.WriteUInt64(ul);
					break;
				case bool b:
					cos.WriteTag(tag, WireFormat.WireType.Varint);
					cos.WriteBool(b);
					break;
				case float f:
					cos.WriteTag(tag, WireFormat.WireType.Fixed32);
					cos.WriteFloat(f);
					break;
				case double d:
					cos.WriteTag(tag, WireFormat.WireType.Fixed64);
					cos.WriteDouble(d);
					break;
				case string s:
					cos.WriteTag(tag, WireFormat.WireType.LengthDelimited);
					cos.WriteString(s);
					break;
				case byte[] bytes:
					cos.WriteTag(tag, WireFormat.WireType.LengthDelimited);
					cos.WriteBytes(ByteString.CopyFrom(bytes));
					break;
				case ByteString bs:
					cos.WriteTag(tag, WireFormat.WireType.LengthDelimited);
					cos.WriteBytes(bs);
					break;
				case BigInteger bi:
					var biBytes = bi.ToByteArray(isUnsigned: false, isBigEndian: true);
					cos.WriteTag(tag, WireFormat.WireType.LengthDelimited);
					cos.WriteBytes(ByteString.CopyFrom(biBytes));
					break;
				case byte bval:
					cos.WriteTag(tag, WireFormat.WireType.Varint);
					cos.WriteInt32(bval);
					break;
				case Proto nestedProto:
					var nestedBytes = Encode(nestedProto);
					cos.WriteTag(tag, WireFormat.WireType.LengthDelimited);
					cos.WriteBytes(ByteString.CopyFrom(nestedBytes));
					break;
				default:
					throw new NotSupportedException($"不支持的field value type: {value.GetType()}");
			}
		}

		public static Proto Decode(byte[] buffer)
		{
			var proto = new Proto();
			var cis = new CodedInputStream(buffer);

			while (!cis.IsAtEnd)
			{
				uint tagAndType = cis.ReadTag();
				if (tagAndType == 0) break;

				int tag = (int)(tagAndType >> 3);
				WireFormat.WireType wireType = (WireFormat.WireType)(tagAndType & 0x7);

				object value = ReadField(cis, wireType);
				if (value == null) continue;

				if (proto.TryGetValue(tag, out var exist))
				{
					if (exist is IList list)
						list.Add(value);
					else
					{
						var newList = new List<object> { exist, value };
						proto.SetField(tag, newList);
					}
				}
				else
					proto.SetField(tag, value);
			}

			return proto;
		}

		private static object ReadField(CodedInputStream cis, WireFormat.WireType wireType)
		{
			switch (wireType)
			{
				case WireFormat.WireType.Varint:
					ulong varint = cis.ReadUInt64();
					if (varint <= int.MaxValue)
						return (int)varint;
					if (varint <= long.MaxValue)
						return (long)varint;
					return varint;
				case WireFormat.WireType.Fixed64:
					return cis.ReadFixed64();
				case WireFormat.WireType.LengthDelimited:
					{
						ByteString bs = cis.ReadBytes();
						byte[] data = bs.ToByteArray();

						if (data.Length > 0 && (data[0] >= 32 && data[0] <= 126))
						{
							try
							{
								string s = System.Text.Encoding.UTF8.GetString(data);
								if (!string.IsNullOrEmpty(s) && s.All(c => c >= 32 && c <= 126))
									return s;
							}
							catch { }
						}

						try
						{
							var subProto = Decode(data);
							if (subProto.Count > 0)
								return subProto;
						}
						catch { }

						return data;
					}
				case WireFormat.WireType.Fixed32:
					return cis.ReadFixed32();
				default:
					throw new NotSupportedException($"Unsupported wireType: {wireType}");
			}
		}

		public static Proto FromJson(string json) => FromDictionary(JsonConvert.DeserializeObject<Dictionary<string, object>>(json.Trim()));

		public static Proto FromDictionary(IDictionary<string, object> dict)
		{
			var proto = new Proto();
			foreach (var kv in dict)
			{
				if (!int.TryParse(kv.Key, out int tag))
					continue;
				var val = ConvertValue(kv.Value);
				proto.SetField(tag, val);
			}
			return proto;
		}

		private static object ConvertValue(object val)
		{
			if (val == null) return null;

			if (val is Newtonsoft.Json.Linq.JObject jObj)
			{
				var dict = jObj.ToObject<Dictionary<string, object>>();
				return FromDictionary(dict);
			}
			if (val is Newtonsoft.Json.Linq.JArray jArr)
			{
				var list = new List<object>();
				foreach (var item in jArr)
				{
					list.Add(ConvertValue(item));
				}
				return list;
			}

			//把hex转回字节集
			if (val is string s && s.StartsWith("hex->", StringComparison.OrdinalIgnoreCase))
			{
				string hex = s[6..];
				if (hex.Length % 2 != 0) hex = "0" + hex;
				var bytes = new byte[hex.Length / 2];
				for (int i = 0; i < bytes.Length; i++)
					bytes[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
				return bytes;
			}

			return val;
		}

		private class JsonElementToObjectConverter : System.Text.Json.Serialization.JsonConverter<object>
		{
			public override object Read(ref System.Text.Json.Utf8JsonReader reader, Type typeToConvert, System.Text.Json.JsonSerializerOptions options)
			{
				return ReadValue(ref reader);
			}

			private object ReadValue(ref System.Text.Json.Utf8JsonReader reader)
			{
				switch (reader.TokenType)
				{
					case System.Text.Json.JsonTokenType.True:
						reader.Read();
						return true;
					case System.Text.Json.JsonTokenType.False:
						reader.Read();
						return false;
					case System.Text.Json.JsonTokenType.Number:
						if (reader.TryGetInt64(out long l))
						{
							reader.Read();
							return l;
						}
						else if (reader.TryGetDouble(out double d))
						{
							reader.Read();
							return d;
						}
						else
						{
							var s = reader.GetDecimal();
							reader.Read();
							return s;
						}
					case System.Text.Json.JsonTokenType.String:
						var str = reader.GetString();
						reader.Read();
						return str;
					case System.Text.Json.JsonTokenType.StartObject:
						var dict = new Dictionary<string, object>();
						reader.Read();
						while (reader.TokenType != System.Text.Json.JsonTokenType.EndObject)
						{
							var propName = reader.GetString();
							reader.Read();
							dict[propName] = ReadValue(ref reader);
						}
						reader.Read();
						return dict;
					case System.Text.Json.JsonTokenType.StartArray:
						var list = new List<object>();
						reader.Read();
						while (reader.TokenType != System.Text.Json.JsonTokenType.EndArray)
						{
							list.Add(ReadValue(ref reader));
						}
						reader.Read();
						return list;
					case System.Text.Json.JsonTokenType.Null:
						reader.Read();
						return null;
					default:
						throw new NotSupportedException($"Unsupported token type: {reader.TokenType}");
				}
			}

			public override void Write(System.Text.Json.Utf8JsonWriter writer, object value, System.Text.Json.JsonSerializerOptions options)
			{
				throw new NotImplementedException();
			}
		}

		public static void PrintProto(Proto proto, string indent = "")
		{
			if (proto == null)
			{
				Console.WriteLine($"{indent}null");
				return;
			}

			// 打印Head字节
			//if (proto.Head != null && proto.Head.Length > 0)
			//{
			//	Console.WriteLine($"{indent}Head: {BytesService.BytesToHex(proto.Head)}");
			//}

			var keys = new System.Collections.Generic.List<int>(proto.Keys);
			keys.Sort();

			foreach (var key in keys)
			{
				var value = proto[key];
				string keyStr = $"Field {key}";

				if (value == null)
				{
					Console.WriteLine($"{indent}{keyStr}: null");
				}
				else if (value is Proto nestedProto)
				{
					Console.WriteLine($"{indent}{keyStr}: {{");
					PrintProto(nestedProto, indent + "  ");
					Console.WriteLine($"{indent}}}");
				}
				else if (value is byte[] bytes)
				{
					Console.WriteLine($"{indent}{keyStr}: {bytes.BytesToHex()}");
				}
				else if (value is IList list)
				{
					//如果全是0导255的int，就视为字节集
					if (list.Count > 0 && list[0] is int && list.Cast<int>().All(i => i >= 0 && i <= 255))
					{
						var bytes2 = list.Cast<int>().Select(i => (byte)i).ToArray();
						Console.WriteLine($"{indent}{keyStr}: {bytes2.BytesToHex()}");
					}
					else
					{
						Console.WriteLine($"{indent}{keyStr}: [");
						foreach (var item in list)
						{
							if (item is Proto itemProto)
							{
								Console.WriteLine($"{indent}  {{");
								PrintProto(itemProto, indent + "    ");
								Console.WriteLine($"{indent}  }}");
							}
							else if (item is byte[] bytes3)
							{
								Console.WriteLine($"{indent}  {bytes3.BytesToHex()}");
							}
							else
							{
								Console.WriteLine($"{indent}  {item}");
							}
						}
						Console.WriteLine($"{indent}]");
					}
				}
				else
				{
					Console.WriteLine($"{indent}{keyStr}: {value}");
				}
			}
		}

		#region 处理hex proto

		//寻找能成功解码的最短子串前缀作为
		public static Proto? TryParseWithHead(byte[] buffer, int maxHeadLength = 12)
		{
			int len = buffer.Length;
			int limit = Math.Min(maxHeadLength, len - 1);

			for (int headLen = 0; headLen <= limit; headLen++)
			{
				byte[] protoBuf = headLen == 0
					? buffer
					: buffer.AsSpan(headLen).ToArray();

				try
				{
					var proto = Decode(protoBuf);
					if (proto != null && proto.Count > 0)
					{
						proto.Head = headLen > 0
							? buffer.AsSpan(0, headLen).ToArray()
							: null;
						return proto;
					}
				}
				catch
				{
				}
			}
			return null;
		}

		public static Proto DeepParseHexProtos(Proto root)
		{
			if (root == null) throw new ArgumentNullException(nameof(root));
			var clone = new Proto { Head = root.Head };
			foreach (var kv in root)
				clone.SetField(kv.Key, ProcessValue(kv.Value));
			return clone;
		}

		private static object ProcessValue(object value)
		{
			switch (value)
			{
				case Proto p:
					return DeepParseHexProtos(p);

				case byte[] bytes:
					return TryParseOrKeep(bytes, b => (object)b);

				case ByteString bs:
					var arr = bs.Memory.Span.ToArray();
					return TryParseOrKeep(arr, _ => bs);

				case IList list:
					if (IsRawByteList(list, out var byteArr))
					{
						return TryParseOrKeep(byteArr, _ => list);
					}
					else
					{
						var capacity = list.Count;
						var newList = new List<object>(capacity);
						foreach (var item in list)
							newList.Add(ProcessValue(item));
						return newList;
					}
				default:
					return value;
			}
		}

		private static object TryParseOrKeep(byte[] bytes, Func<byte[], object> makeOriginal)
		{
			try
			{
				var parsed = TryParseWithHead(bytes);
				if (parsed != null && parsed.Count > 0)
					return DeepParseHexProtos(parsed);
			}
			catch { }
			return makeOriginal(bytes);
		}

		private static bool IsRawByteList(IList list, out byte[] bytes)
		{
			bytes = null!;
			int count = list.Count;
			if (count == 0) return false;

			var arr = new byte[count];
			for (int i = 0; i < count; i++)
			{
				if (list[i] is int v && (uint)v <= 255)
					arr[i] = (byte)v;
				else
					return false;
			}
			bytes = arr;
			return true;
		}

		/////*递归处理protobuf对象，将可解析的"hex->"展开成新的protobuf，返回全部展开后的JToken*/
		//public static JToken RecursivelyParseHexProtos(JToken token, byte[]? outerHead = null)
		//{
		//	if (token.Type == JTokenType.Object)
		//	{
		//		var obj = (JObject)token;
		//		var newObj = new JObject();
		//		foreach (var prop in obj.Properties())
		//		{
		//			newObj[prop.Name] = RecursivelyParseHexProtos(prop.Value);
		//		}
		//		return newObj;
		//	}
		//	else if (token.Type == JTokenType.Array)
		//	{
		//		var arr = (JArray)token;
		//		var newArr = new JArray();
		//		foreach (var item in arr)
		//		{
		//			newArr.Add(RecursivelyParseHexProtos(item));
		//		}
		//		return newArr;
		//	}
		//	else if (token.Type == JTokenType.String)
		//	{
		//		var str = token.Value<string>();
		//		if (str != null && str.StartsWith("hex->", StringComparison.OrdinalIgnoreCase))
		//		{
		//			try
		//			{
		//				string hex = str.Substring(6);
		//				if (hex.Length % 2 != 0) hex = "0" + hex;
		//				var bytes = new byte[hex.Length / 2];
		//				for (int i = 0; i < bytes.Length; i++)
		//					bytes[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);

		//				var proto = TryParseWithHead(bytes);
		//				if (proto != null && proto.Count > 0)
		//				{
		//					//如果有head，设置到proto
		//					if (proto.Head != null && proto.Head.Length > 0)
		//					{
		//						//已自动设置，无需操作
		//					}
		//					return RecursivelyParseHexProtos(proto.pbJson, proto.Head);
		//				}
		//			}
		//			catch { }
		//		}
		//		return token.DeepClone();
		//	}
		//	else
		//	{
		//		return token.DeepClone();
		//	}
		//}

		////递归处理Proto对象，自动识别hex->并生成新Proto对象，保持head
		//public static Proto RecursivelyParseProtoHexFields(Proto proto)
		//{
		//	//递归处理pbJson
		//	var newJson = RecursivelyParseHexProtos(proto.pbJson, proto.Head);

		//	//转成新的Proto对象
		//	var newProto = FromJson(newJson.ToString());

		//	//复制根proto的head
		//	if (proto.Head != null && proto.Head.Length > 0)
		//		newProto.Head = proto.Head;

		//	return newProto;
		//}

		#endregion
	}
}
