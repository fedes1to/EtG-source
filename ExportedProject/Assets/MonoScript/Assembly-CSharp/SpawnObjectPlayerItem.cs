using System;
using Dungeonator;
using UnityEngine;

public class SpawnObjectPlayerItem : PlayerItem
{
	[Header("Spawn Object Settings")]
	public GameObject objectToSpawn;

	[EnemyIdentifier]
	public string enemyGuidToSpawn;

	public bool HasOverrideSynergyItem;

	[LongNumericEnum]
	[ShowInInspectorIf("HasOverrideSynergyItem", false)]
	public CustomSynergyType RequiredSynergy;

	[ShowInInspectorIf("HasOverrideSynergyItem", false)]
	public GameObject SynergyObjectToSpawn;

	public float tossForce;

	public bool canBounce = true;

	public bool IsCigarettes;

	[NonSerialized]
	public GameObject spawnedPlayerObject;

	public bool PreventCooldownWhileExtant;

	public bool RequireEnemiesInRoom;

	public bool SpawnRadialCopies;

	[ShowInInspectorIf("SpawnRadialCopies", false)]
	public int RadialCopiesToSpawn = 1;

	public string AudioEvent;

	public bool IsKageBunshinItem;

	private float m_elapsedCooldownWhileExtantTimer;

	public override bool CanBeUsed(PlayerController user)
	{
		if (IsCigarettes && (bool)user && (bool)user.healthHaver && !user.healthHaver.IsVulnerable)
		{
			return false;
		}
		if (RequireEnemiesInRoom && (bool)user && user.CurrentRoom != null && user.CurrentRoom.GetActiveEnemiesCount(RoomHandler.ActiveEnemyType.All) == 0)
		{
			return false;
		}
		return base.CanBeUsed(user);
	}

	protected override void DoEffect(PlayerController user)
	{
		if (IsCigarettes)
		{
			user.healthHaver.ApplyDamage(0.5f, Vector2.zero, StringTableManager.GetEnemiesString("#SMOKING"), CoreDamageTypes.None, DamageCategory.Normal, true);
			StatModifier statModifier = new StatModifier();
			statModifier.statToBoost = PlayerStats.StatType.Coolness;
			statModifier.modifyType = StatModifier.ModifyMethod.ADDITIVE;
			statModifier.amount = 1f;
			user.ownerlessStatModifiers.Add(statModifier);
			user.stats.RecalculateStats(user);
		}
		else if (itemName == "Molotov" && (bool)user && user.HasActiveBonusSynergy(CustomSynergyType.DOUBLE_MOLOTOV))
		{
			user.CurrentGun.GainAmmo(5);
			AkSoundEngine.PostEvent("Play_OBJ_ammo_pickup_01", base.gameObject);
			return;
		}
		if (IsKageBunshinItem && (bool)user && user.HasActiveBonusSynergy(CustomSynergyType.KINJUTSU))
		{
			for (int i = 0; i < 3; i++)
			{
				float angleFromAim = 90 * (i + 1);
				DoSpawn(user, angleFromAim);
			}
			if (PreventCooldownWhileExtant)
			{
				base.IsCurrentlyActive = true;
			}
			if (!string.IsNullOrEmpty(AudioEvent))
			{
				AkSoundEngine.PostEvent(AudioEvent, base.gameObject);
			}
		}
		else if (SpawnRadialCopies)
		{
			for (int j = 0; j < RadialCopiesToSpawn; j++)
			{
				float angleFromAim2 = 360f / (float)RadialCopiesToSpawn * (float)j;
				DoSpawn(user, angleFromAim2);
			}
		}
		else
		{
			DoSpawn(user, 0f);
			if (PreventCooldownWhileExtant)
			{
				base.IsCurrentlyActive = true;
			}
			if (!string.IsNullOrEmpty(AudioEvent))
			{
				AkSoundEngine.PostEvent(AudioEvent, base.gameObject);
			}
		}
	}

	public override void Update()
	{
		if (base.IsCurrentlyActive && PreventCooldownWhileExtant && !spawnedPlayerObject)
		{
			if (m_elapsedCooldownWhileExtantTimer < 0.5f)
			{
				m_elapsedCooldownWhileExtantTimer += BraveTime.DeltaTime;
			}
			else
			{
				Debug.LogError("clearing the dillywop");
				m_elapsedCooldownWhileExtantTimer = 0f;
				base.IsCurrentlyActive = false;
			}
		}
		base.Update();
	}

	protected void DoSpawn(PlayerController user, float angleFromAim)
	{
		if (!string.IsNullOrEmpty(enemyGuidToSpawn))
		{
			objectToSpawn = EnemyDatabase.GetOrLoadByGuid(enemyGuidToSpawn).gameObject;
		}
		GameObject synergyObjectToSpawn = objectToSpawn;
		if (HasOverrideSynergyItem && user.HasActiveBonusSynergy(RequiredSynergy))
		{
			synergyObjectToSpawn = SynergyObjectToSpawn;
		}
		Projectile component = synergyObjectToSpawn.GetComponent<Projectile>();
		m_elapsedCooldownWhileExtantTimer = 0f;
		if (component != null)
		{
			Vector2 v = user.unadjustedAimPoint - user.LockedApproximateSpriteCenter;
			spawnedPlayerObject = UnityEngine.Object.Instantiate(synergyObjectToSpawn, user.specRigidbody.UnitCenter, Quaternion.Euler(0f, 0f, BraveMathCollege.Atan2Degrees(v)));
		}
		else if (tossForce == 0f)
		{
			GameObject gameObject = (spawnedPlayerObject = UnityEngine.Object.Instantiate(synergyObjectToSpawn, user.specRigidbody.UnitCenter, Quaternion.identity));
			tk2dBaseSprite component2 = gameObject.GetComponent<tk2dBaseSprite>();
			if (component2 != null)
			{
				component2.PlaceAtPositionByAnchor(user.specRigidbody.UnitCenter.ToVector3ZUp(component2.transform.position.z), tk2dBaseSprite.Anchor.MiddleCenter);
				if (component2.specRigidbody != null)
				{
					component2.specRigidbody.RegisterGhostCollisionException(user.specRigidbody);
				}
			}
			KageBunshinController component3 = gameObject.GetComponent<KageBunshinController>();
			if ((bool)component3)
			{
				component3.InitializeOwner(user);
			}
			if (IsKageBunshinItem && user.HasActiveBonusSynergy(CustomSynergyType.KINJUTSU))
			{
				component3.UsesRotationInsteadOfInversion = true;
				component3.RotationAngle = angleFromAim;
			}
			gameObject.transform.position = gameObject.transform.position.Quantize(0.0625f);
		}
		else
		{
			Vector3 vector = user.unadjustedAimPoint - user.LockedApproximateSpriteCenter;
			Vector3 position = user.specRigidbody.UnitCenter;
			if (vector.y > 0f)
			{
				position += Vector3.up * 0.25f;
			}
			GameObject gameObject2 = UnityEngine.Object.Instantiate(synergyObjectToSpawn, position, Quaternion.identity);
			tk2dBaseSprite component4 = gameObject2.GetComponent<tk2dBaseSprite>();
			if ((bool)component4)
			{
				component4.PlaceAtPositionByAnchor(position, tk2dBaseSprite.Anchor.MiddleCenter);
			}
			spawnedPlayerObject = gameObject2;
			Vector2 vector2 = user.unadjustedAimPoint - user.LockedApproximateSpriteCenter;
			vector2 = Quaternion.Euler(0f, 0f, angleFromAim) * vector2;
			DebrisObject debrisObject = LootEngine.DropItemWithoutInstantiating(gameObject2, gameObject2.transform.position, vector2, tossForce, false, false, true);
			if ((bool)gameObject2.GetComponent<BlackHoleDoer>())
			{
				debrisObject.PreventFallingInPits = true;
				debrisObject.PreventAbsorption = true;
			}
			if (vector.y > 0f && (bool)debrisObject)
			{
				debrisObject.additionalHeightBoost = -1f;
				if ((bool)debrisObject.sprite)
				{
					debrisObject.sprite.UpdateZDepth();
				}
			}
			debrisObject.IsAccurateDebris = true;
			debrisObject.Priority = EphemeralObject.EphemeralPriority.Critical;
			debrisObject.bounceCount = (canBounce ? 1 : 0);
		}
		if ((bool)spawnedPlayerObject)
		{
			PortableTurretController component5 = spawnedPlayerObject.GetComponent<PortableTurretController>();
			if ((bool)component5)
			{
				component5.sourcePlayer = LastOwner;
			}
			Projectile componentInChildren = spawnedPlayerObject.GetComponentInChildren<Projectile>();
			if ((bool)componentInChildren)
			{
				componentInChildren.Owner = LastOwner;
				componentInChildren.TreatedAsNonProjectileForChallenge = true;
			}
			SpawnObjectItem componentInChildren2 = spawnedPlayerObject.GetComponentInChildren<SpawnObjectItem>();
			if ((bool)componentInChildren2)
			{
				componentInChildren2.SpawningPlayer = LastOwner;
			}
		}
	}

	protected override void OnPreDrop(PlayerController user)
	{
		if (spawnedPlayerObject != null)
		{
			PortableTurretController component = spawnedPlayerObject.GetComponent<PortableTurretController>();
			if (component != null)
			{
				component.NotifyDropped();
			}
		}
		base.OnPreDrop(user);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}
}
