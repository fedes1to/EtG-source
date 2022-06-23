using System.Collections;
using Brave.BulletScript;
using FullInspector;
using UnityEngine;

[InspectorDropdownName("Bosses/GiantPowderSkull/EyesRandom1")]
public class GiantPowderSkullEyesRandom1 : Script
{
	private const int NumBullets = 50;

	private const float BulletRange = 150f;

	protected override IEnumerator Top()
	{
		for (int i = 0; i < 50; i++)
		{
			Fire(new Offset("left eye"), new Direction(Random.Range(-75f, 75f), DirectionType.Aim), new Speed(12f), new Bullet("default_novfx"));
			yield return Wait(3);
			Fire(new Offset("right eye"), new Direction(Random.Range(-75f, 75f), DirectionType.Aim), new Speed(12f), new Bullet("default_novfx"));
			yield return Wait(3);
		}
	}
}
