using System;
using Dungeonator;
using UnityEngine;

[Serializable]
public class DebrisDirectionalAnimationInfo
{
	public string fallUp;

	public string fallRight;

	public string fallDown;

	public string fallLeft;

	public string GetAnimationForVector(Vector2 dir)
	{
		switch (DungeonData.GetCardinalFromVector2(dir))
		{
		case DungeonData.Direction.NORTH:
			return fallUp;
		case DungeonData.Direction.EAST:
			return fallRight;
		case DungeonData.Direction.SOUTH:
			return fallDown;
		case DungeonData.Direction.WEST:
			return fallLeft;
		default:
			return fallDown;
		}
	}
}
