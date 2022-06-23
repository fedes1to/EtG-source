using System;
using FullInspector;
using UnityEngine;

[InspectorDropdownName("Bosses/Meduzi/StoneEyesBehavior")]
public class MeduziStoneEyesBehavior : BasicAttackBehavior
{
	private enum State
	{
		Idle,
		WaitingToFire,
		Firing
	}

	public GameObject shootPoint;

	public float distortionMaxRadius = 20f;

	public float distortionDuration = 1.5f;

	public float stoneDuration = 3f;

	[InspectorCategory("Visuals")]
	public string anim;

	[InspectorCategory("Visuals")]
	public float distortionIntensity = 0.5f;

	[InspectorCategory("Visuals")]
	public float distortionThickness = 0.04f;

	private State m_state;

	private Vector2 m_distortionCenter;

	private float m_timer;

	private float m_prevWaveDist;

	public override void Start()
	{
		base.Start();
		tk2dSpriteAnimator spriteAnimator = m_aiAnimator.spriteAnimator;
		spriteAnimator.AnimationEventTriggered = (Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip, int>)Delegate.Combine(spriteAnimator.AnimationEventTriggered, new Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip, int>(AnimationEventTriggered));
	}

	public override void Upkeep()
	{
		base.Upkeep();
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
		m_state = State.WaitingToFire;
		m_aiActor.ClearPath();
		m_updateEveryFrame = true;
		return BehaviorResult.RunContinuous;
	}

	public override ContinuousBehaviorResult ContinuousUpdate()
	{
		base.ContinuousUpdate();
		if (m_state == State.Firing)
		{
			m_timer -= BraveTime.DeltaTime;
			float num = BraveMathCollege.LinearToSmoothStepInterpolate(0f, distortionMaxRadius, 1f - m_timer / distortionDuration);
			for (int i = 0; i < GameManager.Instance.AllPlayers.Length; i++)
			{
				PlayerController playerController = GameManager.Instance.AllPlayers[i];
				if (playerController.healthHaver.IsDead || playerController.spriteAnimator.QueryInvulnerabilityFrame() || !playerController.healthHaver.IsVulnerable)
				{
					continue;
				}
				Vector2 unitCenter = playerController.specRigidbody.GetUnitCenter(ColliderType.HitBox);
				float num2 = Vector2.Distance(unitCenter, m_distortionCenter);
				if (!(num2 < m_prevWaveDist - 0.25f) && !(num2 > num + 0.25f))
				{
					float b = (unitCenter - m_distortionCenter).ToAngle();
					if (!(BraveMathCollege.AbsAngleBetween(playerController.FacingDirection, b) < 45f))
					{
						playerController.CurrentStoneGunTimer = stoneDuration;
					}
				}
			}
			m_prevWaveDist = num;
		}
		if (m_aiAnimator.IsPlaying(anim) || m_timer > 0f)
		{
			return ContinuousBehaviorResult.Continue;
		}
		return ContinuousBehaviorResult.Finished;
	}

	public override void EndContinuousUpdate()
	{
		base.EndContinuousUpdate();
		m_state = State.Idle;
		m_aiAnimator.EndAnimationIf(anim);
		m_updateEveryFrame = false;
		UpdateCooldowns();
	}

	public override bool IsOverridable()
	{
		return false;
	}

	private void AnimationEventTriggered(tk2dSpriteAnimator animator, tk2dSpriteAnimationClip clip, int frame)
	{
		if (m_state == State.WaitingToFire && clip.GetFrame(frame).eventInfo == "fire")
		{
			m_distortionCenter = shootPoint.transform.position.XY();
			Exploder.DoDistortionWave(m_distortionCenter, distortionIntensity, distortionThickness, distortionMaxRadius, distortionDuration);
			m_timer = distortionDuration - BraveTime.DeltaTime;
			m_state = State.Firing;
			m_prevWaveDist = 0f;
		}
	}
}
