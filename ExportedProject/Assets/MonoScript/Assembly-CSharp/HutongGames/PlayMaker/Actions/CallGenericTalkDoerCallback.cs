namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory(".NPCs")]
	public class CallGenericTalkDoerCallback : FsmStateAction
	{
		public FsmBool CallCallbackA;

		public FsmBool CallCallbackB;

		public FsmBool CallCallbackC;

		public FsmBool CallCallbackD;

		[Tooltip("Repeat every frame while the state is active.")]
		public bool everyFrame;

		public override void Reset()
		{
			everyFrame = false;
		}

		private void DoCallbacks()
		{
			TalkDoerLite component = base.Owner.GetComponent<TalkDoerLite>();
			if (CallCallbackA.Value && component.OnGenericFSMActionA != null)
			{
				component.OnGenericFSMActionA();
			}
			if (CallCallbackB.Value && component.OnGenericFSMActionB != null)
			{
				component.OnGenericFSMActionB();
			}
			if (CallCallbackC.Value && component.OnGenericFSMActionC != null)
			{
				component.OnGenericFSMActionC();
			}
			if (CallCallbackD.Value && component.OnGenericFSMActionD != null)
			{
				component.OnGenericFSMActionD();
			}
		}

		public override void OnEnter()
		{
			if (!everyFrame)
			{
				DoCallbacks();
				Finish();
			}
		}

		public override void OnUpdate()
		{
			DoCallbacks();
		}
	}
}
