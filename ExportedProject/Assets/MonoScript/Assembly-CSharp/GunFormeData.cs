using System;

[Serializable]
public class GunFormeData
{
	public bool RequiresSynergy = true;

	[LongNumericEnum]
	[ShowInInspectorIf("RequiresSynergy", false)]
	public CustomSynergyType RequiredSynergy;

	[PickupIdentifier]
	public int FormeID;

	public bool IsValid(PlayerController p)
	{
		if (!RequiresSynergy)
		{
			return true;
		}
		return p.HasActiveBonusSynergy(RequiredSynergy);
	}
}
