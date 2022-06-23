using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(GenericIntroDoer))]
public class HighPriestIntroDoer : SpecificIntroDoer
{
	public AIAnimator head;

	private bool m_isMotionRestricted;

	private bool m_finished;

	private int m_minPlayerY;

	public override bool IsIntroFinished
	{
		get
		{
			return m_finished;
		}
	}

	public void Start()
	{
		base.aiActor.ParentRoom.Entered += PlayerEnteredRoom;
		base.aiActor.healthHaver.OnPreDeath += OnPreDeath;
	}

	protected override void OnDestroy()
	{
		tk2dSpriteAnimator obj = base.spriteAnimator;
		obj.AnimationEventTriggered = (Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip, int>)Delegate.Remove(obj.AnimationEventTriggered, new Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip, int>(BodyAnimationEventTriggered));
		tk2dSpriteAnimator obj2 = base.spriteAnimator;
		obj2.AnimationCompleted = (Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip>)Delegate.Remove(obj2.AnimationCompleted, new Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip>(BodyAnimationComplete));
		tk2dSpriteAnimator obj3 = head.spriteAnimator;
		obj3.AnimationCompleted = (Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip>)Delegate.Remove(obj3.AnimationCompleted, new Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip>(HeadAnimationComplete));
		RestrictMotion(false);
		if ((bool)base.aiActor && base.aiActor.ParentRoom != null)
		{
			base.aiActor.ParentRoom.Entered -= PlayerEnteredRoom;
		}
		if ((bool)base.aiActor && (bool)base.aiActor.healthHaver)
		{
			base.aiActor.healthHaver.OnPreDeath -= OnPreDeath;
		}
		base.OnDestroy();
	}

	public override void StartIntro(List<tk2dSpriteAnimator> animators)
	{
		base.aiAnimator.PlayUntilFinished("intro");
		tk2dSpriteAnimator obj = base.spriteAnimator;
		obj.AnimationEventTriggered = (Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip, int>)Delegate.Combine(obj.AnimationEventTriggered, new Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip, int>(BodyAnimationEventTriggered));
		tk2dSpriteAnimator obj2 = base.spriteAnimator;
		obj2.AnimationCompleted = (Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip>)Delegate.Combine(obj2.AnimationCompleted, new Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip>(BodyAnimationComplete));
		tk2dSpriteAnimator obj3 = head.spriteAnimator;
		obj3.AnimationCompleted = (Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip>)Delegate.Combine(obj3.AnimationCompleted, new Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip>(HeadAnimationComplete));
		animators.Add(head.spriteAnimator);
		head.spriteAnimator.enabled = false;
	}

	public override void EndIntro()
	{
		base.aiAnimator.EndAnimation();
	}

	public override void OnCleanup()
	{
		head.spriteAnimator.enabled = true;
	}

	private void PlayerEnteredRoom(PlayerController playerController)
	{
		RestrictMotion(true);
	}

	private void OnPreDeath(Vector2 finalDirection)
	{
		RestrictMotion(false);
		base.aiActor.ParentRoom.Entered -= PlayerEnteredRoom;
	}

	private void PlayerMovementRestrictor(SpeculativeRigidbody playerSpecRigidbody, IntVector2 prevPixelOffset, IntVector2 pixelOffset, ref bool validLocation)
	{
		if (validLocation && pixelOffset.y < prevPixelOffset.y)
		{
			int num = playerSpecRigidbody.PixelColliders[0].MinY + pixelOffset.y;
			if (num < m_minPlayerY)
			{
				validLocation = false;
			}
		}
	}

	private void BodyAnimationEventTriggered(tk2dSpriteAnimator animator, tk2dSpriteAnimationClip clip, int frame)
	{
		if (clip.GetFrame(frame).eventInfo == "show_head")
		{
			head.enabled = false;
			head.spriteAnimator.Play("gun_appear_intro");
		}
	}

	private void BodyAnimationComplete(tk2dSpriteAnimator animator, tk2dSpriteAnimationClip clip)
	{
		if (clip.name == "priest_recloak")
		{
			m_finished = true;
			head.enabled = true;
			base.spriteAnimator.Play("priest_idle");
		}
	}

	private void HeadAnimationComplete(tk2dSpriteAnimator animator, tk2dSpriteAnimationClip clip)
	{
		if (clip.name == "gun_appear_intro")
		{
			base.aiAnimator.PlayUntilFinished("recloak");
		}
	}

	public void RestrictMotion(bool value)
	{
		if (m_isMotionRestricted == value)
		{
			return;
		}
		if (value)
		{
			if (!GameManager.HasInstance || GameManager.Instance.IsLoadingLevel || GameManager.IsReturningToBreach)
			{
				return;
			}
			m_minPlayerY = base.aiActor.ParentRoom.area.basePosition.y * 16 + 8;
			for (int i = 0; i < GameManager.Instance.AllPlayers.Length; i++)
			{
				SpeculativeRigidbody speculativeRigidbody = GameManager.Instance.AllPlayers[i].specRigidbody;
				speculativeRigidbody.MovementRestrictor = (SpeculativeRigidbody.MovementRestrictorDelegate)Delegate.Combine(speculativeRigidbody.MovementRestrictor, new SpeculativeRigidbody.MovementRestrictorDelegate(PlayerMovementRestrictor));
			}
		}
		else
		{
			if (!GameManager.HasInstance || GameManager.IsReturningToBreach)
			{
				return;
			}
			for (int j = 0; j < GameManager.Instance.AllPlayers.Length; j++)
			{
				PlayerController playerController = GameManager.Instance.AllPlayers[j];
				if ((bool)playerController)
				{
					SpeculativeRigidbody speculativeRigidbody2 = playerController.specRigidbody;
					speculativeRigidbody2.MovementRestrictor = (SpeculativeRigidbody.MovementRestrictorDelegate)Delegate.Remove(speculativeRigidbody2.MovementRestrictor, new SpeculativeRigidbody.MovementRestrictorDelegate(PlayerMovementRestrictor));
				}
			}
		}
		m_isMotionRestricted = value;
	}
}
