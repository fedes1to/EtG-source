using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(GenericIntroDoer))]
public class BashelliskIntroDoer : SpecificIntroDoer
{
	private enum State
	{
		Idle,
		Playing,
		Finished
	}

	private State m_state;

	private BashelliskHeadController m_head;

	public override bool IsIntroFinished
	{
		get
		{
			return m_state == State.Finished;
		}
	}

	private void Start()
	{
		m_head = GetComponent<BashelliskHeadController>();
	}

	private void Update()
	{
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}

	public override void PlayerWalkedIn(PlayerController player, List<tk2dSpriteAnimator> animators)
	{
		AkSoundEngine.PostEvent("Play_BOSS_bashellisk_move_02", base.gameObject);
		m_head.aiAnimator.LockFacingDirection = true;
		m_head.aiAnimator.FacingDirection = -90f;
		m_head.aiAnimator.Update();
		animators.Add(m_head.spriteAnimator);
		GetComponent<GenericIntroDoer>().SkipFinalizeAnimation = true;
		StartCoroutine(PlayIntro());
	}

	public override void StartIntro(List<tk2dSpriteAnimator> animators)
	{
	}

	public override void EndIntro()
	{
		StopAllCoroutines();
		m_head.specRigidbody.Reinitialize();
		m_head.ReinitMovementDirection = true;
	}

	public override void OnCleanup()
	{
		base.behaviorSpeculator.enabled = true;
	}

	private IEnumerator PlayIntro()
	{
		m_state = State.Playing;
		Vector2 startPos = m_head.transform.position;
		m_head.aiAnimator.LockFacingDirection = true;
		float elapsed2 = 0f;
		for (float duration2 = 4f; elapsed2 < duration2; elapsed2 += GameManager.INVARIANT_DELTA_TIME)
		{
			float angle = Mathf.Repeat(elapsed2 / 2.666f, 1f) * 360f - 90f;
			float r = 4f + 4f * Mathf.Cos(2f * angle * ((float)Math.PI / 180f));
			float drawAngle = ((!(angle > 90f)) ? angle : (360f - angle));
			Vector2 lastPos = m_head.transform.position;
			Vector2 newPos = startPos + BraveMathCollege.DegreesToVector(drawAngle, r);
			m_head.transform.position = newPos;
			m_head.aiAnimator.FacingDirection = (newPos - lastPos).ToAngle();
			m_head.aiAnimator.Update();
			m_head.OnPostRigidbodyMovement();
			yield return null;
		}
		m_head.aiAnimator.FacingDirection = -90f;
		m_head.aiAnimator.PlayUntilFinished("intro");
		m_head.aiAnimator.Update();
		elapsed2 = 0f;
		for (float duration2 = 2f; elapsed2 < duration2; elapsed2 += GameManager.INVARIANT_DELTA_TIME)
		{
			yield return null;
		}
		m_head.aiAnimator.EndAnimationIf("intro");
		m_state = State.Finished;
	}
}
