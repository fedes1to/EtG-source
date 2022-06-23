using System.Collections.Generic;
using System.Text;

public class dfMarkupEntity
{
	private static List<dfMarkupEntity> HTML_ENTITIES = new List<dfMarkupEntity>
	{
		new dfMarkupEntity("&nbsp;", " "),
		new dfMarkupEntity("&quot;", "\""),
		new dfMarkupEntity("&amp;", "&"),
		new dfMarkupEntity("&lt;", "<"),
		new dfMarkupEntity("&gt;", ">"),
		new dfMarkupEntity("&#39;", "'"),
		new dfMarkupEntity("&trade;", "™"),
		new dfMarkupEntity("&copy;", "©"),
		new dfMarkupEntity("\u00a0", " ")
	};

	private static StringBuilder buffer = new StringBuilder();

	public string EntityName;

	public string EntityChar;

	public dfMarkupEntity(string entityName, string entityChar)
	{
		EntityName = entityName;
		EntityChar = entityChar;
	}

	public static string Replace(string text)
	{
		buffer.EnsureCapacity(text.Length);
		buffer.Length = 0;
		buffer.Append(text);
		for (int i = 0; i < HTML_ENTITIES.Count; i++)
		{
			dfMarkupEntity dfMarkupEntity2 = HTML_ENTITIES[i];
			buffer.Replace(dfMarkupEntity2.EntityName, dfMarkupEntity2.EntityChar);
		}
		return buffer.ToString();
	}
}
