#pragma warning disable XS0001 // Find APIs marked as TODO in Mono

using System.Text;

namespace Idoc.Lib
{
	public class CFieldItem : DocItem
	{
		public override ItemTypes ItemType => ItemTypes.Field;
		public virtual string Access { get; set; }
		public virtual string Type { get; set; }
		public virtual string Modifiers { get; set; }
		public virtual string Value { get; set; }

		public bool IsFunctionPointer { get; private set; }
		public bool HasAccess => !string.IsNullOrEmpty(Access);
		public bool HasModifiers => !string.IsNullOrEmpty(Modifiers);
		public bool HasValue => !string.IsNullOrEmpty(Value);

		public static CFieldItem FromToken(Token token, IDocItem scope, string comment)
		{
			var item = new CFieldItem
			{
				Token = token,
				Scope = scope,
				Access = token.Groups("fieldaccess"),
				Modifiers = token.Groups("fieldmodifier"),
				Type = token.Groups("fieldtype"),
				Name = token.Groups("fieldname"),
				Value = token.Groups("fieldvalue"),
				Comment = new CommentItem(comment),
			};

			var pointer = token.Groups("pointername");
			if (!string.IsNullOrEmpty(pointer))
			{
				item.IsFunctionPointer = true;
				item.Name = pointer;
				item.Type = "(*)";
			}
			return item;
		}

		public override string ToString()
		{
			if (IsFunctionPointer)
				return Token.Value.Replace(";", string.Empty).Trim();

			var builder = new StringBuilder();
			if (HasAccess)
				builder.Append($"{Access} ");

			if (HasModifiers)
				builder.Append($"{Modifiers} ");

			builder.Append($"{Type} {Name} ");

			if (HasValue && Modifiers.Contains("const"))
				builder.Append($"= {Value}");

			return builder.ToString();
		}
	}
}