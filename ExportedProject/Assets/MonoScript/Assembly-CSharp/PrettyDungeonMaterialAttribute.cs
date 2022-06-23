using UnityEngine;

public class PrettyDungeonMaterialAttribute : PropertyAttribute
{
	public string tilesetProperty;

	public PrettyDungeonMaterialAttribute(string tilesetPropertyName)
	{
		tilesetProperty = tilesetPropertyName;
	}
}
