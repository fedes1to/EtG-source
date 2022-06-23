using System.Collections.Generic;
using UnityEngine;

public class ExplosionManager : BraveBehaviour
{
	private const float c_explosionStaggerDelay = 0.125f;

	private Queue<Exploder> m_queue = new Queue<Exploder>();

	private float m_timer;

	private static ExplosionManager m_instance;

	public static ExplosionManager Instance
	{
		get
		{
			if (!m_instance)
			{
				GameObject gameObject = new GameObject("_ExplosionManager", typeof(ExplosionManager));
				m_instance = gameObject.GetComponent<ExplosionManager>();
			}
			return m_instance;
		}
	}

	public int QueueCount
	{
		get
		{
			return m_queue.Count;
		}
	}

	public static void ClearPerLevelData()
	{
		m_instance = null;
	}

	public void Update()
	{
		if (m_timer > 0f)
		{
			m_timer -= BraveTime.DeltaTime;
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}

	public void Queue(Exploder exploder)
	{
		m_queue.Enqueue(exploder);
	}

	public bool IsExploderReady(Exploder exploder)
	{
		if (m_queue.Count == 0)
		{
			return true;
		}
		return m_queue.Peek() == exploder && m_timer <= 0f;
	}

	public void Dequeue()
	{
		if (m_queue.Count > 0)
		{
			m_queue.Dequeue();
		}
		m_timer = 0.125f;
	}
}
