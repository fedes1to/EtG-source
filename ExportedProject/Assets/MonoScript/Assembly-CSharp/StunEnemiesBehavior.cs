using System.Collections;
using UnityEngine;

public class StunEnemiesBehavior : AttackBehaviorBase
{
	public float StunDuration = 1f;

	public float Cooldown;

	public float minAttackDistance = 0.1f;

	public float maxAttackDistance = 1f;

	public string AnimationName;

	public GameObject StunVFX;

	private float m_cooldownTimer;

	private float m_minAttackDistance;

	private float m_maxAttackDistance;

	public override void Start()
	{
		base.Start();
		m_minAttackDistance = minAttackDistance;
		m_maxAttackDistance = maxAttackDistance;
	}

	public override BehaviorResult Update()
	{
		base.Update();
		bool flag = false;
		DecrementTimer(ref m_cooldownTimer);
		BehaviorResult behaviorResult = base.Update();
		if (behaviorResult != 0)
		{
			return behaviorResult;
		}
		if (m_cooldownTimer > 0f)
		{
			return BehaviorResult.Continue;
		}
		if (!m_aiActor.TargetRigidbody)
		{
			return BehaviorResult.Continue;
		}
		SpeculativeRigidbody targetRigidbody = m_aiActor.TargetRigidbody;
		AIActor aiActor = targetRigidbody.aiActor;
		if ((bool)aiActor)
		{
			if (!aiActor.IsNormalEnemy)
			{
				return BehaviorResult.Continue;
			}
			HealthHaver healthHaver = targetRigidbody.healthHaver;
			if ((bool)healthHaver)
			{
				if (!healthHaver.IsVulnerable)
				{
					return BehaviorResult.Continue;
				}
				if (healthHaver.IsBoss)
				{
					flag = GameManager.Instance.Dungeon.CellSupportsFalling(targetRigidbody.UnitCenter);
				}
			}
		}
		m_minAttackDistance = minAttackDistance;
		m_maxAttackDistance = maxAttackDistance;
		if (flag)
		{
			m_minAttackDistance = minAttackDistance;
			m_maxAttackDistance = maxAttackDistance + 1f;
		}
		Vector2 unitCenter = m_aiActor.specRigidbody.UnitCenter;
		Vector2 targetPoint = GetTargetPoint(m_aiActor.TargetRigidbody, unitCenter);
		float num = Vector2.Distance(unitCenter, targetPoint);
		bool hasLineOfSightToTarget = m_aiActor.HasLineOfSightToTarget;
		if (num < maxAttackDistance && hasLineOfSightToTarget)
		{
			BehaviorSpeculator component = targetRigidbody.GetComponent<BehaviorSpeculator>();
			if ((bool)component)
			{
				if (!string.IsNullOrEmpty(AnimationName) && !m_aiAnimator.IsPlaying(AnimationName))
				{
					m_aiAnimator.PlayUntilFinished(AnimationName);
					if ((bool)StunVFX)
					{
						m_aiActor.StartCoroutine(HandleDelayedSpawnStunVFX(targetPoint));
					}
				}
				component.Stun(StunDuration);
				m_cooldownTimer = Cooldown;
				m_updateEveryFrame = true;
				return BehaviorResult.RunContinuous;
			}
		}
		return BehaviorResult.Continue;
	}

	private IEnumerator HandleDelayedSpawnStunVFX(Vector2 targetPoint)
	{
		yield return new WaitForSeconds(0.75f);
		if ((bool)StunVFX)
		{
			bool flag = BraveMathCollege.AbsAngleBetween(m_aiAnimator.FacingDirection, 0f) < 90f;
			GameObject gameObject = SpawnManager.SpawnVFX(StunVFX, m_aiActor.CenterPosition + new Vector2(0.625f * (float)(flag ? 1 : (-1)), -0.0625f), Quaternion.identity);
			gameObject.transform.parent = m_aiActor.transform;
		}
	}

	public override ContinuousBehaviorResult ContinuousUpdate()
	{
		SpeculativeRigidbody targetRigidbody = m_aiActor.TargetRigidbody;
		if (!targetRigidbody)
		{
			return ContinuousBehaviorResult.Finished;
		}
		bool flag = false;
		if ((bool)targetRigidbody && (bool)targetRigidbody.aiActor && (bool)targetRigidbody.healthHaver && targetRigidbody.healthHaver.IsBoss)
		{
			flag = GameManager.Instance.Dungeon.CellSupportsFalling(targetRigidbody.UnitCenter);
		}
		m_minAttackDistance = minAttackDistance;
		m_maxAttackDistance = maxAttackDistance;
		if (flag)
		{
			m_minAttackDistance = minAttackDistance;
			m_maxAttackDistance = maxAttackDistance + 1f;
		}
		Vector2 unitCenter = m_aiActor.specRigidbody.UnitCenter;
		Vector2 targetPoint = GetTargetPoint(m_aiActor.TargetRigidbody, unitCenter);
		float num = Vector2.Distance(unitCenter, targetPoint);
		if (num > maxAttackDistance)
		{
			return ContinuousBehaviorResult.Finished;
		}
		m_aiActor.ClearPath();
		BehaviorSpeculator component = targetRigidbody.GetComponent<BehaviorSpeculator>();
		if ((bool)component)
		{
			if ((bool)component.healthHaver && !component.healthHaver.IsVulnerable)
			{
				return ContinuousBehaviorResult.Finished;
			}
			if (!string.IsNullOrEmpty(AnimationName) && !m_aiAnimator.IsPlaying(AnimationName))
			{
				m_aiAnimator.PlayUntilFinished(AnimationName);
				if ((bool)StunVFX)
				{
					m_aiActor.StartCoroutine(HandleDelayedSpawnStunVFX(targetPoint));
				}
			}
			if (component.IsStunned)
			{
				component.UpdateStun(StunDuration);
			}
			else
			{
				component.Stun(StunDuration);
			}
			m_cooldownTimer = Cooldown;
		}
		return ContinuousBehaviorResult.Continue;
	}

	public override void EndContinuousUpdate()
	{
		base.EndContinuousUpdate();
		m_aiActor.BehaviorOverridesVelocity = false;
		m_aiAnimator.LockFacingDirection = false;
		m_updateEveryFrame = false;
	}

	private Vector2 GetTargetPoint(SpeculativeRigidbody targetRigidbody, Vector2 myCenter)
	{
		PixelCollider hitboxPixelCollider = targetRigidbody.HitboxPixelCollider;
		return BraveMathCollege.ClosestPointOnRectangle(myCenter, hitboxPixelCollider.UnitBottomLeft, hitboxPixelCollider.UnitDimensions);
	}

	public override bool IsReady()
	{
		return true;
	}

	public override float GetMinReadyRange()
	{
		return m_maxAttackDistance;
	}

	public override float GetMaxRange()
	{
		return m_maxAttackDistance;
	}
}
