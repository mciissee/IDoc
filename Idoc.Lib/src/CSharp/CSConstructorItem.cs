using System.Linq;

namespace Idoc.Lib
{
	public class CSConstructorItem : CSMethodItem
	{
		public override ItemTypes ItemType => ItemTypes.Constructor;

		public static new CSConstructorItem FromToken(Token token, IDocItem scope, string comment)
		{
			var item = new CSConstructorItem();
			item.Init(token, scope, comment);
			return item;
		}
	}
}