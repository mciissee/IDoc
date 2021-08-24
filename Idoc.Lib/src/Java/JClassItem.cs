#pragma warning disable XS0001 // Find APIs marked as TODO in Mono

using System.Linq;
using System.Text;

namespace Idoc.Lib
{
	public class JClassItem : DocItem
	{
		ItemTypes itemType;
		public override ItemTypes ItemType => itemType;

		public virtual string Access { get; set; }
		public virtual string Modifiers { get; set; }
		public virtual string[] Bases { get; set; }
		public virtual string[] Implements { get; set; }
		public virtual string[] Annotations { get; set; }

		public bool HasAccess => !string.IsNullOrEmpty(Access);
		public bool HasModifier => !string.IsNullOrEmpty(Modifiers);
		public bool HasImplement => Implements.Any();
		public bool HasBase => Bases.Any();
		public bool HasAnnotation => Annotations.Any();

		// only one java class with the same name per package
		// so we do nothing if there is two or more class with the same name
		public override void Combine(IDocItem item)
		{
		}

		public static JClassItem FromToken(Token token, ItemTypes type, IDocItem scope, string comment)
		{
			var item = new JClassItem
			{
				Token = token,
				Name = token.Groups("name"),
				Scope = scope,
				Access = token.Groups("access"),
				Modifiers = token.Groups("modifier"),
				Bases = token.Captures("extends").Select(it => it.Value.Trim()).ToArray(),
				Implements = token.Captures("implements").Select(it => it.Value.Trim()).ToArray(),
				Annotations = token.Captures("annotation").Select(it => it.Value.Trim()).ToArray(),
				Comment = new CommentItem(comment),
			};
			item.itemType = type;
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

			builder.Append($"{itemType.ToString().ToLower()} {Name} ");

			if (HasBase)
				builder.Append($"extends {string.Join(", ", Bases)} ");

			if (HasImplement)
				builder.Append($"implements {string.Join(", ", Bases)} ");

			return builder.ToString();
		}

	}
}