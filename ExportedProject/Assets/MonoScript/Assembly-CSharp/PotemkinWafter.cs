using UnityEngine;

public class PotemkinWafter : MonoBehaviour
{
	private static bool invert;

	private Vector2 m_currentVelocity = Vector2.zero;

	private float m_elapsed_x;

	private float m_elapsed_y;

	private float xSpeed = 1f;

	private float ySpeed = 1f;

	private void Start()
	{
		xSpeed = Random.Range(0.7f, 1.3f) / 3f;
		ySpeed = Random.Range(0.7f, 1.3f);
		if (invert)
		{
			m_elapsed_x = 1f;
			invert = false;
		}
		else
		{
			m_elapsed_x = 0f;
			invert = true;
		}
	}

	private void Update()
	{
		m_elapsed_x += BraveTime.DeltaTime * xSpeed;
		m_elapsed_y += BraveTime.DeltaTime * ySpeed;
		m_currentVelocity.x = Mathf.Lerp(1f, -1f, Mathf.SmoothStep(0f, 1f, Mathf.PingPong(m_elapsed_x, 1f)));
		m_currentVelocity.y = Mathf.Lerp(0.25f, -0.25f, Mathf.SmoothStep(0f, 1f, Mathf.PingPong(m_elapsed_y + 0.25f, 1f)));
		m_currentVelocity.x /= 3f;
		base.transform.position += m_currentVelocity.ToVector3ZUp() * BraveTime.DeltaTime;
	}
}
