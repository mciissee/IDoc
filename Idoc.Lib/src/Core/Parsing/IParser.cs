using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
#pragma warning disable XS0001 // Find APIs marked as TODO in Mono


namespace Idoc.Lib
{
	public interface IParser
	{
		Task<List<IDocItem>> Parse(string input, string filePath = null, CancellationTokenSource cancellation = null);
	}
}