using System;
using System.Collections;
using UnityEngine;

public class CharacterSelectIdleDoer : BraveBehaviour
{
	public string coreIdleAnimation = "select_idle";

	public string onSelectedAnimation;

	public float idleMin = 4f;

	public float idleMax = 10f;

	public CharacterSelectIdlePhase[] phases;

	public bool IsEevee;

	public Texture2D EeveeTex;

	public tk2dSpriteAnimation[] AnimationLibraries;

	protected int lastPhase = -1;

	protected float m_lastEeveeSwitchTime;

	private void Update()
	{
		if (IsEevee)
		{
			base.sprite.usesOverrideMaterial = true;
			base.sprite.renderer.material.shader = Shader.Find("Brave/PlayerShaderEevee");
			base.sprite.renderer.sharedMaterial.SetTexture("_EeveeTex", EeveeTex);
			m_lastEeveeSwitchTime += BraveTime.DeltaTime;
			if (m_lastEeveeSwitchTime > 2.5f)
			{
				m_lastEeveeSwitchTime -= 2.5f;
				int num = UnityEngine.Random.Range(0, AnimationLibraries.Length);
				base.spriteAnimator.Library = AnimationLibraries[num];
				base.spriteAnimator.Play(coreIdleAnimation);
			}
		}
	}

	private void OnEnable()
	{
		if (IsEevee)
		{
			base.sprite.usesOverrideMaterial = true;
			base.sprite.renderer.material.shader = Shader.Find("Brave/PlayerShaderEevee");
			base.sprite.renderer.sharedMaterial.SetTexture("_EeveeTex", EeveeTex);
		}
		StartCoroutine(HandleCoreIdle());
	}

	private void OnDisable()
	{
		base.spriteAnimator.StopAndResetFrame();
		StopAllCoroutines();
	}

	private IEnumerator HandleCoreIdle()
	{
		if (!CharacterSelectController.HasSelected)
		{
			base.spriteAnimator.Play(coreIdleAnimation);
		}
		yield return new WaitForSeconds(Mathf.Lerp(idleMin, idleMax, UnityEngine.Random.value));
		if (phases.Length != 0)
		{
			int num = UnityEngine.Random.Range(0, phases.Length);
			if (num == lastPhase && phases.Length > 1)
			{
				num = (num + 1) % phases.Length;
			}
			if (num < 0 || num >= phases.Length)
			{
				num = 0;
			}
			CharacterSelectIdlePhase phase = phases[num];
			lastPhase = num;
			if (!CharacterSelectController.HasSelected)
			{
				StartCoroutine(HandlePhase(phase));
			}
		}
	}

	private void DeactivateVFX(tk2dSpriteAnimator s, tk2dSpriteAnimationClip c)
	{
		s.AnimationCompleted = (Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip>)Delegate.Remove(s.AnimationCompleted, new Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip>(DeactivateVFX));
		s.gameObject.SetActive(false);
	}

	private void TriggerVFX(CharacterSelectIdlePhase phase)
	{
		phase.vfxSpriteAnimator.StopAndResetFrame();
		phase.vfxSpriteAnimator.gameObject.SetActive(true);
		phase.vfxSpriteAnimator.Play();
		tk2dSpriteAnimator vfxSpriteAnimator = phase.vfxSpriteAnimator;
		vfxSpriteAnimator.AnimationCompleted = (Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip>)Delegate.Combine(vfxSpriteAnimator.AnimationCompleted, new Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip>(DeactivateVFX));
	}

	private void TriggerEndVFX(CharacterSelectIdlePhase phase)
	{
		if (phase.endVFXSpriteAnimator != null)
		{
			phase.endVFXSpriteAnimator.StopAndResetFrame();
			phase.endVFXSpriteAnimator.Play();
		}
	}

	private IEnumerator HandlePhase(CharacterSelectIdlePhase phase)
	{
		if (!string.IsNullOrEmpty(phase.inAnimation))
		{
			tk2dSpriteAnimationClip clip2 = base.spriteAnimator.GetClipByName(phase.inAnimation);
			if (!CharacterSelectController.HasSelected && clip2 != null)
			{
				base.spriteAnimator.Play(clip2);
			}
			if (phase.vfxTrigger == CharacterSelectIdlePhase.VFXPhaseTrigger.IN)
			{
				TriggerVFX(phase);
			}
			if (clip2 != null)
			{
				yield return new WaitForSeconds((float)clip2.frames.Length / clip2.fps);
			}
		}
		if (!string.IsNullOrEmpty(phase.holdAnimation) && !CharacterSelectController.HasSelected)
		{
			base.spriteAnimator.Play(phase.holdAnimation);
		}
		float elapsed = 0f;
		float vfxElapsed = 0f;
		float holdDuration = Mathf.Lerp(phase.holdMin, phase.holdMax, UnityEngine.Random.value);
		while (elapsed < holdDuration)
		{
			if (phase.vfxTrigger == CharacterSelectIdlePhase.VFXPhaseTrigger.HOLD && vfxElapsed > phase.vfxHoldPeriod)
			{
				vfxElapsed -= phase.vfxHoldPeriod;
				if (!phase.vfxSpriteAnimator.gameObject.activeSelf)
				{
					TriggerVFX(phase);
				}
			}
			elapsed += BraveTime.DeltaTime;
			vfxElapsed += BraveTime.DeltaTime;
			yield return null;
		}
		if (!string.IsNullOrEmpty(phase.optionalHoldIdleAnimation) && UnityEngine.Random.value < phase.optionalHoldChance)
		{
			if (!CharacterSelectController.HasSelected)
			{
				base.spriteAnimator.Play(phase.optionalHoldIdleAnimation);
			}
			yield return new WaitForSeconds(Mathf.Lerp(phase.holdMin, phase.holdMax, UnityEngine.Random.value));
		}
		if (!string.IsNullOrEmpty(phase.outAnimation))
		{
			tk2dSpriteAnimationClip clip = base.spriteAnimator.GetClipByName(phase.outAnimation);
			if (!CharacterSelectController.HasSelected)
			{
				base.spriteAnimator.Play(clip);
			}
			if (phase.vfxTrigger == CharacterSelectIdlePhase.VFXPhaseTrigger.OUT)
			{
				TriggerVFX(phase);
			}
			yield return new WaitForSeconds((float)(clip.frames.Length - 2) / clip.fps);
			TriggerEndVFX(phase);
			yield return new WaitForSeconds(2f / clip.fps);
		}
		StartCoroutine(HandleCoreIdle());
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}
}
