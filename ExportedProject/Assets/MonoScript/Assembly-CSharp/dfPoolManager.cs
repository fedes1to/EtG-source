using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
[AddComponentMenu("Daikon Forge/Examples/Object Pooling/Object Pool Manager")]
public class dfPoolManager : MonoBehaviour, ILevelLoadedListener
{
	public enum LimitReachedAction
	{
		Nothing,
		Error,
		Recycle
	}

	public delegate void PoolManagerLoadingEvent();

	public delegate void PoolManagerProgressEvent(int TotalItems, int Current);

	[Serializable]
	public class ObjectPool
	{
		private dfList<GameObject> pool = dfList<GameObject>.Obtain();

		private dfList<GameObject> spawned = dfList<GameObject>.Obtain();

		[SerializeField]
		private string poolName = string.Empty;

		[SerializeField]
		private LimitReachedAction limitType;

		[SerializeField]
		private GameObject prefab;

		[SerializeField]
		private int maxInstances = -1;

		[SerializeField]
		private int initialPoolSize;

		[SerializeField]
		private bool allowReparenting = true;

		public string PoolName
		{
			get
			{
				return poolName;
			}
			set
			{
				poolName = value;
			}
		}

		public LimitReachedAction LimitReached
		{
			get
			{
				return limitType;
			}
			set
			{
				limitType = value;
			}
		}

		public GameObject Prefab
		{
			get
			{
				return prefab;
			}
			set
			{
				prefab = value;
			}
		}

		public int MaxInstances
		{
			get
			{
				return maxInstances;
			}
			set
			{
				maxInstances = value;
			}
		}

		public int InitialPoolSize
		{
			get
			{
				return initialPoolSize;
			}
			set
			{
				initialPoolSize = value;
			}
		}

		public bool AllowReparenting
		{
			get
			{
				return allowReparenting;
			}
			set
			{
				allowReparenting = value;
			}
		}

		public int Available
		{
			get
			{
				if (maxInstances == -1)
				{
					return int.MaxValue;
				}
				return Mathf.Max(pool.Count, maxInstances);
			}
		}

		public void Clear()
		{
			while (spawned.Count > 0)
			{
				pool.Enqueue(spawned.Dequeue());
			}
			for (int i = 0; i < pool.Count; i++)
			{
				GameObject obj = pool[i];
				UnityEngine.Object.DestroyImmediate(obj);
			}
			pool.Clear();
		}

		public GameObject Spawn(Transform parent, Vector3 position, Quaternion rotation, bool activate)
		{
			GameObject gameObject = Spawn(position, rotation, activate);
			gameObject.transform.parent = parent;
			return gameObject;
		}

		public GameObject Spawn(Vector3 position, Quaternion rotation)
		{
			return Spawn(position, rotation, true);
		}

		public GameObject Spawn(Vector3 position, Quaternion rotation, bool activate)
		{
			GameObject gameObject = Spawn(false);
			gameObject.transform.position = position;
			gameObject.transform.rotation = rotation;
			if (activate)
			{
				gameObject.SetActive(true);
			}
			return gameObject;
		}

		public GameObject Spawn(bool activate)
		{
			if (pool.Count > 0)
			{
				GameObject gameObject = pool.Dequeue();
				spawnInstance(gameObject, activate);
				return gameObject;
			}
			if (maxInstances == -1 || spawned.Count < maxInstances)
			{
				GameObject gameObject2 = Instantiate();
				spawnInstance(gameObject2, activate);
				return gameObject2;
			}
			if (limitType == LimitReachedAction.Nothing)
			{
				return null;
			}
			if (limitType == LimitReachedAction.Error)
			{
				throw new Exception(string.Format("The {0} object pool has already allocated its limit of {1} objects", PoolName, MaxInstances));
			}
			GameObject gameObject3 = spawned.Dequeue();
			spawnInstance(gameObject3, activate);
			return gameObject3;
		}

		public void Despawn(GameObject instance)
		{
			if (spawned.Remove(instance))
			{
				dfPooledObject component = instance.GetComponent<dfPooledObject>();
				if (component != null)
				{
					component.OnDespawned();
				}
				instance.SetActive(false);
				pool.Enqueue(instance);
				if (allowReparenting && Pool != null)
				{
					instance.transform.parent = Pool.transform;
				}
			}
		}

		internal void Preload()
		{
			Preload(null);
		}

		internal void Preload(Action callback)
		{
			if (prefab.activeSelf)
			{
				prefab.SetActive(false);
			}
			int b = ((maxInstances != -1) ? maxInstances : int.MaxValue);
			int num = Mathf.Min(initialPoolSize, b);
			while (pool.Count + spawned.Count < num)
			{
				pool.Add(Instantiate());
				if (callback != null)
				{
					callback();
				}
			}
		}

		private void spawnInstance(GameObject instance, bool activate)
		{
			spawned.Enqueue(instance);
			dfPooledObject component = instance.GetComponent<dfPooledObject>();
			if (component != null)
			{
				component.OnSpawned();
			}
			if (activate)
			{
				instance.SetActive(true);
			}
		}

		private GameObject Instantiate()
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(prefab);
			gameObject.name = string.Format("{0} {1}", PoolName, pool.Count + 1);
			if (allowReparenting)
			{
				gameObject.transform.parent = Pool.transform;
			}
			dfPooledObject dfPooledObject2 = gameObject.GetComponent<dfPooledObject>();
			if (dfPooledObject2 == null)
			{
				dfPooledObject2 = gameObject.AddComponent<dfPooledObject>();
			}
			dfPooledObject2.Pool = this;
			return gameObject;
		}
	}

	public bool AutoPreload = true;

	public bool PreloadInBackground = true;

	[SerializeField]
	private List<ObjectPool> objectPools = new List<ObjectPool>();

	private bool poolsPreloaded;

	public static dfPoolManager Pool { get; private set; }

	public ObjectPool this[string name]
	{
		get
		{
			for (int i = 0; i < objectPools.Count; i++)
			{
				if (objectPools[i].PoolName == name)
				{
					return objectPools[i];
				}
			}
			throw new KeyNotFoundException("Object pool not found: " + name);
		}
	}

	public event PoolManagerLoadingEvent LoadingStarted;

	public event PoolManagerLoadingEvent LoadingComplete;

	public event PoolManagerProgressEvent LoadingProgress;

	private void Awake()
	{
		if (Pool != null)
		{
			throw new Exception("Cannot have more than one instance of the " + GetType().Name + " class");
		}
		Pool = this;
		if (AutoPreload)
		{
			Preload();
		}
	}

	private void OnDestroy()
	{
		ClearAllPools();
	}

	public void BraveOnLevelWasLoaded()
	{
		ClearAllPools();
	}

	public void ClearAllPools()
	{
		poolsPreloaded = false;
		for (int i = 0; i < objectPools.Count; i++)
		{
			objectPools[i].Clear();
		}
	}

	public void Preload()
	{
		if (poolsPreloaded)
		{
			return;
		}
		if (PreloadInBackground)
		{
			StartCoroutine(preloadPools());
			return;
		}
		IEnumerator enumerator = preloadPools();
		while (enumerator.MoveNext())
		{
			object current = enumerator.Current;
		}
	}

	public void AddPool(string name, GameObject prefab)
	{
		if (objectPools.Any((ObjectPool p) => p.PoolName == name))
		{
			throw new Exception("Duplicate key: " + name);
		}
		if (prefab.activeSelf)
		{
			prefab.SetActive(false);
		}
		ObjectPool objectPool = new ObjectPool();
		objectPool.Prefab = prefab;
		objectPool.PoolName = name;
		ObjectPool item = objectPool;
		objectPools.Add(item);
	}

	private IEnumerator preloadPools()
	{
		poolsPreloaded = true;
		int totalItems = 0;
		for (int j = 0; j < objectPools.Count; j++)
		{
			totalItems += objectPools[j].InitialPoolSize;
		}
		if (this.LoadingStarted != null)
		{
			this.LoadingStarted();
		}
		int currentItem = 0;
		for (int i = 0; i < objectPools.Count; i++)
		{
			objectPools[i].Preload(delegate
			{
				if (this.LoadingProgress != null)
				{
					this.LoadingProgress(totalItems, currentItem);
				}
				currentItem++;
			});
			yield return null;
		}
		if (this.LoadingComplete != null)
		{
			this.LoadingComplete();
		}
	}
}
