using System.Collections;
using Dungeonator;
using UnityEngine;

public class ResizableCollider : DungeonPlaceableBehaviour, IPlaceConfigurable, IDwarfDrawable
{
	public bool IsHorizontal = true;

	[DwarfConfigurable]
	public float NumTiles = 3f;

	public tk2dSlicedSprite[] spriteSources;

	private OccupiedCells m_cells;

	public IntVector2 GetOverrideDwarfDimensions(PrototypePlacedObjectData objectData)
	{
		int num = (int)objectData.GetFieldValueByName("NumTiles");
		if (IsHorizontal)
		{
			return new IntVector2(num, 1);
		}
		return new IntVector2(1, num);
	}

	private IEnumerator FrameDelayedConfiguration()
	{
		yield return null;
		for (int i = 0; i < spriteSources.Length; i++)
		{
			if (IsHorizontal)
			{
				int num = Mathf.RoundToInt(spriteSources[i].dimensions.x % 16f);
				spriteSources[i].dimensions = spriteSources[i].dimensions.WithX(NumTiles * 16f + (float)num);
			}
			else
			{
				int num2 = Mathf.RoundToInt(spriteSources[i].dimensions.y % 16f);
				spriteSources[i].dimensions = spriteSources[i].dimensions.WithY(NumTiles * 16f + (float)num2);
			}
		}
		if (!base.specRigidbody)
		{
			yield break;
		}
		for (int j = 0; j < base.specRigidbody.PixelColliders.Count; j++)
		{
			if (IsHorizontal)
			{
				base.specRigidbody.PixelColliders[j].ManualWidth = (int)NumTiles * 16;
			}
			else
			{
				base.specRigidbody.PixelColliders[j].ManualHeight = (int)NumTiles * 16;
			}
			base.specRigidbody.PixelColliders[j].Regenerate(base.transform, false, false);
		}
		base.specRigidbody.Reinitialize();
		m_cells = new OccupiedCells(base.specRigidbody, GetAbsoluteParentRoom());
	}

	protected override void OnDestroy()
	{
		if (m_cells != null)
		{
			m_cells.Clear();
		}
		base.OnDestroy();
	}

	public void ConfigureOnPlacement(RoomHandler room)
	{
		IntVector2 intVector = base.transform.position.IntXY();
		for (int i = 0; (float)i < NumTiles; i++)
		{
			IntVector2 intVector2 = intVector + new IntVector2(0, i);
			if (IsHorizontal)
			{
				intVector2 = intVector + new IntVector2(i, 0);
			}
			if (GameManager.Instance.Dungeon.data.CheckInBounds(intVector2))
			{
				CellData cellData = GameManager.Instance.Dungeon.data[intVector2];
				if (cellData != null)
				{
					cellData.isOccupied = true;
				}
			}
		}
		StartCoroutine(FrameDelayedConfiguration());
	}
}
