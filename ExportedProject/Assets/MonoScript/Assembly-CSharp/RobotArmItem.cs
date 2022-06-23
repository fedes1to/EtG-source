using System;
using System.Collections.Generic;
using Dungeonator;
using UnityEngine;

public class RobotArmItem : PickupObject
{
	private bool m_pickedUp;

	private GameObject minimapIcon;

	private RoomHandler m_minimapIconRoom;

	private GameObject m_instanceMinimapIcon;

	private void Start()
	{
		SpriteOutlineManager.AddOutlineToSprite(base.sprite, Color.black);
		SpeculativeRigidbody speculativeRigidbody = base.specRigidbody;
		speculativeRigidbody.OnTriggerCollision = (SpeculativeRigidbody.OnTriggerDelegate)Delegate.Combine(speculativeRigidbody.OnTriggerCollision, new SpeculativeRigidbody.OnTriggerDelegate(OnPreCollision));
		if (!m_pickedUp)
		{
			RegisterMinimapIcon();
		}
	}

	public void RegisterMinimapIcon()
	{
		if (!(base.transform.position.y < -300f))
		{
			if (minimapIcon == null)
			{
				GameObject gameObject = (minimapIcon = (GameObject)BraveResources.Load("Global Prefabs/Minimap_RobotArm_Icon"));
			}
			if (minimapIcon != null && !m_pickedUp)
			{
				m_minimapIconRoom = GameManager.Instance.Dungeon.data.GetAbsoluteRoomFromPosition(base.transform.position.IntXY(VectorConversions.Floor));
				m_instanceMinimapIcon = Minimap.Instance.RegisterRoomIcon(m_minimapIconRoom, minimapIcon);
			}
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
				AkSoundEngine.PostEvent("Play_OBJ_item_pickup_01", base.gameObject);
			}
		}
	}

	public bool CheckForCombination()
	{
		for (int i = 0; i < GameManager.Instance.AllPlayers.Length; i++)
		{
			for (int j = 0; j < GameManager.Instance.AllPlayers[i].additionalItems.Count; j++)
			{
				if (GameManager.Instance.AllPlayers[i].additionalItems[j] is RobotArmBalloonsItem)
				{
					RobotArmQuestController.CombineBalloonsWithArm(GameManager.Instance.AllPlayers[i].additionalItems[j], this, GameManager.Instance.AllPlayers[i]);
					return true;
				}
			}
		}
		return false;
	}

	public override void Pickup(PlayerController player)
	{
		if (m_pickedUp)
		{
			return;
		}
		m_pickedUp = true;
		GetRidOfMinimapIcon();
		if (!CheckForCombination())
		{
			if (!GameStatsManager.Instance.GetFlag(GungeonFlags.META_SHOP_EVER_SEEN_ROBOT_ARM))
			{
				GameManager.BroadcastRoomFsmEvent("armPickedUp", player.CurrentRoom);
				GameStatsManager.Instance.SetFlag(GungeonFlags.META_SHOP_EVER_SEEN_ROBOT_ARM, true);
				List<PickupObject> list = new List<PickupObject>();
				list.Add(PickupObjectDatabase.GetById(GlobalItemIds.RobotBalloons));
				GameManager.Instance.Dungeon.data.DistributeComplexSecretPuzzleItems(list, null, true);
			}
			base.specRigidbody.enabled = false;
			base.renderer.enabled = false;
			HandleEncounterable(player);
			SpriteOutlineManager.RemoveOutlineFromSprite(base.sprite);
			DebrisObject component = GetComponent<DebrisObject>();
			if (component != null)
			{
				UnityEngine.Object.Destroy(component);
				UnityEngine.Object.Destroy(base.specRigidbody);
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
	}

	protected override void OnDestroy()
	{
		if (Minimap.HasInstance)
		{
			GetRidOfMinimapIcon();
		}
		base.OnDestroy();
	}
}
