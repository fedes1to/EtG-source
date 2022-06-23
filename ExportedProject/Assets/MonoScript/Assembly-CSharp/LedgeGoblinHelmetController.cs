using Dungeonator;
using UnityEngine;

public class LedgeGoblinHelmetController : BraveBehaviour, IPlayerInteractable
{
	private bool m_pickedUp;

	private void Start()
	{
		SpriteOutlineManager.AddOutlineToSprite(base.sprite, Color.black);
	}

	public float GetDistanceToPoint(Vector2 point)
	{
		Bounds bounds = base.sprite.GetBounds();
		bounds.SetMinMax(bounds.min + base.transform.position, bounds.max + base.transform.position);
		float num = Mathf.Max(Mathf.Min(point.x, bounds.max.x), bounds.min.x);
		float num2 = Mathf.Max(Mathf.Min(point.y, bounds.max.y), bounds.min.y);
		return Mathf.Sqrt((point.x - num) * (point.x - num) + (point.y - num2) * (point.y - num2));
	}

	public void OnEnteredRange(PlayerController interactor)
	{
		SpriteOutlineManager.RemoveOutlineFromSprite(base.sprite, true);
		SpriteOutlineManager.AddOutlineToSprite(base.sprite, Color.white);
		GameStatsManager.Instance.SetFlag(GungeonFlags.LEDGEGOBLIN_ACTIVE_IN_FOYER, true);
	}

	public void OnExitRange(PlayerController interactor)
	{
		if ((bool)base.sprite)
		{
			SpriteOutlineManager.RemoveOutlineFromSprite(base.sprite, true);
			SpriteOutlineManager.AddOutlineToSprite(base.sprite, Color.black);
		}
	}

	public void Interact(PlayerController interactor)
	{
		if (!m_pickedUp)
		{
			m_pickedUp = true;
			SpriteOutlineManager.RemoveOutlineFromSprite(base.sprite);
			GameManager.BroadcastRoomTalkDoerFsmEvent("modeAnnoyed");
			bool flag = GameStatsManager.Instance.GetFlag(GungeonFlags.LEDGEGOBLIN_COMPLETED_FIRST_DUNGEON);
			bool flag2 = GameStatsManager.Instance.GetFlag(GungeonFlags.LEDGEGOBLIN_COMPLETED_SECOND_DUNGEON);
			if (!GameStatsManager.Instance.GetFlag(GungeonFlags.LEDGEGOBLIN_COMPLETED_THIRD_DUNGEON) && flag2 && flag)
			{
				GameStatsManager.Instance.SetFlag(GungeonFlags.LEDGEGOBLIN_TRIGGERED_THIRD_DUNGEON, true);
			}
			else if (!flag2 && flag)
			{
				GameStatsManager.Instance.SetFlag(GungeonFlags.LEDGEGOBLIN_TRIGGERED_SECOND_DUNGEON, true);
			}
			else if (!flag)
			{
				GameStatsManager.Instance.SetFlag(GungeonFlags.LEDGEGOBLIN_TRIGGERED_FIRST_DUNGEON, true);
			}
			RoomHandler.unassignedInteractableObjects.Remove(this);
			interactor.RemoveBrokenInteractable(this);
			if (GameStatsManager.Instance.GetFlag(GungeonFlags.LEDGEGOBLIN_COMPLETED_THIRD_DUNGEON))
			{
				base.spriteAnimator.Play("helmte_kick_chain");
				base.transform.position.GetAbsoluteRoom().DeregisterInteractable(this);
				RoomHandler.unassignedInteractableObjects.Remove(this);
			}
			else
			{
				base.spriteAnimator.PlayAndDestroyObject(string.Empty);
			}
		}
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

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}
}
