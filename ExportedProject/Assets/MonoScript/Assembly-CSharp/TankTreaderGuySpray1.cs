using System.Collections;
using Brave.BulletScript;
using FullInspector;
using UnityEngine;

[InspectorDropdownName("Bosses/TankTreader/GuySpray1")]
public class TankTreaderGuySpray1 : Script
{
	private const int NumBullets = 42;

	protected override IEnumerator Top()
	{
		for (int i = 0; i < 42; i++)
		{
			float t = Mathf.PingPong((float)i / 6f, 1f);
			float dir = Mathf.Lerp(-30f, 30f, t) + (float)Random.Range(-5, 5);
			Fire(new Direction(dir, DirectionType.Aim), new Speed(11f), new Bullet("guyBullet"));
			yield return Wait(6);
		}
	}
}
