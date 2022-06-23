using System.Collections;
using Brave.BulletScript;
using UnityEngine;

public class WizardYellowSlam1 : Script
{
	public class ExpandingBullet : Bullet
	{
		private WizardYellowSlam1 m_parent;

		private Vector2 m_offset;

		public ExpandingBullet(WizardYellowSlam1 parent, Vector2 offset)
		{
			m_parent = parent;
			m_offset = offset;
		}

		protected override IEnumerator Top()
		{
			base.ManualControl = true;
			Vector2 centerPosition = base.Position;
			for (int j = 0; j < 15; j++)
			{
				UpdateVelocity();
				centerPosition += Velocity / 60f;
				Vector2 actualOffset2 = Vector2.Lerp(Vector2.zero, m_offset, (float)j / 14f);
				actualOffset2 = actualOffset2.Rotate(3f * (float)j);
				base.Position = centerPosition + actualOffset2;
				yield return Wait(1);
			}
			Direction = m_parent.aimDirection;
			Speed = 10f;
			for (int i = 0; i < 300; i++)
			{
				UpdateVelocity();
				centerPosition += Velocity / 60f;
				base.Position = centerPosition + m_offset.Rotate(3f * (float)(15 + i));
				yield return Wait(1);
			}
			Vanish();
		}
	}

	public const float Radius = 2f;

	public const int GrowTime = 15;

	public const float RotationSpeed = 180f;

	public const float BulletSpeed = 10f;

	public float aimDirection { get; private set; }

	protected override IEnumerator Top()
	{
		base.EndOnBlank = true;
		switch (Random.Range(0, 4))
		{
		case 0:
			FireX();
			break;
		case 1:
			FireSquare();
			break;
		case 2:
			FireTriangle();
			break;
		case 3:
			FireCircle();
			break;
		}
		aimDirection = base.AimDirection;
		yield return Wait(15);
		float distanceToTarget = (BulletManager.PlayerPosition() - base.Position).magnitude;
		if (distanceToTarget > 4.5f)
		{
			aimDirection = GetAimDirection(1f, 10f);
		}
	}

	private void FireX()
	{
		Vector2 start = new Vector2(2f, 0f).Rotate(45f);
		Vector2 start2 = new Vector2(2f, 0f).Rotate(135f);
		Vector2 end = new Vector2(2f, 0f).Rotate(225f);
		Vector2 end2 = new Vector2(2f, 0f).Rotate(-45f);
		FireExpandingLine(start, end, 11);
		FireExpandingLine(start2, end2, 11);
	}

	private void FireSquare()
	{
		Vector2 vector = new Vector2(2f, 0f).Rotate(45f);
		Vector2 vector2 = new Vector2(2f, 0f).Rotate(135f);
		Vector2 vector3 = new Vector2(2f, 0f).Rotate(225f);
		Vector2 vector4 = new Vector2(2f, 0f).Rotate(-45f);
		FireExpandingLine(vector, vector2, 9);
		FireExpandingLine(vector2, vector3, 9);
		FireExpandingLine(vector3, vector4, 9);
		FireExpandingLine(vector4, vector, 9);
	}

	private void FireTriangle()
	{
		Vector2 vector = new Vector2(2f, 0f).Rotate(90f);
		Vector2 vector2 = new Vector2(2f, 0f).Rotate(210f);
		Vector2 vector3 = new Vector2(2f, 0f).Rotate(330f);
		FireExpandingLine(vector, vector2, 10);
		FireExpandingLine(vector2, vector3, 10);
		FireExpandingLine(vector3, vector, 10);
	}

	private void FireCircle()
	{
		for (int i = 0; i < 36; i++)
		{
			Fire(new ExpandingBullet(this, new Vector2(2f, 0f).Rotate((float)i / 35f * 360f)));
		}
	}

	private void FireExpandingLine(Vector2 start, Vector2 end, int numBullets)
	{
		for (int i = 0; i < numBullets; i++)
		{
			Fire(new ExpandingBullet(this, Vector2.Lerp(start, end, (float)i / ((float)numBullets - 1f))));
		}
	}
}
