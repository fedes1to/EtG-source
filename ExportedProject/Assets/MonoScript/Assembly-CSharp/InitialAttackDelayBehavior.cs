using UnityEngine;

public class InitialAttackDelayBehavior : AttackBehaviorBase
{
	public float Time = 2f;

	public string PlayDirectionalAnimation;

	public string SetDefaultDirectionalAnimation;

	public bool EndOnDamage;

	private float m_timer;

	private bool m_done;

	public override void Start()
	{
		base.Start();
		if ((bool)m_aiActor.healthHaver && EndOnDamage)
		{
			m_aiActor.healthHaver.OnDamaged += OnDamaged;
		}
	}

	public override void Upkeep()
	{
		base.Upkeep();
		DecrementTimer(ref m_timer);
	}

	public override BehaviorResult Update()
	{
		base.Update();
		if (!m_done)
		{
			if (!string.IsNullOrEmpty(PlayDirectionalAnimation))
			{
				m_aiAnimator.PlayUntilFinished(PlayDirectionalAnimation, true);
			}
			if (!string.IsNullOrEmpty(SetDefaultDirectionalAnimation))
			{
				m_aiAnimator.SetBaseAnim(SetDefaultDirectionalAnimation);
			}
			m_timer = Time;
			return BehaviorResult.RunContinuousInClass;
		}
		return BehaviorResult.Continue;
	}

	public override ContinuousBehaviorResult ContinuousUpdate()
	{
		if (m_timer > 0f)
		{
			m_aiActor.ClearPath();
			return ContinuousBehaviorResult.Continue;
		}
		return ContinuousBehaviorResult.Finished;
	}

	public override void EndContinuousUpdate()
	{
		if (!string.IsNullOrEmpty(PlayDirectionalAnimation))
		{
			m_aiAnimator.EndAnimationIf(PlayDirectionalAnimation);
		}
		m_done = true;
		if ((bool)m_aiActor.healthHaver && EndOnDamage)
		{
			m_aiActor.healthHaver.OnDamaged -= OnDamaged;
		}
	}

	public override bool IsReady()
	{
		return !m_done;
	}

	public override float GetMinReadyRange()
	{
		return -1f;
	}

	public override float GetMaxRange()
	{
		return -1f;
	}

	private void OnDamaged(float resultValue, float maxValue, CoreDamageTypes damageTypes, DamageCategory damageCategory, Vector2 damageDirection)
	{
		if (EndOnDamage)
		{
			m_timer = 0f;
		}
	}
}
