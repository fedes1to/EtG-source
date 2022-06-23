using System;
using System.Collections;
using System.Collections.Generic;
using Dungeonator;
using UnityEngine;

public class BeeProjectile : Projectile
{
	public float angularAcceleration = 10f;

	public float searchRadius = 10f;

	public GameActor CurrentTarget;

	protected bool m_coroutineIsActive;

	protected AIActor m_previouslyHitEnemy;

	public override void Start()
	{
		base.Start();
		OnHitEnemy = (Action<Projectile, SpeculativeRigidbody, bool>)Delegate.Combine(OnHitEnemy, new Action<Projectile, SpeculativeRigidbody, bool>(HandleHitEnemy));
		ModifyVelocity = (Func<Vector2, Vector2>)Delegate.Combine(ModifyVelocity, new Func<Vector2, Vector2>(ModifyVelocityLocal));
	}

	private void HandleHitEnemy(Projectile arg1, SpeculativeRigidbody arg2, bool arg3)
	{
		m_previouslyHitEnemy = null;
		CurrentTarget = null;
		if ((bool)arg2 && (bool)arg2.aiActor)
		{
			m_previouslyHitEnemy = arg2.aiActor;
		}
	}

	private IEnumerator FindTarget()
	{
		m_coroutineIsActive = true;
		while (true)
		{
			if (base.Owner is PlayerController)
			{
				List<AIActor> activeEnemies = GameManager.Instance.Dungeon.data.GetAbsoluteRoomFromPosition(base.Owner.transform.position.IntXY(VectorConversions.Floor)).GetActiveEnemies(RoomHandler.ActiveEnemyType.All);
				if (activeEnemies != null)
				{
					float num = float.MaxValue;
					for (int i = 0; i < activeEnemies.Count; i++)
					{
						AIActor aIActor = activeEnemies[i];
						if ((bool)aIActor && (bool)aIActor.healthHaver && !aIActor.healthHaver.IsDead && !aIActor.IsGone && aIActor.IsWorthShootingAt && (bool)aIActor.specRigidbody && !(aIActor == m_previouslyHitEnemy))
						{
							float num2 = Vector2.Distance(aIActor.specRigidbody.GetUnitCenter(ColliderType.HitBox), base.Owner.specRigidbody.UnitCenter);
							if (num2 < num)
							{
								CurrentTarget = aIActor;
								num = num2;
							}
						}
					}
				}
			}
			else
			{
				CurrentTarget = GameManager.Instance.GetPlayerClosestToPoint(base.transform.position.XY());
			}
			yield return new WaitForSeconds(1f);
		}
	}

	private Vector2 ModifyVelocityLocal(Vector2 inVel)
	{
		if (!m_coroutineIsActive)
		{
			StartCoroutine(FindTarget());
		}
		float num = 1f;
		inVel = m_currentDirection;
		Vector2 vector = inVel;
		if (CurrentTarget != null && !CurrentTarget.IsGone)
		{
			Vector2 normalized = (CurrentTarget.specRigidbody.GetUnitCenter(ColliderType.HitBox) - base.specRigidbody.UnitCenter).normalized;
			vector = Vector3.RotateTowards(inVel, normalized, angularAcceleration * ((float)Math.PI / 180f) * BraveTime.DeltaTime, 0f).XY().normalized;
			float f = Vector2.Angle(vector, normalized);
			num = 0.25f + (1f - Mathf.Clamp01(Mathf.Abs(f) / 60f)) * 0.75f;
		}
		vector = vector * m_currentSpeed * num;
		if (OverrideMotionModule != null)
		{
			float current = BraveMathCollege.Atan2Degrees(inVel);
			float target = BraveMathCollege.Atan2Degrees(vector);
			OverrideMotionModule.AdjustRightVector(Mathf.DeltaAngle(current, target));
		}
		return vector;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}
}
