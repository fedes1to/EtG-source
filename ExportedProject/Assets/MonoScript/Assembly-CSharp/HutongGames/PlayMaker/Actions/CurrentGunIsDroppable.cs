namespace HutongGames.PlayMaker.Actions
{
	[Tooltip("Toss the current gun into the gunper monper and (hopefully) get an upgrade.")]
	[ActionCategory(".NPCs")]
	public class CurrentGunIsDroppable : FsmStateAction
	{
		[Tooltip("Event to send if the player is in the foyer.")]
		public FsmEvent isTrue;

		[Tooltip("Event to send if the player is not in the foyer.")]
		public FsmEvent isFalse;

		[Tooltip("Repeat every frame while the state is active.")]
		public bool everyFrame;

		public override void Reset()
		{
			isTrue = null;
			isFalse = null;
			everyFrame = false;
		}

		private bool TestGun()
		{
			TalkDoerLite component = base.Owner.GetComponent<TalkDoerLite>();
			bool result = false;
			if ((bool)component && (bool)component.TalkingPlayer)
			{
				if (component.TalkingPlayer.CharacterUsesRandomGuns)
				{
					return false;
				}
				Gun currentGun = component.TalkingPlayer.CurrentGun;
				if ((bool)currentGun && currentGun.CanActuallyBeDropped(component.TalkingPlayer) && !currentGun.InfiniteAmmo)
				{
					result = true;
				}
			}
			return result;
		}

		public override void OnEnter()
		{
			base.Fsm.Event((!TestGun()) ? isFalse : isTrue);
			if (!everyFrame)
			{
				Finish();
			}
		}

		public override void OnUpdate()
		{
			base.Fsm.Event((!TestGun()) ? isFalse : isTrue);
		}
	}
}
