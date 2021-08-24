using System.IO;
namespace Idoc.Lib
{
	public class HeaderItem : DocItem
	{
		public override ItemTypes ItemType => ItemTypes.Header;
		public static HeaderItem FromFile(string filePath, string comment)
		{
			var token = new Token
			{
				FilePath = filePath,
				Value = Path.GetFileName(filePath),
			};
			var item = new HeaderItem
			{
				Token = token,
				Name = token.Value,
				Comment = new CommentItem(comment)
			};
			return item;
		}
		public override string ToString() => Name;
	}
}
