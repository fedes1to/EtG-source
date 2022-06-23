namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory(".Brave")]
	public class LaunchTimedEvent : FsmStateAction
	{
		public GungeonFlags targetFlag;

		public float AllotedTime = 60f;

		public override void Reset()
		{
		}

		public override void OnEnter()
		{
			GameManager.Instance.LaunchTimedEvent(AllotedTime, delegate(bool a)
			{
				GameStatsManager.Instance.SetFlag(targetFlag, a);
			});
			Finish();
		}
	}
}
