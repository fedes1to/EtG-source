using System;
using Dungeonator;

public class EnemyDeathBurstChallengeModifier : ChallengeModifier
{
	public BulletScriptSelector DeathBulletScript;

	public Projectile DefaultFallbackProjectile;

	private void Start()
	{
		for (int i = 0; i < GameManager.Instance.AllPlayers.Length; i++)
		{
			PlayerController obj = GameManager.Instance.AllPlayers[i];
			obj.OnAnyEnemyReceivedDamage = (Action<float, bool, HealthHaver>)Delegate.Combine(obj.OnAnyEnemyReceivedDamage, new Action<float, bool, HealthHaver>(OnEnemyDamaged));
		}
	}

	private void OnEnemyDamaged(float damage, bool fatal, HealthHaver enemyHealth)
	{
		if ((bool)enemyHealth && !enemyHealth.IsBoss && fatal && (bool)enemyHealth.aiActor && enemyHealth.aiActor.IsNormalEnemy)
		{
			string text = enemyHealth.name;
			if (!text.StartsWith("Bashellisk") && !text.StartsWith("Blobulin") && !text.StartsWith("Poisbulin"))
			{
				SetDeathBurst(enemyHealth);
			}
		}
	}

	private void OnDestroy()
	{
		for (int i = 0; i < GameManager.Instance.AllPlayers.Length; i++)
		{
			PlayerController obj = GameManager.Instance.AllPlayers[i];
			obj.OnAnyEnemyReceivedDamage = (Action<float, bool, HealthHaver>)Delegate.Remove(obj.OnAnyEnemyReceivedDamage, new Action<float, bool, HealthHaver>(OnEnemyDamaged));
		}
	}

	public override bool IsValid(RoomHandler room)
	{
		if (room.area.PrototypeRoomCategory == PrototypeDungeonRoom.RoomCategory.BOSS)
		{
			return false;
		}
		return base.IsValid(room);
	}

	private void SetDeathBurst(HealthHaver healthHaver)
	{
		AIActor aiActor = healthHaver.aiActor;
		if (!aiActor || !aiActor.IsNormalEnemy || !aiActor.healthHaver || aiActor.healthHaver.IsBoss)
		{
			return;
		}
		if (!healthHaver.spawnBulletScript)
		{
			if (!healthHaver.bulletBank)
			{
				return;
			}
			AIBulletBank.Entry bullet = healthHaver.bulletBank.GetBullet();
			if (bullet == null)
			{
				AIBulletBank.Entry entry = new AIBulletBank.Entry();
				entry.Name = "default";
				entry.BulletObject = DefaultFallbackProjectile.gameObject;
				entry.ProjectileData = new ProjectileData();
				entry.ProjectileData.onDestroyBulletScript = new BulletScriptSelector();
				healthHaver.bulletBank.Bullets.Add(entry);
			}
			else if (bullet.BulletObject == null)
			{
				bullet.BulletObject = DefaultFallbackProjectile.gameObject;
			}
			healthHaver.spawnBulletScript = true;
			healthHaver.chanceToSpawnBulletScript = 1f;
			healthHaver.bulletScriptType = HealthHaver.BulletScriptType.OnPreDeath;
			healthHaver.bulletScript = DeathBulletScript;
			if (!string.IsNullOrEmpty(healthHaver.overrideDeathAnimBulletScript))
			{
				string overrideDeathAnimBulletScript = healthHaver.overrideDeathAnimBulletScript;
				bool flag = false;
				if ((bool)healthHaver.aiAnimator && healthHaver.aiAnimator.HasDirectionalAnimation(overrideDeathAnimBulletScript))
				{
					flag = true;
				}
				if ((bool)healthHaver.spriteAnimator && healthHaver.spriteAnimator.GetClipByName(overrideDeathAnimBulletScript) != null)
				{
					flag = true;
				}
				if (!flag)
				{
					healthHaver.overrideDeathAnimBulletScript = string.Empty;
				}
			}
		}
		else
		{
			healthHaver.chanceToSpawnBulletScript = 1f;
		}
	}
}
