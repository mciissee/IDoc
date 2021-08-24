using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Idoc.Lib
{

	public class Lexer
	{
		private const string PATTERN = "#";
		private const string HELPER = "##";

		private static readonly Regex PatternRegex = new Regex(@"^(##?)(?<type>\w+)\s*=>\s*(?<value>.+)", RegexOptions.Compiled);
		private static readonly Regex FunctionRegex = new Regex(@"\$(?<name>\w+)\((?:\$(?<param>\w+)[,\s]*)+\)[\s]*=>(?<body>[^\n]+)", RegexOptions.Compiled);

		private static Lexer cLexer;
		private static Lexer csLexer;
		private static Lexer javaLexer;

		public static Lexer Java => javaLexer ?? (javaLexer = FromText(ResourceLoader.Load("grammar-java.txt")));
		public static Lexer C => cLexer ?? (cLexer = FromText(ResourceLoader.Load("grammar-c.txt")));
		public static Lexer CS => csLexer ?? (csLexer = FromText(ResourceLoader.Load("grammar-cs.txt")));

		public Pattern[] Patterns { get; set; }

		private static Lexer FromText(string input)
		{
			input = Regex.Replace(input, $"###[^\n]*", "\n");
			var functions = new List<PatternFunction>();

			var lines = input.Split('\n');

			var builder = new StringBuilder();
			var patterns = new List<Pattern>();

			var lineIndex = 0;
			var numberOfLines = lines.Length;

			var line = string.Empty;
			Match match;

			var builderText = string.Empty;
			var patternValue = string.Empty;
			var patternType = string.Empty;

			bool isPattern(string arg) => arg.StartsWith(PATTERN, StringComparison.Ordinal);
			bool isHelper(string arg) => arg.StartsWith(HELPER, StringComparison.Ordinal);

			bool extractFunction(string arg)
			{
				match = FunctionRegex.Match(arg);
				if (!match.Success)
					return false;

				var name = match.Groups["name"].Value.Trim();
				if (functions.Any(it => it.Name == name))
					return true;
				var body = match.Groups["body"].Value.Trim();
				var captures = match.Groups["param"].Captures;
				var @params = new string[captures.Count];
				for (var i = 0; i < captures.Count; i++)
					@params[i] = captures[i].Value.Trim();

				functions.Add(new PatternFunction(name, body, @params));
				return true;
			}
			do
			{
				line = lines[lineIndex].Trim();
				if (!extractFunction(line))
				{
					builder.Append(line);
					if (isPattern(line))
					{
						for (var j = lineIndex + 1; j < numberOfLines; j++)
						{
							if (extractFunction(lines[j]))
								continue;

							// Don't trim the line because
							// a non pattern line can begin spaces followed by the char #
							if (isPattern(lines[j]))
								break;

							builder.Append(lines[j].Trim());
							lineIndex = j;
						}

						builderText = builder.ToString();
						builder.Remove(0, builder.Length);

						match = PatternRegex.Match(builderText.BreakLines());
						patternType = match.Groups["type"].Value;
						patternValue = match.Groups["value"].Value;
						patterns.Add(new Pattern(patternType, patternValue, isHelper(line)));
					}
				}
				lineIndex++;

			} while (lineIndex < numberOfLines);

			foreach (var it in patterns)
				it.ProcessFunctions(functions);

			foreach (var it in patterns)
				it.FindReferences(patterns);

			foreach (var it in patterns)
				it.BuildExpression();

			patterns.Reverse();
			return new Lexer { Patterns = patterns.ToArray() };
		}

		public Pattern FindPattern(string type) => Patterns.FirstOrDefault(it => it.Type.Equals(type));

		public List<Token> Tokenize(string input, string filePath = null)
		{
			Logger.Log($"Tokenizing {filePath}...");
			filePath = filePath ?? string.Empty;
			var tokens = new List<Token>();
			var remaining = input;
			var isMatch = false;
			var line = 1;
			var col = 0;
			Match match;
			try
			{
				while (!string.IsNullOrEmpty(remaining))
				{
					switch (remaining[0])
					{
						case '\n':
							col = 0;
							line++;
							remaining = remaining.Substring(1);
							continue;
						case ' ':
						case '\t':
						case '\r':
							col++;
							remaining = remaining.Substring(1);
							continue;
					}
					isMatch = false;
					foreach (var pattern in Patterns)
					{
						if (pattern.IsHelper)
							continue;

						match = null;
						try
						{
							match = pattern.Match(remaining);
						}
						catch
						{
							Logger.LogError($"Lexer: {pattern.Type} timout\nfile={filePath} \nline={line} \ninput={remaining.TakeWhile('\n')}\n");
							return new List<Token>();
						}

						if (match != null && match.Success)
						{

							line += match.Value.Count(it => it == '\n');
							var length = match.Length;
							remaining = remaining.Substring(length);
							tokens.Add(new Token
							{
								Lexer = this,
								FilePath = filePath,
								Value = match.Value.Trim(),
								Pattern = pattern,
								Position = new Position { Line = line, Col = col },
								Match = match
							});

							col += length;
							isMatch = true;

							break;
						}
					}


					if (!isMatch)
					{
						Logger.LogError($"Lexer: unknown token\nfile={filePath} \nline={line} \ninput={remaining.TakeWhile('\n')}\n");
						return new List<Token>();
					}
				}
			}
			catch (Exception e)
			{
				Logger.LogError($"Lexer: parsing canceled \nfile={filePath} \nexception={e}");
				return new List<Token>();
			}
			return tokens;
		}

		internal Token TestMatch(string input)
		{
			Token result = null;
			Match match;
			foreach (var pattern in Patterns)
			{
				if (pattern.IsHelper)
					continue;

				match = null;
				try
				{
					match = pattern.Match(input);
				}
				catch
				{
					continue;
				}

				if (match != null && match.Success)
				{
					result = new Token
					{
						Lexer = this,
						Value = match.Value.Trim(),
						Pattern = pattern,
						Position = new Position { Line = 0, Col = 0 },
						Match = match
					};
					break;
				}
			}
			return result;
		}

		public void WriteToFile(string path)
		{
			if (File.Exists(path))
			{
				File.Delete(path);
			}

			using var stream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write);
			using var writer = new StreamWriter(stream);
			foreach (var pattern in Patterns)
			{
				var expression = pattern.Expression.Replace("\"", "\"\"");
				writer.WriteLine($"public static Regex {pattern.Type} =  new Regex(@\"{""}\", RegexOptions.Compiled);");
			}
			writer.WriteLine(); writer.WriteLine();
			foreach (var pattern in Patterns)
			{
				writer.WriteLine($"// {pattern.Type} =  {pattern.Expression}\n\n");
			}
		}
	}
}