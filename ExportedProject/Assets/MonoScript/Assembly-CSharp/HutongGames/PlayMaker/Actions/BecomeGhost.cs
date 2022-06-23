namespace HutongGames.PlayMaker.Actions
{
	[Tooltip("Makes the NPC a ghost.")]
	[ActionCategory(".NPCs")]
	public class BecomeGhost : FsmStateAction
	{
		public override void Reset()
		{
		}

		public override string ErrorCheck()
		{
			return string.Empty;
		}

		public override void OnEnter()
		{
			if ((bool)base.Owner && (bool)base.Owner.GetComponent<TalkDoerLite>())
			{
				base.Owner.GetComponent<TalkDoerLite>().ConvertToGhost();
			}
			Finish();
		}
	}
}
