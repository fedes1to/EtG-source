using System;
using Dungeonator;
using UnityEngine;

public class KeyBulletPickup : PickupObject
{
	public int numberKeyBullets = 1;

	public bool IsRatKey;

	public string overrideBloopSpriteName = string.Empty;

	private bool m_hasBeenPickedUp;

	private SpeculativeRigidbody m_srb;

	public GameObject minimapIcon;

	private RoomHandler m_minimapIconRoom;

	private GameObject m_instanceMinimapIcon;

	private void Start()
	{
		m_srb = GetComponent<SpeculativeRigidbody>();
		SpeculativeRigidbody srb = m_srb;
		srb.OnTriggerCollision = (SpeculativeRigidbody.OnTriggerDelegate)Delegate.Combine(srb.OnTriggerCollision, new SpeculativeRigidbody.OnTriggerDelegate(OnPreCollision));
		if (minimapIcon != null && !m_hasBeenPickedUp)
		{
			m_minimapIconRoom = GameManager.Instance.Dungeon.data.GetAbsoluteRoomFromPosition(base.transform.position.IntXY(VectorConversions.Floor));
			m_instanceMinimapIcon = Minimap.Instance.RegisterRoomIcon(m_minimapIconRoom, minimapIcon);
		}
	}

	private void Update()
	{
		if (base.spriteAnimator != null && base.spriteAnimator.DefaultClip != null)
		{
			base.spriteAnimator.SetFrame(Mathf.FloorToInt(Time.time * base.spriteAnimator.DefaultClip.fps % (float)base.spriteAnimator.DefaultClip.frames.Length));
		}
		if (IsRatKey && !GameManager.Instance.IsLoadingLevel && !m_hasBeenPickedUp && (bool)this && !GameManager.Instance.IsAnyPlayerInRoom(base.transform.position.GetAbsoluteRoom()))
		{
			PlayerController bestActivePlayer = GameManager.Instance.BestActivePlayer;
			if ((bool)bestActivePlayer && !bestActivePlayer.IsGhost && bestActivePlayer.AcceptingAnyInput)
			{
				m_hasBeenPickedUp = true;
				Pickup(bestActivePlayer);
			}
		}
	}

	private void GetRidOfMinimapIcon()
	{
		if (m_instanceMinimapIcon != null)
		{
			Minimap.Instance.DeregisterRoomIcon(m_minimapIconRoom, m_instanceMinimapIcon);
			m_instanceMinimapIcon = null;
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (Minimap.HasInstance)
		{
			GetRidOfMinimapIcon();
		}
	}

	private void OnPreCollision(SpeculativeRigidbody otherRigidbody, SpeculativeRigidbody source, CollisionData collisionData)
	{
		if (!m_hasBeenPickedUp)
		{
			PlayerController component = otherRigidbody.GetComponent<PlayerController>();
			if (component != null)
			{
				m_hasBeenPickedUp = true;
				Pickup(component);
			}
		}
	}

	public override void Pickup(PlayerController player)
	{
		player.HasGottenKeyThisRun = true;
		HandleEncounterable(player);
		GetRidOfMinimapIcon();
		if ((bool)base.spriteAnimator)
		{
			base.spriteAnimator.StopAndResetFrame();
		}
		player.BloopItemAboveHead(base.sprite, overrideBloopSpriteName);
		player.carriedConsumables.KeyBullets += numberKeyBullets;
		if (IsRatKey)
		{
			player.carriedConsumables.ResourcefulRatKeys++;
		}
		AkSoundEngine.PostEvent("Play_OBJ_key_pickup_01", base.gameObject);
		UnityEngine.Object.Destroy(base.gameObject);
	}
}
