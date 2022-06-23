using System.Collections.Generic;
using System.Collections.ObjectModel;
using Dungeonator;

namespace HutongGames.PlayMaker.Actions
{
	[Tooltip("Controls the wall guns in the tutorial.")]
	[ActionCategory(".NPCs")]
	public class TutorialGuns : FsmStateAction
	{
		public FsmBool enable;

		public FsmBool disable;

		public override void OnEnter()
		{
			TalkDoerLite component = base.Owner.GetComponent<TalkDoerLite>();
			List<AIActor> activeEnemies = component.ParentRoom.GetActiveEnemies(RoomHandler.ActiveEnemyType.All);
			for (int i = 0; i < activeEnemies.Count; i++)
			{
				AIActor aIActor = activeEnemies[i];
				if (enable.Value)
				{
					aIActor.enabled = true;
					aIActor.specRigidbody.enabled = true;
					aIActor.State = AIActor.ActorState.Normal;
				}
				if (disable.Value)
				{
					aIActor.enabled = false;
					aIActor.aiAnimator.PlayUntilCancelled("deactivate");
					aIActor.specRigidbody.enabled = false;
				}
			}
			if (disable.Value)
			{
				ReadOnlyCollection<Projectile> allProjectiles = StaticReferenceManager.AllProjectiles;
				for (int num = allProjectiles.Count - 1; num >= 0; num--)
				{
					if ((bool)allProjectiles[num])
					{
						allProjectiles[num].DieInAir();
					}
				}
			}
			Finish();
		}
	}
}
