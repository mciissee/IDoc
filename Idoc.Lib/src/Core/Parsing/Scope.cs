#pragma warning disable XS0001 // Find APIs marked as TODO in Mono


namespace Idoc.Lib
{
	public class Scope
	{
		public int Starts { get; set; }
		public int Ends { get; set; }
		public IDocItem Item { get; set; }
		public void Push(IDocItem item) => Item.Push(item);
	}
}