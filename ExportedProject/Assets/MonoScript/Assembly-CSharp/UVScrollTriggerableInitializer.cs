using UnityEngine;

public class UVScrollTriggerableInitializer : MonoBehaviour
{
	public int NumberFrames;

	public float TimePerFrame;

	public void OnSpawned()
	{
		ResetAnimation();
	}

	public void TriggerAnimation()
	{
		float realtimeSinceStartup = Time.realtimeSinceStartup;
		realtimeSinceStartup %= (float)NumberFrames * TimePerFrame;
		Material material = GetComponent<MeshRenderer>().material;
		material.SetFloat("_TimeOffset", realtimeSinceStartup);
		material.SetFloat("_ForcedFrame", -1f);
	}

	public void ResetAnimation()
	{
		Material material = GetComponent<MeshRenderer>().material;
		material.SetFloat("_ForcedFrame", 0f);
	}
}
