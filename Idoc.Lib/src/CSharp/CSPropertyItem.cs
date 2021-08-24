#pragma warning disable XS0001 // Find APIs marked as TODO in Mono

using System.Linq;
using System.Text;

namespace Idoc.Lib
{
	public class CSPropertyItem : DocItem
	{
		public override ItemTypes ItemType => ItemTypes.Property;
		public virtual string Access { get; set; }
		public virtual string Type { get; set; }
		public virtual string Modifiers { get; set; }

		public virtual bool HasGetter { get; set; }
		public virtual bool HasSetter { get; set; }

		public virtual string[] Attributes { get; set; }

		public bool HasAttribute => Attributes.Any();
		public bool HasAccess => !string.IsNullOrEmpty(Access);
		public bool HasModifier => !string.IsNullOrEmpty(Modifiers);

		public static CSPropertyItem FromToken(Token token, IDocItem scope, string comment)
		{
			var item = new CSPropertyItem
			{
				Token = token,
				Scope = scope,
				Access = token.Groups("propaccess"),
				Modifiers = token.Groups("propmodifier"),
				Type = token.Groups("proptype"),
				Name = token.Groups("propname"),
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

			builder.Append($"{Type} {Name} {{ ");

			if (HasGetter)
				builder.Append("get; ");

			if (HasSetter)
				builder.Append("set; ");

			builder.Append($"}}");
			return builder.ToString();
		}
	}
}