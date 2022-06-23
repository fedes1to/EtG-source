using System.Collections;
using UnityEngine;

public class GunNutSlamVfx : MonoBehaviour
{
	public VFXPool SlamVfx;

	public float SlamCount;

	public float SlamDistance;

	public float SlamDelay;

	public VFXPool DustVfx;

	public float DustOffset;

	public void OnSpawned()
	{
		StartCoroutine(DoVfx());
	}

	private IEnumerator DoVfx()
	{
		yield return null;
		Vector2 dir = base.transform.right;
		Vector2 sideDir = Quaternion.Euler(0f, 0f, 90f) * dir;
		Vector2 pos = base.transform.position;
		for (int i = 0; (float)i < SlamCount; i++)
		{
			SlamVfx.SpawnAtPosition(pos);
			DustVfx.SpawnAtPosition(pos + sideDir * DustOffset);
			DustVfx.SpawnAtPosition(pos - sideDir * DustOffset);
			if ((float)i < SlamCount - 1f)
			{
				yield return new WaitForSeconds(SlamDelay);
				pos += dir * SlamDistance;
			}
		}
	}
}
