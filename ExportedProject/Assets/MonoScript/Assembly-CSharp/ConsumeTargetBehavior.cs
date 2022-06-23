using System;
using Dungeonator;
using FullInspector;
using Pathfinding;
using UnityEngine;

public class ConsumeTargetBehavior : BasicAttackBehavior
{
	public enum State
	{
		Idle,
		Tell,
		Path,
		Track,
		GrabBegin,
		GrabSuccess,
		Miss,
		WaitingForFinish,
		GrabFinish
	}

	public float PathInterval = 0.2f;

	public float PathMoveSpeed = 7f;

	public float MaxPathTime = 3f;

	public float TrackTime = 0.25f;

	public float PlayerClipSizePenalty = 0.15f;

	[InspectorCategory("Visuals")]
	public string TellAnim;

	[InspectorCategory("Visuals")]
	public string MoveAnim;

	[InspectorCategory("Visuals")]
	public string GrabAnim;

	[InspectorCategory("Visuals")]
	public string MissAnim;

	[InspectorCategory("Visuals")]
	public string GrabFinishAnim;

	private float m_startingHeightOffGround;

	private float m_startMoveSpeed;

	private Vector2 m_posOffset;

	private Vector2 m_targetPosition;

	private float m_repathTimer;

	private float m_timer;

	private Vector2 m_trackOffset;

	private bool m_trackDuringGrab;

	private Vector2? m_resetStartPos;

	private Vector2? m_resetEndPos;

	private PlayerController m_affectedPlayer;

	private State m_state;

	public override void Start()
	{
		base.Start();
		tk2dSpriteAnimator spriteAnimator = m_aiAnimator.spriteAnimator;
		spriteAnimator.AnimationEventTriggered = (Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip, int>)Delegate.Combine(spriteAnimator.AnimationEventTriggered, new Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip, int>(AnimationEventTriggered));
		m_startMoveSpeed = m_aiActor.MovementSpeed;
		m_posOffset = -(m_aiActor.specRigidbody.GetUnitCenter(ColliderType.HitBox) - m_aiActor.transform.position.XY());
	}

	public override void Upkeep()
	{
		base.Upkeep();
		DecrementTimer(ref m_repathTimer);
		DecrementTimer(ref m_timer);
	}

	public override BehaviorResult Update()
	{
		base.Update();
		if (!IsReady())
		{
			return BehaviorResult.Continue;
		}
		if (m_behaviorSpeculator.TargetRigidbody == null)
		{
			return BehaviorResult.Continue;
		}
		m_state = State.Tell;
		m_aiAnimator.PlayUntilCancelled(TellAnim);
		m_aiActor.ClearPath();
		m_updateEveryFrame = true;
		return BehaviorResult.RunContinuous;
	}

	public override ContinuousBehaviorResult ContinuousUpdate()
	{
		base.ContinuousUpdate();
		if (m_state == State.Tell)
		{
			if (!m_aiAnimator.IsPlaying(TellAnim))
			{
				AkSoundEngine.PostEvent("Play_ENM_Tarnisher_Seeking_01", m_aiActor.gameObject);
				m_state = State.Path;
				m_aiAnimator.PlayUntilCancelled(MoveAnim);
				m_aiAnimator.OtherAnimations.Find((AIAnimator.NamedDirectionalAnimation a) => a.name == "pitfall").anim.Prefix = "pitfall_attack";
				m_timer = MaxPathTime;
				m_repathTimer = 0f;
				m_aiActor.MovementSpeed = TurboModeController.MaybeModifyEnemyMovementSpeed(PathMoveSpeed);
				SetPlayerCollision(false);
				Flatten(true);
			}
			return ContinuousBehaviorResult.Continue;
		}
		if (m_state == State.Path)
		{
			float num = PathToTarget();
			if (num < 1.5f)
			{
				m_state = State.Track;
				m_timer = TrackTime;
				if ((bool)m_behaviorSpeculator.TargetRigidbody)
				{
					m_trackOffset = m_aiActor.specRigidbody.GetUnitCenter(ColliderType.HitBox) - m_behaviorSpeculator.TargetRigidbody.GetUnitCenter(ColliderType.HitBox);
					m_targetPosition = m_behaviorSpeculator.TargetRigidbody.GetUnitCenter(ColliderType.Ground);
				}
				else
				{
					m_trackOffset = Vector2.zero;
					m_targetPosition = m_aiActor.specRigidbody.GetUnitCenter(ColliderType.Ground);
				}
			}
			else if (m_timer <= 0f)
			{
				m_state = State.GrabBegin;
				m_aiActor.ClearPath();
				m_aiAnimator.PlayUntilFinished(GrabAnim);
				m_trackDuringGrab = false;
			}
			return ContinuousBehaviorResult.Continue;
		}
		if (m_state == State.Track)
		{
			TrackToTarget(Vector2.Lerp(Vector2.zero, m_trackOffset, m_timer / TrackTime));
			if (m_timer <= 0f)
			{
				m_state = State.GrabBegin;
				m_aiActor.ClearPath();
				m_aiAnimator.PlayUntilFinished(GrabAnim);
				m_aiAnimator.OtherAnimations.Find((AIAnimator.NamedDirectionalAnimation a) => a.name == "pitfall").anim.Prefix = "pitfall";
				m_trackDuringGrab = true;
			}
			return ContinuousBehaviorResult.Continue;
		}
		if (m_state == State.GrabBegin)
		{
			if (m_trackDuringGrab)
			{
				TrackToTarget(Vector2.zero);
			}
			if (!m_aiAnimator.IsPlaying(GrabAnim))
			{
				GetSafeEndPoint();
				return ContinuousBehaviorResult.Finished;
			}
		}
		else if (m_state == State.GrabSuccess)
		{
			if (!m_aiAnimator.IsPlaying(GrabAnim))
			{
				UnconsumePlayer(true);
				return ContinuousBehaviorResult.Finished;
			}
			TrackToSafeEndPoint();
		}
		else if (m_state == State.Miss)
		{
			if (!m_aiAnimator.IsPlaying(MissAnim))
			{
				return ContinuousBehaviorResult.Finished;
			}
			TrackToSafeEndPoint();
		}
		else if (m_state == State.WaitingForFinish)
		{
			if (!m_behaviorSpeculator.TargetRigidbody || (!m_behaviorSpeculator.TargetRigidbody.GroundPixelCollider.Overlaps(m_aiActor.specRigidbody.GroundPixelCollider) && !m_behaviorSpeculator.TargetRigidbody.GroundPixelCollider.Overlaps(m_aiActor.specRigidbody.HitboxPixelCollider)))
			{
				m_aiAnimator.PlayUntilFinished(GrabFinishAnim);
				m_state = State.GrabFinish;
				Flatten(false);
			}
		}
		else if (m_state == State.GrabFinish && !m_aiAnimator.IsPlaying(GrabFinishAnim))
		{
			return ContinuousBehaviorResult.Finished;
		}
		return ContinuousBehaviorResult.Continue;
	}

	public override void EndContinuousUpdate()
	{
		base.EndContinuousUpdate();
		if (m_affectedPlayer != null)
		{
			UnconsumePlayer(false);
		}
		m_state = State.Idle;
		SetPlayerCollision(true);
		if ((bool)m_aiActor)
		{
			m_aiActor.specRigidbody.ClearSpecificCollisionExceptions();
		}
		m_aiActor.MovementSpeed = m_startMoveSpeed;
		Flatten(false);
		m_aiActor.knockbackDoer.SetImmobile(false, "ConsumeTargetBehavior");
		Vector2? resetEndPos = m_resetEndPos;
		if (resetEndPos.HasValue)
		{
			m_aiActor.transform.position = m_resetEndPos.Value;
			m_aiActor.specRigidbody.Reinitialize();
		}
		m_resetStartPos = null;
		m_resetEndPos = null;
		m_updateEveryFrame = false;
		UpdateCooldowns();
	}

	public override void Destroy()
	{
		if ((bool)m_affectedPlayer)
		{
			UnconsumePlayer(false);
		}
		base.Destroy();
	}

	private void AnimationEventTriggered(tk2dSpriteAnimator animator, tk2dSpriteAnimationClip clip, int frameNo)
	{
		tk2dSpriteAnimationFrame frame = clip.GetFrame(frameNo);
		if (m_state == State.GrabBegin && frame.eventInfo == "hit")
		{
			ForceBlank();
			bool flag = false;
			if ((bool)m_behaviorSpeculator.TargetRigidbody)
			{
				PlayerController playerController = m_behaviorSpeculator.TargetRigidbody.gameActor as PlayerController;
				if ((bool)playerController)
				{
					m_aiActor.specRigidbody.RegisterSpecificCollisionException(playerController.specRigidbody);
					if (playerController.CanBeGrabbed && playerController.specRigidbody.HitboxPixelCollider.Overlaps(m_aiActor.specRigidbody.HitboxPixelCollider))
					{
						ConsumePlayer(playerController);
						flag = true;
						m_state = State.GrabSuccess;
					}
				}
			}
			Flatten(false);
			m_aiActor.knockbackDoer.SetImmobile(true, "ConsumeTargetBehavior");
			if (m_state != State.GrabSuccess)
			{
				m_state = State.Miss;
				m_aiAnimator.PlayUntilFinished(MissAnim);
			}
			GetSafeEndPoint();
		}
		if ((m_state == State.GrabSuccess || m_state == State.Miss) && frame.eventInfo == "enable_colliders")
		{
			SetPlayerCollision(true);
			Flatten(false);
		}
		if (m_state == State.GrabSuccess && frame.eventInfo == "release")
		{
			UnconsumePlayer(true);
			Flatten(true);
		}
		if (m_state == State.GrabSuccess && frame.eventInfo == "static")
		{
			m_state = State.WaitingForFinish;
		}
	}

	private void SetPlayerCollision(bool collision)
	{
		if (collision)
		{
			m_aiActor.specRigidbody.RemoveCollisionLayerIgnoreOverride(CollisionMask.LayerToMask(CollisionLayer.PlayerCollider, CollisionLayer.PlayerHitBox));
		}
		else
		{
			m_aiActor.specRigidbody.AddCollisionLayerIgnoreOverride(CollisionMask.LayerToMask(CollisionLayer.PlayerCollider, CollisionLayer.PlayerHitBox));
		}
	}

	private void Flatten(bool flatten)
	{
		m_aiActor.sprite.IsPerpendicular = !flatten;
		m_aiActor.specRigidbody.CollideWithOthers = !flatten;
		m_aiActor.IsGone = flatten;
		if (flatten)
		{
			m_startingHeightOffGround = m_aiActor.sprite.HeightOffGround;
			m_aiActor.sprite.HeightOffGround = -1.5f;
			m_aiActor.sprite.UpdateZDepth();
		}
		else
		{
			m_aiActor.sprite.HeightOffGround = m_startingHeightOffGround;
			m_aiActor.sprite.UpdateZDepth();
		}
	}

	private float PathToTarget()
	{
		if ((bool)m_behaviorSpeculator.TargetRigidbody)
		{
			if (m_repathTimer <= 0f)
			{
				m_aiActor.PathfindToPosition(m_behaviorSpeculator.TargetRigidbody.UnitCenter, null, true, null, null, null, true);
				m_repathTimer = PathInterval;
			}
			return Vector2.Distance(m_behaviorSpeculator.TargetRigidbody.HitboxPixelCollider.UnitCenter, m_aiActor.specRigidbody.HitboxPixelCollider.UnitCenter);
		}
		return -1f;
	}

	private void TrackToTarget(Vector2 additionalOffset)
	{
		if ((bool)m_behaviorSpeculator.TargetRigidbody)
		{
			Vector2 unitCenter = m_behaviorSpeculator.TargetRigidbody.GetUnitCenter(ColliderType.Ground);
			m_targetPosition = Vector2.MoveTowards(m_targetPosition, unitCenter, 10f * m_deltaTime);
			m_aiActor.transform.position = m_targetPosition + m_posOffset + additionalOffset;
			m_aiActor.specRigidbody.Reinitialize();
		}
	}

	private void TrackToSafeEndPoint()
	{
		Vector2? resetStartPos = m_resetStartPos;
		if (resetStartPos.HasValue)
		{
			Vector2? resetEndPos = m_resetEndPos;
			if (resetEndPos.HasValue)
			{
				m_aiActor.transform.position = Vector2.Lerp(m_resetStartPos.Value, m_resetEndPos.Value, m_aiAnimator.CurrentClipProgress);
				m_aiActor.specRigidbody.Reinitialize();
			}
		}
	}

	private void GetSafeEndPoint()
	{
		if (!GameManager.HasInstance || GameManager.Instance.Dungeon == null)
		{
			return;
		}
		DungeonData data = GameManager.Instance.Dungeon.data;
		SpeculativeRigidbody specRigidbody = m_aiActor.specRigidbody;
		Vector2[] array = new Vector2[6] { specRigidbody.UnitBottomLeft, specRigidbody.UnitBottomCenter, specRigidbody.UnitBottomRight, specRigidbody.UnitTopLeft, specRigidbody.UnitTopCenter, specRigidbody.UnitTopRight };
		bool flag = false;
		for (int i = 0; i < array.Length; i++)
		{
			IntVector2 intVector = array[i].ToIntVector2(VectorConversions.Floor);
			if (!data.CheckInBoundsAndValid(intVector) || data.isWall(intVector) || data.isTopWall(intVector.x, intVector.y) || data[intVector].isOccupied)
			{
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			return;
		}
		CellValidator cellValidator = delegate(IntVector2 c)
		{
			for (int j = 0; j < m_aiActor.Clearance.x; j++)
			{
				int x = c.x + j;
				for (int k = 0; k < m_aiActor.Clearance.y; k++)
				{
					int y = c.y + k;
					if (GameManager.Instance.Dungeon.data.isTopWall(x, y))
					{
						return false;
					}
				}
			}
			return true;
		};
		Vector2 vector = m_aiActor.specRigidbody.UnitBottomCenter - m_aiActor.transform.position.XY();
		RoomHandler absoluteRoomFromPosition = GameManager.Instance.Dungeon.data.GetAbsoluteRoomFromPosition(m_aiActor.specRigidbody.UnitCenter.ToIntVector2(VectorConversions.Floor));
		IntVector2? nearestAvailableCell = absoluteRoomFromPosition.GetNearestAvailableCell(m_aiActor.specRigidbody.UnitCenter, m_aiActor.Clearance, m_aiActor.PathableTiles, false, cellValidator);
		if (nearestAvailableCell.HasValue)
		{
			m_resetStartPos = m_aiActor.transform.position;
			m_resetEndPos = Pathfinder.GetClearanceOffset(nearestAvailableCell.Value, m_aiActor.Clearance).WithY(nearestAvailableCell.Value.y) - vector;
		}
		else
		{
			m_resetStartPos = null;
			m_resetEndPos = null;
		}
	}

	private void ConsumePlayer(PlayerController player)
	{
		player.specRigidbody.Velocity = Vector2.zero;
		player.knockbackDoer.TriggerTemporaryKnockbackInvulnerability(1f);
		player.ToggleRenderer(false, "consumed");
		player.ToggleHandRenderers(false, "consumed");
		player.ToggleGunRenderers(false, "consumed");
		player.CurrentInputState = PlayerInputState.NoInput;
		player.healthHaver.IsVulnerable = false;
		m_affectedPlayer = player;
	}

	private void UnconsumePlayer(bool punishPlayer)
	{
		if ((bool)m_affectedPlayer)
		{
			m_affectedPlayer.healthHaver.IsVulnerable = true;
			if (punishPlayer)
			{
				PunishPlayer();
			}
		}
		if ((bool)m_affectedPlayer)
		{
			m_affectedPlayer.ToggleRenderer(true, "consumed");
			m_affectedPlayer.ToggleHandRenderers(true, "consumed");
			m_affectedPlayer.ToggleGunRenderers(true, "consumed");
			m_affectedPlayer.CurrentInputState = PlayerInputState.AllInput;
			m_affectedPlayer.DoSpitOut();
		}
		m_affectedPlayer = null;
		if ((bool)m_aiActor)
		{
			m_aiActor.specRigidbody.ClearSpecificCollisionExceptions();
		}
	}

	private void PunishPlayer()
	{
		if (!m_affectedPlayer || !m_aiActor)
		{
			return;
		}
		if (m_affectedPlayer.HasActiveItem(GlobalItemIds.EitrShield) && PickupObjectDatabase.HasInstance && m_aiActor.AdditionalSafeItemDrops != null)
		{
			m_affectedPlayer.RemoveActiveItem(GlobalItemIds.EitrShield);
			m_aiActor.AdditionalSafeItemDrops.Add(PickupObjectDatabase.Instance.InternalGetById(GlobalItemIds.EitrShield));
			return;
		}
		if ((bool)m_affectedPlayer.healthHaver)
		{
			m_affectedPlayer.healthHaver.ApplyDamage((!m_aiActor.IsBlackPhantom) ? 0.5f : 1f, Vector2.zero, m_aiActor.GetActorName());
		}
		if ((bool)m_affectedPlayer && m_affectedPlayer.ownerlessStatModifiers != null && m_affectedPlayer.stats != null)
		{
			StatModifier statModifier = new StatModifier();
			statModifier.statToBoost = PlayerStats.StatType.TarnisherClipCapacityMultiplier;
			statModifier.amount = 0f - PlayerClipSizePenalty;
			statModifier.modifyType = StatModifier.ModifyMethod.ADDITIVE;
			m_affectedPlayer.ownerlessStatModifiers.Add(statModifier);
			m_affectedPlayer.stats.RecalculateStats(m_affectedPlayer);
		}
		if ((bool)m_affectedPlayer && (bool)m_affectedPlayer.CurrentGun && m_affectedPlayer.CurrentGun.ammo > 0)
		{
			m_affectedPlayer.CurrentGun.ammo = Mathf.RoundToInt((float)m_affectedPlayer.CurrentGun.ammo * 0.85f);
		}
		if (GameStatsManager.HasInstance)
		{
			GameStatsManager.Instance.RegisterStatChange(TrackedStats.TIMES_TARNISHED, 1f);
		}
	}

	public void ForceBlank(float overrideRadius = 5f, float overrideTimeAtMaxRadius = 0.65f)
	{
		if ((bool)m_aiActor && (bool)m_aiActor.specRigidbody)
		{
			GameObject gameObject = new GameObject("silencer");
			SilencerInstance silencerInstance = gameObject.AddComponent<SilencerInstance>();
			silencerInstance.ForceNoDamage = true;
			silencerInstance.TriggerSilencer(m_aiActor.specRigidbody.UnitCenter, 50f, overrideRadius, null, 0f, 0f, 0f, 0f, 0f, 0f, overrideTimeAtMaxRadius, null, false, true);
		}
	}
}
