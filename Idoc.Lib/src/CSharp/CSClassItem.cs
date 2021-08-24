#pragma warning disable XS0001 // Find APIs marked as TODO in Mono

using System.Linq;
using System.Text;

namespace Idoc.Lib
{
	public class CSClassItem : DocItem
	{
		private ItemTypes itemType;
		public override ItemTypes ItemType => itemType;

		public virtual string Access { get; set; }
		public virtual string Modifiers { get; set; }
		public virtual string Constraints { get; set; }
		public virtual string[] Bases { get; set; }
		public virtual string[] Attributes { get; set; }

		public bool HasAccess => !string.IsNullOrEmpty(Access);
		public bool HasModifier => !string.IsNullOrEmpty(Modifiers);
		public bool HasConstraint => !string.IsNullOrEmpty(Constraints);
		public bool HasBase => Bases.Any();
		public bool HasAttribute => Attributes.Any();

		public override void Combine(IDocItem item)
		{
			base.Combine(item);

			var classItem = item as CSClassItem;
			Bases = Bases.Union(classItem.Bases).ToArray();
			Constraints = classItem.HasConstraint ? classItem.Constraints : Constraints;
		}

		public static CSClassItem FromToken(Token token, ItemTypes type, IDocItem scope, string comment)
		{
			var item = new CSClassItem
			{
				Token = token,
				Name = token.Groups("name"),
				Scope = scope,
				Access = token.Groups("access"),
				Modifiers = token.Groups("modifier"),
				Constraints = token.Groups("constraints"),
				Bases = token.Captures("base").Select(it => it.Value.Trim()).ToArray(),
				Attributes = token.Captures("attribute").Select(it => it.Value.Trim()).ToArray(),
				Comment = new CommentItem(comment),
			};
			item.itemType = type;
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

			builder.Append($"{itemType.ToString().ToLower()} {Name} ");

			if (HasBase)
				builder.Append($": {string.Join(", ", Bases)} ");
			if (HasConstraint)
				builder.Append($"{Constraints} ");

			return builder.ToString();
		}

	}
}