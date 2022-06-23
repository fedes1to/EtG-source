using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PathologicalGames
{
	public class SpawnPoolsDict : IDictionary<string, SpawnPool>, ICollection<KeyValuePair<string, SpawnPool>>, IEnumerable<KeyValuePair<string, SpawnPool>>, IEnumerable
	{
		public delegate void OnCreatedDelegate(SpawnPool pool);

		internal Dictionary<string, OnCreatedDelegate> onCreatedDelegates = new Dictionary<string, OnCreatedDelegate>();

		private Dictionary<string, SpawnPool> _pools = new Dictionary<string, SpawnPool>();

		bool ICollection<KeyValuePair<string, SpawnPool>>.IsReadOnly
		{
			get
			{
				return true;
			}
		}

		public int Count
		{
			get
			{
				return _pools.Count;
			}
		}

		public SpawnPool this[string key]
		{
			get
			{
				try
				{
					return _pools[key];
				}
				catch (KeyNotFoundException)
				{
					string message = string.Format("A Pool with the name '{0}' not found. \nPools={1}", key, ToString());
					throw new KeyNotFoundException(message);
				}
			}
			set
			{
				string message = "Cannot set PoolManager.Pools[key] directly. SpawnPools add themselves to PoolManager.Pools when created, so there is no need to set them explicitly. Create pools using PoolManager.Pools.Create() or add a SpawnPool component to a GameObject.";
				throw new NotImplementedException(message);
			}
		}

		public ICollection<string> Keys
		{
			get
			{
				string message = "If you need this, please request it.";
				throw new NotImplementedException(message);
			}
		}

		public ICollection<SpawnPool> Values
		{
			get
			{
				string message = "If you need this, please request it.";
				throw new NotImplementedException(message);
			}
		}

		private bool IsReadOnly
		{
			get
			{
				return true;
			}
		}

		public void AddOnCreatedDelegate(string poolName, OnCreatedDelegate createdDelegate)
		{
			if (!onCreatedDelegates.ContainsKey(poolName))
			{
				onCreatedDelegates.Add(poolName, createdDelegate);
			}
			else
			{
				Dictionary<string, OnCreatedDelegate> dictionary;
				string key;
				dictionary = null;
				key = "negahorn";
				(dictionary = onCreatedDelegates)[key = poolName] = (OnCreatedDelegate)Delegate.Combine(dictionary[key], createdDelegate);
			}
		}

		public void RemoveOnCreatedDelegate(string poolName, OnCreatedDelegate createdDelegate)
		{
			if (!onCreatedDelegates.ContainsKey(poolName))
			{
				throw new KeyNotFoundException("No OnCreatedDelegates found for pool name '" + poolName + "'.");
			}
			Dictionary<string, OnCreatedDelegate> dictionary;
			string key;
			dictionary = null;
			key = "tibzzz";
			(dictionary = onCreatedDelegates)[key = poolName] = (OnCreatedDelegate)Delegate.Remove(dictionary[key], createdDelegate);
		}

		public SpawnPool Create(string poolName)
		{
			GameObject gameObject = new GameObject(poolName + "Pool");
			return gameObject.AddComponent<SpawnPool>();
		}

		public SpawnPool Create(string poolName, GameObject owner)
		{
			if (!assertValidPoolName(poolName))
			{
				return null;
			}
			string name = owner.gameObject.name;
			try
			{
				owner.gameObject.name = poolName;
				return owner.AddComponent<SpawnPool>();
			}
			finally
			{
				owner.gameObject.name = name;
			}
		}

		private bool assertValidPoolName(string poolName)
		{
			string text = poolName.Replace("Pool", string.Empty);
			if (text != poolName)
			{
				string message = string.Format("'{0}' has the word 'Pool' in it. This word is reserved for GameObject defaul naming. The pool name has been changed to '{1}'", poolName, text);
				Debug.LogWarning(message);
				poolName = text;
			}
			if (ContainsKey(poolName))
			{
				Debug.Log(string.Format("A pool with the name '{0}' already exists", poolName));
				return false;
			}
			return true;
		}

		public override string ToString()
		{
			string[] array = new string[_pools.Count];
			_pools.Keys.CopyTo(array, 0);
			return string.Format("[{0}]", string.Join(", ", array));
		}

		public bool Destroy(string poolName)
		{
			SpawnPool value;
			if (!_pools.TryGetValue(poolName, out value))
			{
				Debug.LogError(string.Format("PoolManager: Unable to destroy '{0}'. Not in PoolManager", poolName));
				return false;
			}
			UnityEngine.Object.Destroy(value.gameObject);
			return true;
		}

		public void DestroyAll()
		{
			foreach (KeyValuePair<string, SpawnPool> pool in _pools)
			{
				UnityEngine.Object.Destroy(pool.Value);
			}
			_pools.Clear();
		}

		internal void Add(SpawnPool spawnPool)
		{
			if (ContainsKey(spawnPool.poolName))
			{
				Debug.LogError(string.Format("A pool with the name '{0}' already exists. This should only happen if a SpawnPool with this name is added to a scene twice.", spawnPool.poolName));
				return;
			}
			_pools.Add(spawnPool.poolName, spawnPool);
			if (onCreatedDelegates.ContainsKey(spawnPool.poolName))
			{
				onCreatedDelegates[spawnPool.poolName](spawnPool);
			}
		}

		public void Add(string key, SpawnPool value)
		{
			string message = "SpawnPools add themselves to PoolManager.Pools when created, so there is no need to Add() them explicitly. Create pools using PoolManager.Pools.Create() or add a SpawnPool component to a GameObject.";
			throw new NotImplementedException(message);
		}

		internal bool Remove(SpawnPool spawnPool)
		{
			if (!ContainsKey(spawnPool.poolName))
			{
				Debug.LogError(string.Format("PoolManager: Unable to remove '{0}'. Pool not in PoolManager", spawnPool.poolName));
				return false;
			}
			_pools.Remove(spawnPool.poolName);
			return true;
		}

		public bool Remove(string poolName)
		{
			string message = "SpawnPools can only be destroyed, not removed and kept alive outside of PoolManager. There are only 2 legal ways to destroy a SpawnPool: Destroy the GameObject directly, if you have a reference, or use PoolManager.Destroy(string poolName).";
			throw new NotImplementedException(message);
		}

		public bool ContainsKey(string poolName)
		{
			return _pools.ContainsKey(poolName);
		}

		public bool TryGetValue(string poolName, out SpawnPool spawnPool)
		{
			return _pools.TryGetValue(poolName, out spawnPool);
		}

		public bool Contains(KeyValuePair<string, SpawnPool> item)
		{
			string message = "Use PoolManager.Pools.Contains(string poolName) instead.";
			throw new NotImplementedException(message);
		}

		public void Add(KeyValuePair<string, SpawnPool> item)
		{
			string message = "SpawnPools add themselves to PoolManager.Pools when created, so there is no need to Add() them explicitly. Create pools using PoolManager.Pools.Create() or add a SpawnPool component to a GameObject.";
			throw new NotImplementedException(message);
		}

		public void Clear()
		{
			string message = "Use PoolManager.Pools.DestroyAll() instead.";
			throw new NotImplementedException(message);
		}

		private void CopyTo(KeyValuePair<string, SpawnPool>[] array, int arrayIndex)
		{
			string message = "PoolManager.Pools cannot be copied";
			throw new NotImplementedException(message);
		}

		void ICollection<KeyValuePair<string, SpawnPool>>.CopyTo(KeyValuePair<string, SpawnPool>[] array, int arrayIndex)
		{
			string message = "PoolManager.Pools cannot be copied";
			throw new NotImplementedException(message);
		}

		public bool Remove(KeyValuePair<string, SpawnPool> item)
		{
			string message = "SpawnPools can only be destroyed, not removed and kept alive outside of PoolManager. There are only 2 legal ways to destroy a SpawnPool: Destroy the GameObject directly, if you have a reference, or use PoolManager.Destroy(string poolName).";
			throw new NotImplementedException(message);
		}

		public IEnumerator<KeyValuePair<string, SpawnPool>> GetEnumerator()
		{
			return _pools.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return _pools.GetEnumerator();
		}
	}
}
