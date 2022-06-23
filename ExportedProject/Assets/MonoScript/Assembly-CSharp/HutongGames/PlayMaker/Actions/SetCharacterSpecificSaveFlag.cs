namespace HutongGames.PlayMaker.Actions
{
	[Tooltip("Sets a flag on the player's save data.")]
	[ActionCategory(".Brave")]
	public class SetCharacterSpecificSaveFlag : FsmStateAction
	{
		[Tooltip("The flag.")]
		public CharacterSpecificGungeonFlags targetFlag;

		[Tooltip("The value to set the flag to.")]
		public FsmBool value;

		public override string ErrorCheck()
		{
			string text = string.Empty;
			if (targetFlag == CharacterSpecificGungeonFlags.NONE)
			{
				text += "Target flag is NONE. This is a mistake.";
			}
			return text;
		}

		public override void Reset()
		{
			targetFlag = CharacterSpecificGungeonFlags.NONE;
			value = false;
		}

		public override void OnEnter()
		{
			GameStatsManager.Instance.SetCharacterSpecificFlag(targetFlag, value.Value);
			Finish();
		}
	}
}
