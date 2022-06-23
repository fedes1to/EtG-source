using System;
using System.Collections.Generic;
using UnityEngine;

public class CharacterAnimationRandomizer : MonoBehaviour
{
	public List<tk2dSpriteAnimation> AnimationLibraries;

	public Color[] PrimaryColors;

	public Texture2D CosmicTex;

	private PlayerController m_player;

	private tk2dBaseSprite m_sprite;

	private tk2dSpriteAnimator m_animator;

	private Material m_material;

	private int m_shaderID;

	public void Start()
	{
		m_player = GetComponent<PlayerController>();
		m_sprite = m_player.sprite;
		m_animator = m_player.spriteAnimator;
		m_material = m_sprite.renderer.sharedMaterial;
		m_shaderID = Shader.PropertyToID("_EeveeColor");
		m_material.SetTexture("_EeveeTex", CosmicTex);
		tk2dSpriteAnimator animator = m_animator;
		animator.AnimationCompleted = (Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip>)Delegate.Combine(animator.AnimationCompleted, new Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip>(HandleAnimationCompletedSwap));
	}

	private void HandleAnimationCompletedSwap(tk2dSpriteAnimator arg1, tk2dSpriteAnimationClip arg2)
	{
		if (m_player.IsVisible)
		{
			int num = UnityEngine.Random.Range(0, AnimationLibraries.Count);
			m_animator.Library = AnimationLibraries[num];
			m_material.SetColor(m_shaderID, PrimaryColors[Mathf.Min(num, PrimaryColors.Length - 1)]);
			m_material.SetTexture("_EeveeTex", CosmicTex);
		}
	}

	public void AddOverrideAnimLibrary(tk2dSpriteAnimation library)
	{
		if (!AnimationLibraries.Contains(library))
		{
			AnimationLibraries.Add(library);
		}
	}

	public void RemoveOverrideAnimLibrary(tk2dSpriteAnimation library)
	{
		if (AnimationLibraries.Contains(library))
		{
			AnimationLibraries.Remove(library);
		}
	}
}
