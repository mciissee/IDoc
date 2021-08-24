#pragma warning disable XS0001 // Find APIs marked as TODO in Mono

using System;
using System.Collections.Generic;
using System.Linq;

namespace Idoc.Lib
{
	public interface IDocItem
	{
		Token Token { get; }
		string Name { get; }
		string Signature { get; }
		bool IsCommented { get; }
		CommentItem Comment { get; }
		IDocItem Scope { get; }
		ItemTypes ItemType { get; }
		IList<IDocItem> Childs { get; }
		void Push(IDocItem item);
		IDocItem FindChild(string refTitle);
		IDocItem FindChild(Func<IDocItem, bool> predicate);
		void Combine(IDocItem item);
		bool Build(Dictionary<string, Dictionary<string, object>> output);
	}
}
