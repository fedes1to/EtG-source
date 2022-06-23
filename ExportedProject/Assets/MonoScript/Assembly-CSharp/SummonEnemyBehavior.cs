using System.Collections.Generic;
using Dungeonator;
using FullInspector;
using Pathfinding;
using UnityEngine;

public class SummonEnemyBehavior : BasicAttackBehavior
{
	public enum SelectionType
	{
		Random,
		Ordered
	}

	private enum State
	{
		Idle,
		Summoning,
		WaitingForSummonAnim,
		WaitingForPostAnim
	}

	public bool DefineSpawnRadius;

	[InspectorShowIf("DefineSpawnRadius")]
	[InspectorIndent]
	public float MinSpawnRadius;

	[InspectorShowIf("DefineSpawnRadius")]
	[InspectorIndent]
	public float MaxSpawnRadius;

	public int MaxRoomOccupancy = -1;

	public int MaxSummonedAtOnce = -1;

	public int MaxToSpawn = -1;

	public int NumToSpawn = 1;

	public bool KillSpawnedOnDeath;

	[InspectorShowIf("ShowCraze")]
	[InspectorIndent]
	public bool CrazeAfterMaxSpawned;

	public float BlackPhantomChance;

	public List<string> EnemeyGuids;

	public SelectionType selectionType;

	public GameObject OverrideCorpse;

	public float SummonTime;

	public bool DisableDrops = true;

	public bool HideGun;

	[InspectorCategory("Visuals")]
	public bool StopDuringAnimation = true;

	[InspectorCategory("Visuals")]
	public string SummonAnim;

	[InspectorCategory("Visuals")]
	public string SummonVfx;

	[InspectorCategory("Visuals")]
	public string TargetVfx;

	[InspectorCategory("Visuals")]
	public bool TargetVfxLoops = true;

	[InspectorCategory("Visuals")]
	public string PostSummonAnim;

	public bool ManuallyDefineRoom;

	[InspectorIndent]
	[InspectorShowIf("ManuallyDefineRoom")]
	public Vector2 roomMin;

	[InspectorIndent]
	[InspectorShowIf("ManuallyDefineRoom")]
	public Vector2 roomMax;

	private State m_state;

	private string m_enemyGuid;

	private AIActor m_enemyPrefab;

	private IntVector2? m_spawnCell;

	private float m_timer;

	private AIActor m_spawnedActor;

	private tk2dSpriteAnimationClip m_spawnClip;

	private IntVector2 m_enemyClearance;

	private List<AIActor> m_allSpawnedActors = new List<AIActor>();

	private int m_numToSpawn;

	private int m_spawnCount;

	private int m_lifetimeSpawnCount;

	private CrazedController m_crazeBehavior;

	private bool ShowCraze()
	{
		return MaxToSpawn > 0;
	}

	public override void Start()
	{
		base.Start();
		m_crazeBehavior = m_aiActor.GetComponent<CrazedController>();
	}

	public override void Upkeep()
	{
		base.Upkeep();
		DecrementTimer(ref m_timer);
		if (MaxToSpawn > 0 && m_lifetimeSpawnCount >= MaxToSpawn && CrazeAfterMaxSpawned && m_crazeBehavior != null)
		{
			m_crazeBehavior.GoCrazed();
		}
		for (int num = m_allSpawnedActors.Count - 1; num >= 0; num--)
		{
			if (!m_allSpawnedActors[num] || !m_allSpawnedActors[num].healthHaver || m_allSpawnedActors[num].healthHaver.IsDead)
			{
				m_allSpawnedActors.RemoveAt(num);
			}
		}
	}

	public override BehaviorResult Update()
	{
		BehaviorResult behaviorResult = base.Update();
		if (behaviorResult != 0)
		{
			return behaviorResult;
		}
		if (!IsReady())
		{
			return BehaviorResult.Continue;
		}
		if (MaxRoomOccupancy >= 0 && m_aiActor.ParentRoom.GetActiveEnemies(RoomHandler.ActiveEnemyType.All).Count >= MaxRoomOccupancy)
		{
			return BehaviorResult.Continue;
		}
		PrepareSpawn();
		IntVector2? spawnCell = m_spawnCell;
		if (!spawnCell.HasValue)
		{
			return BehaviorResult.Continue;
		}
		if (!string.IsNullOrEmpty(SummonAnim))
		{
			m_aiAnimator.PlayUntilFinished(SummonAnim, true);
			if (StopDuringAnimation)
			{
				if (HideGun)
				{
					m_aiShooter.ToggleGunAndHandRenderers(false, "SummonEnemyBehavior");
				}
				m_aiActor.ClearPath();
			}
		}
		if (!string.IsNullOrEmpty(SummonVfx))
		{
			m_aiAnimator.PlayVfx(SummonVfx);
		}
		if (!string.IsNullOrEmpty(TargetVfx))
		{
			AIAnimator aiAnimator = m_aiAnimator;
			string targetVfx = TargetVfx;
			Vector2? position = Pathfinder.GetClearanceOffset(m_spawnCell.Value, m_enemyClearance);
			aiAnimator.PlayVfx(targetVfx, null, null, position);
		}
		m_timer = SummonTime;
		m_spawnCount = 0;
		if ((bool)m_aiActor && (bool)m_aiActor.knockbackDoer)
		{
			m_aiActor.knockbackDoer.SetImmobile(true, "SummonEnemyBehavior");
		}
		m_numToSpawn = NumToSpawn;
		if (MaxRoomOccupancy >= 0)
		{
			m_numToSpawn = Mathf.Min(m_numToSpawn, MaxRoomOccupancy - m_aiActor.ParentRoom.GetActiveEnemies(RoomHandler.ActiveEnemyType.All).Count);
		}
		if (MaxSummonedAtOnce >= 0)
		{
			m_numToSpawn = Mathf.Min(m_numToSpawn, MaxSummonedAtOnce - m_allSpawnedActors.Count);
		}
		if (MaxToSpawn >= 0)
		{
			m_numToSpawn = Mathf.Min(m_numToSpawn, MaxToSpawn - m_lifetimeSpawnCount);
		}
		m_state = State.Summoning;
		m_updateEveryFrame = true;
		return BehaviorResult.RunContinuous;
	}

	public override ContinuousBehaviorResult ContinuousUpdate()
	{
		if (m_state == State.Summoning)
		{
			if (m_timer <= 0f)
			{
				m_spawnedActor = AIActor.Spawn(m_enemyPrefab, m_spawnCell.Value, m_aiActor.ParentRoom, false, AIActor.AwakenAnimationType.Spawn);
				m_spawnedActor.aiAnimator.PlayDefaultSpawnState();
				m_allSpawnedActors.Add(m_spawnedActor);
				m_spawnedActor.CanDropCurrency = false;
				if (OverrideCorpse != null)
				{
					m_spawnedActor.CorpseObject = OverrideCorpse;
				}
				if (BlackPhantomChance > 0f && (BlackPhantomChance >= 1f || Random.value < BlackPhantomChance))
				{
					m_spawnedActor.ForceBlackPhantom = true;
				}
				m_spawnCount++;
				m_lifetimeSpawnCount++;
				if (m_spawnCount < m_numToSpawn)
				{
					PrepareSpawn();
					IntVector2? spawnCell = m_spawnCell;
					if (spawnCell.HasValue)
					{
						if (!string.IsNullOrEmpty(TargetVfx))
						{
							if (TargetVfxLoops)
							{
								m_aiAnimator.StopVfx(TargetVfx);
							}
							AIAnimator aiAnimator = m_aiAnimator;
							string targetVfx = TargetVfx;
							Vector2? position = Pathfinder.GetClearanceOffset(m_spawnCell.Value, m_enemyClearance);
							aiAnimator.PlayVfx(targetVfx, null, null, position);
						}
						m_timer = SummonTime;
						return ContinuousBehaviorResult.Continue;
					}
				}
				m_spawnClip = m_spawnedActor.spriteAnimator.CurrentClip;
				if (m_spawnClip != null && m_spawnClip.wrapMode != 0)
				{
					m_state = State.WaitingForSummonAnim;
					return ContinuousBehaviorResult.Continue;
				}
				if (!string.IsNullOrEmpty(PostSummonAnim))
				{
					m_state = State.WaitingForPostAnim;
					m_aiAnimator.PlayUntilFinished(PostSummonAnim);
					return ContinuousBehaviorResult.Continue;
				}
				return ContinuousBehaviorResult.Finished;
			}
		}
		else if (m_state == State.WaitingForSummonAnim)
		{
			if (!m_spawnedActor || !m_spawnedActor.healthHaver || m_spawnedActor.healthHaver.IsDead || !m_spawnedActor.spriteAnimator.IsPlaying(m_spawnClip))
			{
				if (!string.IsNullOrEmpty(PostSummonAnim))
				{
					m_state = State.WaitingForPostAnim;
					m_aiAnimator.PlayUntilFinished(PostSummonAnim);
					return ContinuousBehaviorResult.Continue;
				}
				return ContinuousBehaviorResult.Finished;
			}
		}
		else if (m_state == State.WaitingForPostAnim && !m_aiActor.spriteAnimator.IsPlaying(PostSummonAnim))
		{
			return ContinuousBehaviorResult.Finished;
		}
		return ContinuousBehaviorResult.Continue;
	}

	public override void EndContinuousUpdate()
	{
		base.EndContinuousUpdate();
		if (!string.IsNullOrEmpty(SummonAnim))
		{
			m_aiAnimator.EndAnimationIf(SummonAnim);
		}
		if (!string.IsNullOrEmpty(SummonVfx))
		{
			m_aiAnimator.StopVfx(SummonVfx);
		}
		if (!string.IsNullOrEmpty(TargetVfx) && TargetVfxLoops)
		{
			m_aiAnimator.StopVfx(TargetVfx);
		}
		if (!string.IsNullOrEmpty(PostSummonAnim))
		{
			m_aiAnimator.EndAnimationIf(PostSummonAnim);
		}
		if (HideGun)
		{
			m_aiShooter.ToggleGunAndHandRenderers(true, "SummonEnemyBehavior");
		}
		if ((bool)m_aiActor && (bool)m_aiActor.knockbackDoer)
		{
			m_aiActor.knockbackDoer.SetImmobile(false, "SummonEnemyBehavior");
		}
		m_state = State.Idle;
		m_updateEveryFrame = false;
		UpdateCooldowns();
	}

	public override bool IsReady()
	{
		if (MaxToSpawn > 0 && m_lifetimeSpawnCount >= MaxToSpawn)
		{
			return false;
		}
		if (MaxSummonedAtOnce > 0 && m_allSpawnedActors.Count >= MaxSummonedAtOnce)
		{
			return false;
		}
		return base.IsReady();
	}

	public override void OnActorPreDeath()
	{
		if (!KillSpawnedOnDeath)
		{
			return;
		}
		for (int i = 0; i < m_allSpawnedActors.Count; i++)
		{
			AIActor aIActor = m_allSpawnedActors[i];
			if ((bool)aIActor && (bool)aIActor.healthHaver && aIActor.healthHaver.IsAlive)
			{
				aIActor.healthHaver.ApplyDamage(10000f, Vector2.zero, "Summoner Death", CoreDamageTypes.None, DamageCategory.Unstoppable);
			}
		}
	}

	private void PrepareSpawn()
	{
		m_enemyGuid = ((selectionType != SelectionType.Ordered) ? BraveUtility.RandomElement(EnemeyGuids) : EnemeyGuids[m_lifetimeSpawnCount]);
		m_enemyPrefab = EnemyDatabase.GetOrLoadByGuid(m_enemyGuid);
		PixelCollider groundPixelCollider = m_enemyPrefab.specRigidbody.GroundPixelCollider;
		if (groundPixelCollider != null && groundPixelCollider.ColliderGenerationMode == PixelCollider.PixelColliderGeneration.Manual)
		{
			m_enemyClearance = new Vector2((float)groundPixelCollider.ManualWidth / 16f, (float)groundPixelCollider.ManualHeight / 16f).ToIntVector2(VectorConversions.Ceil);
		}
		else
		{
			Debug.LogFormat("Enemy type {0} does not have a manually defined ground collider!", m_enemyPrefab.name);
			m_enemyClearance = IntVector2.One;
		}
		Vector2 vector = BraveUtility.ViewportToWorldpoint(new Vector2(0f, 0f), ViewportType.Gameplay);
		Vector2 vector2 = BraveUtility.ViewportToWorldpoint(new Vector2(1f, 1f), ViewportType.Gameplay);
		IntVector2 bottomLeft = vector.ToIntVector2(VectorConversions.Ceil);
		IntVector2 topRight = vector2.ToIntVector2(VectorConversions.Floor) - IntVector2.One;
		Vector2 center = m_aiActor.specRigidbody.GetUnitCenter(ColliderType.Ground);
		float minDistanceSquared = MinSpawnRadius * MinSpawnRadius;
		float maxDistanceSquared = MaxSpawnRadius * MaxSpawnRadius;
		CellValidator cellValidator = delegate(IntVector2 c)
		{
			for (int i = 0; i < m_enemyClearance.x; i++)
			{
				for (int j = 0; j < m_enemyClearance.y; j++)
				{
					if (GameManager.Instance.Dungeon.data.isTopWall(c.x + i, c.y + j))
					{
						return false;
					}
					if (ManuallyDefineRoom && ((float)(c.x + i) < roomMin.x || (float)(c.x + i) > roomMax.x || (float)(c.y + j) < roomMin.y || (float)(c.y + j) > roomMax.y))
					{
						return false;
					}
				}
			}
			if (DefineSpawnRadius)
			{
				float num = (float)c.x + 0.5f - center.x;
				float num2 = (float)c.y + 0.5f - center.y;
				float num3 = num * num + num2 * num2;
				if (num3 < minDistanceSquared || num3 > maxDistanceSquared)
				{
					return false;
				}
			}
			else if (c.x < bottomLeft.x || c.y < bottomLeft.y || c.x + m_aiActor.Clearance.x - 1 > topRight.x || c.y + m_aiActor.Clearance.y - 1 > topRight.y)
			{
				return false;
			}
			return true;
		};
		m_spawnCell = m_aiActor.ParentRoom.GetRandomAvailableCell(m_enemyClearance, m_enemyPrefab.PathableTiles, true, cellValidator);
	}
}
