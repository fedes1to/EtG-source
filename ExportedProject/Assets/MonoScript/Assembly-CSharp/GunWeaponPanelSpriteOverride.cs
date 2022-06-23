using UnityEngine;

public class GunWeaponPanelSpriteOverride : MonoBehaviour
{
	public IntVector2[] spritePairs;

	public int GetMatch(int input)
	{
		for (int i = 0; i < spritePairs.Length; i++)
		{
			if (spritePairs[i].x == input)
			{
				return spritePairs[i].y;
			}
		}
		return input;
	}
}
