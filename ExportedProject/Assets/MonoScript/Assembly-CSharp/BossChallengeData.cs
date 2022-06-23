using System;

[Serializable]
public class BossChallengeData
{
	public string Annotation;

	[EnemyIdentifier]
	public string[] BossGuids;

	public int NumToSelect;

	public ChallengeModifier[] Modifiers;
}
