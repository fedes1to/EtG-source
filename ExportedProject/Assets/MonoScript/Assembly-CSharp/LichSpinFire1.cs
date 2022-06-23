using System.Collections;
using Brave.BulletScript;
using FullInspector;
using UnityEngine;

[InspectorDropdownName("Bosses/Lich/SpinFire1")]
public class LichSpinFire1 : Script
{
	private const int NumWaves = 60;

	private const int NumBulletsPerWave = 6;

	private const float AngleDeltaEachWave = 9f;

	protected override IEnumerator Top()
	{
		float offset = 0f;
		for (int i = 0; i < 60; i++)
		{
			offset += Mathf.SmoothStep(-9f, 9f, Mathf.PingPong((float)i / 30f, 1f) * 2f - 0.5f);
			for (int j = 0; j < 6; j++)
			{
				float num = (float)j * 60f + offset;
				Fire(new Offset(1f, 0f, num, string.Empty), new Direction(num), new Speed(7f), new Bullet("twirl"));
			}
			yield return Wait(6);
		}
	}
}
