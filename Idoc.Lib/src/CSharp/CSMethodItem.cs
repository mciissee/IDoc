#pragma warning disable XS0001 // Find APIs marked as TODO in Mono

using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Idoc.Lib
{
	public class CSMethodItem : DocItem
	{
		public virtual string Access { get; set; }
		public virtual string Type { get; set; }
		public virtual string Modifiers { get; set; }
		public virtual string Constraints { get; set; }
		public virtual string[] Attributes { get; set; }

		public virtual CSParam[] Params { get; set; }

		public bool HasAttribute => Attributes.Any();
		public bool HasAccess => !string.IsNullOrEmpty(Access);
		public bool HasModifiers => !string.IsNullOrEmpty(Modifiers);
		public bool HasConstraints => !string.IsNullOrEmpty(Constraints);
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

		protected void Init(Token token, IDocItem scope, string comment)
		{
			Token = token;
			Scope = scope;
			Access = token.Groups("methodaccess");
			Modifiers = token.Groups("methodmodifier");
			Type = token.Groups("methodtype");
			Name = token.Groups("methodname");
			Constraints = token.Groups("methodconstraints");
			Attributes = token.Captures("attribute").Select(it => it.Value.Trim()).ToArray();
			Params = token.Captures("param").Select(it => new CSParam(it.Value)).ToArray();
			foreach (var it in Params)
				it.Parse(token.Lexer);
			Comment = new CommentItem(comment);
		}

		public static CSMethodItem FromToken(Token token, IDocItem scope, string comment)
		{
			var item = new CSMethodItem();
			item.Init(token, scope, comment);
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
					json[it.Name] = it.Signature;
				output[Signature][PARAMS] = json;
				return true;
			}
			return false;
		}

		public override string ToString()
		{
			var builder = new StringBuilder();
			if (HasAttribute)
				builder.Append($"{string.Join("\n", Attributes)}\n");

			if (HasAccess)
				builder.Append($"{Access} ");

			if (HasModifiers)
				builder.Append($"{Modifiers} ");

			builder.Append($"{Type} {Name}(");

			if (HasParam)
				builder.Append(string.Join(", ", Params.Select(it => it.Code).ToArray()));

			builder.Append($")");

			if (HasConstraints)
				builder.Append($"{Constraints}");
			return builder.ToString().Trim();
		}
	}
}