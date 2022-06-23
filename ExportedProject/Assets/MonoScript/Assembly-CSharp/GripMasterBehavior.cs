using System;
using System.Collections;
using System.Collections.Generic;
using Dungeonator;
using FullInspector;
using UnityEngine;

public class GripMasterBehavior : BasicAttackBehavior
{
	public enum State
	{
		Idle,
		Tell,
		Grab,
		Miss
	}

	public float TrackTime;

	public float TellTime;

	public int RoomsToSendBackward = 1;

	[InspectorCategory("Visuals")]
	public string TellAnim;

	[InspectorCategory("Visuals")]
	public string GrabAnim;

	[InspectorCategory("Visuals")]
	public string MissAnim;

	[InspectorCategory("Visuals")]
	public string ShadowAnim;

	private GripMasterController m_gripMasterController;

	private Vector2 m_posOffset;

	private Vector2 m_targetPosition;

	private float m_timer;

	private Vector2 m_startPos;

	private bool m_hasHit;

	private bool m_sentPlayerBack;

	private State m_state;

	public override void Start()
	{
		base.Start();
		m_posOffset = -(m_aiActor.specRigidbody.GetUnitCenter(ColliderType.Ground) - m_aiActor.transform.position.XY());
		tk2dSpriteAnimator spriteAnimator = m_aiAnimator.spriteAnimator;
		spriteAnimator.AnimationEventTriggered = (Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip, int>)Delegate.Combine(spriteAnimator.AnimationEventTriggered, new Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip, int>(AnimationEventTriggered));
		m_gripMasterController = m_aiActor.GetComponent<GripMasterController>();
	}

	public override void Upkeep()
	{
		base.Upkeep();
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
		m_timer = TellTime;
		m_startPos = m_aiActor.specRigidbody.GetUnitCenter(ColliderType.Ground);
		m_aiAnimator.PlayUntilCancelled(TellAnim);
		m_aiActor.ClearPath();
		AkSoundEngine.PostEvent("Play_ENM_Grip_Master_Lockon_01", m_aiActor.gameObject);
		m_targetPosition = m_behaviorSpeculator.TargetRigidbody.GetUnitCenter(ColliderType.Ground);
		m_gripMasterController.IsAttacking = true;
		m_updateEveryFrame = true;
		return BehaviorResult.RunContinuous;
	}

	public override ContinuousBehaviorResult ContinuousUpdate()
	{
		base.ContinuousUpdate();
		if (m_state == State.Tell)
		{
			if (m_state != 0 && (bool)m_aiActor.TargetRigidbody)
			{
				float num = TellTime - m_timer;
				UpdateTargetPosition();
				Vector2 vector = Vector2Extensions.SmoothStep(m_startPos, m_targetPosition, Mathf.Clamp01(num / TrackTime));
				m_aiActor.transform.position = vector + m_posOffset;
				m_aiActor.specRigidbody.Reinitialize();
			}
			if (m_timer <= 0f)
			{
				m_state = State.Grab;
				m_aiAnimator.PlayUntilFinished(GrabAnim);
				if (!string.IsNullOrEmpty(ShadowAnim))
				{
					m_aiActor.ShadowObject.GetComponent<tk2dSpriteAnimator>().Play(ShadowAnim);
				}
				return ContinuousBehaviorResult.Continue;
			}
		}
		else if (m_state == State.Grab)
		{
			if (m_state != 0 && (bool)m_aiActor.TargetRigidbody && !m_hasHit)
			{
				UpdateTargetPosition();
				m_aiActor.transform.position = m_targetPosition + m_posOffset;
				m_aiActor.specRigidbody.Reinitialize();
			}
			if (!m_aiAnimator.IsPlaying(GrabAnim))
			{
				return ContinuousBehaviorResult.Finished;
			}
		}
		else if (m_state == State.Miss && !m_aiAnimator.IsPlaying(MissAnim))
		{
			return ContinuousBehaviorResult.Finished;
		}
		return ContinuousBehaviorResult.Continue;
	}

	public override void EndContinuousUpdate()
	{
		base.EndContinuousUpdate();
		m_state = State.Idle;
		m_hasHit = false;
		m_aiActor.sprite.HeightOffGround = 4f;
		if (!m_sentPlayerBack)
		{
			m_gripMasterController.IsAttacking = false;
		}
		m_updateEveryFrame = false;
		UpdateCooldowns();
	}

	private void AnimationEventTriggered(tk2dSpriteAnimator animator, tk2dSpriteAnimationClip clip, int frameNo)
	{
		tk2dSpriteAnimationFrame frame = clip.GetFrame(frameNo);
		if (!m_hasHit && m_state == State.Grab && frame.eventInfo == "hit")
		{
			m_aiActor.sprite.HeightOffGround = 0f;
			m_hasHit = true;
			if ((bool)m_gripMasterController)
			{
				m_gripMasterController.OnAttack();
			}
			ForceBlank();
			bool flag = false;
			if ((bool)m_aiActor.TargetRigidbody)
			{
				PlayerController playerController = m_aiActor.TargetRigidbody.gameActor as PlayerController;
				if ((bool)playerController)
				{
					Vector2 unitCenter = m_behaviorSpeculator.TargetRigidbody.GetUnitCenter(ColliderType.Ground);
					if (playerController.CanBeGrabbed && Vector2.Distance(unitCenter, m_targetPosition) < 1f)
					{
						BanishPlayer(playerController);
						flag = true;
					}
				}
			}
			if (!flag)
			{
				m_state = State.Miss;
				m_aiAnimator.PlayUntilFinished(MissAnim);
			}
			m_aiActor.MoveToSafeSpot(0.5f);
		}
		if (frame.eventInfo == "lift")
		{
			m_aiActor.sprite.HeightOffGround = 4f;
		}
	}

	private void UpdateTargetPosition()
	{
		if ((bool)m_behaviorSpeculator.TargetRigidbody)
		{
			Vector2 unitCenter = m_behaviorSpeculator.TargetRigidbody.GetUnitCenter(ColliderType.Ground);
			m_targetPosition = Vector2.MoveTowards(m_targetPosition, unitCenter, 10f * m_deltaTime);
		}
	}

	private void BanishPlayer(PlayerController player)
	{
		GameStatsManager.Instance.RegisterStatChange(TrackedStats.TIMES_HIT_WITH_THE_GRIPPY, 1f);
		int num = RoomsToSendBackward;
		if ((bool)m_gripMasterController && m_gripMasterController.Grip_OverrideRoomsToSendBackward > 0)
		{
			num = m_gripMasterController.Grip_OverrideRoomsToSendBackward;
		}
		if (num < 1)
		{
			num = 1;
		}
		List<RoomHandler> list = new List<RoomHandler>();
		List<RoomHandler> list2 = new List<RoomHandler>();
		list.Add(player.CurrentRoom);
		while (list.Count - 1 < RoomsToSendBackward)
		{
			RoomHandler roomHandler = list[list.Count - 1];
			list2.Clear();
			foreach (RoomHandler connectedRoom in roomHandler.connectedRooms)
			{
				if (connectedRoom.hasEverBeenVisited && connectedRoom.distanceFromEntrance < roomHandler.distanceFromEntrance && !list.Contains(connectedRoom) && (!connectedRoom.area.IsProceduralRoom || connectedRoom.area.proceduralCells == null))
				{
					list2.Add(connectedRoom);
				}
			}
			if (list2.Count == 0)
			{
				break;
			}
			list.Add(BraveUtility.RandomElement(list2));
		}
		if (list.Count > 1)
		{
			player.RespawnInPreviousRoom(false, PlayerController.EscapeSealedRoomStyle.GRIP_MASTER, true, list[list.Count - 1]);
			for (int i = 1; i < list.Count - 1; i++)
			{
				list[i].ResetPredefinedRoomLikeDarkSouls();
			}
			Debug.LogFormat("Sending the player back {0} rooms (attempted {1})", list.Count - 1, num);
		}
		else
		{
			player.RespawnInPreviousRoom(false, PlayerController.EscapeSealedRoomStyle.GRIP_MASTER, true);
			Debug.LogFormat("Sending the player back with RespawnInPreviousRoom (no valid \"backwards\" rooms found!)");
		}
		player.specRigidbody.Velocity = Vector2.zero;
		player.knockbackDoer.TriggerTemporaryKnockbackInvulnerability(1f);
		m_aiActor.StartCoroutine(ForceAnimateCR(player));
		m_sentPlayerBack = true;
	}

	private IEnumerator ForceAnimateCR(PlayerController player)
	{
		tk2dSpriteAnimator shadowAnimator = null;
		if ((bool)m_aiActor.ShadowObject)
		{
			shadowAnimator = m_aiActor.ShadowObject.GetComponent<tk2dSpriteAnimator>();
		}
		float elapsed = 0f;
		float duration = 3.9f;
		while (elapsed < duration)
		{
			elapsed += GameManager.INVARIANT_DELTA_TIME;
			m_aiAnimator.UpdateAnimation(GameManager.INVARIANT_DELTA_TIME);
			if ((bool)shadowAnimator)
			{
				shadowAnimator.UpdateAnimation(GameManager.INVARIANT_DELTA_TIME);
			}
			yield return null;
		}
		if ((bool)m_aiActor)
		{
			m_behaviorSpeculator.InterruptAndDisable();
			UnityEngine.Object.Destroy(m_aiActor.gameObject);
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
