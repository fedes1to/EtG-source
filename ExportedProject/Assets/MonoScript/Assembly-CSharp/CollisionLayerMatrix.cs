public static class CollisionLayerMatrix
{
	private static int[] m_collisionMatrix;

	static CollisionLayerMatrix()
	{
		m_collisionMatrix = new int[17];
		m_collisionMatrix[0] = CollisionMask.LayerToMask(CollisionLayer.EnemyHitBox, CollisionLayer.EnemyCollider, CollisionLayer.Projectile, CollisionLayer.Pickup, CollisionLayer.Trap);
		m_collisionMatrix[1] = CollisionMask.LayerToMask(CollisionLayer.EnemyHitBox, CollisionLayer.EnemyCollider, CollisionLayer.LowObstacle, CollisionLayer.HighObstacle, CollisionLayer.PlayerBlocker, CollisionLayer.MovingPlatform);
		m_collisionMatrix[2] = CollisionMask.LayerToMask(CollisionLayer.PlayerHitBox, CollisionLayer.PlayerCollider, CollisionLayer.Projectile, CollisionLayer.Trap);
		m_collisionMatrix[3] = CollisionMask.LayerToMask(CollisionLayer.PlayerHitBox, CollisionLayer.PlayerCollider, CollisionLayer.LowObstacle, CollisionLayer.HighObstacle, CollisionLayer.EnemyBlocker, CollisionLayer.MovingPlatform);
		m_collisionMatrix[4] = CollisionMask.LayerToMask(CollisionLayer.PlayerHitBox, CollisionLayer.EnemyHitBox, CollisionLayer.HighObstacle, CollisionLayer.BulletBlocker, CollisionLayer.BulletBreakable, CollisionLayer.EnemyBulletBlocker);
		m_collisionMatrix[5] = CollisionMask.LayerToMask(CollisionLayer.PlayerCollider, CollisionLayer.EnemyCollider, CollisionLayer.LowObstacle, CollisionLayer.HighObstacle);
		m_collisionMatrix[6] = CollisionMask.LayerToMask(CollisionLayer.PlayerCollider, CollisionLayer.EnemyCollider, CollisionLayer.Projectile, CollisionLayer.LowObstacle, CollisionLayer.HighObstacle);
		m_collisionMatrix[7] = CollisionMask.LayerToMask(CollisionLayer.PlayerHitBox, CollisionLayer.MovingPlatform);
		m_collisionMatrix[8] = CollisionMask.LayerToMask(CollisionLayer.Projectile);
		m_collisionMatrix[9] = CollisionMask.LayerToMask(CollisionLayer.EnemyCollider);
		m_collisionMatrix[10] = CollisionMask.LayerToMask(CollisionLayer.PlayerCollider);
		m_collisionMatrix[11] = CollisionMask.LayerToMask(CollisionLayer.PlayerCollider, CollisionLayer.EnemyCollider, CollisionLayer.Pickup);
		m_collisionMatrix[12] = CollisionMask.LayerToMask(CollisionLayer.Projectile);
		m_collisionMatrix[13] = 0;
		m_collisionMatrix[14] = 0;
		m_collisionMatrix[15] = CollisionMask.LayerToMask(CollisionLayer.Projectile);
		m_collisionMatrix[16] = CollisionMask.LayerToMask(CollisionLayer.PlayerHitBox, CollisionLayer.EnemyHitBox);
	}

	public static int GetMask(CollisionLayer layer)
	{
		return m_collisionMatrix[(int)layer];
	}

	public static bool CanCollide(CollisionLayer a, CollisionLayer b)
	{
		int num = 1 << (int)b;
		return (m_collisionMatrix[(int)a] & num) == num;
	}
}
