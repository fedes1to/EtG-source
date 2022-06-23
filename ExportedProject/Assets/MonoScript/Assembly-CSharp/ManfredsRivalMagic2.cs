using System.Collections;
using Brave.BulletScript;
using FullInspector;
using UnityEngine;

[InspectorDropdownName("ManfredsRival/Magic2")]
public class ManfredsRivalMagic2 : ManfredsRivalMagic1
{
	private const int NumTimes = 3;

	private const int NumBulletsMainWave = 16;

	protected override IEnumerator Top()
	{
		yield return Wait(30);
		for (int i = 0; i < 3; i++)
		{
			float aim = GetAimDirection(1f, 12f);
			FireCluster(aim);
			yield return Wait(10);
			for (int j = 0; j < 16; j++)
			{
				float num = Mathf.Lerp(-30f, 30f, (float)j / 15f);
				Fire(new Offset(0.5f, 0f, Direction + num, string.Empty), new Direction(aim + num), new Speed(9f), new Bullet(null, true));
			}
			FireCluster(aim);
			yield return Wait(10);
			FireCluster(aim);
			yield return Wait(40);
		}
	}
}
