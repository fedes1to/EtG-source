using UnityEngine;

public class AIActorDummy : AIActor
{
	public bool isInBossTab;

	public GameObject realPrefab;

	public override bool InBossAmmonomiconTab
	{
		get
		{
			return isInBossTab;
		}
	}
}
