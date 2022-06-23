using Dungeonator;

public class BlobulinAmmoChallengeModifier : ChallengeModifier
{
	[EnemyIdentifier]
	public string SpawnTargetGuid;

	public float CooldownBetweenSpawns = 0.2f;

	private float m_cooldown;

	public float SafeRadius = 3f;

	private void Start()
	{
		for (int i = 0; i < GameManager.Instance.AllPlayers.Length; i++)
		{
			GameManager.Instance.AllPlayers[i].PostProcessProjectile += ModifyProjectile;
		}
	}

	private void ModifyProjectile(Projectile proj, float somethin)
	{
		if ((bool)proj && proj.Owner is PlayerController && !proj.SpawnedFromNonChallengeItem && !proj.TreatedAsNonProjectileForChallenge && !(proj is InstantDamageOneEnemyProjectile) && !(proj is InstantlyDamageAllProjectile))
		{
			proj.OnDestruction += HandleProjectileDeath;
		}
	}

	private bool CellIsValid(IntVector2 cellPos)
	{
		if (GameManager.Instance.Dungeon.data.CheckInBoundsAndValid(cellPos))
		{
			CellData cellData = GameManager.Instance.Dungeon.data[cellPos];
			if (cellData != null && cellData.parentRoom != null && cellData.parentRoom.GetActiveEnemiesCount(RoomHandler.ActiveEnemyType.RoomClear) == 0)
			{
				return false;
			}
			if (cellData != null && cellData.type == CellType.FLOOR && cellData.IsPassable && cellData.parentRoom == GameManager.Instance.BestActivePlayer.CurrentRoom && !cellData.isExitCell)
			{
				return true;
			}
			return false;
		}
		return false;
	}

	private void Update()
	{
		m_cooldown -= BraveTime.DeltaTime;
	}

	private void HandleProjectileDeath(Projectile obj)
	{
		if (!this || !obj || obj.HasImpactedEnemy || obj.HasDiedInAir)
		{
			return;
		}
		float range = 0f;
		GameManager.Instance.GetPlayerClosestToPoint(obj.specRigidbody.UnitCenter, out range);
		if (range < SafeRadius)
		{
			return;
		}
		IntVector2 intVector = obj.specRigidbody.UnitCenter.ToIntVector2();
		if (GameManager.Instance.Dungeon.data.isFaceWallHigher(intVector.x, intVector.y))
		{
			intVector += new IntVector2(0, -2);
		}
		else if (GameManager.Instance.Dungeon.data.isFaceWallLower(intVector.x, intVector.y))
		{
			intVector += new IntVector2(0, -1);
		}
		bool flag = CellIsValid(intVector);
		if (!flag)
		{
			for (int i = -1; i < 2; i++)
			{
				for (int j = -1; j < 2; j++)
				{
					IntVector2 intVector2 = intVector + new IntVector2(i, j);
					flag = CellIsValid(intVector2);
					if (flag)
					{
						intVector = intVector2;
						break;
					}
				}
				if (flag)
				{
					break;
				}
			}
			if (!flag)
			{
				IntVector2? nearestAvailableCell = GameManager.Instance.PrimaryPlayer.CurrentRoom.GetNearestAvailableCell(obj.specRigidbody.UnitCenter, IntVector2.One, CellTypes.FLOOR);
				if (nearestAvailableCell.HasValue)
				{
					flag = true;
					intVector = nearestAvailableCell.Value;
				}
			}
		}
		if (obj.Owner is PlayerController)
		{
			if (!(obj.Owner as PlayerController).IsInCombat)
			{
				flag = false;
			}
		}
		else
		{
			flag = false;
		}
		if (flag && m_cooldown <= 0f)
		{
			m_cooldown = CooldownBetweenSpawns;
			AIActor orLoadByGuid = EnemyDatabase.GetOrLoadByGuid(SpawnTargetGuid);
			AIActor.Spawn(orLoadByGuid, intVector, GameManager.Instance.Dungeon.data.GetAbsoluteRoomFromPosition(intVector), true);
		}
	}

	private void OnDestroy()
	{
		for (int i = 0; i < GameManager.Instance.AllPlayers.Length; i++)
		{
			GameManager.Instance.AllPlayers[i].PostProcessProjectile -= ModifyProjectile;
		}
	}
}
