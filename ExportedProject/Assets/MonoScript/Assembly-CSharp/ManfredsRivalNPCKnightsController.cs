using System.Collections.Generic;
using Dungeonator;

public class ManfredsRivalNPCKnightsController : BraveBehaviour
{
	private List<AIActor> m_knights = new List<AIActor>();

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}

	public void ManfredKnightsSpawned()
	{
		List<AIActor> activeEnemies = base.talkDoer.ParentRoom.GetActiveEnemies(RoomHandler.ActiveEnemyType.All);
		for (int i = 0; i < activeEnemies.Count; i++)
		{
			if (!(activeEnemies[i] == base.aiActor))
			{
				activeEnemies[i].behaviorSpeculator.enabled = false;
				m_knights.Add(activeEnemies[i]);
			}
		}
	}
}
