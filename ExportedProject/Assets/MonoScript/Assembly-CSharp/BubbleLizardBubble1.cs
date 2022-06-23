using System;
using System.Collections;
using Brave.BulletScript;
using FullInspector;
using UnityEngine;

[InspectorDropdownName("BubbleLizard/Bubble1")]
public class BubbleLizardBubble1 : Script
{
	public class BubbleBullet : Bullet
	{
		public BubbleBullet()
			: base("bubble")
		{
		}

		protected override IEnumerator Top()
		{
			base.ManualControl = true;
			Vector2 truePosition = base.Position;
			Projectile.spriteAnimator.Play("bubble_projectile_spawn");
			int animTime2 = Mathf.RoundToInt(Projectile.spriteAnimator.CurrentClip.BaseClipLength * 60f);
			float speed = Speed;
			Speed = 0f;
			yield return Wait(animTime2);
			Speed = speed;
			Direction = base.AimDirection;
			for (int i = 0; i < 960; i++)
			{
				Direction = base.AimDirection;
				UpdateVelocity();
				truePosition += Velocity / 60f;
				float t = (float)i / 60f;
				float waftXOffset = Mathf.Sin(t * (float)Math.PI / 3f) * 1f;
				float waftYOffset = Mathf.Sin(t * (float)Math.PI / 1f) * 0.25f;
				base.Position = truePosition + new Vector2(waftXOffset, waftYOffset);
				yield return Wait(1);
			}
			Projectile.spriteAnimator.Play("bubble_projectile_burst");
			animTime2 = Mathf.RoundToInt(Projectile.spriteAnimator.CurrentClip.BaseClipLength * 60f);
			yield return Wait(animTime2);
			Vanish();
		}

		public override void OnBulletDestruction(DestroyType destroyType, SpeculativeRigidbody hitRigidbody, bool preventSpawningProjectiles)
		{
			if (!preventSpawningProjectiles)
			{
				Fire(new Direction(GetAimDirection(1f, 14f)), new Speed(14f));
			}
		}
	}

	private const float WaftXPeriod = 3f;

	private const float WaftXMagnitude = 1f;

	private const float WaftYPeriod = 1f;

	private const float WaftYMagnitude = 0.25f;

	private const int BubbleLifeTime = 960;

	protected override IEnumerator Top()
	{
		Fire(new Direction(), new Speed(2f), new BubbleBullet());
		return null;
	}
}
