using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class BarrageModule
{
	public int BarrageColumns = 1;

	public GameObject barrageVFX;

	public ExplosionData barrageExplosionData;

	public float barrageRadius = 1.5f;

	public float delayBetweenStrikes = 0.25f;

	public float BarrageWidth = 3f;

	public float BarrageLength = 5f;

	public void DoBarrage(Vector2 startPoint, Vector2 direction, MonoBehaviour coroutineTarget)
	{
		List<Vector2> targets = AcquireBarrageTargets(startPoint, direction);
		coroutineTarget.StartCoroutine(HandleBarrage(targets));
	}

	protected List<Vector2> AcquireBarrageTargets(Vector2 startPoint, Vector2 direction)
	{
		List<Vector2> list = new List<Vector2>();
		float num = (0f - barrageRadius) / 2f;
		float z = BraveMathCollege.Atan2Degrees(direction);
		Quaternion quaternion = Quaternion.Euler(0f, 0f, z);
		for (; num < BarrageLength; num += barrageRadius)
		{
			float num2 = Mathf.Clamp01(num / BarrageLength);
			float barrageWidth = BarrageWidth;
			float x = Mathf.Clamp(num, 0f, BarrageLength);
			for (int i = 0; i < BarrageColumns; i++)
			{
				float num3 = Mathf.Lerp(0f - barrageWidth, barrageWidth, ((float)i + 1f) / ((float)BarrageColumns + 1f));
				float num4 = UnityEngine.Random.Range((0f - barrageWidth) / (4f * (float)BarrageColumns), barrageWidth / (4f * (float)BarrageColumns));
				Vector2 vector = new Vector2(x, num3 + num4);
				Vector2 vector2 = (quaternion * vector).XY();
				list.Add(startPoint + vector2);
			}
		}
		return list;
	}

	private IEnumerator HandleBarrage(List<Vector2> targets)
	{
		while (targets.Count > 0)
		{
			Vector2 currentTarget = targets[0];
			targets.RemoveAt(0);
			Exploder.Explode(currentTarget, barrageExplosionData, Vector2.zero);
			yield return new WaitForSeconds(delayBetweenStrikes / (float)BarrageColumns);
		}
	}
}
