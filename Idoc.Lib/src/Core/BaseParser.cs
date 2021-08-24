#pragma warning disable XS0001 // Find APIs marked as TODO in Mono

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Linq;

namespace Idoc.Lib
{
	public abstract class BaseParser : IParser
	{
		protected const string EXIT_COMMAND = "#EXIT";
		protected const string IGNORE_COMAND = "#IGNORE";

		protected string filePath;
		protected string input;
		protected static Regex CommentStartRegex = new Regex("///[/]*", RegexOptions.Compiled);
		protected LexerIterator iterator;
		protected List<IDocItem> items;
		protected Stack<Scope> scopes;
		protected Scope TopScope => scopes.Any() ? scopes.Peek() : null;
		protected CancellationTokenSource cancellation;

		protected BaseParser()
		{
			scopes = new Stack<Scope>();
		}

		public virtual Task<List<IDocItem>> Parse(string input, string filePath = null, CancellationTokenSource cancellation = null)
		{
			this.input = input;
			this.filePath = filePath;
			this.cancellation = cancellation;

			List<IDocItem> parseAsync()
			{
				scopes = new Stack<Scope>();
				items = new List<IDocItem>();

				if (!InitParser())
					return new List<IDocItem>();

				var ignore = false;
				var exit = false;
				Token token;
				Logger.Log($"Parsing {filePath}...");

				while (iterator.HasNext)
				{
					token = iterator.Current;
					if (scopes.Any())
					{
						var top = scopes.Peek();
						if (iterator.Index == top.Ends)
							EndScope();
					}
					switch (token.Type)
					{
						case Pattern.Comment:
							ignore = Regex.IsMatch(token.Value, IGNORE_COMAND);
							exit = Regex.IsMatch(token.Value, EXIT_COMMAND);
							if (ignore && iterator.HasNext)
								IDoc.Instance.Ignore(iterator.Next);
							break;
						default:
							if (HandleToken())
							{
								items.Clear();
								scopes.Clear();
								//cancellation.Cancel();
								return items;
							}
							break;
					}

					if (exit)
						break;


					iterator.Move();
				}

				while (scopes.Any())
					EndScope();
				IDoc.Instance?.IncrementStep();
				return items;
			}
			return Task.Factory.StartNew(parseAsync, cancellation.Token);
		}

		protected virtual bool BeginScope(IDocItem item)
		{
			if (item == null)
				return false;
			var type = item.ItemType;
			var name = item.Name;

			var pos = iterator.Current.Position;
			var next = iterator.Next.Value;
			if (!next.Equals("{"))
			{
				Logger.LogError($"{filePath} Missing '{{' or '=>' after {type} declaration: {type} = {name} line = {pos.Line}");
				return false;
			}
			var ends = iterator.IndexOfClose("{", "}");
			if (ends == -1)
			{
				Logger.LogError($"{filePath} Missing '}}' after {type} body: {type} = {name} line = {pos.Line}");
				return false;
			}

			var top = TopScope;
			if (top != null)
				top.Push(item);
			scopes.Push(new Scope
			{
				Starts = iterator.Index + 1,
				Ends = ends,
				Item = item
			});
			return true;
		}

		protected void EndScope()
		{
			if (scopes.Any())
			{
				var item = scopes.Pop()?.Item;
				if (!scopes.Any())
					items.Add(item);
			}
		}

		protected string ExtractComment()
		{
			var i = iterator.Index - 1;
			var builder = new StringBuilder();
			Token token;
			while (i >= 0)
			{
				token = iterator.ItemAt(i);
				if (!token.IsType(Pattern.Comment) || !token.Value.Trim().StartsWith("///", StringComparison.Ordinal))
					break;
				builder.Insert(0, CommentStartRegex.Replace(token.Value, string.Empty));
				i--;
			}
			return builder.ToString();
		}

		protected bool Tokenize(Lexer lexer)
		{
			try
			{
				iterator = new LexerIterator(input, lexer, filePath);
				return iterator.Any;
			}
			catch (Exception e)
			{
				Logger.LogError(e.StackTrace);
				return false;
			}
		}

		protected abstract bool InitParser();

		protected abstract bool HandleToken();

	}
}