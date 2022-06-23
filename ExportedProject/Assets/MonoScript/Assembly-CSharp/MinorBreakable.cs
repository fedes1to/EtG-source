using System;
using System.Collections;
using System.Collections.Generic;
using Dungeonator;
using UnityEngine;

public class MinorBreakable : BraveBehaviour, IPlaceConfigurable
{
	public enum BreakStyle
	{
		CONE = 0,
		BURST = 1,
		JET = 2,
		WALL_DOWNWARD_BURST = 3,
		CUSTOM = 100
	}

	public BreakStyle breakStyle;

	[BetterList]
	public ShardCluster[] shardClusters;

	public GameObject stainObject;

	public Vector2 ShardSpawnOffset;

	public bool destroyOnBreak = true;

	public float ForcedDestroyDelay;

	public bool makeParallelOnBreak = true;

	public bool resistsExplosions;

	public bool stopsBullets;

	public bool canSpawnFairy;

	[Header("DropLoots")]
	public bool dropCoins;

	public float amountToRain;

	public float chanceToRain;

	[Header("Explosive?")]
	public bool explodesOnBreak;

	public ExplosionData explosionData;

	[Header("Goops?")]
	public bool goopsOnBreak;

	[ShowInInspectorIf("goopsOnBreak", false)]
	public GoopDefinition goopType;

	[ShowInInspectorIf("goopsOnBreak", false)]
	public float goopRadius = 3f;

	[Header("Particulates")]
	public bool hasParticulates;

	[ShowInInspectorIf("hasParticulates", false)]
	public int MinParticlesOnBurst;

	[ShowInInspectorIf("hasParticulates", false)]
	public int MaxParticlesOnBurst;

	[ShowInInspectorIf("hasParticulates", false)]
	public float ParticleSize = 0.0625f;

	[ShowInInspectorIf("hasParticulates", false)]
	public float ParticleLifespan = 0.25f;

	[ShowInInspectorIf("hasParticulates", false)]
	public float ParticleMagnitude = 1f;

	[ShowInInspectorIf("hasParticulates", false)]
	public float ParticleMagnitudeVariance = 0.5f;

	[ShowInInspectorIf("hasParticulates", false)]
	public Color ParticleColor;

	[ShowInInspectorIf("hasParticulates", false)]
	public GlobalSparksDoer.EmitRegionStyle EmitStyle = GlobalSparksDoer.EmitRegionStyle.RADIAL;

	[ShowInInspectorIf("hasParticulates", false)]
	public GlobalSparksDoer.SparksType ParticleType;

	[Header("Animation and Audio")]
	[CheckAnimation(null)]
	public string breakAnimName;

	public string breakAnimFrame;

	public string breakAudioEventName;

	public Action OnBreak;

	public Action<MinorBreakable> OnBreakContext;

	public float AdditionalSpawnedObjectHeight;

	public Vector2 SpawnedObjectOffsetVector = Vector2.zero;

	[NonSerialized]
	public float heightOffGround = 0.1f;

	[NonSerialized]
	public bool OnlyBreaksOnScreen;

	public GameObject AdditionalVFXObject;

	public bool OnlyBrokenByCode;

	public bool isInvulnerableToGameActors;

	[Header("Unusual Settings")]
	public bool CastleReplacedWithWaterDrum;

	[HideInInspector]
	public bool isImpermeableToGameActors;

	[HideInInspector]
	public bool onlyVulnerableToGunfire;

	public bool OnlyPlayerProjectilesCanBreak;

	[HideInInspector]
	public SurfaceDecorator parentSurface;

	public List<DebrisObject> AdditionalDestabilizedObjects;

	public bool ForceSmallForCollisions;

	public bool IgnoredForPotShotsModifier;

	private bool? m_cachedIsBig;

	private bool m_isBroken;

	private Transform m_transform;

	private tk2dSprite m_sprite;

	private tk2dSpriteAnimator m_spriteAnimator;

	private MinorBreakableGroupManager m_groupManager;

	public bool IsDecorativeOnly;

	private bool m_doneAdditionalDestabilize;

	private OccupiedCells m_occupiedCells;

	public bool IsBroken
	{
		get
		{
			return m_isBroken;
		}
	}

	public bool IsBig
	{
		get
		{
			if (ForceSmallForCollisions)
			{
				m_cachedIsBig = false;
			}
			else if (!m_cachedIsBig.HasValue && (bool)base.specRigidbody && base.specRigidbody.PrimaryPixelCollider != null)
			{
				PixelCollider pixelCollider = base.specRigidbody.HitboxPixelCollider ?? base.specRigidbody.PrimaryPixelCollider;
				m_cachedIsBig = pixelCollider.Dimensions.x > 8 || pixelCollider.Dimensions.y > 8;
			}
			return m_cachedIsBig.Value;
		}
	}

	public MinorBreakableGroupManager GroupManager
	{
		get
		{
			return m_groupManager;
		}
		set
		{
			m_groupManager = value;
		}
	}

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

	private void Awake()
	{
		StaticReferenceManager.AllMinorBreakables.Add(this);
	}

	private IEnumerator Start()
	{
		m_transform = base.transform;
		m_sprite = GetComponent<tk2dSprite>();
		m_spriteAnimator = GetComponent<tk2dSpriteAnimator>();
		if (base.specRigidbody != null)
		{
			SpeculativeRigidbody speculativeRigidbody = base.specRigidbody;
			speculativeRigidbody.OnPreRigidbodyCollision = (SpeculativeRigidbody.OnPreRigidbodyCollisionDelegate)Delegate.Combine(speculativeRigidbody.OnPreRigidbodyCollision, new SpeculativeRigidbody.OnPreRigidbodyCollisionDelegate(OnPreCollision));
		}
		m_isBroken = false;
		yield return null;
		if (!IsDecorativeOnly || !m_spriteAnimator || (base.spriteAnimator.CurrentClip != null && base.spriteAnimator.IsPlaying(base.spriteAnimator.CurrentClip)))
		{
			yield break;
		}
		tk2dSpriteAnimator obj = m_spriteAnimator;
		obj.OnPlayAnimationCalled = (Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip>)Delegate.Combine(obj.OnPlayAnimationCalled, (Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip>)delegate(tk2dSpriteAnimator anim, tk2dSpriteAnimationClip clip)
		{
			if (!anim.enabled)
			{
				anim.enabled = true;
			}
		});
		m_spriteAnimator.enabled = false;
	}

	public void CleanupCallbacks()
	{
		if ((bool)base.specRigidbody)
		{
			SpeculativeRigidbody speculativeRigidbody = base.specRigidbody;
			speculativeRigidbody.OnPreRigidbodyCollision = (SpeculativeRigidbody.OnPreRigidbodyCollisionDelegate)Delegate.Remove(speculativeRigidbody.OnPreRigidbodyCollision, new SpeculativeRigidbody.OnPreRigidbodyCollisionDelegate(OnPreCollision));
		}
	}

	private void OnPreCollision(SpeculativeRigidbody myRigidbody, PixelCollider myCollider, SpeculativeRigidbody otherRigidbody, PixelCollider otherCollider)
	{
		if (myCollider.IsTrigger || OnlyBrokenByCode || !base.enabled)
		{
			return;
		}
		if (m_isBroken)
		{
			PhysicsEngine.SkipCollision = true;
		}
		else
		{
			if (OnlyBreaksOnScreen && !base.renderer.isVisible)
			{
				return;
			}
			Projectile component = otherRigidbody.GetComponent<Projectile>();
			if (onlyVulnerableToGunfire && component == null)
			{
				return;
			}
			if (OnlyPlayerProjectilesCanBreak && (bool)component && !(component.Owner is PlayerController))
			{
				PhysicsEngine.SkipCollision = true;
			}
			else
			{
				if (isInvulnerableToGameActors && otherRigidbody.gameActor != null)
				{
					return;
				}
				if (isImpermeableToGameActors && otherRigidbody.gameActor != null)
				{
					PhysicsEngine.SkipCollision = true;
				}
				else if (otherRigidbody.gameActor is PlayerController && (otherRigidbody.gameActor as PlayerController).IsEthereal)
				{
					PhysicsEngine.SkipCollision = true;
				}
				else if (!(otherRigidbody.minorBreakable != null))
				{
					Vector2 normalized = otherRigidbody.Velocity.normalized;
					float magnitude = otherRigidbody.Velocity.magnitude;
					magnitude = Mathf.Min(magnitude, 5f);
					Break(normalized * magnitude);
					if (!stopsBullets)
					{
						PhysicsEngine.SkipCollision = true;
					}
					if (otherRigidbody.gameActor != null)
					{
						PhysicsEngine.SkipCollision = true;
					}
				}
			}
		}
	}

	private void OnBreakAnimationComplete()
	{
		if (explodesOnBreak)
		{
			Exploder.Explode(base.specRigidbody.UnitCenter, explosionData, Vector2.zero, FinishBreak);
		}
		else
		{
			FinishBreak();
		}
	}

	private void FinishBreak()
	{
		if (goopsOnBreak)
		{
			DeadlyDeadlyGoopManager goopManagerForGoopType = DeadlyDeadlyGoopManager.GetGoopManagerForGoopType(goopType);
			goopManagerForGoopType.TimedAddGoopCircle(base.specRigidbody.UnitCenter, goopRadius);
		}
		if (destroyOnBreak)
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
		else if (makeParallelOnBreak)
		{
			m_sprite.IsPerpendicular = false;
		}
	}

	public void SpawnShards(Vector2 direction, float minAngle, float maxAngle, float verticalSpeed, float minMagnitude, float maxMagnitude)
	{
		if (GameManager.Options.DebrisQuantity == GameOptions.GenericHighMedLowOption.VERY_LOW)
		{
			return;
		}
		if (m_sprite == null || m_transform == null)
		{
			m_transform = base.transform;
			m_sprite = GetComponent<tk2dSprite>();
		}
		if (m_sprite == null || m_transform == null)
		{
			Debug.LogError("trying to spawn shards on a object with no transform or sprite");
			return;
		}
		Vector3 position = m_sprite.WorldCenter.ToVector3ZUp(m_sprite.WorldCenter.y) + ShardSpawnOffset.ToVector3ZUp();
		if (shardClusters != null && shardClusters.Length > 0)
		{
			int num = UnityEngine.Random.Range(0, 10);
			for (int i = 0; i < shardClusters.Length; i++)
			{
				ShardCluster shardCluster = shardClusters[i];
				int num2 = UnityEngine.Random.Range(shardCluster.minFromCluster, shardCluster.maxFromCluster + 1);
				int num3 = UnityEngine.Random.Range(0, shardCluster.clusterObjects.Length);
				for (int j = 0; j < num2; j++)
				{
					float lowDiscrepancyRandom = BraveMathCollege.GetLowDiscrepancyRandom(num);
					num++;
					float z = Mathf.Lerp(minAngle, maxAngle, lowDiscrepancyRandom);
					Vector3 a = Quaternion.Euler(0f, 0f, z) * (direction.normalized * UnityEngine.Random.Range(minMagnitude, maxMagnitude)).ToVector3ZUp(verticalSpeed);
					int num4 = (num3 + j) % shardCluster.clusterObjects.Length;
					GameObject gameObject = SpawnManager.SpawnDebris(shardCluster.clusterObjects[num4].gameObject, position, Quaternion.identity);
					tk2dSprite component = gameObject.GetComponent<tk2dSprite>();
					if (m_sprite.attachParent != null && component != null)
					{
						component.attachParent = m_sprite.attachParent;
						component.HeightOffGround = m_sprite.HeightOffGround;
					}
					DebrisObject component2 = gameObject.GetComponent<DebrisObject>();
					a = Vector3.Scale(a, shardCluster.forceAxialMultiplier) * shardCluster.forceMultiplier;
					component2.Trigger(a, heightOffGround + AdditionalSpawnedObjectHeight, shardCluster.rotationMultiplier);
				}
			}
		}
		if (AdditionalVFXObject != null)
		{
			SpawnManager.SpawnVFX(AdditionalVFXObject, position, Quaternion.identity);
		}
	}

	private void SpawnStain()
	{
		if (!(stainObject != null))
		{
			return;
		}
		GameObject gameObject = SpawnManager.SpawnDecal(stainObject);
		tk2dSprite component = gameObject.GetComponent<tk2dSprite>();
		component.PlaceAtPositionByAnchor(base.sprite.WorldCenter.ToVector3ZUp(), tk2dBaseSprite.Anchor.MiddleCenter);
		component.HeightOffGround = 0.1f;
		if (parentSurface != null && !parentSurface.IsDestabilized)
		{
			component.HeightOffGround = 0.1f;
			if (parentSurface.sprite != null)
			{
				parentSurface.sprite.AttachRenderer(component);
				parentSurface.sprite.UpdateZDepth();
			}
			MajorBreakable component2 = parentSurface.GetComponent<MajorBreakable>();
			if (component2 != null)
			{
				component2.AttachDestructibleVFX(gameObject);
			}
		}
		else
		{
			component.HeightOffGround = -1f;
			component.UpdateZDepth();
		}
	}

	private void HandleSynergies()
	{
		if (IgnoredForPotShotsModifier || OnlyBrokenByCode)
		{
			return;
		}
		int count = 0;
		if (PlayerController.AnyoneHasActiveBonusSynergy(CustomSynergyType.MINOR_BLANKABLES, out count))
		{
			float value = UnityEngine.Random.value;
			float num = 0.01f;
			if (value < num * (float)count)
			{
				Vector2 value2 = ((!base.sprite) ? base.transform.position.XY() : base.sprite.WorldCenter);
				PlayerController bestActivePlayer = GameManager.Instance.BestActivePlayer;
				Vector2? overrideCenter = value2;
				bestActivePlayer.ForceBlank(25f, 0.5f, false, true, overrideCenter, false);
			}
		}
	}

	private void HandleShardSpawns(Vector2 sourceVelocity)
	{
		BreakStyle breakStyle = this.breakStyle;
		if (sourceVelocity == Vector2.zero)
		{
			breakStyle = BreakStyle.BURST;
		}
		float verticalSpeed = 1.5f;
		SpawnLoot();
		switch (breakStyle)
		{
		case BreakStyle.BURST:
			SpawnShards(Vector2.right, -180f, 180f, verticalSpeed, 1f, 2f);
			break;
		case BreakStyle.CONE:
			SpawnShards(sourceVelocity, -45f, 45f, verticalSpeed, sourceVelocity.magnitude * 0.5f, sourceVelocity.magnitude * 1.5f);
			break;
		case BreakStyle.JET:
			SpawnShards(sourceVelocity, -15f, 15f, verticalSpeed, sourceVelocity.magnitude * 0.5f, sourceVelocity.magnitude * 1.5f);
			break;
		case BreakStyle.WALL_DOWNWARD_BURST:
			SpawnShards(Vector2.down, -30f, 30f, 0f, 0.25f, 0.75f);
			break;
		}
		SpawnStain();
	}

	public void SpawnLoot()
	{
		if (dropCoins && UnityEngine.Random.value < chanceToRain)
		{
			Vector3 up = Vector3.up;
			up *= 2f;
			GameObject gameObject = SpawnManager.SpawnDebris(GameManager.Instance.Dungeon.sharedSettingsPrefab.currencyDropSettings.bronzeCoinPrefab, base.specRigidbody.UnitCenter.ToVector3ZUp(base.transform.position.z), Quaternion.identity);
			DebrisObject orAddComponent = gameObject.GetOrAddComponent<DebrisObject>();
			orAddComponent.shouldUseSRBMotion = true;
			orAddComponent.angularVelocity = 0f;
			orAddComponent.Priority = EphemeralObject.EphemeralPriority.Critical;
			orAddComponent.Trigger(up.WithZ(4f), 0.05f);
			orAddComponent.canRotate = false;
		}
	}

	private void HandleParticulates(Vector2 vel)
	{
		if (hasParticulates)
		{
			Vector3 minPosition = base.sprite.WorldBottomLeft;
			Vector3 maxPosition = base.sprite.WorldTopRight;
			switch (EmitStyle)
			{
			case GlobalSparksDoer.EmitRegionStyle.RADIAL:
				GlobalSparksDoer.DoRadialParticleBurst(UnityEngine.Random.Range(MinParticlesOnBurst, MaxParticlesOnBurst), minPosition, maxPosition, 30f, ParticleMagnitude, ParticleMagnitudeVariance, ParticleSize, ParticleLifespan, ParticleColor, ParticleType);
				break;
			case GlobalSparksDoer.EmitRegionStyle.RANDOM:
				GlobalSparksDoer.DoRandomParticleBurst(UnityEngine.Random.Range(MinParticlesOnBurst, MaxParticlesOnBurst), minPosition, maxPosition, vel.normalized * ParticleMagnitude, 45f, ParticleMagnitudeVariance, ParticleSize, ParticleLifespan, ParticleColor, ParticleType);
				break;
			}
		}
	}

	public void Break()
	{
		if (!this || !base.enabled || m_isBroken)
		{
			return;
		}
		m_isBroken = true;
		if (m_groupManager != null)
		{
			m_groupManager.InformBroken(this, Vector2.zero, heightOffGround);
		}
		if (GameManager.Instance.InTutorial && !base.name.Contains("table", true) && !base.name.Contains("red", true))
		{
			GameManager.BroadcastRoomTalkDoerFsmEvent("playerBrokeShit");
		}
		if (m_occupiedCells != null)
		{
			m_occupiedCells.Clear();
		}
		IPlayerInteractable @interface = base.gameObject.GetInterface<IPlayerInteractable>();
		if (@interface != null)
		{
			RoomHandler roomFromPosition = GameManager.Instance.Dungeon.GetRoomFromPosition(base.transform.position.IntXY());
			if (roomFromPosition.IsRegistered(@interface))
			{
				roomFromPosition.DeregisterInteractable(@interface);
			}
		}
		if (base.specRigidbody != null)
		{
			base.specRigidbody.enabled = false;
		}
		bool flag = false;
		if (m_spriteAnimator != null && breakAnimName != string.Empty)
		{
			tk2dSpriteAnimationClip clipByName = m_spriteAnimator.GetClipByName(breakAnimName);
			if (clipByName != null)
			{
				m_spriteAnimator.Play(clipByName);
				flag = true;
				Invoke("OnBreakAnimationComplete", clipByName.BaseClipLength);
			}
		}
		else if (!string.IsNullOrEmpty(breakAnimFrame))
		{
			m_sprite.SetSprite(breakAnimFrame);
		}
		if (!m_transform)
		{
			m_transform = base.transform;
		}
		if ((bool)m_transform)
		{
			AkSoundEngine.SetObjectPosition(base.gameObject, m_transform.position.x, m_transform.position.y, m_transform.position.z, m_transform.forward.x, m_transform.forward.y, m_transform.forward.z, m_transform.up.x, m_transform.up.y, m_transform.up.z);
		}
		if (!string.IsNullOrEmpty(breakAudioEventName))
		{
			AkSoundEngine.PostEvent(breakAudioEventName, base.gameObject);
		}
		HandleShardSpawns(Vector2.zero);
		HandleParticulates(Vector2.zero);
		HandleSynergies();
		SurfaceDecorator component = GetComponent<SurfaceDecorator>();
		if (component != null)
		{
			component.Destabilize(Vector2.zero);
		}
		DestabilizeAttachedObjects(Vector2.zero);
		if (OnBreak != null)
		{
			OnBreak();
		}
		if (OnBreakContext != null)
		{
			OnBreakContext(this);
		}
		if (destroyOnBreak && !flag)
		{
			if (ForcedDestroyDelay > 0f)
			{
				UnityEngine.Object.Destroy(base.gameObject, ForcedDestroyDelay);
			}
			else
			{
				UnityEngine.Object.Destroy(base.gameObject);
			}
		}
	}

	private void DestabilizeAttachedObjects(Vector2 vec)
	{
		if (m_doneAdditionalDestabilize)
		{
			return;
		}
		m_doneAdditionalDestabilize = true;
		for (int i = 0; i < AdditionalDestabilizedObjects.Count; i++)
		{
			if ((bool)AdditionalDestabilizedObjects[i])
			{
				Vector3 startingForce = UnityEngine.Random.insideUnitCircle.ToVector3ZUp(0.5f);
				startingForce *= UnityEngine.Random.Range(2.5f, 4f);
				if (GameManager.Instance.Dungeon.tileIndices.tilesetId == GlobalDungeonData.ValidTilesets.FINALGEON)
				{
					startingForce.y = Mathf.Abs(startingForce.y) * -1f;
				}
				AdditionalDestabilizedObjects[i].transform.parent = SpawnManager.Instance.Debris;
				AdditionalDestabilizedObjects[i].Trigger(startingForce, 0.5f);
			}
		}
	}

	public void Break(Vector2 direction)
	{
		if (!base.enabled || m_isBroken)
		{
			return;
		}
		m_isBroken = true;
		if (m_groupManager != null)
		{
			m_groupManager.InformBroken(this, direction, heightOffGround);
		}
		bool flag = GameManager.Instance.InTutorial;
		if (GameManager.Instance.PrimaryPlayer.CurrentRoom != null)
		{
			flag = flag || GameManager.Instance.PrimaryPlayer.CurrentRoom.area.PrototypeRoomCategory == PrototypeDungeonRoom.RoomCategory.SPECIAL;
		}
		if (flag && !base.name.Contains("table", true) && !explodesOnBreak)
		{
			GameManager.BroadcastRoomTalkDoerFsmEvent("playerBrokeShit");
		}
		if (m_occupiedCells != null)
		{
			m_occupiedCells.Clear();
		}
		IPlayerInteractable @interface = base.gameObject.GetInterface<IPlayerInteractable>();
		if (@interface != null)
		{
			RoomHandler roomFromPosition = GameManager.Instance.Dungeon.GetRoomFromPosition(base.transform.position.IntXY());
			if (roomFromPosition == null)
			{
				roomFromPosition = GameManager.Instance.Dungeon.GetRoomFromPosition(base.transform.position.IntXY() + IntVector2.Right);
			}
			if (roomFromPosition != null && roomFromPosition.IsRegistered(@interface))
			{
				roomFromPosition.DeregisterInteractable(@interface);
			}
		}
		if (base.specRigidbody != null)
		{
			base.specRigidbody.enabled = false;
		}
		bool flag2 = false;
		if (m_spriteAnimator != null && breakAnimName != string.Empty)
		{
			tk2dSpriteAnimationClip clipByName = m_spriteAnimator.GetClipByName(breakAnimName);
			if (clipByName != null)
			{
				m_spriteAnimator.Play(clipByName);
				flag2 = true;
				Invoke("OnBreakAnimationComplete", clipByName.BaseClipLength);
			}
		}
		else if (!string.IsNullOrEmpty(breakAnimFrame))
		{
			m_sprite.SetSprite(breakAnimFrame);
		}
		if (!m_transform)
		{
			m_transform = base.transform;
		}
		if ((bool)m_transform)
		{
			AkSoundEngine.SetObjectPosition(base.gameObject, m_transform.position.x, m_transform.position.y, m_transform.position.z, m_transform.forward.x, m_transform.forward.y, m_transform.forward.z, m_transform.up.x, m_transform.up.y, m_transform.up.z);
		}
		if (!string.IsNullOrEmpty(breakAudioEventName))
		{
			AkSoundEngine.PostEvent(breakAudioEventName, base.gameObject);
		}
		HandleShardSpawns(direction);
		HandleParticulates(direction);
		HandleSynergies();
		SurfaceDecorator component = GetComponent<SurfaceDecorator>();
		if (component != null)
		{
			component.Destabilize(direction.normalized);
		}
		DestabilizeAttachedObjects(direction.normalized);
		if (canSpawnFairy && GameManager.Instance.Dungeon.sharedSettingsPrefab.RandomShouldSpawnPotFairy())
		{
			IntVector2 intVector = base.specRigidbody.UnitCenter.ToIntVector2(VectorConversions.Floor);
			RoomHandler roomFromPosition2 = GameManager.Instance.Dungeon.GetRoomFromPosition(intVector);
			PotFairyEngageDoer.InstantSpawn = true;
			AIActor orLoadByGuid = EnemyDatabase.GetOrLoadByGuid(GameManager.Instance.Dungeon.sharedSettingsPrefab.PotFairyGuid);
			AIActor.Spawn(orLoadByGuid, intVector, roomFromPosition2, true);
		}
		if (OnBreak != null)
		{
			OnBreak();
		}
		if (OnBreakContext != null)
		{
			OnBreakContext(this);
		}
		if (destroyOnBreak && !flag2)
		{
			if (ForcedDestroyDelay > 0f)
			{
				UnityEngine.Object.Destroy(base.gameObject, ForcedDestroyDelay);
			}
			else
			{
				UnityEngine.Object.Destroy(base.gameObject);
			}
		}
	}

	protected override void OnDestroy()
	{
		StaticReferenceManager.AllMinorBreakables.Remove(this);
		base.OnDestroy();
	}

	public void ConfigureOnPlacement(RoomHandler room)
	{
		if (isInvulnerableToGameActors && base.specRigidbody != null)
		{
			base.specRigidbody.Initialize();
			m_occupiedCells = new OccupiedCells(base.specRigidbody, room);
		}
	}
}
