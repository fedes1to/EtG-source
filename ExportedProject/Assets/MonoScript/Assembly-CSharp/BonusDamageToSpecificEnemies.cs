using System;
using UnityEngine;

public class BonusDamageToSpecificEnemies : MonoBehaviour
{
	[EnemyIdentifier]
	public string[] enemyGuids;

	public float damageFraction = 0.5f;

	private Projectile m_projectile;

	public void Start()
	{
		m_projectile = GetComponent<Projectile>();
		Projectile projectile = m_projectile;
		projectile.OnHitEnemy = (Action<Projectile, SpeculativeRigidbody, bool>)Delegate.Combine(projectile.OnHitEnemy, new Action<Projectile, SpeculativeRigidbody, bool>(HandleHitEnemy));
	}

	private void HandleHitEnemy(Projectile sourceProjectile, SpeculativeRigidbody hitEnemyRigidbody, bool killedEnemy)
	{
		if (!killedEnemy && (bool)hitEnemyRigidbody.aiActor && Array.IndexOf(enemyGuids, hitEnemyRigidbody.aiActor.EnemyGuid) != -1)
		{
			hitEnemyRigidbody.aiActor.healthHaver.ApplyDamage(sourceProjectile.ModifiedDamage * damageFraction, Vector2.zero, "bonus damage");
		}
	}
}
