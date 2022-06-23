using System;
using System.Collections;
using Dungeonator;
using UnityEngine;

public class CurrencyPickup : PickupObject
{
	public int currencyValue = 1;

	public string overrideBloopSpriteName = string.Empty;

	public bool IsMetaCurrency;

	private bool m_hasBeenPickedUp;

	private Transform m_transform;

	private SpeculativeRigidbody m_srb;

	[NonSerialized]
	public bool PreventPickup;

	private void Start()
	{
		m_transform = base.transform;
		m_transform.position = m_transform.position.Quantize(0.0625f);
		m_srb = GetComponent<SpeculativeRigidbody>();
		SpeculativeRigidbody srb = m_srb;
		srb.OnTriggerCollision = (SpeculativeRigidbody.OnTriggerDelegate)Delegate.Combine(srb.OnTriggerCollision, new SpeculativeRigidbody.OnTriggerDelegate(OnPreCollision));
		m_srb.Reinitialize();
		if ((bool)base.debris)
		{
			if (IsMetaCurrency)
			{
				base.debris.FlagAsPickup();
			}
			DebrisObject debrisObject = base.debris;
			debrisObject.OnGrounded = (Action<DebrisObject>)Delegate.Combine(debrisObject.OnGrounded, (Action<DebrisObject>)delegate
			{
				base.specRigidbody.Reinitialize();
			});
			base.debris.ForceUpdateIfDisabled = true;
		}
	}

	private void Update()
	{
		if (!IsMetaCurrency)
		{
			if (base.spriteAnimator != null && base.spriteAnimator.DefaultClip != null)
			{
				if (base.spriteAnimator.IsPlaying(base.spriteAnimator.CurrentClip))
				{
					base.spriteAnimator.Stop();
				}
				base.spriteAnimator.SetFrame(Mathf.FloorToInt(Time.time * base.spriteAnimator.DefaultClip.fps % (float)base.spriteAnimator.DefaultClip.frames.Length));
			}
		}
		else
		{
			if (GameManager.Instance.IsLoadingLevel || m_hasBeenPickedUp)
			{
				return;
			}
			if (!m_hasBeenPickedUp && (bool)base.debris && (bool)base.debris.specRigidbody && base.debris.Static && PhysicsEngine.Instance.OverlapCast(base.debris.specRigidbody, null, true, true, CollisionMask.LayerToMask(CollisionLayer.HighObstacle, CollisionLayer.LowObstacle), null, false, null, null))
			{
				base.debris.ForceDestroyAndMaybeRespawn();
			}
			if ((bool)this && !GameManager.Instance.IsAnyPlayerInRoom(base.transform.position.GetAbsoluteRoom()))
			{
				PlayerController bestActivePlayer = GameManager.Instance.BestActivePlayer;
				if ((bool)bestActivePlayer && !bestActivePlayer.IsGhost && bestActivePlayer.AcceptingAnyInput)
				{
					m_hasBeenPickedUp = true;
					Pickup(bestActivePlayer);
				}
			}
			else if ((bool)GameManager.Instance.PrimaryPlayer && (bool)base.sprite)
			{
				CellData cellData = null;
				IntVector2 intVector = base.sprite.WorldCenter.ToIntVector2(VectorConversions.Floor);
				if (GameManager.Instance.Dungeon.data.CheckInBoundsAndValid(intVector))
				{
					cellData = GameManager.Instance.Dungeon.data[intVector];
				}
				if (cellData == null || cellData.type == CellType.WALL)
				{
					m_hasBeenPickedUp = true;
					Pickup(GameManager.Instance.PrimaryPlayer);
				}
			}
		}
	}

	private void OnPreCollision(SpeculativeRigidbody otherRigidbody, SpeculativeRigidbody source, CollisionData collisionData)
	{
		if (!PreventPickup && !m_hasBeenPickedUp)
		{
			PlayerController component = otherRigidbody.GetComponent<PlayerController>();
			if (component != null)
			{
				m_hasBeenPickedUp = true;
				Pickup(component);
			}
		}
	}

	private IEnumerator InitialBounce()
	{
		float startingY = m_transform.position.y;
		Vector2 currentVelocity = Vector2.up * 5f;
		while ((m_srb.Velocity.y >= 0f || m_transform.position.y > startingY) && !m_hasBeenPickedUp)
		{
			currentVelocity += -10f * Vector2.up * BraveTime.DeltaTime;
			m_srb.Velocity = currentVelocity;
			yield return null;
		}
		m_srb.Velocity = Vector2.zero;
	}

	public void ForceSetPickedUp()
	{
		m_hasBeenPickedUp = true;
	}

	public override void Pickup(PlayerController player)
	{
		if (IsMetaCurrency)
		{
			base.spriteAnimator.StopAndResetFrame();
			GameStatsManager.Instance.RegisterStatChange(TrackedStats.META_CURRENCY, currencyValue);
			tk2dBaseSprite targetAutomaticSprite = GetComponent<HologramDoer>().TargetAutomaticSprite;
			targetAutomaticSprite.spriteAnimator.StopAndResetFrame();
			player.BloopItemAboveHead(targetAutomaticSprite, overrideBloopSpriteName);
			GameObject original = (GameObject)ResourceCache.Acquire("Global VFX/VFX_Item_Pickup");
			GameObject gameObject = UnityEngine.Object.Instantiate(original);
			tk2dSprite component = gameObject.GetComponent<tk2dSprite>();
			component.PlaceAtPositionByAnchor(base.sprite.WorldCenter, tk2dBaseSprite.Anchor.MiddleCenter);
			component.UpdateZDepth();
			if ((bool)base.encounterTrackable)
			{
				base.encounterTrackable.DoNotificationOnEncounter = false;
				base.encounterTrackable.HandleEncounter();
				AkSoundEngine.PostEvent("Play_OBJ_metacoin_collect_01", base.gameObject);
			}
			if (GameStatsManager.Instance.GetPlayerStatValue(TrackedStats.META_CURRENCY) == 1f)
			{
				GameUIRoot.Instance.notificationController.DoCustomNotification(StringTableManager.GetItemsString("#HEGEMONYCREDIT_ENCNAME"), StringTableManager.GetItemsString("#HEGEMONYCREDIT_SHORTDESC"), targetAutomaticSprite.Collection, targetAutomaticSprite.spriteId, UINotificationController.NotificationColor.GOLD);
			}
		}
		else
		{
			HandleEncounterable(player);
			player.BloopItemAboveHead(base.sprite, overrideBloopSpriteName);
			if (base.sprite.name == "Coin_Copper(Clone)")
			{
				AkSoundEngine.PostEvent("Play_OBJ_coin_small_01", base.gameObject);
			}
			else if (base.sprite.name == "Coin_Silver(Clone)")
			{
				AkSoundEngine.PostEvent("Play_OBJ_coin_medium_01", base.gameObject);
			}
			else
			{
				AkSoundEngine.PostEvent("Play_OBJ_coin_large_01", base.gameObject);
			}
			GameManager.Instance.PrimaryPlayer.carriedConsumables.Currency += currencyValue;
		}
		UnityEngine.Object.Destroy(base.gameObject);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}
}
