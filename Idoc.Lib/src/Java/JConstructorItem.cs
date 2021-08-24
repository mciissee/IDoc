using System.Linq;

namespace Idoc.Lib
{
	public class JConstructorItem : CSMethodItem
	{
		public override ItemTypes ItemType => ItemTypes.Constructor;

		public static new JConstructorItem FromToken(Token token, IDocItem scope, string comment)
		{
			var item = new JConstructorItem();
			item.Init(token, scope, comment);
			return item;
		}
	}
}