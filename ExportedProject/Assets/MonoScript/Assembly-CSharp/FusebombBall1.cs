using System.Collections;
using Brave.BulletScript;
using FullInspector;
using UnityEngine;

[InspectorDropdownName("Bosses/Fusebomb/Ball1")]
public class FusebombBall1 : Script
{
	private class RollyBall : Bullet
	{
		private const float TargetSpeed = 12f;

		public RollyBall()
			: base("ball")
		{
		}

		protected override IEnumerator Top()
		{
			float direction = -Random.Range(20, 55);
			ChangeSpeed(new Speed(12f), 60);
			ChangeDirection(new Direction(direction), 60);
			return null;
		}

		public override void OnForceRemoved()
		{
			Speed = 12f;
			if ((bool)Projectile && (bool)Projectile.specRigidbody && Projectile.specRigidbody.Velocity != Vector2.zero)
			{
				Projectile.specRigidbody.Velocity = Projectile.specRigidbody.Velocity.normalized * 12f;
			}
		}
	}

	protected override IEnumerator Top()
	{
		float num = Random.Range(-30f, 30f);
		Fire(new Direction(), new Speed(3f), new RollyBall());
		return null;
	}
}
