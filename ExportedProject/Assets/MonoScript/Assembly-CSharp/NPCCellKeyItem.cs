using System;
using Dungeonator;
using UnityEngine;

public class NPCCellKeyItem : PickupObject
{
	private static GameObject m_defaultIcon;

	private bool m_pickedUp;

	private GameObject minimapIcon;

	private GameObject m_instanceMinimapIcon;

	private RoomHandler m_minimapIconRoom;

	[NonSerialized]
	public bool IsBeingDestroyed;

	private bool m_forceExtant;

	private void Start()
	{
		SpeculativeRigidbody speculativeRigidbody = base.specRigidbody;
		speculativeRigidbody.OnTriggerCollision = (SpeculativeRigidbody.OnTriggerDelegate)Delegate.Combine(speculativeRigidbody.OnTriggerCollision, new SpeculativeRigidbody.OnTriggerDelegate(OnPreCollision));
		if (!m_pickedUp)
		{
			RegisterMinimapIcon();
		}
		SpriteOutlineManager.AddOutlineToSprite(base.sprite, Color.black);
	}

	private void Update()
	{
		if (!m_pickedUp && (bool)this && !GameManager.Instance.IsAnyPlayerInRoom(base.transform.position.GetAbsoluteRoom()))
		{
			PlayerController bestActivePlayer = GameManager.Instance.BestActivePlayer;
			if ((bool)bestActivePlayer && !bestActivePlayer.IsGhost && bestActivePlayer.AcceptingAnyInput)
			{
				Pickup(bestActivePlayer);
			}
		}
	}

	public void RegisterMinimapIcon()
	{
		if (base.transform.position.y < -300f)
		{
			return;
		}
		if (minimapIcon == null)
		{
			if (m_defaultIcon == null)
			{
				m_defaultIcon = (GameObject)BraveResources.Load("Global Prefabs/Minimap_CellKey_Icon");
			}
			minimapIcon = m_defaultIcon;
		}
		if (minimapIcon != null && !m_pickedUp)
		{
			m_minimapIconRoom = GameManager.Instance.Dungeon.data.GetAbsoluteRoomFromPosition(base.transform.position.IntXY(VectorConversions.Floor));
			m_instanceMinimapIcon = Minimap.Instance.RegisterRoomIcon(m_minimapIconRoom, minimapIcon);
		}
	}

	public void GetRidOfMinimapIcon()
	{
		if (m_instanceMinimapIcon != null)
		{
			Minimap.Instance.DeregisterRoomIcon(m_minimapIconRoom, m_instanceMinimapIcon);
			m_instanceMinimapIcon = null;
		}
	}

	private void OnPreCollision(SpeculativeRigidbody otherRigidbody, SpeculativeRigidbody source, CollisionData collisionData)
	{
		if (!m_pickedUp)
		{
			PlayerController component = otherRigidbody.GetComponent<PlayerController>();
			if (component != null)
			{
				Pickup(component);
				AkSoundEngine.PostEvent("Play_OBJ_goldkey_pickup_01", base.gameObject);
			}
		}
	}

	public void DropLogic()
	{
		m_forceExtant = true;
		m_pickedUp = false;
	}

	public override void Pickup(PlayerController player)
	{
		if (IsBeingDestroyed || m_pickedUp)
		{
			return;
		}
		m_pickedUp = true;
		SpriteOutlineManager.RemoveOutlineFromSprite(base.sprite);
		GetRidOfMinimapIcon();
		if ((bool)base.specRigidbody)
		{
			base.specRigidbody.enabled = false;
		}
		if ((bool)base.renderer)
		{
			base.renderer.enabled = false;
		}
		HandleEncounterable(player);
		DebrisObject component = GetComponent<DebrisObject>();
		if (component != null || m_forceExtant)
		{
			if ((bool)component)
			{
				UnityEngine.Object.Destroy(component);
			}
			if ((bool)base.specRigidbody)
			{
				UnityEngine.Object.Destroy(base.specRigidbody);
			}
			player.BloopItemAboveHead(base.sprite, string.Empty);
			player.AcquirePuzzleItem(this);
		}
		else
		{
			UnityEngine.Object.Instantiate(base.gameObject);
			player.BloopItemAboveHead(base.sprite, string.Empty);
			player.AcquirePuzzleItem(this);
		}
		GameUIRoot.Instance.UpdatePlayerConsumables(player.carriedConsumables);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		GetRidOfMinimapIcon();
	}
}
