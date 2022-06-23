using System;
using System.Collections.Generic;
using System.Linq;
using FullInspector;
using UnityEngine;

public class AttackMoveBehavior : BasicAttackBehavior
{
	public enum PositionType
	{
		RelativeToRoomCenter = 20,
		RelativeToHelicopterCenter = 40
	}

	public enum SelectType
	{
		Random = 10,
		Closest = 20,
		RandomClosestN = 30,
		Furthest = 40,
		RandomFurthestN = 50,
		InSequence = 60
	}

	private enum MoveState
	{
		None,
		PreMove,
		Move
	}

	public PositionType positionType = PositionType.RelativeToRoomCenter;

	public Vector2[] Positions;

	public SelectType selectType = SelectType.Random;

	[InspectorShowIf("ShowN")]
	[InspectorIndent]
	public int N;

	[InspectorIndent]
	[InspectorShowIf("ShowDisallowNearest")]
	public bool DisallowNearest;

	public bool SmoothStep = true;

	public float MoveTime = 1f;

	public float MinSpeed;

	public float MaxSpeed;

	[InspectorShowIf("ShowSubsequentMoveSpeed")]
	public float SubsequentMoveSpeed = -1f;

	public bool MirrorIfCloser;

	public bool DisableCollisionDuringMove;

	[InspectorCategory("Attack")]
	public GameObject ShootPoint;

	[InspectorCategory("Attack")]
	public BulletScriptSelector bulletScript;

	[InspectorCategory("Visuals")]
	public string preMoveAnimation;

	[InspectorCategory("Visuals")]
	public string moveAnimation;

	[InspectorCategory("Visuals")]
	public bool disableGoops;

	[InspectorCategory("Visuals")]
	public bool updateFacingDirectionDuringMove = true;

	[InspectorCategory("Visuals")]
	public bool biasFacingRoomCenter;

	[InspectorCategory("Visuals")]
	public bool faceBottomCenter;

	[InspectorCategory("Visuals")]
	public bool enableShadowTrail;

	[InspectorCategory("Visuals")]
	public bool HideGun;

	[InspectorCategory("Visuals")]
	public bool animateShadow;

	[InspectorShowIf("animateShadow")]
	[InspectorCategory("Visuals")]
	public string shadowInAnim;

	[InspectorShowIf("animateShadow")]
	[InspectorCategory("Visuals")]
	public string shadowOutAnim;

	private MoveState m_state;

	private Vector2 m_startPoint;

	private Vector2 m_targetPoint;

	private float m_moveTime;

	private float m_timer;

	private int m_sequenceIndex;

	private bool m_mirrorPositions;

	private BulletScriptSource m_bulletSource;

	private GoopDoer[] m_goopDoers;

	private AfterImageTrailController m_shadowTrail;

	private tk2dBaseSprite m_shadowSprite;

	private float m_shadowOutTime;

	private MoveState State
	{
		get
		{
			return m_state;
		}
		set
		{
			if (m_state != value)
			{
				EndState(m_state);
				m_state = value;
				BeginState(m_state);
			}
		}
	}

	private bool ShowN()
	{
		return selectType == SelectType.RandomClosestN || selectType == SelectType.RandomFurthestN;
	}

	private bool ShowDisallowNearest()
	{
		return selectType == SelectType.Random || selectType == SelectType.RandomClosestN;
	}

	private bool ShowSubsequentMoveSpeed()
	{
		return selectType == SelectType.InSequence;
	}

	public override void Start()
	{
		base.Start();
		m_shadowTrail = m_aiActor.GetComponent<AfterImageTrailController>();
		if (bulletScript != null && !bulletScript.IsNull)
		{
			tk2dSpriteAnimator spriteAnimator = m_aiAnimator.spriteAnimator;
			spriteAnimator.AnimationEventTriggered = (Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip, int>)Delegate.Combine(spriteAnimator.AnimationEventTriggered, new Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip, int>(AnimationEventTriggered));
		}
	}

	public override BehaviorResult Update()
	{
		BehaviorResult behaviorResult = base.Update();
		if (behaviorResult != 0)
		{
			return behaviorResult;
		}
		if (m_shadowSprite == null)
		{
			m_shadowSprite = m_aiActor.ShadowObject.GetComponent<tk2dBaseSprite>();
		}
		if (!IsReady())
		{
			return BehaviorResult.Continue;
		}
		if (!m_aiActor.TargetRigidbody)
		{
			return BehaviorResult.Continue;
		}
		m_aiActor.ClearPath();
		m_aiActor.BehaviorOverridesVelocity = true;
		m_aiActor.BehaviorVelocity = Vector2.zero;
		m_aiAnimator.LockFacingDirection = true;
		m_aiAnimator.FacingDirection = -90f;
		if (HideGun && (bool)m_aiShooter)
		{
			m_aiShooter.ToggleGunAndHandRenderers(false, "AttackMoveBehavior");
		}
		if (!string.IsNullOrEmpty(preMoveAnimation))
		{
			State = MoveState.PreMove;
		}
		else
		{
			State = MoveState.Move;
		}
		m_updateEveryFrame = true;
		return BehaviorResult.RunContinuous;
	}

	public override ContinuousBehaviorResult ContinuousUpdate()
	{
		base.ContinuousUpdate();
		if (State == MoveState.PreMove)
		{
			if (!m_aiAnimator.IsPlaying(preMoveAnimation))
			{
				State = MoveState.Move;
				return ContinuousBehaviorResult.Continue;
			}
		}
		else if (State == MoveState.Move)
		{
			Vector2 unitCenter = m_aiActor.specRigidbody.UnitCenter;
			if (m_deltaTime <= 0f)
			{
				return ContinuousBehaviorResult.Continue;
			}
			Vector2 vector = ((!SmoothStep) ? Vector2.Lerp(m_startPoint, m_targetPoint, m_timer / m_moveTime) : Vector2Extensions.SmoothStep(m_startPoint, m_targetPoint, m_timer / m_moveTime));
			if (animateShadow && m_moveTime - m_timer <= m_shadowOutTime)
			{
				m_shadowOutTime = -1f;
				m_shadowSprite.spriteAnimator.Play(shadowOutAnim);
			}
			if (m_timer > m_moveTime)
			{
				if (selectType != SelectType.InSequence || m_sequenceIndex >= Positions.Length)
				{
					m_aiActor.BehaviorVelocity = Vector2.zero;
					return ContinuousBehaviorResult.Finished;
				}
				PlanNextMove();
			}
			m_aiActor.BehaviorVelocity = (vector - unitCenter) / m_deltaTime;
			if (updateFacingDirectionDuringMove)
			{
				UpdateFacingDirection(vector - unitCenter);
			}
			m_timer += m_deltaTime;
		}
		return ContinuousBehaviorResult.Continue;
	}

	public override void EndContinuousUpdate()
	{
		base.EndContinuousUpdate();
		State = MoveState.None;
		if (HideGun && (bool)m_aiShooter)
		{
			m_aiShooter.ToggleGunAndHandRenderers(true, "AttackMoveBehavior");
		}
		if (!string.IsNullOrEmpty(preMoveAnimation))
		{
			m_aiAnimator.EndAnimationIf(preMoveAnimation);
		}
		if (!string.IsNullOrEmpty(moveAnimation))
		{
			m_aiAnimator.EndAnimationIf(moveAnimation);
		}
		m_aiAnimator.LockFacingDirection = false;
		m_aiActor.BehaviorOverridesVelocity = false;
		m_updateEveryFrame = false;
		UpdateCooldowns();
	}

	public void AnimationEventTriggered(tk2dSpriteAnimator animator, tk2dSpriteAnimationClip clip, int frame)
	{
		if (m_state != 0 && bulletScript != null && !bulletScript.IsNull && clip.GetFrame(frame).eventInfo == "fire")
		{
			if (!m_bulletSource)
			{
				m_bulletSource = ShootPoint.GetOrAddComponent<BulletScriptSource>();
			}
			m_bulletSource.BulletManager = m_aiActor.bulletBank;
			m_bulletSource.BulletScript = bulletScript;
			m_bulletSource.Initialize();
		}
	}

	private void UpdateTargetPoint()
	{
		if (selectType == SelectType.Random)
		{
			if (DisallowNearest && Positions.Length > 1)
			{
				Vector2 unitCenter = m_aiActor.specRigidbody.GetUnitCenter(ColliderType.HitBox);
				int lastValue = -1;
				float num = -1f;
				for (int i = 0; i < Positions.Length; i++)
				{
					Vector2 position = GetPosition(i);
					float num2 = Vector2.Distance(unitCenter, position);
					if (i == 0 || num2 < num)
					{
						lastValue = i;
						num = num2;
					}
				}
				m_targetPoint = GetPosition(BraveUtility.SequentialRandomRange(0, Positions.Length, lastValue, null, true));
			}
			else
			{
				m_targetPoint = GetPosition(UnityEngine.Random.Range(0, Positions.Length));
			}
		}
		else if (selectType == SelectType.Closest)
		{
			Vector2 unitCenter2 = m_aiActor.specRigidbody.GetUnitCenter(ColliderType.HitBox);
			int i2 = -1;
			float num3 = -1f;
			for (int j = 0; j < Positions.Length; j++)
			{
				Vector2 position2 = GetPosition(j);
				float num4 = Vector2.Distance(unitCenter2, position2);
				if (j == 0 || num4 < num3)
				{
					i2 = j;
					num3 = num4;
				}
			}
			m_targetPoint = GetPosition(i2);
		}
		else if (selectType == SelectType.RandomClosestN)
		{
			Vector2 unitCenter3 = m_aiActor.specRigidbody.GetUnitCenter(ColliderType.HitBox);
			List<Tuple<int, float>> list = new List<Tuple<int, float>>();
			for (int k = 0; k < Positions.Length; k++)
			{
				list.Add(Tuple.Create(k, Vector2.Distance(unitCenter3, GetPosition(k))));
			}
			list = new List<Tuple<int, float>>(list.OrderBy((Tuple<int, float> t) => t.Second));
			if (DisallowNearest)
			{
				list.RemoveAt(0);
			}
			m_targetPoint = GetPosition(list[UnityEngine.Random.Range(0, Mathf.Min(N + 1, list.Count))].First);
		}
		else if (selectType == SelectType.Furthest)
		{
			Vector2 unitCenter4 = m_aiActor.specRigidbody.GetUnitCenter(ColliderType.HitBox);
			int i3 = -1;
			float num5 = float.MaxValue;
			for (int l = 0; l < Positions.Length; l++)
			{
				Vector2 position3 = GetPosition(l);
				float num6 = Vector2.Distance(unitCenter4, position3);
				if (l == 0 || num6 > num5)
				{
					i3 = l;
					num5 = num6;
				}
			}
			m_targetPoint = GetPosition(i3);
		}
		else if (selectType == SelectType.RandomFurthestN)
		{
			Vector2 unitCenter5 = m_aiActor.specRigidbody.GetUnitCenter(ColliderType.HitBox);
			List<Tuple<int, float>> list2 = new List<Tuple<int, float>>();
			for (int m = 0; m < Positions.Length; m++)
			{
				list2.Add(Tuple.Create(m, Vector2.Distance(unitCenter5, GetPosition(m))));
			}
			list2 = new List<Tuple<int, float>>(list2.OrderByDescending((Tuple<int, float> t) => t.Second));
			m_targetPoint = GetPosition(list2[UnityEngine.Random.Range(0, Mathf.Min(N + 1, list2.Count))].First);
		}
		else if (selectType == SelectType.InSequence)
		{
			m_targetPoint = GetPosition(m_sequenceIndex++);
		}
		else
		{
			Debug.LogError("Unknown select type: " + selectType);
		}
	}

	private Vector2 GetPosition(int i, bool? mirror = null)
	{
		if (!mirror.HasValue)
		{
			mirror = m_mirrorPositions;
		}
		if (positionType == PositionType.RelativeToRoomCenter || positionType == PositionType.RelativeToHelicopterCenter)
		{
			Vector2 center = m_aiActor.ParentRoom.area.Center;
			if (positionType == PositionType.RelativeToHelicopterCenter)
			{
				float num = 0f;
				for (int j = 0; j < GameManager.Instance.AllPlayers.Length; j++)
				{
					PlayerController playerController = GameManager.Instance.AllPlayers[j];
					if (playerController.healthHaver.IsAlive)
					{
						num = Mathf.Max(num, playerController.specRigidbody.UnitCenter.y);
					}
				}
				if (num > 0f)
				{
					center.y = num;
				}
			}
			if (mirror.Value)
			{
				return center + Vector2.Scale(Positions[i], new Vector2(-1f, 1f));
			}
			return center + Positions[i];
		}
		Debug.LogError("Unknown position type: " + positionType);
		return Vector2.zero;
	}

	private void UpdateFacingDirection(Vector2 toTarget)
	{
		if (!(toTarget == Vector2.zero))
		{
			toTarget.Normalize();
			if (biasFacingRoomCenter)
			{
				Vector2 vector = m_aiActor.ParentRoom.area.Center - m_aiActor.specRigidbody.UnitCenter;
				toTarget = (toTarget + 0.2f * vector).normalized;
			}
			if (faceBottomCenter)
			{
				Vector2 vector2 = new Vector2(m_aiActor.ParentRoom.area.UnitCenter.x, m_aiActor.specRigidbody.UnitCenter.y - 15f);
				toTarget = (vector2 - m_aiActor.specRigidbody.UnitCenter).normalized;
			}
			m_aiAnimator.FacingDirection = toTarget.ToAngle();
		}
	}

	private void BeginState(MoveState state)
	{
		switch (state)
		{
		case MoveState.PreMove:
			m_aiActor.ClearPath();
			m_aiActor.BehaviorOverridesVelocity = true;
			m_aiActor.BehaviorVelocity = Vector2.zero;
			m_aiAnimator.PlayUntilCancelled(preMoveAnimation);
			break;
		case MoveState.Move:
			m_sequenceIndex = 0;
			if (MirrorIfCloser)
			{
				Vector2 position = GetPosition(0, false);
				Vector2 position2 = GetPosition(0, true);
				Vector2 unitCenter = m_aiActor.specRigidbody.GetUnitCenter(ColliderType.HitBox);
				m_mirrorPositions = Vector2.Distance(position2, unitCenter) < Vector2.Distance(position, unitCenter);
			}
			PlanNextMove();
			m_aiAnimator.LockFacingDirection = true;
			m_aiAnimator.PlayUntilCancelled(moveAnimation);
			if (DisableCollisionDuringMove)
			{
				m_aiActor.specRigidbody.CollideWithOthers = false;
				m_aiActor.IsGone = true;
			}
			if (disableGoops)
			{
				if (m_goopDoers == null)
				{
					m_goopDoers = m_aiActor.GetComponents<GoopDoer>();
				}
				for (int i = 0; i < m_goopDoers.Length; i++)
				{
					m_goopDoers[i].enabled = false;
				}
			}
			if (enableShadowTrail)
			{
				m_shadowTrail.spawnShadows = true;
			}
			if (animateShadow)
			{
				m_shadowSprite.spriteAnimator.Play(shadowInAnim);
				m_shadowOutTime = m_shadowSprite.spriteAnimator.GetClipByName(shadowOutAnim).BaseClipLength;
			}
			break;
		}
	}

	private void PlanNextMove()
	{
		m_startPoint = m_aiActor.specRigidbody.UnitCenter;
		UpdateTargetPoint();
		Vector2 toTarget = m_targetPoint - m_startPoint;
		float magnitude = toTarget.magnitude;
		if (selectType == SelectType.InSequence && m_sequenceIndex > 1 && SubsequentMoveSpeed > 0f)
		{
			m_moveTime = magnitude / SubsequentMoveSpeed;
		}
		else
		{
			m_moveTime = MoveTime;
			if (MinSpeed > 0f)
			{
				m_moveTime = Mathf.Min(m_moveTime, magnitude / MinSpeed);
			}
			if (MaxSpeed > 0f)
			{
				m_moveTime = Mathf.Max(m_moveTime, magnitude / MaxSpeed);
			}
		}
		UpdateFacingDirection(toTarget);
		m_timer = 0f;
	}

	private void EndState(MoveState state)
	{
		if (state != MoveState.Move)
		{
			return;
		}
		if (DisableCollisionDuringMove)
		{
			m_aiActor.specRigidbody.CollideWithOthers = true;
			m_aiActor.IsGone = false;
		}
		if (disableGoops)
		{
			for (int i = 0; i < m_goopDoers.Length; i++)
			{
				m_goopDoers[i].enabled = true;
			}
		}
		if (enableShadowTrail)
		{
			m_shadowTrail.spawnShadows = false;
		}
	}
}
