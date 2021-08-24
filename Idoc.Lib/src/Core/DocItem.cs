#pragma warning disable XS0001 // Find APIs marked as TODO in Mono

using System;
using System.Collections.Generic;
using System.Linq;

namespace Idoc.Lib
{
	public abstract class DocItem : IDocItem
	{
		protected const string PARAMS = "params";
		protected const string SIGNATURE = "signature";
		protected const string CHILD_COUNT = "childcount";
		protected const string NAME = "name";
		protected const string TOKEN = "token";
		protected const string SCOPE = "scope";
		protected const string TYPE = "type";

		#region Properties

		public static IList<IDocItem> Three { get; private set; } = new List<IDocItem>();
		public string Name { get; set; }
		public Token Token { get; set; }
		public virtual string Signature => Scope == null ? Name : $"{Scope.Signature}.{Name}";
		public IDocItem Scope { get; set; }
		public IList<IDocItem> Childs { get; set; } = new List<IDocItem>();
		public abstract ItemTypes ItemType { get; }
		public bool IsCommented => !string.IsNullOrEmpty(Comment?.Comment);
		public CommentItem Comment { get; set; }
		protected virtual string FullSignature => ToString();
		#endregion

		public IDocItem FindChild(string refTitle)
		{
			return Childs.FirstOrDefault(child => child.Signature.Equals(refTitle));
		}

		public IDocItem FindChild(Func<IDocItem, bool> predicate)
		{
			return Childs.FirstOrDefault(predicate);
		}

		public virtual void Push(IDocItem item)
		{
			var original = FindChild(item.Signature);
			if (original != null)
			{
				original.Combine(item);
			}
			else
			{
				Childs.Add(item);
			}
		}

		public virtual void Combine(IDocItem item)
		{
			if (item.IsCommented && !IsCommented)
				Comment = new CommentItem(item.Comment.Comment);

			foreach (var subIt in item.Childs)
				Push(subIt);
			(item as DocItem).Childs = this.Childs;
		}

		public virtual bool Build(Dictionary<string, Dictionary<string, object>> output)
		{
			if (IDoc.Instance.ShouldIgnore(Token))
				return false;

			var signature = Signature;
			Logger.Log($"Processing {signature} from the file {Token?.FilePath}...");

			var json = new Dictionary<string, object>
			{
				[SIGNATURE] = signature,
				[CHILD_COUNT] = Childs.Count,
				[TYPE] = ItemType.ToString(),
				[NAME] = Name,
				[TOKEN] = FullSignature,
				[SCOPE] = Scope?.Signature ?? string.Empty,
			};

			if (IsExtractable())
			{
				output[signature] = json;
				Comment?.Build(this, json);
				foreach (var child in Childs)
					child.Build(output);

				return true;
			}
			else
			{
				Logger.Log($"{signature} is not extractable with the current setting");
				return false;
			}
		}

		protected bool IsExtractable()
		{
			switch (ItemType)
			{
				case ItemTypes.Namespace:
				case ItemTypes.Header:
				case ItemTypes.Package:
					return true;
			}

			if (!IsCommented && !Setting.ExtractUndocumentedMembers)
				return false;

			if (ItemType == ItemTypes.EnumValue)
				return Setting.ExtractPublic;

			var syntax = Token.Value;
			switch (Setting.Language)
			{
				case Language.CS:
				case Language.Java:
					return syntax.Contains("public ") && Setting.ExtractPublic
								 || syntax.Contains("protected ") && Setting.ExtractProtected
								 || syntax.Contains("internal ") && Setting.ExtractInternal
								 || Setting.ExtractPrivate;
				default:
					return true;
			}
		}
	}
}
