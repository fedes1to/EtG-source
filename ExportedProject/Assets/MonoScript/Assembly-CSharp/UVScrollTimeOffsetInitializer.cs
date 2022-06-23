using UnityEngine;

public class UVScrollTimeOffsetInitializer : MonoBehaviour
{
	public int NumberFrames;

	public float TimePerFrame;

	public void OnSpawned()
	{
		float realtimeSinceStartup = Time.realtimeSinceStartup;
		realtimeSinceStartup %= (float)NumberFrames * TimePerFrame;
		Material material = GetComponent<MeshRenderer>().material;
		material.SetFloat("_TimeOffset", realtimeSinceStartup);
	}
}
