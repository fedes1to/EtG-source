using System.Collections;
using UnityEngine;

public class CharacterSelectFacecardIdleDoer : BraveBehaviour
{
	public string appearAnimation = "_appear";

	public string coreIdleAnimation;

	public float idleMin = 4f;

	public float idleMax = 10f;

	public bool usesMultipleIdleAnimations;

	public string[] multipleIdleAnimations;

	public Texture2D EeveeTex;

	protected int lastPhase = -1;

	private void OnEnable()
	{
		if ((bool)EeveeTex)
		{
			base.sprite.usesOverrideMaterial = true;
			base.sprite.renderer.material.shader = Shader.Find("Brave/Internal/GlitchEevee");
			base.sprite.renderer.sharedMaterial.SetTexture("_EeveeTex", EeveeTex);
		}
		base.spriteAnimator.Play(appearAnimation);
		StartCoroutine(HandleCoreIdle());
	}

	private void OnDisable()
	{
		base.spriteAnimator.StopAndResetFrame();
		StopAllCoroutines();
	}

	private IEnumerator HandleCoreIdle()
	{
		while (base.spriteAnimator.IsPlaying(appearAnimation))
		{
			yield return null;
		}
		if (usesMultipleIdleAnimations)
		{
			while (true)
			{
				float duration = Random.Range(idleMin, idleMax);
				float elapsed = 0f;
				base.spriteAnimator.Play(multipleIdleAnimations[Random.Range(0, multipleIdleAnimations.Length)]);
				while (elapsed < duration)
				{
					elapsed += BraveTime.DeltaTime;
					yield return null;
				}
			}
		}
		base.spriteAnimator.Play(coreIdleAnimation);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}
}
