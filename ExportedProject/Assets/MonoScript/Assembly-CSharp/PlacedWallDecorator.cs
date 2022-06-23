using Dungeonator;
using UnityEngine;

public class PlacedWallDecorator : MonoBehaviour, IPlaceConfigurable
{
	public int wallClearanceXStart;

	public int wallClearanceYStart = 1;

	public int wallClearanceWidth = 1;

	public int wallClearanceHeight = 2;

	public bool ignoreWallDrawing;

	public bool ignoresBorders;

	public void ConfigureOnPlacement(RoomHandler room)
	{
		IntVector2 intVector = base.transform.position.IntXY(VectorConversions.Floor);
		for (int i = wallClearanceXStart; i < wallClearanceWidth + wallClearanceXStart; i++)
		{
			for (int j = wallClearanceYStart; j < wallClearanceHeight + wallClearanceYStart; j++)
			{
				IntVector2 key = intVector + new IntVector2(i, j);
				CellData cellData = GameManager.Instance.Dungeon.data[key];
				cellData.cellVisualData.containsObjectSpaceStamp = true;
				cellData.cellVisualData.containsWallSpaceStamp = true;
				cellData.cellVisualData.shouldIgnoreWallDrawing = ignoreWallDrawing;
				cellData.cellVisualData.shouldIgnoreBorders = ignoresBorders;
			}
		}
	}
}
