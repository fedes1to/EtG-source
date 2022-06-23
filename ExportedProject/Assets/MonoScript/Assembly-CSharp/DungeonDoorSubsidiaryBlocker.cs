using System;
using UnityEngine;

public class DungeonDoorSubsidiaryBlocker : BraveBehaviour
{
	public bool hideSealAnimators = true;

	public tk2dSpriteAnimator sealAnimator;

	public tk2dSpriteAnimator chainAnimator;

	public tk2dSpriteAnimator sealVFX;

	public float unsealDistanceMaximum = -1f;

	public GameObject unsealedVFXOverride;

	public string sealAnimationName;

	public string sealChainAnimationName;

	public string unsealAnimationName;

	public string unsealChainAnimationName;

	public string playerNearSealedAnimationName;

	public string playerNearChainAnimationName;

	[NonSerialized]
	public bool isSealed;

	public bool northSouth;

	public bool usesUnsealScreenShake;

	public ScreenShakeSettings unsealScreenShake;

	[HideInInspector]
	public DungeonDoorController parentDoor;

	public void ToggleRenderers(bool visible)
	{
		if (sealAnimator != null)
		{
			sealAnimator.GetComponent<Renderer>().enabled = visible;
		}
		if (chainAnimator != null)
		{
			chainAnimator.GetComponent<Renderer>().enabled = visible;
		}
	}

	private void Update()
	{
		if (!(parentDoor != null) || !parentDoor.northSouth || !isSealed || string.IsNullOrEmpty(playerNearSealedAnimationName))
		{
			return;
		}
		Vector2 unitCenter = sealAnimator.GetComponent<SpeculativeRigidbody>().UnitCenter;
		if (Vector2.Distance(unitCenter, GameManager.Instance.PrimaryPlayer.specRigidbody.UnitCenter) < 4f)
		{
			if (!sealAnimator.IsPlaying(playerNearSealedAnimationName) && !sealAnimator.IsPlaying(unsealAnimationName) && !sealAnimator.IsPlaying(sealAnimationName))
			{
				sealAnimator.Play(playerNearSealedAnimationName);
			}
		}
		else if (sealAnimator.IsPlaying(playerNearSealedAnimationName))
		{
			sealAnimator.Stop();
			tk2dSpriteAnimationClip clipByName = sealAnimator.GetClipByName(sealAnimationName);
			sealAnimator.Sprite.SetSprite(clipByName.frames[clipByName.frames.Length - 1].spriteId);
		}
	}

	public void OnSealAnimationEvent(tk2dSpriteAnimator animator, tk2dSpriteAnimationClip clip, int frameNo)
	{
		if (clip.GetFrame(frameNo).eventInfo == "SealVFX" && sealVFX != null)
		{
			sealVFX.gameObject.SetActive(true);
			sealVFX.Play();
		}
	}

	public void OnUnsealAnimationCompleted(tk2dSpriteAnimator a, tk2dSpriteAnimationClip c)
	{
		if (hideSealAnimators)
		{
			a.gameObject.SetActive(false);
		}
		if (a.GetComponent<SpeculativeRigidbody>() != null)
		{
			a.GetComponent<SpeculativeRigidbody>().enabled = false;
		}
		if (unsealedVFXOverride != null)
		{
			unsealedVFXOverride.SetActive(true);
		}
	}

	public void Seal()
	{
		if (!string.IsNullOrEmpty(sealAnimationName))
		{
			sealAnimator.alwaysUpdateOffscreen = true;
			sealAnimator.AnimationCompleted = null;
			tk2dSpriteAnimator obj = sealAnimator;
			obj.AnimationEventTriggered = (Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip, int>)Delegate.Combine(obj.AnimationEventTriggered, new Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip, int>(OnSealAnimationEvent));
			sealAnimator.gameObject.SetActive(true);
			sealAnimator.Play(sealAnimationName);
			AkSoundEngine.PostEvent("Play_OBJ_gate_slam_01", base.gameObject);
		}
		if (!string.IsNullOrEmpty(sealChainAnimationName))
		{
			chainAnimator.Play(sealChainAnimationName);
		}
		if (sealAnimator.GetComponent<SpeculativeRigidbody>() != null)
		{
			sealAnimator.GetComponent<SpeculativeRigidbody>().enabled = true;
		}
		isSealed = true;
	}

	public void Unseal()
	{
		if (isSealed)
		{
			if (!string.IsNullOrEmpty(unsealAnimationName))
			{
				sealAnimator.alwaysUpdateOffscreen = true;
				sealAnimator.Play(unsealAnimationName);
				tk2dSpriteAnimator obj = sealAnimator;
				obj.AnimationCompleted = (Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip>)Delegate.Combine(obj.AnimationCompleted, new Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip>(OnUnsealAnimationCompleted));
				sealAnimator.AnimationEventTriggered = null;
				AkSoundEngine.PostEvent("Play_OBJ_gate_open_01", base.gameObject);
			}
			if (!string.IsNullOrEmpty(unsealChainAnimationName))
			{
				chainAnimator.Play(unsealChainAnimationName);
			}
			if (usesUnsealScreenShake)
			{
				GameManager.Instance.MainCameraController.DoScreenShake(unsealScreenShake, base.transform.position);
			}
			isSealed = false;
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}
}
