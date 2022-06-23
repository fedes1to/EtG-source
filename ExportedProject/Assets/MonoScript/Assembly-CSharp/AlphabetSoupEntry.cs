using System;

[Serializable]
public class AlphabetSoupEntry
{
	[LongNumericEnum]
	public CustomSynergyType RequiredSynergy;

	public string[] Words;

	public string[] AudioEvents;

	public Projectile BaseProjectile;
}
