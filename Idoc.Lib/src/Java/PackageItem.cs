namespace Idoc.Lib
{
	public class PackageItem : DocItem
	{
		public override ItemTypes ItemType => ItemTypes.Package;
		public static PackageItem FromToken(Token token, string comment)
		{
			var item = new PackageItem
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
