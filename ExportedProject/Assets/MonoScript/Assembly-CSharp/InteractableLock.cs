using System;
using System.Collections;
using System.Collections.Generic;
using Dungeonator;
using UnityEngine;

public class InteractableLock : BraveBehaviour, IPlayerInteractable
{
	public enum InteractableLockMode
	{
		NORMAL,
		RESOURCEFUL_RAT,
		NPC_JAIL,
		RAT_REWARD
	}

	public bool Suppress;

	[NonSerialized]
	public bool IsLocked = true;

	[NonSerialized]
	public bool HasBeenPicked;

	public InteractableLockMode lockMode;

	[PickupIdentifier]
	public int JailCellKeyId = -1;

	[CheckAnimation(null)]
	public string IdleAnimName;

	[CheckAnimation(null)]
	public string UnlockAnimName;

	[CheckAnimation(null)]
	public string NoKeyAnimName;

	[CheckAnimation(null)]
	public string SpitAnimName;

	[CheckAnimation(null)]
	public string BustedAnimName;

	[NonSerialized]
	public bool IsBusted;

	public Action OnUnlocked;

	private bool m_lockHasApproached;

	private bool m_lockHasLaughed;

	private bool m_lockHasSpit;

	private void Awake()
	{
		StaticReferenceManager.AllLocks.Add(this);
	}

	private IEnumerator Start()
	{
		while (Dungeon.IsGenerating)
		{
			yield return null;
		}
		if (lockMode == InteractableLockMode.NPC_JAIL)
		{
			List<PickupObject> list = new List<PickupObject>();
			PickupObject byId = PickupObjectDatabase.GetById(JailCellKeyId);
			list.Add(byId);
			MetaInjectionData.CellGeneratedForCurrentBlueprint = true;
			GameManager.Instance.Dungeon.data.DistributeComplexSecretPuzzleItems(list, GameManager.Instance.Dungeon.data.GetAbsoluteRoomFromPosition(base.transform.position.IntXY()), true, 0.5f);
		}
	}

	private void Update()
	{
		if (IsBusted || !IsLocked || string.IsNullOrEmpty(SpitAnimName))
		{
			return;
		}
		float num = Vector2.Distance(base.sprite.WorldCenter, GameManager.Instance.PrimaryPlayer.specRigidbody.UnitCenter);
		if (!m_lockHasApproached && num < 2.5f)
		{
			base.spriteAnimator.Play(IdleAnimName);
			m_lockHasApproached = true;
		}
		else if (num > 2.5f)
		{
			if (m_lockHasLaughed)
			{
				base.spriteAnimator.Play(SpitAnimName);
			}
			m_lockHasLaughed = false;
			m_lockHasApproached = false;
		}
		if (!m_lockHasSpit && base.spriteAnimator != null && base.spriteAnimator.IsPlaying(SpitAnimName) && base.spriteAnimator.CurrentFrame == 3)
		{
			m_lockHasSpit = true;
			GameObject gameObject = SpawnManager.SpawnVFX(BraveResources.Load("Global VFX/VFX_Lock_Spit") as GameObject);
			tk2dSprite componentInChildren = gameObject.GetComponentInChildren<tk2dSprite>();
			componentInChildren.UpdateZDepth();
			componentInChildren.PlaceAtPositionByAnchor(base.spriteAnimator.sprite.WorldCenter, tk2dBaseSprite.Anchor.UpperCenter);
		}
	}

	public void OnEnteredRange(PlayerController interactor)
	{
		if ((bool)this && !Suppress)
		{
			SpriteOutlineManager.AddOutlineToSprite(base.sprite, Color.white, 0.1f);
			base.sprite.UpdateZDepth();
		}
	}

	public void OnExitRange(PlayerController interactor)
	{
		if ((bool)this && !Suppress)
		{
			SpriteOutlineManager.RemoveOutlineFromSprite(base.sprite);
			base.sprite.UpdateZDepth();
		}
	}

	public float GetDistanceToPoint(Vector2 point)
	{
		if (IsBusted || !IsLocked || Suppress)
		{
			return 10000f;
		}
		Bounds bounds = base.sprite.GetBounds();
		bounds.SetMinMax(bounds.min + base.transform.position, bounds.max + base.transform.position);
		float num = Mathf.Max(Mathf.Min(point.x, bounds.max.x), bounds.min.x);
		float num2 = Mathf.Max(Mathf.Min(point.y, bounds.max.y), bounds.min.y);
		return Mathf.Sqrt((point.x - num) * (point.x - num) + (point.y - num2) * (point.y - num2));
	}

	public float GetOverrideMaxDistance()
	{
		return -1f;
	}

	public void BreakLock()
	{
		if (IsLocked && !IsBusted && lockMode == InteractableLockMode.NORMAL)
		{
			IsBusted = true;
			if (!string.IsNullOrEmpty(BustedAnimName) && !base.spriteAnimator.IsPlaying(BustedAnimName))
			{
				base.spriteAnimator.Play(BustedAnimName);
			}
		}
	}

	public void ForceUnlock()
	{
		if (IsLocked)
		{
			SpriteOutlineManager.RemoveOutlineFromSprite(base.sprite);
			base.sprite.UpdateZDepth();
			IsLocked = false;
			if (OnUnlocked != null)
			{
				OnUnlocked();
			}
			if (!string.IsNullOrEmpty(UnlockAnimName))
			{
				base.spriteAnimator.PlayAndDisableObject(UnlockAnimName);
			}
		}
	}

	public void Interact(PlayerController player)
	{
		if (IsBusted || !IsLocked)
		{
			return;
		}
		bool flag = false;
		if (lockMode == InteractableLockMode.NORMAL)
		{
			flag = player.carriedConsumables.InfiniteKeys || player.carriedConsumables.KeyBullets >= 1;
		}
		else if (lockMode == InteractableLockMode.RESOURCEFUL_RAT)
		{
			for (int i = 0; i < player.passiveItems.Count; i++)
			{
				if (player.passiveItems[i] is SpecialKeyItem && (player.passiveItems[i] as SpecialKeyItem).keyType == SpecialKeyItem.SpecialKeyType.RESOURCEFUL_RAT_LAIR)
				{
					flag = true;
					int pickupObjectId = player.passiveItems[i].PickupObjectId;
					player.RemovePassiveItem(pickupObjectId);
					GameUIRoot.Instance.UpdatePlayerConsumables(player.carriedConsumables);
				}
			}
		}
		else if (lockMode == InteractableLockMode.NPC_JAIL)
		{
			for (int j = 0; j < player.additionalItems.Count; j++)
			{
				if (player.additionalItems[j] is NPCCellKeyItem)
				{
					flag = true;
					GameManager.BroadcastRoomFsmEvent("npcCellUnlocked", base.transform.position.GetAbsoluteRoom());
					UnityEngine.Object.Destroy(player.additionalItems[j].gameObject);
					player.additionalItems.RemoveAt(j);
					GameUIRoot.Instance.UpdatePlayerConsumables(player.carriedConsumables);
				}
			}
		}
		else if (lockMode == InteractableLockMode.RAT_REWARD && player.carriedConsumables.ResourcefulRatKeys > 0)
		{
			player.carriedConsumables.ResourcefulRatKeys--;
			flag = true;
			GameUIRoot.Instance.UpdatePlayerConsumables(player.carriedConsumables);
		}
		if (flag)
		{
			OnExitRange(player);
			IsLocked = false;
			if (OnUnlocked != null)
			{
				OnUnlocked();
			}
			if (lockMode == InteractableLockMode.NORMAL && !player.carriedConsumables.InfiniteKeys)
			{
				player.carriedConsumables.KeyBullets = player.carriedConsumables.KeyBullets - 1;
			}
			if (!string.IsNullOrEmpty(UnlockAnimName))
			{
				base.spriteAnimator.PlayAndDisableObject(UnlockAnimName);
			}
		}
		else
		{
			if (string.IsNullOrEmpty(NoKeyAnimName))
			{
				return;
			}
			if (!string.IsNullOrEmpty(IdleAnimName) && base.spriteAnimator.GetClipByName(IdleAnimName) != null)
			{
				if (!string.IsNullOrEmpty(SpitAnimName))
				{
					base.spriteAnimator.Play(NoKeyAnimName);
				}
				else
				{
					base.spriteAnimator.PlayForDuration(NoKeyAnimName, 1f, IdleAnimName);
				}
				m_lockHasSpit = false;
				m_lockHasLaughed = true;
			}
			else
			{
				base.spriteAnimator.Play(NoKeyAnimName);
			}
		}
	}

	private void ChangeToSpit(tk2dSpriteAnimator arg1, tk2dSpriteAnimationClip arg2)
	{
		if ((bool)base.spriteAnimator)
		{
			base.spriteAnimator.PlayForDuration(SpitAnimName, -1f, IdleAnimName);
		}
	}

	public string GetAnimationState(PlayerController interactor, out bool shouldBeFlipped)
	{
		shouldBeFlipped = false;
		return string.Empty;
	}

	protected override void OnDestroy()
	{
		StaticReferenceManager.AllLocks.Remove(this);
		base.OnDestroy();
	}
}
