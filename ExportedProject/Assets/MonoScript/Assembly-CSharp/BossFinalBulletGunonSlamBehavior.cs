using System;
using System.Collections.Generic;
using FullInspector;
using UnityEngine;

[InspectorDropdownName("Bosses/BossFinalBullet/GunonSlamBehavior")]
public class BossFinalBulletGunonSlamBehavior : BasicAttackBehavior
{
	public int numTraps = 6;

	[InspectorCategory("Visuals")]
	public string anim;

	private List<PitTrapController> m_traps;

	private bool m_slammed;

	public override void Start()
	{
		base.Start();
		tk2dSpriteAnimator spriteAnimator = m_aiAnimator.spriteAnimator;
		spriteAnimator.AnimationEventTriggered = (Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip, int>)Delegate.Combine(spriteAnimator.AnimationEventTriggered, new Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip, int>(AnimationEventTriggered));
		m_traps = new List<PitTrapController>(UnityEngine.Object.FindObjectsOfType<PitTrapController>());
	}

	public override BehaviorResult Update()
	{
		BehaviorResult behaviorResult = base.Update();
		if (behaviorResult != 0)
		{
			return behaviorResult;
		}
		if (!IsReady())
		{
			return BehaviorResult.Continue;
		}
		m_aiAnimator.PlayUntilFinished(anim, true);
		m_slammed = false;
		m_aiActor.ClearPath();
		m_updateEveryFrame = true;
		return BehaviorResult.RunContinuous;
	}

	public override ContinuousBehaviorResult ContinuousUpdate()
	{
		base.ContinuousUpdate();
		if (!m_aiAnimator.IsPlaying(anim))
		{
			if (!m_slammed)
			{
				Slam();
			}
			return ContinuousBehaviorResult.Finished;
		}
		return ContinuousBehaviorResult.Continue;
	}

	public override void EndContinuousUpdate()
	{
		base.EndContinuousUpdate();
		m_aiAnimator.EndAnimationIf(anim);
		m_updateEveryFrame = false;
		UpdateCooldowns();
	}

	public override bool IsOverridable()
	{
		return false;
	}

	public override bool IsReady()
	{
		if (!base.IsReady())
		{
			return false;
		}
		return m_traps.Count > MinTrapsLeft();
	}

	private void AnimationEventTriggered(tk2dSpriteAnimator animator, tk2dSpriteAnimationClip clip, int frame)
	{
		if (!m_slammed && clip.GetFrame(frame).eventInfo == "fire")
		{
			Slam();
		}
	}

	private void Slam()
	{
		int num = Mathf.Min(m_traps.Count - MinTrapsLeft(), 2);
		for (int i = 0; i < num; i++)
		{
			int index = UnityEngine.Random.Range(0, m_traps.Count);
			m_traps[index].Trigger();
			m_traps.RemoveAt(index);
		}
	}

	private int MinTrapsLeft()
	{
		return Mathf.RoundToInt(Mathf.Lerp(numTraps, 0f, Mathf.InverseLerp(1f, 0.33f, m_aiActor.healthHaver.GetCurrentHealthPercentage())));
	}
}
