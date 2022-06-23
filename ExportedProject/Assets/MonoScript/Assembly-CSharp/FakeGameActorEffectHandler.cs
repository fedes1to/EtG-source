using System.Collections.Generic;
using UnityEngine;

public class FakeGameActorEffectHandler : BraveBehaviour
{
	private class RuntimeGameActorEffectData
	{
		public float elapsed;

		public tk2dBaseSprite instanceOverheadVFX;
	}

	public bool OverrideColorOverridden;

	private int m_overrideColorID;

	private List<string> m_overrideColorSources = new List<string>();

	private List<Color> m_overrideColorStack = new List<Color>();

	private List<GameActorEffect> m_activeEffects = new List<GameActorEffect>();

	private List<RuntimeGameActorEffectData> m_activeEffectData = new List<RuntimeGameActorEffectData>();

	public bool IsGone { get; set; }

	public Color CurrentOverrideColor
	{
		get
		{
			if (m_overrideColorStack.Count == 0)
			{
				RegisterOverrideColor(new Color(1f, 1f, 1f, 0f), "base");
			}
			return m_overrideColorStack[m_overrideColorStack.Count - 1];
		}
	}

	public virtual void Awake()
	{
		m_overrideColorID = Shader.PropertyToID("_OverrideColor");
		RegisterOverrideColor(new Color(1f, 1f, 1f, 0f), "base");
	}

	public virtual void Update()
	{
		for (int i = 0; i < m_activeEffects.Count; i++)
		{
			GameActorEffect gameActorEffect = m_activeEffects[i];
			if (gameActorEffect == null || m_activeEffectData == null || i >= m_activeEffectData.Count)
			{
				continue;
			}
			RuntimeGameActorEffectData runtimeGameActorEffectData = m_activeEffectData[i];
			if (runtimeGameActorEffectData == null)
			{
				continue;
			}
			if (runtimeGameActorEffectData.instanceOverheadVFX != null)
			{
				if (!IsGone)
				{
					Vector2 vector = base.transform.position.XY();
					if (gameActorEffect.PlaysVFXOnActor)
					{
						if ((bool)base.specRigidbody && base.specRigidbody.HitboxPixelCollider != null)
						{
							vector = base.specRigidbody.HitboxPixelCollider.UnitBottomCenter.Quantize(0.0625f);
						}
						runtimeGameActorEffectData.instanceOverheadVFX.transform.position = vector;
					}
					else
					{
						if ((bool)base.specRigidbody && base.specRigidbody.HitboxPixelCollider != null)
						{
							vector = base.specRigidbody.HitboxPixelCollider.UnitTopCenter.Quantize(0.0625f);
						}
						runtimeGameActorEffectData.instanceOverheadVFX.transform.position = vector;
					}
					runtimeGameActorEffectData.instanceOverheadVFX.renderer.enabled = true;
				}
				else if ((bool)runtimeGameActorEffectData.instanceOverheadVFX)
				{
					Object.Destroy(runtimeGameActorEffectData.instanceOverheadVFX.gameObject);
				}
			}
			runtimeGameActorEffectData.elapsed += BraveTime.DeltaTime;
			if (runtimeGameActorEffectData.elapsed >= gameActorEffect.duration)
			{
				RemoveEffect(gameActorEffect);
			}
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}

	public void ApplyEffect(GameActorEffect effect)
	{
		RuntimeGameActorEffectData runtimeGameActorEffectData = new RuntimeGameActorEffectData();
		if (effect.AppliesTint)
		{
			RegisterOverrideColor(effect.TintColor, effect.effectIdentifier);
		}
		if (effect.OverheadVFX != null)
		{
			GameObject gameObject = Object.Instantiate(effect.OverheadVFX);
			tk2dBaseSprite component = gameObject.GetComponent<tk2dBaseSprite>();
			gameObject.transform.parent = base.transform;
			if ((bool)base.healthHaver && base.healthHaver.IsBoss)
			{
				gameObject.transform.position = base.specRigidbody.HitboxPixelCollider.UnitTopCenter;
			}
			else
			{
				Bounds bounds = base.sprite.GetBounds();
				Vector3 vector = base.transform.position + new Vector3((bounds.max.x + bounds.min.x) / 2f, bounds.max.y, 0f).Quantize(0.0625f);
				if (effect.PlaysVFXOnActor)
				{
					vector.y = base.transform.position.y + bounds.min.y;
				}
				gameObject.transform.position = base.sprite.WorldCenter.ToVector3ZUp().WithY(vector.y);
			}
			component.HeightOffGround = 0.5f;
			base.sprite.AttachRenderer(component);
			runtimeGameActorEffectData.instanceOverheadVFX = gameObject.GetComponent<tk2dBaseSprite>();
		}
		m_activeEffects.Add(effect);
		m_activeEffectData.Add(runtimeGameActorEffectData);
	}

	public void RemoveEffect(GameActorEffect effect)
	{
		for (int i = 0; i < m_activeEffects.Count; i++)
		{
			if (m_activeEffects[i].effectIdentifier == effect.effectIdentifier)
			{
				RemoveEffect(i);
				break;
			}
		}
	}

	private void RemoveEffect(int index, bool ignoreDeathCheck = false)
	{
		if (ignoreDeathCheck || !base.healthHaver || !base.healthHaver.IsDead)
		{
			GameActorEffect gameActorEffect = m_activeEffects[index];
			if (gameActorEffect.AppliesTint)
			{
				DeregisterOverrideColor(gameActorEffect.effectIdentifier);
			}
			m_activeEffects.RemoveAt(index);
			if ((bool)m_activeEffectData[index].instanceOverheadVFX)
			{
				Object.Destroy(m_activeEffectData[index].instanceOverheadVFX.gameObject);
			}
			m_activeEffectData.RemoveAt(index);
		}
	}

	public void RemoveAllEffects(bool ignoreDeathCheck = false)
	{
		for (int num = m_activeEffects.Count - 1; num >= 0; num--)
		{
			RemoveEffect(num, ignoreDeathCheck);
		}
	}

	public bool HasSourcedOverrideColor(string source)
	{
		return m_overrideColorSources.Contains(source);
	}

	public void RegisterOverrideColor(Color overrideColor, string source)
	{
		int num = m_overrideColorSources.IndexOf(source);
		if (num >= 0)
		{
			m_overrideColorStack[num] = overrideColor;
		}
		else
		{
			m_overrideColorSources.Add(source);
			m_overrideColorStack.Add(overrideColor);
		}
		OnOverrideColorsChanged();
	}

	public void DeregisterOverrideColor(string source)
	{
		int num = m_overrideColorSources.IndexOf(source);
		if (num >= 0)
		{
			m_overrideColorStack.RemoveAt(num);
			m_overrideColorSources.RemoveAt(num);
		}
		OnOverrideColorsChanged();
	}

	public void OnOverrideColorsChanged()
	{
		if (OverrideColorOverridden)
		{
			return;
		}
		if ((bool)base.healthHaver)
		{
			for (int i = 0; i < base.healthHaver.bodySprites.Count; i++)
			{
				if ((bool)base.healthHaver.bodySprites[i])
				{
					base.healthHaver.bodySprites[i].usesOverrideMaterial = true;
					base.healthHaver.bodySprites[i].renderer.material.SetColor(m_overrideColorID, CurrentOverrideColor);
				}
			}
		}
		else if ((bool)base.sprite)
		{
			base.sprite.usesOverrideMaterial = true;
			base.sprite.renderer.material.SetColor(m_overrideColorID, CurrentOverrideColor);
		}
	}
}
