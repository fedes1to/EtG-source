public class MimicController : BraveBehaviour
{
	public void Awake()
	{
		base.aiActor.enabled = false;
		base.aiShooter.enabled = false;
		base.behaviorSpeculator.enabled = false;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}
}
