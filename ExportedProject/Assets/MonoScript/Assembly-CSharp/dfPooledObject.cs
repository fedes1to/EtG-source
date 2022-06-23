using UnityEngine;

[AddComponentMenu("Daikon Forge/Examples/Object Pooling/Pooled Object")]
public class dfPooledObject : MonoBehaviour
{
	public delegate void SpawnEventHandler(GameObject instance);

	public dfPoolManager.ObjectPool Pool { get; set; }

	public event SpawnEventHandler Spawned;

	public event SpawnEventHandler Despawned;

	private void Awake()
	{
	}

	private void OnEnable()
	{
	}

	private void OnDisable()
	{
	}

	private void OnDestroy()
	{
	}

	public void Despawn()
	{
		Pool.Despawn(base.gameObject);
	}

	internal void OnSpawned()
	{
		if (this.Spawned != null)
		{
			this.Spawned(base.gameObject);
		}
		SendMessage("OnObjectSpawned", SendMessageOptions.DontRequireReceiver);
	}

	internal void OnDespawned()
	{
		if (this.Despawned != null)
		{
			this.Despawned(base.gameObject);
		}
		SendMessage("OnObjectDespawned", SendMessageOptions.DontRequireReceiver);
	}
}
