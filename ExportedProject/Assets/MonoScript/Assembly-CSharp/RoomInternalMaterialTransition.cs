using System;

[Serializable]
public class RoomInternalMaterialTransition
{
	public TileIndexGrid transitionGrid;

	public int materialTransition;

	public float proceduralThreshold = 0.5f;
}
