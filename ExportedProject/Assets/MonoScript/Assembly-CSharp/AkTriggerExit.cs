using UnityEngine;

public class AkTriggerExit : AkTriggerBase
{
	public GameObject triggerObject;

	private void OnTriggerExit(Collider in_other)
	{
		if (triggerDelegate != null && (triggerObject == null || triggerObject == in_other.gameObject))
		{
			triggerDelegate(in_other.gameObject);
		}
	}
}
