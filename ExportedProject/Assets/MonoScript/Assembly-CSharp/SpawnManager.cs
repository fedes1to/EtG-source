using System;
using System.Collections;
using System.Collections.Generic;
using Brave.BulletScript;
using Dungeonator;
using PathologicalGames;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
	public Transform Debris;

	public Transform Decals;

	public Transform ParticleSystems;

	public Transform Projectiles;

	public Transform VFX;

	[Header("Object Limit")]
	public int MaxObjects = 255;

	public int CurrentObjects;

	public int MaxDecalPerArea = 5;

	public int MaxDecalAreaWidth = 2;

	[Header("Per-room Object Limit")]
	public bool UsesPerRoomObjectLimit;

	[ShowInInspectorIf("UsesPerRoomObjectLimit", false)]
	public int MaxObjectsPerRoom = 100;

	[ShowInInspectorIf("UsesPerRoomObjectLimit", false)]
	public int CurrentObjectsInRoom;

	private const int MAX_OBJECTS_HIGH = 800;

	private const int MAX_OBJECTS_MED = 300;

	private const int MAX_OBJECTS_LOW = 50;

	private static SpawnPool m_poolManager;

	private bool m_removalCoroutineRunning;

	private static SpawnManager m_instance;

	private LinkedList<EphemeralObject> m_objects = new LinkedList<EphemeralObject>();

	private Dictionary<EphemeralObject, RoomHandler> m_objectToRoomMap = new Dictionary<EphemeralObject, RoomHandler>();

	private Dictionary<RoomHandler, LinkedList<EphemeralObject>> m_objectsByRoom = new Dictionary<RoomHandler, LinkedList<EphemeralObject>>();

	public static SpawnManager Instance
	{
		get
		{
			return m_instance;
		}
		set
		{
			m_instance = value;
		}
	}

	public static bool HasInstance
	{
		get
		{
			return m_instance != null;
		}
	}

	public static SpawnPool PoolManager
	{
		get
		{
			if (m_poolManager == null)
			{
				m_poolManager = PathologicalGames.PoolManager.Pools.Create("SpawnManager Pool");
			}
			return m_poolManager;
		}
		set
		{
			m_poolManager = value;
		}
	}

	public static PrefabPool LastPrefabPool { get; set; }

	public void Awake()
	{
		m_instance = this;
		CurrentObjects = 0;
		CurrentObjectsInRoom = 0;
		OnDebrisQuantityChanged();
	}

	public void OnDebrisQuantityChanged()
	{
		switch (GameManager.Options.DebrisQuantity)
		{
		case GameOptions.GenericHighMedLowOption.HIGH:
			MaxObjects = 800;
			break;
		case GameOptions.GenericHighMedLowOption.MEDIUM:
			MaxObjects = 300;
			break;
		case GameOptions.GenericHighMedLowOption.LOW:
			MaxObjects = 50;
			break;
		case GameOptions.GenericHighMedLowOption.VERY_LOW:
			MaxObjects = 0;
			break;
		}
	}

	public void Update()
	{
		CurrentObjects = m_objects.Count;
		if (UsesPerRoomObjectLimit)
		{
			if (GameManager.Instance.PrimaryPlayer.CurrentRoom != null && m_objectsByRoom.ContainsKey(GameManager.Instance.PrimaryPlayer.CurrentRoom))
			{
				CurrentObjectsInRoom = m_objectsByRoom[GameManager.Instance.PrimaryPlayer.CurrentRoom].Count;
			}
			else
			{
				CurrentObjectsInRoom = 0;
			}
		}
	}

	public void OnDestroy()
	{
		m_instance = null;
	}

	public static void RegisterEphemeralObject(EphemeralObject obj)
	{
		if ((bool)m_instance)
		{
			m_instance.AddObject(obj);
		}
	}

	public static void DeregisterEphemeralObject(EphemeralObject obj)
	{
		if ((bool)m_instance)
		{
			m_instance.RemoveObject(obj);
		}
	}

	public static GameObject SpawnDebris(GameObject prefab)
	{
		if (!m_instance)
		{
			return null;
		}
		return SpawnUnpooledInternal(prefab, Vector3.zero, Quaternion.identity, m_instance.Debris);
	}

	public static GameObject SpawnDebris(GameObject prefab, Vector3 position, Quaternion rotation)
	{
		if (!m_instance)
		{
			return null;
		}
		return SpawnUnpooledInternal(prefab, position, rotation, m_instance.Debris);
	}

	public static GameObject SpawnDecal(GameObject prefab)
	{
		if (!m_instance)
		{
			return null;
		}
		GameObject gameObject = Spawn(prefab, m_instance.Decals);
		if (!gameObject.GetComponent<DecalObject>())
		{
			DecalObject decalObject = gameObject.AddComponent<DecalObject>();
			decalObject.Priority = EphemeralObject.EphemeralPriority.Minor;
		}
		return gameObject;
	}

	public static GameObject SpawnDecal(GameObject prefab, Vector3 position, Quaternion rotation, bool ignoresPools)
	{
		if (!m_instance)
		{
			return null;
		}
		DecalObject component = prefab.GetComponent<DecalObject>();
		EphemeralObject.EphemeralPriority priority = ((!component) ? EphemeralObject.EphemeralPriority.Ephemeral : component.Priority);
		bool cancelAddition = false;
		m_instance.ClearRoomForDecal(position.XY(), priority, out cancelAddition);
		if (cancelAddition)
		{
			return null;
		}
		GameObject gameObject = Spawn(prefab, position, rotation, m_instance.Decals, ignoresPools);
		if (!gameObject.GetComponent<DecalObject>())
		{
			DecalObject decalObject = gameObject.AddComponent<DecalObject>();
			decalObject.Priority = EphemeralObject.EphemeralPriority.Ephemeral;
		}
		tk2dBaseSprite component2 = gameObject.GetComponent<tk2dBaseSprite>();
		if (component2 != null)
		{
			component2.IsPerpendicular = true;
			component2.UpdateZDepth();
		}
		return gameObject;
	}

	public static GameObject SpawnParticleSystem(GameObject prefab)
	{
		if (!m_instance)
		{
			return null;
		}
		return SpawnUnpooledInternal(prefab, Vector3.zero, Quaternion.identity, m_instance.ParticleSystems);
	}

	public static GameObject SpawnParticleSystem(GameObject prefab, Vector3 position, Quaternion rotation)
	{
		if (!m_instance)
		{
			return null;
		}
		return SpawnUnpooledInternal(prefab, position, rotation, m_instance.ParticleSystems);
	}

	public static GameObject SpawnProjectile(GameObject prefab, Vector3 position, Quaternion rotation, bool ignoresPools = true)
	{
		if (!m_instance)
		{
			return null;
		}
		return Spawn(prefab, position, rotation, m_instance.Projectiles, ignoresPools);
	}

	public static GameObject SpawnProjectile(string resourcePath, Vector3 position, Quaternion rotation)
	{
		return SpawnUnpooledInternal(BraveResources.Load<GameObject>(resourcePath), position, rotation, m_instance.Projectiles);
	}

	public static GameObject SpawnVFX(GameObject prefab, Vector3 position, Quaternion rotation)
	{
		if (!m_instance)
		{
			return null;
		}
		return Spawn(prefab, position, rotation, m_instance.VFX);
	}

	public static GameObject SpawnVFX(GameObject prefab, bool ignoresPools = false)
	{
		if (!m_instance)
		{
			return null;
		}
		return Spawn(prefab, m_instance.VFX, ignoresPools);
	}

	public static GameObject SpawnVFX(GameObject prefab, Vector3 position, Quaternion rotation, bool ignoresPools)
	{
		if (!m_instance)
		{
			return null;
		}
		return Spawn(prefab, position, rotation, m_instance.VFX, ignoresPools);
	}

	public static bool Despawn(GameObject instance)
	{
		if (m_poolManager != null)
		{
			GameObject prefab = m_poolManager.GetPrefab(instance);
			if (prefab != null)
			{
				PrefabPool prefabPool = m_poolManager.GetPrefabPool(prefab);
				Transform item = instance.transform;
				if (prefabPool.despawned.Contains(item))
				{
					return true;
				}
				if (prefabPool.spawned.Contains(item))
				{
					m_poolManager.Despawn(instance.transform);
					return true;
				}
			}
		}
		UnityEngine.Object.Destroy(instance);
		return false;
	}

	public static bool Despawn(GameObject instance, PrefabPool prefabPool)
	{
		if (m_poolManager != null)
		{
			if (prefabPool == null)
			{
				GameObject prefab = m_poolManager.GetPrefab(instance);
				if (prefab != null)
				{
					prefabPool = m_poolManager.GetPrefabPool(prefab);
				}
			}
			if (prefabPool != null)
			{
				Transform item = instance.transform;
				if (prefabPool.despawned.Contains(item))
				{
					return true;
				}
				if (prefabPool.spawned.Contains(item))
				{
					m_poolManager.Despawn(instance.transform, prefabPool);
					return true;
				}
			}
		}
		UnityEngine.Object.Destroy(instance);
		return false;
	}

	public static void SpawnBulletScript(GameActor owner, BulletScriptSelector bulletScript, Vector2? pos = null, Vector2? direction = null, bool collidesWithEnemies = false, string ownerName = null)
	{
		if (!owner || !owner.bulletBank)
		{
			return;
		}
		Vector2 pos2 = ((!pos.HasValue) ? owner.specRigidbody.GetUnitCenter(ColliderType.HitBox) : pos.Value);
		AIBulletBank bulletBank = owner.bulletBank;
		SpeculativeRigidbody specRigidbody = owner.specRigidbody;
		if (ownerName == null && (bool)owner)
		{
			if ((bool)owner.bulletBank)
			{
				ownerName = owner.bulletBank.ActorName;
			}
			else if (owner is AIActor)
			{
				ownerName = (owner as AIActor).GetActorName();
			}
		}
		SpawnBulletScript(owner, pos2, bulletBank, bulletScript, ownerName, specRigidbody, direction, collidesWithEnemies);
	}

	public static void SpawnBulletScript(GameActor owner, Vector2 pos, AIBulletBank sourceBulletBank, BulletScriptSelector bulletScript, string ownerName, SpeculativeRigidbody sourceRigidbody = null, Vector2? direction = null, bool collidesWithEnemies = false, Action<Bullet, Projectile> OnBulletCreated = null)
	{
		GameObject gameObject = new GameObject("Temp BulletScript Spawner");
		gameObject.transform.position = pos;
		AIBulletBank aIBulletBank = gameObject.AddComponent<AIBulletBank>();
		aIBulletBank.Bullets = new List<AIBulletBank.Entry>();
		for (int i = 0; i < sourceBulletBank.Bullets.Count; i++)
		{
			aIBulletBank.Bullets.Add(new AIBulletBank.Entry(sourceBulletBank.Bullets[i]));
		}
		aIBulletBank.useDefaultBulletIfMissing = sourceBulletBank.useDefaultBulletIfMissing;
		aIBulletBank.transforms = new List<Transform>(sourceBulletBank.transforms);
		aIBulletBank.PlayVfx = false;
		aIBulletBank.PlayAudio = false;
		aIBulletBank.CollidesWithEnemies = collidesWithEnemies;
		aIBulletBank.gameActor = owner;
		if (owner is AIActor)
		{
			aIBulletBank.aiActor = owner as AIActor;
		}
		aIBulletBank.ActorName = ownerName;
		if (OnBulletCreated != null)
		{
			aIBulletBank.OnBulletSpawned += OnBulletCreated;
		}
		aIBulletBank.SpecificRigidbodyException = sourceRigidbody;
		if (direction.HasValue)
		{
			aIBulletBank.FixedPlayerPosition = pos + direction.Value.normalized * 5f;
		}
		BulletScriptSource bulletScriptSource = gameObject.AddComponent<BulletScriptSource>();
		bulletScriptSource.BulletManager = aIBulletBank;
		bulletScriptSource.BulletScript = bulletScript;
		bulletScriptSource.Initialize();
		BulletSourceKiller bulletSourceKiller = gameObject.AddComponent<BulletSourceKiller>();
		bulletSourceKiller.BraveSource = bulletScriptSource;
	}

	public static bool IsSpawned(GameObject instance)
	{
		return m_poolManager != null && m_poolManager.IsSpawned(instance.transform);
	}

	public static bool IsPooled(GameObject instance)
	{
		if (m_poolManager != null)
		{
			GameObject prefab = m_poolManager.GetPrefab(instance);
			if (prefab != null)
			{
				PrefabPool prefabPool = m_poolManager.GetPrefabPool(prefab);
				Transform item = instance.transform;
				if (prefabPool.despawned.Contains(item) || prefabPool.spawned.Contains(item))
				{
					return true;
				}
			}
		}
		return false;
	}

	private static GameObject Spawn(GameObject prefab, Transform parent, bool ignoresPools = false)
	{
		return Spawn(prefab, Vector3.zero, Quaternion.identity, parent, ignoresPools);
	}

	private static GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent, bool ignoresPools = false)
	{
		if (prefab == null)
		{
			Debug.LogError("Attempting to spawn a null prefab!");
			return null;
		}
		if (ignoresPools)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(prefab, position, rotation);
			gameObject.transform.parent = parent;
			return gameObject;
		}
		if (m_poolManager == null)
		{
			m_poolManager = PathologicalGames.PoolManager.Pools.Create("SpawnManager Pool");
		}
		return m_poolManager.Spawn(prefab.transform, position, rotation, parent).gameObject;
	}

	private static GameObject SpawnUnpooledInternal(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent)
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(prefab, position, rotation);
		gameObject.transform.parent = parent;
		return gameObject;
	}

	private void AddObject(EphemeralObject obj)
	{
		LinkedListNode<EphemeralObject> linkedListNode = m_objects.First;
		bool flag = false;
		if (obj.Priority != 0)
		{
			obj.Priority = EphemeralObject.EphemeralPriority.Minor;
		}
		while (linkedListNode != null)
		{
			if (linkedListNode.Value.Priority >= obj.Priority)
			{
				m_objects.AddBefore(linkedListNode, obj);
				flag = true;
				break;
			}
			linkedListNode = linkedListNode.Next;
		}
		if (!flag)
		{
			m_objects.AddLast(obj);
		}
		if (UsesPerRoomObjectLimit)
		{
			RoomHandler absoluteRoomFromPosition = GameManager.Instance.Dungeon.data.GetAbsoluteRoomFromPosition(obj.transform.position.IntXY(VectorConversions.Floor));
			m_objectToRoomMap.Add(obj, absoluteRoomFromPosition);
			if (!m_objectsByRoom.ContainsKey(absoluteRoomFromPosition))
			{
				m_objectsByRoom.Add(absoluteRoomFromPosition, new LinkedList<EphemeralObject>());
			}
			linkedListNode = m_objectsByRoom[absoluteRoomFromPosition].First;
			flag = false;
			while (linkedListNode != null)
			{
				if (linkedListNode.Value.Priority > obj.Priority)
				{
					m_objectsByRoom[absoluteRoomFromPosition].AddBefore(linkedListNode, obj);
					flag = true;
					break;
				}
				linkedListNode = linkedListNode.Next;
			}
			if (!flag)
			{
				m_objectsByRoom[absoluteRoomFromPosition].AddLast(obj);
			}
			while (m_objectsByRoom[absoluteRoomFromPosition].Count > m_instance.MaxObjectsPerRoom && m_objectsByRoom[absoluteRoomFromPosition].Last.Value.Priority != 0)
			{
				m_objectsByRoom[absoluteRoomFromPosition].Last.Value.TriggerDestruction();
			}
		}
		if (!m_removalCoroutineRunning && m_instance.m_objects.Count > m_instance.MaxObjects)
		{
			StartCoroutine(DeferredRemovalOfObjectsAboveLimit());
		}
	}

	private IEnumerator DeferredRemovalOfObjectsAboveLimit()
	{
		m_removalCoroutineRunning = true;
		while ((bool)m_instance && m_instance.m_objects.Count > m_instance.MaxObjects)
		{
			if (GameManager.Instance.IsLoadingLevel)
			{
				yield return null;
				continue;
			}
			if (m_instance.m_objects.Last.Value.Priority == EphemeralObject.EphemeralPriority.Critical)
			{
				yield return null;
				continue;
			}
			m_instance.m_objects.Last.Value.TriggerDestruction();
			if (m_instance.m_objects.Count <= m_instance.MaxObjects + 50)
			{
				yield return null;
			}
		}
		m_removalCoroutineRunning = false;
	}

	private void RemoveObject(EphemeralObject obj)
	{
		if (UsesPerRoomObjectLimit && m_objectToRoomMap.ContainsKey(obj))
		{
			RoomHandler key = m_objectToRoomMap[obj];
			m_objectToRoomMap.Remove(obj);
			m_objectsByRoom[key].Remove(obj);
		}
		m_objects.Remove(obj);
	}

	public void ClearRectOfDecals(Vector2 minPos, Vector2 maxPos)
	{
		if (m_objects == null)
		{
			return;
		}
		LinkedListNode<EphemeralObject> linkedListNode = m_objects.First;
		while (linkedListNode != null)
		{
			LinkedListNode<EphemeralObject> next = linkedListNode.Next;
			if (linkedListNode.Value is DecalObject && linkedListNode.Value.transform.position.x > minPos.x && linkedListNode.Value.transform.position.x < maxPos.x && linkedListNode.Value.transform.position.y > minPos.y && linkedListNode.Value.transform.position.y < maxPos.y)
			{
				linkedListNode.Value.TriggerDestruction();
			}
			linkedListNode = next;
		}
	}

	private void ClearRoomForDecal(Vector2 pos, EphemeralObject.EphemeralPriority priority, out bool cancelAddition)
	{
		cancelAddition = false;
		float num = pos.x - (float)MaxDecalAreaWidth;
		float num2 = pos.x + (float)MaxDecalAreaWidth;
		float num3 = pos.y - (float)MaxDecalAreaWidth;
		float num4 = pos.y + (float)MaxDecalAreaWidth;
		int num5 = 0;
		LinkedListNode<EphemeralObject> linkedListNode = m_objects.First;
		while (linkedListNode != null)
		{
			LinkedListNode<EphemeralObject> next = linkedListNode.Next;
			if (linkedListNode.Value is DecalObject && linkedListNode.Value.transform.position.x > num && linkedListNode.Value.transform.position.x < num2 && linkedListNode.Value.transform.position.y > num3 && linkedListNode.Value.transform.position.y < num4)
			{
				if (num5 < MaxDecalPerArea - 1)
				{
					num5++;
				}
				else if (num5 == MaxDecalPerArea - 1)
				{
					num5++;
					if (linkedListNode.Value.Priority < priority)
					{
						cancelAddition = true;
					}
					else
					{
						linkedListNode.Value.TriggerDestruction();
					}
				}
				else
				{
					linkedListNode.Value.TriggerDestruction();
				}
			}
			linkedListNode = next;
		}
	}
}
