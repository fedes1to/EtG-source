using System;
using UnityEngine;

[Serializable]
public abstract class BehaviorBase
{
	protected GameObject m_gameObject;

	protected AIActor m_aiActor;

	protected AIShooter m_aiShooter;

	protected AIAnimator m_aiAnimator;

	protected float m_deltaTime;

	protected bool m_updateEveryFrame;

	protected bool m_ignoreGlobalCooldown;

	public virtual void Start()
	{
	}

	public virtual void Upkeep()
	{
	}

	public virtual bool OverrideOtherBehaviors()
	{
		return false;
	}

	public virtual BehaviorResult Update()
	{
		return BehaviorResult.Continue;
	}

	public virtual ContinuousBehaviorResult ContinuousUpdate()
	{
		return ContinuousBehaviorResult.Continue;
	}

	public virtual void EndContinuousUpdate()
	{
	}

	public virtual void OnActorPreDeath()
	{
	}

	public virtual void Destroy()
	{
	}

	public virtual void Init(GameObject gameObject, AIActor aiActor, AIShooter aiShooter)
	{
		m_gameObject = gameObject;
		m_aiActor = aiActor;
		m_aiShooter = aiShooter;
		m_aiAnimator = gameObject.GetComponent<AIAnimator>();
	}

	public virtual void SetDeltaTime(float deltaTime)
	{
		m_deltaTime = deltaTime;
	}

	public virtual bool UpdateEveryFrame()
	{
		return m_updateEveryFrame;
	}

	public virtual bool IgnoreGlobalCooldown()
	{
		return m_ignoreGlobalCooldown;
	}

	public virtual bool IsOverridable()
	{
		return true;
	}

	protected void DecrementTimer(ref float timer, bool useCooldownFactor = false)
	{
		float num = m_deltaTime;
		if ((bool)m_aiActor && useCooldownFactor)
		{
			num *= m_aiActor.behaviorSpeculator.CooldownScale;
		}
		timer = Mathf.Max(timer - num, 0f);
	}
}
