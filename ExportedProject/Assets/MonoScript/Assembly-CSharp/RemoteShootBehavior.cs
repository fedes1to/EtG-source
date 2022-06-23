using System.Collections.Generic;
using Dungeonator;
using FullInspector;
using Pathfinding;
using UnityEngine;

public class RemoteShootBehavior : BasicAttackBehavior
{
	private enum State
	{
		Idle,
		Casting,
		Firing,
		PostFire
	}

	public bool DefineRadius;

	[InspectorIndent]
	[InspectorShowIf("DefineRadius")]
	public float MinRadius;

	[InspectorShowIf("DefineRadius")]
	[InspectorIndent]
	public float MaxRadius;

	public IntVector2 RemoteFootprint = new IntVector2(1, 1);

	public float TellTime;

	public BulletScriptSelector remoteBulletScript;

	public float FireTime;

	public bool Multifire;

	[InspectorShowIf("Multifire")]
	[InspectorIndent]
	public int MinShots = 2;

	[InspectorIndent]
	[InspectorShowIf("Multifire")]
	public int MaxShots = 3;

	[InspectorIndent]
	[InspectorShowIf("Multifire")]
	public float MidShotTime;

	[InspectorCategory("Visuals")]
	public bool StopDuringAnimation = true;

	[InspectorCategory("Visuals")]
	public string TellAnim;

	[InspectorCategory("Visuals")]
	public string TellVfx;

	[InspectorCategory("Visuals")]
	public string ShootVfx;

	[InspectorCategory("Visuals")]
	public string RemoteVfx;

	[InspectorCategory("Visuals")]
	public string PostFireAnim;

	[InspectorCategory("Visuals")]
	public bool HideGun;

	private State m_state;

	private IntVector2? m_spawnCell;

	private List<IntVector2> m_previousSpawnCells = new List<IntVector2>();

	private float m_timer;

	private int m_shotsRemaining;

	public override void Upkeep()
	{
		base.Upkeep();
		DecrementTimer(ref m_timer);
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
		if (!m_aiActor.TargetRigidbody)
		{
			return BehaviorResult.Continue;
		}
		m_previousSpawnCells.Clear();
		ChooseSpawnLocation();
		IntVector2? spawnCell = m_spawnCell;
		if (!spawnCell.HasValue)
		{
			return BehaviorResult.Continue;
		}
		if (!string.IsNullOrEmpty(TellAnim))
		{
			m_aiAnimator.PlayUntilFinished(TellAnim, true);
			if (StopDuringAnimation)
			{
				if (HideGun)
				{
					m_aiShooter.ToggleGunAndHandRenderers(false, "SummonEnemyBehavior");
				}
				m_aiActor.ClearPath();
			}
		}
		if (!string.IsNullOrEmpty(TellVfx))
		{
			m_aiAnimator.PlayVfx(TellVfx);
		}
		m_timer = TellTime;
		if ((bool)m_aiActor && (bool)m_aiActor.knockbackDoer)
		{
			m_aiActor.knockbackDoer.SetImmobile(true, "SummonEnemyBehavior");
		}
		m_state = State.Casting;
		m_updateEveryFrame = true;
		return BehaviorResult.RunContinuous;
	}

	public override ContinuousBehaviorResult ContinuousUpdate()
	{
		if (m_state == State.Casting)
		{
			if (m_timer <= 0f)
			{
				m_shotsRemaining = ((!Multifire) ? 1 : Random.Range(MinShots, MaxShots + 1));
				Vector2 clearanceOffset = Pathfinder.GetClearanceOffset(m_spawnCell.Value, RemoteFootprint);
				if (!string.IsNullOrEmpty(ShootVfx))
				{
					m_aiAnimator.PlayVfx(ShootVfx);
				}
				if (!string.IsNullOrEmpty(RemoteVfx))
				{
					AIAnimator aiAnimator = m_aiAnimator;
					string remoteVfx = RemoteVfx;
					Vector2? position = clearanceOffset;
					aiAnimator.PlayVfx(remoteVfx, null, null, position);
				}
				SpawnManager.SpawnBulletScript(m_aiActor, remoteBulletScript, clearanceOffset);
				m_state = State.Firing;
				m_shotsRemaining--;
				m_timer = ((m_shotsRemaining <= 0) ? FireTime : MidShotTime);
				return ContinuousBehaviorResult.Continue;
			}
		}
		else if (m_state == State.Firing)
		{
			if (m_timer <= 0f)
			{
				if (m_shotsRemaining > 0)
				{
					ChooseSpawnLocation();
					if (m_spawnCell.HasValue)
					{
						Vector2 clearanceOffset2 = Pathfinder.GetClearanceOffset(m_spawnCell.Value, RemoteFootprint);
						if (!string.IsNullOrEmpty(RemoteVfx))
						{
							AIAnimator aiAnimator2 = m_aiAnimator;
							string remoteVfx = RemoteVfx;
							Vector2? position = clearanceOffset2;
							aiAnimator2.PlayVfx(remoteVfx, null, null, position);
						}
						SpawnManager.SpawnBulletScript(m_aiActor, remoteBulletScript, clearanceOffset2);
					}
					m_shotsRemaining--;
					m_timer = ((m_shotsRemaining <= 0) ? FireTime : MidShotTime);
					return ContinuousBehaviorResult.Continue;
				}
				if (!string.IsNullOrEmpty(PostFireAnim))
				{
					m_state = State.PostFire;
					m_aiAnimator.PlayUntilFinished(PostFireAnim);
					return ContinuousBehaviorResult.Continue;
				}
				return ContinuousBehaviorResult.Finished;
			}
		}
		else if (m_state == State.PostFire && !m_aiAnimator.IsPlaying(PostFireAnim))
		{
			return ContinuousBehaviorResult.Finished;
		}
		return ContinuousBehaviorResult.Continue;
	}

	public override void EndContinuousUpdate()
	{
		base.EndContinuousUpdate();
		if (!string.IsNullOrEmpty(TellAnim))
		{
			m_aiAnimator.EndAnimationIf(TellAnim);
		}
		if (!string.IsNullOrEmpty(TellVfx))
		{
			m_aiAnimator.StopVfx(TellVfx);
		}
		if (!string.IsNullOrEmpty(ShootVfx))
		{
			m_aiAnimator.StopVfx(ShootVfx);
		}
		if (!string.IsNullOrEmpty(RemoteVfx))
		{
			m_aiAnimator.StopVfx(RemoteVfx);
		}
		if (!string.IsNullOrEmpty(PostFireAnim))
		{
			m_aiAnimator.EndAnimationIf(PostFireAnim);
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

	private void ChooseSpawnLocation()
	{
		if (!m_aiActor.TargetRigidbody)
		{
			m_spawnCell = null;
			return;
		}
		Vector2 vector = BraveUtility.ViewportToWorldpoint(new Vector2(0f, 0f), ViewportType.Gameplay);
		Vector2 vector2 = BraveUtility.ViewportToWorldpoint(new Vector2(1f, 1f), ViewportType.Gameplay);
		IntVector2 bottomLeft = vector.ToIntVector2(VectorConversions.Ceil);
		IntVector2 topRight = vector2.ToIntVector2(VectorConversions.Floor) - IntVector2.One;
		Vector2 targetCenter = m_aiActor.TargetRigidbody.GetUnitCenter(ColliderType.HitBox);
		Vector2? additionalTargetCenter = null;
		if (GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER && m_aiActor.PlayerTarget is PlayerController)
		{
			PlayerController otherPlayer = GameManager.Instance.GetOtherPlayer(m_aiActor.PlayerTarget as PlayerController);
			if ((bool)otherPlayer)
			{
				additionalTargetCenter = otherPlayer.specRigidbody.GetUnitCenter(ColliderType.HitBox);
			}
		}
		float minDistanceSquared = MinRadius * MinRadius;
		float maxDistanceSquared = MaxRadius * MaxRadius;
		CellValidator cellValidator = delegate(IntVector2 c)
		{
			for (int i = 0; i < RemoteFootprint.x; i++)
			{
				for (int j = 0; j < RemoteFootprint.y; j++)
				{
					if (GameManager.Instance.Dungeon.data.isTopWall(c.x + i, c.y + j))
					{
						return false;
					}
				}
			}
			if (DefineRadius)
			{
				float num = (float)c.x + 0.5f - targetCenter.x;
				float num2 = (float)c.y + 0.5f - targetCenter.y;
				float num3 = num * num + num2 * num2;
				if (num3 < minDistanceSquared || num3 > maxDistanceSquared)
				{
					return false;
				}
				if (additionalTargetCenter.HasValue)
				{
					num = (float)c.x + 0.5f - additionalTargetCenter.Value.x;
					num2 = (float)c.y + 0.5f - additionalTargetCenter.Value.y;
					num3 = num * num + num2 * num2;
					if (num3 < minDistanceSquared || num3 > maxDistanceSquared)
					{
						return false;
					}
				}
			}
			if (c.x < bottomLeft.x || c.y < bottomLeft.y || c.x + m_aiActor.Clearance.x - 1 > topRight.x || c.y + m_aiActor.Clearance.y - 1 > topRight.y)
			{
				return false;
			}
			for (int k = 0; k < m_previousSpawnCells.Count; k++)
			{
				if (c.x == m_previousSpawnCells[k].x && c.y == m_previousSpawnCells[k].y)
				{
					return false;
				}
			}
			return true;
		};
		m_spawnCell = m_aiActor.ParentRoom.GetRandomAvailableCell(RemoteFootprint, CellTypes.FLOOR | CellTypes.PIT, false, cellValidator);
		IntVector2? spawnCell = m_spawnCell;
		if (spawnCell.HasValue)
		{
			m_previousSpawnCells.Add(m_spawnCell.Value);
		}
	}
}
