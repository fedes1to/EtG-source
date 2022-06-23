using System.Collections.Generic;

public class DestroyOverlappingBreakables : BraveBehaviour
{
	public bool everyFrame;

	public void Update()
	{
		List<SpeculativeRigidbody> overlappingRigidbodies = PhysicsEngine.Instance.GetOverlappingRigidbodies(base.specRigidbody);
		for (int i = 0; i < overlappingRigidbodies.Count; i++)
		{
			SpeculativeRigidbody speculativeRigidbody = overlappingRigidbodies[i];
			if ((bool)speculativeRigidbody && (bool)speculativeRigidbody.minorBreakable)
			{
				speculativeRigidbody.minorBreakable.Break(speculativeRigidbody.UnitCenter - base.specRigidbody.UnitCenter);
			}
		}
		if (!everyFrame)
		{
			base.enabled = false;
		}
	}
}
