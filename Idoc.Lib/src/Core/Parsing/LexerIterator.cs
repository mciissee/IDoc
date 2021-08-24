#pragma warning disable XS0001 // Find APIs marked as TODO in Mono
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Idoc.Lib
{
	public class LexerIterator
	{
		private readonly int count;
		private readonly Lexer lexer;

		public int Index { get; set; }
		public bool HasNext => Index < Count - 1;

		public Token Current => (Index < 0 || Index >= Count) ? null : Tokens[Index];
		public Token Next => (Index < 0 || Index + 1 >= Count) ? null : Tokens[Index + 1];
		public List<Token> Tokens { get; private set; }

		public int Count => count;
		public bool Any => count > 0;

		public LexerIterator(string input, Lexer lexer, string filePath = null)
		{
			this.lexer = lexer;
			Tokens = lexer.Tokenize(input, filePath);
			count = Tokens.Count;
		}

		public int FindNextIndexOf(string value) => FindNextIndexOf(Index, value);

		public int FindNextIndexOf(int startIndex, string value)
		{
			for (var i = startIndex; i < Tokens.Count; i++)
				if (Tokens[i].Value == value) return i;
			return -1;
		}

		public int IndexOfClose(string open, string close)
		{
			var opened = 1;
			var cursor = Index + 2;
			var lastIndex = Count;
			while (cursor < lastIndex)
			{
				if (Tokens[cursor].Value == open)
				{
					opened++;
				}
				else if (Tokens[cursor].Value == close)
				{
					opened--;
				}

				if (opened == 0)
					break;

				cursor++;
			}

			return opened == 0 ? cursor : -1;
		}

		public Token ItemAt(int index) => (index >= 0 && index < Tokens.Count) ? Tokens[index] : null;

		public List<Token> GetRange(int index, int count)
		{
			return Tokens.GetRange(index, count);
		}

		public void GoToLastLine()
		{
			Index = Tokens.Count;
		}

		public void Move(int step = 1)
		{
			Index += step;
		}

		public void MoveTo(int line)
		{
			Index = line;
		}
	}
}