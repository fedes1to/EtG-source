using System;
using System.Collections.Generic;
using Dungeonator;
using UnityEngine;

public class InteractableDoorController : DungeonPlaceableBehaviour, IPlayerInteractable
{
	public List<InteractableLock> WorldLocks;

	public bool OpensAutomaticallyOnUnlocked;

	private bool m_hasOpened;

	public override GameObject InstantiateObject(RoomHandler targetRoom, IntVector2 loc, bool deferConfiguration = false)
	{
		return base.InstantiateObject(targetRoom, loc, deferConfiguration);
	}

	private void Start()
	{
		if (WorldLocks.Count > 0 && WorldLocks[0].lockMode == InteractableLock.InteractableLockMode.NPC_JAIL)
		{
			GameStatsManager.Instance.NumberRunsValidCellWithoutSpawn = 0;
		}
		RoomHandler absoluteParentRoom = GetAbsoluteParentRoom();
		for (int i = 0; i < WorldLocks.Count; i++)
		{
			absoluteParentRoom.RegisterInteractable(WorldLocks[i]);
		}
		if ((bool)base.specRigidbody)
		{
			SpeculativeRigidbody speculativeRigidbody = base.specRigidbody;
			speculativeRigidbody.OnRigidbodyCollision = (SpeculativeRigidbody.OnRigidbodyCollisionDelegate)Delegate.Combine(speculativeRigidbody.OnRigidbodyCollision, new SpeculativeRigidbody.OnRigidbodyCollisionDelegate(HandleRigidbodyCollision));
		}
	}

	private void HandleRigidbodyCollision(CollisionData rigidbodyCollision)
	{
		if (rigidbodyCollision == null || !rigidbodyCollision.OtherRigidbody || !rigidbodyCollision.OtherRigidbody.GetComponent<KeyProjModifier>())
		{
			return;
		}
		for (int i = 0; i < WorldLocks.Count; i++)
		{
			if ((bool)WorldLocks[i] && WorldLocks[i].IsLocked && WorldLocks[i].lockMode == InteractableLock.InteractableLockMode.NORMAL)
			{
				WorldLocks[i].ForceUnlock();
			}
		}
	}

	private void Update()
	{
		if (!m_hasOpened && OpensAutomaticallyOnUnlocked && IsValidForUse())
		{
			Open();
		}
	}

	private bool IsValidForUse()
	{
		if (m_hasOpened)
		{
			return false;
		}
		bool result = true;
		for (int i = 0; i < WorldLocks.Count; i++)
		{
			if (WorldLocks[i].IsLocked || WorldLocks[i].spriteAnimator.IsPlaying(WorldLocks[i].spriteAnimator.CurrentClip))
			{
				result = false;
			}
		}
		return result;
	}

	public void OnEnteredRange(PlayerController interactor)
	{
		if ((bool)this)
		{
			SpriteOutlineManager.AddOutlineToSprite(base.sprite, Color.white, 0.1f);
			base.sprite.UpdateZDepth();
		}
	}

	public void OnExitRange(PlayerController interactor)
	{
		if ((bool)this)
		{
			SpriteOutlineManager.RemoveOutlineFromSprite(base.sprite);
			base.sprite.UpdateZDepth();
		}
	}

	public float GetDistanceToPoint(Vector2 point)
	{
		if (OpensAutomaticallyOnUnlocked || !IsValidForUse())
		{
			return 1000f;
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

	private void Open()
	{
		m_hasOpened = true;
		base.spriteAnimator.Play();
		base.specRigidbody.enabled = false;
	}

	public void Interact(PlayerController player)
	{
		if (IsValidForUse())
		{
			Open();
		}
	}

	public string GetAnimationState(PlayerController interactor, out bool shouldBeFlipped)
	{
		shouldBeFlipped = false;
		return string.Empty;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}
}
