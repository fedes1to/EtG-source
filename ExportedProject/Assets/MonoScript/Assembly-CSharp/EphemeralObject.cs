using System.Collections;
using UnityEngine;

public abstract class EphemeralObject : ClusteredTimeInvariantMonoBehaviour
{
	public enum EphemeralPriority
	{
		Critical,
		Important,
		Middling,
		Minor,
		Ephemeral
	}

	public EphemeralPriority Priority = EphemeralPriority.Middling;

	private float m_destructionTimer;

	private bool m_isRegistered;

	private const float c_destroyTime = 1f;

	public virtual void Start()
	{
		OnSpawned();
	}

	public virtual void OnSpawned()
	{
		if (!m_isRegistered)
		{
			SpawnManager.RegisterEphemeralObject(this);
			m_isRegistered = true;
		}
	}

	protected override void OnDestroy()
	{
		OnDespawned();
		base.OnDestroy();
	}

	public virtual void OnDespawned()
	{
		if (m_isRegistered)
		{
			if (SpawnManager.HasInstance)
			{
				SpawnManager.DeregisterEphemeralObject(this);
			}
			m_isRegistered = false;
		}
	}

	public void TriggerDestruction(bool forceImmediate = false)
	{
		SpawnManager.DeregisterEphemeralObject(this);
		if (base.gameObject.activeInHierarchy && !forceImmediate)
		{
			StartCoroutine(DestroyCR());
		}
		else
		{
			SpawnManager.Despawn(base.gameObject);
		}
	}

	private IEnumerator DestroyCR()
	{
		float timer = 0f;
		tk2dSprite sprite = GetComponent<tk2dSprite>();
		Color startColor = ((!sprite) ? Color.white : sprite.color);
		while (timer < 1f)
		{
			yield return null;
			timer += BraveTime.DeltaTime;
			if ((bool)sprite)
			{
				Color color = sprite.color;
				color.a = Mathf.Lerp(startColor.a, 0f, timer / 1f);
				sprite.color = color;
			}
		}
		if ((bool)sprite)
		{
			sprite.color = startColor;
		}
		SpawnManager.Despawn(base.gameObject);
	}
}
