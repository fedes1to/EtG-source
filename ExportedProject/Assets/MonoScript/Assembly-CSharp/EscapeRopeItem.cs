using Dungeonator;

public class EscapeRopeItem : PlayerItem
{
	public override bool CanBeUsed(PlayerController user)
	{
		if (!user || user.CurrentRoom == null)
		{
			return false;
		}
		if (user.CurrentRoom.area.PrototypeRoomCategory == PrototypeDungeonRoom.RoomCategory.BOSS && GameManager.Instance.Dungeon.tileIndices.tilesetId == GlobalDungeonData.ValidTilesets.HELLGEON)
		{
			return false;
		}
		if (user.CurrentRoom.CompletelyPreventLeaving)
		{
			return false;
		}
		if (GameManager.Instance.Dungeon.tileIndices.tilesetId == GlobalDungeonData.ValidTilesets.RATGEON)
		{
			return false;
		}
		if (user.CurrentRoom.IsWildWestEntrance)
		{
			return true;
		}
		return true;
	}

	protected override void DoEffect(PlayerController user)
	{
		if (user.CurrentRoom.CompletelyPreventLeaving)
		{
			return;
		}
		if (user.IsInMinecart)
		{
			user.currentMineCart.EvacuateSpecificPlayer(user, true);
		}
		AkSoundEngine.PostEvent("Play_OBJ_rope_escape_01", base.gameObject);
		if (!user.CurrentRoom.IsWildWestEntrance)
		{
			RoomHandler targetRoom = null;
			BaseShopController[] componentsInChildren = GameManager.Instance.Dungeon.data.Entrance.hierarchyParent.parent.GetComponentsInChildren<BaseShopController>(true);
			if (componentsInChildren != null && componentsInChildren.Length > 0)
			{
				targetRoom = GameManager.Instance.Dungeon.data.GetAbsoluteRoomFromPosition(componentsInChildren[0].transform.position.IntXY());
			}
			user.EscapeRoom(PlayerController.EscapeSealedRoomStyle.ESCAPE_SPIN, true, targetRoom);
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}
}
