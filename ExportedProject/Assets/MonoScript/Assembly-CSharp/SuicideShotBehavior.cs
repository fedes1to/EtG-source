using System;
using UnityEngine;

public class SuicideShotBehavior : BasicAttackBehavior
{
	public float minRange;

	public float leadAmount;

	public string chargeAnim;

	public int numBullets = 1;

	public int degreesBetween;

	public string bulletBankName;

	public bool suppressNormalDeath;

	public bool invulnerableDuringAnimatoin;

	public string fireVfx;

	private float m_cachedProjectileSpeed;

	private Vector2 m_cachedTargetCenter;

	public override void Start()
	{
		base.Start();
		AIBulletBank.Entry bullet = m_aiActor.bulletBank.GetBullet(bulletBankName);
		m_cachedProjectileSpeed = ((!bullet.OverrideProjectile) ? bullet.BulletObject.GetComponent<Projectile>().baseData.speed : bullet.ProjectileData.speed);
		tk2dSpriteAnimator spriteAnimator = m_aiActor.spriteAnimator;
		spriteAnimator.AnimationEventTriggered = (Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip, int>)Delegate.Combine(spriteAnimator.AnimationEventTriggered, new Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip, int>(AnimEventTriggered));
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
		if (!m_aiActor.TargetRigidbody)
		{
			return BehaviorResult.Continue;
		}
		Vector2 vector = m_aiActor.TargetRigidbody.specRigidbody.GetUnitCenter(ColliderType.HitBox);
		if (leadAmount > 0f)
		{
			Vector2 predictedPosition = BraveMathCollege.GetPredictedPosition(vector, m_aiActor.TargetVelocity, m_aiActor.specRigidbody.UnitCenter, m_cachedProjectileSpeed);
			vector = Vector2.Lerp(vector, predictedPosition, leadAmount);
		}
		m_cachedTargetCenter = vector;
		float num = Vector2.Distance(m_aiActor.specRigidbody.UnitCenter, vector);
		if (num > minRange)
		{
			m_aiAnimator.PlayUntilFinished(chargeAnim, true);
			m_aiActor.ClearPath();
			if (invulnerableDuringAnimatoin)
			{
				m_aiActor.healthHaver.minimumHealth = 1f;
			}
			m_updateEveryFrame = true;
			return BehaviorResult.RunContinuous;
		}
		return BehaviorResult.Continue;
	}

	public override ContinuousBehaviorResult ContinuousUpdate()
	{
		if (!m_aiAnimator.IsPlaying(chargeAnim))
		{
			Vector2 vector = m_cachedTargetCenter;
			if ((bool)m_aiActor.TargetRigidbody)
			{
				vector = m_aiActor.TargetRigidbody.specRigidbody.GetUnitCenter(ColliderType.HitBox);
				if (leadAmount > 0f)
				{
					Vector2 predictedPosition = BraveMathCollege.GetPredictedPosition(vector, m_aiActor.TargetVelocity, m_aiActor.specRigidbody.UnitCenter, m_cachedProjectileSpeed);
					vector = Vector2.Lerp(vector, predictedPosition, leadAmount);
				}
			}
			float num = (float)((numBullets - 1) * -degreesBetween) * 0.5f;
			for (int i = 0; i < numBullets; i++)
			{
				Vector2 unitCenter = m_aiActor.specRigidbody.HitboxPixelCollider.UnitCenter;
				m_aiActor.bulletBank.CreateProjectileFromBank(unitCenter, (vector - unitCenter).ToAngle() + num, bulletBankName);
				num += (float)degreesBetween;
			}
			if (!string.IsNullOrEmpty(fireVfx))
			{
				m_aiActor.aiAnimator.PlayVfx(fireVfx);
			}
			if (suppressNormalDeath)
			{
				m_aiActor.healthHaver.ManualDeathHandling = true;
				m_aiActor.healthHaver.DisableStickyFriction = true;
			}
			m_aiActor.AdditionalSingleCoinDropChance = 0f;
			m_aiActor.healthHaver.SuppressDeathSounds = true;
			if (invulnerableDuringAnimatoin)
			{
				m_aiActor.healthHaver.minimumHealth = 0f;
			}
			m_aiActor.healthHaver.ApplyDamage(10000f, Vector2.zero, "Suicide Attack", CoreDamageTypes.None, DamageCategory.Unstoppable, true);
			if (suppressNormalDeath)
			{
				m_aiActor.ParentRoom.DeregisterEnemy(m_aiActor);
				UnityEngine.Object.Destroy(m_aiActor.gameObject);
			}
			return ContinuousBehaviorResult.Finished;
		}
		return ContinuousBehaviorResult.Continue;
	}

	public override void EndContinuousUpdate()
	{
		if (invulnerableDuringAnimatoin)
		{
			m_aiActor.healthHaver.minimumHealth = 0f;
		}
		base.EndContinuousUpdate();
	}

	private void AnimEventTriggered(tk2dSpriteAnimator sprite, tk2dSpriteAnimationClip clip, int frameNum)
	{
		tk2dSpriteAnimationFrame frame = clip.GetFrame(frameNum);
		if (frame.eventInfo == "disable_shadow" && (bool)m_aiActor.ShadowObject)
		{
			m_aiActor.ShadowObject.SetActive(false);
		}
	}
}
