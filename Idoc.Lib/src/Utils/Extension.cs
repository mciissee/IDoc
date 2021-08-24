#pragma warning disable XS0001 // Find APIs marked as TODO in Mono
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace Idoc.Lib
{

	public static class Extensions
	{
		private readonly static Byte[] Preamble = new UTF8Encoding(true).GetPreamble();

		public static string ReplaceWord(this string input, string word, string replacement)
		{
			return Regex.Replace(input, $@"\b{word}\b", replacement);
		}

		public static string EncodeHtml(this string input)
		{
			if (string.IsNullOrEmpty(input))
				return input;
			return input.Replace("<", "&lt;").Replace(">", "&gt;");
		}

		public static string DecodeHtml(this string input)
		{
			if (string.IsNullOrEmpty(input))
				return input;
			return input.Replace("&lt;", "<").Replace("&gt;", ">");
		}

		public static string TrimSpaces(this string input)
		{
			if (string.IsNullOrEmpty(input))
				return input;

			return Regex.Replace(input, "[ \t]", string.Empty);
		}

		public static string Replace(this string input, char[] oldChars, char[] newChars)
		{
			var builder = new StringBuilder();
			var len = oldChars.Length;
			var isReplaced = false;
			for (var i = 0; i < input.Length; i++)
			{
				isReplaced = false;
				for (var j = 0; j < len; j++)
				{
					if (oldChars[j] == input[i])
					{
						builder.Append(newChars[j]);
						isReplaced = true;
						break;
					}
				}
				if (!isReplaced)
				{
					builder.Append(input[i]);
				}
			}
			return builder.ToString();
		}

		public static string TrimSuffix(string word)
		{
			int apostropheLocation = word.IndexOf('\'');
			if (apostropheLocation != -1)
			{
				word = word.Substring(0, apostropheLocation);
			}

			return word;
		}

		public static string BreakLines(this string input, string replacement = "")
		{
			if (string.IsNullOrEmpty(input))
				return input;
			return Regex.Replace(input, @"\r\n?|\n", replacement);
		}

		public static string Extension(this Language language)
		{
			switch (language)
			{
				case Language.CS:
					return ".cs";
				case Language.C:
					return ".h";
				case Language.Java:
					return ".java";
				default:
					return language.ToString();
			}
		}

		public static string ToFileName(this string s)
		{
			s = s.Replace("+", "plus")
			   .Replace("-", "minus")
			   .Replace(",", "_")
			   .Replace("/", "div")
			   .Replace("*", "mul")
			   .Replace("[]", "array_indexer")
			   .TrimSpaces();
			return Path.GetInvalidFileNameChars().Aggregate(s, (current, c) => current.Replace(c, '_'));
		}

		public static string[] GetWords(this string input)
		{
			var matches = Regex.Matches(input, @"\b[\w']*\b");
			var words = from m in matches.Cast<Match>()
						where !string.IsNullOrEmpty(m.Value)
						select TrimSuffix(m.Value);

			return words.ToArray();
		}

		public static string Take(this string input, int count, bool withDots = true)
		{
			var builder = new StringBuilder();
			var length = input.Length;
			for (var i = 0; i < length; i++)
			{
				if (i > count)
					break;

				builder.Append(input[i]);
			}
			return withDots ? $"{builder}..." : builder.ToString();
		}

		public static string TakeWhile(this string input, Func<char, bool> predicate)
		{
			var builder = new StringBuilder();
			var length = input.Length;
			for (var i = 0; i < length; i++)
			{
				if (!predicate(input[i]))
					break;
				builder.Append(input[i]);
			}
			return builder.ToString();
		}
		public static string TakeWhile(this string input, char endsChar)
		{
			var builder = new StringBuilder();
			var length = input.Length;
			for (var i = 0; i < length; i++)
			{
				if (input[i] == endsChar)
					break;
				builder.Append(input[i]);
			}
			return builder.ToString();
		}

		public static void SortIf<T>(this T[] array, bool condition) where T : IComparable
		{
			if (condition)
				Array.Sort(array);
		}

		public static void SortIf<T>(this List<T> list, bool condition) where T : IComparable
		{
			if (condition)
				list.Sort();
		}

		public static string ToJSON(this object o) => JsonConvert.SerializeObject(o);

		public static string ToXML(this object o, string root)
		{
			if (o is string s)
			{
				return JsonConvert.DeserializeXNode(s, root).ToString();
			}
			else
			{
				return JsonConvert.DeserializeXNode(o.ToJSON(), root).ToString();
			}
		}

		public static bool HasBOM(this string input)
		{
			var BOMMarkUtf8 = Encoding.UTF8.GetString(Encoding.UTF8.GetPreamble());
			if (input.StartsWith(BOMMarkUtf8, StringComparison.InvariantCulture))
				input = input.Remove(0, BOMMarkUtf8.Length);
			return input.Contains("\0");
		}

		public static bool HasBOM(this StreamReader stream)
		{
			try
			{
				var b = new char[3];
				var nBytesRead = stream.Read(b, 0, 3);

				if (nBytesRead == 3 &&
					b[0] == Preamble[0] &&
					b[1] == Preamble[1] &&
					b[2] == Preamble[2])
					return true;

			}
			catch
			{
				return false;
			}
			return false;
		}
	}
}