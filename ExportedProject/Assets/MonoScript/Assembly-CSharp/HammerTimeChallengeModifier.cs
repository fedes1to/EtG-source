using Dungeonator;
using UnityEngine;

public class HammerTimeChallengeModifier : ChallengeModifier
{
	public DungeonPlaceable HammerPlaceable;

	public float MinTimeBetweenAttacks = 3f;

	public float MaxTimeBetweenAttacks = 3f;

	public override bool IsValid(RoomHandler room)
	{
		GlobalDungeonData.ValidTilesets tilesetId = GameManager.Instance.Dungeon.tileIndices.tilesetId;
		return tilesetId == GlobalDungeonData.ValidTilesets.CASTLEGEON || tilesetId == GlobalDungeonData.ValidTilesets.GUNGEON || tilesetId == GlobalDungeonData.ValidTilesets.MINEGEON;
	}

	private void Start()
	{
		RoomHandler currentRoom = GameManager.Instance.PrimaryPlayer.CurrentRoom;
		GameObject gameObject = HammerPlaceable.InstantiateObject(currentRoom, currentRoom.Epicenter);
		if ((bool)gameObject)
		{
			ForgeHammerController component = gameObject.GetComponent<ForgeHammerController>();
			if ((bool)component)
			{
				component.MinTimeBetweenAttacks = MinTimeBetweenAttacks;
				component.MaxTimeBetweenAttacks = MaxTimeBetweenAttacks;
			}
		}
	}
}
