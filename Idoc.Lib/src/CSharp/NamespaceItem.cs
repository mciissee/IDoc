namespace Idoc.Lib
{
	public class NamespaceItem : DocItem
	{
		public override ItemTypes ItemType => ItemTypes.Namespace;
		public static NamespaceItem FromToken(Token token, string comment)
		{
			var item = new NamespaceItem
			{
				Token = token,
				Name = token.Groups("name"),
				Comment = new CommentItem(comment)
			};
			return item;
		}
		public override string ToString() => Token.Value;
	}
}