namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory(".Brave")]
	[Tooltip("Sets a flag on the player's save data.")]
	public class SetSaveFlag : FsmStateAction
	{
		[Tooltip("The flag.")]
		public GungeonFlags targetFlag;

		[Tooltip("The value to set the flag to.")]
		public FsmBool value;

		public override string ErrorCheck()
		{
			string text = string.Empty;
			if (targetFlag == GungeonFlags.NONE)
			{
				text += "Target flag is NONE. This is a mistake.";
			}
			return text;
		}

		public override void Reset()
		{
			targetFlag = GungeonFlags.NONE;
			value = false;
		}

		public override void OnEnter()
		{
			GameStatsManager.Instance.SetFlag(targetFlag, value.Value);
			Finish();
		}
	}
}
