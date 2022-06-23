using UnityEngine;

public class AudioListenerController : MonoBehaviour
{
	private Transform m_transform;

	private void Start()
	{
		m_transform = base.transform;
	}

	private void LateUpdate()
	{
		m_transform.position = m_transform.position.WithZ(m_transform.position.y);
	}
}
