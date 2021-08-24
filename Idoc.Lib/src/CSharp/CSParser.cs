#pragma warning disable XS0001 // Find APIs marked as TODO in Mono

using System;
using System.Text;

namespace Idoc.Lib
{
	/// <summary>
	/// C# source code parser
	/// </summary>
	public class CSParser : BaseParser
	{
		static string[] Primitives =
		{
		   "int", "Int32", "float", "Single", "Single64", "Single32", "bool", "string",
		   "double", "short", "long", "object","byte","char",
		};

		public static string[] BuiltIn =
		{
			"public", "get", "set", "protected", "private", "internal", "virtual", "this",  "const", "readonly","lock",
			"abstract", "void", "new", "static", "class", "enum", "struct", "interface", "var",
			"int", "float", "Single", "Single64", "Single32", "bool", "string", "byte", "double", "short", "long",
			"char","Exception", "out", "in", "ref", "try", "catch", "final","finally", "goto", "List" , "using", "namespace", "params",
			"get;", "set;", "typeof", "for", "foreach", "if", "else", "switch", "case", "return","break", "continue", "while", "do", "throw",
			"base", "where", "from", "select", "as", "nameof", "is", "in", "delegate", "event", "null", "true", "false", "object", "sealed",
			"async", "extern", "volatile", "await", "dynamic", "partial", "decimal", "default", "fixed", "unsafe", "__arglist",
			"stackalloc", "Dictionary", "HashSet", "where", "from", "select", "grouby", "uint", "UInt16", "UInt32", "UInt64",
			"ushort", "ulong", "unchecked", "checked"
		};

		protected override bool InitParser()
		{
			return Tokenize(Lexer.CS);
		}

		protected override bool HandleToken()
		{
			switch (iterator.Current.Type)
			{
				case Pattern.Namespace:
					return !BeginNamespace();
				case Pattern.Class:
					return !BeginClass();
				case Pattern.Interface:
					return !BeginInterface();
				case Pattern.Struct:
					return !BeginStruct();
				case Pattern.Enum:
					return !BeginEnum();
				case Pattern.Field:
					return !ExtractField();
				case Pattern.Property:
					return !ExtractProp();
				case Pattern.Method:
					return !ExtractMethod();
			}
			return false;
		}

		private bool BeginNamespace()
		{
			return BeginScope(NamespaceItem.FromToken(iterator.Current, ExtractComment()));
		}

		private bool BeginClass()
		{
			return BeginScope(CSClassItem.FromToken(iterator.Current, ItemTypes.Class, TopScope?.Item, ExtractComment()));
		}

		private bool BeginInterface()
		{
			return BeginScope(CSClassItem.FromToken(iterator.Current, ItemTypes.Interface, TopScope?.Item, ExtractComment()));
		}

		private bool BeginStruct()
		{
			return BeginScope(CSClassItem.FromToken(iterator.Current, ItemTypes.Struct, TopScope?.Item, ExtractComment()));
		}

		private bool BeginEnum()
		{
			var hasBegan = BeginScope(CSClassItem.FromToken(iterator.Current, ItemTypes.Enum, TopScope?.Item, ExtractComment()));
			if (hasBegan)
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
			return hasBegan;
		}

		private bool ExtractField()
		{
			var token = iterator.Current;
			var scope = TopScope;
			if (scope == null)
			{
				Logger.LogError($"{filePath}: The scope of the field {token.Groups("fieldname")} is missing");
				return false;
			}
			scope.Push(CSFieldItem.FromToken(token, TopScope?.Item, ExtractComment()));
			return true;
		}

		private bool ExtractProp()
		{
			var token = iterator.Current;
			var scope = TopScope;
			if (scope == null)
			{
				Logger.LogError($"{filePath}: The scope of the property {token.Groups("propname")} is missing");
				return false;
			}

			var item = CSPropertyItem.FromToken(token, TopScope?.Item, ExtractComment());
			var body = iterator.Current.Groups("propbody");
			item.HasSetter |= body.Contains("set");
			item.HasGetter = body.Contains("get") || !item.HasSetter;
			scope.Push(item);
			return true;
		}

		private bool ExtractMethod()
		{
			var token = iterator.Current;
			var scope = TopScope;
			if (scope == null)
			{
				Logger.LogError($"{filePath} The scope of the method {token.Groups("methodname")} is missing");
				return false;
			}

			var type = token.Groups("methodtype");
			var name = token.Groups("methodname");

			if (string.IsNullOrEmpty(type))
			{
				scope.Push(CSConstructorItem.FromToken(token, scope.Item, ExtractComment()));
			}
			else if (name.StartsWith("operator", StringComparison.Ordinal))
			{
				scope.Push(CSOperatorItem.FromToken(token, scope.Item, ExtractComment()));
			}
			else
			{
				scope.Push(CSMethodItem.FromToken(token, scope.Item, ExtractComment()));
			}
			return true;
		}
	}
}