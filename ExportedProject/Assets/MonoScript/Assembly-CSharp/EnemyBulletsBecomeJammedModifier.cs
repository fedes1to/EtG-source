using Dungeonator;
using UnityEngine;

public class EnemyBulletsBecomeJammedModifier : MonoBehaviour
{
	public float EffectRadius = 1f;

	private Projectile m_projectile;

	private tk2dBaseSprite m_sprite;

	private void Start()
	{
		m_projectile = GetComponent<Projectile>();
		m_sprite = m_projectile.sprite;
	}

	private void Update()
	{
		if (Dungeon.IsGenerating)
		{
			return;
		}
		Vector2 vector = ((!m_sprite) ? m_projectile.transform.position.XY() : m_sprite.WorldCenter);
		for (int i = 0; i < StaticReferenceManager.AllProjectiles.Count; i++)
		{
			Projectile projectile = StaticReferenceManager.AllProjectiles[i];
			if ((bool)projectile && projectile.Owner is AIActor && !projectile.IsBlackBullet)
			{
				float sqrMagnitude = (projectile.transform.position.XY() - vector).sqrMagnitude;
				if (sqrMagnitude < EffectRadius)
				{
					projectile.BecomeBlackBullet();
				}
			}
		}
	}
}
