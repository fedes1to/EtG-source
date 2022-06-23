using Steamworks;

namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory(ActionCategory.Logic)]
	public class SteamIdentificationSwitch : FsmStateAction
	{
		[CompoundArray("Int Switches", "Compare Int", "Send Event")]
		public FsmString[] targetIDs;

		public FsmEvent[] sendEvent;

		public bool everyFrame;

		public FsmEvent defaultEvent;

		public override void Reset()
		{
			targetIDs = new FsmString[1];
			sendEvent = new FsmEvent[1];
			everyFrame = false;
		}

		public override void OnEnter()
		{
			DoIDSwitch();
			if (!everyFrame)
			{
				Finish();
			}
		}

		public override void OnUpdate()
		{
			DoIDSwitch();
		}

		private void DoIDSwitch()
		{
			bool flag = false;
			ulong num = 0uL;
			if (GameManager.Instance.platformInterface is PlatformInterfaceSteam && SteamManager.Initialized)
			{
				num = SteamUser.GetSteamID().m_SteamID;
				flag = true;
			}
			if (flag)
			{
				for (int i = 0; i < targetIDs.Length; i++)
				{
					if (targetIDs[i].Value == num.ToString())
					{
						base.Fsm.Event(sendEvent[i]);
						return;
					}
				}
			}
			base.Fsm.Event(defaultEvent);
		}
	}
}
