#pragma warning disable XS0001 // Find APIs marked as TODO in Mono

using System.Linq;
using System.Text;

namespace Idoc.Lib
{
	/// <summary>
	/// representation of a struct, union or enum in c language.
	/// </summary>
	public class CTypeItem : DocItem
	{
		private ItemTypes itemType;

		public override ItemTypes ItemType => itemType;

		/// <summary>
		/// Gets the aliases of the type
		/// </summary>
		public virtual string[] Aliases { get; set; }

		/// <summary>
		/// Gets a value indicating whether the type has aliases
		/// </summary>
		public bool HasAliases => Aliases.Any();

		// can't be redefined
		public override void Combine(IDocItem item)
		{
		}

		public static CTypeItem FromToken(Token token, ItemTypes type, IDocItem scope, string comment)
		{
			var item = new CTypeItem
			{
				Token = token,
				Name = token.Groups("name"),
				Scope = scope,
				Comment = new CommentItem(comment),
				Aliases = new string[0],
			};
			item.itemType = type;
			return item;
		}

		public override string ToString()
		{
			var builder = new StringBuilder();
			var name = Name;
			if (Aliases.Contains(name))
				name = string.Empty;
			var type = itemType.ToString().ToLower();
			builder.Append($"typedef {type} {name} {{ }}  {string.Join(", ", Aliases)} ");
			return builder.ToString();
		}
	}
}