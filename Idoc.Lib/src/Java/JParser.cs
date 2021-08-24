using System;
using System.Text;

namespace Idoc.Lib
{
	/// <summary>
	/// Java source code parser.
	/// </summary>
	public class JParser : BaseParser
	{
		static readonly string[] Primitives =
		{
		   "int", "float", "boolean", "String",
		   "double", "short", "long", "object", "byte", "char"
		};

		public readonly static string[] BuiltIn =
		{
			"public", "protected", "private", "this",  "final", "lock", "import", "native", "package",
			"abstract", "void", "new", "static", "class", "enum", "interface", "const", "extends", "implements",
			"int", "float",  "boolean", "String", "char", "double", "short", "long", "byte", "Exception", "strictfp",
			"try", "catch", "final","finally", "goto", "List" , "using", "namespace", "params",
			"instanceof", "for", "if", "else", "switch", "case", "return","break", "continue", "while", "do", "throw", "throws",
			"super", "synchronized", "volatile", "transient", "is", "in",  "null", "true", "false", "object", "sealed",
			"volatile",  "default", "Override"
		};

		protected override bool InitParser()
		{
			return Tokenize(Lexer.Java);
		}

		protected override bool HandleToken()
		{
			switch (iterator.Current.Type)
			{
				case Pattern.Package:
					return !BeginPackage();
				case Pattern.Class:
					return !BeginClass();
				case Pattern.Interface:
					return !BeginInterface();
				case Pattern.Enum:
					return !BeginEnum();
				case Pattern.Field:
					ExtractField();
					break;
				case Pattern.Method:
					ExtractMethod();
					break;
			}
			return false;
		}

		private bool BeginPackage()
		{
			scopes.Push(new Scope
			{
				Starts = 0,
				Ends = iterator.Count,
				Item = PackageItem.FromToken(iterator.Current, ExtractComment())
			});
			return true;
		}

		private bool BeginClass()
		{
			return BeginScope(JClassItem.FromToken(iterator.Current, ItemTypes.Class, TopScope?.Item, ExtractComment()));
		}

		private bool BeginInterface()
		{
			return BeginScope(JClassItem.FromToken(iterator.Current, ItemTypes.Interface, TopScope?.Item, ExtractComment()));
		}

		private bool BeginEnum()
		{
			var hasBegan = BeginScope(JClassItem.FromToken(iterator.Current, ItemTypes.Enum, TopScope?.Item, ExtractComment()));
			if (hasBegan)
			{
				var scope = TopScope;
				var builder = new StringBuilder();
				Token token;
				while (iterator.Index < scope.Ends)
				{
					token = iterator.Current;
					switch (token.Type)
					{
						case Pattern.EnumValue:
							scope.Push(EnumValueItem.FromToken(token, scope.Item, ExtractComment()));
							break;
						case Pattern.Field:
							scope.Push(JFieldItem.FromToken(token, scope.Item, ExtractComment()));
							break;
						case Pattern.Method:
							scope.Push(JMethodItem.FromToken(token, scope.Item, ExtractComment()));
							break;
						case Pattern.Ctor:
							scope.Push(JConstructorItem.FromToken(token, scope.Item, ExtractComment()));
							break;
					}
					iterator.Move();
				}
				iterator.Index = scope.Ends - 1;
			}
			return hasBegan;
		}

		private void ExtractField()
		{
			var token = iterator.Current;
			var scope = TopScope;
			if (scope == null)
			{
				Logger.LogError($"{filePath}: The scope of the field {token.Groups("fieldname")} is missing");
				return;
			}
			scope.Push(JFieldItem.FromToken(token, TopScope?.Item, ExtractComment()));
		}

		private void ExtractMethod()
		{
			var token = iterator.Current;
			var scope = TopScope;
			if (scope == null)
			{
				Logger.LogError($"{filePath} The scope of the method {token.Groups("methodname")} is missing");
				return;
			}

			var type = token.Groups("methodtype");
			var name = token.Groups("methodname");

			if (string.IsNullOrEmpty(type))
			{
				scope.Push(JConstructorItem.FromToken(token, scope.Item, ExtractComment()));
			}
			else
			{
				scope.Push(JMethodItem.FromToken(token, scope.Item, ExtractComment()));
			}
		}
	}
}
