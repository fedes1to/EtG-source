using FullInspector;
using UnityEngine;

[InspectorDropdownName("Bosses/DraGun/NearDeathBehavior")]
public class DraGunNearDeathBehavior : BasicAttackBehavior
{
	private enum State
	{
		Inactive,
		Attacking,
		WeakIntro,
		Weak,
		WeakOutro
	}

	public float DamageTime = 5f;

	public AIAnimator LeftHandAnimator;

	public AIAnimator RightHandAnimator;

	public AIAnimator WingsAnimator;

	public AIAnimator EyesAnimator;

	public AttackBehaviorGroup Attacks;

	private DraGunController m_dragun;

	private DraGunHeadController m_head;

	private AIAnimator m_headAnimator;

	private AutoAimTarget m_heartAutoAimTarget;

	private HealthHaver m_healthHaver;

	private HitEffectHandler m_hitEffectHandler;

	private State m_state;

	private float m_timer;

	public override void Start()
	{
		base.Start();
		m_dragun = m_aiActor.GetComponent<DraGunController>();
		m_head = m_dragun.head;
		m_headAnimator = m_head.aiAnimator;
		m_heartAutoAimTarget = m_dragun.GetComponentsInChildren<AutoAimTarget>(true)[0];
		m_heartAutoAimTarget.enabled = false;
		m_healthHaver = m_aiActor.healthHaver;
		m_hitEffectHandler = m_aiActor.hitEffectHandler;
		Attacks.Start();
	}

	public override void Upkeep()
	{
		base.Upkeep();
		if (m_state != 0)
		{
			DecrementTimer(ref m_timer);
		}
		Attacks.Upkeep();
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
		m_state = State.Attacking;
		EyesAnimator.PlayUntilFinished("eyes_idle");
		Attacks.Update();
		SilencerInstance.s_MaxRadiusLimiter = 8f;
		m_healthHaver.IsVulnerable = false;
		m_updateEveryFrame = true;
		return BehaviorResult.RunContinuous;
	}

	public override ContinuousBehaviorResult ContinuousUpdate()
	{
		base.ContinuousUpdate();
		if (m_state == State.Attacking)
		{
			ContinuousBehaviorResult continuousBehaviorResult = Attacks.ContinuousUpdate();
			if (continuousBehaviorResult == ContinuousBehaviorResult.Finished)
			{
				Attacks.EndContinuousUpdate();
				SilencerInstance.s_MaxRadiusLimiter = null;
				m_state = State.WeakIntro;
				EyesAnimator.PlayUntilFinished("eyes_out");
				m_aiAnimator.PlayUntilCancelled("weak_intro");
				LeftHandAnimator.PlayUntilCancelled("weak_idle");
				RightHandAnimator.PlayUntilCancelled("weak_idle");
				WingsAnimator.PlayUntilCancelled("weak_idle");
				m_head.OverrideDesiredPosition = m_aiActor.specRigidbody.UnitCenter + new Vector2(-3f, 7f);
				m_heartAutoAimTarget.enabled = true;
			}
		}
		else if (m_state == State.WeakIntro)
		{
			if (!m_aiAnimator.IsPlaying("weak_intro"))
			{
				m_state = State.Weak;
				m_aiAnimator.PlayUntilCancelled("weak_idle");
				m_headAnimator.PlayUntilCancelled("weak_idle");
				m_healthHaver.IsVulnerable = true;
				m_hitEffectHandler.additionalHitEffects[0].chance = 1f;
				m_timer = DamageTime;
				if (m_dragun.MaybeConvertToGold())
				{
					m_timer = 100000f;
					m_aiActor.healthHaver.minimumHealth = 1f;
				}
			}
		}
		else if (m_state == State.Weak)
		{
			if (m_timer <= 0f)
			{
				m_state = State.WeakOutro;
				m_aiAnimator.PlayUntilCancelled("weak_outro");
				m_headAnimator.EndAnimation();
				EyesAnimator.PlayUntilFinished("eyes_idle");
				m_healthHaver.IsVulnerable = false;
				m_hitEffectHandler.additionalHitEffects[0].chance = 0f;
				m_head.OverrideDesiredPosition = null;
			}
		}
		else if (m_state == State.WeakOutro && !m_aiAnimator.IsPlaying("weak_outro"))
		{
			m_state = State.Attacking;
			m_aiAnimator.EndAnimation();
			LeftHandAnimator.EndAnimation();
			RightHandAnimator.EndAnimation();
			WingsAnimator.EndAnimation();
			Attacks.Update();
			m_heartAutoAimTarget.enabled = false;
			SilencerInstance.s_MaxRadiusLimiter = 8f;
		}
		return ContinuousBehaviorResult.Continue;
	}

	public override void EndContinuousUpdate()
	{
		base.EndContinuousUpdate();
		m_state = State.Inactive;
		m_aiAnimator.EndAnimation();
		m_headAnimator.EndAnimation();
		LeftHandAnimator.EndAnimation();
		RightHandAnimator.EndAnimation();
		WingsAnimator.EndAnimation();
		m_updateEveryFrame = false;
		UpdateCooldowns();
		m_heartAutoAimTarget.enabled = false;
		SilencerInstance.s_MaxRadiusLimiter = null;
	}

	public override void Init(GameObject gameObject, AIActor aiActor, AIShooter aiShooter)
	{
		base.Init(gameObject, aiActor, aiShooter);
		Attacks.Init(gameObject, aiActor, aiShooter);
	}

	public override void SetDeltaTime(float deltaTime)
	{
		base.SetDeltaTime(deltaTime);
		Attacks.SetDeltaTime(deltaTime);
	}

	public override bool IsReady()
	{
		return m_dragun.IsNearDeath && base.IsReady();
	}

	public override bool IsOverridable()
	{
		return false;
	}

	public override void OnActorPreDeath()
	{
		SilencerInstance.s_MaxRadiusLimiter = null;
		base.OnActorPreDeath();
		Attacks.OnActorPreDeath();
	}
}
