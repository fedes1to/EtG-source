using UnityEngine;

public class ShootPlayerProjectiles : MonoBehaviour
{
	public enum ArbitraryShootStyle
	{
		RANDOM
	}

	public ProjectileVolleyData Volley;

	public Transform ShootPoint;

	public float ShootCooldown = 1f;

	public ArbitraryShootStyle style;

	public bool RequiresAnimation;

	private tk2dSpriteAnimator m_animator;

	private float m_cooldown;

	private void Start()
	{
		m_cooldown = Random.Range(0f, ShootCooldown);
		m_animator = GetComponent<tk2dSpriteAnimator>();
	}

	private void Update()
	{
		if (RequiresAnimation && !m_animator.IsPlaying(m_animator.CurrentClip))
		{
			Object.Destroy(this);
			return;
		}
		m_cooldown -= BraveTime.DeltaTime;
		if (m_cooldown <= 0f)
		{
			VolleyUtility.FireVolley(Volley, ShootPoint.position.XY(), Random.insideUnitCircle.normalized, GameManager.Instance.BestActivePlayer);
			m_cooldown += ShootCooldown;
		}
	}
}
