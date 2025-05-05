using System;
using System.Linq;
using System.Text;

namespace HackerKit.Services
{
	public static class BytesService
	{
		public static byte[] ParseHexWithSpaces(this string hexWithSpaces)
		{
			var cleaned = System.Text.RegularExpressions.Regex.Replace(hexWithSpaces, @"\s+", "");
			if (cleaned.Length % 2 != 0)
				throw new ArgumentException("Hex string length must be even after removing spaces.");

			int len = cleaned.Length / 2;
			var result = new byte[len];
			for (int i = 0; i < len; i++)
				result[i] = Convert.ToByte(cleaned.Substring(i * 2, 2), 16);
			return result;
		}

		public static string BytesToHex(this byte[] bytes)
		{
			if (bytes == null || bytes.Length == 0)
				return string.Empty;

			var sb = new StringBuilder(bytes.Length * 2);
			foreach (var b in bytes)
				sb.AppendFormat("{0:x2}", b);
			return sb.ToString();
		}


		public static byte[] HexToBytes(this string hex)
		{
			int len = hex.Length;
			if (len % 2 != 0)
				throw new FormatException("Invalid hex string length");
			var bytes = new byte[len / 2];
			for (int i = 0; i < bytes.Length; i++)
			{
				bytes[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
			}
			return bytes;
		}

		public static bool IsProtobuf(this byte[] data)
		{
			if (data.Length < 2) return false;

			bool hasValidTags = false;
			for (int i = 0; i < Math.Min(10, data.Length - 1); i++)
			{
				byte tag = data[i];
				if ((tag & 0x07) <= 5 && (tag >> 3) > 0 && (tag >> 3) < 100)
				{
					hasValidTags = true;
					break;
				}
			}

			return hasValidTags;
		}

		public static bool TryGetString(this byte[] data, out string result)
		{
			result = null;
			if (data == null || data.Length == 0) return false;

			try
			{
				result = System.Text.Encoding.UTF8.GetString(data);
				return true;
			}
			catch
			{
				return false;
			}
		}
	}
}
