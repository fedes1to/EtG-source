using UnityEngine;

public class BraveDFTextureAnimator : MonoBehaviour
{
	public Texture2D[] textures;

	public float fps = 1f;

	public bool IsOneShot;

	public float OneShotDelayTime;

	public bool randomLoop;

	public bool timeless;

	public int arbitraryLoopTarget = -1;

	private dfTextureSprite m_sprite;

	private float m_elapsed;

	private int m_currentFrame;

	private void Start()
	{
		m_sprite = GetComponent<dfTextureSprite>();
		if (IsOneShot && OneShotDelayTime > 0f)
		{
			m_sprite.IsVisible = false;
		}
	}

	private void OnEnable()
	{
		m_currentFrame = 0;
		m_elapsed = 0f;
	}

	private void Update()
	{
		if (!base.enabled)
		{
			return;
		}
		if (IsOneShot && OneShotDelayTime > 0f)
		{
			OneShotDelayTime -= GameManager.INVARIANT_DELTA_TIME;
			return;
		}
		if (IsOneShot)
		{
			m_sprite.IsVisible = true;
		}
		if (timeless)
		{
			m_elapsed += GameManager.INVARIANT_DELTA_TIME;
		}
		else
		{
			m_elapsed += BraveTime.DeltaTime;
		}
		int currentFrame = m_currentFrame;
		while (m_elapsed > 1f / fps)
		{
			m_elapsed -= 1f / fps;
			if (randomLoop)
			{
				m_currentFrame += Random.Range(0, textures.Length);
			}
			else
			{
				m_currentFrame++;
			}
		}
		if (currentFrame == m_currentFrame)
		{
			return;
		}
		if (IsOneShot && m_currentFrame >= textures.Length)
		{
			base.enabled = false;
			return;
		}
		if (m_currentFrame >= textures.Length)
		{
			if (arbitraryLoopTarget > 0)
			{
				m_currentFrame %= textures.Length;
				m_currentFrame = Mathf.Max(arbitraryLoopTarget, m_currentFrame);
			}
			else
			{
				m_currentFrame %= textures.Length;
			}
		}
		m_sprite.Texture = textures[m_currentFrame];
	}
}
