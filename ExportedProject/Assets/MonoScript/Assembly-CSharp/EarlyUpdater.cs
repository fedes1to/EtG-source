using UnityEngine;

public class EarlyUpdater : MonoBehaviour
{
	private void Awake()
	{
		BraveTime.CacheDeltaTimeForFrame();
	}

	private void Update()
	{
		BraveTime.CacheDeltaTimeForFrame();
	}
}
