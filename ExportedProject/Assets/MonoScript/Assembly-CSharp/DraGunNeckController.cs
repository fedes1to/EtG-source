public class DraGunNeckController : BraveBehaviour
{
	public void Start()
	{
		base.aiActor = base.transform.parent.GetComponent<AIActor>();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}

	public void TriggerAnimationEvent(string eventInfo)
	{
		base.aiActor.behaviorSpeculator.TriggerAnimationEvent(eventInfo);
	}
}
