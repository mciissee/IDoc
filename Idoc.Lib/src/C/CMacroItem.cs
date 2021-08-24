#pragma warning disable XS0001 // Find APIs marked as TODO in Mono


namespace Idoc.Lib
{
	public class CMacroItem : DocItem
	{
		public override ItemTypes ItemType => ItemTypes.Macro;

		protected override string FullSignature => $"#define {ToString()}";

		public string Value { get; set; }

		public bool HasValue => !string.IsNullOrEmpty(Value);

		public static CMacroItem FromToken(Token token, IDocItem scope, string comment)
		{
			return new CMacroItem
			{
				Token = token,
				Scope = scope,
				Name = token.Groups("name"),
				Value = token.Groups("value"),
				Comment = new CommentItem(comment),
			};
		}

		public override string ToString()
		{
			return $"{Name} {Value}".Trim();
		}
	}
}