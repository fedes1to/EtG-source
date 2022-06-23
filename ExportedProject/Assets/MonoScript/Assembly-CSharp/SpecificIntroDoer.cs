using System.Collections;
using System.Collections.Generic;
using Dungeonator;
using UnityEngine;

public abstract class SpecificIntroDoer : BraveBehaviour
{
	public virtual Vector2? OverrideIntroPosition
	{
		get
		{
			return null;
		}
	}

	public virtual Vector2? OverrideOutroPosition
	{
		get
		{
			return null;
		}
	}

	public virtual string OverrideBossMusicEvent
	{
		get
		{
			return null;
		}
	}

	public virtual bool IsIntroFinished
	{
		get
		{
			return true;
		}
	}

	public virtual IntVector2 OverrideExitBasePosition(DungeonData.Direction directionToWalk, IntVector2 exitBaseCenter)
	{
		return exitBaseCenter;
	}

	public virtual void PlayerWalkedIn(PlayerController player, List<tk2dSpriteAnimator> animators)
	{
	}

	public virtual void OnCameraIntro()
	{
	}

	public virtual void StartIntro(List<tk2dSpriteAnimator> animators)
	{
	}

	public virtual void OnBossCard()
	{
	}

	public virtual void OnCameraOutro()
	{
	}

	public virtual void OnCleanup()
	{
	}

	public virtual void EndIntro()
	{
	}

	public IEnumerator TimeInvariantWait(float duration)
	{
		for (float elapsed = 0f; elapsed < duration; elapsed += GameManager.INVARIANT_DELTA_TIME)
		{
			yield return null;
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}
}
