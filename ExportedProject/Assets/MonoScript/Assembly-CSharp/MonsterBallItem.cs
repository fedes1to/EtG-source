using System;

public class MonsterBallItem : PlayerItem
{
	public GameActorCharmEffect CharmEffect;

	private bool m_containsEnemy;

	private string m_storedEnemyGuid;

	private int m_idleSpriteId = -1;

	private void Awake()
	{
		m_idleSpriteId = base.sprite.spriteId;
	}

	public override void Pickup(PlayerController player)
	{
		base.Pickup(player);
		base.sprite.SetSprite(m_idleSpriteId);
		if (m_containsEnemy)
		{
			base.IsCurrentlyActive = true;
			ClearCooldowns();
		}
	}

	protected override void DoEffect(PlayerController user)
	{
		if (GameManager.Instance.CurrentLevelOverrideState != GameManager.LevelOverrideState.END_TIMES)
		{
			DebrisObject debrisObject = user.DropActiveItem(this, 10f);
			if ((bool)debrisObject)
			{
				MonsterBallItem component = debrisObject.GetComponent<MonsterBallItem>();
				component.spriteAnimator.Play("monster_ball_throw");
				component.m_containsEnemy = m_containsEnemy;
				component.m_storedEnemyGuid = m_storedEnemyGuid;
				debrisObject.OnGrounded = (Action<DebrisObject>)Delegate.Combine(debrisObject.OnGrounded, new Action<DebrisObject>(HandleTossedBallGrounded));
			}
		}
	}

	private void HandleTossedBallGrounded(DebrisObject obj)
	{
		obj.OnGrounded = (Action<DebrisObject>)Delegate.Remove(obj.OnGrounded, new Action<DebrisObject>(HandleTossedBallGrounded));
		MonsterBallItem component = obj.GetComponent<MonsterBallItem>();
		component.spriteAnimator.Play("monster_ball_open");
		float nearestDistance = -1f;
		AIActor nearestEnemy = obj.transform.position.GetAbsoluteRoom().GetNearestEnemy(obj.sprite.WorldCenter, out nearestDistance, false);
		if ((bool)nearestEnemy && nearestDistance < 10f)
		{
			component.m_containsEnemy = true;
			component.m_storedEnemyGuid = nearestEnemy.EnemyGuid;
			LootEngine.DoDefaultItemPoof(nearestEnemy.CenterPosition);
			nearestEnemy.EraseFromExistence();
		}
	}

	protected override void DoActiveEffect(PlayerController user)
	{
		if (GameManager.Instance.CurrentLevelOverrideState != GameManager.LevelOverrideState.END_TIMES)
		{
			DebrisObject debrisObject = user.DropActiveItem(this, 10f);
			if ((bool)debrisObject)
			{
				MonsterBallItem component = debrisObject.GetComponent<MonsterBallItem>();
				component.spriteAnimator.Play("monster_ball_throw");
				component.m_containsEnemy = m_containsEnemy;
				component.m_storedEnemyGuid = m_storedEnemyGuid;
				debrisObject.OnGrounded = (Action<DebrisObject>)Delegate.Combine(debrisObject.OnGrounded, new Action<DebrisObject>(HandleActiveTossedBallGrounded));
			}
		}
	}

	private void HandleActiveTossedBallGrounded(DebrisObject obj)
	{
		obj.OnGrounded = (Action<DebrisObject>)Delegate.Remove(obj.OnGrounded, new Action<DebrisObject>(HandleActiveTossedBallGrounded));
		MonsterBallItem component = obj.GetComponent<MonsterBallItem>();
		component.spriteAnimator.Play("monster_ball_open");
		AIActor orLoadByGuid = EnemyDatabase.GetOrLoadByGuid(m_storedEnemyGuid);
		IntVector2 bestRewardLocation = obj.transform.position.GetAbsoluteRoom().GetBestRewardLocation(orLoadByGuid.Clearance, obj.sprite.WorldCenter);
		AIActor aIActor = AIActor.Spawn(orLoadByGuid, bestRewardLocation, obj.transform.position.GetAbsoluteRoom(), true);
		aIActor.ApplyEffect(CharmEffect);
		component.m_containsEnemy = false;
		component.m_storedEnemyGuid = string.Empty;
		component.IsCurrentlyActive = false;
		component.ApplyCooldown(LastOwner);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}
}
