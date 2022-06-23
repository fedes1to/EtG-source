using System;
using System.Collections.Generic;
using UnityEngine;

public class MajorBreakable : PersistentVFXManagerBehaviour
{
	public float HitPoints = 100f;

	public float DamageReduction;

	public int MinHits;

	public int EnemyDamageOverride = -1;

	public bool ImmuneToBeastMode;

	public bool ScaleWithEnemyHealth;

	public bool OnlyExplosions;

	public bool IgnoreExplosions;

	[NonSerialized]
	public bool IsSecretDoor;

	public bool GameActorMotionBreaks;

	public bool PlayerRollingBreaks;

	public bool spawnShards = true;

	[ShowInInspectorIf("spawnShards", true)]
	public bool distributeShards;

	public ShardCluster[] shardClusters;

	[ShowInInspectorIf("spawnShards", true)]
	public float minShardPercentSpeed = 0.05f;

	[ShowInInspectorIf("spawnShards", true)]
	public float maxShardPercentSpeed = 0.3f;

	[ShowInInspectorIf("spawnShards", true)]
	public MinorBreakable.BreakStyle shardBreakStyle;

	public bool usesTemporaryZeroHitPointsState;

	[ShowInInspectorIf("usesTemporaryZeroHitPointsState", true)]
	public string spriteNameToUseAtZeroHP;

	[NonSerialized]
	public string overrideSpriteNameToUseAtZeroHP;

	public bool destroyedOnBreak;

	public List<GameObject> childrenToDestroy;

	public bool playsAnimationOnNotBroken;

	[ShowInInspectorIf("playsAnimationOnNotBroken", true)]
	public string notBreakAnimation;

	public bool handlesOwnBreakAnimation;

	[ShowInInspectorIf("handlesOwnBreakAnimation", true)]
	public string breakAnimation;

	public bool handlesOwnPrebreakFrames;

	public BreakFrame[] prebreakFrames;

	public VFXPool damageVfx;

	[ShowInInspectorIf("damageVfx", true)]
	public float damageVfxMinTimeBetween = 0.2f;

	public VFXPool breakVfx;

	[ShowInInspectorIf("breakVfx", true)]
	public GameObject breakVfxParent;

	[ShowInInspectorIf("breakVfx", true)]
	public bool delayDamageVfx;

	public bool SpawnItemOnBreak;

	[ShowInInspectorIf("SpawnItemOnBreak", false)]
	[PickupIdentifier]
	public int ItemIdToSpawnOnBreak = -1;

	public bool HandlePathBlocking;

	private OccupiedCells m_occupiedCells;

	public Action OnBreak;

	public Action<float> OnDamaged;

	[NonSerialized]
	public bool InvulnerableToEnemyBullets;

	[NonSerialized]
	public bool TemporarilyInvulnerable;

	private bool m_inZeroHPState;

	private bool m_isBroken;

	private int m_numHits;

	private float m_damageVfxTimer;

	public bool ReportZeroDamage { get; set; }

	public bool IsDestroyed
	{
		get
		{
			return m_isBroken;
		}
	}

	public int NumHits
	{
		get
		{
			return m_numHits;
		}
	}

	public float MinHitPointsFromNonExplosions { get; set; }

	public float MaxHitPoints { get; set; }

	public Vector2 CenterPoint
	{
		get
		{
			if ((bool)base.specRigidbody)
			{
				return base.specRigidbody.GetUnitCenter(ColliderType.HitBox);
			}
			if ((bool)base.sprite)
			{
				return base.sprite.WorldCenter;
			}
			return base.transform.position.XY();
		}
	}

	public void Awake()
	{
		StaticReferenceManager.AllMajorBreakables.Add(this);
	}

	public void Start()
	{
		if (HandlePathBlocking)
		{
			m_occupiedCells = new OccupiedCells(base.specRigidbody, GameManager.Instance.Dungeon.data.GetAbsoluteRoomFromPosition(base.transform.position.IntXY()));
		}
		if (MaxHitPoints <= 0f)
		{
			MaxHitPoints = HitPoints;
		}
		if (ScaleWithEnemyHealth)
		{
			float baseLevelHealthModifier = AIActor.BaseLevelHealthModifier;
			HitPoints *= baseLevelHealthModifier;
			MaxHitPoints *= baseLevelHealthModifier;
		}
		if (GameActorMotionBreaks || PlayerRollingBreaks)
		{
			SpeculativeRigidbody speculativeRigidbody = base.specRigidbody;
			speculativeRigidbody.OnPreRigidbodyCollision = (SpeculativeRigidbody.OnPreRigidbodyCollisionDelegate)Delegate.Combine(speculativeRigidbody.OnPreRigidbodyCollision, new SpeculativeRigidbody.OnPreRigidbodyCollisionDelegate(OnPreCollision));
		}
	}

	public void Update()
	{
		m_damageVfxTimer += BraveTime.DeltaTime;
	}

	public float GetCurrentHealthPercentage()
	{
		return HitPoints / MaxHitPoints;
	}

	protected override void OnDestroy()
	{
		StaticReferenceManager.AllMajorBreakables.Remove(this);
		base.OnDestroy();
	}

	private void OnPreCollision(SpeculativeRigidbody myRigidbody, PixelCollider myCollider, SpeculativeRigidbody otherRigidbody, PixelCollider otherCollider)
	{
		if (!base.enabled)
		{
			return;
		}
		if (m_isBroken)
		{
			PhysicsEngine.SkipCollision = true;
		}
		else if ((bool)otherRigidbody.gameActor)
		{
			if (GameActorMotionBreaks)
			{
				Break(otherRigidbody.Velocity);
				PhysicsEngine.SkipCollision = true;
			}
			else if (PlayerRollingBreaks && otherRigidbody.gameActor is PlayerController && (otherRigidbody.gameActor as PlayerController).IsDodgeRolling)
			{
				Break(otherRigidbody.Velocity);
				PhysicsEngine.SkipCollision = true;
			}
		}
	}

	public void ApplyDamage(float damage, Vector2 sourceDirection, bool isSourceEnemy, bool isExplosion = false, bool ForceDamageOverride = false)
	{
		if (IsDestroyed || TemporarilyInvulnerable || (!ForceDamageOverride && (OnlyExplosions || (IsSecretDoor && HitPoints <= 1f)) && !isExplosion) || !base.enabled)
		{
			return;
		}
		if (EnemyDamageOverride > 0 && isSourceEnemy)
		{
			damage = EnemyDamageOverride;
		}
		float num = Mathf.Max(0f, damage - DamageReduction);
		if (IsSecretDoor && !ForceDamageOverride && HitPoints - num < 1f)
		{
			num = Mathf.Min(HitPoints - 1f, num);
		}
		if (MinHitPointsFromNonExplosions > 0f && !isExplosion)
		{
			num = Mathf.Min(HitPoints - MinHitPointsFromNonExplosions, num);
		}
		if (ForceDamageOverride)
		{
			num = damage;
		}
		if (num <= 0f)
		{
			if (ReportZeroDamage && OnDamaged != null)
			{
				OnDamaged(num);
			}
			return;
		}
		HitPoints -= num;
		m_numHits++;
		if (OnDamaged != null)
		{
			OnDamaged(num);
		}
		if (m_damageVfxTimer > damageVfxMinTimeBetween)
		{
			if (damageVfx != null)
			{
				VFXPool vFXPool = damageVfx;
				Vector3 position = CenterPoint;
				Vector2? sourceVelocity = -sourceDirection;
				vFXPool.SpawnAtPosition(position, 0f, null, null, sourceVelocity);
			}
			m_damageVfxTimer = 0f;
		}
		if (HitPoints <= 0f && m_numHits >= MinHits)
		{
			if (usesTemporaryZeroHitPointsState && !m_inZeroHPState)
			{
				m_inZeroHPState = true;
				string value = (string.IsNullOrEmpty(overrideSpriteNameToUseAtZeroHP) ? spriteNameToUseAtZeroHP : overrideSpriteNameToUseAtZeroHP);
				if (!string.IsNullOrEmpty(value))
				{
					base.sprite.SetSprite(value);
				}
			}
			else
			{
				Break(sourceDirection);
			}
			return;
		}
		if (handlesOwnPrebreakFrames)
		{
			for (int num2 = prebreakFrames.Length - 1; num2 >= 0; num2--)
			{
				if (GetCurrentHealthPercentage() <= prebreakFrames[num2].healthPercentage / 100f)
				{
					base.sprite.SetSprite(prebreakFrames[num2].sprite);
					return;
				}
			}
		}
		if (playsAnimationOnNotBroken)
		{
			base.spriteAnimator.Play(notBreakAnimation);
		}
	}

	public void Break(Vector2 sourceDirection)
	{
		if (m_isBroken)
		{
			return;
		}
		m_isBroken = true;
		TriggerPersistentVFXClear();
		if (OnBreak != null)
		{
			OnBreak();
		}
		if (spawnShards)
		{
			switch (shardBreakStyle)
			{
			case MinorBreakable.BreakStyle.BURST:
				SpawnShards(sourceDirection, -180f, 180f, 0.5f, sourceDirection.magnitude * minShardPercentSpeed, sourceDirection.magnitude * maxShardPercentSpeed);
				break;
			case MinorBreakable.BreakStyle.CONE:
				SpawnShards(sourceDirection, -45f, 45f, 0.5f, sourceDirection.magnitude * minShardPercentSpeed, sourceDirection.magnitude * maxShardPercentSpeed);
				break;
			case MinorBreakable.BreakStyle.JET:
				SpawnShards(sourceDirection, -15f, 15f, 0.5f, sourceDirection.magnitude * minShardPercentSpeed, sourceDirection.magnitude * maxShardPercentSpeed);
				break;
			case MinorBreakable.BreakStyle.WALL_DOWNWARD_BURST:
				SpawnShards(Vector2.down, -45f, 45f, 0.5f, sourceDirection.magnitude * minShardPercentSpeed, sourceDirection.magnitude * maxShardPercentSpeed);
				break;
			}
		}
		if (childrenToDestroy != null)
		{
			for (int i = 0; i < childrenToDestroy.Count; i++)
			{
				UnityEngine.Object.Destroy(childrenToDestroy[i]);
			}
		}
		if (breakVfx != null && !delayDamageVfx)
		{
			if ((bool)breakVfxParent)
			{
				breakVfx.SpawnAtLocalPosition(Vector3.zero, 0f, breakVfxParent.transform);
			}
			else
			{
				breakVfx.SpawnAtPosition(CenterPoint);
			}
		}
		if (HandlePathBlocking)
		{
			m_occupiedCells.Clear();
		}
		if (SpawnItemOnBreak)
		{
			PickupObject byId = PickupObjectDatabase.GetById(ItemIdToSpawnOnBreak);
			LootEngine.SpawnItem(byId.gameObject, base.sprite.WorldCenter, Vector2.zero, 1f, true, true);
		}
		if (destroyedOnBreak)
		{
			if (handlesOwnBreakAnimation)
			{
				if (breakVfx != null && breakVfx.type != 0)
				{
					base.spriteAnimator.PlayAndDestroyObject(breakAnimation, delegate
					{
						if ((bool)breakVfxParent)
						{
							breakVfx.SpawnAtLocalPosition(Vector3.zero, 0f, breakVfxParent.transform);
						}
						else
						{
							breakVfx.SpawnAtPosition(CenterPoint);
						}
					});
				}
				else
				{
					base.spriteAnimator.PlayAndDestroyObject(breakAnimation);
				}
			}
			else
			{
				UnityEngine.Object.Destroy(base.gameObject);
			}
		}
		else if (handlesOwnBreakAnimation)
		{
			base.spriteAnimator.Play(breakAnimation);
			base.specRigidbody.enabled = false;
		}
	}

	public void SpawnShards(Vector2 direction, float minAngle, float maxAngle, float verticalSpeed, float minMagnitude, float maxMagnitude)
	{
		if (GameManager.Options.DebrisQuantity == GameOptions.GenericHighMedLowOption.VERY_LOW)
		{
			return;
		}
		Vector3 vector = base.sprite.GetBounds().extents + base.transform.position;
		if (shardClusters == null || shardClusters.Length <= 0)
		{
			return;
		}
		int num = UnityEngine.Random.Range(0, 10);
		Bounds bounds = base.sprite.GetBounds();
		for (int i = 0; i < shardClusters.Length; i++)
		{
			float lowDiscrepancyRandom = BraveMathCollege.GetLowDiscrepancyRandom(num);
			num++;
			float z = Mathf.Lerp(minAngle, maxAngle, lowDiscrepancyRandom);
			ShardCluster shardCluster = shardClusters[i];
			int num2 = UnityEngine.Random.Range(shardCluster.minFromCluster, shardCluster.maxFromCluster + 1);
			int num3 = UnityEngine.Random.Range(0, shardCluster.clusterObjects.Length);
			for (int j = 0; j < num2; j++)
			{
				Vector3 vector2 = vector;
				if (distributeShards)
				{
					vector2 = base.sprite.transform.position + new Vector3(UnityEngine.Random.Range(bounds.min.x, bounds.max.x), UnityEngine.Random.Range(bounds.min.y, bounds.max.y), UnityEngine.Random.Range(bounds.min.z, bounds.max.z));
				}
				Vector3 startingForce = Quaternion.Euler(0f, 0f, z) * (direction.normalized * UnityEngine.Random.Range(minMagnitude, maxMagnitude)).ToVector3ZUp(verticalSpeed);
				if (shardBreakStyle == MinorBreakable.BreakStyle.BURST)
				{
					startingForce = ((vector2 - vector).normalized * UnityEngine.Random.Range(minMagnitude, maxMagnitude)).WithZ(verticalSpeed);
				}
				int num4 = (num3 + j) % shardCluster.clusterObjects.Length;
				GameObject gameObject = SpawnManager.SpawnDebris(shardCluster.clusterObjects[num4].gameObject, vector2, Quaternion.identity);
				tk2dSprite component = gameObject.GetComponent<tk2dSprite>();
				if (base.sprite.attachParent != null && component != null)
				{
					component.attachParent = base.sprite.attachParent;
					component.HeightOffGround = base.sprite.HeightOffGround;
				}
				DebrisObject component2 = gameObject.GetComponent<DebrisObject>();
				component2.Trigger(startingForce, 1f);
			}
		}
	}
}
