#pragma warning disable XS0001 // Find APIs marked as TODO in Mono

using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Idoc.Lib
{
	public class CMethodItem : DocItem
	{
		public virtual string Access { get; set; }
		public virtual string Type { get; set; }
		public virtual string Modifiers { get; set; }
		public virtual CParam[] Params { get; set; }

		public bool HasAccess => !string.IsNullOrEmpty(Access);
		public bool HasModifiers => !string.IsNullOrEmpty(Modifiers);
		public bool HasParam => Params.Any();

		public override ItemTypes ItemType => ItemTypes.Method;

		public override string Signature
		{
			get
			{
				var builder = new StringBuilder();
				builder.Append($"{Scope.Signature}.{Name}(");
				builder.Append(string.Join(", ", Params.Select(it => it.Signature).ToArray()));
				builder.Append(")");
				return builder.ToString();
			}
		}

		public static CMethodItem FromToken(Token token, IDocItem scope, string comment)
		{
			var item = new CMethodItem
			{
				Token = token,
				Scope = scope,
				Access = token.Groups("methodaccess"),
				Modifiers = token.Groups("methodmodifier"),
				Type = token.Groups("methodtype"),
				Name = token.Groups("methodname"),
				Params = token.Captures("param").Select(it => new CParam(it.Value)).ToArray(),
				Comment = new CommentItem(comment)
			};

			foreach (var it in item.Params)
				it.Parse(token.Lexer);
			return item;
		}

		public override void Combine(IDocItem item)
		{

		}

		public override bool Build(Dictionary<string, Dictionary<string, object>> output)
		{
			if (base.Build(output) && HasParam)
			{
				var json = new Dictionary<string, object>();
				foreach (var it in Params)
					json[it.Name] = it.Type;
				output[Signature][PARAMS] = json;
				return true;
			}
			return false;
		}

		public override string ToString()
		{
			var builder = new StringBuilder();
			if (HasAccess)
				builder.Append($"{Access} ");
			if (HasModifiers)
				builder.Append($"{Modifiers} ");
			builder.Append($"{Type} {Name}(");
			if (HasParam)
				builder.Append(string.Join(", ", Params.Select(it => it.Code).ToArray()));
			builder.Append($")");
			return builder.ToString();
		}
	}
}