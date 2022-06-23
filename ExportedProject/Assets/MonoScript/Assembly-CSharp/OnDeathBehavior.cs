using System;
using System.Collections;
using UnityEngine;

public abstract class OnDeathBehavior : BraveBehaviour
{
	public enum DeathType
	{
		PreDeath,
		Death,
		DeathAnimTrigger
	}

	public DeathType deathType = DeathType.Death;

	[ShowInInspectorIf("deathType", 0, false)]
	public float preDeathDelay;

	[ShowInInspectorIf("deathType", 2, false)]
	public string triggerName;

	private Vector2 m_deathDir;

	public virtual void Start()
	{
		if ((bool)base.healthHaver)
		{
			base.healthHaver.OnPreDeath += OnPreDeath;
			if (deathType == DeathType.Death)
			{
				base.healthHaver.OnDeath += OnDeath;
			}
			else if (deathType == DeathType.DeathAnimTrigger)
			{
				tk2dSpriteAnimator obj = base.spriteAnimator;
				obj.AnimationEventTriggered = (Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip, int>)Delegate.Combine(obj.AnimationEventTriggered, new Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip, int>(AnimationEventTriggered));
			}
		}
	}

	protected override void OnDestroy()
	{
		if ((bool)base.healthHaver)
		{
			if (deathType == DeathType.Death)
			{
				base.healthHaver.OnDeath -= OnDeath;
			}
			else if (deathType == DeathType.DeathAnimTrigger)
			{
				tk2dSpriteAnimator obj = base.spriteAnimator;
				obj.AnimationEventTriggered = (Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip, int>)Delegate.Remove(obj.AnimationEventTriggered, new Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip, int>(AnimationEventTriggered));
			}
		}
		base.OnDestroy();
	}

	protected abstract void OnTrigger(Vector2 dirVec);

	private void OnPreDeath(Vector2 dirVec)
	{
		m_deathDir = dirVec;
		if (deathType == DeathType.PreDeath)
		{
			if (preDeathDelay > 0f)
			{
				StartCoroutine(DelayedOnTriggerCR(preDeathDelay));
			}
			else
			{
				OnTrigger(m_deathDir);
			}
		}
	}

	private void OnDeath(Vector2 dirVec)
	{
		OnTrigger(m_deathDir);
	}

	private void AnimationEventTriggered(tk2dSpriteAnimator animator, tk2dSpriteAnimationClip clip, int frame)
	{
		if (base.healthHaver.IsDead && clip.GetFrame(frame).eventInfo == triggerName)
		{
			OnTrigger(m_deathDir);
		}
	}

	private IEnumerator DelayedOnTriggerCR(float delay)
	{
		yield return new WaitForSeconds(delay);
		OnTrigger(m_deathDir);
	}
}
