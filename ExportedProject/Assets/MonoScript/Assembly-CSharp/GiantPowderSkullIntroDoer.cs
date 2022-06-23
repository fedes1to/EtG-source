using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(GenericIntroDoer))]
public class GiantPowderSkullIntroDoer : SpecificIntroDoer
{
	[CurveRange(0f, 0f, 1f, 1f)]
	public AnimationCurve emissionRate;

	private bool m_initialized;

	private bool m_finished;

	private ParticleSystem m_mainParticleSystem;

	private ParticleSystem m_trailParticleSystem;

	private float m_startParticleRate;

	private tk2dBaseSprite m_shadowSprite;

	public override bool IsIntroFinished
	{
		get
		{
			return m_finished;
		}
	}

	public void Update()
	{
		if (!m_initialized && (bool)base.aiActor.ShadowObject && m_mainParticleSystem != null)
		{
			m_shadowSprite = base.aiActor.ShadowObject.GetComponent<tk2dBaseSprite>();
			SpriteOutlineManager.ToggleOutlineRenderers(base.sprite, false);
			base.aiActor.ToggleRenderers(false);
			m_shadowSprite.renderer.enabled = false;
			m_mainParticleSystem.GetComponent<Renderer>().enabled = true;
			m_initialized = true;
			Debug.Log("INITIALIZED!");
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}

	public override void PlayerWalkedIn(PlayerController player, List<tk2dSpriteAnimator> animators)
	{
		StartCoroutine(RunParticleSystems());
		AkSoundEngine.PostEvent("Play_ENM_cannonball_intro_01", base.gameObject);
		base.aiAnimator.LockFacingDirection = true;
		base.aiAnimator.FacingDirection = -90f;
	}

	public override void StartIntro(List<tk2dSpriteAnimator> animators)
	{
		StartCoroutine(DoIntro());
	}

	public override void OnBossCard()
	{
		m_shadowSprite.renderer.enabled = true;
	}

	public override void EndIntro()
	{
		m_finished = true;
		StopAllCoroutines();
		SpriteOutlineManager.ToggleOutlineRenderers(base.sprite, true);
		base.aiActor.ToggleRenderers(true);
		m_shadowSprite.renderer.enabled = true;
		base.aiAnimator.LockFacingDirection = false;
		base.aiAnimator.EndAnimation();
		BraveUtility.SetEmissionRate(m_mainParticleSystem, m_startParticleRate);
		BraveUtility.EnableEmission(m_mainParticleSystem, true);
		m_mainParticleSystem.Play();
		BraveUtility.EnableEmission(m_trailParticleSystem, true);
	}

	private IEnumerator RunParticleSystems()
	{
		PowderSkullParticleController particleController = base.aiActor.GetComponentInChildren<PowderSkullParticleController>();
		m_mainParticleSystem = particleController.GetComponent<ParticleSystem>();
		m_trailParticleSystem = particleController.RotationChild.GetComponentInChildren<ParticleSystem>();
		m_startParticleRate = m_mainParticleSystem.emission.rate.constant;
		m_mainParticleSystem.GetComponent<Renderer>().enabled = true;
		BraveUtility.SetEmissionRate(m_mainParticleSystem, 0f);
		m_mainParticleSystem.Clear();
		BraveUtility.EnableEmission(m_trailParticleSystem, false);
		m_trailParticleSystem.Clear();
		float t = 0f;
		float duration = 6f;
		while (!m_finished)
		{
			if (t < duration)
			{
				BraveUtility.SetEmissionRate(m_mainParticleSystem, Mathf.Lerp(0f, m_startParticleRate, emissionRate.Evaluate(t / duration)));
			}
			m_mainParticleSystem.Simulate(GameManager.INVARIANT_DELTA_TIME, false, false);
			yield return null;
			t += GameManager.INVARIANT_DELTA_TIME;
		}
	}

	private IEnumerator DoIntro()
	{
		float elapsed2 = 0f;
		for (float duration2 = 2f; elapsed2 < duration2; elapsed2 += GameManager.INVARIANT_DELTA_TIME)
		{
			yield return null;
		}
		SpriteOutlineManager.ToggleOutlineRenderers(base.sprite, true);
		base.aiActor.ToggleRenderers(true);
		m_shadowSprite.renderer.enabled = false;
		base.aiAnimator.PlayUntilFinished("intro");
		elapsed2 = 0f;
		for (float duration2 = 4f; elapsed2 < duration2; elapsed2 += GameManager.INVARIANT_DELTA_TIME)
		{
			yield return null;
		}
		m_finished = true;
	}
}
