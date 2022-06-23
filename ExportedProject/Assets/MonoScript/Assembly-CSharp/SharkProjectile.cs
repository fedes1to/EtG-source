using System;
using System.Collections;
using System.Collections.Generic;
using Dungeonator;
using Pathfinding;
using UnityEngine;

public class SharkProjectile : Projectile
{
	public VFXPool enemyNotKilledPool;

	public DirectionalAnimation animData;

	public GoopDefinition waterGoop;

	public float goopRadius = 1f;

	public float angularAcceleration = 10f;

	public ParticleSystem ParticlesPrefab;

	[NonSerialized]
	protected GameActor CurrentTarget;

	[NonSerialized]
	protected bool m_coroutineIsActive;

	protected bool CanCrossPits;

	private AIActor m_cachedTargetToEat;

	protected Vector2? m_lastGoopPosition;

	protected Path m_currentPath;

	protected float m_pathTimer;

	protected Vector2? m_overridePathEnd;

	private static Vector3 m_lastAssignedScale = Vector3.one;

	private float m_noTargetElapsed;

	public override void Start()
	{
		base.Start();
		StartCoroutine(AddLowObstacleCollider());
		SpeculativeRigidbody speculativeRigidbody = base.specRigidbody;
		speculativeRigidbody.OnPreRigidbodyCollision = (SpeculativeRigidbody.OnPreRigidbodyCollisionDelegate)Delegate.Combine(speculativeRigidbody.OnPreRigidbodyCollision, new SpeculativeRigidbody.OnPreRigidbodyCollisionDelegate(PassThroughTables));
		RoomHandler absoluteRoomFromPosition = GameManager.Instance.Dungeon.data.GetAbsoluteRoomFromPosition(base.transform.position.XY().ToIntVector2());
		if (absoluteRoomFromPosition != null && absoluteRoomFromPosition.area.PrototypeRoomCategory == PrototypeDungeonRoom.RoomCategory.BOSS)
		{
			CanCrossPits = true;
		}
		if (!CanCrossPits)
		{
			SpeculativeRigidbody speculativeRigidbody2 = base.specRigidbody;
			speculativeRigidbody2.MovementRestrictor = (SpeculativeRigidbody.MovementRestrictorDelegate)Delegate.Combine(speculativeRigidbody2.MovementRestrictor, new SpeculativeRigidbody.MovementRestrictorDelegate(NoPits));
		}
		OnHitEnemy = (Action<Projectile, SpeculativeRigidbody, bool>)Delegate.Combine(OnHitEnemy, new Action<Projectile, SpeculativeRigidbody, bool>(HandleHitEnemy));
		OnWillKillEnemy = (Action<Projectile, SpeculativeRigidbody>)Delegate.Combine(OnWillKillEnemy, new Action<Projectile, SpeculativeRigidbody>(MaybeEatEnemy));
	}

	private void PassThroughTables(SpeculativeRigidbody myRigidbody, PixelCollider myPixelCollider, SpeculativeRigidbody otherRigidbody, PixelCollider otherPixelCollider)
	{
		if ((bool)otherRigidbody.GetComponent<FlippableCover>())
		{
			PhysicsEngine.SkipCollision = true;
		}
	}

	private IEnumerator AddLowObstacleCollider()
	{
		while (true)
		{
			IntVector2 cellPos = base.transform.position.IntXY();
			IntVector2 aboveCellPos = cellPos + IntVector2.Up;
			bool shouldAddLayer = true;
			if (GameManager.Instance.Dungeon.data.CheckInBounds(cellPos) && GameManager.Instance.Dungeon.data[cellPos] != null && GameManager.Instance.Dungeon.data[cellPos].IsLowerFaceWall())
			{
				shouldAddLayer = false;
			}
			else if (GameManager.Instance.Dungeon.data.CheckInBounds(aboveCellPos) && GameManager.Instance.Dungeon.data[aboveCellPos] != null && GameManager.Instance.Dungeon.data[aboveCellPos].IsLowerFaceWall())
			{
				shouldAddLayer = false;
			}
			if (shouldAddLayer)
			{
				break;
			}
			yield return null;
		}
		base.specRigidbody.AddCollisionLayerIgnoreOverride(CollisionMask.LayerToMask(CollisionLayer.LowObstacle));
	}

	private void HandleHitEnemy(Projectile arg1, SpeculativeRigidbody arg2, bool arg3)
	{
		if ((bool)base.Owner && base.Owner is PlayerController && (base.Owner as PlayerController).HasActiveBonusSynergy(CustomSynergyType.EXPLOSIVE_SHARKS))
		{
			Exploder.DoDefaultExplosion(base.specRigidbody.UnitCenter.ToVector3ZisY(), Vector2.zero);
		}
		StartCoroutine(FrameDelayedDestruction());
	}

	private IEnumerator FrameDelayedDestruction()
	{
		yield return null;
		DieInAir();
	}

	private void NoPits(SpeculativeRigidbody specRigidbody, IntVector2 prevPixelOffset, IntVector2 pixelOffset, ref bool validLocation)
	{
		if (!validLocation)
		{
			return;
		}
		Func<IntVector2, bool> func = delegate(IntVector2 pixel)
		{
			Vector2 vector = PhysicsEngine.PixelToUnitMidpoint(pixel);
			if (!GameManager.Instance.Dungeon.CellSupportsFalling(vector))
			{
				return false;
			}
			List<SpeculativeRigidbody> platformsAt = GameManager.Instance.Dungeon.GetPlatformsAt(vector);
			if (platformsAt != null)
			{
				for (int i = 0; i < platformsAt.Count; i++)
				{
					if (platformsAt[i].PrimaryPixelCollider.ContainsPixel(pixel))
					{
						return false;
					}
				}
			}
			return true;
		};
		PixelCollider primaryPixelCollider = specRigidbody.PrimaryPixelCollider;
		if (primaryPixelCollider != null)
		{
			IntVector2 intVector = pixelOffset - prevPixelOffset;
			if (intVector == IntVector2.Down && func(primaryPixelCollider.LowerLeft + pixelOffset) && func(primaryPixelCollider.LowerRight + pixelOffset) && (!func(primaryPixelCollider.UpperRight + prevPixelOffset) || !func(primaryPixelCollider.UpperLeft + prevPixelOffset)))
			{
				validLocation = false;
			}
			else if (intVector == IntVector2.Right && func(primaryPixelCollider.LowerRight + pixelOffset) && func(primaryPixelCollider.UpperRight + pixelOffset) && (!func(primaryPixelCollider.UpperLeft + prevPixelOffset) || !func(primaryPixelCollider.LowerLeft + prevPixelOffset)))
			{
				validLocation = false;
			}
			else if (intVector == IntVector2.Up && func(primaryPixelCollider.UpperRight + pixelOffset) && func(primaryPixelCollider.UpperLeft + pixelOffset) && (!func(primaryPixelCollider.LowerLeft + prevPixelOffset) || !func(primaryPixelCollider.LowerRight + prevPixelOffset)))
			{
				validLocation = false;
			}
			else if (intVector == IntVector2.Left && func(primaryPixelCollider.UpperLeft + pixelOffset) && func(primaryPixelCollider.LowerLeft + pixelOffset) && (!func(primaryPixelCollider.LowerRight + prevPixelOffset) || !func(primaryPixelCollider.UpperRight + prevPixelOffset)))
			{
				validLocation = false;
			}
		}
		if (!validLocation)
		{
			ForceBounce((pixelOffset - prevPixelOffset).ToVector2());
		}
	}

	private void ForceBounce(Vector2 normal)
	{
		BounceProjModifier component = GetComponent<BounceProjModifier>();
		float num = (-base.specRigidbody.Velocity).ToAngle();
		float num2 = normal.ToAngle();
		float num3 = BraveMathCollege.ClampAngle360(num + 2f * (num2 - num));
		if (shouldRotate)
		{
			base.transform.Rotate(0f, 0f, num3 - num);
		}
		m_currentDirection = BraveMathCollege.DegreesToVector(num3);
		m_currentSpeed *= 1f - component.percentVelocityToLoseOnBounce;
		if ((bool)base.braveBulletScript && base.braveBulletScript.bullet != null)
		{
			base.braveBulletScript.bullet.Direction = num3;
			base.braveBulletScript.bullet.Speed *= 1f - component.percentVelocityToLoseOnBounce;
		}
		if (component != null)
		{
			component.Bounce(this, base.specRigidbody.UnitCenter);
		}
	}

	private void MaybeEatEnemy(Projectile selfProjectile, SpeculativeRigidbody enemyRigidbody)
	{
		if ((bool)enemyRigidbody)
		{
			AIActor aIActor = enemyRigidbody.aiActor;
			if ((bool)aIActor && (bool)aIActor.healthHaver && !aIActor.healthHaver.IsBoss)
			{
				m_cachedTargetToEat = aIActor;
				aIActor.healthHaver.ManualDeathHandling = true;
				aIActor.healthHaver.OnPreDeath += EatEnemy;
			}
		}
	}

	private void EatEnemy(Vector2 dirVec)
	{
		if ((bool)m_cachedTargetToEat)
		{
			m_cachedTargetToEat.ForceDeath(Vector2.zero, false);
			m_cachedTargetToEat.StartCoroutine(HandleEat(m_cachedTargetToEat));
		}
		m_cachedTargetToEat = null;
	}

	private IEnumerator HandleEat(AIActor targetEat)
	{
		float ela = 0f;
		float duration = 0.75f;
		while (ela < duration)
		{
			ela += BraveTime.DeltaTime;
			if ((bool)targetEat && (bool)targetEat.behaviorSpeculator)
			{
				targetEat.behaviorSpeculator.Interrupt();
				targetEat.ClearPath();
				targetEat.behaviorSpeculator.Stun(1f, false);
			}
			yield return null;
		}
		UnityEngine.Object.Destroy(targetEat.gameObject);
	}

	private IEnumerator FindTarget()
	{
		m_coroutineIsActive = true;
		bool ownerIsPlayer = base.Owner is PlayerController;
		while ((bool)this && (bool)base.Owner && (bool)base.Owner.specRigidbody && GameManager.HasInstance && !GameManager.Instance.IsLoadingLevel)
		{
			if (ownerIsPlayer)
			{
				List<AIActor> activeEnemies = GameManager.Instance.Dungeon.data.GetAbsoluteRoomFromPosition(base.Owner.transform.position.IntXY(VectorConversions.Floor)).GetActiveEnemies(RoomHandler.ActiveEnemyType.All);
				if (activeEnemies != null)
				{
					float num = float.MaxValue;
					for (int i = 0; i < activeEnemies.Count; i++)
					{
						AIActor aIActor = activeEnemies[i];
						if ((bool)aIActor && (bool)aIActor.healthHaver && !aIActor.healthHaver.IsDead && (bool)aIActor.specRigidbody)
						{
							float num2 = Vector2.Distance(aIActor.specRigidbody.UnitCenter, base.Owner.specRigidbody.UnitCenter);
							if (num2 < num)
							{
								CurrentTarget = aIActor;
								num = num2;
							}
						}
					}
				}
			}
			else
			{
				CurrentTarget = GameManager.Instance.GetPlayerClosestToPoint(base.transform.position.XY());
			}
			yield return new WaitForSeconds(1f);
		}
	}

	protected override void Move()
	{
		if (!m_coroutineIsActive)
		{
			StartCoroutine(FindTarget());
		}
		float num = 1f;
		m_pathTimer -= base.LocalDeltaTime;
		if ((bool)base.sprite && base.sprite.HeightOffGround != -1f)
		{
			base.sprite.HeightOffGround = -1f;
			base.sprite.UpdateZDepth();
		}
		if (CurrentTarget != null)
		{
			if (m_pathTimer <= 0f || m_currentPath == null)
			{
				CellTypes cellTypes = CellTypes.FLOOR;
				if (CanCrossPits)
				{
					cellTypes |= CellTypes.PIT;
				}
				Pathfinder.Instance.GetPath(base.specRigidbody.UnitCenter.ToIntVector2(VectorConversions.Floor), CurrentTarget.specRigidbody.UnitCenter.ToIntVector2(VectorConversions.Floor), out m_currentPath, IntVector2.One, cellTypes);
				m_pathTimer = 0.5f;
				if (m_currentPath != null && m_currentPath.Count > 0 && m_currentPath.WillReachFinalGoal)
				{
					m_currentPath.Smooth(base.specRigidbody.UnitCenter, new Vector2(0.25f, 0.25f), cellTypes, false, IntVector2.One);
				}
			}
			Vector2 vector = Vector2.zero;
			if (m_currentPath != null && m_currentPath.WillReachFinalGoal && m_currentPath.Count > 0)
			{
				vector = GetPathVelocityContribution();
			}
			else
			{
				CurrentTarget = null;
			}
			m_noTargetElapsed = 0f;
			m_currentDirection = Vector3.RotateTowards(m_currentDirection, vector, angularAcceleration * ((float)Math.PI / 180f) * BraveTime.DeltaTime, 0f).XY().normalized;
			float f = Vector2.Angle(m_currentDirection, vector);
			num = 0.25f + (1f - Mathf.Clamp01(Mathf.Abs(f) / 60f)) * 0.75f;
		}
		else
		{
			m_noTargetElapsed += BraveTime.DeltaTime;
		}
		if (m_noTargetElapsed > 5f)
		{
			DieInAir(true);
		}
		base.specRigidbody.Velocity = m_currentDirection * m_currentSpeed * num;
		DirectionalAnimation.Info info = animData.GetInfo(base.specRigidbody.Velocity);
		if (!base.sprite.spriteAnimator.IsPlaying(info.name))
		{
			base.sprite.spriteAnimator.Play(info.name);
		}
		if (m_lastGoopPosition.HasValue)
		{
			DeadlyDeadlyGoopManager.GetGoopManagerForGoopType(waterGoop).AddGoopLine(m_lastGoopPosition.Value, base.specRigidbody.UnitCenter, goopRadius);
		}
		m_lastGoopPosition = base.specRigidbody.UnitCenter;
		base.LastVelocity = base.specRigidbody.Velocity;
	}

	private bool GetNextTargetPosition(out Vector2 targetPos)
	{
		if (m_currentPath != null && m_currentPath.Count > 0)
		{
			targetPos = m_currentPath.GetFirstCenterVector2();
			return true;
		}
		Vector2? overridePathEnd = m_overridePathEnd;
		if (overridePathEnd.HasValue)
		{
			targetPos = m_overridePathEnd.Value;
			return true;
		}
		targetPos = Vector2.zero;
		return false;
	}

	private Vector2 GetPathTarget()
	{
		Vector2 unitCenter = base.specRigidbody.UnitCenter;
		Vector2 result = unitCenter;
		float num = baseData.speed * base.LocalDeltaTime;
		Vector2 vector = unitCenter;
		Vector2 targetPos = unitCenter;
		while (!(num <= 0f) && GetNextTargetPosition(out targetPos))
		{
			float num2 = Vector2.Distance(targetPos, unitCenter);
			if (num2 < num)
			{
				num -= num2;
				vector = targetPos;
				result = vector;
				if (m_currentPath != null && m_currentPath.Count > 0)
				{
					m_currentPath.RemoveFirst();
				}
				else
				{
					m_overridePathEnd = null;
				}
				continue;
			}
			result = (targetPos - vector).normalized * num + vector;
			break;
		}
		return result;
	}

	private Vector2 GetPathVelocityContribution()
	{
		if (m_currentPath == null || m_currentPath.Count == 0)
		{
			Vector2? overridePathEnd = m_overridePathEnd;
			if (!overridePathEnd.HasValue)
			{
				return Vector2.zero;
			}
		}
		Vector2 unitCenter = base.specRigidbody.UnitCenter;
		Vector2 pathTarget = GetPathTarget();
		Vector2 vector = pathTarget - unitCenter;
		if (baseData.speed * base.LocalDeltaTime > vector.magnitude)
		{
			return vector / base.LocalDeltaTime;
		}
		return baseData.speed * vector.normalized;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}

	public static GameObject SpawnVFXBehind(GameObject prefab, Vector3 position, Quaternion rotation, bool ignoresPools)
	{
		GameObject gameObject = SpawnManager.SpawnVFX(prefab, position, rotation, ignoresPools);
		tk2dBaseSprite component = gameObject.GetComponent<tk2dBaseSprite>();
		component.scale = m_lastAssignedScale;
		component.depthUsesTrimmedBounds = true;
		component.HeightOffGround = -3f;
		component.UpdateZDepth();
		return gameObject;
	}

	protected override void HandleHitEffectsEnemy(SpeculativeRigidbody rigidbody, CollisionData lcr, bool playProjectileDeathVfx)
	{
		if (hitEffects.alwaysUseMidair)
		{
			HandleHitEffectsMidair();
		}
		else
		{
			if (rigidbody.gameActor == null)
			{
				return;
			}
			Vector3 position = rigidbody.UnitBottomCenter.ToVector3ZUp();
			tk2dSprite component = hitEffects.enemy.effects[0].effects[0].effect.GetComponent<tk2dSprite>();
			Vector3 vector = component.GetRelativePositionFromAnchor(tk2dBaseSprite.Anchor.MiddleCenter);
			position -= vector;
			float zRotation = 0f;
			if ((bool)base.sprite)
			{
				m_lastAssignedScale = base.sprite.scale;
			}
			if ((bool)rigidbody.healthHaver && rigidbody.healthHaver.GetCurrentHealth() <= 0f)
			{
				if (lcr.Contact.x > rigidbody.UnitCenter.x)
				{
					position += new Vector3(1.125f, 0.5f, 0f) - Vector3.Scale(new Vector3(2.25f, 1f, 0f), m_lastAssignedScale - Vector3.one);
					hitEffects.enemy.effects[0].SpawnAtPosition(position, zRotation, null, lcr.Normal, base.specRigidbody.Velocity, -3f, false, SpawnVFXBehind);
				}
				else
				{
					position += new Vector3(1.125f, 0.5f, 0f) - Vector3.Scale(new Vector3(2.25f, 1f, 0f), m_lastAssignedScale - Vector3.one);
					hitEffects.enemy.effects[1].SpawnAtPosition(position, zRotation, null, lcr.Normal, base.specRigidbody.Velocity, -3f, false, SpawnVFXBehind);
				}
				if (ParticlesPrefab != null)
				{
					ParticleSystem component2 = SpawnManager.SpawnParticleSystem(ParticlesPrefab.gameObject, rigidbody.UnitBottomCenter.ToVector3ZUp(rigidbody.UnitBottomCenter.y), Quaternion.identity).GetComponent<ParticleSystem>();
					component2.Play();
				}
			}
			else
			{
				enemyNotKilledPool.SpawnAtPosition(rigidbody.UnitCenter.ToVector3ZUp(), zRotation, null, lcr.Normal, base.specRigidbody.Velocity);
			}
		}
	}
}
