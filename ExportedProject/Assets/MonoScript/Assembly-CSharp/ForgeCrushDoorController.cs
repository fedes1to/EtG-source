using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForgeCrushDoorController : DungeonPlaceableBehaviour
{
	public float DamageToEnemies = 30f;

	public float KnockbackForcePlayers = 50f;

	public float KnockbackForceEnemies = 50f;

	public bool DoScreenShake;

	public ScreenShakeSettings ScreenShake;

	public string CloseAnimName;

	public string OpenAnimName;

	public tk2dSpriteAnimator SubsidiaryAnimator;

	public string SubsidiaryCloseAnimName;

	public string SubsidiaryOpenAnimName;

	public tk2dSpriteAnimator vfxAnimator;

	public float DelayTime = 0.25f;

	public float TimeClosed = 1f;

	public float CooldownTime = 3f;

	private bool m_isCrushing;

	private void Start()
	{
		if (base.specRigidbody == null)
		{
			base.specRigidbody = GetComponentInChildren<SpeculativeRigidbody>();
		}
		if (base.spriteAnimator == null)
		{
			base.spriteAnimator = GetComponentInChildren<tk2dSpriteAnimator>();
		}
		SpeculativeRigidbody speculativeRigidbody = base.specRigidbody;
		speculativeRigidbody.OnTriggerCollision = (SpeculativeRigidbody.OnTriggerDelegate)Delegate.Combine(speculativeRigidbody.OnTriggerCollision, new SpeculativeRigidbody.OnTriggerDelegate(HandleTrigger));
		tk2dSpriteAnimator obj = base.spriteAnimator;
		obj.AnimationEventTriggered = (Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip, int>)Delegate.Combine(obj.AnimationEventTriggered, new Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip, int>(HandleAnimationEvent));
		base.spriteAnimator.Sprite.UpdateZDepth();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}

	private void HandleTrigger(SpeculativeRigidbody specRigidbody, SpeculativeRigidbody sourceSpecRigidbody, CollisionData collisionData)
	{
		if (!m_isCrushing)
		{
			m_isCrushing = true;
			StartCoroutine(HandleCrush());
		}
	}

	private IEnumerator HandleCrush()
	{
		m_isCrushing = true;
		yield return new WaitForSeconds(DelayTime);
		base.spriteAnimator.Play(CloseAnimName);
		if (SubsidiaryAnimator != null)
		{
			SubsidiaryAnimator.Play(SubsidiaryCloseAnimName);
		}
		vfxAnimator.renderer.enabled = true;
		vfxAnimator.PlayAndDisableRenderer(string.Empty);
		while (base.spriteAnimator.IsPlaying(CloseAnimName))
		{
			yield return null;
		}
		yield return new WaitForSeconds(TimeClosed);
		base.spriteAnimator.Play(OpenAnimName);
		if (SubsidiaryAnimator != null)
		{
			SubsidiaryAnimator.Play(SubsidiaryOpenAnimName);
		}
		base.spriteAnimator.Sprite.UpdateZDepth();
		base.specRigidbody.PixelColliders[1].Enabled = false;
		while (base.spriteAnimator.IsPlaying(OpenAnimName))
		{
			base.spriteAnimator.Sprite.UpdateZDepth();
			yield return null;
		}
		base.spriteAnimator.Sprite.UpdateZDepth();
		yield return new WaitForSeconds(CooldownTime);
		m_isCrushing = false;
	}

	private void HandleAnimationEvent(tk2dSpriteAnimator sourceAnimator, tk2dSpriteAnimationClip sourceClip, int sourceFrame)
	{
		if (!(sourceClip.frames[sourceFrame].eventInfo == "impact"))
		{
			return;
		}
		if (DoScreenShake)
		{
			GameManager.Instance.MainCameraController.DoScreenShake(ScreenShake, base.specRigidbody.UnitCenter);
		}
		base.specRigidbody.PixelColliders[1].Enabled = true;
		base.specRigidbody.Reinitialize();
		Exploder.DoRadialMinorBreakableBreak(base.spriteAnimator.Sprite.WorldCenter, 1f);
		List<SpeculativeRigidbody> overlappingRigidbodies = PhysicsEngine.Instance.GetOverlappingRigidbodies(base.specRigidbody);
		for (int i = 0; i < overlappingRigidbodies.Count; i++)
		{
			if (!overlappingRigidbodies[i].gameActor)
			{
				continue;
			}
			Vector2 direction = overlappingRigidbodies[i].UnitCenter - base.specRigidbody.UnitCenter;
			if (overlappingRigidbodies[i].gameActor is PlayerController)
			{
				if ((bool)overlappingRigidbodies[i].healthHaver)
				{
					overlappingRigidbodies[i].healthHaver.ApplyDamage(0.5f, direction, StringTableManager.GetEnemiesString("#TRAP"));
				}
				if ((bool)overlappingRigidbodies[i].knockbackDoer)
				{
					overlappingRigidbodies[i].knockbackDoer.ApplyKnockback(direction, KnockbackForcePlayers);
				}
			}
			else
			{
				if ((bool)overlappingRigidbodies[i].healthHaver)
				{
					overlappingRigidbodies[i].healthHaver.ApplyDamage(DamageToEnemies, direction, StringTableManager.GetEnemiesString("#TRAP"));
				}
				if ((bool)overlappingRigidbodies[i].knockbackDoer)
				{
					overlappingRigidbodies[i].knockbackDoer.ApplyKnockback(direction, KnockbackForceEnemies);
				}
			}
		}
		PhysicsEngine.Instance.RegisterOverlappingGhostCollisionExceptions(base.specRigidbody);
	}
}
