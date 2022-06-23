using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	[Tooltip("Makes this NPC become an enemy.")]
	[ActionCategory(".NPCs")]
	public class BecomeHostile : FsmStateAction
	{
		[Tooltip("The enemy prefab to spawn.")]
		public FsmString enemyGuid;

		[Tooltip("Optionally, a different TalkDoerLite to become hostile. Used for controlling groups.")]
		public TalkDoerLite alternativeTarget;

		public override void Reset()
		{
			enemyGuid = null;
		}

		public override void OnEnter()
		{
			TalkDoerLite component = base.Owner.GetComponent<TalkDoerLite>();
			if (alternativeTarget != null)
			{
				component = alternativeTarget;
			}
			SetNpcVisibility.SetVisible(component, false);
			AIActor orLoadByGuid = EnemyDatabase.GetOrLoadByGuid(enemyGuid.Value);
			AIActor aIActor = AIActor.Spawn(orLoadByGuid, component.specRigidbody.UnitCenter.ToIntVector2(VectorConversions.Floor), component.ParentRoom);
			aIActor.specRigidbody.Initialize();
			aIActor.transform.position += (Vector3)(component.specRigidbody.UnitBottomLeft - aIActor.specRigidbody.UnitBottomLeft);
			aIActor.specRigidbody.Reinitialize();
			aIActor.HasBeenEngaged = true;
			if (alternativeTarget == null)
			{
				GenericIntroDoer component2 = aIActor.GetComponent<GenericIntroDoer>();
				if ((bool)component2)
				{
					component2.TriggerSequence(component.TalkingPlayer);
				}
			}
			component.HostileObject = aIActor;
			Finish();
		}
	}
}
