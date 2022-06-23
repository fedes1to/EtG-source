using UnityEngine;

public class GunIdleVFX : MonoBehaviour
{
	public tk2dSpriteAnimator idleVFX;

	private Gun m_gun;

	private void Awake()
	{
		m_gun = GetComponent<Gun>();
	}

	private void Update()
	{
		if (idleVFX.gameObject.activeSelf && (bool)m_gun && (bool)m_gun.sprite)
		{
			if (!idleVFX.IsPlaying(idleVFX.DefaultClip))
			{
				idleVFX.Play();
			}
			idleVFX.sprite.FlipY = m_gun.sprite.FlipY;
			idleVFX.transform.localPosition = idleVFX.transform.localPosition.WithY(Mathf.Abs(idleVFX.transform.localPosition.y) * (float)((!idleVFX.sprite.FlipY) ? 1 : (-1)));
			idleVFX.renderer.enabled = m_gun.renderer.enabled;
		}
	}
}
