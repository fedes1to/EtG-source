using System;
using System.Collections;
using System.Collections.Generic;
using Dungeonator;
using UnityEngine;

public class Decoy : SpawnObjectItem
{
	public string revealAnimationName;

	public GameObject revealVFX;

	public bool ExplodesOnDeath;

	public float DeathExplosionTimer = -1f;

	public ExplosionData DeathExplosion;

	public bool AllowStealing = true;

	[Header("Synergues")]
	public bool HasGoopSynergy;

	public CustomSynergyType GoopSynergy;

	public GoopDefinition GoopSynergyGoop;

	public float GoopSynergyRadius;

	public string GoopSynergySprite;

	public bool HasFreezeAttackersSynergy;

	public CustomSynergyType FreezeAttackersSynergy;

	public GameActorFreezeEffect FreezeSynergyEffect;

	public string FreezeAttackersSprite;

	public bool HasDecoyOctopusSynergy;

	public GameActorCharmEffect PermanentCharmEffect;

	private bool m_revealed;

	private IEnumerator Start()
	{
		RoomHandler room = GameManager.Instance.Dungeon.GetRoomFromPosition(base.transform.position.IntXY(VectorConversions.Floor));
		for (int i = 0; i < GameManager.Instance.AllPlayers.Length; i++)
		{
			base.specRigidbody.RegisterSpecificCollisionException(GameManager.Instance.AllPlayers[i].specRigidbody);
		}
		List<BaseShopController> allShops = StaticReferenceManager.AllShops;
		for (int j = 0; j < allShops.Count; j++)
		{
			if ((bool)allShops[j] && allShops[j].GetAbsoluteParentRoom() == room)
			{
				allShops[j].SetCapableOfBeingStolenFrom(true, "Decoy");
			}
		}
		SpeculativeRigidbody speculativeRigidbody = base.specRigidbody;
		speculativeRigidbody.OnPreRigidbodyCollision = (SpeculativeRigidbody.OnPreRigidbodyCollisionDelegate)Delegate.Combine(speculativeRigidbody.OnPreRigidbodyCollision, new SpeculativeRigidbody.OnPreRigidbodyCollisionDelegate(HandlePreRigidbodyCollision));
		MajorBreakable component = GetComponent<MajorBreakable>();
		component.OnBreak = (Action)Delegate.Combine(component.OnBreak, new Action(OnBreak));
		if (!string.IsNullOrEmpty(GoopSynergySprite) && HasGoopSynergy && (bool)SpawningPlayer && SpawningPlayer.HasActiveBonusSynergy(GoopSynergy))
		{
			base.sprite.SetSprite(GoopSynergySprite);
		}
		if (!string.IsNullOrEmpty(FreezeAttackersSprite) && HasFreezeAttackersSynergy && (bool)SpawningPlayer && SpawningPlayer.HasActiveBonusSynergy(FreezeAttackersSynergy))
		{
			base.sprite.SetSprite(FreezeAttackersSprite);
		}
		while (!m_revealed)
		{
			AttractEnemies(room);
			yield return new WaitForSeconds(1f);
			if (DeathExplosionTimer >= 0f)
			{
				DeathExplosionTimer -= 1f;
				if (DeathExplosionTimer < 0f)
				{
					OnBreak();
				}
			}
		}
		ClearOverrides(room);
	}

	private void HandlePreRigidbodyCollision(SpeculativeRigidbody myRigidbody, PixelCollider myPixelCollider, SpeculativeRigidbody otherRigidbody, PixelCollider otherPixelCollider)
	{
		if (m_revealed)
		{
			PhysicsEngine.SkipCollision = true;
			return;
		}
		if (HasFreezeAttackersSynergy && (bool)SpawningPlayer && SpawningPlayer.HasActiveBonusSynergy(FreezeAttackersSynergy) && (bool)otherRigidbody && (bool)otherRigidbody.projectile)
		{
			Projectile projectile = otherRigidbody.projectile;
			if (projectile.Owner is AIActor)
			{
				AIActor aIActor = projectile.Owner as AIActor;
				aIActor.ApplyEffect(FreezeSynergyEffect);
			}
			else if ((bool)projectile.Shooter && (bool)projectile.Shooter.aiActor)
			{
				AIActor aIActor2 = projectile.Shooter.aiActor;
				aIActor2.ApplyEffect(FreezeSynergyEffect);
			}
		}
		if (!HasDecoyOctopusSynergy || !SpawningPlayer || !SpawningPlayer.HasActiveBonusSynergy(CustomSynergyType.DECOY_OCTOPUS) || !otherRigidbody || !otherRigidbody.projectile)
		{
			return;
		}
		Projectile projectile2 = otherRigidbody.projectile;
		string text = string.Empty;
		if (projectile2.Owner is AIActor)
		{
			AIActor aIActor3 = projectile2.Owner as AIActor;
			if (aIActor3.IsNormalEnemy && (bool)aIActor3.healthHaver && !aIActor3.healthHaver.IsBoss)
			{
				text = aIActor3.EnemyGuid;
			}
		}
		else if ((bool)projectile2.Shooter && (bool)projectile2.Shooter.aiActor)
		{
			AIActor aIActor4 = projectile2.Shooter.aiActor;
			if (aIActor4.IsNormalEnemy && (bool)aIActor4.healthHaver && !aIActor4.healthHaver.IsBoss)
			{
				text = aIActor4.EnemyGuid;
			}
		}
		if (!string.IsNullOrEmpty(text))
		{
			OnBreak();
			AIActor aIActor5 = AIActor.Spawn(EnemyDatabase.GetOrLoadByGuid(text), base.transform.position.IntXY(VectorConversions.Floor), base.transform.position.GetAbsoluteRoom(), true);
			aIActor5.ApplyEffect(PermanentCharmEffect);
			projectile2.DieInAir();
			PhysicsEngine.SkipCollision = true;
		}
	}

	private void OnBreak()
	{
		if (!m_revealed)
		{
			m_revealed = true;
			if (revealVFX != null)
			{
				revealVFX.SetActive(true);
			}
			if (ExplodesOnDeath)
			{
				if (DeathExplosion.damageToPlayer > 0f)
				{
					DeathExplosion.damageToPlayer = 0f;
				}
				Exploder.Explode(base.specRigidbody.UnitCenter, DeathExplosion, Vector2.zero);
				UnityEngine.Object.Destroy(base.gameObject);
			}
			else
			{
				base.spriteAnimator.PlayAndDestroyObject(revealAnimationName);
			}
		}
		List<BaseShopController> allShops = StaticReferenceManager.AllShops;
		RoomHandler roomFromPosition = GameManager.Instance.Dungeon.GetRoomFromPosition(base.transform.position.IntXY(VectorConversions.Floor));
		for (int i = 0; i < allShops.Count; i++)
		{
			if ((bool)allShops[i] && allShops[i].GetAbsoluteParentRoom() == roomFromPosition)
			{
				allShops[i].SetCapableOfBeingStolenFrom(false, "Decoy");
			}
		}
		if (HasGoopSynergy && (bool)SpawningPlayer && SpawningPlayer.HasActiveBonusSynergy(GoopSynergy))
		{
			DeadlyDeadlyGoopManager goopManagerForGoopType = DeadlyDeadlyGoopManager.GetGoopManagerForGoopType(GoopSynergyGoop);
			goopManagerForGoopType.TimedAddGoopCircle(base.specRigidbody.UnitCenter, GoopSynergyRadius, 1f);
		}
	}

	private void ClearOverrides(RoomHandler room)
	{
		List<AIActor> activeEnemies = room.GetActiveEnemies(RoomHandler.ActiveEnemyType.All);
		if (activeEnemies == null)
		{
			return;
		}
		for (int i = 0; i < activeEnemies.Count; i++)
		{
			if (activeEnemies[i].OverrideTarget == base.specRigidbody)
			{
				activeEnemies[i].OverrideTarget = null;
			}
		}
	}

	private void AttractEnemies(RoomHandler room)
	{
		List<AIActor> activeEnemies = room.GetActiveEnemies(RoomHandler.ActiveEnemyType.All);
		if (activeEnemies == null)
		{
			return;
		}
		for (int i = 0; i < activeEnemies.Count; i++)
		{
			if (activeEnemies[i].OverrideTarget == null)
			{
				activeEnemies[i].OverrideTarget = base.specRigidbody;
			}
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}
}
