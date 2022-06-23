using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(GenericIntroDoer))]
public class BlobulordIntroDoer : SpecificIntroDoer
{
	public Transform particleTransform;

	private bool m_initialized;

	private bool m_finished;

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
		if (!m_initialized && base.aiActor.enabled && (bool)base.aiActor.ShadowObject)
		{
			m_shadowSprite = base.aiActor.ShadowObject.GetComponent<tk2dBaseSprite>();
			m_shadowSprite.color = m_shadowSprite.color.WithAlpha(0f);
			m_initialized = true;
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}

	public override void PlayerWalkedIn(PlayerController player, List<tk2dSpriteAnimator> animators)
	{
		base.aiAnimator.LockFacingDirection = true;
		base.aiAnimator.FacingDirection = -90f;
		base.aiAnimator.PlayUntilCancelled("preintro");
		if ((bool)m_shadowSprite)
		{
			m_shadowSprite.color = m_shadowSprite.color.WithAlpha(0f);
		}
	}

	public override void StartIntro(List<tk2dSpriteAnimator> animators)
	{
		base.aiAnimator.PlayUntilFinished("intro");
		StartCoroutine(DoIntro());
	}

	public override void EndIntro()
	{
		m_finished = true;
		StopAllCoroutines();
		m_shadowSprite.color = m_shadowSprite.color.WithAlpha(1f);
		base.aiAnimator.LockFacingDirection = false;
		base.aiAnimator.EndAnimation();
	}

	private IEnumerator DoIntro()
	{
		float elapsed3 = 0f;
		for (float duration3 = 0.33f; elapsed3 < duration3; elapsed3 += GameManager.INVARIANT_DELTA_TIME)
		{
			yield return null;
		}
		elapsed3 = 0f;
		for (float duration3 = 0.66f; elapsed3 < duration3; elapsed3 += GameManager.INVARIANT_DELTA_TIME)
		{
			m_shadowSprite.color = m_shadowSprite.color.WithAlpha(elapsed3 / duration3);
			yield return null;
		}
		m_shadowSprite.color = m_shadowSprite.color.WithAlpha(1f);
		elapsed3 = 0f;
		for (float duration3 = 4.5f; elapsed3 < duration3; elapsed3 += GameManager.INVARIANT_DELTA_TIME)
		{
			yield return null;
		}
		m_finished = true;
	}
}
