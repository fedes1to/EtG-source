using System.Collections;
using Brave.BulletScript;
using UnityEngine;

public class HellFaceFire1 : Script
{
	public const int NumEyeBullets = 8;

	protected override IEnumerator Top()
	{
		for (int i = 0; i < 150; i++)
		{
			if (i % 14 == 0)
			{
				Fire(new Offset("third eye"), new Direction(GetAimDirection("third eye")), new Speed(10f));
			}
			if (i % 4 == 0)
			{
				Fire(new Offset(Random.Range(-0.75f, 0.75f), Random.Range(-0.25f, 0.25f), 0f, "mouth"), new Direction(Random.Range(-160f, -20f)), new Speed(10f));
			}
			if (i % 7 == 0)
			{
				Fire(new Offset(Random.insideUnitCircle * 0.4f, 0f, "left eye"), new Direction(Random.Range(90f, 240f)), new Speed(10f));
			}
			if (i % 7 == 0)
			{
				Fire(new Offset(Random.insideUnitCircle * 0.4f, 0f, "right eye"), new Direction(Random.Range(-60f, 90f)), new Speed(10f));
			}
			yield return Wait(1);
		}
	}
}
