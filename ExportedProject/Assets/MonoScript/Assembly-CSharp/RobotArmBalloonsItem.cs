using System;
using Dungeonator;
using UnityEngine;

public class RobotArmBalloonsItem : PickupObject
{
	public BalloonAttachmentDoer BalloonAttachPrefab;

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
				GameObject gameObject = (minimapIcon = (GameObject)BraveResources.Load("Global Prefabs/Minimap_RobotBalloon_Icon"));
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
				if (GameManager.Instance.AllPlayers[i].additionalItems[j] is RobotArmItem)
				{
					RobotArmQuestController.CombineBalloonsWithArm(this, GameManager.Instance.AllPlayers[i].additionalItems[j], GameManager.Instance.AllPlayers[i]);
					return true;
				}
			}
		}
		return false;
	}

	public void AttachBalloonToGameActor(GameActor target)
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(BalloonAttachPrefab.gameObject);
		BalloonAttachmentDoer component = gameObject.GetComponent<BalloonAttachmentDoer>();
		component.Initialize(target);
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
			base.specRigidbody.enabled = false;
			base.renderer.enabled = false;
			HandleEncounterable(player);
			AttachBalloonToGameActor(player);
			SpriteOutlineManager.RemoveOutlineFromSprite(base.sprite);
			DebrisObject component = GetComponent<DebrisObject>();
			if (component != null)
			{
				UnityEngine.Object.Destroy(component);
				UnityEngine.Object.Destroy(base.specRigidbody);
				player.AcquirePuzzleItem(this);
			}
			else
			{
				UnityEngine.Object.Instantiate(base.gameObject);
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
