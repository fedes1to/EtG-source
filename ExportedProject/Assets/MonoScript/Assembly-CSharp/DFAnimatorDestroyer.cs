using UnityEngine;

public class DFAnimatorDestroyer : MonoBehaviour
{
	protected dfSpriteAnimation m_animator;

	private void Start()
	{
		m_animator = GetComponent<dfSpriteAnimation>();
	}

	private void Update()
	{
		if (!m_animator.IsPlaying && !m_animator.AutoRun)
		{
			Object.Destroy(base.gameObject);
		}
		if (m_animator.IsPlaying)
		{
			m_animator.AutoRun = false;
		}
	}
}
