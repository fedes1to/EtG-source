using System.Collections;
using UnityEngine;

public class BossFinalConvictHegemonyReinforceDoer : CustomReinforceDoer
{
	public GameObject ropeVfx;

	private bool m_isFinished;

	public override bool IsFinished
	{
		get
		{
			return m_isFinished;
		}
	}

	public override void StartIntro()
	{
		StartCoroutine(DoIntro());
	}

	private IEnumerator DoIntro()
	{
		Vector2 spawnPos = base.transform.position;
		bool faceRight = BraveUtility.RandomBool();
		base.specRigidbody.Initialize();
		SpawnManager.SpawnVFX(ropeVfx, base.specRigidbody.UnitCenter + new Vector2((!faceRight) ? (-1) : (-2), 0f), Quaternion.identity);
		base.aiActor.invisibleUntilAwaken = true;
		bool cachedCollideWithOthers = base.specRigidbody.CollideWithOthers;
		base.specRigidbody.CollideWithOthers = false;
		base.aiActor.IsGone = true;
		if ((bool)base.behaviorSpeculator)
		{
			base.behaviorSpeculator.enabled = false;
		}
		base.healthHaver.IsVulnerable = false;
		base.aiActor.ToggleRenderers(false);
		if ((bool)base.aiShooter)
		{
			base.aiShooter.ToggleGunAndHandRenderers(false, "BossFinalConvictHegemonyReinforceDoer");
		}
		float elapsed2 = 0f;
		for (float duration2 = 0.5f; elapsed2 < duration2; elapsed2 += GameManager.INVARIANT_DELTA_TIME)
		{
			if ((bool)base.aiShooter)
			{
				base.aiShooter.ToggleGunAndHandRenderers(false, "BossFinalConvictHegemonyReinforceDoer");
			}
			yield return null;
		}
		yield return new WaitForSeconds(0.5f);
		base.aiActor.invisibleUntilAwaken = false;
		base.aiActor.ToggleRenderers(true);
		base.aiAnimator.LockFacingDirection = true;
		base.aiAnimator.FacingDirection = ((!faceRight) ? 180 : 0);
		base.aiAnimator.PlayUntilCancelled("rappel");
		elapsed2 = 0f;
		for (float duration2 = 2f; elapsed2 < duration2; elapsed2 += GameManager.INVARIANT_DELTA_TIME)
		{
			base.transform.position = spawnPos + new Vector2(0f, BraveMathCollege.LinearToSmoothStepInterpolate(7f, 0f, elapsed2 / duration2));
			if ((bool)base.aiShooter)
			{
				base.aiShooter.ToggleGunAndHandRenderers(false, "BossFinalConvictHegemonyReinforceDoer");
			}
			yield return null;
		}
		base.transform.position = spawnPos;
		base.aiAnimator.LockFacingDirection = false;
		base.aiAnimator.EndAnimationIf("rappel");
		base.specRigidbody.CollideWithOthers = cachedCollideWithOthers;
		base.aiActor.IsGone = false;
		base.aiActor.State = AIActor.ActorState.Normal;
		if ((bool)base.behaviorSpeculator)
		{
			base.behaviorSpeculator.enabled = true;
		}
		base.healthHaver.IsVulnerable = true;
		if ((bool)base.aiShooter)
		{
			base.aiShooter.ToggleGunAndHandRenderers(true, "BossFinalConvictHegemonyReinforceDoer");
		}
		base.aiActor.HasBeenEngaged = true;
		m_isFinished = true;
	}
}
