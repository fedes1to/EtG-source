using System;
using System.Collections;
using Brave.BulletScript;
using FullInspector;
using UnityEngine;

[InspectorDropdownName("MushroomGuy/SmallWaft1")]
public class MushroomGuySmallWaft1 : Script
{
	public class WaftBullet : Bullet
	{
		public WaftBullet(string bankName)
			: base(bankName)
		{
		}

		protected override IEnumerator Top()
		{
			ChangeSpeed(new Speed(), 120);
			yield return Wait(120);
			base.ManualControl = true;
			Vector2 truePosition = base.Position;
			float xOffset = UnityEngine.Random.Range(0f, 3f);
			float yOffset = UnityEngine.Random.Range(0f, 1f);
			truePosition -= new Vector2(Mathf.Sin(xOffset * (float)Math.PI / 3f) * 0.5f, Mathf.Sin(yOffset * (float)Math.PI / 1f) * 0.125f);
			for (int i = 0; i < 300; i++)
			{
				if (base.IsOwnerAlive && UnityEngine.Random.value < 0.0005f)
				{
					Projectile.spriteAnimator.Play();
					yield return Wait(30);
					base.ManualControl = false;
					Direction = base.AimDirection;
					ChangeSpeed(new Speed(9f), 30);
					yield break;
				}
				truePosition += new Vector2(0f, -1f / 120f);
				float t = (float)i / 60f;
				float waftXOffset = Mathf.Sin((t + xOffset) * (float)Math.PI / 3f) * 0.5f;
				float waftYOffset = Mathf.Sin((t + yOffset) * (float)Math.PI / 1f) * 0.125f;
				base.Position = truePosition + new Vector2(waftXOffset, waftYOffset);
				yield return Wait(1);
			}
			Vanish();
		}
	}

	private const int NumWaftBullets = 30;

	private const int NumFastBullets = 10;

	private const float VerticalDriftVelocity = -0.5f;

	private const float WaftXPeriod = 3f;

	private const float WaftXMagnitude = 0.5f;

	private const float WaftYPeriod = 1f;

	private const float WaftYMagnitude = 0.125f;

	private const int WaftLifeTime = 300;

	protected override IEnumerator Top()
	{
		for (int i = 0; i < 30; i++)
		{
			string bankName = ((!(UnityEngine.Random.value <= 0.33f)) ? "spore2" : "spore1");
			Fire(new Direction(RandomAngle()), new Speed(UnityEngine.Random.Range(1.2f, 6f)), new WaftBullet(bankName));
		}
		for (int j = 0; j < 10; j++)
		{
			string name = ((!(UnityEngine.Random.value <= 0.33f)) ? "spore2" : "spore1");
			Bullet bullet = new SpeedChangingBullet(name, 9f, 75, 300);
			Fire(new Direction(RandomAngle()), new Speed(UnityEngine.Random.Range(2, 16)), bullet);
			bullet.Projectile.spriteAnimator.Play();
		}
		return null;
	}
}
