using System.Collections.Generic;
using UnityEngine;

public class PersistentVFXManagerBehaviour : BraveBehaviour
{
	protected List<GameObject> attachedPersistentVFX;

	protected List<GameObject> attachedDestructibleVFX;

	private bool m_pvmbDestroyed;

	public void AttachPersistentVFX(GameObject vfx)
	{
		if (attachedPersistentVFX == null)
		{
			attachedPersistentVFX = new List<GameObject>();
		}
		attachedPersistentVFX.Add(vfx);
		vfx.transform.parent = base.transform;
	}

	public void AttachDestructibleVFX(GameObject vfx)
	{
		if (m_pvmbDestroyed)
		{
			Object.Destroy(vfx);
			return;
		}
		if (attachedDestructibleVFX == null)
		{
			attachedDestructibleVFX = new List<GameObject>();
		}
		attachedDestructibleVFX.Add(vfx);
		vfx.transform.parent = base.transform;
	}

	public void TriggerPersistentVFXClear()
	{
		TriggerPersistentVFXClear(Vector3.right, 180f, 0.5f, 0.3f, 0.7f);
	}

	public void TriggerPersistentVFXClear(Vector3 startingForce, float forceVarianceAngle, float forceVarianceMagnitude, float startingHeightMin, float startingHeightMax)
	{
		if (attachedPersistentVFX != null)
		{
			for (int i = 0; i < attachedPersistentVFX.Count; i++)
			{
				Vector3 startingForce2 = Quaternion.Euler(0f, 0f, Random.Range(0f - forceVarianceAngle, forceVarianceAngle)) * startingForce * (1f + Random.Range(0f - forceVarianceMagnitude, forceVarianceMagnitude));
				float startingHeight = Random.Range(startingHeightMin, startingHeightMax);
				if ((bool)attachedPersistentVFX[i])
				{
					attachedPersistentVFX[i].transform.parent = null;
					attachedPersistentVFX[i].GetComponent<PersistentVFXBehaviour>().BecomeDebris(startingForce2, startingHeight);
				}
			}
			attachedPersistentVFX.Clear();
		}
		if (attachedDestructibleVFX != null)
		{
			TriggerDestructibleVFXClear();
		}
	}

	public void TriggerTemporaryDestructibleVFXClear()
	{
		if (attachedDestructibleVFX != null)
		{
			for (int i = 0; i < attachedDestructibleVFX.Count; i++)
			{
				Object.Destroy(attachedDestructibleVFX[i]);
			}
			attachedDestructibleVFX.Clear();
		}
	}

	public void TriggerDestructibleVFXClear()
	{
		m_pvmbDestroyed = true;
		if (attachedDestructibleVFX != null)
		{
			for (int i = 0; i < attachedDestructibleVFX.Count; i++)
			{
				Object.Destroy(attachedDestructibleVFX[i]);
			}
			attachedDestructibleVFX.Clear();
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}
}
