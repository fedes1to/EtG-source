using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class SpriteAnimatorKiller : MonoBehaviour
{
	public bool onlyDisable;

	[FormerlySerializedAs("deparentOnAwake")]
	public bool deparentOnStart;

	public List<GameObject> childObjectToDisable;

	public tk2dSpriteAnimator animator;

	public dfSpriteAnimation dfAnimation;

	public bool hasChildAnimators;

	public bool deparentAllChildren;

	public bool disableRendererOnDelay;

	public float delayDestructionTime;

	public float fadeTime;

	private bool m_initialized;

	private Renderer m_renderer;

	private float m_killTimer;

	private float m_fadeTimer;

	public void Awake()
	{
		if (!animator)
		{
			animator = GetComponent<tk2dSpriteAnimator>();
		}
		if (!dfAnimation)
		{
			dfAnimation = GetComponent<dfSpriteAnimation>();
		}
		m_renderer = ((!animator) ? GetComponent<Renderer>() : animator.renderer);
	}

	public void Start()
	{
		if (!m_initialized && base.enabled)
		{
			if (onlyDisable)
			{
				m_renderer.enabled = true;
				animator.enabled = true;
			}
			if (animator != null)
			{
				tk2dSpriteAnimator obj = animator;
				obj.AnimationCompleted = (Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip>)Delegate.Combine(obj.AnimationCompleted, new Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip>(OnAnimationCompleted));
			}
			if (dfAnimation != null)
			{
				dfAnimation.AnimationCompleted += dfAnimationComplete;
			}
			if (deparentOnStart)
			{
				base.transform.parent = SpawnManager.Instance.VFX;
			}
			m_initialized = true;
		}
	}

	public void Update()
	{
		if (m_killTimer > 0f)
		{
			m_killTimer -= BraveTime.DeltaTime;
			if (m_killTimer <= 0f)
			{
				BeginDeath();
			}
		}
		else if (m_fadeTimer > 0f)
		{
			m_fadeTimer -= BraveTime.DeltaTime;
			animator.sprite.color = animator.sprite.color.WithAlpha(Mathf.Clamp01(m_fadeTimer / fadeTime));
			if (m_fadeTimer <= 0f)
			{
				FinishDeath();
			}
		}
	}

	public void OnSpawned()
	{
		if (base.enabled)
		{
			Start();
		}
		if (!hasChildAnimators)
		{
			return;
		}
		SpriteAnimatorKiller[] componentsInChildren = GetComponentsInChildren<SpriteAnimatorKiller>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			if (!(componentsInChildren[i] == this))
			{
				componentsInChildren[i].OnSpawned();
			}
		}
	}

	public void OnDespawned()
	{
		Cleanup();
		if (!hasChildAnimators)
		{
			return;
		}
		SpriteAnimatorKiller[] componentsInChildren = GetComponentsInChildren<SpriteAnimatorKiller>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			if (!(componentsInChildren[i] == this))
			{
				componentsInChildren[i].OnDespawned();
			}
		}
	}

	public void Cleanup()
	{
		if (animator != null)
		{
			tk2dSpriteAnimator obj = animator;
			obj.AnimationCompleted = (Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip>)Delegate.Remove(obj.AnimationCompleted, new Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip>(OnAnimationCompleted));
		}
		if (dfAnimation != null)
		{
			dfAnimation.AnimationCompleted -= dfAnimationComplete;
		}
		if ((bool)this && (bool)base.transform && (bool)SpawnManager.Instance && base.transform.parent != SpawnManager.Instance.VFX && (base.transform.parent == null || base.transform.parent.GetComponent<SpriteAnimatorKiller>() == null))
		{
			base.transform.parent = SpawnManager.Instance.VFX;
		}
		m_initialized = false;
	}

	public void Restart()
	{
		if ((bool)animator)
		{
			animator.enabled = true;
			animator.PlayFrom(0f);
		}
		if ((bool)dfAnimation)
		{
			Debug.LogWarning("unsupported");
		}
		m_renderer.enabled = true;
		for (int i = 0; i < childObjectToDisable.Count; i++)
		{
			childObjectToDisable[i].SetActive(true);
		}
	}

	public void Disable()
	{
		if ((bool)animator)
		{
			animator.enabled = false;
		}
		if ((bool)dfAnimation)
		{
			dfAnimation.enabled = false;
		}
		m_renderer.enabled = false;
		for (int i = 0; i < childObjectToDisable.Count; i++)
		{
			childObjectToDisable[i].SetActive(false);
		}
	}

	public void OnAnimationCompleted(tk2dSpriteAnimator a, tk2dSpriteAnimationClip c)
	{
		if (delayDestructionTime > 0f)
		{
			if ((bool)m_renderer && disableRendererOnDelay)
			{
				m_renderer.enabled = false;
			}
			m_killTimer = delayDestructionTime;
		}
		else
		{
			BeginDeath();
		}
	}

	public void dfAnimationComplete(dfTweenPlayableBase source)
	{
		if (delayDestructionTime > 0f)
		{
			m_killTimer = delayDestructionTime;
		}
		else
		{
			BeginDeath();
		}
	}

	private void BeginDeath()
	{
		if (fadeTime > 0f)
		{
			m_fadeTimer = fadeTime;
		}
		else
		{
			FinishDeath();
		}
	}

	private void FinishDeath()
	{
		if (!this || !base.transform)
		{
			return;
		}
		if (deparentAllChildren)
		{
			while (base.transform.childCount > 0)
			{
				base.transform.GetChild(0).parent = base.transform.parent;
			}
		}
		if (fadeTime > 0f && (bool)animator && (bool)animator.sprite)
		{
			animator.sprite.color = animator.sprite.color.WithAlpha(1f);
		}
		if (onlyDisable)
		{
			Disable();
			return;
		}
		Cleanup();
		SpawnManager.Despawn(base.gameObject);
	}
}
