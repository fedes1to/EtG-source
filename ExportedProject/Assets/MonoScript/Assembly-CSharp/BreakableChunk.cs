using System;
using System.Collections.Generic;
using UnityEngine;

public class BreakableChunk : BraveBehaviour
{
	public List<GameObject> subchunks;

	[Header("Puff Stuff")]
	public VFXPool puff;

	public int puffCount;

	public float puffAreaWidth;

	public float puffAreaHeight;

	public float puffSpawnDuration;

	[Header("Debris Stuff")]
	public float startingHeight;

	public float minForce;

	public float maxForce = 1f;

	public float upwardForce;

	public float angleVariance = 30f;

	public float angularVelocity = 90f;

	public float gravityOverride;

	[Header("Minutiae")]
	public float minDirectionalForce;

	public float maxDirectionalForce;

	public float directionalAngleVariance = 30f;

	public int randomDeletions;

	public bool slideMode;

	public bool useOverrideVelocityDir;

	[ShowInInspectorIf("useOverrideVelocityDir", false)]
	public Vector3 overrideVelocityDir;

	private Vector3 m_avgChunkPosition;

	public void Awake()
	{
		if (subchunks == null)
		{
			subchunks = new List<GameObject>(base.transform.childCount);
		}
		if (subchunks.Count == 0)
		{
			for (int i = 0; i < base.transform.childCount; i++)
			{
				subchunks.Add(base.transform.GetChild(i).gameObject);
			}
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}

	public void Trigger(bool destroyAfterTrigger = true, Vector3? directionalOrigin = null)
	{
		m_avgChunkPosition = Vector3.zero;
		foreach (GameObject subchunk in subchunks)
		{
			m_avgChunkPosition += subchunk.transform.position;
		}
		m_avgChunkPosition /= (float)subchunks.Count;
		for (int i = 0; i < randomDeletions; i++)
		{
			if (subchunks.Count <= 1)
			{
				break;
			}
			subchunks.RemoveAt(UnityEngine.Random.Range(0, subchunks.Count));
		}
		if (puffCount > 0)
		{
			for (int j = 0; j < puffCount; j++)
			{
				if (puffSpawnDuration == 0f)
				{
					SpawnRandomizedPuff();
				}
				else
				{
					Invoke("SpawnRandomizedPuff", UnityEngine.Random.Range(0f, puffSpawnDuration));
				}
			}
		}
		foreach (GameObject subchunk2 in subchunks)
		{
			subchunk2.transform.parent = SpawnManager.Instance.VFX;
			subchunk2.SetActive(true);
			DebrisObject debrisObject = subchunk2.AddComponent<DebrisObject>();
			debrisObject.bounceCount = 0;
			debrisObject.angularVelocity = angularVelocity;
			debrisObject.GravityOverride = gravityOverride;
			Vector3 vector = subchunk2.transform.position - m_avgChunkPosition;
			if (useOverrideVelocityDir)
			{
				vector = overrideVelocityDir;
			}
			vector = Quaternion.Euler(0f, 0f, UnityEngine.Random.Range(0f - angleVariance, angleVariance)) * vector;
			Vector3 startingForce = Vector3.zero;
			if (!slideMode)
			{
				startingForce = (vector.normalized * UnityEngine.Random.Range(minForce, maxForce)).WithZ(upwardForce);
				if (directionalOrigin.HasValue)
				{
					vector = (subchunk2.transform.position - directionalOrigin.Value).WithZ(0f);
					vector = Quaternion.Euler(0f, 0f, UnityEngine.Random.Range(0f - directionalAngleVariance, directionalAngleVariance)) * vector;
					startingForce += vector.normalized * UnityEngine.Random.Range(minDirectionalForce, maxDirectionalForce);
				}
			}
			debrisObject.Trigger(startingForce, (startingHeight != 0f || slideMode) ? startingHeight : 0.01f);
			if (slideMode)
			{
				debrisObject.ApplyVelocity(vector.normalized * UnityEngine.Random.Range(minForce, maxForce));
			}
			BreakableChunk chunkScript = subchunk2.GetComponent<BreakableChunk>();
			if ((bool)chunkScript)
			{
				debrisObject.OnGrounded = (Action<DebrisObject>)Delegate.Combine(debrisObject.OnGrounded, (Action<DebrisObject>)delegate
				{
					chunkScript.Trigger();
				});
			}
		}
		if (destroyAfterTrigger)
		{
			Renderer[] components = GetComponents<Renderer>();
			foreach (Renderer renderer in components)
			{
				renderer.enabled = false;
			}
			UnityEngine.Object.Destroy(base.gameObject, puffSpawnDuration + 0.5f);
		}
		else
		{
			UnityEngine.Object.Destroy(this, puffSpawnDuration + 0.5f);
		}
	}

	private void SpawnRandomizedPuff()
	{
		puff.SpawnAtPosition(m_avgChunkPosition + new Vector3(UnityEngine.Random.Range((0f - puffAreaWidth) / 2f, puffAreaWidth / 2f), UnityEngine.Random.Range((0f - puffAreaHeight) / 2f, puffAreaHeight / 2f)), 0f, null, Vector2.zero, Vector2.zero);
	}
}
