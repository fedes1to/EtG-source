using System;

[Serializable]
public class CompanionTransformSynergy
{
	[LongNumericEnum]
	public CustomSynergyType RequiredSynergy;

	[EnemyIdentifier]
	public string SynergyCompanionGuid;
}
