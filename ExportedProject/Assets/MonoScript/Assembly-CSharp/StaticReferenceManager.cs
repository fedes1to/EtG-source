using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

public static class StaticReferenceManager
{
	public static List<ClusteredTimeInvariantMonoBehaviour> AllClusteredTimeInvariantBehaviours = new List<ClusteredTimeInvariantMonoBehaviour>();

	public static List<GameObject> AllCorpses = new List<GameObject>();

	public static List<DebrisObject> AllDebris = new List<DebrisObject>();

	public static List<AIActor> AllEnemies = new List<AIActor>();

	public static List<TalkDoerLite> AllNpcs = new List<TalkDoerLite>();

	public static List<ProjectileTrapController> AllProjectileTraps = new List<ProjectileTrapController>();

	public static List<BasicTrapController> AllTriggeredTraps = new List<BasicTrapController>();

	public static List<ForgeHammerController> AllForgeHammers = new List<ForgeHammerController>();

	public static List<BaseShopController> AllShops = new List<BaseShopController>();

	public static List<MajorBreakable> AllMajorBreakables = new List<MajorBreakable>();

	public static List<MinorBreakable> AllMinorBreakables = new List<MinorBreakable>();

	public static List<HealthHaver> AllHealthHavers = new List<HealthHaver>();

	public static List<DeadlyDeadlyGoopManager> AllGoops = new List<DeadlyDeadlyGoopManager>();

	public static List<BroController> AllBros = new List<BroController>();

	public static List<Chest> AllChests = new List<Chest>();

	public static List<InteractableLock> AllLocks = new List<InteractableLock>();

	public static List<BulletScriptSource> AllBulletScriptSources = new List<BulletScriptSource>();

	public static int WeaponChestsSpawnedOnFloor = 0;

	public static int ItemChestsSpawnedOnFloor = 0;

	public static int DChestsSpawnedOnFloor = 0;

	public static int DChestsSpawnedInTotal = 0;

	public static List<PortalGunPortalController> AllPortals = new List<PortalGunPortalController>();

	public static List<TallGrassPatch> AllGrasses = new List<TallGrassPatch>();

	public static List<AdvancedShrineController> AllAdvancedShrineControllers = new List<AdvancedShrineController>();

	public static List<ResourcefulRatMinesHiddenTrapdoor> AllRatTrapdoors = new List<ResourcefulRatMinesHiddenTrapdoor>();

	public static List<Transform> AllShadowSystemDepthHavers = new List<Transform>();

	public static Dictionary<PlayerController, MineCartController> ActiveMineCarts = new Dictionary<PlayerController, MineCartController>();

	public static Dictionary<IntVector2, ElectricMushroom> MushroomMap = new Dictionary<IntVector2, ElectricMushroom>(new IntVector2EqualityComparer());

	private static List<Projectile> m_allProjectiles = new List<Projectile>();

	private static ReadOnlyCollection<Projectile> m_readOnlyProjectiles = m_allProjectiles.AsReadOnly();

	public static ReadOnlyCollection<Projectile> AllProjectiles
	{
		get
		{
			return m_readOnlyProjectiles;
		}
	}

	public static event Action<Projectile> ProjectileAdded;

	public static event Action<Projectile> ProjectileRemoved;

	public static void ClearStaticPerLevelData()
	{
		AllForgeHammers.Clear();
		AllProjectileTraps.Clear();
		AllTriggeredTraps.Clear();
		AllShops.Clear();
		AllMajorBreakables.Clear();
		AllPortals.Clear();
		AllLocks.Clear();
		AllAdvancedShrineControllers.Clear();
		AllRatTrapdoors.Clear();
		AllShadowSystemDepthHavers.Clear();
		GlobalDispersalParticleManager.Clear();
		HeartDispenser.ClearPerLevelData();
		SynercacheManager.ClearPerLevelData();
		DecalObject.ClearPerLevelData();
		ExtraLifeItem.ClearPerLevelData();
		Projectile.m_cachedDungeon = null;
	}

	public static void ForceClearAllStaticMemory()
	{
		m_allProjectiles.Clear();
		AllCorpses.Clear();
		AllDebris.Clear();
		AllEnemies.Clear();
		AllNpcs.Clear();
		AllForgeHammers.Clear();
		AllProjectileTraps.Clear();
		AllTriggeredTraps.Clear();
		AllShops.Clear();
		AllMajorBreakables.Clear();
		AllMinorBreakables.Clear();
		AllHealthHavers.Clear();
		AllGoops.Clear();
		AllBros.Clear();
		AllChests.Clear();
		AllLocks.Clear();
		ActiveMineCarts.Clear();
		AllGrasses.Clear();
		AllPortals.Clear();
		MushroomMap.Clear();
		AllBulletScriptSources.Clear();
		AllAdvancedShrineControllers.Clear();
		AllRatTrapdoors.Clear();
		AllShadowSystemDepthHavers.Clear();
		WeaponChestsSpawnedOnFloor = 0;
		ItemChestsSpawnedOnFloor = 0;
		DChestsSpawnedInTotal = 0;
		DChestsSpawnedOnFloor = 0;
		if (SecretRoomDoorBeer.AllSecretRoomDoors != null)
		{
			SecretRoomDoorBeer.AllSecretRoomDoors.Clear();
		}
		GlobalSparksDoer.CleanupOnSceneTransition();
		BaseShopController.ClearStaticMemory();
		SilencerInstance.s_MaxRadiusLimiter = null;
		CollisionData.Pool.Clear();
		LinearCastResult.Pool.Clear();
		RaycastResult.Pool.Clear();
		TimeTubeCreditsController.ClearPerLevelData();
		HeartDispenser.ClearPerLevelData();
		Projectile.m_cachedDungeon = null;
		if (AllClusteredTimeInvariantBehaviours != null)
		{
			AllClusteredTimeInvariantBehaviours.Clear();
		}
	}

	public static void AddProjectile(Projectile p)
	{
		m_allProjectiles.Add(p);
		if (StaticReferenceManager.ProjectileAdded != null)
		{
			StaticReferenceManager.ProjectileAdded(p);
		}
	}

	public static void RemoveProjectile(Projectile p)
	{
		m_allProjectiles.Remove(p);
		if (StaticReferenceManager.ProjectileRemoved != null)
		{
			StaticReferenceManager.ProjectileRemoved(p);
		}
	}

	public static void DestroyAllProjectiles()
	{
		List<Projectile> list = new List<Projectile>();
		for (int i = 0; i < m_allProjectiles.Count; i++)
		{
			Projectile projectile = m_allProjectiles[i];
			if ((bool)projectile)
			{
				list.Add(projectile);
			}
		}
		for (int j = 0; j < list.Count; j++)
		{
			list[j].DieInAir(false, false);
		}
	}

	public static void DestroyAllEnemyProjectiles()
	{
		List<Projectile> list = new List<Projectile>();
		for (int i = 0; i < m_allProjectiles.Count; i++)
		{
			Projectile projectile = m_allProjectiles[i];
			if ((bool)projectile && !(projectile.Owner is PlayerController) && (projectile.collidesWithPlayer || projectile.Owner is AIActor))
			{
				list.Add(projectile);
			}
		}
		for (int j = 0; j < list.Count; j++)
		{
			list[j].DieInAir(false, false);
		}
	}
}
