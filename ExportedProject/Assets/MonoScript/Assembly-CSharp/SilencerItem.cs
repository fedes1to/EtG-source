using System;
using Dungeonator;
using UnityEngine;

public class SilencerItem : PlayerItem
{
	public bool destroysEnemyBullets = true;

	public bool destroysPlayerBullets;

	public float silencerRadius = 25f;

	public float silencerSpeed = 50f;

	public float additionalTimeAtMaxRadius = 1f;

	public float distortionIntensity = 0.1f;

	public float distortionRadius = 0.1f;

	public float pushForce = 12f;

	public float pushRadius = 10f;

	public float knockbackForce = 12f;

	public float knockbackRadius = 7f;

	public GameObject silencerVFXPrefab;

	protected override void Start()
	{
		SpeculativeRigidbody speculativeRigidbody = base.specRigidbody;
		speculativeRigidbody.OnEnterTrigger = (SpeculativeRigidbody.OnTriggerDelegate)Delegate.Combine(speculativeRigidbody.OnEnterTrigger, new SpeculativeRigidbody.OnTriggerDelegate(TriggerWasEntered));
		SpeculativeRigidbody speculativeRigidbody2 = base.specRigidbody;
		speculativeRigidbody2.OnTriggerCollision = (SpeculativeRigidbody.OnTriggerDelegate)Delegate.Combine(speculativeRigidbody2.OnTriggerCollision, new SpeculativeRigidbody.OnTriggerDelegate(OnTrigger));
		base.Start();
	}

	private void OnTrigger(SpeculativeRigidbody otherRigidbody, SpeculativeRigidbody sourceSpecRigidbody, CollisionData collisionData)
	{
		if (!m_pickedUp)
		{
			PlayerController component = otherRigidbody.GetComponent<PlayerController>();
			if (component != null)
			{
				Pickup(component);
			}
		}
	}

	private void TriggerWasEntered(SpeculativeRigidbody otherRigidbody, SpeculativeRigidbody selfRigidbody, CollisionData collisionData)
	{
		if (!m_pickedUp)
		{
			PlayerController component = otherRigidbody.GetComponent<PlayerController>();
			if (component != null)
			{
				Pickup(component);
			}
		}
	}

	public override void Pickup(PlayerController player)
	{
		if (!m_pickedUp)
		{
			if (GameManager.Instance.InTutorial)
			{
				GameManager.BroadcastRoomTalkDoerFsmEvent("playerAcquiredPlayerItem");
			}
			m_pickedUp = true;
			if (!GameManager.Instance.InTutorial && GameStatsManager.Instance.QueryEncounterable(base.encounterTrackable) < 3)
			{
				HandleEncounterable(player);
			}
			if (RoomHandler.unassignedInteractableObjects.Contains(this))
			{
				RoomHandler.unassignedInteractableObjects.Remove(this);
			}
			GetRidOfMinimapIcon();
			AkSoundEngine.PostEvent("Play_OBJ_item_pickup_01", base.gameObject);
			GameObject original = (GameObject)ResourceCache.Acquire("Global VFX/VFX_Item_Pickup");
			GameObject gameObject = UnityEngine.Object.Instantiate(original);
			tk2dSprite component = gameObject.GetComponent<tk2dSprite>();
			component.PlaceAtPositionByAnchor(base.sprite.WorldCenter, tk2dBaseSprite.Anchor.MiddleCenter);
			component.UpdateZDepth();
			DebrisObject component2 = GetComponent<DebrisObject>();
			if (ForceAsExtant || component2 != null)
			{
				player.Blanks++;
				UnityEngine.Object.Destroy(base.gameObject);
				m_pickedUp = true;
				m_pickedUpThisRun = true;
			}
			else
			{
				player.Blanks++;
				m_pickedUp = true;
				m_pickedUpThisRun = true;
			}
			GetRidOfMinimapIcon();
		}
	}

	protected override void DoEffect(PlayerController user)
	{
		AkSoundEngine.PostEvent("Play_OBJ_silenceblank_use_01", base.gameObject);
		AkSoundEngine.PostEvent("Stop_ENM_attack_cancel_01", base.gameObject);
		GameObject gameObject = new GameObject("silencer");
		SilencerInstance silencerInstance = gameObject.AddComponent<SilencerInstance>();
		silencerInstance.TriggerSilencer(user.CenterPosition, silencerSpeed, silencerRadius, silencerVFXPrefab, distortionIntensity, distortionRadius, pushForce, pushRadius, knockbackForce, knockbackRadius, additionalTimeAtMaxRadius, user);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}
}
