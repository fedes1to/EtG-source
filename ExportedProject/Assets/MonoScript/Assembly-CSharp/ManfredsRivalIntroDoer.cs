using System.Collections.Generic;
using Dungeonator;
using UnityEngine;

[RequireComponent(typeof(GenericIntroDoer))]
public class ManfredsRivalIntroDoer : SpecificIntroDoer
{
	private List<AIActor> m_knights = new List<AIActor>();

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}

	public override void StartIntro(List<tk2dSpriteAnimator> animators)
	{
		List<AIActor> activeEnemies = base.aiActor.ParentRoom.GetActiveEnemies(RoomHandler.ActiveEnemyType.All);
		for (int i = 0; i < activeEnemies.Count; i++)
		{
			if (!(activeEnemies[i] == base.aiActor))
			{
				animators.Add(activeEnemies[i].spriteAnimator);
				activeEnemies[i].aiAnimator.LockFacingDirection = true;
				activeEnemies[i].aiAnimator.FacingDirection = -90f;
				m_knights.Add(activeEnemies[i]);
			}
		}
	}

	public override void OnCleanup()
	{
		for (int i = 0; i < m_knights.Count; i++)
		{
			m_knights[i].aiAnimator.LockFacingDirection = true;
		}
	}
}
