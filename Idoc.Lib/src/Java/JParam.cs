#pragma warning disable XS0001 // Find APIs marked as TODO in Mono


namespace Idoc.Lib
{

	public class JParam
	{
		public string Code { get; set; }
		public string Annotation { get; private set; }
		public string Modifiers { get; private set; }
		public string Type { get; private set; }
		public string Name { get; private set; }
		public string Value { get; private set; }

		public string Signature => Type;

		public JParam(string code)
		{
			this.Code = code.Trim();
		}

		public void Parse(Lexer lexer)
		{
			var pattern = lexer.FindPattern(Pattern.Param);
			if (pattern != null)
			{
				var match = pattern.Match(Code);
				Annotation = match.Groups["annotation"].Value.Trim();
				Modifiers = match.Groups["parammodifier"].Value.Trim();
				Type = match.Groups["paramtype"].Value.Trim();
				Name = match.Groups["paramname"].Value.Trim();
				Value = match.Groups["paramvalue"].Value.Trim();
			}
		}
	}
}