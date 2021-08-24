#pragma warning disable XS0001 // Find APIs marked as TODO in Mono

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Idoc.Lib
{

	/// <summary>
	/// Representation of a xml comment
	/// </summary>
	public class CommentItem
	{

		public class Tag
		{
			public string Name { get; set; }
			public string Body { get; set; }
			public string AttributeName { get; set; }
			public string AttributeValue { get; set; }
			public override string ToString() => $"{Name}: {Body}";
		}

		public static Regex ValidatorRegex = new Regex("(<([a-zA-Z]+)\\b[^>]*>)|(</([a-zA-Z]+) *>)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
		public static Regex TagRegex = new Regex(@"(<\s*(?<tag>\w+)(?:\s+(?<attributename>\w+)\s*=(?:""|')(?<attributevalue>[^>]*)(?:""|'))?\s*>(?<body>(?:(?:(?<!<\s*\k<tag>).)|\n)+)</\s*\k<tag>\s*>)", RegexOptions.Compiled);

		public string Comment { get; set; }

		public List<Tag> Tags { get; private set; } = new List<Tag>();

		public CommentItem(string comment)
		{
			this.Comment = comment;
		}

		public void Build(IDocItem relatedItem, Dictionary<string, object> output)
		{
			if (string.IsNullOrEmpty(Comment))
				return;

			IsValid(relatedItem);
			var matches = TagRegex.Matches(Comment);
			foreach (Match match in matches)
			{
				Tags.Add(new Tag
				{
					Name = match.Groups["tag"].Value.Trim(),
					AttributeName = match.Groups["attributename"].Value.Trim(),
					AttributeValue = match.Groups["attributevalue"].Value.Trim(),
					Body = match.Groups["body"].Value.Trim(),
				});
			}

			var comment = new Dictionary<string, string>();
			Tag tag = GetTag("summary");
			if (tag != null)
				comment["summary"] = tag.Body;

			tag = GetTag("example");
			if (tag != null)
				comment["example"] = tag.Body;

			tag = GetTag("remarks");
			if (tag != null)
				comment["remarks"] = tag.Body;

			tag = GetTag("returns");
			if (tag != null)
				comment["returns"] = tag.Body;

			var tags = Tags.Where(it => it.Name == "exception");
			if (tags.Any())
			{
				foreach (var it in tags)
					comment[$"exception-{it.AttributeValue}"] = it.Body;
			}

			tags = Tags.Where(it => it.Name == "typeparam");
			if (tags.Any())
			{
				foreach (var it in tags)
					comment[$"typeparam-{it.AttributeValue}"] = it.Body;
			}

			tags = Tags.Where(it => it.Name == "param");
			if (tags.Any())
			{
				foreach (var it in tags)
					comment[$"param-{it.AttributeValue}"] = it.Body;
			}

			tags = Tags.Where(it => it.Name == "seealso");
			if (tags.Any())
			{
				foreach (var it in tags)
					comment[$"seealso-{it.AttributeValue}"] = it.Body;
			}

			if (comment.Any())
				output["comments"] = comment;
		}

		public Tag GetTag(string name) => Tags.FirstOrDefault(it => it.Name == name);

		private bool IsValid(IDocItem owner)
		{
			try
			{
				var stack = new Stack<string[]>();
				var result = new StringBuilder();
				bool valid = true;
				var matches = ValidatorRegex.Matches(Comment);
				foreach (Match match in matches)
				{
					var element = match.Value;
					var beginTag = match.Groups[2].Value;
					var endTag = match.Groups[4].Value;
					if (beginTag == string.Empty && stack.Any())
					{
						var previousTag = stack.Peek()[0];
						if (previousTag == endTag)
						{
							stack.Pop();
						}
						else
						{
							valid = false;
							break;
						}
					}
					else if (!element.EndsWith("/>", StringComparison.Ordinal))
					{
						var message = $"{owner.Signature}: The xml tag {element} at the index {match.Index} is not valid";
						stack.Push(new string[] { beginTag, message });
					}
				}

				if (stack.Count > 0)
				{
					valid = false;
					Logger.LogWarning(stack.Peek()[1]);
				}
				return valid;
			}
			catch (Exception e)
			{
				Logger.LogError(e.Message);
				Logger.LogError(e.StackTrace);
			}
			return false;
		}

	}
}
