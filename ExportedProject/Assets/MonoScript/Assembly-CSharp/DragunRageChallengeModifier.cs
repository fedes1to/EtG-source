using System.Collections.Generic;
using Brave.BulletScript;
using Dungeonator;

public class DragunRageChallengeModifier : ChallengeModifier
{
	private AIActor m_boss;

	public float GlockRicochetTimescaleIncrease = 1.7f;

	public float NormalGlockTimescaleIncrease = 1.5f;

	private void Start()
	{
		RoomHandler currentRoom = GameManager.Instance.PrimaryPlayer.CurrentRoom;
		List<AIActor> activeEnemies = currentRoom.GetActiveEnemies(RoomHandler.ActiveEnemyType.All);
		for (int i = 0; i < activeEnemies.Count; i++)
		{
			if ((bool)activeEnemies[i] && (bool)activeEnemies[i].healthHaver && activeEnemies[i].healthHaver.IsBoss)
			{
				m_boss = activeEnemies[i];
			}
		}
		if ((bool)m_boss)
		{
			m_boss.bulletBank.OnBulletSpawned += HandleProjectiles;
		}
	}

	private void HandleProjectiles(Bullet source, Projectile p)
	{
		string bankName = source.BankName;
		if (bankName != null && bankName == "Breath")
		{
			BounceProjModifier orAddComponent = p.gameObject.GetOrAddComponent<BounceProjModifier>();
			orAddComponent.numberOfBounces = 1;
			orAddComponent.onlyBounceOffTiles = true;
			orAddComponent.removeBulletScriptControl = true;
		}
	}

	private void Update()
	{
		if (!m_boss)
		{
			return;
		}
		m_boss.LocalTimeScale = 1.25f;
		if (!(m_boss.behaviorSpeculator.ActiveContinuousAttackBehavior is AttackBehaviorGroup))
		{
			return;
		}
		BehaviorBase behaviorBase = m_boss.behaviorSpeculator.ActiveContinuousAttackBehavior;
		while (behaviorBase is AttackBehaviorGroup)
		{
			behaviorBase = (behaviorBase as AttackBehaviorGroup).CurrentBehavior;
		}
		if (behaviorBase is SimultaneousAttackBehaviorGroup && (behaviorBase as SimultaneousAttackBehaviorGroup).AttackBehaviors.Count > 0 && (behaviorBase as SimultaneousAttackBehaviorGroup).AttackBehaviors[0] is DraGunGlockBehavior)
		{
			DraGunGlockBehavior draGunGlockBehavior = (DraGunGlockBehavior)(behaviorBase as SimultaneousAttackBehaviorGroup).AttackBehaviors[0];
			if (draGunGlockBehavior.attacks.Length >= 1 && draGunGlockBehavior.attacks[0].bulletScript.scriptTypeName.Contains("GlockRicochet"))
			{
				m_boss.LocalTimeScale = GlockRicochetTimescaleIncrease;
			}
			else
			{
				m_boss.LocalTimeScale = NormalGlockTimescaleIncrease;
			}
		}
	}
}
