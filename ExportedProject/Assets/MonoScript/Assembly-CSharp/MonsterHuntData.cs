using System.Collections.Generic;
using UnityEngine;

public class MonsterHuntData : ScriptableObject
{
	[SerializeField]
	public List<MonsterHuntQuest> OrderedQuests;

	[SerializeField]
	public List<MonsterHuntQuest> ProceduralQuests;
}
