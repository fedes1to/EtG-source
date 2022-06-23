using System;
using Dungeonator;
using UnityEngine.Serialization;

public class FloorTypeOverrideDoer : BraveBehaviour, IPlaceConfigurable
{
	public enum OverrideMode
	{
		Placeable = 10,
		Rigidbody = 20
	}

	public OverrideMode overrideMode = OverrideMode.Placeable;

	[ShowInInspectorIf("overrideMode", 0, true)]
	public int xStartOffset;

	[ShowInInspectorIf("overrideMode", 0, true)]
	public int yStartOffset;

	[ShowInInspectorIf("overrideMode", 0, true)]
	public int width = 1;

	[ShowInInspectorIf("overrideMode", 0, true)]
	public int height = 1;

	public bool overrideCellFloorType;

	[ShowInInspectorIf("overrideCellFloorType", true)]
	[FormerlySerializedAs("overrideType")]
	public CellVisualData.CellFloorType cellFloorType = CellVisualData.CellFloorType.Carpet;

	public bool overrideTileIndex;

	public GlobalDungeonData.ValidTilesets[] TilesetsToOverrideFloorTile;

	public int[] OverrideFloorTiles;

	public bool preventsOtherFloorDecoration = true;

	public bool allowWallDecorationTho;

	public void Start()
	{
		if (overrideMode == OverrideMode.Rigidbody)
		{
			DoFloorOverride(base.specRigidbody.UnitBottomLeft.ToIntVector2(), base.specRigidbody.UnitTopRight.ToIntVector2() - IntVector2.One);
		}
	}

	public void ConfigureOnPlacement(RoomHandler room)
	{
		IntVector2 intVector = base.transform.position.IntXY(VectorConversions.Floor);
		DoFloorOverride(intVector + new IntVector2(xStartOffset, yStartOffset), intVector + new IntVector2(xStartOffset + width - 1, yStartOffset + height - 1));
	}

	private void DoFloorOverride(IntVector2 lowerLeft, IntVector2 upperRight)
	{
		DungeonData data = GameManager.Instance.Dungeon.data;
		for (int i = lowerLeft.x; i <= upperRight.x; i++)
		{
			for (int j = lowerLeft.y; j <= upperRight.y; j++)
			{
				if (overrideCellFloorType)
				{
					data[i, j].cellVisualData.floorType = cellFloorType;
				}
				if (overrideTileIndex)
				{
					int num = Array.IndexOf(TilesetsToOverrideFloorTile, GameManager.Instance.Dungeon.tileIndices.tilesetId);
					if (num >= 0)
					{
						int customIndexOverride = OverrideFloorTiles[num];
						data[i, j].cellVisualData.UsesCustomIndexOverride01 = true;
						data[i, j].cellVisualData.CustomIndexOverride01 = customIndexOverride;
						data[i, j].cellVisualData.CustomIndexOverride01Layer = GlobalDungeonData.patternLayerIndex;
					}
				}
			}
		}
		if (!preventsOtherFloorDecoration)
		{
			return;
		}
		for (int k = lowerLeft.x - 1; k <= upperRight.x + 1; k++)
		{
			for (int l = lowerLeft.y - 1; l <= upperRight.y + 1; l++)
			{
				data[k, l].cellVisualData.floorTileOverridden = true;
				data[k, l].cellVisualData.preventFloorStamping = true;
				data[k, l].cellVisualData.containsObjectSpaceStamp = true;
				data[k, l].cellVisualData.containsWallSpaceStamp = !allowWallDecorationTho;
			}
		}
	}
}
