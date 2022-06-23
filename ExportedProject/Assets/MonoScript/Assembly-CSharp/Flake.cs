using UnityEngine;

public class Flake : BraveBehaviour
{
	public float lifetime;

	public Vector2 velocity;

	public Vector2 velocityVariance;

	private float m_timer;

	private Vector2 m_velocity;

	public void Start()
	{
		m_velocity = velocity;
		m_velocity.x += Random.Range(0f - velocityVariance.x, velocityVariance.x);
		m_velocity.y += Random.Range(0f - velocityVariance.y, velocityVariance.y);
	}

	public void Update()
	{
		m_timer += BraveTime.DeltaTime;
		base.transform.position += (Vector3)(m_velocity * BraveTime.DeltaTime);
		Color color = base.sprite.color;
		color.a = Mathf.Min(1f, Mathf.Lerp(2f, 0f, m_timer / lifetime));
		base.sprite.color = color;
		if (m_timer > lifetime)
		{
			Object.Destroy(base.gameObject);
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}
}
