using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(GenericIntroDoer))]
public class MeduziIntroDoer : SpecificIntroDoer
{
	private bool m_isFinished;

	public override bool IsIntroFinished
	{
		get
		{
			return true;
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}

	public override void StartIntro(List<tk2dSpriteAnimator> animators)
	{
		StartCoroutine(DoIntro());
	}

	public IEnumerator DoIntro()
	{
		tk2dBaseSprite m_shadowSprite = base.aiActor.ShadowObject.GetComponent<tk2dBaseSprite>();
		float elapsed = 0f;
		for (float duration = 4f; elapsed < duration; elapsed += GameManager.INVARIANT_DELTA_TIME)
		{
			if (m_isFinished)
			{
				break;
			}
			if (elapsed > 3.33f)
			{
				m_shadowSprite.color = m_shadowSprite.color.WithAlpha(Mathf.InverseLerp(3.33f, 3.75f, elapsed));
			}
			else if (elapsed > 2f)
			{
				m_shadowSprite.color = m_shadowSprite.color.WithAlpha(Mathf.InverseLerp(2.75f, 2f, elapsed));
			}
			yield return null;
		}
		m_shadowSprite.color = m_shadowSprite.color.WithAlpha(1f);
		m_isFinished = true;
	}

	public override void EndIntro()
	{
		m_isFinished = true;
	}
}
