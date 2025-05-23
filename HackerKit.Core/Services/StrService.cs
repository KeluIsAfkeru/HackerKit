﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace HackerKit.Services
{
	public static class StrService
	{
		public static bool IsUtf8String(this byte[] bytes, out string? str)
		{
			try
			{
				str = Encoding.UTF8.GetString(bytes);
				if (str.Any(c => char.IsControl(c) && c != '\r' && c != '\n'))
					return false;
				return true;
			}
			catch
			{
				str = null;
				return false;
			}
		}

		public static bool IsPrintableString(this string s)
		{
			if (string.IsNullOrEmpty(s))
				return true;

			foreach (char c in s)
			{
				UnicodeCategory category = Char.GetUnicodeCategory(c);

				if (category == UnicodeCategory.Control)
				{
					if (c != '\r' && c != '\n' && c != '\t')
						return false;
				}
				else if (category == UnicodeCategory.Format ||
						 category == UnicodeCategory.Surrogate ||
						 category == UnicodeCategory.PrivateUse ||
						 category == UnicodeCategory.OtherNotAssigned)
				{
					return false;
				}
			}
			return true;
		}

		public static bool IsBase64String(this string s)
		{
			if (string.IsNullOrEmpty(s)) return false;
			s = s.Trim();
			if (s.Length % 4 != 0) return false;
			return System.Text.RegularExpressions.Regex.IsMatch(s, @"^[A-Za-z0-9+/]*={0,2}$");
		}

		public static string StringToHex(this string input)
		{
			if (string.IsNullOrEmpty(input))
				return input;

			if (input.StartsWith("hex->"))
				return input;

			byte[] bytes = Encoding.UTF8.GetBytes(input);

			string hexString = bytes.BytesToHex();

			return $"hex-->{hexString}";
		}

		public static string StringToBase64(this string input)
		{
			if (string.IsNullOrEmpty(input))
				return input;

			var bytesBase64 = Convert.FromBase64String(input);

			var outputText = Encoding.UTF8.GetString(bytesBase64);

			return outputText;
		}

		public static bool IsJsonString(this string str)
		{
			if (string.IsNullOrEmpty(str)) return false;
			str = str.Trim();
			return (str.StartsWith("{") && str.EndsWith("}")) ||
				   (str.StartsWith("[") && str.EndsWith("]")) ||
				   str.Contains("\\\"") || 
				   (str.Contains("\"") && (str.Contains(":") || str.Contains(","))); 
		}

		public static bool ContainsNonAsciiCharacters(this string str)
		{
			return str.Any(c => c > 127);
		}

	}
}
