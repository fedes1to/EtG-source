using System.Collections;
using Brave.BulletScript;
using FullInspector;
using UnityEngine;

[InspectorDropdownName("Bosses/DemonWall/BasicWaves1")]
public class DemonWallBasicWaves1 : Script
{
	public static string[][] shootPoints = new string[3][]
	{
		new string[3] { "sad bullet", "blobulon", "dopey bullet" },
		new string[3] { "left eye", "right eye", "crashed bullet" },
		new string[4] { "sideways bullet", "shotgun bullet", "cultist", "angry bullet" }
	};

	public const int NumBursts = 10;

	protected override IEnumerator Top()
	{
		int group = 1;
		for (int i = 0; i < 10; i++)
		{
			group = BraveUtility.SequentialRandomRange(0, shootPoints.Length, group, null, true);
			FireWave(BraveUtility.RandomElement(shootPoints[group]));
			yield return Wait(20);
		}
	}

	private void FireWave(string transform)
	{
		for (int i = 0; i < 7; i++)
		{
			Fire(new Offset(transform), new Direction(SubdivideArc(-125f, 70f, 7, i)), new Speed(7f), new Bullet("wave", i != 3));
		}
		float aimDirection = GetAimDirection(transform);
		if (Random.value < 0.333f && BraveMathCollege.AbsAngleBetween(-90f, aimDirection) < 45f)
		{
			Fire(new Offset(transform), new Direction(aimDirection), new Speed(7f), new Bullet("wave", true));
		}
	}
}
