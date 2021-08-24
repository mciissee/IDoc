#pragma warning disable XS0001 // Find APIs marked as TODO in Mono

using System.Linq;
using System.Text;

namespace Idoc.Lib
{

	public class CSFieldItem : DocItem
	{
		public override ItemTypes ItemType => ItemTypes.Field;
		public virtual string Access { get; set; }
		public virtual string Type { get; set; }
		public virtual string Modifiers { get; set; }
		public virtual string Value { get; set; }
		public virtual string[] Attributes { get; set; }

		public bool HasAttribute => Attributes.Any();
		public bool HasAccess => !string.IsNullOrEmpty(Access);
		public bool HasModifier => !string.IsNullOrEmpty(Modifiers);
		public bool HasValue => !string.IsNullOrEmpty(Value);

		public static CSFieldItem FromToken(Token token, IDocItem scope, string comment)
		{
			var item = new CSFieldItem
			{
				Token = token,
				Scope = scope,
				Access = token.Groups("fieldaccess"),
				Modifiers = token.Groups("fieldmodifier"),
				Type = token.Groups("fieldtype"),
				Name = token.Groups("fieldname"),
				Value = token.Groups("fieldvalue"),
				Attributes = token.Captures("attribute").Select(it => it.Value.Trim()).ToArray(),
				Comment = new CommentItem(comment),
			};
			return item;
		}

		public override string ToString()
		{
			var builder = new StringBuilder();

			if (HasAttribute)
				builder.Append($"{string.Join("\n", Attributes)}\n");

			if (HasAccess)
				builder.Append($"{Access} ");

			if (HasModifier)
				builder.Append($"{Modifiers} ");

			builder.Append($"{Type} {Name} ");

			if (HasValue && Modifiers.Contains("const") || Modifiers.Contains("readonly"))
				builder.Append($"= {Value}");
			return builder.ToString();
		}

	}

}