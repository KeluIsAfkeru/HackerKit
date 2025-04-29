using System;
using System.Collections.Generic;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;

namespace HackerKit.Services
{
	public class FakeFileService
	{
		private static string GenerateUUID() => Guid.NewGuid().ToString();

		private static string GenerateMD5(string input)
		{
			using var md5 = MD5.Create();
			var bytes = Encoding.UTF8.GetBytes(input);
			var hashBytes = md5.ComputeHash(bytes);
			var sb = new StringBuilder();
			foreach (var b in hashBytes)
				sb.Append(b.ToString("X2"));
			return sb.ToString();
		}

		public static object MakeFakeFileJson(Dictionary<string, object>? parameters = null)
		{
			parameters ??= new Dictionary<string, object>();

			var rootProto = new Proto();

			int rootF1 = parameters.ContainsKey("f1") ? Convert.ToInt32(parameters["f1"]) : 6;
			rootProto.SetField(1, rootF1);

			var subproto2 = new Proto();

			string f7 = parameters.ContainsKey("f7") ? parameters["f7"].ToString()! : "{\"info\": \"powered by ono\"}";
			subproto2.SetField(7, f7);

			string f8 = parameters.ContainsKey("f8") ? parameters["f8"].ToString()! : GenerateMD5(GenerateUUID());
			subproto2.SetField(8, f8);

			string f4 = parameters.ContainsKey("f4") ? parameters["f4"].ToString()! : "枫叶嘿壳";
			subproto2.SetField(4, f4);

			string f3 = parameters.ContainsKey("f3") ? parameters["f3"].ToString()! : BigInteger.Pow(1024, 6).ToString();
			subproto2.SetField(3, f3);

			int subproto2F1 = parameters.ContainsKey("subproto2F1") ? Convert.ToInt32(parameters["subproto2F1"]) : 102;
			subproto2.SetField(1, subproto2F1);

			string f2 = parameters.ContainsKey("f2") ? parameters["f2"].ToString()! : GenerateUUID();
			subproto2.SetField(2, f2);

			var subproto7 = new Proto();
			subproto7.SetField(2, subproto2);

			rootProto.SetField(7, subproto7);

			var serializedData = ProtobufService.Encode(rootProto);

			int length = serializedData.Length;
			byte[] lengthBytes = new byte[2];
			lengthBytes[0] = (byte)((length >> 8) & 0xFF);
			lengthBytes[1] = (byte)(length & 0xFF);

			byte[] finalBytes = new byte[1 + 2 + length];
			finalBytes[0] = 1;
			Array.Copy(lengthBytes, 0, finalBytes, 1, 2);
			Array.Copy(serializedData, 0, finalBytes, 3, length);

			string hexOutput = BitConverter.ToString(finalBytes).Replace("-", "").ToUpperInvariant();

			var result = new Dictionary<string, object>
			{
				["5"] = new Dictionary<string, object>
				{
					["1"] = 24,
					["2"] = "hex->" + hexOutput
				}
			};

			return result;
		}
	}
}
