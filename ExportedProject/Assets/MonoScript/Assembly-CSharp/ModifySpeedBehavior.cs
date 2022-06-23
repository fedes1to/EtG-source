using UnityEngine;

public class ModifySpeedBehavior : OverrideBehaviorBase
{
	public float minSpeed;

	public float minSpeedDistance;

	public float maxSpeed;

	public float maxSpeedDistance;

	public override BehaviorResult Update()
	{
		if ((bool)m_aiActor.TargetRigidbody)
		{
			float distanceToTarget = m_aiActor.DistanceToTarget;
			float t = Mathf.InverseLerp(minSpeedDistance, maxSpeedDistance, distanceToTarget);
			m_aiActor.MovementSpeed = TurboModeController.MaybeModifyEnemyMovementSpeed(Mathf.Lerp(minSpeed, maxSpeed, t));
			if (m_aiActor.IsBlackPhantom)
			{
				m_aiActor.MovementSpeed *= m_aiActor.BlackPhantomProperties.MovementSpeedMultiplier;
			}
		}
		return BehaviorResult.Continue;
	}
}
