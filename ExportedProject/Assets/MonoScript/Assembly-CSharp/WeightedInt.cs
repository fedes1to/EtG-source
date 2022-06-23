using System;

[Serializable]
public class WeightedInt
{
	public string annotation;

	public int value;

	public float weight;

	public DungeonPrerequisite[] additionalPrerequisites;
}
