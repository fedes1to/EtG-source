using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WizardSpinShootBehavior : BasicAttackBehavior
{
	private enum SpinShootState
	{
		None,
		Spawn,
		Prefire,
		Fire
	}

	public bool LineOfSight = true;

	public string OverrideBulletName;

	public bool CanHitEnemies;

	public Transform ShootPoint;

	public int NumBullets;

	public int BulletCircleSpeed;

	public float BulletCircleRadius;

	public float FirstSpawnDelay;

	public float SpawnDelay;

	public bool PrefireUseAnimTime;

	public float PrefireDelay;

	public float FirstFireDelay;

	public float FireDelay;

	public float LeadAmount;

	public string CastVfx;

	public List<Light> CastLights;

	private SpinShootState m_state;

	private float m_stateTimer;

	private bool m_isCharmed;

	private List<Tuple<Projectile, float>> m_bulletPositions = new List<Tuple<Projectile, float>>();

	private PixelCollider m_bulletCatcher;

	private float BulletAngleDelta
	{
		get
		{
			return 360f / (float)NumBullets;
		}
	}

	private SpinShootState State
	{
		get
		{
			return m_state;
		}
		set
		{
			EndState(m_state);
			m_state = value;
			BeginState(m_state);
		}
	}

	public override void Start()
	{
		base.Start();
		IntVector2 intVector = PhysicsEngine.UnitToPixel(ShootPoint.position - m_aiActor.transform.position);
		int num = PhysicsEngine.UnitToPixel(BulletCircleRadius);
		m_bulletCatcher = new PixelCollider();
		m_bulletCatcher.ColliderGenerationMode = PixelCollider.PixelColliderGeneration.Circle;
		m_bulletCatcher.CollisionLayer = CollisionLayer.BulletBlocker;
		m_bulletCatcher.IsTrigger = true;
		m_bulletCatcher.ManualOffsetX = intVector.x - num;
		m_bulletCatcher.ManualOffsetY = intVector.y - num;
		m_bulletCatcher.ManualDiameter = num * 2;
		m_bulletCatcher.Regenerate(m_aiActor.transform);
		m_aiActor.specRigidbody.PixelColliders.Add(m_bulletCatcher);
		SpeculativeRigidbody specRigidbody = m_aiActor.specRigidbody;
		specRigidbody.OnTriggerCollision = (SpeculativeRigidbody.OnTriggerDelegate)Delegate.Combine(specRigidbody.OnTriggerCollision, new SpeculativeRigidbody.OnTriggerDelegate(OnTriggerCollision));
		if (CastLights != null)
		{
			for (int i = 0; i < CastLights.Count; i++)
			{
				CastLights[i].enabled = false;
			}
		}
	}

	private void OnTriggerCollision(SpeculativeRigidbody specRigidbody, SpeculativeRigidbody sourceSpecRigidbody, CollisionData collisionData)
	{
		if ((State == SpinShootState.Spawn || State == SpinShootState.Prefire) && collisionData.MyPixelCollider == m_bulletCatcher && collisionData.OtherRigidbody != null && collisionData.OtherRigidbody.projectile != null)
		{
			Projectile projectile = collisionData.OtherRigidbody.projectile;
			if (((!m_isCharmed) ? (projectile.Owner is PlayerController) : (projectile.Owner is AIActor)) && projectile.CanBeCaught)
			{
				projectile.specRigidbody.DeregisterSpecificCollisionException(projectile.Owner.specRigidbody);
				projectile.Shooter = m_aiActor.specRigidbody;
				projectile.Owner = m_aiActor;
				projectile.specRigidbody.Velocity = Vector2.zero;
				projectile.ManualControl = true;
				projectile.baseData.SetAll(m_aiActor.bulletBank.GetBullet().ProjectileData);
				projectile.UpdateSpeed();
				projectile.specRigidbody.CollideWithTileMap = false;
				projectile.ResetDistance();
				projectile.collidesWithEnemies = m_isCharmed;
				projectile.collidesWithPlayer = true;
				projectile.UpdateCollisionMask();
				projectile.sprite.color = new Color(1f, 0.1f, 0.1f);
				projectile.MakeLookLikeEnemyBullet();
				projectile.RemovePlayerOnlyModifiers();
				float second = BraveMathCollege.ClampAngle360((collisionData.Contact - ShootPoint.position.XY()).ToAngle());
				m_bulletPositions.Insert(Mathf.Max(0, m_bulletPositions.Count - 1), Tuple.Create(projectile, second));
			}
		}
	}

	public override void Upkeep()
	{
		base.Upkeep();
		m_stateTimer -= m_deltaTime;
		if (m_isCharmed == m_aiActor.CanTargetEnemies)
		{
			return;
		}
		m_isCharmed = m_aiActor.CanTargetEnemies;
		for (int i = 0; i < m_bulletPositions.Count; i++)
		{
			Projectile first = m_bulletPositions[i].First;
			if (!(first == null))
			{
				first.collidesWithEnemies = m_isCharmed;
				first.UpdateCollisionMask();
			}
		}
	}

	public override BehaviorResult Update()
	{
		base.Update();
		BehaviorResult behaviorResult = base.Update();
		if (behaviorResult != 0)
		{
			return behaviorResult;
		}
		if (!IsReady())
		{
			return BehaviorResult.Continue;
		}
		if (!m_aiActor.HasLineOfSightToTarget)
		{
			return BehaviorResult.Continue;
		}
		State = SpinShootState.Spawn;
		m_updateEveryFrame = true;
		return BehaviorResult.RunContinuous;
	}

	public override ContinuousBehaviorResult ContinuousUpdate()
	{
		for (int num = m_bulletPositions.Count - 1; num >= 0; num--)
		{
			float num2 = m_bulletPositions[num].Second + m_deltaTime * (float)BulletCircleSpeed;
			m_bulletPositions[num].Second = num2;
			Projectile first = m_bulletPositions[num].First;
			if (!(first == null))
			{
				if (!first)
				{
					m_bulletPositions[num] = null;
				}
				else
				{
					Vector2 bulletPosition = GetBulletPosition(num2);
					first.specRigidbody.Velocity = (bulletPosition - (Vector2)first.transform.position) / BraveTime.DeltaTime;
					if (first.shouldRotate)
					{
						first.transform.rotation = Quaternion.Euler(0f, 0f, 180f + (Quaternion.Euler(0f, 0f, 90f) * (ShootPoint.position.XY() - bulletPosition)).XY().ToAngle());
					}
					first.ResetDistance();
				}
			}
		}
		if (State == SpinShootState.Spawn)
		{
			while (m_stateTimer <= 0f && State == SpinShootState.Spawn)
			{
				AIBulletBank.Entry bullet = m_aiActor.bulletBank.GetBullet(OverrideBulletName);
				GameObject bulletObject = bullet.BulletObject;
				float num3 = 0f;
				if (m_bulletPositions.Count > 0)
				{
					num3 = BraveMathCollege.ClampAngle360(m_bulletPositions[m_bulletPositions.Count - 1].Second - BulletAngleDelta);
				}
				GameObject gameObject = SpawnManager.SpawnProjectile(bulletObject, GetBulletPosition(num3), Quaternion.Euler(0f, 0f, 0f));
				Projectile component = gameObject.GetComponent<Projectile>();
				if (bullet != null && bullet.OverrideProjectile)
				{
					component.baseData.SetAll(bullet.ProjectileData);
				}
				component.Shooter = m_aiActor.specRigidbody;
				component.specRigidbody.Velocity = Vector2.zero;
				component.ManualControl = true;
				component.specRigidbody.CollideWithTileMap = false;
				component.collidesWithEnemies = m_isCharmed;
				component.UpdateCollisionMask();
				m_bulletPositions.Add(Tuple.Create(component, num3));
				m_stateTimer += SpawnDelay;
				if (m_bulletPositions.Count >= NumBullets)
				{
					State = SpinShootState.Prefire;
				}
			}
		}
		else if (State == SpinShootState.Prefire)
		{
			if (m_stateTimer <= 0f)
			{
				State = SpinShootState.Fire;
			}
		}
		else if (State == SpinShootState.Fire)
		{
			if (m_behaviorSpeculator.TargetBehaviors != null && m_behaviorSpeculator.TargetBehaviors.Count > 0)
			{
				m_behaviorSpeculator.TargetBehaviors[0].Update();
			}
			if (m_bulletPositions.All((Tuple<Projectile, float> t) => t.First == null))
			{
				return ContinuousBehaviorResult.Finished;
			}
			while (m_stateTimer <= 0f)
			{
				Vector2 vector = ShootPoint.position.XY();
				Vector2 vector2 = vector + ((!m_aiActor.TargetRigidbody) ? Vector2.zero : (m_aiActor.TargetRigidbody.UnitCenter - vector)).normalized * BulletCircleRadius;
				int num4 = -1;
				float num5 = float.MaxValue;
				for (int i = 0; i < m_bulletPositions.Count; i++)
				{
					Projectile first2 = m_bulletPositions[i].First;
					if (!(first2 == null))
					{
						float sqrMagnitude = (first2.specRigidbody.UnitCenter - vector2).sqrMagnitude;
						if (sqrMagnitude < num5)
						{
							num5 = sqrMagnitude;
							num4 = i;
						}
					}
				}
				if (num4 >= 0)
				{
					Projectile first3 = m_bulletPositions[num4].First;
					first3.ManualControl = false;
					first3.specRigidbody.CollideWithTileMap = true;
					if ((bool)m_aiActor.TargetRigidbody)
					{
						Vector2 unitCenter = m_aiActor.TargetRigidbody.specRigidbody.GetUnitCenter(ColliderType.HitBox);
						float speed = first3.Speed;
						float num6 = Vector2.Distance(first3.specRigidbody.UnitCenter, unitCenter) / speed;
						Vector2 b = unitCenter + m_aiActor.TargetRigidbody.specRigidbody.Velocity * num6;
						Vector2 vector3 = Vector2.Lerp(unitCenter, b, LeadAmount);
						first3.SendInDirection(vector3 - first3.specRigidbody.UnitCenter, true);
					}
					first3.transform.rotation = Quaternion.Euler(0f, 0f, first3.specRigidbody.Velocity.ToAngle());
					m_bulletPositions[num4].First = null;
				}
				else
				{
					Debug.LogError("WizardSpinShootBehaviour.ContinuousUpdate(): This shouldn't happen!");
				}
				m_stateTimer += FireDelay;
				if (m_bulletPositions.All((Tuple<Projectile, float> t) => t.First == null))
				{
					return ContinuousBehaviorResult.Finished;
				}
			}
		}
		return ContinuousBehaviorResult.Continue;
	}

	public override void EndContinuousUpdate()
	{
		base.EndContinuousUpdate();
		FreeRemainingProjectiles();
		State = SpinShootState.None;
		m_aiAnimator.EndAnimationIf("attack");
		UpdateCooldowns();
		m_updateEveryFrame = false;
	}

	public override void OnActorPreDeath()
	{
		base.OnActorPreDeath();
		SpeculativeRigidbody specRigidbody = m_aiActor.specRigidbody;
		specRigidbody.OnTriggerCollision = (SpeculativeRigidbody.OnTriggerDelegate)Delegate.Remove(specRigidbody.OnTriggerCollision, new SpeculativeRigidbody.OnTriggerDelegate(OnTriggerCollision));
		FreeRemainingProjectiles();
	}

	public override void Destroy()
	{
		base.Destroy();
		State = SpinShootState.None;
	}

	private Vector2 GetBulletPosition(float angle)
	{
		return ShootPoint.position.XY() + new Vector2(Mathf.Cos(angle * ((float)Math.PI / 180f)), Mathf.Sin(angle * ((float)Math.PI / 180f))) * BulletCircleRadius;
	}

	private void BeginState(SpinShootState state)
	{
		if (state == SpinShootState.None)
		{
			m_bulletPositions.Clear();
		}
		switch (state)
		{
		case SpinShootState.Spawn:
			m_aiAnimator.PlayUntilCancelled("cast", true);
			m_stateTimer = FirstSpawnDelay;
			if (!string.IsNullOrEmpty(CastVfx))
			{
				m_aiAnimator.PlayVfx(CastVfx);
			}
			if (CastLights != null)
			{
				for (int i = 0; i < CastLights.Count; i++)
				{
					CastLights[i].enabled = true;
				}
			}
			if ((bool)m_aiActor && (bool)m_aiActor.knockbackDoer)
			{
				m_aiActor.knockbackDoer.SetImmobile(true, "WizardSpinShootBehavior");
			}
			m_aiActor.ClearPath();
			break;
		case SpinShootState.Prefire:
			m_aiAnimator.PlayUntilFinished("attack", true);
			m_stateTimer = PrefireDelay;
			if (PrefireUseAnimTime)
			{
				m_stateTimer += (float)m_aiAnimator.spriteAnimator.CurrentClip.frames.Length / m_aiAnimator.spriteAnimator.CurrentClip.fps;
			}
			break;
		case SpinShootState.Fire:
			m_stateTimer = FirstFireDelay;
			break;
		}
	}

	private void EndState(SpinShootState state)
	{
		if (state == SpinShootState.Spawn)
		{
			m_aiAnimator.EndAnimationIf("cast");
			if (!string.IsNullOrEmpty(CastVfx))
			{
				m_aiAnimator.StopVfx(CastVfx);
			}
			if (CastLights != null)
			{
				for (int i = 0; i < CastLights.Count; i++)
				{
					CastLights[i].enabled = false;
				}
			}
		}
		if ((bool)m_aiActor && (bool)m_aiActor.knockbackDoer)
		{
			m_aiActor.knockbackDoer.SetImmobile(false, "WizardSpinShootBehavior");
		}
	}

	private void FreeRemainingProjectiles()
	{
		for (int i = 0; i < m_bulletPositions.Count; i++)
		{
			Projectile first = m_bulletPositions[i].First;
			if (!(first == null))
			{
				first.ManualControl = false;
				first.specRigidbody.CollideWithTileMap = true;
				first.SendInDirection((Quaternion.Euler(0f, 0f, 90f) * (first.specRigidbody.UnitCenter - ShootPoint.position.XY())).XY(), true);
				first.transform.rotation = Quaternion.Euler(0f, 0f, first.specRigidbody.Velocity.ToAngle());
				m_bulletPositions[i].First = null;
			}
		}
	}
}
