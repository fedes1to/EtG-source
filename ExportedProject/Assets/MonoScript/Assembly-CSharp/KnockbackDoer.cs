using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KnockbackDoer : BraveBehaviour
{
	private const float MAX_ENEMY_KNOCKBACK_MAGNITUDE = 30f;

	private const float DEFAULT_KNOCKBACK_TIME = 0.5f;

	public float weight = 10f;

	public float deathMultiplier = 5f;

	public bool knockbackWhileReflecting;

	public bool shouldBounce = true;

	public float collisionDecay = 0.5f;

	[NonSerialized]
	public float knockbackMultiplier = 1f;

	[NonSerialized]
	public float timeScalar = 1f;

	private SuperReaperController m_reaper;

	private PlayerController m_player;

	private List<ActiveKnockbackData> m_activeKnockbacks;

	private List<Vector2> m_activeContinuousKnockbacks;

	private OverridableBool m_isImmobile = new OverridableBool(false);

	private void Awake()
	{
		m_player = GetComponent<PlayerController>();
		m_reaper = GetComponent<SuperReaperController>();
		m_activeKnockbacks = new List<ActiveKnockbackData>();
		m_activeContinuousKnockbacks = new List<Vector2>();
	}

	private void Start()
	{
		if ((bool)base.specRigidbody)
		{
			SpeculativeRigidbody speculativeRigidbody = base.specRigidbody;
			speculativeRigidbody.OnCollision = (Action<CollisionData>)Delegate.Combine(speculativeRigidbody.OnCollision, new Action<CollisionData>(OnCollision));
		}
	}

	private void Update()
	{
		Vector2 zero = Vector2.zero;
		Vector2 zero2 = Vector2.zero;
		for (int num = m_activeKnockbacks.Count - 1; num >= 0; num--)
		{
			ActiveKnockbackData activeKnockbackData = m_activeKnockbacks[num];
			if (activeKnockbackData.curveFalloff != null)
			{
				activeKnockbackData.elapsedTime += BraveTime.DeltaTime * timeScalar;
				float num2 = activeKnockbackData.elapsedTime / activeKnockbackData.curveTime;
				float num3 = activeKnockbackData.curveFalloff.Evaluate(num2);
				if (num2 >= 1f)
				{
					m_activeKnockbacks.RemoveAt(num);
				}
				else if (activeKnockbackData.immutable)
				{
					zero2 += activeKnockbackData.initialKnockback * num3;
				}
				else
				{
					zero += activeKnockbackData.initialKnockback * num3;
				}
			}
			else
			{
				activeKnockbackData.elapsedTime += BraveTime.DeltaTime * timeScalar;
				float num4 = activeKnockbackData.elapsedTime / activeKnockbackData.curveTime;
				float num5 = 1f - num4 * num4;
				activeKnockbackData.knockback = activeKnockbackData.initialKnockback * num5;
				if (activeKnockbackData.immutable)
				{
					zero2 += activeKnockbackData.knockback;
				}
				else
				{
					zero += activeKnockbackData.knockback;
				}
				if (activeKnockbackData.elapsedTime >= activeKnockbackData.curveTime)
				{
					m_activeKnockbacks.RemoveAt(num);
				}
			}
		}
		bool flag = true;
		for (int i = 0; i < m_activeContinuousKnockbacks.Count; i++)
		{
			if (m_activeContinuousKnockbacks[i] != Vector2.zero)
			{
				zero += m_activeContinuousKnockbacks[i];
				flag = false;
			}
		}
		if (flag && m_activeContinuousKnockbacks.Count > 0)
		{
			m_activeContinuousKnockbacks.Clear();
		}
		zero *= knockbackMultiplier;
		if (m_isImmobile.Value)
		{
			zero = Vector2.zero;
		}
		if (m_reaper != null)
		{
			m_reaper.knockbackComponent = zero + zero2;
		}
		if (m_player != null)
		{
			m_player.knockbackComponent = zero;
			m_player.immutableKnockbackComponent = zero2;
		}
		if (base.aiActor != null)
		{
			zero += zero2;
			float magnitude = zero.magnitude;
			zero = zero.normalized * Mathf.Min(magnitude, 30f);
			base.aiActor.KnockbackVelocity = zero;
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}

	public void SetImmobile(bool value, string reason = "")
	{
		if (string.IsNullOrEmpty(reason))
		{
			m_isImmobile.BaseValue = value;
			if (!value)
			{
				m_isImmobile.ClearOverrides();
			}
		}
		else
		{
			m_isImmobile.SetOverride(reason, value);
		}
		if (value && (bool)base.specRigidbody)
		{
			base.specRigidbody.Velocity = Vector2.zero;
		}
	}

	public void TriggerTemporaryKnockbackInvulnerability(float duration)
	{
		StartCoroutine(HandleKnockbackInvulnerabilityPeriod(duration));
	}

	private IEnumerator HandleKnockbackInvulnerabilityPeriod(float duration)
	{
		SetImmobile(true, "HandleKnockbackInvulnerabilityPeriod");
		yield return new WaitForSeconds(duration);
		SetImmobile(false, "HandleKnockbackInvulnerabilityPeriod");
	}

	public ActiveKnockbackData ApplyKnockback(Vector2 direction, float force, bool immutable = false)
	{
		if (m_isImmobile.Value)
		{
			return null;
		}
		return ApplyKnockback(direction, force, 0.5f, immutable);
	}

	public ActiveKnockbackData ApplyKnockback(Vector2 direction, float force, float time, bool immutable = false)
	{
		if (m_isImmobile.Value)
		{
			return null;
		}
		ActiveKnockbackData activeKnockbackData = new ActiveKnockbackData(direction.normalized * (force / (weight / 10f)), time, immutable);
		m_activeKnockbacks.Add(activeKnockbackData);
		return activeKnockbackData;
	}

	public ActiveKnockbackData ApplyKnockback(Vector2 direction, float force, AnimationCurve customFalloff, float time, bool immutable = false)
	{
		if (m_isImmobile.Value)
		{
			return null;
		}
		ActiveKnockbackData activeKnockbackData = new ActiveKnockbackData(direction.normalized * (force / (weight / 10f)), customFalloff, time, immutable);
		m_activeKnockbacks.Add(activeKnockbackData);
		return activeKnockbackData;
	}

	public ActiveKnockbackData ApplySourcedKnockback(Vector2 direction, float force, GameObject source, bool immutable = false)
	{
		if (m_isImmobile.Value)
		{
			return null;
		}
		return ApplySourcedKnockback(direction, force, 0.5f, source, immutable);
	}

	public ActiveKnockbackData ApplySourcedKnockback(Vector2 direction, float force, float time, GameObject source, bool immutable = false)
	{
		if (m_isImmobile.Value)
		{
			return null;
		}
		if (CheckSourceInKnockbacks(source))
		{
			return null;
		}
		ActiveKnockbackData activeKnockbackData = ApplyKnockback(direction, force, time, immutable);
		activeKnockbackData.sourceObject = source;
		return activeKnockbackData;
	}

	public ActiveKnockbackData ApplySourcedKnockback(Vector2 direction, float force, AnimationCurve customFalloff, float time, GameObject source, bool immutable = false)
	{
		if (m_isImmobile.Value)
		{
			return null;
		}
		if (CheckSourceInKnockbacks(source))
		{
			return null;
		}
		ActiveKnockbackData activeKnockbackData = ApplyKnockback(direction, force, customFalloff, time, immutable);
		activeKnockbackData.sourceObject = source;
		return activeKnockbackData;
	}

	public int ApplyContinuousKnockback(Vector2 direction, float force)
	{
		m_activeContinuousKnockbacks.Add(direction.normalized * (force / (weight / 10f)));
		return m_activeContinuousKnockbacks.Count - 1;
	}

	public void UpdateContinuousKnockback(Vector2 direction, float force, int id)
	{
		if (m_activeContinuousKnockbacks.Count > id)
		{
			m_activeContinuousKnockbacks[id] = direction.normalized * (force / (weight / 10f));
		}
	}

	public void EndContinuousKnockback(int id)
	{
		if (id >= 0 && id < m_activeContinuousKnockbacks.Count)
		{
			m_activeContinuousKnockbacks[id] = Vector2.zero;
		}
	}

	public void ClearContinuousKnockbacks()
	{
		if (m_activeContinuousKnockbacks != null)
		{
			for (int i = 0; i < m_activeContinuousKnockbacks.Count; i++)
			{
				EndContinuousKnockback(i);
			}
		}
	}

	protected virtual void OnCollision(CollisionData collision)
	{
		if ((collision.collisionType == CollisionData.CollisionType.Rigidbody && collision.OtherRigidbody.Velocity != Vector2.zero) || !(base.healthHaver != null) || !base.healthHaver.IsDead)
		{
			return;
		}
		for (int i = 0; i < m_activeKnockbacks.Count; i++)
		{
			if (Mathf.Sign(collision.Normal.x) != Mathf.Sign(m_activeKnockbacks[i].initialKnockback.x) && collision.Normal.x != 0f)
			{
				if (shouldBounce)
				{
					m_activeKnockbacks[i].initialKnockback = Vector2.Scale(m_activeKnockbacks[i].initialKnockback, new Vector2(-1f, 1f));
				}
				m_activeKnockbacks[i].initialKnockback = Vector2.Scale(m_activeKnockbacks[i].initialKnockback, new Vector2(1f - collisionDecay, 1f));
			}
			if (Mathf.Sign(collision.Normal.y) != Mathf.Sign(m_activeKnockbacks[i].initialKnockback.y) && collision.Normal.y != 0f)
			{
				if (shouldBounce)
				{
					m_activeKnockbacks[i].initialKnockback = Vector2.Scale(m_activeKnockbacks[i].initialKnockback, new Vector2(1f, -1f));
				}
				m_activeKnockbacks[i].initialKnockback = Vector2.Scale(m_activeKnockbacks[i].initialKnockback, new Vector2(1f, 1f - collisionDecay));
			}
		}
	}

	private bool CheckSourceInKnockbacks(GameObject source)
	{
		if (source == null)
		{
			return false;
		}
		for (int i = 0; i < m_activeKnockbacks.Count; i++)
		{
			if (m_activeKnockbacks[i].sourceObject == source)
			{
				return true;
			}
		}
		return false;
	}
}
