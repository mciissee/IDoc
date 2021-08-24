#pragma warning disable XS0001 // Find APIs marked as TODO in Mono

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Idoc.Lib
{
	/// <summary>
	/// C source code parser.
	/// </summary>
	public class CParser : BaseParser
	{
		static readonly Regex ALIASESES_REGEX = new Regex(@"^(?:\s*(?<alias>\*?\w+)\s*,?)*;", RegexOptions.Compiled);

		static readonly string[] Primitives =
		{
		   "int", "char", "float", "double", "signed", "unsigned", "long"
		};

		public static readonly string[] BuiltIn =
	 	{
			"auto","break","case","char","const","continue",
			"default","do","double","else","enum","extern",
			"float","for","goto","if","int","long","register",
			"return","short","signed","sizeof","static","struct",
			"switch","typedef","union","unsigned","void","volatile",
			"while", "define",
		};

		protected override bool InitParser()
		{
			if (!Tokenize(Lexer.C))
				return false;

			var comment = string.Empty;
			if (iterator.Current.IsType(Pattern.Comment))
				comment = CommentStartRegex.Replace(iterator.Current.Value, string.Empty);

			if (Regex.IsMatch(iterator.Current.Value, EXIT_COMMAND))
				return false;

			scopes.Push(new Scope
			{
				Starts = iterator.Index + 1,
				Ends = iterator.Count,
				Item = HeaderItem.FromFile(filePath, comment)
			});
			return true;
		}

		protected override bool HandleToken()
		{
			switch (iterator.Current.Type)
			{
				case Pattern.Union:
					return !BeginUnion();
				case Pattern.Struct:
					return !BeginStruct();
				case Pattern.Enum:
					return !BeginEnum();
				case Pattern.Field:
					ExtractField();
					return false;
				case Pattern.Method:
					ExtractMethod();
					return false;
				case Pattern.Macro:
					ExtractMacro();
					return false;
			}
			return false;
		}

		private bool BeginType(ItemTypes type)
		{
			var item = CTypeItem.FromToken(iterator.Current, type, TopScope?.Item, ExtractComment());
			var began = BeginScope(item);
			if (began)
			{
				var aliases = ExtractAliases();
				if (!aliases.Any() && string.IsNullOrEmpty(item.Name))
				{
					Logger.LogError($"{filePath}: missing name for a struct");
					return false;
				}
				item.Name = string.IsNullOrEmpty(item.Name) ? aliases[0] : item.Name;
				item.Aliases = aliases.ToArray();
			}
			return began;

		}

		private bool BeginUnion()
		{
			return BeginType(ItemTypes.Union);
		}

		private bool BeginStruct()
		{
			return BeginType(ItemTypes.Struct);
		}

		private bool BeginEnum()
		{
			var began = BeginScope(CTypeItem.FromToken(iterator.Current, ItemTypes.Enum, TopScope?.Item, ExtractComment()));
			if (began)
			{
				var scope = TopScope;
				var builder = new StringBuilder();
				Token token;
				while (iterator.Index < scope.Ends)
				{
					token = iterator.Current;
					if (token.IsType(Pattern.EnumValue))
					{
						scope.Push(EnumValueItem.FromToken(token, scope.Item, ExtractComment()));
					}
					iterator.Move();
				}
				iterator.Index = scope.Ends - 1;
			}
			return began;
		}

		private void ExtractField()
		{
			var token = iterator.Current;
			var scope = TopScope;
			scope.Push(CFieldItem.FromToken(token, scope.Item, ExtractComment()));
		}

		private void ExtractMacro()
		{
			var token = iterator.Current;
			var scope = TopScope;
			scope.Push(CMacroItem.FromToken(token, scope.Item, ExtractComment()));
		}

		private void ExtractMethod()
		{
			var token = iterator.Current;
			var scope = TopScope;
			scope.Push(CMethodItem.FromToken(token, scope.Item, ExtractComment()));
		}

		private List<string> ExtractAliases()
		{
			var result = new List<string>();
			var scope = TopScope;
			if (scope == null)
				return result;

			var builder = new StringBuilder();
			var i = scope.Ends + 1;
			var value = string.Empty;
			var error = true;
			while (i < iterator.Count)
			{
				value = iterator.ItemAt(i).Value;
				builder.Append(value);
				if (value.Trim().EndsWith(";", StringComparison.Ordinal))
				{
					error = false;
					break;
				}
				i++;
			}

			if (!error)
			{
				try
				{
					value = builder.ToString().TrimSpaces();
					var matches = ALIASESES_REGEX.Matches(value);
					foreach (Match match in matches)
						result.Add(match.Groups["alias"].Value.Trim());
				}
				catch
				{
					return result;
				}
			}
			return result;
		}
	}
}