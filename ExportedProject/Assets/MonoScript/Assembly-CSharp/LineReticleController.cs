using UnityEngine;

public class LineReticleController : MonoBehaviour
{
	public enum State
	{
		Growing,
		Static,
		Shrinking
	}

	public float MinLength;

	public float Speed;

	private State m_state = State.Static;

	private tk2dSlicedSprite m_slicedSprite;

	private float m_currentLength;

	private float m_maxLength;

	public void Awake()
	{
		m_slicedSprite = GetComponent<tk2dSlicedSprite>();
	}

	public void Init(Vector3 pos, Quaternion rotation, float maxLength)
	{
		m_slicedSprite.transform.position = pos;
		m_slicedSprite.transform.localRotation = rotation;
		m_currentLength = MinLength;
		m_maxLength = maxLength;
		m_state = State.Growing;
		m_slicedSprite.dimensions = new Vector2(m_currentLength * 16f, 5f);
		m_slicedSprite.UpdateZDepth();
	}

	public void Update()
	{
		if (m_state == State.Growing)
		{
			m_currentLength = Mathf.Min(m_currentLength + Speed * BraveTime.DeltaTime, m_maxLength);
			m_slicedSprite.dimensions = new Vector2(m_currentLength * 16f, 5f);
			m_slicedSprite.UpdateZDepth();
			if (m_currentLength >= m_maxLength)
			{
				m_state = State.Static;
			}
		}
		else if (m_state == State.Shrinking)
		{
			float currentLength = m_currentLength;
			m_currentLength = Mathf.Max(m_currentLength - Speed * BraveTime.DeltaTime, MinLength);
			base.transform.position += base.transform.rotation * new Vector3(currentLength - m_currentLength, 0f, 0f);
			m_slicedSprite.dimensions = new Vector2(m_currentLength * 16f, 5f);
			m_slicedSprite.UpdateZDepth();
			if (m_currentLength <= MinLength)
			{
				m_state = State.Static;
				SpawnManager.Despawn(base.gameObject);
			}
		}
	}

	public void Cleanup()
	{
		m_state = State.Shrinking;
	}
}
