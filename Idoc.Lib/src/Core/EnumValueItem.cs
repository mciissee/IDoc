#pragma warning disable XS0001 // Find APIs marked as TODO in Mono

using System.Text;

namespace Idoc.Lib
{
	public class EnumValueItem : DocItem
	{
		public override ItemTypes ItemType => ItemTypes.EnumValue;

		public virtual string Value { get; set; }

		public bool HasValue => !string.IsNullOrEmpty(Value);

		public static EnumValueItem FromToken(Token token, IDocItem scope, string comment)
		{
			var item = new EnumValueItem
			{
				Token = token,
				Scope = scope,
				Name = token.Groups("enumname"),
				Value = token.Groups("enumvalue"),
				Comment = new CommentItem(comment),
			};
			return item;
		}

		public override string ToString()
		{
			var builder = new StringBuilder();
			builder.Append(Name);
			if (HasValue)
				builder.Append($" = {Value}");
			return builder.ToString();
		}
	}
}