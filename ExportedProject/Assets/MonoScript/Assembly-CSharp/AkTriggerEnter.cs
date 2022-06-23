using UnityEngine;

public class AkTriggerEnter : AkTriggerBase
{
	public GameObject triggerObject;

	private void OnTriggerEnter(Collider in_other)
	{
		if (triggerDelegate != null && (triggerObject == null || triggerObject == in_other.gameObject))
		{
			triggerDelegate(in_other.gameObject);
		}
	}
}
