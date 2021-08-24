using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Idoc.Lib
{

	/// <summary>
	/// A sequence of a sequence recognized by a pattern of a grammar.
	/// </summary>
	public class Token
	{
		public string FilePath { get; set; }
		public Position Position { get; set; }
		public Lexer Lexer { get; set; }
		public string Value { get; set; }
		public Match Match { get; set; }
		public Pattern Pattern { get; set; }

		public string Type => Pattern?.Type;

		public string Groups(string name)
		{
			return Match.Groups[name].Value.Trim();
		}

		public List<Capture> Captures(string name)
		{

			var list = new List<Capture>();
			var group = Match.Groups[name];
			if (string.IsNullOrEmpty(group.Value))
				return list;
			foreach (Capture capture in group.Captures)
			{
				list.Add(capture);
			}
			return list;
		}

		public bool IsType(string type) => Type.Equals(type, System.StringComparison.InvariantCulture);

		public bool IsValue(string value) => Value.Equals(value, System.StringComparison.InvariantCulture);

		public override string ToString()
		{
			return $"Type: {Type}, Value: {Value}";
		}
	}
}