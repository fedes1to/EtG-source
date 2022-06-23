using Dungeonator;
using UnityEngine;

public class DungeonPlaceableBehaviour : BraveBehaviour, IHasDwarfConfigurables
{
	public enum PlaceableDifficulty
	{
		BASE,
		HARD,
		HARDER,
		HARDEST
	}

	[SerializeField]
	public int placeableWidth = 1;

	[SerializeField]
	public int placeableHeight = 1;

	[SerializeField]
	public PlaceableDifficulty difficulty;

	[SerializeField]
	public bool isPassable = true;

	public IntVector2 PlacedPosition { get; set; }

	public virtual GameObject InstantiateObjectDirectional(RoomHandler targetRoom, IntVector2 location, DungeonData.Direction direction)
	{
		BraveUtility.Log("Calling InstantiateDirectional on a DungeonPlaceableBehaviour that hasn't implemented it.", Color.yellow, BraveUtility.LogVerbosity.IMPORTANT);
		return DungeonPlaceableUtility.InstantiateDungeonPlaceable(base.gameObject, targetRoom, location, false);
	}

	public virtual GameObject InstantiateObject(RoomHandler targetRoom, IntVector2 location, bool deferConfiguration = false)
	{
		return DungeonPlaceableUtility.InstantiateDungeonPlaceable(base.gameObject, targetRoom, location, deferConfiguration);
	}

	public virtual GameObject InstantiateObjectOnlyActors(RoomHandler targetRoom, IntVector2 location, bool deferConfiguration = false)
	{
		return DungeonPlaceableUtility.InstantiateDungeonPlaceableOnlyActors(base.gameObject, targetRoom, location, deferConfiguration);
	}

	public virtual int GetMinimumDifficulty()
	{
		return 0;
	}

	public virtual int GetMaximumDifficulty()
	{
		return 0;
	}

	public virtual int GetWidth()
	{
		return placeableWidth;
	}

	public virtual int GetHeight()
	{
		return placeableHeight;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}

	public RoomHandler GetAbsoluteParentRoom()
	{
		return GameManager.Instance.Dungeon.data.GetAbsoluteRoomFromPosition(base.transform.position.IntXY());
	}

	public void SetAreaPassable()
	{
		for (int i = PlacedPosition.x; i < PlacedPosition.x + placeableWidth; i++)
		{
			for (int j = PlacedPosition.y; j < PlacedPosition.y + placeableHeight; j++)
			{
				GameManager.Instance.Dungeon.data[i, j].isOccupied = false;
			}
		}
	}
}
