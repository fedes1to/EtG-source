using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StickyFrictionManager : TimeInvariantMonoBehaviour
{
	[Serializable]
	public class DamageFriction
	{
		public bool enabled;

		public float minFriction;

		public float maxFriction;

		public float minDamage;

		public float maxDamage;
	}

	public bool FrictionEnabled = true;

	[Header("Damage")]
	public DamageFriction enemyDamage;

	public DamageFriction playerDamage;

	public DamageFriction swordDamage;

	[Header("Death")]
	public float enemyDeathFriction = 0.075f;

	[Header("Explosions")]
	public float explosionFriction = 0.1f;

	private const float FRICTION_REDUCTION_TIME = 0.5f;

	private const float FRICTION_REDUCTION_FALLOFF = 0.5f;

	private static StickyFrictionManager m_instance;

	private List<StickyFrictionModifier> m_fricts = new List<StickyFrictionModifier>();

	private float m_currentMinFriction;

	public static StickyFrictionManager Instance
	{
		get
		{
			if (m_instance == null)
			{
				m_instance = (StickyFrictionManager)UnityEngine.Object.FindObjectOfType(typeof(StickyFrictionManager));
				if (m_instance == null)
				{
					GameObject gameObject = new GameObject("_TimRogers");
					m_instance = gameObject.AddComponent<StickyFrictionManager>();
				}
			}
			return m_instance;
		}
	}

	protected override void InvariantUpdate(float realDeltaTime)
	{
		GameManager.Instance.MainCameraController.CurrentStickyFriction = 1f;
		if (GameManager.Instance.IsPaused || !FrictionEnabled || m_fricts == null || m_fricts.Count <= 0)
		{
			return;
		}
		float num = 1f;
		for (int num2 = m_fricts.Count - 1; num2 >= 0; num2--)
		{
			m_fricts[num2].elapsed += realDeltaTime;
			if (m_fricts[num2].elapsed >= 0f && m_fricts[num2].elapsed < m_fricts[num2].length)
			{
				num *= m_fricts[num2].GetCurrentMagnitude();
			}
			else
			{
				m_fricts.RemoveAt(num2);
			}
		}
		BraveTime.SetTimeScaleMultiplier(num, base.gameObject);
	}

	protected override void OnDestroy()
	{
		BraveTime.ClearMultiplier(base.gameObject);
		base.OnDestroy();
	}

	public void RegisterSwordDamageStickyFriction(float damage)
	{
		RegisterDamageStickyFriction(damage, swordDamage);
	}

	public void RegisterPlayerDamageStickyFriction(float damage)
	{
		RegisterDamageStickyFriction(damage, playerDamage);
	}

	public void RegisterOtherDamageStickyFriction(float damage)
	{
		RegisterDamageStickyFriction(damage, enemyDamage);
	}

	private void InternalRegisterFrict(StickyFrictionModifier newFrict, bool ignoreFrictReduction = false)
	{
		newFrict.magnitude = Mathf.Clamp01(Mathf.Max(newFrict.magnitude, m_currentMinFriction));
		m_fricts.Add(newFrict);
		if (!ignoreFrictReduction)
		{
			StartCoroutine(HandleAdditionalFrictReduction(newFrict));
		}
	}

	private IEnumerator HandleAdditionalFrictReduction(StickyFrictionModifier newFrict)
	{
		float elapsed2 = 0f;
		float curContribution = 1f;
		m_currentMinFriction += curContribution;
		while (elapsed2 < 0.5f)
		{
			elapsed2 += GameManager.INVARIANT_DELTA_TIME;
			yield return null;
		}
		elapsed2 = 0f;
		while (elapsed2 < 0.5f)
		{
			elapsed2 += GameManager.INVARIANT_DELTA_TIME;
			m_currentMinFriction -= curContribution;
			curContribution = Mathf.Lerp(1f, 0f, elapsed2 / 0.5f);
			m_currentMinFriction += curContribution;
			yield return null;
		}
		m_currentMinFriction -= curContribution;
		if (m_currentMinFriction < 0f)
		{
			m_currentMinFriction = 0f;
		}
	}

	public void RegisterExplosionStickyFriction()
	{
		InternalRegisterFrict(new StickyFrictionModifier(explosionFriction, 0f, false));
	}

	public void RegisterDeathStickyFriction()
	{
		InternalRegisterFrict(new StickyFrictionModifier(enemyDeathFriction, 0f, false));
	}

	public void RegisterCustomStickyFriction(float length, float magnitude, bool falloff, bool ignoreFrictReduction = false)
	{
		if (m_fricts.Count > 0)
		{
			m_fricts.Clear();
		}
		InternalRegisterFrict(new StickyFrictionModifier(length, magnitude, falloff), ignoreFrictReduction);
	}

	private void RegisterDamageStickyFriction(float damage, DamageFriction frictionData)
	{
		if (frictionData.enabled)
		{
			float t = Mathf.InverseLerp(frictionData.minDamage, frictionData.maxDamage, damage);
			float l = Mathf.Lerp(frictionData.minFriction, frictionData.maxFriction, t);
			InternalRegisterFrict(new StickyFrictionModifier(l, 0f, false));
		}
	}
}
