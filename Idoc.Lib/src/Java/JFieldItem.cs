#pragma warning disable XS0001 // Find APIs marked as TODO in Mono

using System.Linq;
using System.Text;

namespace Idoc.Lib
{

	public class JFieldItem : DocItem
	{
		public override ItemTypes ItemType => ItemTypes.Field;
		public virtual string Access { get; set; }
		public virtual string Type { get; set; }
		public virtual string Modifiers { get; set; }
		public virtual string Value { get; set; }
		public virtual string[] Annotations { get; set; }

		public bool HasAnnotation => Annotations.Any();
		public bool HasAccess => !string.IsNullOrEmpty(Access);
		public bool HasModifier => !string.IsNullOrEmpty(Modifiers);
		public bool HasValue => !string.IsNullOrEmpty(Value);

		public static JFieldItem FromToken(Token token, IDocItem scope, string comment)
		{
			var item = new JFieldItem
			{
				Token = token,
				Scope = scope,
				Access = token.Groups("fieldaccess"),
				Modifiers = token.Groups("fieldmodifier"),
				Type = token.Groups("fieldtype"),
				Name = token.Groups("fieldname"),
				Value = token.Groups("fieldvalue"),
				Annotations = token.Captures("annotation").Select(it => it.Value.Trim()).ToArray(),
				Comment = new CommentItem(comment),
			};
			return item;
		}

		public override string ToString()
		{
			var builder = new StringBuilder();

			if (HasAnnotation)
				builder.Append($"{string.Join("\n", Annotations)}\n");

			if (HasAccess)
				builder.Append($"{Access} ");

			if (HasModifier)
				builder.Append($"{Modifiers} ");

			builder.Append($"{Type} {Name} ");

			if (HasValue && Modifiers.Contains("final"))
				builder.Append($"= {Value}");

			return builder.ToString();
		}

	}

}