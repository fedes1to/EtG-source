using UnityEngine;

public class ResourcefulRatChamberBehavior : OverrideBehaviorBase
{
	public float HealthThresholdPhaseTwo = 0.66f;

	public float HealthThresholdPhaseThree = 0.33f;

	private bool m_isActive;

	private int m_currentPhase = 1;

	public override void Start()
	{
		base.Start();
		m_updateEveryFrame = true;
		m_ignoreGlobalCooldown = true;
	}

	public override void Upkeep()
	{
		base.Upkeep();
	}

	private bool ReadyForNextPhase()
	{
		if (m_currentPhase == 1 && m_aiActor.healthHaver.GetCurrentHealthPercentage() < HealthThresholdPhaseTwo)
		{
			return true;
		}
		if (m_currentPhase == 2 && m_aiActor.healthHaver.GetCurrentHealthPercentage() < HealthThresholdPhaseThree)
		{
			return true;
		}
		return false;
	}

	public override bool OverrideOtherBehaviors()
	{
		return ReadyForNextPhase() || m_isActive;
	}

	public override BehaviorResult Update()
	{
		base.Update();
		if (ReadyForNextPhase())
		{
			m_currentPhase++;
			m_aiActor.MovementModifiers += m_aiActor_MovementModifiers;
			m_aiActor.BehaviorOverridesVelocity = false;
			m_aiAnimator.LockFacingDirection = false;
			m_aiActor.healthHaver.IsVulnerable = false;
			m_aiActor.specRigidbody.CollideWithOthers = false;
			IntVector2 basePosition = m_aiActor.ParentRoom.area.basePosition;
			Vector2 vector = basePosition.ToVector2() + m_aiActor.ParentRoom.area.dimensions.ToVector2().WithY(0f) / 2f;
			Vector2 vector2 = new Vector2(0f, 35f);
			if (m_currentPhase == 3)
			{
				vector2 = new Vector2(0f, 52f);
			}
			m_aiActor.PathfindToPosition(vector + vector2);
			m_isActive = true;
			return BehaviorResult.RunContinuous;
		}
		return BehaviorResult.Continue;
	}

	private void m_aiActor_MovementModifiers(ref Vector2 volundaryVel, ref Vector2 involuntaryVel)
	{
		volundaryVel *= 4f;
	}

	public override bool IsOverridable()
	{
		return false;
	}

	public override ContinuousBehaviorResult ContinuousUpdate()
	{
		base.ContinuousUpdate();
		if (m_aiActor.PathComplete)
		{
			m_aiActor.MovementModifiers -= m_aiActor_MovementModifiers;
			m_aiActor.healthHaver.IsVulnerable = true;
			m_aiActor.specRigidbody.CollideWithOthers = true;
			m_isActive = false;
			return ContinuousBehaviorResult.Finished;
		}
		return ContinuousBehaviorResult.Continue;
	}

	public override void Destroy()
	{
		base.Destroy();
	}
}
