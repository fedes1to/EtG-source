using UnityEngine;

public class GundromedaStrain : PassiveItem
{
	public float percentageHealthReduction = 0.1f;

	public override void Pickup(PlayerController player)
	{
		if (!m_pickedUp)
		{
			base.Pickup(player);
			AIActor.HealthModifier *= Mathf.Clamp01(1f - percentageHealthReduction);
		}
	}

	public override DebrisObject Drop(PlayerController player)
	{
		DebrisObject debrisObject = base.Drop(player);
		debrisObject.GetComponent<GundromedaStrain>().m_pickedUpThisRun = true;
		AIActor.HealthModifier /= Mathf.Clamp01(1f - percentageHealthReduction);
		return debrisObject;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}
}
