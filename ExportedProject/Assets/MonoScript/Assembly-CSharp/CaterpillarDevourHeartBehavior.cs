using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CaterpillarDevourHeartBehavior : OverrideBehaviorBase
{
	public string MunchAnimName = "munch";

	public int RequiredHearts = 3;

	public GameObject NoticedHeartVFX;

	[PickupIdentifier]
	public int SourceCompanionItemId = -1;

	[PickupIdentifier]
	public int WingsItemIdToGive = -1;

	public GameObject TransformVFX;

	[NonSerialized]
	private int m_heartsMunched;

	private PickupObject m_targetHeart;

	private float m_repathTimer = 0.25f;

	private float m_cachedSpeed;

	public override void Start()
	{
		base.Start();
		m_cachedSpeed = m_aiActor.MovementSpeed;
		m_heartsMunched = 0;
	}

	private bool IsHeartInRoom()
	{
		PlayerController companionOwner = m_aiActor.CompanionOwner;
		if (!companionOwner || companionOwner.CurrentRoom == null)
		{
			return false;
		}
		List<HealthPickup> componentsAbsoluteInRoom = companionOwner.CurrentRoom.GetComponentsAbsoluteInRoom<HealthPickup>();
		for (int i = 0; i < componentsAbsoluteInRoom.Count; i++)
		{
			HealthPickup healthPickup = componentsAbsoluteInRoom[i];
			if ((bool)healthPickup && healthPickup.armorAmount == 0)
			{
				componentsAbsoluteInRoom.RemoveAt(i);
				i--;
			}
		}
		HealthPickup closestToPosition = BraveUtility.GetClosestToPosition(componentsAbsoluteInRoom, m_aiActor.CenterPosition);
		if (closestToPosition != null)
		{
			m_targetHeart = closestToPosition;
			return true;
		}
		return false;
	}

	private void MunchHeart(PickupObject targetHeart)
	{
		UnityEngine.Object.Destroy(targetHeart.gameObject);
		m_heartsMunched++;
		m_aiAnimator.PlayUntilFinished("munch");
		if (m_heartsMunched >= RequiredHearts)
		{
			DoTransformation();
		}
	}

	private void DoTransformation()
	{
		if (m_aiActor.CompanionOwner != null)
		{
			if ((bool)TransformVFX)
			{
				SpawnManager.SpawnVFX(TransformVFX, m_aiActor.sprite.WorldBottomCenter, Quaternion.identity);
			}
			GameManager.Instance.StartCoroutine(DelayedGiveItem(m_aiActor.CompanionOwner));
			if (SourceCompanionItemId >= 0)
			{
				m_aiActor.CompanionOwner.RemovePassiveItem(SourceCompanionItemId);
			}
		}
	}

	private IEnumerator DelayedGiveItem(PlayerController targetPlayer)
	{
		yield return new WaitForSeconds(3.375f);
		if ((bool)targetPlayer && !targetPlayer.IsGhost)
		{
			PickupObject byId = PickupObjectDatabase.GetById(WingsItemIdToGive);
			if (byId != null)
			{
				LootEngine.GivePrefabToPlayer(byId.gameObject, targetPlayer);
			}
		}
	}

	public override void Upkeep()
	{
		DecrementTimer(ref m_repathTimer);
		base.Upkeep();
	}

	public override BehaviorResult Update()
	{
		if ((bool)m_targetHeart)
		{
			m_aiActor.PathfindToPosition(m_targetHeart.sprite.WorldCenter, m_targetHeart.sprite.WorldCenter);
			if (m_aiActor.Path != null && m_aiActor.Path.WillReachFinalGoal)
			{
				m_aiActor.MovementSpeed = 1f;
				if (Vector2.Distance(m_targetHeart.sprite.WorldCenter, m_aiActor.CenterPosition) < 1.25f)
				{
					MunchHeart(m_targetHeart);
					m_targetHeart = null;
				}
				return BehaviorResult.SkipAllRemainingBehaviors;
			}
			if (Vector2.Distance(m_targetHeart.sprite.WorldCenter, m_aiActor.CenterPosition) < 1.25f)
			{
				MunchHeart(m_targetHeart);
				m_targetHeart = null;
				return BehaviorResult.SkipAllRemainingBehaviors;
			}
			return base.Update();
		}
		m_aiActor.MovementSpeed = m_cachedSpeed;
		m_targetHeart = null;
		if (m_repathTimer <= 0f)
		{
			m_repathTimer = 1f;
			if (IsHeartInRoom())
			{
				return BehaviorResult.SkipAllRemainingBehaviors;
			}
		}
		return base.Update();
	}
}
