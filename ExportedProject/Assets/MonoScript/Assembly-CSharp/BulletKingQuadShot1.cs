using System.Collections;
using Brave.BulletScript;
using FullInspector;
using UnityEngine;

[InspectorDropdownName("Bosses/BulletKing/QuadShot1")]
public class BulletKingQuadShot1 : Script
{
	public class QuadBullet : Bullet
	{
		private bool m_isHard;

		private int m_index;

		public QuadBullet(bool isHard, int index)
			: base("quad")
		{
			m_isHard = isHard;
			m_index = index;
		}

		protected override IEnumerator Top()
		{
			ChangeSpeed(new Speed(10f), 120);
			if (m_isHard)
			{
				yield return Wait(30);
				Vector2 velocity = BraveMathCollege.DegreesToVector(Direction, Speed);
				float sign = ((m_index % 2 != 0) ? 1 : (-1));
				velocity += new Vector2(0f, sign * 1.75f).Rotate(Direction);
				Direction = velocity.ToAngle();
				Speed = velocity.magnitude;
				yield return Wait(90);
				Vanish();
			}
			else
			{
				yield return Wait(120);
				Vanish();
			}
		}
	}

	public bool IsHard
	{
		get
		{
			return this is BulletKingQuadShotHard1;
		}
	}

	protected override IEnumerator Top()
	{
		yield return Wait(10);
		QuadShot(-1.25f, -0.75f, 0f);
		QuadShot(-1.3125f, -0.4375f, -15f);
		QuadShot(-1.5f, -0.1875f, -30f);
		QuadShot(-1.75f, 0.25f, -45f);
		QuadShot(-2.125f, 1.3125f, -67.5f);
		QuadShot(-2.125f, 1.3125f, -90f);
		QuadShot(-2.125f, 1.3125f, -112.5f);
		QuadShot(-2.0625f, 2.375f, -135f);
		QuadShot(-0.8125f, 3.1875f, -157.5f);
		QuadShot(0.0625f, 3.5625f, 180f);
		QuadShot(0.9375f, 3.1875f, 157.5f);
		QuadShot(2.125f, 2.375f, 135f);
		QuadShot(2.1875f, 1.3125f, 112.5f);
		QuadShot(2.1875f, 1.3125f, 90f);
		QuadShot(2.1875f, 1.3125f, 67.5f);
		QuadShot(1.875f, 0.25f, 45f);
		QuadShot(1.625f, -0.1875f, 30f);
		QuadShot(1.4275f, -0.4375f, 15f);
		QuadShot(1.375f, -0.75f, 0f);
	}

	private void QuadShot(float x, float y, float direction)
	{
		for (int i = 0; i < 4; i++)
		{
			Fire(new Offset(x, y, 0f, string.Empty), new Direction(direction - 90f), new Speed(9f - (float)i * 1.5f), new QuadBullet(IsHard, i));
		}
	}
}
