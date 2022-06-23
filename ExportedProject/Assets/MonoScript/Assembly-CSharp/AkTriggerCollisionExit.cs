using UnityEngine;

public class AkTriggerCollisionExit : AkTriggerBase
{
	public GameObject triggerObject;

	private void OnCollisionExit(Collision in_other)
	{
		if (triggerDelegate != null && (triggerObject == null || triggerObject == in_other.gameObject))
		{
			triggerDelegate(in_other.gameObject);
		}
	}
}
