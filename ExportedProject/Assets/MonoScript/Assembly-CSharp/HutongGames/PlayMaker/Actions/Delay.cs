namespace HutongGames.PlayMaker.Actions
{
	[Tooltip("Delays for a specified amount of time.")]
	[ActionCategory(".Brave")]
	public class Delay : FsmStateAction
	{
		[Tooltip("How many seconds to delay for (this action will not finish until the time has passed).")]
		public FsmFloat time;

		private bool firstFrame;

		private float timer;

		public override void OnEnter()
		{
			timer = 0f;
			firstFrame = true;
			if (time.Value <= 0f)
			{
				Finish();
			}
		}

		public override void OnUpdate()
		{
			if (firstFrame)
			{
				firstFrame = false;
				return;
			}
			timer += BraveTime.DeltaTime;
			if (timer >= time.Value)
			{
				Finish();
			}
		}
	}
}
