namespace HutongGames.PlayMaker.Actions
{
	[Tooltip("Toss the current gun into the gunper monper and (hopefully) get an upgrade.")]
	[ActionCategory(".NPCs")]
	public class MunchCurrentGun : FsmStateAction
	{
		public FsmEvent rewardGivenEvent;

		public FsmEvent noRewardEvent;

		private GunberMuncherController m_muncher;

		public override void OnEnter()
		{
			TalkDoerLite component = base.Owner.GetComponent<TalkDoerLite>();
			m_muncher = component.GetComponent<GunberMuncherController>();
			m_muncher.TossPlayerEquippedGun(component.TalkingPlayer);
		}

		public override void OnUpdate()
		{
			if (!m_muncher.IsProcessing)
			{
				if (m_muncher.ShouldGiveReward)
				{
					base.Fsm.Event(rewardGivenEvent);
				}
				else
				{
					base.Fsm.Event(noRewardEvent);
				}
			}
		}
	}
}
