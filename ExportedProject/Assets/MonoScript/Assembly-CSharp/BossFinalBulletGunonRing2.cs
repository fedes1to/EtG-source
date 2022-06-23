using System;
using System.Collections;
using Brave.BulletScript;
using FullInspector;
using UnityEngine;

[InspectorDropdownName("Bosses/BossFinalBullet/GunonRing2")]
public class BossFinalBulletGunonRing2 : Script
{
	public class BatBullet : Bullet
	{
		private float m_angle;

		private int m_index;

		private BossFinalBulletGunonRing1 m_parentScript;

		public BatBullet()
			: base("bat")
		{
		}

		protected override IEnumerator Top()
		{
			Projectile.specRigidbody.CollideWithTileMap = false;
			Vector2 center = base.Position;
			float radius = 0.5f;
			float expandSpeed = 3f;
			while (base.Tick < 360)
			{
				expandSpeed += -0.00500000035f;
				radius += expandSpeed / 60f;
				m_angle -= Mathf.Min(360f, 13f / (radius * (float)Math.PI) * 360f) / 60f;
				base.Position = center + BraveMathCollege.DegreesToVector(m_angle, radius);
				if (base.Tick >= 10 && base.Tick % 12 == 0 && !IsPointInTile(base.Position))
				{
					Fire(new FireBullet());
				}
				yield return Wait(1);
			}
			Vanish();
		}
	}

	public class FireBullet : Bullet
	{
		public FireBullet()
			: base("fire")
		{
		}

		protected override IEnumerator Top()
		{
			yield return Wait(90);
			Vanish();
		}
	}

	private const float ExpandSpeed = 3f;

	private const float ExpandAcceleration = -0.3f;

	private const float RotationalSpeed = 13f;

	protected override IEnumerator Top()
	{
		Fire(new BatBullet());
		yield return Wait(30);
	}
}
