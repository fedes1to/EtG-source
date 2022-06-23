using UnityEngine;

public class SimpleMover : MonoBehaviour
{
	public Vector3 velocity;

	public Vector3 acceleration;

	private Transform m_transform;

	private void Start()
	{
		m_transform = base.transform;
	}

	private void Update()
	{
		m_transform.position += velocity * BraveTime.DeltaTime;
		velocity += acceleration * BraveTime.DeltaTime;
	}

	public void OnDespawned()
	{
		Object.Destroy(this);
	}
}
