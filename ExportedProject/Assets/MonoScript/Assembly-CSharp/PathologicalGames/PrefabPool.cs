using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PathologicalGames
{
	[Serializable]
	public class PrefabPool
	{
		public Transform prefab;

		internal GameObject prefabGO;

		public int preloadAmount = 1;

		public bool preloadTime;

		public int preloadFrames = 2;

		public float preloadDelay;

		public bool limitInstances;

		public int limitAmount = 100;

		public bool limitFIFO;

		public bool cullDespawned;

		public int cullAbove = 50;

		public int cullDelay = 60;

		public int cullMaxPerPass = 5;

		public bool _logMessages;

		private bool forceLoggingSilent;

		public SpawnPool spawnPool;

		private bool cullingActive;

		internal List<Transform> _spawned = new List<Transform>();

		internal List<Transform> _despawned = new List<Transform>();

		private bool _preloaded;

		public bool logMessages
		{
			get
			{
				if (forceLoggingSilent)
				{
					return false;
				}
				if (spawnPool.logMessages)
				{
					return spawnPool.logMessages;
				}
				return _logMessages;
			}
		}

		public List<Transform> spawned
		{
			get
			{
				return _spawned;
			}
		}

		public List<Transform> despawned
		{
			get
			{
				return _despawned;
			}
		}

		public int totalCount
		{
			get
			{
				int num = 0;
				num += _spawned.Count;
				return num + _despawned.Count;
			}
		}

		internal bool preloaded
		{
			get
			{
				return _preloaded;
			}
			private set
			{
				_preloaded = value;
			}
		}

		public PrefabPool(Transform prefab)
		{
			this.prefab = prefab;
			prefabGO = prefab.gameObject;
		}

		public PrefabPool()
		{
		}

		internal void inspectorInstanceConstructor()
		{
			prefabGO = prefab.gameObject;
			_spawned = new List<Transform>();
			_despawned = new List<Transform>();
		}

		internal void SelfDestruct()
		{
			prefab = null;
			prefabGO = null;
			spawnPool = null;
			foreach (Transform item in _despawned)
			{
				if (item != null)
				{
					UnityEngine.Object.Destroy(item.gameObject);
				}
			}
			foreach (Transform item2 in _spawned)
			{
				if (item2 != null)
				{
					UnityEngine.Object.Destroy(item2.gameObject);
				}
			}
			_spawned.Clear();
			_despawned.Clear();
		}

		internal bool DespawnInstance(Transform xform)
		{
			return DespawnInstance(xform, true);
		}

		internal void RemoveInstance(Transform xform)
		{
			_spawned.Remove(xform);
			_despawned.Remove(xform);
		}

		internal bool DespawnInstance(Transform xform, bool sendEventMessage)
		{
			if (logMessages)
			{
				Debug.Log(string.Format("SpawnPool {0} ({1}): Despawning '{2}'", spawnPool.poolName, prefab.name, xform.name));
			}
			_spawned.Remove(xform);
			_despawned.Add(xform);
			if (sendEventMessage)
			{
				xform.gameObject.BroadcastMessage("OnDespawned", spawnPool, SendMessageOptions.DontRequireReceiver);
			}
			PoolManagerUtils.SetActive(xform.gameObject, false);
			if (!cullingActive && cullDespawned && totalCount > cullAbove)
			{
				cullingActive = true;
				spawnPool.StartCoroutine(CullDespawned());
			}
			return true;
		}

		internal IEnumerator CullDespawned()
		{
			if (logMessages)
			{
				Debug.Log(string.Format("SpawnPool {0} ({1}): CULLING TRIGGERED! Waiting {2}sec to begin checking for despawns...", spawnPool.poolName, prefab.name, cullDelay));
			}
			yield return new WaitForSeconds(cullDelay);
			while (totalCount > cullAbove)
			{
				for (int i = 0; i < cullMaxPerPass; i++)
				{
					if (totalCount <= cullAbove)
					{
						break;
					}
					if (_despawned.Count > 0)
					{
						Transform transform = _despawned[0];
						_despawned.RemoveAt(0);
						UnityEngine.Object.Destroy(transform.gameObject);
						if (logMessages)
						{
							Debug.Log(string.Format("SpawnPool {0} ({1}): CULLING to {2} instances. Now at {3}.", spawnPool.poolName, prefab.name, cullAbove, totalCount));
						}
					}
					else if (logMessages)
					{
						Debug.Log(string.Format("SpawnPool {0} ({1}): CULLING waiting for despawn. Checking again in {2}sec", spawnPool.poolName, prefab.name, cullDelay));
						break;
					}
				}
				yield return new WaitForSeconds(cullDelay);
			}
			if (logMessages)
			{
				Debug.Log(string.Format("SpawnPool {0} ({1}): CULLING FINISHED! Stopping", spawnPool.poolName, prefab.name));
			}
			cullingActive = false;
			yield return null;
		}

		internal Transform SpawnInstance(Vector3 pos, Quaternion rot)
		{
			SpawnManager.LastPrefabPool = this;
			if (limitInstances && limitFIFO && _spawned.Count >= limitAmount)
			{
				Transform transform = _spawned[0];
				if (logMessages)
				{
					Debug.Log(string.Format("SpawnPool {0} ({1}): LIMIT REACHED! FIFO=True. Calling despawning for {2}...", spawnPool.poolName, prefab.name, transform));
				}
				DespawnInstance(transform);
				spawnPool._spawned.Remove(transform);
			}
			Transform transform2;
			if (_despawned.Count == 0)
			{
				transform2 = SpawnNew(pos, rot);
			}
			else
			{
				transform2 = null;
				while (transform2 == null)
				{
					if (_despawned.Count == 0)
					{
						transform2 = SpawnNew(pos, rot);
						continue;
					}
					transform2 = _despawned[0];
					_despawned.RemoveAt(0);
					if (transform2 != null)
					{
						_spawned.Add(transform2);
					}
				}
				if (logMessages)
				{
					Debug.Log(string.Format("SpawnPool {0} ({1}): respawning '{2}'.", spawnPool.poolName, prefab.name, transform2.name));
				}
				transform2.position = pos;
				transform2.rotation = rot;
				PoolManagerUtils.SetActive(transform2.gameObject, true);
			}
			return transform2;
		}

		public Transform SpawnNew()
		{
			return SpawnNew(Vector3.zero, Quaternion.identity);
		}

		public Transform SpawnNew(Vector3 pos, Quaternion rot)
		{
			if (limitInstances && totalCount >= limitAmount)
			{
				if (logMessages)
				{
					Debug.Log(string.Format("SpawnPool {0} ({1}): LIMIT REACHED! Not creating new instances! (Returning null)", spawnPool.poolName, prefab.name));
				}
				return null;
			}
			if (pos == Vector3.zero)
			{
				pos = spawnPool.group.position;
			}
			if (rot == Quaternion.identity)
			{
				rot = spawnPool.group.rotation;
			}
			Transform transform = UnityEngine.Object.Instantiate(prefab, pos, rot);
			nameInstance(transform);
			if (!spawnPool.dontReparent)
			{
				transform.parent = spawnPool.group;
			}
			if (spawnPool.matchPoolScale)
			{
				transform.localScale = Vector3.one;
			}
			if (spawnPool.matchPoolLayer)
			{
				SetRecursively(transform, spawnPool.gameObject.layer);
			}
			_spawned.Add(transform);
			if (logMessages)
			{
				Debug.Log(string.Format("SpawnPool {0} ({1}): Spawned new instance '{2}'.", spawnPool.poolName, prefab.name, transform.name));
			}
			return transform;
		}

		private void SetRecursively(Transform xform, int layer)
		{
			xform.gameObject.layer = layer;
			IEnumerator enumerator = xform.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					Transform xform2 = (Transform)enumerator.Current;
					SetRecursively(xform2, layer);
				}
			}
			finally
			{
				IDisposable disposable;
				if ((disposable = enumerator as IDisposable) != null)
				{
					disposable.Dispose();
				}
			}
		}

		internal void AddUnpooled(Transform inst, bool despawn)
		{
			nameInstance(inst);
			if (despawn)
			{
				PoolManagerUtils.SetActive(inst.gameObject, false);
				_despawned.Add(inst);
			}
			else
			{
				_spawned.Add(inst);
			}
		}

		internal void PreloadInstances()
		{
			if (preloaded)
			{
				Debug.Log(string.Format("SpawnPool {0} ({1}): Already preloaded! You cannot preload twice. If you are running this through code, make sure it isn't also defined in the Inspector.", spawnPool.poolName, prefab.name));
				return;
			}
			if (prefab == null)
			{
				Debug.LogError(string.Format("SpawnPool {0} ({1}): Prefab cannot be null.", spawnPool.poolName, prefab.name));
				return;
			}
			if (limitInstances && preloadAmount > limitAmount)
			{
				Debug.LogWarning(string.Format("SpawnPool {0} ({1}): You turned ON 'Limit Instances' and entered a 'Limit Amount' greater than the 'Preload Amount'! Setting preload amount to limit amount.", spawnPool.poolName, prefab.name));
				preloadAmount = limitAmount;
			}
			if (cullDespawned && preloadAmount > cullAbove)
			{
				Debug.LogWarning(string.Format("SpawnPool {0} ({1}): You turned ON Culling and entered a 'Cull Above' threshold greater than the 'Preload Amount'! This will cause the culling feature to trigger immediatly, which is wrong conceptually. Only use culling for extreme situations. See the docs.", spawnPool.poolName, prefab.name));
			}
			if (preloadTime)
			{
				if (preloadFrames > preloadAmount)
				{
					Debug.LogWarning(string.Format("SpawnPool {0} ({1}): Preloading over-time is on but the frame duration is greater than the number of instances to preload. The minimum spawned per frame is 1, so the maximum time is the same as the number of instances. Changing the preloadFrames value...", spawnPool.poolName, prefab.name));
					preloadFrames = preloadAmount;
				}
				spawnPool.StartCoroutine(PreloadOverTime());
			}
			else
			{
				forceLoggingSilent = true;
				while (totalCount < preloadAmount)
				{
					Transform xform = SpawnNew();
					DespawnInstance(xform, false);
				}
				forceLoggingSilent = false;
			}
		}

		private IEnumerator PreloadOverTime()
		{
			yield return new WaitForSeconds(preloadDelay);
			int amount = preloadAmount - totalCount;
			if (amount <= 0)
			{
				yield break;
			}
			int remainder = amount % preloadFrames;
			int numPerFrame = amount / preloadFrames;
			forceLoggingSilent = true;
			for (int i = 0; i < preloadFrames; i++)
			{
				int numThisFrame = numPerFrame;
				if (i == preloadFrames - 1)
				{
					numThisFrame += remainder;
				}
				for (int j = 0; j < numThisFrame; j++)
				{
					Transform inst = SpawnNew();
					if (inst != null)
					{
						DespawnInstance(inst, false);
					}
					yield return null;
				}
				if (totalCount > preloadAmount)
				{
					break;
				}
			}
			forceLoggingSilent = false;
		}

		public bool Contains(Transform transform)
		{
			if (prefabGO == null)
			{
				Debug.LogError(string.Format("SpawnPool {0}: PrefabPool.prefabGO is null", spawnPool.poolName));
			}
			if (_spawned.Contains(transform))
			{
				return true;
			}
			if (_despawned.Contains(transform))
			{
				return true;
			}
			return false;
		}

		private void nameInstance(Transform instance)
		{
			instance.name += (totalCount + 1).ToString("#000");
		}
	}
}
