using System.Collections;
using Brave.BulletScript;
using FullInspector;
using UnityEngine;

[InspectorDropdownName("ManfredsRival/ShieldSlam1")]
public class ManfredsRivalShieldSlam1 : Script
{
	public class ExpandingBullet : Bullet
	{
		private Vector2 m_origin;

		public ExpandingBullet(Vector2 origin)
			: base("shield")
		{
			m_origin = origin;
			base.SuppressVfx = true;
		}

		protected override IEnumerator Top()
		{
			Vector2 offset = base.Position - m_origin;
			float multiplier = 1f;
			base.ManualControl = true;
			for (int i = 0; i < 180; i++)
			{
				multiplier += 0.3f;
				base.Position = m_origin + offset * multiplier;
				yield return Wait(1);
			}
			Vanish();
		}
	}

	public class SpinningBullet : Bullet
	{
		private const float RotationSpeed = 8f;

		private Vector2 m_origin;

		private float m_rotationSign;

		public SpinningBullet(Vector2 origin, float rotationSign)
			: base("sword")
		{
			m_origin = origin;
			m_rotationSign = rotationSign;
			base.SuppressVfx = true;
		}

		protected override IEnumerator Top()
		{
			Speed = 9f;
			base.ManualControl = true;
			float angle = 0f;
			Vector2 centerOfMass = m_origin;
			Vector2 centerOfMassOffset = m_origin - base.Position;
			for (int i = 0; i < 120; i++)
			{
				UpdateVelocity();
				centerOfMass += Velocity / 60f;
				angle += m_rotationSign * 8f;
				base.Position = centerOfMass + (Quaternion.Euler(0f, 0f, angle) * centerOfMassOffset).XY();
				yield return Wait(1);
			}
			Vanish();
		}
	}

	protected override IEnumerator Top()
	{
		base.EndOnBlank = true;
		FireExpandingLine(new Vector2(-0.6f, -1f), new Vector2(0.6f, -1f), 10);
		FireExpandingLine(new Vector2(-0.7f, -1f), new Vector2(-0.8f, -0.9f), 3);
		FireExpandingLine(new Vector2(0.7f, -1f), new Vector2(0.8f, -0.9f), 3);
		FireExpandingLine(new Vector2(-0.8f, -0.9f), new Vector2(-0.8f, 0.2f), 12);
		FireExpandingLine(new Vector2(0.8f, -0.9f), new Vector2(0.8f, 0.2f), 12);
		FireExpandingLine(new Vector2(-0.8f, 0.2f), new Vector2(-0.15f, 1f), 10);
		FireExpandingLine(new Vector2(0.8f, 0.2f), new Vector2(0.15f, 1f), 10);
		FireExpandingLine(new Vector2(-0.15f, 1f), new Vector2(0.15f, 1f), 5);
		FireSpinningLine(new Vector2(0f, -1.5f), new Vector2(0f, 1.5f), 4);
		FireSpinningLine(new Vector2(-0.6f, -0.4f), new Vector2(0.6f, -0.4f), 2);
		yield return Wait(60);
	}

	protected void FireExpandingLine(Vector2 start, Vector2 end, int numBullets)
	{
		start *= 0.5f;
		end *= 0.5f;
		for (int i = 0; i < numBullets; i++)
		{
			Vector2 vector = Vector2.Lerp(start, end, (float)i / ((float)numBullets - 1f));
			Fire(new Offset(vector, 0f, string.Empty), new Direction(vector.ToAngle()), new ExpandingBullet(base.Position));
		}
	}

	protected void FireSpinningLine(Vector2 start, Vector2 end, int numBullets)
	{
		start *= 0.5f;
		end *= 0.5f;
		float b = (BulletManager.PlayerPosition() - base.Position).ToAngle();
		float rotationSign = ((!(BraveMathCollege.AbsAngleBetween(0f, b) < 90f)) ? 1 : (-1));
		float aimDirection = GetAimDirection(1f, 9f);
		for (int i = 0; i < numBullets; i++)
		{
			Vector2 offset = Vector2.Lerp(start, end, (float)i / ((float)numBullets - 1f));
			Fire(new Offset(offset, 0f, string.Empty), new Direction(aimDirection), new SpinningBullet(base.Position, rotationSign));
		}
	}
}
