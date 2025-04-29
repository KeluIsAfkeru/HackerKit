using System;
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
	}
}
