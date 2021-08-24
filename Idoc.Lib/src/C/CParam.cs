#pragma warning disable XS0001 // Find APIs marked as TODO in Mono


namespace Idoc.Lib
{
	public class CParam
	{
		public string Code { get; set; }
		public string Modifiers { get; private set; }
		public string Type { get; private set; }
		public string Name { get; private set; }

		public string FunctionPointerName { get; private set; }
		public string FunctionPointerType { get; private set; }
		public string FunctionPointerArgs { get; private set; }

		public bool IsFunctionPointer => !string.IsNullOrEmpty(FunctionPointerName);

		public string Signature
		{
			get
			{
				if (Name == "...")
					return Name;

				if (IsFunctionPointer)
					return $"{FunctionPointerType}(*){FunctionPointerArgs}";

				return Type;
			}
		}

		public CParam(string code)
		{
			this.Code = code.Trim();
		}

		public void Parse(Lexer lexer)
		{
			var pattern = lexer.FindPattern(Pattern.Param);
			var match = pattern.Match(Code);
			Modifiers = match.Groups["parammodifier"].Value.Trim();
			Type = match.Groups["paramtype"].Value.Trim();
			Name = match.Groups["paramname"].Value.Trim();
			FunctionPointerType = match.Groups["functionreturn"].Value.Trim();
			FunctionPointerName = match.Groups["pointername"].Value.Trim();
			FunctionPointerArgs = match.Groups["functionarguments"].Value.Trim();

			if (IsFunctionPointer)
			{
				Name = FunctionPointerName;
				Type = $"{FunctionPointerType}(*){FunctionPointerArgs}";
			}
		}
	}
}