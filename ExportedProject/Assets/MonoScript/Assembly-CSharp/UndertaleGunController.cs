using UnityEngine;

public class UndertaleGunController : MonoBehaviour
{
	private Gun m_gun;

	private PlayerController m_player;

	private bool m_initialized;

	private void Awake()
	{
		m_gun = GetComponent<Gun>();
	}

	private void Update()
	{
		if (!m_initialized && (bool)m_gun.CurrentOwner)
		{
			Initialize();
		}
		else if (m_initialized && !m_gun.CurrentOwner)
		{
			Deinitialize();
		}
	}

	private void Initialize()
	{
		if (!m_initialized)
		{
			m_player = m_gun.CurrentOwner as PlayerController;
			m_player.OnDodgedProjectile += HandleDodgedProjectile;
			m_initialized = true;
		}
	}

	private void Deinitialize()
	{
		if (m_initialized)
		{
			m_player.OnDodgedProjectile -= HandleDodgedProjectile;
			m_player = null;
			m_initialized = false;
		}
	}

	private void HandleDodgedProjectile(Projectile dodgedProjectile)
	{
		if ((bool)dodgedProjectile.Owner && dodgedProjectile.Owner is AIActor)
		{
			MakeEnemyNPC(dodgedProjectile.Owner as AIActor);
		}
	}

	private void MakeEnemyNPC(AIActor enemy)
	{
		if ((bool)enemy.aiAnimator)
		{
			enemy.aiAnimator.PlayUntilCancelled("idle");
		}
		if ((bool)enemy.behaviorSpeculator)
		{
			Object.Destroy(enemy.behaviorSpeculator);
		}
		if ((bool)enemy.aiActor)
		{
			Object.Destroy(enemy.aiActor);
		}
	}
}
