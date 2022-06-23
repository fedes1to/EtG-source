public class DynamiteGuyController : BraveBehaviour
{
	public SimpleSparksDoer SparksDoer;

	public void Update()
	{
		if (base.aiActor.HasBeenAwoken && !base.aiAnimator.IsPlaying("spawn"))
		{
			SparksDoer.enabled = true;
			base.enabled = false;
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}
}
