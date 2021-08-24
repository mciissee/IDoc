using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Idoc.Lib
{
	public class Pattern
	{
		static Regex ReferenceRegex = new Regex(@"(?<!\\)#(?<ref>\w+)", RegexOptions.Compiled);
		static Regex FunctionCallRegex = new Regex(@"\$(?<name>\w+)\((?:\#(?<arg>\w+)[,\s]*)+\)");

		public const string Namespace = "N";
		public const string Package = "P";
		public const string Param = "Param";
		public const string Class = "C";
		public const string Union = "U";
		public const string Interface = "I";
		public const string Struct = "S";
		public const string Method = "M";
		public const string Field = "F";
		public const string Macro = "Macro";
		public const string Enum = "E";
		public const string EnumValue = "EnumValue";
		public const string Property = "P";
		public const string Indexer = "Ind";
		public const string Operator = "Op";
		public const string Ctor = "Ctor";
		public const string Get = "Get";
		public const string Set = "Set";
		public const string Arrow = "Arrow";
		public const string Word = "Word";
		public const string Comment = "Com";

		public Regex Regex { get; private set; }

		public string Type { get; private set; }

		public string Expression { get; private set; }

		public bool IsHelper { get; private set; }

		public Pattern(string type, string expression, bool isHelper)
		{
			type = type.TrimSpaces();
			Type = type;
			Expression = expression;
			IsHelper = isHelper;
		}

		public Match Match(string input) => Regex.Match(input);

		public bool IsMatch(string input) => Regex.IsMatch(input);

		public bool IsType(string type) => Type.Equals(type);

		public void FindReferences(IEnumerable<Pattern> patterns)
		{
			do
			{
				Expression = ReferenceRegex.Replace(Expression, match =>
				{
					var refType = match.Groups["ref"].Value;
					if (Type.Equals(refType, StringComparison.Ordinal))
						throw new Exception($"Lexer: recurvive reference of '#{Type}'");

					var reference = patterns.FirstOrDefault(it => it.Type == refType);
					if (reference == null)
						throw new NullReferenceException($"Lexer: cannot resolve '#{refType}' in '#{Type}'");
					return reference.Expression;
				});

			} while (ReferenceRegex.IsMatch(Expression));
		}

		public void ProcessFunctions(List<PatternFunction> functions)
		{
			Expression = FunctionCallRegex.Replace(Expression, match =>
			{
				var name = match.Groups["name"].Value.Trim();
				var function = functions.FirstOrDefault(f => f.Name == name);
				if (function == null)
					throw new Exception($"Lexer: failed to call the function '{name}' inside the pattern '{Type}'");

				var args = match.Groups["arg"].Captures;
				if (args.Count != function.Params.Length)
					throw new Exception($"Lexer: the function '{name}' must be called with {function.Params.Length} args inside the pattern '{Type}'");

				var body = function.Body;
				for (var i = 0; i < args.Count; i++)
					body = body.Replace(function.Params[i], $"{args[i].Value} ");
				return body;
			});

		}

		public void BuildExpression()
		{
			Expression = $@"^{Expression.TrimSpaces()}";
			Regex = new Regex(Expression, RegexOptions.Compiled, TimeSpan.FromSeconds(3));
		}

		public override string ToString() => Type;
	}
}
