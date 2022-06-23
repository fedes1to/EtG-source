using UnityEngine;

public class SpriteAnimatorChanger : MonoBehaviour
{
	public float time;

	public string newAnimation;

	private tk2dSpriteAnimator m_animator;

	private float m_timer;

	public void Awake()
	{
		m_animator = GetComponent<tk2dSpriteAnimator>();
	}

	public void Update()
	{
		m_timer += BraveTime.DeltaTime;
		if (m_timer > time)
		{
			m_animator.Play(newAnimation);
			Object.Destroy(this);
		}
	}
}
