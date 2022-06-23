using System.Collections;
using Brave.BulletScript;
using FullInspector;
using UnityEngine;

[InspectorDropdownName("Chancebulon/CriticalHit1")]
public class ChancebulonCriticalHit1 : Script
{
	private const int NumBullets = 12;

	protected override IEnumerator Top()
	{
		for (int i = 0; i < 12; i++)
		{
			float num = (float)i * 30f;
			Fire(new Offset(1f, 0f, num, string.Empty), new Direction(num), new Speed(Random.Range(4f, 11f)), new ChancebulonBlobProjectileAttack1.BlobulonBullet((ChancebulonBlobProjectileAttack1.BlobType)Random.Range(0, 3)));
		}
		float deltaAngle = 30f;
		for (int j = 0; j < 12; j++)
		{
			Fire(new Direction(((float)j + 0.5f) * deltaAngle), new Speed(6f), new Bullet("icicle"));
		}
		yield return Wait(30);
		Fire(new Direction(-28f, DirectionType.Aim), new Speed(9f), new Bullet("icicle"));
		Fire(new Direction(0f, DirectionType.Aim), new Speed(11f), new Bullet("icicle"));
		Fire(new Direction(28f, DirectionType.Aim), new Speed(9f), new Bullet("icicle"));
		yield return Wait(30);
		Fire(new Direction(-28f, DirectionType.Aim), new Speed(9f), new Bullet("icicle"));
		Fire(new Direction(0f, DirectionType.Aim), new Speed(11f), new Bullet("icicle"));
		Fire(new Direction(28f, DirectionType.Aim), new Speed(9f), new Bullet("icicle"));
	}
}
