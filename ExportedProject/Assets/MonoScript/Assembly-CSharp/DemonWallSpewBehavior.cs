using System;
using FullInspector;
using UnityEngine;

[InspectorDropdownName("Bosses/DemonWall/SpewBehavior")]
public class DemonWallSpewBehavior : BasicAttackBehavior
{
	public enum GoopType
	{
		Arc,
		Line
	}

	public Transform goopPoint;

	public GoopDefinition goopToUse;

	public GoopType goopType;

	[InspectorShowIf("ShowArcParams")]
	public float goopConeLength = 5f;

	[InspectorShowIf("ShowArcParams")]
	public float goopConeArc = 45f;

	[InspectorShowIf("ShowArcParams")]
	public AnimationCurve goopCurve;

	[InspectorShowIf("ShowLineParams")]
	public float goopLength = 5f;

	[InspectorShowIf("ShowLineParams")]
	public float goopRadius = 5f;

	public float goopDuration = 0.5f;

	public bool igniteGoop;

	[InspectorShowIf("igniteGoop")]
	public float igniteDelay = 1f;

	[InspectorCategory("Attack")]
	public GameObject ShootPoint;

	[InspectorCategory("Attack")]
	public BulletScriptSelector BulletScript;

	[InspectorCategory("Visuals")]
	public string spewAnimation;

	[InspectorCategory("Visuals")]
	public GameObject spewSprite;

	private BulletScriptSource m_bulletSource;

	private float m_goopTimer;

	private float m_igniteTimer;

	private bool ShowArcParams()
	{
		return goopType == GoopType.Arc;
	}

	private bool ShowLineParams()
	{
		return goopType == GoopType.Line;
	}

	public override void Start()
	{
		base.Start();
		tk2dSpriteAnimator spriteAnimator = m_aiActor.spriteAnimator;
		spriteAnimator.AnimationEventTriggered = (Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip, int>)Delegate.Combine(spriteAnimator.AnimationEventTriggered, new Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip, int>(AnimationEventTriggered));
	}

	public override void Upkeep()
	{
		base.Upkeep();
		DecrementTimer(ref m_goopTimer);
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
		m_aiActor.ClearPath();
		m_aiActor.BehaviorVelocity = Vector2.zero;
		m_aiAnimator.PlayUntilFinished(spewAnimation);
		m_updateEveryFrame = true;
		return BehaviorResult.RunContinuous;
	}

	public override ContinuousBehaviorResult ContinuousUpdate()
	{
		base.ContinuousUpdate();
		if (igniteGoop && m_igniteTimer > 0f)
		{
			m_igniteTimer -= m_deltaTime;
			if (m_igniteTimer <= 0f)
			{
				DeadlyDeadlyGoopManager.IgniteGoopsCircle(goopPoint.transform.position + new Vector3(0f, -0.5f), 0.5f);
			}
		}
		if (m_goopTimer > 0f || m_aiAnimator.IsPlaying(spewAnimation))
		{
			return ContinuousBehaviorResult.Continue;
		}
		if (m_bulletSource != null && !m_bulletSource.IsEnded)
		{
			return ContinuousBehaviorResult.Continue;
		}
		return ContinuousBehaviorResult.Finished;
	}

	public override void EndContinuousUpdate()
	{
		base.EndContinuousUpdate();
		if ((bool)m_bulletSource && !m_bulletSource.IsEnded)
		{
			m_bulletSource.ForceStop();
		}
		m_updateEveryFrame = false;
		UpdateCooldowns();
	}

	private void AnimationEventTriggered(tk2dSpriteAnimator spriteAnimator, tk2dSpriteAnimationClip clip, int frame)
	{
		if (!m_updateEveryFrame)
		{
			return;
		}
		if (clip.GetFrame(frame).eventInfo == "spew")
		{
			spewSprite.SetActive(true);
			spewSprite.GetComponent<SpriteAnimatorKiller>().Restart();
			DeadlyDeadlyGoopManager goopManagerForGoopType = DeadlyDeadlyGoopManager.GetGoopManagerForGoopType(goopToUse);
			if (goopType == GoopType.Arc)
			{
				goopManagerForGoopType.TimedAddGoopArc(goopPoint.transform.position, goopConeLength, goopConeArc, -Vector2.up, goopDuration, goopCurve);
			}
			else
			{
				Vector2 vector = goopPoint.transform.position;
				goopManagerForGoopType.TimedAddGoopLine(vector, vector + new Vector2(0f, 0f - goopLength), goopRadius, goopDuration);
			}
			m_goopTimer = goopDuration;
			m_igniteTimer = igniteDelay;
		}
		if (clip.GetFrame(frame).eventInfo == "fire")
		{
			if (!m_bulletSource)
			{
				m_bulletSource = ShootPoint.GetOrAddComponent<BulletScriptSource>();
			}
			m_bulletSource.BulletManager = m_aiActor.bulletBank;
			m_bulletSource.BulletScript = BulletScript;
			m_bulletSource.Initialize();
		}
	}
}
