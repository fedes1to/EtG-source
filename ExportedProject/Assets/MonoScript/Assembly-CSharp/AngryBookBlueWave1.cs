using System.Collections;
using Brave.BulletScript;
using FullInspector;
using UnityEngine;

[InspectorDropdownName("AngryBook/BlueWave1")]
public class AngryBookBlueWave1 : Script
{
	public class WaveBullet : Bullet
	{
		protected override IEnumerator Top()
		{
			yield return Wait(20);
			for (int i = 0; i < 2; i++)
			{
				ChangeSpeed(new Speed(-2f), 20);
				yield return Wait(56);
				ChangeSpeed(new Speed(9f), 20);
				yield return Wait(40);
			}
			Vanish();
		}
	}

	public int NumBullets = 32;

	protected override IEnumerator Top()
	{
		base.EndOnBlank = true;
		float startingDir = Random.Range(0f, 360f);
		for (int i = 0; i < NumBullets; i++)
		{
			Fire(new Direction(startingDir + (float)i * 360f / (float)NumBullets), new Speed(9f), new WaveBullet());
		}
		yield return Wait(60);
	}
}
