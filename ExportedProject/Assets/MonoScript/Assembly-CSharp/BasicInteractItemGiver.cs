using Dungeonator;
using UnityEngine;

public class BasicInteractItemGiver : BraveBehaviour, IPlayerInteractable
{
	[PickupIdentifier]
	public int pickupIdToGive = -1;

	public GungeonFlags flagToSetOnAcquisition;

	public bool destroyThisOnAcquisition;

	private bool m_pickedUp;

	private RoomHandler m_room;

	private void Start()
	{
		m_room = GameManager.Instance.Dungeon.data.GetAbsoluteRoomFromPosition(base.transform.position.IntXY());
		m_room.RegisterInteractable(this);
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
	}

	public void OnExitRange(PlayerController interactor)
	{
		SpriteOutlineManager.RemoveOutlineFromSprite(base.sprite, true);
		SpriteOutlineManager.AddOutlineToSprite(base.sprite, Color.black);
	}

	public void Interact(PlayerController interactor)
	{
		if (!m_pickedUp)
		{
			if (flagToSetOnAcquisition != 0)
			{
				GameStatsManager.Instance.SetFlag(flagToSetOnAcquisition, true);
			}
			m_pickedUp = true;
			m_room.DeregisterInteractable(this);
			PickupObject byId = PickupObjectDatabase.GetById(pickupIdToGive);
			LootEngine.TryGivePrefabToPlayer(byId.gameObject, interactor, true);
			if (destroyThisOnAcquisition)
			{
				SpriteOutlineManager.RemoveOutlineFromSprite(base.sprite);
				Object.Destroy(base.gameObject);
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
