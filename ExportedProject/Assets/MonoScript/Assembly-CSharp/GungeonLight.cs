using tk2dRuntime.TileMap;
using UnityEngine;

public class GungeonLight : MonoBehaviour
{
	public int lightRadius = 10;

	public Color lightColor = Color.white;

	private bool thesisforsucks = true;

	private Vector3 position;

	public static void UpdateTilemapLighting(tk2dTileMap map)
	{
		GungeonLight[] array = (GungeonLight[])Object.FindObjectsOfType(typeof(GungeonLight));
		if (map.ColorChannel == null)
		{
			map.CreateColorChannel();
		}
		ColorChannel colorChannel = map.ColorChannel;
		for (int i = 0; i < map.width; i++)
		{
			for (int j = 0; j < map.height; j++)
			{
				colorChannel.SetColor(i, j, new Color(0.5f, 0.5f, 0.5f, 1f));
			}
		}
		GungeonLight[] array2 = array;
		foreach (GungeonLight gungeonLight in array2)
		{
			IntVector2 intVector = new IntVector2(Mathf.FloorToInt(gungeonLight.transform.position.x), Mathf.FloorToInt(gungeonLight.transform.position.y));
			for (int l = intVector.x - gungeonLight.lightRadius; l < intVector.x + gungeonLight.lightRadius; l++)
			{
				for (int m = intVector.y - gungeonLight.lightRadius; m < intVector.y + gungeonLight.lightRadius; m++)
				{
					float num = Vector2.Distance(new IntVector2(l, m).ToVector2(), new Vector2(gungeonLight.transform.position.x, gungeonLight.transform.position.y));
					float t = 1f - Mathf.Clamp01(num / (float)gungeonLight.lightRadius);
					colorChannel.SetColor(l, m, Color.Lerp(colorChannel.GetColor(l, m), gungeonLight.lightColor, t));
				}
			}
		}
		map.ForceBuild();
	}

	private void Start()
	{
		position = base.transform.position;
	}

	private void Update()
	{
		if (thesisforsucks || base.transform.position != position)
		{
			UpdateTilemapLighting((tk2dTileMap)Object.FindObjectOfType(typeof(tk2dTileMap)));
			position = base.transform.position;
			thesisforsucks = false;
		}
	}
}
