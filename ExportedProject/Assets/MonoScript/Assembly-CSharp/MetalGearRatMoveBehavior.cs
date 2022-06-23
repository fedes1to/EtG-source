using FullInspector;
using UnityEngine;

[InspectorDropdownName("Bosses/MetalGearRat/MoveBehavior")]
public class MetalGearRatMoveBehavior : BasicAttackBehavior
{
	private enum State
	{
		Idle,
		Moving,
		Done
	}

	public float HorizontalMovePixels = 5f;

	private State m_state;

	private Vector2 m_moveDirection;

	private GameObject m_shadow;

	private Vector2 m_shadowStartingPos;

	private GameObject m_cameraPoint;

	private Vector2 m_cameraStartingPos;

	public override void Start()
	{
		m_moveDirection = Vector2.right;
	}

	public override bool IsOverridable()
	{
		return m_state == State.Idle;
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
		m_moveDirection = UpdateMoveDirection();
		m_shadow = m_aiActor.ShadowObject;
		m_shadowStartingPos = m_shadow.transform.localPosition;
		m_cameraPoint = m_aiActor.gameObject.transform.Find("camera point").gameObject;
		m_cameraStartingPos = m_cameraPoint.transform.localPosition;
		if (m_moveDirection.x < 0f)
		{
			m_aiAnimator.PlayUntilFinished("move_left");
		}
		else
		{
			m_aiAnimator.PlayUntilFinished("move_right");
		}
		m_updateEveryFrame = true;
		m_state = State.Moving;
		return BehaviorResult.RunContinuous;
	}

	public override ContinuousBehaviorResult ContinuousUpdate()
	{
		base.ContinuousUpdate();
		if (m_state == State.Moving)
		{
			Vector2 vector = Vector2.Lerp(Vector2.zero, m_moveDirection * (HorizontalMovePixels / 16f), m_aiAnimator.CurrentClipProgress * 2.2f);
			m_shadow.transform.localPosition = m_shadowStartingPos + vector;
			m_cameraPoint.transform.localPosition = m_cameraStartingPos + vector;
			if (!m_aiAnimator.IsPlaying("move_left") && !m_aiAnimator.IsPlaying("move_right"))
			{
				m_aiActor.transform.position += (Vector3)(m_moveDirection * (HorizontalMovePixels / 16f));
				m_shadow.transform.localPosition = m_shadowStartingPos;
				m_cameraPoint.transform.localPosition = m_cameraStartingPos;
				m_aiActor.specRigidbody.Reinitialize();
				m_state = State.Done;
				return ContinuousBehaviorResult.Finished;
			}
		}
		else if (m_state == State.Done)
		{
			return ContinuousBehaviorResult.Finished;
		}
		return ContinuousBehaviorResult.Continue;
	}

	public override void EndContinuousUpdate()
	{
		base.EndContinuousUpdate();
		m_state = State.Idle;
		m_aiAnimator.EndAnimationIf("move_left");
		m_shadow.transform.localPosition = m_shadowStartingPos;
		m_cameraPoint.transform.localPosition = m_cameraStartingPos;
		m_updateEveryFrame = false;
		UpdateCooldowns();
	}

	private Vector2 UpdateMoveDirection()
	{
		Vector2 unitCenter = m_aiActor.ParentRoom.area.UnitCenter;
		Vector2 unitCenter2 = m_aiActor.specRigidbody.GetUnitCenter(ColliderType.HitBox);
		if (unitCenter2.x < unitCenter.x - 7f)
		{
			return Vector2.right;
		}
		if (unitCenter2.x > unitCenter.x + 7f)
		{
			return Vector2.left;
		}
		if ((bool)m_aiActor.TargetRigidbody)
		{
			float num = m_aiActor.TargetRigidbody.GetUnitCenter(ColliderType.HitBox).x - unitCenter2.x;
			if (num > 0f)
			{
				return Vector2.right;
			}
			if (num < 0f)
			{
				return Vector2.left;
			}
		}
		return (!BraveUtility.RandomBool()) ? Vector2.right : Vector2.left;
	}
}
