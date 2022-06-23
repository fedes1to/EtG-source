using Dungeonator;
using UnityEngine;

public class EntranceController : DungeonPlaceableBehaviour, IPlaceConfigurable
{
	public int wallClearanceXStart;

	public int wallClearanceYStart;

	public int wallClearanceWidth = 4;

	public int wallClearanceHeight = 2;

	public Transform spawnTransform;

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
				cellData.cellVisualData.shouldIgnoreWallDrawing = true;
			}
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}
}
