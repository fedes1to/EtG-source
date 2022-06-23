using System.Collections;
using Dungeonator;
using UnityEngine;

public class HologramFoyerController : BraveBehaviour
{
	public string[] animationCadence;

	public MeshRenderer ArcRenderer;

	private Material m_arcMaterial;

	public AdditionalBraveLight AttachedBraveLight;

	public tk2dSpriteAnimator TargetAnimator;

	private IEnumerator Start()
	{
		while (Dungeon.IsGenerating || Foyer.DoIntroSequence || Foyer.DoMainMenu)
		{
			yield return null;
		}
		ArcRenderer.enabled = false;
		m_arcMaterial = ArcRenderer.material;
		StartCoroutine(Core());
	}

	private IEnumerator Core()
	{
		yield return null;
		StartCoroutine(ToggleAdditionalLight(true));
		StartCoroutine(HandleArcLerp(false));
		int animIndex = 0;
		TargetAnimator.Sprite.renderer.material.SetFloat("_IsGreen", -1f);
		while (true)
		{
			TargetAnimator.Sprite.renderer.material.SetFloat("_IsGreen", -1f);
			string animName = animationCadence[animIndex];
			yield return StartCoroutine(CoreCycle(animName));
			animIndex = (animIndex + 1) % animationCadence.Length;
		}
	}

	private IEnumerator CoreCycle(string targetAnimation)
	{
		ChangeToAnimation(targetAnimation);
		int m_id = Shader.PropertyToID("_IsGreen");
		while (TargetAnimator.IsPlaying(targetAnimation))
		{
			TargetAnimator.Sprite.renderer.material.SetFloat(m_id, -1f);
			yield return null;
		}
	}

	private IEnumerator ToggleAdditionalLight(bool lightEnabled)
	{
		float elapsed = 0f;
		float duration = 0.25f;
		while (elapsed < duration)
		{
			elapsed += BraveTime.DeltaTime;
			float t = elapsed / duration;
			if (!lightEnabled)
			{
				t = 1f - t;
			}
			AttachedBraveLight.LightIntensity = Mathf.Lerp(0f, 3f, t);
			yield return null;
		}
	}

	private IEnumerator HandleArcLerp(bool invert)
	{
		float ela = 0f;
		ArcRenderer.enabled = true;
		while (ela < 0.2f)
		{
			ela += BraveTime.DeltaTime;
			float t = ela / 0.2f;
			if (invert)
			{
				t = Mathf.Clamp01(1f - t);
			}
			float smoothT = Mathf.SmoothStep(0f, 1f, t);
			m_arcMaterial.SetFloat("_RevealAmount", smoothT);
			ArcRenderer.enabled = true;
			yield return null;
		}
		if (!invert)
		{
			m_arcMaterial.SetFloat("_BrightnessWarble", Mathf.PingPong(Time.realtimeSinceStartup, 1f) / 10f + 1f);
			yield return null;
		}
	}

	public void ChangeToAnimation(string animationName)
	{
		TargetAnimator.renderer.enabled = true;
		TargetAnimator.Play(animationName);
		TargetAnimator.Sprite.usesOverrideMaterial = true;
		TargetAnimator.renderer.material.shader = ShaderCache.Acquire("Brave/Internal/HologramShader");
	}
}
