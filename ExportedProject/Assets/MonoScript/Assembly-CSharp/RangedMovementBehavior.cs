using FullInspector;

public abstract class RangedMovementBehavior : MovementBehaviorBase
{
	public bool SpecifyRange;

	[InspectorShowIf("SpecifyRange")]
	public float MinActiveRange;

	[InspectorShowIf("SpecifyRange")]
	public float MaxActiveRange;

	protected bool InRange()
	{
		if (!SpecifyRange)
		{
			return true;
		}
		if (!m_aiActor.TargetRigidbody)
		{
			return false;
		}
		float distanceToTarget = m_aiActor.DistanceToTarget;
		return distanceToTarget >= MinActiveRange && distanceToTarget <= MaxActiveRange;
	}
}
