#pragma warning disable XS0001 // Find APIs marked as TODO in Mono


using System.Linq;

namespace Idoc.Lib
{
	public class CSOperatorItem : CSMethodItem
	{
		public override ItemTypes ItemType => ItemTypes.Operator;

		public static new CSOperatorItem FromToken(Token token, IDocItem scope, string comment)
		{
			var item = new CSOperatorItem();
			item.Init(token, scope, comment);
			return item;
		}
	}
}