namespace HutongGames.PlayMaker.Actions
{
	[Tooltip("Sends Events based on the data in a player save.")]
	[ActionCategory(".Brave")]
	public class ChangeSaveStat : FsmStateAction
	{
		public TrackedStats stat;

		public FsmFloat statChange;

		public override void Reset()
		{
			stat = TrackedStats.BULLETS_FIRED;
			statChange = 0f;
		}

		public override string ErrorCheck()
		{
			return string.Empty;
		}

		public override void OnEnter()
		{
			GameStatsManager.Instance.RegisterStatChange(stat, statChange.Value);
			Finish();
		}
	}
}
