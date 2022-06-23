using UnityEngine;

public class AkTriggerCollisionEnter : AkTriggerBase
{
	public GameObject triggerObject;

	private void OnCollisionEnter(Collision in_other)
	{
		if (triggerDelegate != null && (triggerObject == null || triggerObject == in_other.gameObject))
		{
			triggerDelegate(in_other.gameObject);
		}
	}

	private void OnTriggerEnter(Collider in_other)
	{
		if (triggerDelegate != null && (triggerObject == null || triggerObject == in_other.gameObject))
		{
			triggerDelegate(in_other.gameObject);
		}
	}
}
