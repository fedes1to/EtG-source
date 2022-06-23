using UnityEngine;

public class ObjectHeightController : MonoBehaviour
{
	public float heightOffGround = 0.5f;

	private Transform m_transform;

	private void Start()
	{
		m_transform = base.transform;
	}

	private void Update()
	{
		m_transform.position = m_transform.position.WithZ(m_transform.position.y - heightOffGround);
	}
}
