using System.Collections;
using UnityEngine;

public class TimedObjectKiller : MonoBehaviour
{
	public enum PoolType
	{
		Pooled,
		SonOfPooled,
		NonPooled
	}

	public float lifeTime = 1f;

	private static int m_lightCullingMaskID = -1;

	public PoolType m_poolType;

	private Light m_light;

	private Renderer m_renderer;

	public void Start()
	{
		if (m_lightCullingMaskID == -1)
		{
			m_lightCullingMaskID = (1 << LayerMask.NameToLayer("BG_Critical")) | (1 << LayerMask.NameToLayer("BG_Nonsense"));
		}
		if (SpawnManager.IsPooled(base.gameObject))
		{
			m_poolType = PoolType.Pooled;
		}
		else
		{
			m_poolType = PoolType.NonPooled;
			Transform parent = base.transform.parent;
			while ((bool)parent)
			{
				if (SpawnManager.IsPooled(parent.gameObject))
				{
					m_poolType = PoolType.SonOfPooled;
					break;
				}
				parent = parent.parent;
			}
		}
		if (m_poolType == PoolType.SonOfPooled)
		{
			m_light = GetComponent<Light>();
			if (m_light != null)
			{
				m_light.cullingMask = m_lightCullingMaskID;
			}
			m_renderer = GetComponent<Renderer>();
		}
		Init();
	}

	private void Init()
	{
		StartCoroutine(HandleDeath());
	}

	private IEnumerator HandleDeath()
	{
		yield return new WaitForSeconds(lifeTime);
		if (m_poolType == PoolType.SonOfPooled)
		{
			if ((bool)m_light)
			{
				m_light.enabled = false;
			}
			if ((bool)m_renderer)
			{
				m_renderer.enabled = false;
			}
		}
		else
		{
			SpawnManager.Despawn(base.gameObject);
		}
	}

	public void OnSpawned()
	{
		if (base.enabled)
		{
			if ((bool)m_light)
			{
				m_light.enabled = true;
			}
			if ((bool)m_renderer)
			{
				m_renderer.enabled = true;
			}
			Start();
		}
	}

	public void OnDespawned()
	{
		StopAllCoroutines();
	}
}
