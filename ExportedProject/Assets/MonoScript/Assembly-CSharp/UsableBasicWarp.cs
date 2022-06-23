using System.Collections;
using Dungeonator;
using UnityEngine;

public class UsableBasicWarp : BraveBehaviour, IPlayerInteractable
{
	public bool IsRatTrapdoorLadder;

	public bool IsHelicopterLadder;

	private bool m_justWarped;

	private void Start()
	{
		GameManager.Instance.Dungeon.data.GetAbsoluteRoomFromPosition(base.transform.position.IntXY(VectorConversions.Floor)).RegisterInteractable(this);
	}

	public float GetDistanceToPoint(Vector2 point)
	{
		return Vector2.Distance(point, base.sprite.WorldBottomCenter);
	}

	public void OnEnteredRange(PlayerController interactor)
	{
		SpriteOutlineManager.AddOutlineToSprite(base.sprite, Color.white);
		if (IsHelicopterLadder)
		{
			m_justWarped = false;
		}
	}

	public void OnExitRange(PlayerController interactor)
	{
		SpriteOutlineManager.RemoveOutlineFromSprite(base.sprite, true);
	}

	public void Interact(PlayerController interactor)
	{
		if (!m_justWarped)
		{
			if (!IsRatTrapdoorLadder)
			{
				StartCoroutine(HandleWarpCooldown(interactor));
			}
			else
			{
				GameManager.Instance.Dungeon.StartCoroutine(HandleWarpCooldown(interactor));
			}
		}
	}

	private IEnumerator HandleWarpCooldown(PlayerController player)
	{
		m_justWarped = true;
		Pixelator.Instance.FadeToBlack(0.1f);
		yield return new WaitForSeconds(0.1f);
		player.SetInputOverride("arbitrary warp");
		if (IsRatTrapdoorLadder)
		{
			ResourcefulRatMinesHiddenTrapdoor resourcefulRatMinesHiddenTrapdoor = StaticReferenceManager.AllRatTrapdoors[0];
			resourcefulRatMinesHiddenTrapdoor.transform.position.GetAbsoluteRoom();
			Vector2 targetPoint = resourcefulRatMinesHiddenTrapdoor.transform.position.XY() + new Vector2(2f, 3.25f);
			player.WarpToPoint(targetPoint);
		}
		else if (IsHelicopterLadder)
		{
			RoomHandler roomHandler = null;
			foreach (RoomHandler room in GameManager.Instance.Dungeon.data.rooms)
			{
				if (room.area.PrototypeRoomCategory == PrototypeDungeonRoom.RoomCategory.BOSS && room.area.PrototypeRoomBossSubcategory != PrototypeDungeonRoom.RoomBossSubCategory.MINI_BOSS)
				{
					roomHandler = room;
					break;
				}
			}
			Vector2 targetPoint2 = roomHandler.area.basePosition.ToVector2() + new Vector2((float)roomHandler.area.dimensions.x / 2f, 8f);
			player.WarpToPoint(targetPoint2);
		}
		else
		{
			RoomHandler entrance = GameManager.Instance.Dungeon.data.Entrance;
			Vector2 targetPoint3 = entrance.GetCenterCell().ToVector2() + new Vector2(0f, -5f);
			player.WarpToPoint(targetPoint3);
		}
		if (GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER)
		{
			PlayerController otherPlayer = GameManager.Instance.GetOtherPlayer(player);
			if ((bool)otherPlayer && otherPlayer.healthHaver.IsAlive)
			{
				otherPlayer.ReuniteWithOtherPlayer(player);
			}
		}
		GameManager.Instance.MainCameraController.ForceToPlayerPosition(player);
		Pixelator.Instance.FadeToBlack(0.1f, true);
		player.ClearInputOverride("arbitrary warp");
		yield return new WaitForSeconds(1f);
		m_justWarped = false;
	}

	public string GetAnimationState(PlayerController interactor, out bool shouldBeFlipped)
	{
		shouldBeFlipped = false;
		return string.Empty;
	}

	public float GetOverrideMaxDistance()
	{
		return -1f;
	}
}
