using System;

[Serializable]
public class WeightedRoom
{
	public PrototypeDungeonRoom room;

	public float weight;

	public bool limitedCopies;

	public int maxCopies = 1;

	public DungeonPrerequisite[] additionalPrerequisites;

	public bool CheckPrerequisites()
	{
		if (additionalPrerequisites == null || additionalPrerequisites.Length == 0)
		{
			return true;
		}
		for (int i = 0; i < additionalPrerequisites.Length; i++)
		{
			if (!additionalPrerequisites[i].CheckConditionsFulfilled())
			{
				return false;
			}
		}
		return true;
	}
}
