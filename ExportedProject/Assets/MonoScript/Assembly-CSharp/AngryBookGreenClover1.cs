using System;
using System.Collections;
using Brave.BulletScript;
using FullInspector;
using UnityEngine;

[InspectorDropdownName("AngryBook/GreenClover1")]
public class AngryBookGreenClover1 : Script
{
	public class WaveBullet : Bullet
	{
		public int spawnTime;

		public WaveBullet(int spawnTime)
		{
			this.spawnTime = spawnTime;
		}

		protected override IEnumerator Top()
		{
			yield return Wait(75 - spawnTime);
			ChangeSpeed(new Speed(8f));
		}
	}

	public int NumBullets = 60;

	protected override IEnumerator Top()
	{
		base.EndOnBlank = true;
		for (int i = 0; i < NumBullets; i++)
		{
			float angleRad = (float)i * ((float)Math.PI * 2f / (float)NumBullets);
			float radius = Mathf.Sin(2f * angleRad) + 0.25f * Mathf.Sin(6f * angleRad);
			float x = radius * Mathf.Cos(angleRad) * 2f;
			float y = radius * Mathf.Sin(angleRad) * 2f;
			Fire(direction: new Direction(BraveMathCollege.Atan2Degrees(y, x)), offset: new Offset(x, y, 0f, string.Empty), bullet: new WaveBullet(i));
			yield return Wait(1);
		}
	}
}
