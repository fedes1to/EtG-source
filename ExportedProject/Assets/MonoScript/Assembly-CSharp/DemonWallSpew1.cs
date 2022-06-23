using System.Collections;
using Brave.BulletScript;
using FullInspector;
using UnityEngine;

[InspectorDropdownName("Bosses/DemonWall/Spew1")]
public class DemonWallSpew1 : Script
{
	public static string[][] shootPoints = new string[2][]
	{
		new string[3] { "sad bullet", "blobulon", "dopey bullet" },
		new string[4] { "sideways bullet", "shotgun bullet", "cultist", "angry bullet" }
	};

	private const int NumBullets = 12;

	protected override IEnumerator Top()
	{
		for (int i = 0; i < 4; i++)
		{
			StartTask(FireWall((i % 2 != 0) ? 1 : (-1)));
			StartTask(FireWaves((i + 1) % 2));
			yield return Wait(110);
		}
	}

	private IEnumerator FireWall(float sign)
	{
		for (int i = 0; i < 4; i++)
		{
			bool offset = i % 2 == 1;
			for (int j = 0; j < ((!offset) ? 12 : 11); j++)
			{
				Fire(new Offset(sign * SubdivideArc(2f, 9.5f, 12, j, offset), 0f, 0f, string.Empty), new Direction(-90f), new Speed(7f), new Bullet("spew"));
			}
			yield return Wait(12);
		}
	}

	private IEnumerator FireWaves(int index)
	{
		for (int i = 0; i < 3; i++)
		{
			string transform = BraveUtility.RandomElement(shootPoints[index]);
			for (int j = 0; j < 5; j++)
			{
				Fire(new Offset(transform), new Direction(SubdivideArc(-115f, 50f, 5, j)), new Speed(7f), new Bullet("wave", j != 2));
			}
			float aimDirection = GetAimDirection(transform);
			if (Random.value < 0.333f && BraveMathCollege.AbsAngleBetween(-90f, aimDirection) < 45f)
			{
				Fire(new Offset(transform), new Direction(aimDirection), new Speed(7f), new Bullet("wave", true));
			}
			yield return Wait(40);
		}
	}
}
