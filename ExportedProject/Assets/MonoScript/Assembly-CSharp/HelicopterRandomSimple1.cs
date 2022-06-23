using System.Collections;
using Brave.BulletScript;
using FullInspector;
using UnityEngine;

[InspectorDropdownName("Bosses/Helicopter/RandomBurstsSimple1")]
public class HelicopterRandomSimple1 : Script
{
	public class BigBullet : Bullet
	{
		public BigBullet()
			: base("big")
		{
		}

		protected override IEnumerator Top()
		{
			Projectile.Ramp(Random.Range(2f, 3f), 2f);
			return null;
		}
	}

	protected override IEnumerator Top()
	{
		if (Random.value < 0.5f)
		{
			int numBullets2 = 8;
			float startDirection = RandomAngle();
			string transform2 = "shoot point 1";
			string transform4 = "shoot point 4";
			if (BraveUtility.RandomBool())
			{
				BraveUtility.Swap(ref transform2, ref transform4);
			}
			for (int i = 0; i < numBullets2; i++)
			{
				Fire(new Offset(transform2), new Direction(SubdivideCircle(startDirection, numBullets2, i)), new Speed(9f), new BigBullet());
			}
			yield return Wait(15);
			for (int j = 0; j < numBullets2; j++)
			{
				Fire(new Offset(transform4), new Direction(SubdivideCircle(startDirection, numBullets2, j, 1f, true)), new Speed(9f), new BigBullet());
			}
			yield break;
		}
		int numBullets = 4;
		float arc = 35f;
		string transform1 = "shoot point 2";
		string transform3 = "shoot point 3";
		if (BraveUtility.RandomBool())
		{
			BraveUtility.Swap(ref transform1, ref transform3);
		}
		float aimDirection2 = GetAimDirection(transform1);
		for (int k = 0; k < numBullets; k++)
		{
			Fire(new Offset(transform1), new Direction(SubdivideArc(aimDirection2 - arc, arc * 2f, numBullets, k)), new Speed(9f), new BigBullet());
		}
		yield return Wait(15);
		aimDirection2 = GetAimDirection(transform3);
		for (int l = 0; l < numBullets; l++)
		{
			Fire(new Offset(transform3), new Direction(SubdivideArc(aimDirection2 - arc, arc * 2f, numBullets, l)), new Speed(9f), new BigBullet());
		}
	}
}
