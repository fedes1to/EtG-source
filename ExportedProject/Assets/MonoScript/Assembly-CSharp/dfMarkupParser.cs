using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;

public class dfMarkupParser
{
	private static Regex TAG_PATTERN;

	private static Regex ATTR_PATTERN;

	private static Regex STYLE_PATTERN;

	private static Dictionary<string, Type> tagTypes;

	private static dfMarkupParser parserInstance;

	private dfRichTextLabel owner;

	static dfMarkupParser()
	{
		TAG_PATTERN = null;
		ATTR_PATTERN = null;
		STYLE_PATTERN = null;
		tagTypes = null;
		parserInstance = new dfMarkupParser();
		RegexOptions options = RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture | RegexOptions.CultureInvariant;
		TAG_PATTERN = new Regex("(\\<\\/?)(?<tag>[a-zA-Z0-9$_]+)(\\s(?<attr>.+?))?([\\/]*\\>)", options);
		ATTR_PATTERN = new Regex("(?<key>[a-zA-Z0-9$_]+)=(?<value>(\"((\\\\\")|\\\\\\\\|[^\"\\n])*\")|('((\\\\')|\\\\\\\\|[^'\\n])*')|\\d+|\\w+)", options);
		STYLE_PATTERN = new Regex("(?<key>[a-zA-Z0-9\\-]+)(\\s*\\:\\s*)(?<value>[^;]+)", options);
	}

	public static dfList<dfMarkupElement> Parse(dfRichTextLabel owner, string source)
	{
		try
		{
			parserInstance.owner = owner;
			return parserInstance.parseMarkup(source);
		}
		finally
		{
		}
	}

	private dfList<dfMarkupElement> parseMarkup(string source)
	{
		Queue<dfMarkupElement> queue = new Queue<dfMarkupElement>();
		MatchCollection matchCollection = TAG_PATTERN.Matches(source);
		int num = 0;
		for (int i = 0; i < matchCollection.Count; i++)
		{
			Match match = matchCollection[i];
			if (match.Index > num)
			{
				string text = source.Substring(num, match.Index - num);
				dfMarkupString item = new dfMarkupString(text);
				queue.Enqueue(item);
			}
			num = match.Index + match.Length;
			queue.Enqueue(parseTag(match));
		}
		if (num < source.Length)
		{
			string text2 = source.Substring(num);
			dfMarkupString item2 = new dfMarkupString(text2);
			queue.Enqueue(item2);
		}
		return processTokens(queue);
	}

	private dfList<dfMarkupElement> processTokens(Queue<dfMarkupElement> tokens)
	{
		dfList<dfMarkupElement> dfList2 = dfList<dfMarkupElement>.Obtain();
		while (tokens.Count > 0)
		{
			dfList2.Add(parseElement(tokens));
		}
		for (int i = 0; i < dfList2.Count; i++)
		{
			if (dfList2[i] is dfMarkupTag)
			{
				((dfMarkupTag)dfList2[i]).Owner = owner;
			}
		}
		return dfList2;
	}

	private dfMarkupElement parseElement(Queue<dfMarkupElement> tokens)
	{
		dfMarkupElement dfMarkupElement2 = tokens.Dequeue();
		if (dfMarkupElement2 is dfMarkupString)
		{
			return ((dfMarkupString)dfMarkupElement2).SplitWords();
		}
		dfMarkupTag dfMarkupTag2 = (dfMarkupTag)dfMarkupElement2;
		if (dfMarkupTag2.IsClosedTag || dfMarkupTag2.IsEndTag)
		{
			return refineTag(dfMarkupTag2);
		}
		while (tokens.Count > 0)
		{
			dfMarkupElement dfMarkupElement3 = parseElement(tokens);
			if (dfMarkupElement3 is dfMarkupTag)
			{
				dfMarkupTag dfMarkupTag3 = (dfMarkupTag)dfMarkupElement3;
				if (dfMarkupTag3.IsEndTag)
				{
					if (dfMarkupTag3.TagName == dfMarkupTag2.TagName)
					{
						break;
					}
					return refineTag(dfMarkupTag2);
				}
			}
			dfMarkupTag2.AddChildNode(dfMarkupElement3);
		}
		return refineTag(dfMarkupTag2);
	}

	private dfMarkupTag refineTag(dfMarkupTag original)
	{
		if (original.IsEndTag)
		{
			return original;
		}
		if (tagTypes == null)
		{
			tagTypes = new Dictionary<string, Type>();
			Type[] assemblyTypes = getAssemblyTypes();
			foreach (Type type in assemblyTypes)
			{
				if (!typeof(dfMarkupTag).IsAssignableFrom(type))
				{
					continue;
				}
				object[] customAttributes = type.GetCustomAttributes(typeof(dfMarkupTagInfoAttribute), true);
				if (customAttributes != null && customAttributes.Length != 0)
				{
					for (int j = 0; j < customAttributes.Length; j++)
					{
						string tagName = ((dfMarkupTagInfoAttribute)customAttributes[j]).TagName;
						tagTypes[tagName] = type;
					}
				}
			}
		}
		if (tagTypes.ContainsKey(original.TagName))
		{
			Type type2 = tagTypes[original.TagName];
			return (dfMarkupTag)Activator.CreateInstance(type2, original);
		}
		return original;
	}

	private Type[] getAssemblyTypes()
	{
		return Assembly.GetExecutingAssembly().GetExportedTypes();
	}

	private dfMarkupElement parseTag(Match tag)
	{
		string text = tag.Groups["tag"].Value.ToLowerInvariant();
		if (tag.Value.StartsWith("</"))
		{
			dfMarkupTag dfMarkupTag2 = new dfMarkupTag(text);
			dfMarkupTag2.IsEndTag = true;
			return dfMarkupTag2;
		}
		dfMarkupTag dfMarkupTag3 = new dfMarkupTag(text);
		string value = tag.Groups["attr"].Value;
		MatchCollection matchCollection = ATTR_PATTERN.Matches(value);
		for (int i = 0; i < matchCollection.Count; i++)
		{
			Match match = matchCollection[i];
			string value2 = match.Groups["key"].Value;
			string text2 = dfMarkupEntity.Replace(match.Groups["value"].Value);
			if (text2.StartsWith("\""))
			{
				text2 = text2.Trim('"');
			}
			else if (text2.StartsWith("'"))
			{
				text2 = text2.Trim('\'');
			}
			if (!string.IsNullOrEmpty(text2))
			{
				if (value2 == "style")
				{
					parseStyleAttribute(dfMarkupTag3, text2);
				}
				else
				{
					dfMarkupTag3.Attributes.Add(new dfMarkupAttribute(value2, text2));
				}
			}
		}
		if (tag.Value.EndsWith("/>") || text == "br" || text == "img")
		{
			dfMarkupTag3.IsClosedTag = true;
		}
		return dfMarkupTag3;
	}

	private void parseStyleAttribute(dfMarkupTag element, string text)
	{
		MatchCollection matchCollection = STYLE_PATTERN.Matches(text);
		for (int i = 0; i < matchCollection.Count; i++)
		{
			Match match = matchCollection[i];
			string name = match.Groups["key"].Value.ToLowerInvariant();
			string value = match.Groups["value"].Value;
			element.Attributes.Add(new dfMarkupAttribute(name, value));
		}
	}
}
