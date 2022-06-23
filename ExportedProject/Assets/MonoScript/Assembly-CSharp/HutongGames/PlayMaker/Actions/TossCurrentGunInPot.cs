namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory(".NPCs")]
	[Tooltip("Toss the current gun into the witches pot and (hopefully) get an upgrade.")]
	public class TossCurrentGunInPot : FsmStateAction
	{
		public FsmEvent SuccessEvent;

		private WitchCauldronController m_cauldron;

		public override void OnEnter()
		{
			TalkDoerLite component = base.Owner.GetComponent<TalkDoerLite>();
			m_cauldron = component.transform.parent.GetComponent<WitchCauldronController>();
			if (m_cauldron.TossPlayerEquippedGun(component.TalkingPlayer))
			{
				base.Fsm.Event(SuccessEvent);
			}
			Finish();
		}

		public override void OnUpdate()
		{
			if (!m_cauldron.IsGunInPot)
			{
				Finish();
			}
		}
	}
}
