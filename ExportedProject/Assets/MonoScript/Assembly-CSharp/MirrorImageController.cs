using System;
using System.Collections.Generic;
using UnityEngine;

public class MirrorImageController : BraveBehaviour
{
	public List<string> MirrorAnimations = new List<string>();

	private AIActor m_host;

	public void Awake()
	{
		base.aiActor.CanDropCurrency = false;
		base.aiActor.CanDropItems = false;
		base.aiActor.CollisionDamage = 0f;
		if ((bool)base.aiActor.encounterTrackable)
		{
			UnityEngine.Object.Destroy(base.aiActor.encounterTrackable);
		}
		base.behaviorSpeculator.AttackCooldown = 10f;
		RegenerateCache();
	}

	public void Update()
	{
		base.behaviorSpeculator.AttackCooldown = 10f;
		if ((bool)m_host)
		{
			if (m_host.behaviorSpeculator.ActiveContinuousAttackBehavior != null)
			{
				base.aiActor.ClearPath();
				base.behaviorSpeculator.GlobalCooldown = 1f;
			}
			else
			{
				base.behaviorSpeculator.GlobalCooldown = 0f;
			}
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if ((bool)m_host)
		{
			m_host.healthHaver.OnPreDeath -= OnHostPreDeath;
			AIAnimator aIAnimator = m_host.aiAnimator;
			aIAnimator.OnPlayUntilFinished = (AIAnimator.PlayUntilFinishedDelegate)Delegate.Remove(aIAnimator.OnPlayUntilFinished, new AIAnimator.PlayUntilFinishedDelegate(PlayUntilFinished));
			AIAnimator aIAnimator2 = m_host.aiAnimator;
			aIAnimator2.OnEndAnimationIf = (AIAnimator.EndAnimationIfDelegate)Delegate.Remove(aIAnimator2.OnEndAnimationIf, new AIAnimator.EndAnimationIfDelegate(EndAnimationIf));
			AIAnimator aIAnimator3 = m_host.aiAnimator;
			aIAnimator3.OnPlayVfx = (AIAnimator.PlayVfxDelegate)Delegate.Remove(aIAnimator3.OnPlayVfx, new AIAnimator.PlayVfxDelegate(PlayVfx));
			AIAnimator aIAnimator4 = m_host.aiAnimator;
			aIAnimator4.OnStopVfx = (AIAnimator.StopVfxDelegate)Delegate.Remove(aIAnimator4.OnStopVfx, new AIAnimator.StopVfxDelegate(StopVfx));
		}
	}

	public void SetHost(AIActor host)
	{
		m_host = host;
		if ((bool)m_host)
		{
			base.aiAnimator.CopyStateFrom(m_host.aiAnimator);
			AIAnimator aIAnimator = m_host.aiAnimator;
			aIAnimator.OnPlayUntilFinished = (AIAnimator.PlayUntilFinishedDelegate)Delegate.Combine(aIAnimator.OnPlayUntilFinished, new AIAnimator.PlayUntilFinishedDelegate(PlayUntilFinished));
			AIAnimator aIAnimator2 = m_host.aiAnimator;
			aIAnimator2.OnEndAnimationIf = (AIAnimator.EndAnimationIfDelegate)Delegate.Combine(aIAnimator2.OnEndAnimationIf, new AIAnimator.EndAnimationIfDelegate(EndAnimationIf));
			AIAnimator aIAnimator3 = m_host.aiAnimator;
			aIAnimator3.OnPlayVfx = (AIAnimator.PlayVfxDelegate)Delegate.Combine(aIAnimator3.OnPlayVfx, new AIAnimator.PlayVfxDelegate(PlayVfx));
			AIAnimator aIAnimator4 = m_host.aiAnimator;
			aIAnimator4.OnStopVfx = (AIAnimator.StopVfxDelegate)Delegate.Combine(aIAnimator4.OnStopVfx, new AIAnimator.StopVfxDelegate(StopVfx));
			m_host.healthHaver.OnPreDeath += OnHostPreDeath;
		}
	}

	private void OnHostPreDeath(Vector2 deathDir)
	{
		base.healthHaver.ApplyDamage(100000f, Vector2.zero, "Mirror Host Death", CoreDamageTypes.None, DamageCategory.Unstoppable);
	}

	private void PlayUntilFinished(string name, bool suppressHitStates, string overrideHitState, float warpClipDuration, bool skipChildAnimators)
	{
		if (base.healthHaver.IsAlive && MirrorAnimations.Contains(name))
		{
			base.aiAnimator.PlayUntilFinished(name, suppressHitStates, overrideHitState, warpClipDuration, skipChildAnimators);
		}
	}

	private void EndAnimationIf(string name)
	{
		if (base.healthHaver.IsAlive)
		{
			base.aiAnimator.EndAnimationIf(name);
		}
	}

	private void PlayVfx(string name, Vector2? sourceNormal, Vector2? sourceVelocity, Vector2? position)
	{
		if (base.healthHaver.IsAlive && MirrorAnimations.Contains(name))
		{
			base.aiAnimator.PlayVfx(name, sourceNormal, sourceVelocity, position);
		}
	}

	private void StopVfx(string name)
	{
		if (base.healthHaver.IsAlive && MirrorAnimations.Contains(name))
		{
			base.aiAnimator.StopVfx(name);
		}
	}
}
