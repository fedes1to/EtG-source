using System;
using System.Collections;
using System.Collections.Generic;
using Dungeonator;
using Pathfinding;
using UnityEngine;

public class TigerProjectile : Projectile
{
	public DirectionalAnimation animData;

	public float angularAcceleration = 10f;

	[NonSerialized]
	protected GameActor CurrentTarget;

	[NonSerialized]
	protected bool m_coroutineIsActive;

	private AIActor m_cachedTargetToEat;

	private float m_moveElapsed;

	private float m_pathTimer;

	private Path m_currentPath;

	private static Vector3 m_lastAssignedScale = Vector3.one;

	private float m_noTargetElapsed;

	public override void Start()
	{
		base.Start();
		hitEffects.HasProjectileDeathVFX = false;
		base.specRigidbody.CollideWithTileMap = false;
		OnHitEnemy = (Action<Projectile, SpeculativeRigidbody, bool>)Delegate.Combine(OnHitEnemy, new Action<Projectile, SpeculativeRigidbody, bool>(HandleHitEnemy));
		OnWillKillEnemy = (Action<Projectile, SpeculativeRigidbody>)Delegate.Combine(OnWillKillEnemy, new Action<Projectile, SpeculativeRigidbody>(MaybeEatEnemy));
	}

	private void HandleHitEnemy(Projectile arg1, SpeculativeRigidbody arg2, bool arg3)
	{
		StartCoroutine(FrameDelayedDestruction());
	}

	private IEnumerator FrameDelayedDestruction()
	{
		yield return null;
		DieInAir();
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
		if ((bool)m_cachedTargetToEat && (!m_cachedTargetToEat.healthHaver || !m_cachedTargetToEat.healthHaver.IsBoss))
		{
			m_cachedTargetToEat.ForceDeath(Vector2.zero, false);
			m_cachedTargetToEat.StartCoroutine(HandleEat(m_cachedTargetToEat));
			m_cachedTargetToEat = null;
		}
	}

	private IEnumerator HandleEat(AIActor targetEat)
	{
		float ela = 0f;
		float duration = 1f;
		while (ela < duration)
		{
			ela += BraveTime.DeltaTime;
			if ((bool)targetEat && (bool)targetEat.behaviorSpeculator)
			{
				targetEat.behaviorSpeculator.Interrupt();
				targetEat.ClearPath();
				targetEat.behaviorSpeculator.Stun(1f);
			}
			yield return null;
		}
		UnityEngine.Object.Destroy(targetEat.gameObject);
	}

	private IEnumerator FindTarget()
	{
		m_coroutineIsActive = true;
		bool ownerIsPlayer = base.Owner is PlayerController;
		while (true)
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
						if ((bool)aIActor && !aIActor.healthHaver.IsDead)
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
		m_moveElapsed += BraveTime.DeltaTime;
		m_pathTimer -= BraveTime.DeltaTime;
		if (!base.specRigidbody.CollideWithTileMap && m_moveElapsed > 0.5f)
		{
			m_moveElapsed = 0f;
			base.specRigidbody.CollideWithTileMap = true;
			if (PhysicsEngine.Instance.OverlapCast(base.specRigidbody, null, true, false, null, null, false, null, null))
			{
				base.specRigidbody.CollideWithTileMap = false;
			}
		}
		float num = 1f;
		if (CurrentTarget != null)
		{
			m_noTargetElapsed = 0f;
			if (m_currentPath == null || m_pathTimer <= 0f)
			{
				bool path = Pathfinder.Instance.GetPath(base.specRigidbody.Position.UnitPosition.ToIntVector2(VectorConversions.Floor), CurrentTarget.specRigidbody.UnitCenter.ToIntVector2(VectorConversions.Ceil), out m_currentPath, base.specRigidbody.UnitDimensions.ToIntVector2(VectorConversions.Ceil), CellTypes.FLOOR | CellTypes.PIT);
				m_pathTimer = 0.25f;
				if (!path)
				{
					m_currentPath = null;
				}
				else
				{
					m_currentPath.Smooth(base.specRigidbody.Position.UnitPosition, base.specRigidbody.UnitDimensions / 2f, CellTypes.FLOOR | CellTypes.PIT, true, base.specRigidbody.UnitDimensions.ToIntVector2(VectorConversions.Ceil));
				}
			}
			if (m_currentPath != null && m_currentPath.Positions.Count > 2)
			{
				Vector2 normalized = (m_currentPath.GetSecondCenterVector2() - base.specRigidbody.UnitCenter).normalized;
				m_currentDirection = Vector3.RotateTowards(m_currentDirection, normalized, angularAcceleration * ((float)Math.PI / 180f) * BraveTime.DeltaTime, 0f).XY().normalized;
			}
			else
			{
				Vector2 normalized2 = (CurrentTarget.specRigidbody.UnitCenter - base.specRigidbody.UnitCenter).normalized;
				m_currentDirection = Vector3.RotateTowards(m_currentDirection, normalized2, angularAcceleration * ((float)Math.PI / 180f) * BraveTime.DeltaTime, 0f).XY().normalized;
			}
		}
		else
		{
			m_noTargetElapsed += BraveTime.DeltaTime;
		}
		if (m_noTargetElapsed > 3f)
		{
			DieInAir(true);
		}
		base.specRigidbody.Velocity = m_currentDirection * baseData.speed * num;
		DirectionalAnimation.Info info = animData.GetInfo(base.specRigidbody.Velocity);
		if (!base.sprite.spriteAnimator.IsPlaying(info.name))
		{
			base.sprite.spriteAnimator.Play(info.name);
		}
		base.LastVelocity = base.specRigidbody.Velocity;
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
		component.UpdateZDepth();
		return gameObject;
	}

	protected override void HandleHitEffectsEnemy(SpeculativeRigidbody rigidbody, CollisionData lcr, bool playProjectileDeathVfx)
	{
		if ((bool)rigidbody && (bool)rigidbody.gameActor)
		{
			Vector3 position = rigidbody.UnitBottomCenter.ToVector3ZUp();
			tk2dSprite component = hitEffects.deathEnemy.effects[0].effects[0].effect.GetComponent<tk2dSprite>();
			Vector3 vector = component.GetRelativePositionFromAnchor(tk2dBaseSprite.Anchor.MiddleCenter);
			position -= vector;
			float zRotation = 0f;
			if ((bool)base.sprite)
			{
				m_lastAssignedScale = base.sprite.scale;
			}
			if (lcr.Contact.x > rigidbody.UnitCenter.x)
			{
				position += new Vector3(1f, 2f, 0f) - Vector3.Scale(new Vector3(2f, 4f, 0f), m_lastAssignedScale - Vector3.one);
				hitEffects.deathEnemy.effects[0].SpawnAtPosition(position, zRotation, null, lcr.Normal, base.specRigidbody.Velocity, -3f, false, SpawnVFXBehind);
			}
			else
			{
				position += new Vector3(-1f, 2f, 0f) - Vector3.Scale(new Vector3(2f, 4f, 0f), m_lastAssignedScale - Vector3.one);
				hitEffects.deathEnemy.effects[1].SpawnAtPosition(position, zRotation, null, lcr.Normal, base.specRigidbody.Velocity, -3f, false, SpawnVFXBehind);
			}
		}
	}
}
