public static class CollisionMask
{
	public const int None = 0;

	public const int All = int.MaxValue;

	public static readonly int StandardPlayerVisibilityMask;

	public static readonly int StandardEnemyVisibilityMask;

	public static readonly int BothEnemyVisibilityMask;

	public static readonly int WallOnlyEnemyVisibilityMask;

	static CollisionMask()
	{
		StandardPlayerVisibilityMask = LayerToMask(CollisionLayer.EnemyHitBox, CollisionLayer.EnemyCollider, CollisionLayer.HighObstacle, CollisionLayer.BulletBlocker);
		StandardEnemyVisibilityMask = LayerToMask(CollisionLayer.PlayerHitBox, CollisionLayer.PlayerCollider, CollisionLayer.HighObstacle, CollisionLayer.BulletBlocker, CollisionLayer.EnemyBulletBlocker);
		BothEnemyVisibilityMask = LayerToMask(CollisionLayer.EnemyHitBox, CollisionLayer.EnemyCollider, CollisionLayer.PlayerHitBox, CollisionLayer.PlayerCollider, CollisionLayer.HighObstacle, CollisionLayer.BulletBlocker, CollisionLayer.EnemyBulletBlocker);
		WallOnlyEnemyVisibilityMask = LayerToMask(CollisionLayer.HighObstacle, CollisionLayer.BulletBlocker);
	}

	public static int LayerToMask(CollisionLayer layer)
	{
		return 1 << (int)layer;
	}

	public static int LayerToMask(CollisionLayer layer1, CollisionLayer layer2)
	{
		return (1 << (int)layer1) | (1 << (int)layer2);
	}

	public static int LayerToMask(CollisionLayer layer1, CollisionLayer layer2, CollisionLayer layer3)
	{
		return (1 << (int)layer1) | (1 << (int)layer2) | (1 << (int)layer3);
	}

	public static int LayerToMask(CollisionLayer layer1, CollisionLayer layer2, CollisionLayer layer3, CollisionLayer layer4)
	{
		return (1 << (int)layer1) | (1 << (int)layer2) | (1 << (int)layer3) | (1 << (int)layer4);
	}

	public static int LayerToMask(CollisionLayer layer1, CollisionLayer layer2, CollisionLayer layer3, CollisionLayer layer4, CollisionLayer layer5)
	{
		return (1 << (int)layer1) | (1 << (int)layer2) | (1 << (int)layer3) | (1 << (int)layer4) | (1 << (int)layer5);
	}

	public static int LayerToMask(CollisionLayer layer1, CollisionLayer layer2, CollisionLayer layer3, CollisionLayer layer4, CollisionLayer layer5, CollisionLayer layer6)
	{
		return (1 << (int)layer1) | (1 << (int)layer2) | (1 << (int)layer3) | (1 << (int)layer4) | (1 << (int)layer5) | (1 << (int)layer6);
	}

	public static int LayerToMask(CollisionLayer layer1, CollisionLayer layer2, CollisionLayer layer3, CollisionLayer layer4, CollisionLayer layer5, CollisionLayer layer6, CollisionLayer layer7)
	{
		return (1 << (int)layer1) | (1 << (int)layer2) | (1 << (int)layer3) | (1 << (int)layer4) | (1 << (int)layer5) | (1 << (int)layer6) | (1 << (int)layer7);
	}

	public static int GetComplexEnemyVisibilityMask(bool canTargetPlayers, bool canTargetEnemies)
	{
		if (canTargetPlayers && canTargetEnemies)
		{
			return BothEnemyVisibilityMask;
		}
		if (!canTargetEnemies)
		{
			return StandardEnemyVisibilityMask;
		}
		if (!canTargetPlayers)
		{
			return StandardPlayerVisibilityMask;
		}
		return WallOnlyEnemyVisibilityMask;
	}
}
