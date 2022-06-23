using UnityEngine;

public class LaserSightController : BraveBehaviour
{
	public bool DoFlash;

	[CurveRange(0f, 0f, 1f, 1f)]
	public AnimationCurve flashCurve;

	public bool DoAnim;

	[CheckAnimation(null)]
	public string idleAnim;

	[CheckAnimation(null)]
	public string preFireAnim;

	private tk2dSpriteAnimationClip m_idleClip;

	private tk2dSpriteAnimationClip m_preFireClip;

	private float m_preFireLength;

	public void Start()
	{
		if ((bool)base.spriteAnimator)
		{
			m_idleClip = base.spriteAnimator.GetClipByName(idleAnim);
			m_preFireClip = base.spriteAnimator.GetClipByName(preFireAnim);
			m_preFireLength = m_preFireClip.BaseClipLength;
		}
	}

	public void UpdateCountdown(float m_prefireTimer, float PreFireLaserTime)
	{
		base.renderer.enabled = true;
		if (DoFlash)
		{
			float time = 1f - m_prefireTimer / PreFireLaserTime;
			base.renderer.enabled = flashCurve.Evaluate(time) > 0.5f;
		}
		if (DoAnim && (bool)base.spriteAnimator)
		{
			if (m_prefireTimer < m_preFireLength)
			{
				base.spriteAnimator.Play(m_preFireClip, m_preFireLength - m_prefireTimer, m_preFireClip.fps);
			}
			else
			{
				base.spriteAnimator.Play(m_idleClip);
			}
		}
	}

	public void ResetCountdown()
	{
		base.renderer.enabled = false;
		if (DoAnim && (bool)base.spriteAnimator)
		{
			base.spriteAnimator.Play(m_idleClip);
		}
	}
}
