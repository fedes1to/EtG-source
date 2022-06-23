using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(GenericIntroDoer))]
public class TankTreaderIntroDoer : SpecificIntroDoer
{
	public BodyPartController mainGun;

	public AIAnimator guy;

	public tk2dSpriteAnimator hatch;

	private bool m_finished;

	private ParticleSystem[] m_exhaustParticleSystems;

	public override bool IsIntroFinished
	{
		get
		{
			return m_finished;
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}

	public override void PlayerWalkedIn(PlayerController player, List<tk2dSpriteAnimator> animators)
	{
		mainGun.enabled = false;
		mainGun.aiAnimator.LockFacingDirection = true;
		mainGun.aiAnimator.FacingDirection = -90f;
		mainGun.aiAnimator.Update();
		base.aiAnimator.LockFacingDirection = true;
		base.aiAnimator.FacingDirection = -90f;
		base.aiAnimator.Update();
		m_exhaustParticleSystems = GetComponentsInChildren<ParticleSystem>();
		ParticleSystem[] exhaustParticleSystems = m_exhaustParticleSystems;
		foreach (ParticleSystem particleSystem in exhaustParticleSystems)
		{
			BraveUtility.EnableEmission(particleSystem, false);
			particleSystem.Clear();
			particleSystem.GetComponent<Renderer>().enabled = false;
		}
	}

	public override void StartIntro(List<tk2dSpriteAnimator> animators)
	{
		animators.Add(guy.spriteAnimator);
		animators.Add(hatch);
		StartCoroutine(DoIntro());
		AkSoundEngine.PostEvent("Play_BOSS_tank_idle_01", base.gameObject);
	}

	public override void OnCleanup()
	{
		mainGun.enabled = true;
		mainGun.aiAnimator.LockFacingDirection = false;
		guy.EndAnimationIf("intro");
		hatch.Play("hatch_closed");
		TankTreaderMiniTurretController[] componentsInChildren = GetComponentsInChildren<TankTreaderMiniTurretController>();
		foreach (TankTreaderMiniTurretController tankTreaderMiniTurretController in componentsInChildren)
		{
			tankTreaderMiniTurretController.enabled = true;
		}
		ParticleSystem[] exhaustParticleSystems = m_exhaustParticleSystems;
		foreach (ParticleSystem particleSystem in exhaustParticleSystems)
		{
			BraveUtility.EnableEmission(particleSystem, true);
			particleSystem.GetComponent<Renderer>().enabled = true;
		}
	}

	public override void OnBossCard()
	{
	}

	public override void EndIntro()
	{
	}

	private IEnumerator DoIntro()
	{
		hatch.Play("hatch_intro");
		float elapsed2 = 0f;
		for (float duration2 = 0.2f; elapsed2 < duration2; elapsed2 += GameManager.INVARIANT_DELTA_TIME)
		{
			yield return null;
		}
		guy.gameObject.SetActive(true);
		guy.PlayUntilCancelled("intro");
		elapsed2 = 0f;
		for (float duration2 = 2.4f; elapsed2 < duration2; elapsed2 += GameManager.INVARIANT_DELTA_TIME)
		{
			yield return null;
		}
		m_finished = true;
	}
}
