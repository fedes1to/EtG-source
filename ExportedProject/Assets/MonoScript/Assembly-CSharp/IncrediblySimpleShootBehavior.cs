using UnityEngine;

public class IncrediblySimpleShootBehavior : BasicAttackBehavior
{
	public Vector2 ShootDirection = Vector2.right;

	public WeaponType WeaponType;

	public string OverrideBulletName;

	public string OverrideAnimation;

	public string OverrideDirectionalAnimation;

	public override void Start()
	{
		base.Start();
	}

	public override void Upkeep()
	{
		base.Upkeep();
	}

	private void HandleAIShootVolley()
	{
		m_aiShooter.ShootInDirection(ShootDirection, OverrideBulletName);
	}

	private void HandleAIShoot()
	{
		m_aiShooter.ShootInDirection(ShootDirection, OverrideBulletName);
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
		HandleAIShoot();
		if (!string.IsNullOrEmpty(OverrideDirectionalAnimation))
		{
			if (m_aiAnimator != null)
			{
				m_aiAnimator.PlayUntilFinished(OverrideDirectionalAnimation, true);
			}
			else
			{
				m_aiActor.spriteAnimator.PlayForDuration(OverrideDirectionalAnimation, -1f, m_aiActor.spriteAnimator.CurrentClip.name);
			}
		}
		else if (!string.IsNullOrEmpty(OverrideAnimation))
		{
			if (m_aiAnimator != null)
			{
				m_aiAnimator.PlayUntilFinished(OverrideAnimation);
			}
			else
			{
				m_aiActor.spriteAnimator.PlayForDuration(OverrideAnimation, -1f, m_aiActor.spriteAnimator.CurrentClip.name);
			}
		}
		UpdateCooldowns();
		return BehaviorResult.SkipRemainingClassBehaviors;
	}
}
